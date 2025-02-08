Here is the converted C# code:

```csharp
// vips_foreign_load_mat_get_flags_filename (from vips_foreign_load_mat.c)

public static VipsForeignFlags GetFlagsFilename(string filename)
{
    return 0;
}

// vips_foreign_load_mat_get_flags (from vips_foreign_load_mat.c)

public static VipsForeignFlags GetFlags(VipsForeignLoad load)
{
    var mat = (VipsForeignLoadMat)load;

    return GetFlagsFilename(mat.Filename);
}

// vips_foreign_load_mat_header (from vips_foreign_load_mat.c)

public static int Header(VipsForeignLoad load)
{
    var mat = (VipsForeignLoadMat)load;

    if (Vips.__mat_header(mat.Filename, load.Out))
        return -1;

    VIPS.SetStr(load.Out.Filename, mat.Filename);

    return 0;
}

// vips_foreign_load_mat_load (from vips_foreign_load_mat.c)

public static int Load(VipsForeignLoad load)
{
    var mat = (VipsForeignLoadMat)load;

    if (Vips.__mat_load(mat.Filename, load.Real))
        return -1;

    return 0;
}

// vips_foreign_load_mat_class_init (from vips_foreign_load_mat.c)

public class VipsForeignLoadMatClass : VipsObjectClass
{
    public override void SetProperty(GObject gobject, string name, Value value)
    {
        base.SetProperty(gobject, name, value);
    }

    public override Value GetProperty(GObject gobject, string name)
    {
        return base.GetProperty(gobject, name);
    }
}

public class VipsForeignLoadMat : VipsForeignLoad
{
    public string Filename { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (Filename != null)
            Glib.Free(Filename);

        base.Dispose(disposing);
    }
}

// vips_matload (from vips_foreign_load_mat.c)

public static int Matload(string filename, out VipsImage image, params object[] args)
{
    var result = Vips.CallSplit("matload", filename, out image, args);

    return result;
}
```