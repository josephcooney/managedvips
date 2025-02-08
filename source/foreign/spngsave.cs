Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

// save to spng
//
// 2/12/11
// 	- wrap a class around the spng writer
// 16/7/12
// 	- compression should be 0-9, not 1-10
// 20/6/18 [felixbuenemann]
// 	- support spng8 palette write with palette, colours, Q, dither
// 24/6/20
// 	- add @bitdepth, deprecate @colours
// 11/11/21
// 	- use libspng for save
// 15/7/22 [lovell]
// 	- default filter to none
// 17/11/22
// 	- add exif save

public class VipsForeignSaveSpng : VipsForeignSave
{
    public int Compression { get; set; }
    public bool Interlace { get; set; }
    public VipsForeignPngFilter Filter { get; set; }
    public bool Palette { get; set; }
    public int Q { get; set; }
    public double Dither { get; set; }
    public int Bitdepth { get; set; }
    public int Effort { get; set; }

    // Set by subclasses.
    public VipsTarget Target { get; set; }

    // Write state.
    private spng_ctx ctx;
    private List<spng_text> text_chunks = new List<spng_text>();
    private VipsImage Memory;
    private int sizeof_line;
    private Pel[] line;

    // Deprecated.
    public int Colours { get; set; }

    public override void Dispose()
    {
        if (Target != null)
            Target.Dispose();

        if (Memory != null)
            Memory.Dispose();

        foreach (var text in text_chunks)
        {
            VIPS_FREE(text.text);
            VIPS_FREE(text);
        }
        text_chunks.Clear();
        VIPS_FREE(line);

        base.Dispose();
    }

    public int Text(VipsForeignSaveSpng self, string keyword, string value)
    {
        var text = new spng_text();
        text.keyword = (char[])keyword.ToCharArray().Take(sizeof(text.keyword)).ToArray();
        text.type = SPNG_TEXT;
        text.length = value.Length;
        text.text = GSTRDUP(value);

        text_chunks.Add(text);
        return 0;
    }

    public void* Comment(VipsImage image, string field, GValue value, object user_data)
    {
        var self = (VipsForeignSaveSpng)user_data;

        if (string.IsNullOrEmpty(field))
            return null;

        if (field.StartsWith("png-comment-"))
        {
            const char[] key = new char[256];
            int i;
            string value_str;

            if (!image.GetString(field, out value_str))
                return image;

            if (field.Length > 256 || sscanf(field, "png-comment-%d-%80s", out i, key) != 2)
            {
                VIPS_ERROR("vips2png", "%s", _("bad png comment key"));
                return image;
            }

            Text(self, key, value_str);
        }

        return null;
    }

    public int Profile(VipsForeignSaveSpng self, VipsImage in_image)
    {
        var save = (VipsForeignSave)self;

        spng_iccp iccp = new spng_iccp();

        // A profile supplied as an argument overrides an embedded
        // profile.
        if (save.Profile != null)
        {
            var blob = new VipsBlob();
            if (!vips_profile_load(save.Profile, out blob))
                return -1;

            if (blob != null)
            {
                size_t length;
                byte[] data = vips_blob_get(blob, out length);
                string basename = GPath.GetFileName(save.Profile);

#ifdef DEBUG
                Console.WriteLine("write_vips: attaching {0} bytes of ICC profile", length);
#endif /*DEBUG*/

                iccp.profile_name = (char[])basename.ToCharArray().Take(sizeof(iccp.profile_name)).ToArray();
                iccp.profile_len = (int)length;
                iccp.profile = data;

                spng_set_iccp(self.ctx, ref iccp);

                vips_area_unref((VipsArea)blob);
                GFree(basename);
            }
        }
        else if (image_get_typeof(in_image, VIPS_META_ICC_NAME))
        {
            byte[] data;
            size_t length;

            if (!image_get_blob(in_image, VIPS_META_ICC_NAME, out data, out length))
                return -1;

#ifdef DEBUG
            Console.WriteLine("write_vips: attaching {0} bytes of ICC profile", length);
#endif /*DEBUG*/

            iccp.profile_name = "icc";
            iccp.profile_len = (int)length;
            iccp.profile = data;

            spng_set_iccp(self.ctx, ref iccp);
        }

        return 0;
    }

