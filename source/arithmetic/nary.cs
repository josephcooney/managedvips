```csharp
// vips_nary_build
public int Build(VipsObject object)
{
    VipsArithmetic arithmetic = (VipsArithmetic)object;
    VipsNary nary = (VipsNary)object;

    if (nary.In != null)
    {
        arithmetic.In = nary.In.Data;
        arithmetic.N = nary.In.N;
    }

    if (((VipsObjectClass)vips_nary_parent_class).Build(object) == -1)
        return -1;

    return 0;
}

// vips_nary_class_init
public class VipsNaryClass : VipsArithmeticClass
{
    public static void ClassInit(VipsNaryClass klass)
    {
        GObjectClass gobject_class = (GObjectClass)klass;
        VipsObjectClass vobject_class = (VipsObjectClass)klass;

        gobject_class.SetProperty = vips_object_set_property;
        gobject_class.GetProperty = vips_object_get_property;

        vobject_class.Nickname = "nary";
        vobject_class.Description = _("nary operations");
        vobject_class.Build = new VipsNaryBuild(Build);

        // Create properties.
        VIPS_ARG_BOXED(klass, "in", 0,
            _("Input"),
            _("Array of input images"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsNary, In),
            VIPS_TYPE_ARRAY_IMAGE);
    }
}

// vips_nary_init
public class VipsNary : VipsArithmetic
{
    public VipsNary()
    {
        // Init our instance fields.
    }
}
```