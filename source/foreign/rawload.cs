```csharp
// vips_foreign_load_raw_get_flags
public static VipsForeignFlags GetFlags(VipsForeignLoad load)
{
    return VIPS_FOREIGN_PARTIAL;
}

// vips_foreign_load_raw_get_flags_filename
public static VipsForeignFlags GetFlagsFilename(string filename)
{
    return VIPS_FOREIGN_PARTIAL;
}

// vips_foreign_load_raw_header
public static int Header(VipsForeignLoad load)
{
    var raw = (VipsForeignLoadRaw)load;

    VipsImage out;
    VipsImage x;

    if (!(out = VipsImage.NewFromFileRaw(raw.Filename,
        raw.Width, raw.Height,
        VipsFormat.SizeOfUnsafe((VipsBandFormat)raw.Format) * raw.Bands,
        raw.Offset)))
        return -1;

    if (VipsCopy(out, ref x,
        "interpretation", raw.Interpretation,
        "format", raw.Format,
        "bands", raw.Bands,
        null))
    {
        out.Dispose();
        return -1;
    }
    out.Dispose();
    out = x;

    // Remove the @out that's there now.
    var loadOut = (VipsImage)load.GetProperty("out");
    loadOut.Dispose();

    ((VipsForeignLoadRaw)load).SetProperty("out", out);

    return 0;
}

// vips_foreign_load_raw_class_init
public class VipsForeignLoadRawClass : VipsObjectClass
{
    public override void ClassInit(VipsObjectClass gobjectClass)
    {
        base.ClassInit(gobjectClass);
        var operationClass = (VipsOperationClass)gobjectClass;
        var loadClass = (VipsForeignLoadClass)gobjectClass;

        // You're unlikely to want to use this on untrusted files.
        operationClass.Flags |= VIPS_OPERATION_UNTRUSTED;

        gobjectClass.SetProperty = VipsObject.SetProperty;
        gobjectClass.GetProperty = VipsObject.GetProperty;

        loadClass.GetFlags = GetFlags;
        loadClass.GetFlagsFilename = GetFlagsFilename;
        loadClass.Header = Header;

        VipsArgString("filename", 1, _("Filename"), _("Filename to load from"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Filename),
            null);

        VipsArgInt("width", 20, _("Width"), _("Image width in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Width),
            0, VIPS_MAX_COORD, 0);

        VipsArgInt("height", 21, _("Height"), _("Image height in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Height),
            0, VIPS_MAX_COORD, 0);

        VipsArgInt("bands", 22, _("Bands"), _("Number of bands in image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Bands),
            0, VIPS_MAX_COORD, 0);

        VipsArgUInt64("offset", 23, _("Size of header"), _("Offset in bytes from start of file"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Offset),
            0, 100000000000, 0);

        VipsArgEnum("format", 24, _("Format"), _("Pixel format in image"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Format),
            typeof(VipsBandFormat), (int)VipsBandFormat.UCHAR);

        VipsArgEnum("interpretation", 25, _("Interpretation"), _("Pixel interpretation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadRaw, Interpretation),
            typeof(VipsInterpretation), (int)VipsInterpretation.Multiband);
    }
}

// vips_foreign_load_raw_init
public class VipsForeignLoadRaw : VipsObject
{
    public VipsForeignLoadRaw()
    {
        Format = VipsBandFormat.UCHAR;
        Interpretation = VipsInterpretation.Multiband;
    }
}

// vips_rawload
public static int Rawload(string filename, ref VipsImage out,
    int width, int height, int bands, params object[] args)
{
    var result = VipsCallSplit("rawload", filename, ref out, width, height, bands);
    return result;
}
```