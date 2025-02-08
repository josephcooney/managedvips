Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;

public class VipsForeignLoadPng : VipsForeignLoad
{
    public VipsForeignLoadPng(VipsSource source, bool unlimited = false)
    {
        this.source = source;
        this.unlimited = unlimited;
    }

    private VipsSource source;
    private bool unlimited;

    protected override int GetFlagsFilename(string filename)
    {
        var flags = 0;
        if (VipsForeignLoadPng.GetFlagsSource(source) != null)
            flags |= VipsForeignFlags.Sequential | VipsForeignFlags.Partial;
        return flags;
    }

    protected override int GetFlags(VipsForeignLoad load)
    {
        return VipsForeignLoadPng.GetFlagsSource((VipsSource)load.Source);
    }

    public static VipsForeignFlags? GetFlagsSource(VipsSource source)
    {
        var ctx = spng_ctx_new(SPNG_CTX_IGNORE_ADLER32);
        try
        {
            if (!source.Rewind())
                return null;
            spng_set_png_stream(ctx, (spng_read_func)LoadStream, source);
            var ihdr = new spng_ihdr();
            if (spng_get_ihdr(ctx, ref ihdr))
                return null;

            var flags = 0;
            if (ihdr.interlace_method != SPNG_INTERLACE_NONE)
                flags |= VipsForeignFlags.Partial;
            else
                flags |= VipsForeignFlags.Sequential;

            return flags;
        }
        finally
        {
            spng_ctx_free(ctx);
        }
    }

    public static void SetText(VipsImage image, int i, string key, string value)
    {
        if (string.Compare(key, "XML:com.adobe.xmp", StringComparison.Ordinal) == 0)
        {
            var blob = new VipsBlob();
            blob.Copy(value);
            image.SetBlobCopy("XMP", blob);
        }
        else
        {
            var name = $"png-comment-{i}-{key}";
            image.SetString(name, value);
        }
    }

    public int SetHeader(VipsImage image)
    {
        double xres, yres;
        var iccp = new spng_iccp();
        var exif = new spng_exif();
        var phys = new spng_phys();
        var bkgd = new spng_bkgd();
        uint n_text;

        // Get resolution. Default to 72 pixels per inch.
        xres = 72.0 / 25.4;
        yres = 72.0 / 25.4;
        if (!spng_get_phys(ctx, ref phys))
        {
            // unit 1 means pixels per metre, otherwise unspecified.
            xres = phys.unit_specifier == 1
                ? phys.ppu_x / 1000.0
                : phys.ppu_x;
            yres = phys.unit_specifier == 1
                ? phys.ppu_y / 1000.0
                : phys.ppu_y;
        }

        image.InitFields(ihdr.width, ihdr.height, bands, format, VipsCoding.None, interpretation, xres, yres);

        if (image.Filename != null)
            image.Filename = VipsConnection.GetFileName(VipsConnection.Source(source));

        if (image.PipelineV(VipsDemandStyle.ThinStrip, null) != 0)
            return -1;

        if (!spng_get_iccp(ctx, ref iccp))
            image.SetBlobCopy("ICC", iccp.Profile, iccp.ProfileLen);

        if (!spng_get_text(ctx, null, ref n_text))
        {
            var text = new spng_text[n_text];
            if (!spng_get_text(ctx, text, ref n_text))
            {
                for (int i = 0; i < n_text; i++)
                    SetText(image, i, text[i].Keyword, text[i].Text);
            }
        }

        if (!spng_get_exif(ctx, ref exif))
            image.SetBlobCopy("EXIF", exif.Data, exif.Length);

        image.SetInt(VipsMeta.BitsPerSample, ihdr.bit_depth);

        if (ihdr.color_type == SPNG_COLOR_TYPE_INDEXED)
        {
            // Deprecated "palette-bit-depth" use "bits-per-sample" instead.
            image.SetInt("palette-bit-depth", ihdr.bit_depth);
            image.SetInt(VipsMeta.Palette, 1);
        }

        // Let our caller know. These are very expensive to decode.
        if (ihdr.interlace_method != SPNG_INTERLACE_NONE)
            image.SetInt("interlaced", 1);

        if (!spng_get_bkgd(ctx, ref bkgd))
        {
            const int scale =
                image.BandFmt == VipsFormat.UChar ? 1 : 256;

            double[] array = new double[3];
            int n;

            switch (ihdr.color_type)
            {
                case SPNG_COLOR_TYPE_GRAYSCALE:
                case SPNG_COLOR_TYPE_GRAYSCALE_ALPHA:
                    array[0] = bkgd.Gray / scale;
                    n = 1;
                    break;

                case SPNG_COLOR_TYPE_TRUECOLOR:
                case SPNG_COLOR_TYPE_TRUECOLOR_ALPHA:
                    array[0] = bkgd.Red / scale;
                    array[1] = bkgd.Green / scale;
                    array[2] = bkgd.Blue / scale;
                    n = 3;
                    break;

                case SPNG_COLOR_TYPE_INDEXED:
                default:
                    // Not sure what to do here. I suppose we should read
                    // the palette.
                    n = 0;
                    break;
            }

            if (n > 0)
                image.SetArrayDouble("background", array, n);
        }

        return 0;
    }

