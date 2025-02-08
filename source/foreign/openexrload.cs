```csharp
// vips_foreign_load_openexr_get_flags_filename
public static VipsForeignFlags GetFlagsFilename(string filename)
{
    var flags = 0;
    if (Vips.__OpenEXR_IsTiled(filename))
        flags |= Vips.ForeignFlags.Partial;

    return flags;
}

// vips_foreign_load_openexr_get_flags
public static VipsForeignFlags GetFlags(VipsForeignLoad load)
{
    var openexr = (VipsForeignLoadOpenexr)load;

    return GetFlagsFilename(openexr.Filename);
}

// vips_foreign_load_openexr_header
public static int Header(VipsForeignLoad load)
{
    var openexr = (VipsForeignLoadOpenexr)load;

    if (!Vips.__OpenEXR_ReadHeader(openexr.Filename, load.Out))
        return -1;

    VIPS.SetStr(load.Out.Filename, openexr.Filename);

    return 0;
}

// vips_foreign_load_openexr_load
public static int Load(VipsForeignLoad load)
{
    var openexr = (VipsForeignLoadOpenexr)load;

    if (!Vips.__OpenEXR_Read(openexr.Filename, load.Real))
        return -1;

    return 0;
}

// vips_foreign_openexr_suffs
public static string[] Suffixes { get; } = new string[] { ".exr", null };

// vips_foreign_load_openexr_class_init
public class VipsForeignLoadOpenexrClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        ObjectClass.Nickname = "openexrload";
        ObjectClass.Description = _("load an OpenEXR image");

        OperationClass.Flags |= Vips.OperationFlags.Untrusted;

        ForeignClass.Suffixes = Suffixes;
        ForeignClass.Priority = 200;

        LoadClass.IsA = Vips.__OpenEXR_IsExr;
        LoadClass.GetFlagsFilename = GetFlagsFilename;
        LoadClass.GetFlags = GetFlags;
        LoadClass.Header = Header;
        LoadClass.Load = Load;

        ArgString("filename", 1, _("Filename"), _("Filename to load from"),
            Vips.ArgumentRequiredInput, typeof(VipsForeignLoadOpenexr), "filename");
    }
}

// vips_foreign_load_openexr_init
public class VipsForeignLoadOpenexr : VipsObject
{
    public string Filename { get; set; }

    public VipsForeignLoadOpenexr()
    {
        // empty constructor
    }
}

// vips_openexrload
public static int OpenEXRLoad(string filename, out VipsImage image)
{
    var result = Vips.CallSplit("openexrload", filename, out image);

    return result;
}
```