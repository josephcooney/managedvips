Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignLoadNsgif : VipsForeignLoad
{
    public int Page { get; set; }
    public int N { get; set; }
    public VipsSource Source { get; set; }
    public nsgif_t Anim { get; set; }
    public unsigned char[] Data { get; set; }
    public size_t Size { get; set; }
    public const nsgif_info_t Info { get; set; }
    public int[] Delay { get; set; }
    public int GifDelay { get; set; }
    public bool HasTransparency { get; set; }
    public bool Interlaced { get; set; }
    public bool LocalPalette { get; set; }
    public nsgif_bitmap_t Bitmap { get; set; }
    public int FrameNumber { get; set; }

    public VipsForeignLoadNsgif()
    {
        Anim = new nsgif_t();
        Delay = new int[0];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            VIPS_FREEF(nsgif_destroy, Anim);
            VIPS_UNREF(Source);
            VIPS_FREE(Delay);
        }
        base.Dispose(disposing);
    }

    public static VipsForeignFlags GetFlagsFilename(string filename)
    {
        return VIPS_FOREIGN_SEQUENTIAL;
    }

    public static VipsForeignFlags GetFlags(VipsForeignLoad load)
    {
        return VIPS_FOREIGN_SEQUENTIAL;
    }

    public bool IsASource(VipsSource source)
    {
        const unsigned char[] data = vips_source_sniff(source, 4);
        if (data != null && data[0] == 'G' && data[1] == 'I' && data[2] == 'F' && data[3] == '8')
            return true;
        return false;
    }

    public void SetHeader(VipsImage image)
    {
        double[] array = new double[3];
        const uint8_t[] bg = (uint8_t[])Info.background;
        size_t entries;
        uint32_t[] table = new uint32_t[NSGIF_MAX_COLOURS];
        int colours;

        VIPS_DEBUG_MSG("vips_foreign_load_nsgif_set_header:\n");

        vips_image_init_fields(image, Info.width, Info.height * N, HasTransparency ? 4 : 3, VIPS_FORMAT_UCHAR, VIPS_CODING_NONE, VIPS_INTERPRETATION_sRGB, 1.0, 1.0);
        vips_image_pipelinev(image, VIPS_DEMAND_STYLE_FATSTRIP, null);

        if (N > 1)
            vips_image_set_int(image, VIPS_META_PAGE_HEIGHT, Info.height);
        vips_image_set_int(image, VIPS_META_N_PAGES, Info.frame_count);
        vips_image_set_int(image, "loop", Info.loop_max);

        vips_image_set_array_int(image, "delay", Delay, Info.frame_count);

        bg.CopyTo(array, 0);
        vips_image_set_array_double(image, "background", array, 3);

        VIPS_SETSTR(image.filename, vips_connection_filename(VIPS_CONNECTION(Source)));

        // DEPRECATED "gif-loop"
        vips_image_set_int(image, "gif-loop", Info.loop_max == 0 ? 0 : Info.loop_max - 1);

        // The deprecated gif-delay field is in centiseconds.
        vips_image_set_int(image, "gif-delay", GifDelay);

        if (!LocalPalette)
        {
            nsgif_global_palette(Anim, table, ref entries);
            vips_image_set_array_int(image, "gif-palette", (int[])table, entries);

            colours = entries;
        }
        else
        {
            int i;

            colours = 0;

            if (Info.global_palette)
            {
                nsgif_global_palette(Anim, table, ref entries);
                colours = entries;
            }

            for (i = 0; i < Info.frame_count; i++)
            {
                if (nsgif_local_palette(Anim, i, table, ref entries))
                    colours = Math.Max(colours, entries);
            }
        }

        vips_image_set_int(image, VIPS_META_BITS_PER_SAMPLE, (int)Math.Ceiling(Math.Log(colours, 2)));

        // Deprecated "palette-bit-depth" use "bits-per-sample" instead.
        vips_image_set_int(image, "palette-bit-depth", (int)Math.Ceiling(Math.Log(colours, 2)));

        vips_image_set_int(image, VIPS_META_PALETTE, 1);

        if (Interlaced)
            vips_image_set_int(image, "interlaced", 1);
    }

    public int Header(VipsForeignLoad load)
    {
        const void* data;
        size_t size;
        nsgif_error result;
        int i;

        VIPS_DEBUG_MSG("vips_foreign_load_nsgif_header:\n");

        // Map the whole source into memory.
        if ((data = vips_source_map(Source, ref size)) == null)
            return -1;

        // Treat errors from _scan() as warnings. If libnsgif really can't do
        // something it'll fail gracefully later when we try to read out
        // frame data.
        result = nsgif_data_scan(Anim, size, data);
        VIPS_DEBUG_MSG("nsgif_data_scan() = %s\n", nsgif_strerror(result));
        switch (result)
        {
            case NSGIF_ERR_END_OF_DATA:
                if (load.fail_on >= VipsFailOn.Truncated)
                {
                    vips_foreign_load_nsgif_error(this, result);
                    return -1;
                }
                else
                    g_warning("%s", nsgif_strerror(result));
                break;

            case NSGIF_OK:
                break;

            default:
                if (load.fail_on >= VipsFailOn.Warning)
                {
                    vips_foreign_load_nsgif_error(this, result);
                    return -1;
                }
                else
                    g_warning("%s", nsgif_strerror(result));
                break;
        }

        // Tell libnsgif that that's all the data we have. This will let us
        // read out any truncated final frames.
        nsgif_data_complete(Anim);

        Info = nsgif_get_info(Anim);
#ifdef VERBOSE
            print_animation(Anim, Info);
#endif /*VERBOSE*/
        if (Info.frame_count == 0)
        {
            vips_error("gifload", "%s", _("no frames in GIF"));
            return -1;
        }

        // Update our global struct based on the information in the
        // individual frames.
        for (i = 0; i < Info.frame_count; i++)
        {
            const nsgif_frame_info_t* frame_info;

            if ((frame_info = nsgif_get_frame_info(Anim, i)) != null)
            {
                if (frame_info.transparency)
                    HasTransparency = true;
                if (frame_info.interlaced)
                    Interlaced = true;
                if (frame_info.local_palette)
                    LocalPalette = true;
            }
        }

        if (N == -1)
            N = Info.frame_count - Page;

        if (Page < 0 || N <= 0 || Page + N > Info.frame_count)
        {
            vips_error("gifload", "%s", _("bad page number"));
            return -1;
        }

        // In ms, frame_delay in cs.
        VIPS_FREE(Delay);
        Delay = new int[Info.frame_count];
        for (i = 0; i < Info.frame_count; i++)
        {
            const nsgif_frame_info_t* frame_info;

            frame_info = nsgif_get_frame_info(Anim, i);
            if (frame_info == null)
            {
                vips_error("gifload", "%s", _("bad frame"));
                return -1;
            }
            Delay[i] = 10 * frame_info.delay;
        }

        GifDelay = Delay[0] / 10;

        SetHeader(load.out);

        return 0;
    }

    public int Generate(VipsRegion out_region, void* seq, void* a, void* b, bool* stop)
    {
        VipsRect r = out_region.valid;
        VipsForeignLoadNsgif gif = (VipsForeignLoadNsgif)a;

        int y;

#ifdef VERBOSE
            VIPS_DEBUG_MSG("vips_foreign_load_nsgif_generate: "
                           "top = %d, height = %d\n",
                r.top, r.height);
#endif /*VERBOSE*/

        for (y = 0; y < r.height; y++)
        {
            // The page for this output line, and the line number in page.
            int page = (r.top + y) / Info.height + Page;
            int line = (r.top + y) % Info.height;

            nsgif_error result;
            VipsPel* p, *q;

            g_assert(line >= 0 && line < Info.height);
            g_assert(page >= 0 && page < Info.frame_count);

            if (gif.FrameNumber != page)
            {
                result = nsgif_frame_decode(gif.Anim,
                    page, ref gif.Bitmap);
                VIPS_DEBUG_MSG("  nsgif_frame_decode(%d) = %d\n",
                    page, result);
                if (result != NSGIF_OK)
                {
                    vips_foreign_load_nsgif_error(this, result);
                    return -1;
                }

#ifdef VERBOSE
            print_frame(nsgif_get_frame_info(gif.Anim, page));
#endif /*VERBOSE*/

                gif.FrameNumber = page;
            }

            p = (VipsPel*)gif.Bitmap + line * Info.width * sizeof(int);
            q = VIPS_REGION_ADDR(out_region, 0, r.top + y);
            if (HasTransparency)
                memcpy(q, p, VIPS_REGION_SIZEOF_LINE(out_region));
            else
            {
                int i;

                for (i = 0; i < r.width; i++)
                {
                    q[0] = p[0];
                    q[1] = p[1];
                    q[2] = p[2];

                    q += 3;
                    p += 4;
                }
            }
        }

        return 0;
    }

    public int TileHeight()
    {
        int height = Info.height;

        int i;

        // First, check the perfect size.
        if (height % 16 == 0)
            return 16;

        // Next, check larger and smaller sizes.
        for (i = 1; i < 16; i++)
        {
            if (height % (16 + i) == 0)
                return 16 + i;
            if (height % (16 - i) == 0)
                return 16 - i;
        }

        return 1;
    }
}

