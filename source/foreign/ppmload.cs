Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;

public class VipsForeignLoadPpm : VipsForeignLoad
{
    public override int Build(VipsObject obj)
    {
        if (Source != null)
            Sbuf = new VipsSbuf(Source);

        return base.Build(obj);
    }

    public override void Dispose(GObject gobject)
    {
        base.Dispose(gobject);
        VIPS.Unref(Sbuf);
        VIPS.Unref(Source);
    }

    public override int GetFlags(VipsForeignLoad load)
    {
        VipsForeignFlags flags = 0;

        if (!HaveReadHeader && ParseHeader() != 0)
            return 0;

        if (VIPS.SourceIsMappable(Source) &&
            !Ascii &&
            Bits >= 8)
            flags |= VipsForeignFlags.Partial;
        else
            flags |= VipsForeignFlags.Sequential;

        return flags;
    }

    public override void SetImageMetadata(VipsImage image)
    {
        image.Type = Interpretation;

        if (Index == 6 || Index == 7)
            vips_image_set_double(image, "pfm-scale", Math.Abs(Scale));
        else
            vips_image_set_double(image, "ppm-max-value", Math.Abs(MaxValue));

        VIPS.SetStr(image.Filename,
            VIPS.ConnectionFilename(VIPS.Connection(Source)));
    }

    public override void SetImage(VipsImage image)
    {
        vips_image_init_fields(image,
            Width, Height, Bands, Format,
            VipsCoding.None, Interpretation, 1.0, 1.0);

        vips_image_pipelinev(image, VipsDemandStyle.ThinStrip, null);

        vips_foreign_load_ppm_set_image_metadata(this, image);
    }

    public override int Header(VipsForeignLoad load)
    {
        if (!HaveReadHeader && ParseHeader() != 0)
            return 0;

        vips_foreign_load_ppm_set_image(this, load.Out);

        VIPS.SourceMinimise(Source);

        return 0;
    }

    public override VipsImage Scan()
    {
        VipsImage[] t = new VipsImage[2];

        if (!Ascii && Bits >= 8)
            generate = vips_foreign_load_ppm_generate_binary;
        else if (!Ascii && Bits == 1)
            generate = vips_foreign_load_ppm_generate_1bit_binary;
        else if (Ascii && Bits == 1)
            generate = vips_foreign_load_ppm_generate_1bit_ascii;
        else
            generate = vips_foreign_load_ppm_generate_ascii_int;

        t[0] = new VipsImage();
        vips_foreign_load_ppm_set_image(this, t[0]);
        if (vips_image_generate(t[0], null, generate, null, this, null) ||
            vips_sequential(t[0], out t[1], null))
            return null;

        return t[1];
    }

    public override int Load(VipsForeignLoad load)
    {
        VipsImage[] t = new VipsImage[2];

        if (!HaveReadHeader && ParseHeader() != 0)
            return 0;

        if (VIPS.SourceIsMappable(Source) &&
            !Ascii &&
            Bits >= 8)
        {
            if ((t[0] = vips_foreign_load_ppm_map(this)) == null)
                return -1;
        }
        else
        {
            if ((t[0] = Scan()) == null)
                return -1;
        }

        // Don't byteswap the ascii formats.
        if (vips__byteswap_bool(t[0], out t[1],
                !Ascii &&
                    VIPS.AmiMSBfirst() != MSBFirst) ||
            vips_image_write(t[1], load.Real))
            return -1;

        if (VIPS.SourceDecode(Source))
            return -1;

        return 0;
    }

    public override void ClassInit(VipsForeignLoadClass class_)
    {
        base.ClassInit(class_);
        VIPS.ObjectClass.Dispose += Dispose;
        VIPS.ObjectClass.SetProperty += SetProperty;
        VIPS.ObjectClass.GetProperty += GetProperty;
        VIPS.OperationClass.Build += Build;
        VIPS.ForeignClass.Suffs = new string[] { ".pbm", ".pgm", ".ppm", ".pfm" };
    }

    public override void Init()
    {
        Scale = 1.0;
    }
}

public class VipsForeignLoadPpmFile : VipsForeignLoadPpm
{
    public string Filename { get; set; }

    public override int Build(VipsObject obj)
    {
        if (Filename != null &&
            Source == null)
        {
            Source = new VIPS.Source(Filename);
            G.ObjectRef(Source);
        }

        return base.Build(obj);
    }

    public override void ClassInit(VipsForeignLoadClass class_)
    {
        base.ClassInit(class_);
        VIPS.ObjectClass.SetProperty += SetProperty;
        VIPS.ObjectClass.GetProperty += GetProperty;
        VIPS.OperationClass.Build += Build;
        VIPS.ForeignClass.IsASource = vips_foreign_load_ppm_is_a_source;

        VIPS.ArgString(class_, "filename", 1,
            _("Filename"),
            _("Filename to load from"),
            VIPS.Argument.RequiredInput, G.StructOffset<VipsForeignLoadPpmFile>(this, nameof(Filename)), null);
    }

    public override void Init()
    {
    }
}

public class VipsForeignLoadPpmSource : VipsForeignLoadPpm
{
    public VIPS.Source Source { get; set; }

    public override int Build(VipsObject obj)
    {
        if (Source != null)
            base.Source = Source;

        return base.Build(obj);
    }

    public override void ClassInit(VipsForeignLoadClass class_)
    {
        base.ClassInit(class_);
        VIPS.ObjectClass.SetProperty += SetProperty;
        VIPS.ObjectClass.GetProperty += GetProperty;
        VIPS.OperationClass.Build += Build;
        VIPS.ForeignClass.IsASource = vips_foreign_load_ppm_is_a_source;

        VIPS.ArgObject(class_, "source", 1,
            _("Source"),
            _("Source to load from"),
            VIPS.Argument.RequiredInput, G.StructOffset<VipsForeignLoadPpmSource>(this, nameof(Source)), VIPS.Type.Source);
    }

    public override void Init()
    {
    }
}

public class PPMLoad
{
    public static int Load(const string filename, out VipsImage image)
    {
        return vips_call_split("ppmload", null, filename, ref image);
    }

    public static int LoadSource(VIPS.Source source, out VipsImage image)
    {
        return vips_call_split("ppmload_source", null, source, ref image);
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsForeignLoad`, `VipsObject`, `VIPS.Source`, etc.) which are not defined in the provided C code snippet. You may need to modify the code to match your specific VIPS implementation.

Also, please note that this is a direct translation from the C code and might require some adjustments to work correctly with the .NET framework.