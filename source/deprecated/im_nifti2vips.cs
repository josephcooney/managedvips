```csharp
// im_nifti2vips
public static int ImNifti2Vips(string name, Image out)
{
    using (var t = new VipsImage())
    {
        if (Vips.Niftiload(name, out))
            return -1;
        if (!out.Write(t))
        {
            g_object_unref(t);
            return -1;
        }
        g_object_unref(t);

        return 0;
    }
}

// nifti_suffs
private static readonly string[] NiftiSuffs = new[]
{
    ".nii", ".nii.gz",
    ".hdr", ".hdr.gz",
    ".img", ".img.gz",
    ".nia", ".nia.gz"
};

// nifti_flags
public static VipsFormatFlags NiftiFlags(string name)
{
    var filename = Path.GetFileName(name);
    var mode = Path.GetExtension(name);

    return (VipsFormatFlags)Vips.ForeignFlags("niftiload", filename);
}

// isnifti
public static bool IsNifti(string name)
{
    var filename = Path.GetFileName(name);
    var mode = Path.GetExtension(name);

    return Vips.ForeignIsA("niftiload", filename);
}

// vips_format_nifti_class_init
public class VipsFormatNiftiClass : VipsObjectClass, VipsFormatClass
{
    public override string Nickname => "im_nifti";
    public override string Description => _("NIfTI");

    public int Priority => 100;
    public Func<string, bool> IsA => isnifti;
    public Func<string, Image, int> Load => im_nifti2vips;
    public Func<string, VipsFormatFlags> GetFlags => nifti_flags;
    public string[] Suffs => NiftiSuffs;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

// vips_format_nifti_init
public class VipsFormatNifti : VipsFormatNiftiClass
{
}

public static class VipsFormatNiftiType
{
    public static readonly Type Type = typeof(VipsFormatNifti);

    public static void Register(Type type)
    {
        // G_DEFINE_TYPE equivalent in C#
        var vips_format_nifti_class = (VipsFormatNiftiClass)Activator.CreateInstance(Type);
        VipsObjectClass.Register(type, vips_format_nifti_class);
    }
}
```