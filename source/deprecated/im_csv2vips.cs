```csharp
// im_csv2vips

public int ImCsv2Vips(string filename, out Image out)
{
    // Read options.
    int startSkip = 0;
    string whitespace = " ";
    string separator = ";,\t";
    int lines = -1;

    char[] name = new char[FILENAME_MAX];
    char[] mode = new char[FILENAME_MAX];
    string p, q, r;

    VipsImage x;

    // Parse mode string.
    ImFilenameSplit(filename, name, mode);
    p = &mode[0];
    while ((q = ImGetNextOption(&p)) != null)
    {
        if (ImIsPrefix("ski", q) && (r = ImGetSuboption(q)) != null)
            startSkip = int.Parse(r);
        else if (ImIsPrefix("whi", q) && (r = ImGetSuboption(q)) != null)
            whitespace = r;
        else if (ImIsPrefix("sep", q) && (r = ImGetSuboption(q)) != null)
            separator = r;
        else if (ImIsPrefix("lin", q) && (r = ImGetSuboption(q)) != null)
            lines = int.Parse(r);
    }

    if (VipsCsvload(name, out ref x,
        "skip", startSkip,
        "lines", lines,
        "whitespace", whitespace,
        "separator", separator,
        null))
        return -1;
    if (VipsImageWrite(x, out))
    {
        g_object_unref(x);
        return -1;
    }
    g_object_unref(x);

    return 0;
}

// vips_format_csv_class_init

public class VipsFormatCsvClass : VipsObjectClass
{
    public static void VipsFormatCsvClassInit(Type type)
    {
        VipsObjectClass objectClass = (VipsObjectClass)type;
        VipsFormatClass formatClass = (VipsFormatClass)type;

        objectClass.nickname = "csv";
        objectClass.description = _("CSV");

        formatClass.load = ImCsv2Vips;
        formatClass.save = ImVips2Csv;
        formatClass.suffs = new string[] { ".csv", null };
    }
}

// vips_format_csv_init

public class VipsFormatCsv : VipsObject
{
    public static void VipsFormatCsvInit(Type type)
    {
    }

    public static Type RegisterType()
    {
        return G_define_type(VipsFormatCsv, VIPS_TYPE_FORMAT);
    }
}
```