    public int Metadata(VipsForeignSaveSpng self, VipsImage in_image)
    {
        uint32_t n_text;
        spng_text[] text_chunk_array;
        spng_exif exif = new spng_exif();
        int i;
        foreach (var p in text_chunks)
        {
            var text = (spng_text)p;
            text_chunk_array[i++] = text;
        }
        n_text = text_chunks.Count;

#ifdef DEBUG
        Console.WriteLine("attaching {0} text items", n_text);
#endif /*DEBUG*/
        spng_set_text(self.ctx, text_chunk_array, n_text);

        return 0;
    }

    public void Pack(VipsForeignSaveSpng self, Pel[] q, Pel[] p, int n)
    {
        int pixel_mask = (8 / self.Bitdepth) - 1;
        int shift = self.Palette ? 0 : 8 - self.Bitdepth;

        Pel bits = 0;
        for (int x = 0; x < n; x++)
        {
            bits <<= self.Bitdepth;
            bits |= p[x] >> shift;

            if ((x & pixel_mask) == pixel_mask)
                q[x / (pixel_mask + 1)] = bits;
        }

        // Any left-over bits? Need to be left-aligned.
        if ((x & pixel_mask) != 0)
        {
            // The number of bits we've collected and must
            // left-align and flush.
            int collected_bits = (x & pixel_mask) << (self.Bitdepth - 1);

            q[x / (pixel_mask + 1)] = bits << (8 - collected_bits);
        }
    }

    public int WriteFn(spng_ctx ctx, object user, byte[] data, size_t n)
    {
        var self = (VipsForeignSaveSpng)user;

        if (!vips_target_write(self.Target, data, n))
            return SPNG_IO_ERROR;

        return 0;
    }

    public int WriteBlock(VipsRegion region, VipsRect area, object user)
    {
        var self = (VipsForeignSaveSpng)user;
        var class = VIPS_OBJECT_GET_CLASS(self);

        int y;
        int error;

        // The area to write is always a set of complete scanlines.
        g_assert(area.Left == 0);
        g_assert(area.Width == region.im.Xsize);
        g_assert(area.Top + area.Height <= region.im.Ysize);

        for (y = 0; y < area.Height; y++)
        {
            Pel[] line;
            size_t sizeof_line;

            line = VIPS_REGION_ADDR(region, 0, area.Top + y);
            sizeof_line = VIPS_REGION_SIZEOF_LINE(region);

            if (self.Bitdepth < 8)
                Pack(self, self.line, line, sizeof_line);
            else
                line = self.line;

            if ((error = spng_encode_row(self.ctx, line, sizeof_line)))
                break;
        }

        // You can get SPNG_EOI for the final scanline.
        if (error && error != SPNG_EOI)
        {
            VIPS_ERROR(class.Nickname, "%s", spng_strerror(error));
            return -1;
        }

        return 0;
    }

