```csharp
// im_mat2vips (const char *filename, IMAGE *out)

public int ImMat2Vips(string filename, Image out)
{
    VipsImage t;

    if (Vips.MatLoad(filename, out))
        return -1;
    if (!Vips.ImageWrite(t, out))
    {
        g_object_unref(t);
        return -1;
    }
    g_object_unref(t);

    return 0;
}

// ismat (const char *filename)

public static bool IsMat(string filename)
{
    return Vips.ForeignIsA("matload", filename);
}

// mat_suffs

private const string[] MatSuffs = { ".mat", null };

// vips_format_mat_class_init (VipsFormatMatClass *class)

public class VipsFormatMat : VipsFormat
{
    public static void VipsFormatMatClassInit(Type type)
    {
        var objectClass = (VipsObjectClass)type;
        var formatClass = (VipsFormatClass)type;

        objectClass.Nickname = "mat";
        objectClass.Description = _("Matlab");

        formatClass.IsA = IsMat;
        formatClass.Load = ImMat2Vips;
        formatClass.Save = null;
        formatClass.Suffs = MatSuffs;
    }
}

// vips_format_mat_init (VipsFormatMat *object)

public class VipsFormatMat : VipsFormat
{
    public VipsFormatMat()
    {
    }
}
```