    public int Header(VipsForeignLoad load)
    {
        var class = VipsObject.GetClass(load);
        var png = (VipsForeignLoadPng)load;

        int flags;
        int error;
        spng_trns trns;

        // In non-fail mode, ignore CRC errors.
        flags = 0;
        if (load.FailOn < VipsFailOn.Error)
            flags |= SPNG_CTX_IGNORE_ADLER32;
        png.ctx = spng_ctx_new(flags);
        if (load.FailOn < VipsFailOn.Error)
            // Ignore and don't calculate checksums.
            spng_set_crc_action(png.ctx, SPNG_CRC_USE, SPNG_CRC_USE);

        // Set limits to avoid decompression bombs. Set chunk limits to 60mb
        // -- we've seen 50mb XMP blocks in the wild.

        if (!png.unlimited)
        {
            spng_set_image_limits(png.ctx,
                VipsMaxCoord, VipsMaxCoord);
            spng_set_chunk_limits(png.ctx,
                60 * 1024 * 1024, 60 * 1024 * 1024);
        }

        if (source.Rewind())
            return -1;
        spng_set_png_stream(png.ctx,
            (spng_read_func)LoadStream, source);
        if ((error = spng_get_ihdr(png.ctx, ref ihdr)))
        {
            VipsError(class.Nickname, "%s", spng_strerror(error));
            return -1;
        }

#ifdef DEBUG
        Console.WriteLine($"width: {ihdr.width}");
        Console.WriteLine($"height: {ihdr.height}");
        Console.WriteLine($"bit depth: {ihdr.bit_depth}");
        Console.WriteLine($"color type: {ihdr.color_type}");
        Console.WriteLine($"compression method: {ihdr.compression_method}");
        Console.WriteLine($"filter method: {ihdr.filter_method}");
        Console.WriteLine($"interlace method: {ihdr.interlace_method}");
#endif

        // Just convert to host-endian if nothing else applies.
        png.fmt = SPNG_FMT_PNG;

        switch (ihdr.color_type)
        {
            case SPNG_COLOR_TYPE_INDEXED:
                png.bands = 3;
                break;

            case SPNG_COLOR_TYPE_GRAYSCALE_ALPHA:
            case SPNG_COLOR_TYPE_GRAYSCALE:
                png.bands = 1;
                break;

            case SPNG_COLOR_TYPE_TRUECOLOR:
            case SPNG_COLOR_TYPE_TRUECOLOR_ALPHA:
                png.bands = 3;
                break;

            default:
                VipsError(class.Nickname, "%s", _("unknown color type"));
                return -1;
        }

        // Set libvips format and interpretation.
        if (ihdr.bit_depth > 8)
        {
            if (png.bands < 3)
                png.interpretation = VipsInterpretation.Grey16;
            else
                png.interpretation = VipsInterpretation.RGB16;

            png.format = VipsFormat.UShort;
        }
        else
        {
            if (png.bands < 3)
                png.interpretation = VipsInterpretation.B_W;
            else
                png.interpretation = VipsInterpretation.sRGB;

            png.format = VipsFormat.UChar;
        }

        // Expand palette images.
        if (ihdr.color_type == SPNG_COLOR_TYPE_INDEXED)
            png.fmt = SPNG_FMT_RGB8;

        // Expand <8 bit images to full bytes.
        if (ihdr.color_type == SPNG_COLOR_TYPE_GRAYSCALE &&
            ihdr.bit_depth < 8)
            png.fmt = SPNG_FMT_G8;

        // Try reading the optional transparency chunk. This will cause all
        // chunks up to the first IDAT to be read in, so it can fail if any
        // chunk has an error.
        error = spng_get_trns(png.ctx, ref trns);
        if (error &&
            error != SPNG_ECHUNKAVAIL)
        {
            VipsError(class.Nickname, "%s", spng_strerror(error));
            return -1;
        }

        // Expand transparency.

        // The _ALPHA types should not have the optional trns chunk (they
        // always have a transparent band), see
        // https://www.w3.org/TR/2003/REC-PNG-20031110/#11tRNS

        // It's quick and safe to call spng_get_trns() again, and we now know
        // it will only fail for no transparency chunk.
        if (ihdr.color_type == SPNG_COLOR_TYPE_GRAYSCALE_ALPHA ||
            ihdr.color_type == SPNG_COLOR_TYPE_TRUECOLOR_ALPHA)
            png.bands += 1;
        else if (!spng_get_trns(png.ctx, ref trns))
        {
            png.bands += 1;

            if (ihdr.color_type == SPNG_COLOR_TYPE_TRUECOLOR)
            {
                if (ihdr.bit_depth == 16)
                    png.fmt = SPNG_FMT_RGBA16;
                else
                    png.fmt = SPNG_FMT_RGBA8;
            }
            else if (ihdr.color_type == SPNG_COLOR_TYPE_INDEXED)
                png.fmt = SPNG_FMT_RGBA8;
            else if (ihdr.color_type == SPNG_COLOR_TYPE_GRAYSCALE)
            {
                if (ihdr.bit_depth == 16)
                    png.fmt = SPNG_FMT_GA16;
                else
                    png.fmt = SPNG_FMT_GA8;
            }
        }

        source.Minimise();

        return SetHeader(image);
    }

