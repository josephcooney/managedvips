Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignSaveCgif : VipsForeignSave
{
    public enum Mode { Global, Local }

    private double dither = 1.0;
    private int effort = 7;
    private int bitdepth = 8;
    private double interframe_maxerror = 0.0;
    private bool reuse = false;
    private bool interlace = false;
    private double interpalette_maxerror = 3.0;
    private Mode mode = Mode.Global;

    public VipsForeignSaveCgif()
    {
        // Initialize properties
        dither = 1.0;
        effort = 7;
        bitdepth = 8;
        interframe_maxerror = 0.0;
        reuse = false;
        interlace = false;
        interpalette_maxerror = 3.0;
        mode = Mode.Global;
    }

    public override int Build(VipsObject obj)
    {
        // C# equivalent of vips_foreign_save_cgif_build
        VipsImage in_image = (VipsImage)obj.GetInput();
        if (!in_image.HasAlpha())
        {
            // Add alpha channel to image
            VipsImage new_image;
            if (vips_addalpha(in_image, out new_image))
                return -1;
            in_image = new_image;
        }

        // Animation properties
        int[] delay = null;
        int delay_length = 0;
        if (in_image.HasMetadata("delay"))
        {
            if (!vips_image_get_array_int(in_image, "delay", out delay, ref delay_length))
                return -1;
        }
        int loop = 0;
        if (in_image.HasMetadata("loop"))
        {
            if (!vips_image_get_int(in_image, "loop", out loop))
                return -1;
        }

        // Frame properties
        int frame_height = in_image.GetPageHeight();
        int frame_width = in_image.Xsize;

        // Check for large frames
        if ((ulong)frame_width * frame_height > (uint.MaxValue / 4) ||
            frame_width > 65535 || frame_height > 65535)
        {
            vips_error("gifsave_base", "frame too large");
            return -1;
        }

        // Create RGBA frame as contiguous buffer
        byte[] frame_bytes = new byte[4 * frame_width * frame_height];

        // Previous RGBA frame (for spotting pixels which haven't changed)
        byte[] previous_frame = new byte[4 * frame_width * frame_height];

        // Frame index buffer
        byte[] index = new byte[frame_width * frame_height];

        // Set up libimagequant
        VipsQuantiseAttr attr = vips__quantise_attr_create();
        vips__quantise_set_max_colors(attr, 255);
        vips__quantise_set_quality(attr, 0, 100);
        vips__quantise_set_speed(attr, 11 - effort);

        // Read palette on input if not reoptimising
        int[] palette = null;
        int n_colours = 0;
        if (reuse && in_image.HasMetadata("gif-palette"))
        {
            if (!vips_image_get_array_int(in_image, "gif-palette", out palette, ref n_colours))
                return -1;

            if (n_colours > 256)
            {
                vips_error("gifsave_base", "gif-palette too large");
                return -1;
            }
        }

        // Global mode if there's an input palette or palette maxerror is huge
        if (palette != null || interpalette_maxerror > 255)
            mode = Mode.Global;
        else
            mode = Mode.Local;

        // Set up libimagequant for quantisation
        VipsQuantiseImage image = vips__quantise_image_create_rgba(attr, palette, n_colours + 1, 1, 0);
        if (vips__quantise_image_quantize_fixed(image, attr, out VipsQuantiseResult result))
        {
            vips_error("gifsave_base", "quantisation failed");
            return -1;
        }

        // Create cgif context
        CGIF_Config config = new CGIF_Config();
        config.genFlags = 0;
        config.attrFlags = 0;

#ifdef HAVE_CGIF_GEN_KEEP_IDENT_FRAMES
        if (keep_duplicate_frames)
            config.genFlags |= CGIF_GEN_KEEP_IDENT_FRAMES;
#endif

#ifdef HAVE_CGIF_ATTR_NO_LOOP
        config.attrFlags |= CGIF_ATTR_IS_ANIMATED | (loop == 1 ? CGIF_ATTR_NO_LOOP : 0);
        config.numLoops = loop > 1 ? loop - 1 : loop;
#else
        config.attrFlags = CGIF_ATTR_IS_ANIMATED;
        config.numLoops = loop;
#endif

        config.width = frame_width;
        config.height = frame_height;
        config.pGlobalPalette = palette;
        config.numGlobalPaletteEntries = n_colours;

        // Write frames to cgif context
        for (int y = 0; y < frame_height; y++)
        {
            // Copy pixels from input image to frame buffer
            byte[] line = new byte[frame_width * 4];
            Array.Copy(VIPS_REGION_ADDR(in_image, 0, y), line, line.Length);

            // Set up libimagequant for quantisation
            VipsQuantiseImage new_image = vips__quantise_image_create_rgba(attr, line, frame_width, 1, 0);
            if (vips__quantise_image_quantize_fixed(new_image, attr, out result))
            {
                vips_error("gifsave_base", "quantisation failed");
                return -1;
            }

            // Get palette from libimagequant
            int[] rgb_palette = new int[256 * 3];
            vips_foreign_save_cgif_get_rgb_palette(result, rgb_palette);

            // Write frame to cgif context
            config.pImageData = index;
            CGIF_AddFrame(cgif_context, ref config);
        }

        return 0;
    }
}

public class VipsForeignSaveCgifTarget : VipsForeignSaveCgif
{
    public override int Build(VipsObject obj)
    {
        // C# equivalent of vips_foreign_save_cgif_target_build
        base.Build(obj);

        target = (VipsTarget)obj.GetInput();
        return 0;
    }
}

public class VipsForeignSaveCgifFile : VipsForeignSaveCgif
{
    public string filename;

    public override int Build(VipsObject obj)
    {
        // C# equivalent of vips_foreign_save_cgif_file_build
        base.Build(obj);

        if (!(target = vips_target_new_to_file(filename)))
            return -1;

        return 0;
    }
}

public class VipsForeignSaveCgifBuffer : VipsForeignSaveCgif
{
    public VipsArea buffer;

    public override int Build(VipsObject obj)
    {
        // C# equivalent of vips_foreign_save_cgif_buffer_build
        base.Build(obj);

        if (!(target = vips_target_new_to_memory()))
            return -1;

        g_object_get(target, "blob", out buffer);
        return 0;
    }
}

public class VipsForeignSaveCgifMode : System.Enum
{
    public static readonly VipsForeignSaveCgifMode Global = new VipsForeignSaveCgifMode();
    public static readonly VipsForeignSaveCgifMode Local = new VipsForeignSaveCgifMode();
}

public class VipsQuantiseAttr
{
    // C# equivalent of vips__quantise_attr_create
}

public class VipsQuantiseImage
{
    // C# equivalent of vips__quantise_image_create_rgba
}

public class VipsQuantiseResult
{
    // C# equivalent of vips__quantise_result_destroy
}

public class CGIF_Config
{
    public int genFlags;
    public int attrFlags;
    public int numLoops;
    public int width;
    public int height;
    public byte[] pGlobalPalette;
    public int numGlobalPaletteEntries;
    public byte[] pImageData;
}
```

Note that this is not a direct translation, but rather an implementation of the C code in C#. Some changes were made to adapt the code to the C# syntax and conventions. Additionally, some functions and classes from the VIPS library are not implemented here as they are not part of the provided C code.

Also note that the `vips__quantise_attr_create`, `vips__quantise_image_create_rgba`, `vips__quantise_result_destroy` and other functions from the VIPS library are not implemented here, you should implement them according to your needs.