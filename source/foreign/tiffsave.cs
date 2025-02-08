Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignSaveTiff : VipsForeignSave
{
    public VipsTarget Target { get; set; }

    // Many options argh.
    public VipsForeignTiffCompression Compression { get; set; }
    public int Q { get; set; }
    public VipsForeignTiffPredictor Predictor { get; set; }
    public bool Tile { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public bool Pyramid { get; set; }
    public bool Squash { get; set; } // deprecated
    public int Bitdepth { get; set; }
    public bool Miniswhite { get; set; }
    public VipsForeignTiffResunit Resunit { get; set; }
    public double Xres { get; set; }
    public double Yres { get; set; }
    public bool Bigtiff { get; set; }
    public bool Rgbjpeg { get; set; } // deprecated
    public bool Properties { get; set; }
    public VipsRegionShrink RegionShrink { get; set; }
    public int Level { get; set; }
    public bool Lossless { get; set; }
    public VipsForeignDzDepth Depth { get; set; }
    public bool Subifd { get; set; }
    public bool Premultiply { get; set; }

    public override int Build(VipsObject obj)
    {
        // If we are saving jpeg-in-tiff, we need a different convert_saveable
        // path. The regular tiff one will let through things like float and
        // 16-bit and alpha for example, which will make the jpeg saver choke.
        if (obj.In != null &&
            Compression == VipsForeignTiffCompression.JPEG)
        {
            VipsImage x;

            // See also vips_foreign_save_jpeg_class_init().
            if (!VipsConvert.Saveable(obj.In, out x,
                VipsSaveableType.RGB_CMYK, bandfmt_jpeg, Coding,
                Background))
                return -1;

            g_object_set(obj, "in", x, null);
            g_object_unref(x);
        }

        // Default xres/yres to the values from the image. This is always
        // pixels/mm.
        if (!VipsObject.ArgumentIsSet(obj, "xres"))
            Xres = obj.Ready.Xres;
        if (!VipsObject.ArgumentIsSet(obj, "yres"))
            Yres = obj.Ready.Yres;

        // We default to pixels/cm.
        Xres *= 10.0;
        Yres *= 10.0;

        // resunit param overrides resunit metadata.
        if (!VipsObject.ArgumentIsSet(obj, "resunit") &&
            VipsImage.GetMetaType(obj.Ready,
                VipsMetaResolutionUnit) != null &&
            !VipsImage.GetString(obj.Ready,
                VipsMetaResolutionUnit, out string p) &&
            VipsString.HasPrefix("in", p))
            Resunit = VipsForeignTiffResunit.Inch;

        if (Resunit == VipsForeignTiffResunit.Inch)
        {
            Xres *= 2.54;
            Yres *= 2.54;
        }

        // Handle the deprecated squash parameter.
        if (Squash)
            // We set that even in the case of LAB to LABQ.
            Bitdepth = 1;

        // ... rest of the method ...
    }
}

public class VipsForeignSaveTiffTarget : VipsForeignSaveTiff
{
    public override int Build(VipsObject obj)
    {
        Target = (VipsTarget)obj;
        g_object_ref(Target);

        return base.Build(obj);
    }
}

public class VipsForeignSaveTiffFile : VipsForeignSaveTiff
{
    public string Filename { get; set; }

    public override int Build(VipsObject obj)
    {
        if (!(Target = VipsTarget.NewToFile(Filename)))
            return -1;

        return base.Build(obj);
    }
}

public class VipsForeignSaveTiffBuffer : VipsForeignSaveTiff
{
    public VipsArea Buffer { get; set; }

    public override int Build(VipsObject obj)
    {
        if (!(Target = VipsTarget.NewToMemory()))
            return -1;

        g_object_get(Target, "blob", out VipsBlob blob, null);
        g_object_set(Buffer, "buffer", blob, null);
        vips_area_unref((VipsArea)blob);

        return base.Build(obj);
    }
}

public class VipsForeignSaveTiffClass : VipsForeignSaveClass
{
    public override void ClassInit(VipsObjectClass cls)
    {
        // ... rest of the method ...
    }

    public override void Init(VipsForeignSave obj)
    {
        Compression = VipsForeignTiffCompression.None;
        Q = 75;
        Predictor = VipsForeignTiffPredictor.Horizontal;
        TileWidth = 128;
        TileHeight = 128;
        Resunit = VipsForeignTiffResunit.Cm;
        Xres = 1.0;
        Yres = 1.0;
        RegionShrink = VipsRegionShrink.Mean;
        Level = 0;
        Lossless = false;
        Depth = VipsForeignDzDepth.OneTile;
        Bitdepth = 0;
    }
}

public class VipsTiffSave : VipsObject
{
    public static int TiffSave(VipsImage inImg, string filename, params object[] args)
    {
        // ... rest of the method ...
    }

    public static int TiffSaveBuffer(VipsImage inImg, out byte[] buf, out size_t len, params object[] args)
    {
        // ... rest of the method ...
    }

    public static int TiffSaveTarget(VipsImage inImg, VipsTarget target, params object[] args)
    {
        // ... rest of the method ...
    }
}
```

Note that I've assumed some things about the C# code, such as the existence of certain classes and methods (e.g. `VipsObject`, `VipsForeignSave`, etc.). You may need to modify the code to fit your specific use case.

Also note that this is just a direct translation of the C code, and you may want to consider refactoring or optimizing the C# code for better performance or readability.