Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class MagickReader
{
    public struct Read
    {
        public string filename;
        public VipsImage im;
        public byte[] buf;
        public int len;
        public int page;
        public int n;
        public Image image;
        public ImageInfo image_info;
        public Exception exception;
        public int n_pages;
        public int n_frames;
        public Image[] frames;
        public int frame_height;
        public object lock_obj;
    }

    // read_free
    private static void ReadFree(Read read)
    {
#if DEBUG
        Console.WriteLine("magick2vips: read_free: " + read.filename);
#endif

        if (read.filename != null) { read.filename = null; }
        if (read.image != null) { DestroyImageList(read.image); }
        if (read.image_info != null) { DestroyImageInfo(read.image_info); }
        if (read.frames != null) { read.frames = null; }
        if (read.exception != null) { magick_destroy_exception(read.exception); }
        if (read.lock_obj != null) { vips_g_mutex_free((GMutex)read.lock_obj); }
    }

    // read_close
    private static int ReadClose(VipsImage im, Read read)
    {
        ReadFree(read);

        return 0;
    }

    // read_new
    private static Read ReadNew(string filename, VipsImage im,
        byte[] buf, int len, string density, int page, int n)
    {
        magick_genesis();

        if (n == -1) { n = 10000000; }

        var read = new Read();
        read.filename = filename != null ? Glib.StringDup(filename) : null;
        read.buf = buf;
        read.len = len;
        read.page = page;
        read.n = n;
        read.im = im;
        read.image = null;
        read.image_info = CloneImageInfo(null);
        read.exception = magick_acquire_exception();
        read.n_pages = 0;
        read.n_frames = 0;
        read.frames = null;
        read.frame_height = 0;
        read.lock_obj = vips_g_mutex_new();

        Glib.SignalConnect(im, "close", (object sender, EventArgs e) => { ReadClose(im, read); });

        if (!read.image_info) return null;

        if (filename != null)
            Glib.Str.LCopy(read.image_info.filename, filename, MaxTextExtent);

        // Any extra file format detection.
        if (filename != null)
            magick_sniff_file(read.image_info, filename);
        if (buf != null)
            magick_sniff_bytes(read.image_info, buf, len);

        // Canvas resolution for rendering vector formats like SVG.
        Vips.SetString(read.image_info.density, density);

        // When reading DICOM images, we want to ignore any
        // window_center/_width setting, since it may put pixels outside the
        // 0-65535 range and lose data.
        //
        // These window settings are attached as vips metadata, so our caller
        // can interpret them if it wants.
        magick_set_image_option(read.image_info,
            "dcm:display-range", "reset");

        if (read.page > 0)
            magick_set_number_scenes(read.image_info,
                read.page, read.n);

#if DEBUG
        Console.WriteLine("magick2vips: read_new: " + read.filename);
#endif

        return read;
    }

    // get_bands
    private static int GetBands(Image image)
    {
        int bands;
        ImageType type = GetImageType(image, ref image.exception);

        switch (type)
        {
            case BilevelType:
                bands = 1;
                break;

            case GrayscaleType:
                bands = 1;
                break;

            case GrayscaleMatteType:
                // ImageMagick also has PaletteBilevelMatteType, but GraphicsMagick
                // does not. Skip for portability.
                bands = 2;
                break;

            case PaletteType:
            case TrueColorType:
                bands = 3;
                break;

            case PaletteMatteType:
            case TrueColorMatteType:
            case ColorSeparationType:
                bands = 4;
                break;

            case ColorSeparationMatteType:
                bands = 5;
                break;

            default:
                Vips.Error("magick2vips", "unsupported image type {0}",
                    (int)type);
                return -1;
        }

        return bands;
    }

    // parse_header
    private static int ParseHeader(Read read)
    {
        var im = read.im;
        var image = read.image;

        int depth;
        Image p;
        int i;

#if DEBUG
        Console.WriteLine("parse_header: filename = " + read.filename);
        Console.WriteLine("GetImageChannelDepth(AllChannels) = {0}",
            GetImageChannelDepth(image, AllChannels, ref image.exception));
        Console.WriteLine("GetImageDepth() = {0}",
            GetImageDepth(image, ref image.exception));
        Console.WriteLine("image->depth = {0}", image.depth);
        Console.WriteLine("GetImageType() = {0}",
            GetImageType(image, ref image.exception));
        Console.WriteLine("IsGrayImage() = {0}",
            IsGrayImage(image, ref image.exception));
        Console.WriteLine("IsMonochromeImage() = {0}",
            IsMonochromeImage(image, ref image.exception));
        Console.WriteLine("IsOpaqueImage() = {0}",
            IsOpaqueImage(image, ref image.exception));
        Console.WriteLine("image->columns = {0}", image.columns);
        Console.WriteLine("image->rows = {0}", image.rows);
#endif

        im.Coding = VipsCoding.None;
        im.Xsize = image.columns;
        im.Ysize = image.rows;
        read.frame_height = image.rows;
        im.Bands = GetBands(image);

        if (im.Xsize <= 0 ||
            im.Ysize <= 0 ||
            im.Bands <= 0 ||
            im.Xsize >= Vips.MaxCoord ||
            im.Ysize >= Vips.MaxCoord ||
            im.Bands >= Vips.MaxCoord)
        {
            Vips.Error("magick2vips",
                "bad image dimensions {0} x {1} pixels, {2} bands",
                im.Xsize, im.Ysize, im.Bands);
            return -1;
        }

        // Depth can be 'fractional'.
        //
        // You'd think we should use
        // GetImageDepth() but that seems unreliable. 16-bit mono DICOM images
        // are reported as depth 1, for example.
        //
        // Try GetImageChannelDepth(), maybe that works.
        depth = GetImageChannelDepth(image, AllChannels, ref image.exception);
        im.BandFmt = -1;
        if (depth >= 1 && depth <= 8)
            im.BandFmt = VipsFormat.UChar;
        if (depth >= 9 && depth <= 16)
            im.BandFmt = VipsFormat.USHort;
#ifdef UseHDRI
        if (depth == 32)
            im.BandFmt = VipsFormat.Float;
        if (depth == 64)
            im.BandFmt = VipsFormat.Double;
#else  /*UseHDRI*/
        if (depth == 32)
            im.BandFmt = VipsFormat.UInt;
#endif /*UseHDRI*/

        if (im.BandFmt == -1)
        {
            Vips.Error("magick2vips", "unsupported bit depth {0}",
                (int)depth);
            return -1;
        }

        switch (image.units)
        {
            case PixelsPerInchResolution:
                im.Xres = image.x_resolution / 25.4;
                im.Yres = image.y_resolution / 25.4;
                break;

            case PixelsPerCentimeterResolution:
                im.Xres = image.x_resolution / 10.0;
                im.Yres = image.y_resolution / 10.0;
                break;

            default:
                im.Xres = 1.0;
                im.Yres = 1.0;
                break;
        }

        // this can be wrong for some GM versions and must be sanity checked (see
        // below)
        switch (image.colorspace)
        {
            case GRAYColorspace:
                if (im.BandFmt == VipsFormat.USHort)
                    im.Type = VipsInterpretation.Grey16;
                else
                    im.Type = VipsInterpretation.B_W;
                break;

            case sRGBColorspace:
            case RGBColorspace:
                if (im.BandFmt == VipsFormat.USHort)
                    im.Type = VipsInterpretation.RGB16;
                else
                    im.Type = VipsInterpretation.sRGB;
                break;

            case CMYKColorspace:
                im.Type = VipsInterpretation.CMYK;
                break;

            default:
                im.Type = VipsInterpretation.Error;
                break;
        }

        // revise the interpretation if it seems crazy
        im.Type = VipsImage.GuessInterpretation(im);

        if (VipsImage.Pipelinev(im, VipsDemandStyle.SmallTile, null))
            return -1;

        // Set vips metadata from ImageMagick profiles.
        if (magick_set_vips_profile(im, image))
            return -1;

#ifdef HAVE_RESETIMAGEPROPERTYITERATOR
        {
            char* key;

            // This is the most recent imagemagick API, test for this first.
            ResetImagePropertyIterator(image);
            while ((key = GetNextImageProperty(image)))
            {
                char name_text[256];
                VipsBuf name = VipsBuf.Static(name_text);

                Vips.BufAppendf(ref name, "magick-%s", key);
                Vips.Image.SetString(im,
                    Vips.BufAll(ref name), GetImageProperty(image, key));
            }
        }
#elif defined(HAVE_RESETIMAGEATTRIBUTEITERATOR)
        {
            const ImageAttribute* attr;

            // magick6.1-ish and later, deprecated in 6.5ish.
            ResetImageAttributeIterator(image);
            while ((attr = GetNextImageAttribute(image)))
            {
                char name_text[256];
                VipsBuf name = VipsBuf.Static(name_text);

                Vips.BufAppendf(ref name, "magick-%s", attr.key);
                Vips.Image.SetString(im, Vips.BufAll(ref name), attr.value);
            }
        }
#else
        {
            const ImageAttribute* attr;

            // GraphicsMagick is missing the iterator: we have to loop ourselves.
            // ->attributes is marked as private in the header, but there's no
            // getter so we have to access it directly.
            for (attr = image.attributes; attr != null; attr = attr.next)
            {
                char name_text[256];
                VipsBuf name = VipsBuf.Static(name_text);

                Vips.BufAppendf(ref name, "magick-%s", attr.key);
                Vips.Image.SetString(im, Vips.BufAll(ref name), attr.value);
            }
        }
#endif

        // Something like "BMP".
        if (strlen(read.image.magick) > 0)
            Vips.Image.SetString(im, "magick-format",
                read.image.magick);

        // Do we have a set of equal-sized frames? Append them.
        //
        // FIXME ... there must be an attribute somewhere from dicom read
        // which says this is a volumetric image
        read.n_pages = GetImageListLength(image);
        read.n_frames = 0;
        for (p = image; p != null; p = GetNextImageInList(p))
        {
            int p_depth =
                GetImageChannelDepth(p, AllChannels, ref p.exception);

            if (p.columns != im.Xsize ||
                p.rows != im.Ysize ||
                GetBands(p) != im.Bands ||
                p_depth != depth)
            {
#if DEBUG
                Console.WriteLine("frame {0} differs",
                    read.n_frames);
                Console.WriteLine("{1}x{2}, {3} bands",
                    p.columns, p.rows, GetBands(p));
                Console.WriteLine("first frame is {0}x{1}, {2} bands",
                    im.Xsize, im.Ysize, im.Bands);
#endif /*DEBUG*/

                break;
            }

            read.n_frames += 1;
        }
        if (p != null)
            // Nope ... just do the first image in the list.
            read.n_frames = 1;

#if DEBUG
        Console.WriteLine("will read {0} frames",
            read.n_frames);
#endif /*DEBUG*/

        if (read.n != -1)
            read.n_frames = Vips.Min(read.n_frames, read.n);

        // Record frame pointers.
        if (!(read.frames = new Image[read.n_frames]))
            return -1;
        p = image;
        for (i = 0; i < read.n_frames; i++)
        {
            read.frames[i] = p;
            p = GetNextImageInList(p);
        }

        if (read.n_frames > 1)
        {
            Vips.Image.SetInt(im, VipsMeta.PageHeight, im.Ysize);
            im.Ysize *= read.n_frames;
        }

        Vips.Image.SetInt(im, VipsMeta.NPages, read.n_pages);

        Vips.Image.SetInt(im, VipsMeta.Orientation,
            Vips.Clip(1, image.orientation, 8));

        Vips.Image.SetInt(im, VipsMeta.BitsPerSample, depth);

        return 0;
    }

    // unpack_pixels
    private static void UnpackPixels(VipsImage im, byte[] q8, PixelPacket[] pixels, int n)
    {
        int x;

        switch (im.Bands)
        {
            case 1:
                // Gray.
                switch (im.BandFmt)
                {
                    case VipsFormat.UChar:
                        GRAY_LOOP(unsigned char, 255);
                        break;
                    case VipsFormat.USHort:
                        GRAY_LOOP(unsigned short, 65535);
                        break;
                    case VipsFormat.UInt:
                        GRAY_LOOP(unsigned int, 4294967295UL);
                        break;
                    case VipsFormat.Double:
                        GRAY_LOOP(double, QuantumRange);
                        break;

                    default:
                        Glib.AssertNotReached();
                }
                break;

            case 2:
                // Gray plus alpha.
                switch (im.BandFmt)
                {
                    case VipsFormat.UChar:
                        GRAYA_LOOP(unsigned char, 255);
                        break;
                    case VipsFormat.USHort:
                        GRAYA_LOOP(unsigned short, 65535);
                        break;
                    case VipsFormat.UInt:
                        GRAYA_LOOP(unsigned int, 4294967295UL);
                        break;
                    case VipsFormat.Double:
                        GRAYA_LOOP(double, QuantumRange);
                        break;

                    default:
                        Glib.AssertNotReached();
                }
                break;

            case 3:
                // RGB.
                switch (im.BandFmt)
                {
                    case VipsFormat.UChar:
                        RGB_LOOP(unsigned char, 255);
                        break;
                    case VipsFormat.USHort:
                        RGB_LOOP(unsigned short, 65535);
                        break;
                    case VipsFormat.UInt:
                        RGB_LOOP(unsigned int, 4294967295UL);
                        break;
                    case VipsFormat.Double:
                        RGB_LOOP(double, QuantumRange);
                        break;

                    default:
                        Glib.AssertNotReached();
                }
                break;

            case 4:
                // RGBA or CMYK.
                switch (im.BandFmt)
                {
                    case VipsFormat.UChar:
                        RGBA_LOOP(unsigned char, 255);
                        break;
                    case VipsFormat.USHort:
                        RGBA_LOOP(unsigned short, 65535);
                        break;
                    case VipsFormat.UInt:
                        RGBA_LOOP(unsigned int, 4294967295UL);
                        break;
                    case VipsFormat.Double:
                        RGBA_LOOP(double, QuantumRange);
                        break;

                    default:
                        Glib.AssertNotReached();
                }
                break;

            default:
                Glib.AssertNotReached();
        }
    }

    // get_pixels
    private static PixelPacket[] GetPixels(Image image,
        int left, int top, int width, int height)
    {
        PixelPacket[] pixels;

#if HAVE_GETVIRTUALPIXELS
        if (!(pixels = (PixelPacket[])GetVirtualPixels(image,
            left, top, width, height, ref image.exception)))
#else
        if (!(pixels = GetImagePixels(image, left, top, width, height)))
#endif
            return null;

// Can't happen if red/green/blue are doubles.
#ifndef UseHDRI
        // Unpack palette.
        if (image.storage_class == PseudoClass)
        {
#if HAVE_GETVIRTUALPIXELS
            IndexPacket[] indexes = (IndexPacket[])GetVirtualIndexQueue(image);
#else
            // Was GetIndexes(), but that's now deprecated.
            IndexPacket[] indexes = AccessMutableIndexes(image);
#endif

            int i;

            for (i = 0; i < width * height; i++)
            {
                IndexPacket x = indexes[i];

                if (x < image.colors)
                {
                    pixels[i].red = image.colormap[x].red;
                    pixels[i].green = image.colormap[x].green;
                    pixels[i].blue = image.colormap[x].blue;
                }
            }
        }
#endif /*UseHDRI*/

        return pixels;
    }

    // magick_fill_region
    private static int MagickFillRegion(VipsRegion out,
        byte[] seq, object a, object b, bool[] stop)
    {
        var read = (Read)a;

        for (int y = 0; y < out.valid.height; y++)
        {
            int top = out.valid.top + y;
            int frame = top / read.frame_height;
            int line = top % read.frame_height;

            PixelPacket[] pixels;

            vips__worker_lock(read.lock_obj);

            pixels = GetPixels(read.frames[frame],
                out.valid.left, line, out.valid.width, 1);

            g_mutex_unlock(read.lock_obj);

            if (pixels == null)
            {
                Vips.ForeignLoadInvalidate(read.im);
                Vips.Error("magick2vips",
                    "%s", "unable to read pixels");
                return -1;
            }

            UnpackPixels(read.im, out.region_addr(out.valid.left, top),
                pixels, out.valid.width);
        }

        return 0;
    }

    // vips__magick_read
    public static int VipsMagickRead(string filename,
        VipsImage out, string density, int page, int n)
    {
#if DEBUG
        Console.WriteLine("magick2vips: vips__magick_read: " + filename);
#endif

        var read = ReadNew(filename, out, null, n, density, page, n);

        if (read == null) return -1;

#if DEBUG
        Console.WriteLine("magick2vips: calling ReadImage() ...");
#endif

        read.image = ReadImage(read.image_info, ref read.exception);
        if (!read.image)
        {
            magick_vips_error("magick2vips", read.exception);
            Vips.Error("magick2vips",
                "unable to read file \"{0}\"", filename);
            return -1;
        }

        if (ParseHeader(read) == 0)
        {
            if (VipsImage.Generate(out,
                    null, MagickFillRegion, null, read, null))
                return -1;

            return 0;
        }
        else
            return -1;
    }

    // vips__magick_read_header
    public static int VipsMagickReadHeader(string filename,
        VipsImage out, string density, int page, int n)
    {
#if DEBUG
        Console.WriteLine("vips__magick_read_header: " + filename);
#endif

        var read = ReadNew(filename, out, null, 0, density, page, n);

        if (read == null) return -1;

#if DEBUG
        Console.WriteLine("vips__magick_read_header: reading image ...");
#endif

        // It would be great if we could PingImage and just read the header,
        // but sadly many IM coders do not support ping. The critical one for
        // us is DICOM. TGA also has issues.
        read.image = ReadImage(read.image_info, ref read.exception);
        if (!read.image)
        {
            magick_vips_error("magick2vips", read.exception);
            Vips.Error("magick2vips",
                "unable to read file \"{0}\"", filename);
            return -1;
        }

        if (ParseHeader(read) == 0)
        {
            if (out.Xsize <= 0 ||
                out.Ysize <= 0)
            {
                Vips.Error("magick2vips", "%s", "bad image size");
                return -1;
            }

            // Just a header read: we can free the read early and save an fd.
            ReadFree(read);

            return 0;
        }
        else
            return -1;
    }

    // vips__magick_read_buffer
    public static int VipsMagickReadBuffer(byte[] buf, int len,
        VipsImage out, string density, int page, int n)
    {
#if DEBUG
        Console.WriteLine("magick2vips: vips__magick_read_buffer: " + buf + " " + len);
#endif

        var read = ReadNew(null, out, buf, len, density, page, n);

        if (read == null) return -1;

#if DEBUG
        Console.WriteLine("magick2vips: calling BlobToImage() ...");
#endif

        read.image = BlobToImage(read.image_info,
            buf, len, ref read.exception);
        if (!read.image)
        {
            magick_vips_error("magick2vips", read.exception);
            Vips.Error("magick2vips",
                "%s", "unable to read buffer");
            return -1;
        }

        if (ParseHeader(read) == 0)
        {
            if (VipsImage.Generate(out,
                    null, MagickFillRegion, null, read, null))
                return -1;

            return 0;
        }
        else
            return -1;
    }

    // vips__magick_read_buffer_header
    public static int VipsMagickReadBufferHeader(byte[] buf, int len,
        VipsImage out, string density, int page, int n)
    {
#if DEBUG
        Console.WriteLine("vips__magick_read_buffer_header: " + buf + " " + len);
#endif

        var read = ReadNew(null, out, buf, len, density, page, n);

        if (read == null) return -1;

#if DEBUG
        Console.WriteLine("vips__magick_read_buffer_header: pinging blob ...");
#endif

        // It would be great if we could PingBlob and just read the header,
        // but sadly many IM coders do not support ping well. The critical one
        // for us is DICOM. TGA also has issues.
        read.image = BlobToImage(read.image_info,
            buf, len, ref read.exception);
        if (!read.image)
        {
            magick_vips_error("magick2vips", read.exception);
            Vips.Error("magick2vips",
                "%s", "unable to ping blob");
            return -1;
        }

        if (ParseHeader(read) == 0)
        {
            if (out.Xsize <= 0 ||
                out.Ysize <= 0)
            {
                Vips.Error("magick2vips", "%s", "bad image size");
                return -1;
            }

            return 0;
        }
        else
            return -1;
    }
}
```

Note that I've assumed the existence of certain types and methods (e.g. `VipsImage`, `Glib.StringDup`, etc.) which are not defined in this code snippet, but are likely part of a larger VIPS library or framework. You may need to modify the code to match your specific use case.