Here is the C# code equivalent to the provided C code:

```csharp
// im_png2vips(const char *name, IMAGE *out)
public static int ImPng2Vips(string name, Image out)
{
    string filename = "";
    string mode = "";

    // im_filename_split(name, filename, mode);
    var parts = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length > 1)
        filename = parts[parts.Length - 1];
    else
        filename = name;

    VipsImage x;
    if (Vips.PngLoad(filename, out, null) != 0)
        return -1;
    if (out.Write(x))
    {
        x.Dispose();
        return -1;
    }
    x.Dispose();

    return 0;
}

// ispng(const char *filename)
public static bool IsPng(string filename)
{
    return Vips.ForeignIsA("pngload", filename);
}

// vips_format_png_class_init(VipsFormatPngClass *class)
public class VipsFormatPng : VipsFormat
{
    public override string Nickname { get; } = "png";
    public override string Description { get; } = _("PNG");

    public static bool IsA(string filename) => IsPng(filename);
    public static int Load(string name, Image out) => ImPng2Vips(name, out);
    public static int Save(Image inImage, string name) => Vips.ImVips2Png(inImage, name);
    public static string[] Suffs { get; } = new[] { ".png" };
}

// vips_format_png_init(VipsFormatPng *object)
public class VipsFormatPng : VipsFormat
{
}
```

Note that this code assumes you have the `Vips` namespace available, which is not included in the standard .NET Framework. You may need to install a NuGet package or use a different library to access the VIPS functionality.

Also note that I've used the `G_DEFINE_TYPE` macro equivalent from the `GLib` library, but since this is C# we don't need it as the type registration is handled by the base class.