public class VipsForeignLoadNsgifFile : VipsForeignLoadNsgif
{
    public string Filename { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static bool IsAFile(string filename)
    {
        VipsSource source;
        bool result;

        if ((source = vips_source_new_from_file(filename)) != null)
        {
            result = vips_foreign_load_nsgif_is_a_source(source);
            VIPS_UNREF(source);
        }
        else
            return false;

        return result;
    }

    protected override void Build()
    {
        base.Build();
        if (Filename != null && !(Source = vips_source_new_from_file(Filename)))
            return;
    }

    public static string[] Suffs { get; } = new string[] { ".gif", null };
}

public class VipsForeignLoadNsgifBuffer : VipsForeignLoadNsgif
{
    public VipsBlob Blob { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static bool IsABuffer(const byte[] buf, size_t len)
    {
        VipsSource source;
        bool result;

        if ((source = vips_source_new_from_memory(buf, len)) != null)
        {
            result = vips_foreign_load_nsgif_is_a_source(source);
            VIPS_UNREF(source);
        }
        else
            return false;

        return result;
    }

    protected override void Build()
    {
        base.Build();
        if (Blob != null && !(Source = vips_source_new_from_memory(Blob.Data, Blob.Length)))
            return;
    }

    public static string[] Suffs { get; } = new string[] { ".gif", null };
}

public class VipsForeignLoadNsgifSource : VipsForeignLoadNsgif
{
    public VipsSource Source { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static bool IsASource(VipsSource source)
    {
        return vips_foreign_load_nsgif_is_a_source(source);
    }

    protected override void Build()
    {
        base.Build();
        if (Source != null)
        {
            Source = VIPS_OBJECT(Source).Reference();
        }
    }

    public static string[] Suffs { get; } = new string[] { ".gif", null };
}

public class VipsForeignLoadNsgifFileClass : VipsForeignLoadNsgif
{
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static bool IsAFile(string filename)
    {
        return VipsForeignLoadNsgifFile.IsAFile(filename);
    }
}

public class VipsForeignLoadNsgifBufferClass : VipsForeignLoadNsgif
{
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static bool IsABuffer(const byte[] buf, size_t len)
    {
        return VipsForeignLoadNsgifBuffer.IsABuffer(buf, len);
    }
}

public class VipsForeignLoadNsgifSourceClass : VipsForeignLoadNsgif
{
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static bool IsASource(VipsSource source)
    {
        return VipsForeignLoadNsgifSource.IsASource(source);
    }
}

public class VipsGifload : VipsOperation
{
    public const string Nickname = "gifload";
    public const string Description = _("load GIF with libnsgif");

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            base.Dispose(disposing);
    }

    public static int Gifload(string filename, out VipsImage image, params object[] args)
    {
        return vips_call_split(Nickname, args, filename, ref image);
    }

    public static int GifloadBuffer(byte[] buf, size_t len, out VipsImage image, params object[] args)
    {
        VipsBlob blob = new VipsBlob(buf, len);
        return vips_call_split(Nickname, args, blob, ref image);
    }

    public static int GifloadSource(VipsSource source, out VipsImage image, params object[] args)
    {
        return vips_call_split(Nickname, args, source, ref image);
    }
}
```