```csharp
// im_analyze2vips
public int Im_Analyze2Vips(string filename, Image out)
{
    VipsImage t;

    if (VipsForeign.AnalyzeLoad(filename, out, out))
        return -1;
    if (!out.Write(t))
    {
        g_object_unref(t);
        return -1;
    }
    g_object_unref(t);

    return 0;
}

// isanalyze
public bool IsAnalyze(string filename)
{
    return VipsForeign.IsA("analyzeload", filename);
}

// analyze_flags
public VipsFormatFlags AnalyzeFlags(string filename)
{
    return (VipsFormatFlags)VipsForeign.Flags("analyzeload", filename);
}

// vips_format_analyze_class_init
private static void VipsFormatAnalyzeClassInit(Type type)
{
    VipsObjectClass objectClass = (VipsObjectClass)type;
    VipsFormatClass formatClass = (VipsFormatClass)type;

    objectClass.Nickname = "analyze";
    objectClass.Description = _("Analyze 6.0");

    formatClass.IsA = IsAnalyze;
    formatClass.Load = Im_Analyze2Vips;
    formatClass.GetFlags = AnalyzeFlags;
    formatClass.Suffs = new string[] { ".img", ".hdr", null };
}

// vips_format_analyze_init
private static void VipsFormatAnalyzeInit(VipsFormatAnalyze obj)
{
}

public class VipsFormatAnalyze : VipsFormat
{
    public override Type GetObjectType()
    {
        return typeof(VipsFormatAnalyze);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
```