```csharp
// C method: im_rad2vips
public static int ImRad2Vips(string filename, Image out)
{
    VipsImage t;

    if (Vips.RadLoad(filename, out, out))
        return -1;
    if (!Vips.ImageWrite(out, out))
    {
        GObject.Unref(t);
        return -1;
    }
    GObject.Unref(t);

    return 0;
}

// C method: israd
public static bool IsRad(string filename)
{
    return Vips.ForeignIsA("radload", filename);
}

// C method: im_vips2rad
public static int ImVips2Rad(Image in, string filename)
{
    return Vips.RadSave(in, filename, null);
}

// C method: vips_format_rad_class_init
public class VipsFormatRadClass : VipsObjectClass
{
    public override string Nickname { get; set; } = "rad";
    public override string Description { get; set; } = _("Radiance");

    public bool IsA(string filename)
    {
        return IsRad(filename);
    }

    public int Load(string filename, Image out)
    {
        return ImRad2Vips(filename, out);
    }

    public int Save(Image in, string filename)
    {
        return ImVips2Rad(in, filename);
    }

    public string[] Suffs { get; set; } = new string[] { ".hdr", null };
}

// C method: vips_format_rad_init
public class VipsFormatRad : VipsObject
{
}

// C method: G_DEFINE_TYPE
public static void DefineType(Type type, Type base_type)
{
    // Implementation of G_DEFINE_TYPE macro
}
```