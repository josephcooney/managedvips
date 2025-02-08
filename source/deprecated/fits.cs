```csharp
// im_fits2vips
public int ImFits2Vips(string filename, VipsImage out)
{
    VipsImage t;

    if (Vips.FITSLoad(filename, out))
        return -1;
    if (out.Write(t))
    {
        t.Dispose();
        return -1;
    }
    t.Dispose();

    return 0;
}

// im_vips2fits
public int ImVips2Fits(VipsImage in, string filename)
{
    if (in.FITSSave(filename))
        return -1;

    return 0;
}

// isfits
private bool IsFITS(string name)
{
    return Vips.ForeignIsA("fitsload", name);
}

// fits_suffs
private static readonly string[] FitsSuffs = { ".fits", null };

// vips_format_fits_class_init
public class VipsFormatFits : VipsFormat
{
    public override string Nickname => "fits";
    public override string Description => _("FITS");

    public bool IsA(string name) => IsFITS(name);
    public int Load(string filename, VipsImage out) => ImFits2Vips(filename, out);
    public int Save(VipsImage in, string filename) => ImVips2Fits(in, filename);
    public string[] Suffs => FitsSuffs;
}

// vips_format_fits_init
public class VipsFormatFits : VipsFormat { }

public static class VipsFormatFitsClass
{
    public static void Init(VipsFormatFits obj) { }
}
```