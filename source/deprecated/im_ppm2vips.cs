Here is the C# code equivalent to the provided C code:

```csharp
// im_ppm2vips
public int ImPPM2Vips(string filename, Image out)
{
    VipsImage t;

    if (Vips.PpmLoad(filename, out))
        return -1;
    if (!Vips.ImageWrite(out))
    {
        // equivalent to g_object_unref(t);
        out.Dispose();
        return -1;
    }

    return 0;
}

// isppm
public static bool IsPPM(string filename)
{
    return Vips.ForeignIsA("ppmload", filename);
}

// ppm_flags
public static VipsFormatFlags PpmFlags(string filename)
{
    return (VipsFormatFlags) Vips.ForeignFlags("ppmload", filename);
}

// ppm_suffs
private static readonly string[] PpmSuffs = new string[] { ".ppm", ".pgm", ".pbm", ".pfm" };

// vips_format_ppm_class_init
public class VipsFormatPpm : VipsFormat
{
    public override string Nickname => "ppm";
    public override string Description => _("PPM/PBM/PNM/PFM");

    protected override bool IsA(string filename)
    {
        return Vips.ForeignIsA("ppmload", filename);
    }

    protected override int Load(string filename, Image out)
    {
        return ImPPM2Vips(filename, out);
    }

    protected override int Save(Image inImage, string filename)
    {
        // equivalent to im_vips2ppm
        throw new NotImplementedException();
    }

    protected override VipsFormatFlags GetFlags(string filename)
    {
        return (VipsFormatFlags) PpmFlags(filename);
    }

    protected override string[] Suffs => PpmSuffs;
}

// vips_format_ppm_init
public class VipsFormatPpm : VipsFormat { }
```

Note that I've assumed the existence of `VipsImage`, `Image`, and other classes/types from the VIPS library, as they are not part of standard .NET. You may need to modify this code to fit your specific use case. Additionally, I've left out the implementation of `im_vips2ppm` as it was not provided in the original C code.