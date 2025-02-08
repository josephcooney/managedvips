```csharp
// im_exr2vips
public static int ImExr2Vips(string filename, Image out)
{
    #if HAVE_OPENEXR
        return VipsOpenexrRead(filename, out);
    #else
        throw new Exception("no OpenEXR support in your libvips");
    #endif
}

// exr_suffs
private static readonly string[] ExrSuffs = { ".exr", null };

// exr_flags
public static VipsFormatFlags ExrFlags(string name)
{
    var filename = new char[FILENAME_MAX];
    var mode = new char[FILENAME_MAX];

    ImFilenameSplit(name, filename, mode);

    return (VipsFormatFlags)VipsForeignFlags("openexrload", filename);
}

// isexr
public static bool IsExr(string name)
{
    var filename = new char[FILENAME_MAX];
    var mode = new char[FILENAME_MAX];

    ImFilenameSplit(name, filename, mode);

    return VipsForeignIsA("openexrload", filename);
}

// vips_format_exr_class_init
public class VipsFormatExrClass : VipsObjectClass
{
    public override string Nickname => "exr";
    public override string Description => _("OpenEXR");

    public static bool IsA(string name)
    {
        var filename = new char[FILENAME_MAX];
        var mode = new char[FILENAME_MAX];

        ImFilenameSplit(name, filename, mode);

        return VipsForeignIsA("openexrload", filename);
    }

    public static int Load(string filename, Image out)
    {
        #if HAVE_OPENEXR
            return VipsOpenexrRead(filename, out);
        #else
            throw new Exception("no OpenEXR support in your libvips");
        #endif
    }

    public static VipsFormatFlags GetFlags(string name)
    {
        var filename = new char[FILENAME_MAX];

        ImFilenameSplit(name, filename);

        return (VipsFormatFlags)VipsForeignFlags("openexrload", filename);
    }

    public static string[] Suffs => ExrSuffs;
}

// vips_format_exr_init
public class VipsFormatExr : VipsObject
{
}
```