    public int Write(VipsForeignSaveSpng self, VipsImage in_image)
    {
        var class = VIPS_OBJECT_GET_CLASS(self);

        int error;
        spng_ihdr ihdr = new spng_ihdr();
        spng_phys phys = new spng_phys();
        int fmt;
        enum spng_encode_flags encode_flags;

        self.ctx = spng_ctx_new(SPNG_CTX_ENCODER);

        if ((error = spng_set_png_stream(self.ctx, WriteFn, self)))
        {
            VIPS_ERROR(class.Nickname, "%s", spng_strerror(error));
            return -1;
        }

        ihdr.width = in_image.Xsize;
        ihdr.height = in_image.Ysize;
        ihdr.bit_depth = self.Bitdepth;

        switch (in_image.Bands)
        {
            case 1:
                ihdr.color_type = SPNG_COLOR_TYPE_GRAYSCALE;
                break;

            case 2:
                ihdr.color_type = SPNG_COLOR_TYPE_GRAYSCALE_ALPHA;
                break;

            case 3:
                ihdr.color_type = SPNG_COLOR_TYPE_TRUECOLOR;
                break;

            case 4:
                ihdr.color_type = SPNG_COLOR_TYPE_TRUECOLOR_ALPHA;
                break;

            default:
                VIPS_ERROR(class.Nickname, "%s", _("bad bands"));
                return -1;
        }

#ifdef HAVE_QUANTIZATION
        // Enable image quantisation to paletted 8bpp PNG if palette is set.
        if (self.Palette)
            ihdr.color_type = SPNG_COLOR_TYPE_INDEXED;
#else
        if (self.Palette)
            g_warning("%s", _("ignoring palette (no quantisation support)"));
#endif /*HAVE_QUANTIZATION*/

        ihdr.compression_method = 0;
        ihdr.filter_method = 0;
        ihdr.interlace_method = self.Interlace ? 1 : 0;

        if ((error = spng_set_ihdr(self.ctx, ref ihdr)))
        {
            VIPS_ERROR(class.Nickname, "%s", spng_strerror(error));
            return -1;
        }

        spng_set_option(self.ctx, SPNG_IMG_COMPRESSION_LEVEL, self.Compression);
        spng_set_option(self.ctx, SPNG_TEXT_COMPRESSION_LEVEL, self.Compression);
        spng_set_option(self.ctx, SPNG_FILTER_CHOICE, self.Filter);

        // Set resolution. spng uses pixels per meter.
        phys.unit_specifier = 1;
        phys.ppu_x = (int)(in_image.Xres * 1000.0);
        phys.ppu_y = (int)(in_image.Xres * 1000.0);
        spng_set_phys(self.ctx, ref phys);

        // Metadata.
        if (Profile(self, in_image) || Metadata(self, in_image))
            return -1;

#ifdef HAVE_QUANTIZATION
        if (self.Palette)
        {
            spng_plte plte = new spng_plte();
            spng_trns trns = new spng_trns();

            VipsImage im_index;
            VipsImage im_palette;
            int palette_count;
            int i;

            if (!vips__quantise_image(in_image, out im_index, out im_palette))
                return -1;

            // PNG is 8-bit index only.
            palette_count = im_palette.Xsize;
            g_assert(palette_count <= 256);

            for (i = 0; i < palette_count; i++)
            {
                Pel[] p = VIPS_IMAGE_ADDR(im_palette, i, 0);
                spng_plte_entry entry;

                entry.red = p[0];
                entry.green = p[1];
                entry.blue = p[2];
                plte.n_entries += 1;

                trns.type3_alpha[i] = p[3];
                if (p[3] != 255)
                    trns.n_type3_entries = i + 1;
            }

#ifdef DEBUG
            Console.WriteLine("attaching {0} entry palette", plte.n_entries);
            if (trns.n_type3_entries)
                Console.WriteLine("attaching {0} transparency values", trns.n_type3_entries);
#endif /*DEBUG*/

            VIPS_UNREF(im_palette);

            spng_set_plte(self.ctx, ref plte);
            if (trns.n_type3_entries)
                spng_set_trns(self.ctx, ref trns);

            in_image = self.Memory = im_index;
        }
#endif /*HAVE_QUANTIZATION*/

        // Low-bitdepth write needs an extra buffer for packing pixels.
        if (self.Bitdepth < 8)
        {
            self.sizeof_line = VIPS_IMAGE_SIZEOF_LINE(in_image) / (8 / self.Bitdepth);
            self.line = new Pel[self.sizeof_line];
        }

        fmt = SPNG_FMT_PNG;
        encode_flags = SPNG_ENCODE_PROGRESSIVE | SPNG_ENCODE_FINALIZE;

        if ((error = spng_encode_image(self.ctx, null, -1, fmt, encode_flags)))
        {
            VIPS_ERROR(class.Nickname, "%s", spng_strerror(error));
            return -1;
        }

        if (self.Interlace)
        {
            // Force the input into memory, if it's not there already.
            if (!self.Memory)
            {
                self.Memory = vips_image_copy_memory(in_image);
                in_image = self.Memory;
            }

            do
            {
                spng_row_info row_info;
                Pel[] line;
                size_t sizeof_line;

                if ((error = spng_get_row_info(self.ctx, out row_info)))
                    break;

                line = VIPS_IMAGE_ADDR(in_image, 0, row_info.row_num);
                sizeof_line = VIPS_IMAGE_SIZEOF_LINE(in_image);

                if (self.Bitdepth < 8)
                    Pack(self, self.line, line, sizeof_line);
                else
                    line = self.line;

                error = spng_encode_row(self.ctx, line, sizeof_line);
            } while (!error);

            if (error != SPNG_EOI)
            {
                VIPS_ERROR(class.Nickname, "%s", spng_strerror(error));
                return -1;
            }
        }
        else
        {
            if (!vips_sink_disc(in_image, WriteBlock, self))
                return -1;
        }

        if (!vips_target_end(self.Target))
            return -1;

        return 0;
    }