    public int Generate(VipsRegion out_region, void* seq, void* a, void* b, bool* stop)
    {
        var r = out_region.Valid;
        var load = (VipsForeignLoad)a;
        var png = (VipsForeignLoadPng)load;

#ifdef DEBUG
        Console.WriteLine($"vips_foreign_load_png_generate: line {r.top}, {r.height} rows");
        Console.WriteLine($"vips_foreign_load_png_generate: y_top = {png.y_pos}");
#endif

        // We're inside a tilecache where tiles are the full image width, so
        // this should always be true.
        g_assert(r.left == 0);
        g_assert(r.width == out_region.im.Xsize);
        g_assert(VipsRect.Bottom(r) <= out_region.im.Ysize);

        // Tiles should always be a strip in height, unless it's the final
        // strip.
        g_assert(r.height ==
            VipsMin(VipsFatStripHeight, out_region.im.Ysize - r.top));

        // And check that y_pos is correct. It should be, since we are inside
        // a vips_sequential().
        if (r.top != png.y_pos)
        {
            VipsError(class.Nickname,
                _("out of order read at line {0}"), png.y_pos);
            return -1;
        }

        for (int y = 0; y < r.height; y++)
        {
            // libspng returns EOI when successfully reading the
            // final line of input.
            var error = spng_decode_row(png.ctx,
                VipsRegionAddr(out_region, 0, r.top + y),
                VipsRegionSizeOfLine(out_region));
            if (error != 0 &&
                error != SPNG_EOI)
            {
                // We've failed to read some pixels. Knock this
                // operation out of cache.
                VipsOperation.Invalidate(VipsOperation(png));

#ifdef DEBUG
                Console.WriteLine($"vips_foreign_load_png_generate:");
                Console.WriteLine($"  spng_decode_row() failed, line {r.top + y}");
                Console.WriteLine($"  thread {g_thread_self()}");
                Console.WriteLine($"  error {spng_strerror(error)}");
#endif

                g_warning($"{class.Nickname}: {spng_strerror(error)}");

                // And bail if trunc is on.
                if (load.FailOn >= VipsFailOn.Truncated)
                {
                    VipsError(class.Nickname,
                        "%s", _("libspng read error"));
                    return -1;
                }
            }

            png.y_pos += 1;
        }

        return 0;
    }

