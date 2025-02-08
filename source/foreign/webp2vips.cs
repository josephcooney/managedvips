Here is the converted C# code:

```csharp
using System;
using System.IO;

public class Read
{
    public VipsImage Out { get; set; }
    public VipsSource Source { get; set; }

    // The data we load, as a webp object.
    public WebPData Data { get; set; }

    // Load this page (frame number).
    public int Page { get; set; }

    // Load this many pages.
    public int N { get; set; }

    // Scale-on-load factor. Use this to set FrameWidth.
    public double Scale { get; set; }

    // Size of each frame in input image coordinates.
    public int CanvasWidth { get; set; }
    public int CanvasHeight { get; set; }

    // Size of each frame, in scaled output image coordinates,
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }

    // Size of final output image.
    public int Width { get; set; }
    public int Height { get; set; }

    // TRUE if we will save the final image as RGBA.
    public bool Alpha { get; set; }

    // Number of frames in file.
    public int FrameCount { get; set; }

    // Delays between frames (in milliseconds).
    public int[] Delays { get; set; }

    // Parse with this.
    public WebPDemuxer Demux { get; set; }

    // Decoder config.
    public WebPDecoderConfig Config { get; set; }

    // The current accumulated frame as a VipsImage. These are the pixels
    // we send to the output. It's a FrameWidth * FrameHeight memory image.
    public VipsImage Frame { get; set; }

    // The frame number currently in @Frame. Numbered from 1, so 0 means
    // before the first frame.
    public int FrameNo { get; set; }

    // Iterate through the frames with this. iter.FrameNum is the number
    // of the currently loaded frame.
    public WebPIterator Iter { get; set; }

    // How to junk the current frame when we move on.
    public WebPMuxAnimDispose DisposeMethod { get; set; }
    public VipsRect DisposeRect { get; set; }
}

public class ReadWebpGenerate : IRegionGenerator
{
    private readonly Read read;

    public ReadWebpGenerate(Read read)
    {
        this.read = read;
    }

    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        // iter.FrameNum numbers from 1.
        int frame = 1 + outRegion.Valid.Top / read.FrameHeight + read.Page;
        int line = outRegion.Valid.Top % read.FrameHeight;

#ifdef DEBUG_VERBOSE
        Console.WriteLine("read_webp_generate: line " + outRegion.Valid.Top);
#endif /*DEBUG_VERBOSE*/

        var r = outRegion.Valid;
        while (read.FrameNo < frame)
        {
            if (!ReadNextFrame(read))
                return -1;

            read.FrameNo += 1;
        }

        if (outRegion.Im.Bands == 4)
            Array.Copy(VIPS_REGION_ADDR(outRegion, 0, r.Top), VIPS_IMAGE_ADDR(read.Frame, 0, line), VIPS_IMAGE_SIZEOF_LINE(read.Frame));
        else
        {
            int x;
            var p = VIPS_IMAGE_ADDR(read.Frame, 0, line);
            var q = VIPS_REGION_ADDR(outRegion, 0, r.Top);
            for (x = 0; x < r.Width; x++)
            {
                q[0] = p[0];
                q[1] = p[1];
                q[2] = p[2];

                q += 3;
                p += 4;
            }
        }

        return 0;
    }
}

public class ReadWebp : IImageGenerator
{
    private readonly VipsImage outImage;

    public ReadWebp(VipsImage outImage)
    {
        this.outImage = outImage;
    }

    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        var read = (Read)a;

        // Make the output pipeline.
        var t = new VipsImage[outImage.Bands];
        t[0] = VipsImage.New(outImage);
        if (!ReadHeader(read, t[0]))
            return -1;

        if (!VipsImage.Generate(t[0], null, ReadWebpGenerate.Create(read), null, read, null) ||
            !Vips.Sequential(t[0], t, null) ||
            !Vips.Image.Write(t[1], outImage))
            return -1;

        return 0;
    }
}

public class VipsWebpReadHeaderSource : IImageGenerator
{
    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        var source = (VipsSource)a;
        var outImage = (VipsImage)b;

        var read = ReadNew(outImage, source, 0, -1, 1.0);
        if (!ReadHeader(read, outImage))
            return -1;

        return 0;
    }
}

public class VipsWebpReadSource : IImageGenerator
{
    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        var source = (VipsSource)a;
        var outImage = (VipsImage)b;

        var read = ReadNew(outImage, source, 0, -1, 1.0);
        if (!ReadImage(read, outImage))
            return -1;

        return 0;
    }
}

public class VipsWebpNames
{
    public const string Iccp = "ICCP";
    public const string Exif = "EXIF";
    public const string Xmp = "XMP ";
}

public static class WebPDataExtensions
{
    public static void Map(VipsSource source, out size_t size)
    {
        var data = new byte[source.Size];
        source.Map(data, 0, (int)size);
        size = data.Length;
    }
}

public static class ReadExtensions
{
    public static bool ReadHeader(this Read read, VipsImage outImage)
    {
        if (!read.Demux)
            return false;

        var flags = WebPDemuxGetI(read.Demux, WEBP_FF_FORMAT_FLAGS);

        read.Alpha = (flags & ALPHA_FLAG) != 0;

        // We do everything as RGBA and then, if we can, drop the alpha on save.
        read.Config.Output.Colorspace = MODE_RGBA;

        read.CanvasWidth = WebPDemuxGetI(read.Demux, WEBP_FF_CANVAS_WIDTH);
        read.CanvasHeight = WebPDemuxGetI(read.Demux, WEBP_FF_CANVAS_HEIGHT);

        if (flags & ANIMATION_FLAG)
        {
            int loopCount;
            var iter = new WebPIterator();

            loopCount = WebPDemuxGetI(read.Demux, WEBP_FF_LOOP_COUNT);
            read.FrameCount = WebPDemuxGetI(read.Demux, WEBP_FF_FRAME_COUNT);

            Vips.Image.SetInt(outImage, "loop", loopCount);

            // DEPRECATED "gif-loop"
            // Not the correct behavior as loop=1 became gif-loop=0
            // but we want to keep the old behavior untouched!
            Vips.Image.SetInt(outImage, "gif-loop", loopCount == 0 ? 0 : loopCount - 1);

            if (WebPDemuxGetFrame(read.Demux, 1, iter))
            {
                int i;

                read.Delays = new int[read.FrameCount];
                for (i = 0; i < read.FrameCount; i++)
                    read.Delays[i] = 40;

                do
                {
                    var frameNum = iter.FrameNum;
                    g_assert(frameNum >= 1 && frameNum <= read.FrameCount);

                    read.Delays[frameNum - 1] = iter.Duration;

                    // We need the alpha in an animation if:
                    //   - any frame has transparent pixels
                    //   - any frame doesn't fill the whole canvas.
                    if (iter.HasAlpha || iter.Width != read.CanvasWidth || iter.Height != read.CanvasHeight)
                        read.Alpha = true;

                    // We must disable shrink-on-load if any frame doesn't fill the whole canvas. We won't be able to shrink-on-load it to the exact position in a downsized canvas.
                    if (iter.Width != read.CanvasWidth || iter.Height != read.CanvasHeight)
                        read.Scale = 1.0;
                } while (WebPDemuxNextFrame(iter));

                Vips.Image.SetArrayInt(outImage, "delay", read.Delays, read.FrameCount);

                // webp uses ms for delays, gif uses centiseconds.
                Vips.Image.SetInt(outImage, "gif-delay", VIPS_RINT(read.Delays[0] / 10.0));
            }

            WebPDemuxReleaseIterator(iter);
        }

        // We round-to-nearest cf. pdfload etc.
        read.FrameWidth = (int)(read.CanvasWidth * read.Scale);
        read.FrameHeight = (int)(read.CanvasHeight * read.Scale);

        if (flags & ANIMATION_FLAG)
        {
            // Only set page-height if we have more than one page, or this could accidentally turn into an animated image later.
            if (read.N > 1)
                Vips.Image.SetInt(outImage, VIPS_META_PAGE_HEIGHT, read.FrameHeight);

            read.Width = read.FrameWidth;
            read.Height = read.N * read.FrameHeight;
        }
        else
        {
            read.Width = read.FrameWidth;
            read.Height = read.FrameHeight;
            read.FrameCount = 1;
        }

        // height can be huge if this is an animated webp image.
        if (read.Width <= 0 || read.Height <= 0 || read.Width > 0x3FFF || read.Height >= VIPS_MAX_COORD || read.FrameWidth <= 0 || read.FrameHeight <= 0 || read.FrameWidth > 0x3FFF || read.FrameHeight > 0x3FFF)
        {
            throw new Exception("bad image dimensions");
        }

        for (int i = 0; i < vips__n_webp_names; i++)
        {
            const string vips = vips__webp_names[i].vips;
            const string webp = vips__webp_names[i].webp;

            if ((flags & vips__webp_names[i].flags) != 0)
            {
                var iter = new WebPChunkIterator();

                WebPDemuxGetChunk(read.Demux, webp, 1, iter);
                Vips.Image.SetBlobCopy(outImage, vips, iter.Chunk.Bytes, iter.Chunk.Size);
                WebPDemuxReleaseChunkIterator(iter);
            }
        }

        // The canvas is always RGBA, we drop alpha to RGB on output if we can.
        read.Frame = Vips.Image.NewMemory();
        Vips.Image.InitFields(read.Frame, read.FrameWidth, read.FrameHeight, 4, VIPS_FORMAT_UCHAR, VIPS_CODING_NONE, VIPS_INTERPRETATION_sRGB, 1.0, 1.0);
        if (Vips.Image.Pipelinev(read.Frame, VIPS_DEMAND_STYLE_THINSTRIP, null) || Vips.Image.WritePrepare(read.Frame))
            return false;

        Vips.Image.InitFields(outImage, read.Width, read.Height, read.Alpha ? 4 : 3, VIPS_FORMAT_UCHAR, VIPS_CODING_NONE, VIPS_INTERPRETATION_sRGB, 1.0, 1.0);
        if (Vips.Image.Pipelinev(outImage, VIPS_DEMAND_STYLE_THINSTRIP, null))
            return false;
        Vips.SetStr(outImage.Filename, Vips.ConnectionFilename(VIPS_CONNECTION(read.Source)));

        if (!WebPDemuxGetFrame(read.Demux, 1, iter))
        {
            throw new Exception("unable to loop through frames");
        }

        return true;
    }
}

public static class ReadExtensions
{
    public static bool ReadImage(this Read read, VipsImage outImage)
    {
        var t = new VipsImage[outImage.Bands];
        t[0] = Vips.Image.New(outImage);
        if (!ReadHeader(read, t[0]))
            return false;

        if (!Vips.Image.Generate(t[0], null, ReadWebpGenerate.Create(read), null, read, null) ||
            !Vips.Sequential(t[0], t, null) ||
            !Vips.Image.Write(t[1], outImage))
            return false;

        return true;
    }
}

public static class VipsExtensions
{
    public static void SetInt(this VipsImage image, string name, int value)
    {
        image.SetField(name, value);
    }

    public static void SetArrayInt(this VipsImage image, string name, int[] values, int length)
    {
        var array = new object[length];
        for (int i = 0; i < length; i++)
            array[i] = values[i];

        image.SetField(name, array);
    }

    public static void SetBlobCopy(this VipsImage image, string name, byte[] data, int offset, int size)
    {
        var blob = new object();
        ((VipsBlob)blob).Data = data;
        ((VipsBlob)blob).Offset = offset;
        ((VipsBlob)blob).Size = size;

        image.SetField(name, blob);
    }

    public static void SetStr(this VipsImage image, string name, string value)
    {
        image.SetField(name, value);
    }
}

public class ReadNextFrame : IRegionGenerator
{
    private readonly Read read;

    public ReadNextFrame(Read read)
    {
        this.read = read;
    }

    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        // Area of this frame, in output image coordinates. We must rint(), since we need the same rules as the overall image scale, or we'll sometimes have missing pixels on edges.
        var area = new VipsRect();
        area.Left = (int)(read.Iter.XOffset * read.Scale);
        area.Top = (int)(read.Iter.YOffset * read.Scale);
        area.Width = (int)(read.Iter.Width * read.Scale);
        area.Height = (int)(read.Iter.Height * read.Scale);

        // Dispose from the previous frame.
        if (read.DisposeMethod == WEBP_MUX_DISPOSE_BACKGROUND)
        {
            // We must clear the pixels occupied by the previous webp frame (not the whole of the read frame) to 0 (transparent).
            //
            // We do not clear to WEBP_FF_BACKGROUND_COLOR. That's only used to composite down to RGB. Perhaps we should attach background as metadata.
            var zero = new VipsPel[4];
            zero[0] = 0;
            zero[1] = 0;
            zero[2] = 0;
            zero[3] = 0;

            Vips.Image.PaintArea(read.Frame, area, zero);
        }

        // Note this frame's dispose for next time.
        read.DisposeMethod = read.Iter.DisposeMethod;
        read.DisposeRect = area;

        if (!ReadFrame(read, area.Width, area.Height, read.Iter.Fragment.Bytes, read.Iter.Fragment.Size))
            return -1;

        // Now blend or copy the new pixels into our accumulator.
        Vips.Image.PaintImage(read.Frame, frame, area.Left, area.Top, read.Iter.FrameNum > 1 && read.Iter.BlendMethod == WEBP_MUX_BLEND);

        g_object_unref(frame);

        // If there's another frame, move on.
        if (read.Iter.FrameNum < read.FrameCount)
        {
            if (!WebPDemuxNextFrame(read.Iter))
                return -1;
        }

        return 0;
    }
}

public class ReadFrame : IRegionGenerator
{
    private readonly Read read;

    public ReadFrame(Read read)
    {
        this.read = read;
    }

    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        // Area of this frame, in output image coordinates. We must rint(), since we need the same rules as the overall image scale, or we'll sometimes have missing pixels on edges.
        var area = new VipsRect();
        area.Left = (int)(read.Iter.XOffset * read.Scale);
        area.Top = (int)(read.Iter.YOffset * read.Scale);
        area.Width = (int)(read.Iter.Width * read.Scale);
        area.Height = (int)(read.Iter.Height * read.Scale);

        var frame = Vips.Image.NewMemory();
        Vips.Image.InitFields(frame, area.Width, area.Height, 4, VIPS_FORMAT_UCHAR, VIPS_CODING_NONE, VIPS_INTERPRETATION_sRGB, 1.0, 1.0);
        if (Vips.Image.Pipelinev(frame, VIPS_DEMAND_STYLE_THINSTRIP, null) || Vips.Image.WritePrepare(frame))
            return -1;

        read.Config.Output.RGBA.Rgba = VIPS_IMAGE_ADDR(frame, 0, 0);
        read.Config.Output.RGBA.Stride = VIPS_IMAGE_SIZEOF_LINE(frame);
        read.Config.Output.RGBA.Size = VIPS_IMAGE_SIZEOF_IMAGE(frame);
        if (read.Scale != 1.0)
        {
            read.Config.Options.UseScaling = 1;
            read.Config.Options.ScaledWidth = area.Width;
            read.Config.Options.ScaledHeight = area.Height;
        }

        var data = new byte[area.Width * area.Height * 4];
        Array.Copy(read.Iter.Fragment.Bytes, data, area.Width * area.Height * 4);
        if (WebPDecode(data, area.Width * area.Height * 4, read.Config) != VP8_STATUS_OK)
            return -1;

        return 0;
    }
}

public class ReadNew : IImageGenerator
{
    private readonly VipsImage outImage;
    private readonly VipsSource source;
    private readonly int page;
    private readonly int n;
    private readonly double scale;

    public ReadNew(VipsImage outImage, VipsSource source, int page, int n, double scale)
    {
        this.outImage = outImage;
        this.source = source;
        this.page = page;
        this.n = n;
        this.scale = scale;
    }

    public int Generate(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        var read = new Read();
        read.Out = outImage;
        read.Source = source;
        g_object_ref(source);
        read.Page = page;
        read.N = n;
        read.Scale = scale;
        read.Delays = null;
        read.Demux = null;
        read.Frame = null;
        read.DisposeMethod = WEBP_MUX_DISPOSE_NONE;
        read.FrameNo = 0;

        // Everything has to stay open until read has finished, unfortunately,
        // since webp relies on us mapping the whole source.
        Vips.Image.Connect(outImage, "close", (image) => { read_free(read); });

        WebPInitDecoderConfig(read.Config);
        read.Config.Options.UseThreads = 1;
        read.Config.Output.IsExternalMemory = 1;

        // Map the whole source into memory.
        var data = new byte[source.Size];
        source.Map(data, 0, (int)data.Length);

        return 0;
    }
}

public class ReadFree : IDisposable
{
    private readonly Read read;

    public ReadFree(Read read)
    {
        this.read = read;
    }

    public void Dispose()
    {
        WebPDemuxReleaseIterator(read.Iter);
        Vips.Unref(read.Frame);
        VIPS_FREEF(WebPDemuxDelete, read.Demux);
        WebPFreeDecBuffer(&read.Config.Output);

        Vips.Unref(read.Source);
        VIPS_FREE(read.Delays);
        VIPS_FREE(read);
    }
}
```

Note that I've assumed some things about the C# code, such as the existence of certain classes and methods (e.g. `VipsImage`, `VipsSource`, `WebPData`, etc.). You may need to modify the code to fit your specific use case.

Also, please note that this is a direct translation from the provided C code, without any optimizations or improvements. The resulting C# code may not be as efficient or idiomatic as it could be.