    public override bool Build(VipsObject obj)
    {
        var save = (VipsForeignSave)obj;
        var self = (VipsForeignSaveSpng)obj;

        VipsImage in_image;

        if (base.Build(obj))
            return false;

        in_image = save.Ready;
        G_OBJECT_REF(in_image);

        // If no output bitdepth has been specified, use input Type to pick.
        if (!vips_object_argument_isset(obj, "bitdepth"))
            self.Bitdepth =
                in_image.Type == VIPS_INTERPRETATION_RGB16 ||
                    in_image.Type == VIPS_INTERPRETATION_GREY16
                ? 16
                : 8;

        // Deprecated "colours" arg just sets bitdepth large enough to hold
        // that many colours.
        if (vips_object_argument_isset(obj, "colours"))
            self.Bitdepth = (int)Math.Ceiling(Math.Log2(self.Colours));

        // Cast in down to 8 bit if we can.
        if (self.Bitdepth <= 8)
        {
            VipsImage x;

            if (!vips_cast(in_image, out x, VIPS_FORMAT_UCHAR))
                return false;
            G_OBJECT_UNREF(in_image);
            in_image = x;
        }

        // If this is a RGB or RGBA image and a low bit depth has been
        // requested, enable palettisation.
        if (in_image.Bands > 2 && self.Bitdepth < 8)
            self.Palette = true;

        // Disable palettization for >8 bit save.
        if (self.Bitdepth > 8)
            self.Palette = false;

        if (!Write(self, in_image))
        {
            G_OBJECT_UNREF(in_image);
            return false;
        }

        G_OBJECT_UNREF(in_image);

        return true;
    }
}

public class VipsForeignSaveSpngTarget : VipsForeignSaveSpng
{
    public override bool Build(VipsObject obj)
    {
        var self = (VipsForeignSaveSpngTarget)obj;

        self.Target = Target;
        G_OBJECT_REF(self.Target);

        if (!base.Build(obj))
            return false;

        return true;
    }
}

public class VipsForeignSaveSpngFile : VipsForeignSaveSpng
{
    public string Filename { get; set; }

    public override bool Build(VipsObject obj)
    {
        var self = (VipsForeignSaveSpngFile)obj;

        if (!(self.Target = vips_target_new_to_file(self.Filename)))
            return false;

        if (!base.Build(obj))
            return false;

        return true;
    }
}

public class VipsForeignSaveSpngBuffer : VipsForeignSaveSpng
{
    public VipsBlob Buffer { get; set; }

    public override bool Build(VipsObject obj)
    {
        var self = (VipsForeignSaveSpngBuffer)obj;

        if (!(self.Target = vips_target_new_to_memory()))
            return false;

        G_OBJECT_GET(self.Target, "blob", out self.Buffer);

        if (!base.Build(obj))
            return false;

        return true;
    }
}
```