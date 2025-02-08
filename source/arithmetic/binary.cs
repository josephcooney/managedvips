```csharp
// vips_binary_build method converted from C code
public int Build(VipsObject object)
{
    VipsArithmetic arithmetic = (VipsArithmetic)object;
    VipsBinary binary = (VipsBinary)object;

    arithmetic.N = 2;
    arithmetic.In = new VipsImage[2];
    arithmetic.In[0] = binary.Left;
    arithmetic.In[1] = binary.Right;

    if (arithmetic.In[0] != null)
        GRef(arithmetic.In[0]);
    if (arithmetic.In[1] != null)
        GRef(arithmetic.In[1]);

    return VIPS_OBJECT_CLASS(vips_binary_parent_class).Build(object);
}

// vips_binary_class_init method converted from C code
public class VipsBinaryClass : VipsObjectClass
{
    public override void ClassInit(VipsBinaryClass klass)
    {
        base.ClassInit(klass);

        // Create properties.
        VIPS_ARG_IMAGE("left", 1, _("Left"), _("Left-hand image argument"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsBinary, left));
        VIPS_ARG_IMAGE("right", 2, _("Right"), _("Right-hand image argument"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsBinary, right));
    }
}

// vips_binary_init method converted from C code
public void Init(VipsBinary binary)
{
    // Init our instance fields.
}
```