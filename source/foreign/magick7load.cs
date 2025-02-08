Here is the C# code equivalent to the provided C code:

```csharp
// vips_foreign_load_magick7.c

using System;
using System.Runtime.InteropServices;

public class VipsForeignLoadMagick7 : VipsForeignLoad
{
    public VipsForeignLoadMagick7()
    {
        n = 1;
    }

    public override int Build(VipsObject obj)
    {
        var magick7 = (VipsForeignLoadMagick7)obj;
        magick_genesis();

        magick7.image_info = CloneImageInfo(null);
        magick7.exception = magick_acquire_exception();
        magick7.lock = vips_g_mutex_new();

        if (!magick7.image_info)
            return -1;

        if (all_frames)
            n = -1;

        VIPS_SETSTR(magick7.image_info.density, density);

        magick_set_image_option(magick7.image_info, "dcm:display-range", "reset");

        if (page > 0)
            magick_set_number_scenes(magick7.image_info, page, n);

        return VIPS_OBJECT_CLASS(vips_foreign_load_magick7_parent_class).Build(obj);
    }

    public override void Dispose(GObject gobject)
    {
        var magick7 = (VipsForeignLoadMagick7)gobject;

        for (int i = 0; i < magick7.n_frames; i++)
            VIPS_FREEF(DestroyCacheView, magick7.cache_view[i]);

        VIPS_FREEF(DestroyImageList, magick7.image);
        VIPS_FREEF(DestroyImageInfo, magick7.image_info);
        VIPS_FREE(magick7.frames);
        VIPS_FREE(magick7.cache_view);
        VIPS_FREEF(magick_destroy_exception, magick7.exception);
        VIPS_FREEF(vips_g_mutex_free, magick7.lock);

        G_OBJECT_CLASS(vips_foreign_load_magick7_parent_class).Dispose(gobject);
    }

    public override int Parse(VipsForeignLoadMagick7 magick7, Image image, VipsImage out)
    {
        var class = (VipsObjectClass)VIPS_OBJECT_GET_CLASS(magick7);

        const char* key;
        Image p;

#ifdef DEBUG
        Console.WriteLine("image->depth = " + image.depth);
        Console.WriteLine("GetImageType() = " + GetImageType(image));
        vips_magick7_print_image_type(image);
        Console.WriteLine("GetPixelChannels() = " + GetPixelChannels(image));
        Console.WriteLine("image->columns = " + image.columns);
        Console.WriteLine("image->rows = " + image.rows);
#endif

        out.Coding = VIPS_CODING_NONE;
        out.Xsize = image.columns;
        out.Ysize = image.rows;
        magick7.frame_height = image.rows;
        out.Bands = magick_get_bands(image);

        if (out.Xsize <= 0 || out.Ysize <= 0 || out.Bands <= 0 ||
            out.Xsize >= VIPS_MAX_COORD || out.Ysize >= VIPS_MAX_COORD ||
            out.Bands >= VIPS_MAX_COORD)
        {
            vips_error(class.nickname, "bad image dimensions {0} x {1} pixels, {2} bands",
                out.Xsize, out.Ysize, out.Bands);
            return -1;
        }

        // Depth can be 'fractional'. You'd think we should use
        // GetImageDepth() but that seems to compute something very complex.
        out.BandFmt = -1;
        if (image.depth >= 1 && image.depth <= 8)
            out.BandFmt = VIPS_FORMAT_UCHAR;
        if (image.depth >= 9 && image.depth <= 16)
            out.BandFmt = VIPS_FORMAT_USHORT;
        if (image.depth == 32)
            out.BandFmt = VIPS_FORMAT_FLOAT;
        if (image.depth == 64)
            out.BandFmt = VIPS_FORMAT_DOUBLE;

        if (out.BandFmt == -1)
        {
            vips_error(class.nickname, "unsupported bit depth {0}", image.depth);
            return -1;
        }

        switch (image.units)
        {
            case PixelsPerInchResolution:
                out.Xres = image.resolution.x / 25.4;
                out.Yres = image.resolution.y / 25.4;
                vips_image_set_string(out, VIPS_META_RESOLUTION_UNIT, "in");
                break;

            case PixelsPerCentimeterResolution:
                out.Xres = image.resolution.x / 10.0;
                out.Yres = image.resolution.y / 10.0;
                vips_image_set_string(out, VIPS_META_RESOLUTION_UNIT, "cm");
                break;

            default:
                // Things like GIF have no resolution info.
                out.Xres = 1.0;
                out.Yres = 1.0;
                break;
        }

        switch (image.colorspace)
        {
            case GRAYColorspace:
                if (out.BandFmt == VIPS_FORMAT_USHORT)
                    out.Type = VIPS_INTERPRETATION_GREY16;
                else
                    out.Type = VIPS_INTERPRETATION_B_W;
                break;

            case sRGBColorspace:
            case RGBColorspace:
                if (out.BandFmt == VIPS_FORMAT_USHORT)
                    out.Type = VIPS_INTERPRETATION_RGB16;
                else
                    out.Type = VIPS_INTERPRETATION_sRGB;
                break;

            case CMYKColorspace:
                out.Type = VIPS_INTERPRETATION_CMYK;
                break;

            default:
                out.Type = VIPS_INTERPRETATION_ERROR;
                break;
        }

        // revise the interpretation if it seems crazy
        out.Type = vips_image_guess_interpretation(out);

        if (vips_image_pipelinev(out, VIPS_DEMAND_STYLE_SMALLTILE, null))
            return -1;

        // Get all the string metadata.
        ResetImagePropertyIterator(image);
        while ((key = GetNextImageProperty(image)))
        {
            char name_text[256];
            VipsBuf name = VIPS_BUF_STATIC(name_text);
            const char* value;

            value = GetImageProperty(image, key, magick7.exception);
            if (!value)
            {
                vips_foreign_load_magick7_error(magick7);
                return -1;
            }
            vips_buf_appendf(&name, "magick-%s", key);
            vips_image_set_string(out, vips_buf_all(&name), value);
        }

        // Set vips metadata from ImageMagick profiles.
        if (magick_set_vips_profile(out, image))
            return -1;

        // Something like "BMP".
        if (strlen(magick7.image.magick) > 0)
            vips_image_set_string(out, "magick-format", magick7.image.magick);

        magick7.n_pages = GetImageListLength(GetFirstImageInList(image));
#ifdef DEBUG
        Console.WriteLine("image has {0} pages", magick7.n_pages);
#endif

        // Do we have a set of equal-sized frames? Append them.
        //
        // FIXME ... there must be an attribute somewhere from dicom read
        // which says this is a volumetric image
        magick7.n_frames = 0;
        for (p = image; p != null; p = GetNextImageInList(p))
        {
            if (p.columns != out.Xsize || p.rows != out.Ysize ||
                magick_get_bands(p) != out.Bands || p.depth != image.depth)
            {
#ifdef DEBUG
                Console.WriteLine("frame {0} differs", magick7.n_frames);
                Console.WriteLine("{1}x{2}, {3} bands",
                    p.columns, p.rows, magick_get_bands(p));
                Console.WriteLine("first frame is {0}x{1}, {2} bands",
                    out.Xsize, out.Ysize, out.Bands);
#endif

                break;
            }

            magick7.n_frames++;
        }
        if (p != null)
            // Nope ... just do the first image in the list.
            magick7.n_frames = 1;

#ifdef DEBUG
        // Only display the traits from frame0, they should all be the same.
        vips_magick7_print_traits(magick7.frames[0]);
        vips_magick7_print_channel_names(magick7.frames[0]);
#endif

        if (magick7.n != -1)
            magick7.n_frames = VIPS_MIN(magick7.n_frames, magick7.n);

        // So we can finally set the height.
        if (magick7.n_frames > 1)
        {
            vips_image_set_int(out, VIPS_META_PAGE_HEIGHT, out.Ysize);
            out.Ysize *= magick7.n_frames;
        }

        vips_image_set_int(out, VIPS_META_N_PAGES, magick7.n_pages);

        vips_image_set_int(out, VIPS_META_ORIENTATION,
            VIPS_CLIP(1, image.orientation, 8));

        vips_image_set_int(out, VIPS_META_BITS_PER_SAMPLE, image.depth);

        return 0;
    }

    public override int FillRegion(VipsRegion out_region, void* seq, void* a, void* b, bool* stop)
    {
        var magick7 = (VipsForeignLoadMagick7)a;

        for (int y = 0; y < out_region.valid.height; y++)
        {
            int top = out_region.valid.top + y;
            int frame = top / magick7.frame_height;
            int line = top % magick7.frame_height;
            Image image = magick7.frames[frame];

            Quantum* p;
            VipsPel* q;

            vips__worker_lock(magick7.lock);

            p = GetCacheViewAuthenticPixels(magick7.cache_view[frame],
                out_region.valid.left, line, out_region.valid.width, 1,
                magick7.exception);

            g_mutex_unlock(magick7.lock);

            if (!p)
                // This can happen if, for example, some frames of a
                // gif are shorter than others. It's not always
                // an error.
                continue;

            q = VIPS_REGION_ADDR(out_region, out_region.valid.left, top);

            switch (out_region.im.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    UNPACK(unsigned char);
                    break;

                case VIPS_FORMAT_USHORT:
                    UNPACK(unsigned short);
                    break;

                case VIPS_FORMAT_FLOAT:
                    UNPACK(float);
                    break;

                case VIPS_FORMAT_DOUBLE:
                    UNPACK(double);
                    break;

                default:
                    g_assert_not_reached();
            }
        }

        return 0;
    }

    public override int Load(VipsForeignLoad magick7)
    {
        var class = (VipsObjectClass)VIPS_OBJECT_GET_CLASS(magick7);

        if (vips_foreign_load_magick7_parse((VipsForeignLoadMagick7)magick7,
            ((VipsForeignLoadMagick7)magick7).image, magick7.out))
            return -1;

        // Record frame pointers.
        g_assert(!magick7.frames);
        if (!(magick7.frames = VIPS_ARRAY(null, magick7.n_frames, Image*)))
            return -1;
        var p = ((VipsForeignLoadMagick7)magick7).image;
        for (int i = 0; i < magick7.n_frames; i++)
        {
            magick7.frames[i] = p;
            p = GetNextImageInList(p);
        }

        // And a cache_view for each frame.
        g_assert(!magick7.cache_view);
        if (!(magick7.cache_view = VIPS_ARRAY(null,
              magick7.n_frames, CacheView*)))
            return -1;
        for (int i = 0; i < magick7.n_frames; i++)
        {
            magick7.cache_view[i] = AcquireAuthenticCacheView(
                magick7.frames[i], magick7.exception);
        }

#ifdef DEBUG
        // Only display the traits from frame0, they should all be the same.
        vips_magick7_print_traits(magick7.frames[0]);
        vips_magick7_print_channel_names(magick7.frames[0]);
#endif

        if (vips_image_generate(magick7.out,
            null, vips_foreign_load_magick7_fill_region, null,
            magick7, null))
            return -1;

        return 0;
    }

    public override int FileHeader(VipsForeignLoad load)
    {
        var magick7 = (VipsForeignLoadMagick7)load;
        var file = (VipsForeignLoadMagick7File)load;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_magick7_file_header: {0}", load);
#endif

        g_strlcpy(magick7.image_info.filename, file.filename,
            MagickPathExtent);

        magick_sniff_file(magick7.image_info, file.filename);

        // It would be great if we could PingImage and just read the header,
        // but sadly many IM coders do not support ping. The critical one for
        // us is DICOM.
        //
        // We have to read the whole image in _header.
        magick7.image = ReadImage(magick7.image_info, magick7.exception);
        if (!magick7.image)
        {
            vips_foreign_load_magick7_error(magick7);
            return -1;
        }

        if (vips_foreign_load_magick7_load((VipsForeignLoadMagick7)load))
            return -1;

        VIPS_SETSTR(load.out.filename, file.filename);

        return 0;
    }
}

public class VipsForeignLoadMagick7File : VipsForeignLoadMagick7
{
    public VipsForeignLoadMagick7File()
    {
    }

    public override bool IsA(const char* filename)
    {
        // Fetch up to the first 100 bytes. Hopefully that'll be enough.
        unsigned char buf[100];
        int len;

        return (len = vips__get_bytes(filename, buf, 100)) > 10 &&
            magick_ismagick(buf, len);
    }

    public override int Header(VipsForeignLoad load)
    {
        var magick7 = (VipsForeignLoadMagick7)load;
        var file = (VipsForeignLoadMagick7File)load;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_magick7_file_header: {0}", load);
#endif

        g_strlcpy(magick7.image_info.filename, file.filename,
            MagickPathExtent);

        magick_sniff_file(magick7.image_info, file.filename);

        // It would be great if we could PingImage and just read the header,
        // but sadly many IM coders do not support ping. The critical one for
        // us is DICOM.
        //
        // We have to read the whole image in _header.
        magick7.image = ReadImage(magick7.image_info, magick7.exception);
        if (!magick7.image)
        {
            vips_foreign_load_magick7_error(magick7);
            return -1;
        }

        if (vips_foreign_load_magick7_load((VipsForeignLoadMagick7)load))
            return -1;

        VIPS_SETSTR(load.out.filename, file.filename);

        return 0;
    }
}

public class VipsForeignLoadMagick7Buffer : VipsForeignLoadMagick7
{
    public VipsForeignLoadMagick7Buffer()
    {
    }

    public override bool IsA_buffer(const void* buf, size_t len)
    {
        return len > 10 && magick_ismagick((const unsigned char*)buf, len);
    }

    public override int Header(VipsForeignLoad load)
    {
        var magick7 = (VipsForeignLoadMagick7)load;
        var buffer = (VipsForeignLoadMagick7Buffer)load;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_magick7_buffer_header: {0}", load);
#endif

        // It would be great if we could PingBlob and just read the header,
        // but sadly many IM coders do not support ping. The critical one for
        // us is DICOM.
        //
        // We have to read the whole image in _header.
        magick_sniff_bytes(magick7.image_info, buffer.buf.data, buffer.buf.length);
        magick7.image = BlobToImage(magick7.image_info,
            buffer.buf.data, buffer.buf.length, magick7.exception);
        if (!magick7.image)
        {
            vips_foreign_load_magick7_error(magick7);
            return -1;
        }

        if (vips_foreign_load_magick7_load((VipsForeignLoadMagick7)load))
            return -1;

        return 0;
    }
}
```

Note that this code uses the `System.Runtime.InteropServices` namespace to call into native C libraries, and assumes that the necessary native libraries are available. The `vips__get_bytes`, `magick_ismagick`, `magick_sniff_file`, `ReadImage`, `BlobToImage`, `CloneImageInfo`, `AcquireAuthenticCacheView`, `GetCacheViewAuthenticPixels`, `DestroyCacheView`, `DestroyImageList`, `DestroyImageInfo`, `vips_g_mutex_new`, and `vips_g_mutex_free` functions are assumed to be available in the native libraries.