    public int Load(VipsForeignLoad load)
    {
        var class = VipsObject.GetClass(load);
        var png = (VipsForeignLoadPng)load;

        if (source.Decode())
            return -1;

        // Decode transparency, if available.
        var flags = SPNG_DECODE_TRNS;

        if (ihdr.interlace_method != SPNG_INTERLACE_NONE)
        {
            // Arg awful interlaced image. We have to load to a huge mem
            // buffer, then copy to out.
            var t = new VipsImage[3];
            t[0] = new VipsImage();
            if (SetHeader(png, t[0]) ||
                t[0].WritePrepare())
                return -1;

            if ((var error = spng_decode_image(png.ctx,
                    VipsImageAddr(t[0], 0, 0),
                    VipsImageSizeOfImage(t[0]), png.fmt, flags)) != 0)
            {
                VipsError(class.Nickname,
                    "%s", spng_strerror(error));
                return -1;
            }

            // We've now finished reading the file.
            source.Minimise();

            if (t[0].Write(load.Real))
                return -1;
        }
        else
        {
            t = new VipsImage[3];
            t[0] = new VipsImage();
            if (SetHeader(png, t[0]))
                return -1;

            // We can decode these progressively.
            flags |= SPNG_DECODE_PROGRESSIVE;

            if ((var error = spng_decode_image(png.ctx, null, 0,
                    png.fmt, flags)) != 0)
            {
                VipsError(class.Nickname,
                    "%s", spng_strerror(error));
                return -1;
            }

            // Close input immediately at end of read.
            t[0].Connect("minimise",
                (VipsObject obj) => { png.Minimise(); });

            if (t[0].Generate(null, LoadGenerate, null,
                    png, null) ||
                VipsSequential(t[0], ref t[1],
                    "tile_height", VipsFatStripHeight,
                    null) ||
                t[1].Write(load.Real))
                return -1;
        }

        return 0;
    }

    public void Dispose()
    {
        spng_ctx_free(ctx);
        VIPS_UNREF(source);
    }

    private static int LoadStream(spng_ctx ctx, void* user, void* dest, size_t length)
    {
        var source = (VipsSource)user;

        while (length > 0)
        {
            var bytes_read = source.Read(dest, length);
            if (bytes_read < 0)
                return SPNG_IO_ERROR;
            if (bytes_read == 0)
                return SPNG_IO_EOF;

            dest = (char*)dest + bytes_read;
            length -= bytes_read;
        }

        return 0;
    }
}

public class VipsForeignLoadPngSource : VipsForeignLoadPng
{
    public VipsForeignLoadPngSource(VipsSource source)
    {
        this.source = source;
    }

    protected override int Build(VipsObject obj)
    {
        var png = (VipsForeignLoadPng)obj;

        if (source != null)
            png.Source = source;

        return base.Build(obj);
    }
}

public class VipsForeignLoadPngFile : VipsForeignLoadPng
{
    public VipsForeignLoadPngFile(string filename, bool unlimited = false)
    {
        this.filename = filename;
        this.unlimited = unlimited;
    }

    private string filename;

    protected override int Build(VipsObject obj)
    {
        var png = (VipsForeignLoadPng)obj;
        var file = (VipsForeignLoadPngFile)obj;

        if (file.Filename != null &&
            !(png.Source = VipsSource.NewFromFile(file.Filename)))
            return -1;

        return base.Build(obj);
    }

    public static bool IsA(string filename)
    {
        var source = VipsSource.NewFromFile(filename);
        if (source == null)
            return false;
        return VipsForeignLoadPng.IsASource(source);
    }
}

public class VipsForeignLoadPngBuffer : VipsForeignLoadPng
{
    public VipsForeignLoadPngBuffer(VipsBlob blob, bool unlimited = false)
    {
        this.blob = blob;
        this.unlimited = unlimited;
    }

    private VipsBlob blob;

    protected override int Build(VipsObject obj)
    {
        var png = (VipsForeignLoadPng)obj;
        var buffer = (VipsForeignLoadPngBuffer)obj;

        if (buffer.Blob != null &&
            !(png.Source = VipsSource.NewFromMemory(
                VIPS_AREA(buffer.Blob).Data,
                VIPS_AREA(buffer.Blob).Length)))
            return -1;

        return base.Build(obj);
    }

    public static bool IsABuffer(byte[] buf, int len)
    {
        var source = VipsSource.NewFromMemory(buf, len);
        if (source == null)
            return false;
        return VipsForeignLoadPng.IsASource(source);
    }
}
```

Note that this code assumes the existence of certain classes and methods from the `Vips` library, which are not shown here. Also, some parts of the original C code have been simplified or omitted for brevity.