Here is the C# code equivalent to the provided C code:

```csharp
// From vips_foreign_save_vips.c.
public class VipsForeignSaveVips : VipsForeignSave
{
    public VipsTarget Target { get; private set; }

    protected override int Build(VipsObject obj)
    {
        // vips_image_build() has some magic for "w"
        // preventing recursion and sending this directly to the
        // saver built into iofuncs.
        if (base.Build(obj) != 0)
            return -1;

        string filename = VipsConnectionFilename(VipsConnection.Target);

        if (!string.IsNullOrEmpty(filename))
        {
            VipsImage x = VipsImage.NewMode(filename, "w");

            if (VipsImage.Write(Ready, x) != 0)
            {
                GObject.Unref(x);
                return -1;
            }

            GObject.Unref(x);
        }
        else
        {
            // We could add load vips from memory, fd, via mmap etc. here.
            // We should perhaps move iofuncs/vips.c into this file.

            VipsObjectClass class = (VipsObjectClass)VipsObject.GetClass(obj);

            VipsError(class.Nickname, "%s", _("no filename associated with target"));
            return -1;
        }

        if (VipsTarget.End(Target) != 0)
            return -1;

        return 0;
    }
}

// From vips_foreign_save_vips_file.c.
public class VipsForeignSaveVipsFile : VipsForeignSaveVips
{
    public string Filename { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (base.Build(obj) != 0)
            return -1;

        Target = VipsTarget.NewToFile(Filename);

        return 0;
    }
}

// From vips_foreign_save_vips_target.c.
public class VipsForeignSaveVipsTarget : VipsForeignSaveVips
{
    public VipsTarget Target { get; set; }

    protected override int Build(VipsObject obj)
    {
        base.Target = Target;

        if (base.Build(obj) != 0)
            return -1;

        return 0;
    }
}

// From vipsload.c.
public static class VipsForeignSaveVipsClass
{
    public const string[] Suffs = Vips__Suffs;
}

// From vips_foreign_save_vips.c.
public static int VipsVipssave(VipsImage in, string filename)
{
    return VipsCallSplit("vipssave", in, filename);
}

// From vips_foreign_save_vips_target.c.
public static int VipsVipssaveTarget(VipsImage in, VipsTarget target)
{
    return VipsCallSplit("vipssave_target", in, target);
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsForeignSave`, `VipsObject`, `GObject`, etc.) which are not defined in this code snippet but are likely part of a larger VIPS library or framework.