```csharp
// vips_unary_build
public int Build(VipsObject object)
{
    VipsArithmetic arithmetic = (VipsArithmetic)object;
    VipsUnary unary = (VipsUnary)object;

    arithmetic.N = 1;
    arithmetic.In = new VipsImage*[1];
    arithmetic.In[0] = unary.In;
    if (arithmetic.In[0] != null)
        ((VipsObject)arithmetic.In[0]).Ref();

    if (((VipsObjectClass)vips_unary_parent_class).Build(object))
        return -1;

    return 0;
}

// vips_unary_class_init
public class VipsUnaryClass : VipsArithmeticClass
{
    public VipsUnaryClass()
    {
        // Create properties.
        VIPS_ARG_IMAGE("in", 1,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsUnary).GetField("In"));
    }
}

// vips_unary_init
public void Init(VipsUnary unary)
{
    // Init our instance fields.
}

// vips_unary_copy
public int Copy(VipsUnary unary)
{
    VipsArithmetic arithmetic = (VipsArithmetic)unary;

    // This isn't set by arith until build(), so we have to set again here.
    // Should arith set out in _init()?
    ((VipsObject)unary).SetProperty("out", new VipsImage());

    return vips_image_write(unary.In, arithmetic.Out);
}
```