```csharp
// vips_shrink_build (from vips_shrink.c)
public int VipsShrinkBuild(VipsObject obj)
{
    VipsResample resample = (VipsResample)obj;
    VipsShrink shrink = (VipsShrink)obj;
    VipsImage[] t = new VipsImage[2];

    // Check if the parent class build method returns an error
    if (VIPS_OBJECT_CLASS(VipsShrinkParentClass).Build(obj) != 0)
        return -1;

    int hshrink_int = (int)Math.Round(shrink.Hshrink);
    int vshrink_int = (int)Math.Round(shrink.Vshrink);

    // If the shrink factors are integers, use a box filter
    if (hshrink_int != shrink.Hshrink || vshrink_int != shrink.Vshrink)
    {
        // Shrink by integer factors and reduce to final size
        if (VipsReduceVertical(resample.In, t[0], shrink.Vshrink, "gap", 1.0) ||
            VipsReduceHorizontal(t[0], t[1], shrink.Hshrink, "gap", 1.0) ||
            VipsImageWrite(t[1], resample.Out))
            return -1;
    }
    else
    {
        // Use a box filter for non-integer factors
        if (VipsShrinkVertical(resample.In, t[0], shrink.Vshrink, "ceil", shrink.Ceil) ||
            VipsShrinkHorizontal(t[0], t[1], shrink.Hshrink, "ceil", shrink.Ceil) ||
            VipsImageWrite(t[1], resample.Out))
            return -1;
    }

    return 0;
}

// vips_shrink_class_init (from vips_shrink.c)
public class VipsShrinkClass : VipsResampleClass
{
    public static void VipsShrinkClassInit(VipsShrinkClass klass)
    {
        // Initialize the GObject and VipsObject classes
        GObjectClass gobject_class = G_OBJECT_CLASS(klass);
        VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(klass);

        // Set up the properties for this class
        VIPS_ARG_DOUBLE(klass, "vshrink", 9,
            _("Vshrink"),
            _("Vertical shrink factor"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsShrink, Vshrink),
            1.0, 1000000.0, 1.0);

        VIPS_ARG_DOUBLE(klass, "hshrink", 8,
            _("Hshrink"),
            _("Horizontal shrink factor"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsShrink, Hshrink),
            1.0, 1000000.0, 1.0);

        VIPS_ARG_BOOL(klass, "ceil", 10,
            _("Ceil"),
            _("Round-up output dimensions"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsShrink, Ceil),
            false);

        // The old names .. now use h and v everywhere.
        VIPS_ARG_DOUBLE(klass, "xshrink", 8,
            _("Xshrink"),
            _("Horizontal shrink factor"),
            VIPS_ARGUMENT_REQUIRED_INPUT | VIPS_ARGUMENT_DEPRECATED,
            G_STRUCT_OFFSET(VipsShrink, Hshrink),
            1.0, 1000000.0, 1.0);

        VIPS_ARG_DOUBLE(klass, "yshrink", 9,
            _("Yshrink"),
            _("Vertical shrink factor"),
            VIPS_ARGUMENT_REQUIRED_INPUT | VIPS_ARGUMENT_DEPRECATED,
            G_STRUCT_OFFSET(VipsShrink, Vshrink),
            1.0, 1000000.0, 1.0);
    }
}

// vips_shrink_init (from vips_shrink.c)
public class VipsShrink : VipsResample
{
    public VipsShrink() { }
}

// vips_shrink (from vips_shrink.c)
public int VipsShrink(VipsImage in, out VipsImage[] out,
    double hshrink, double vshrink, params object[] args)
{
    // Call the split function to handle the arguments
    return VipsCallSplit("shrink", args, in, out, hshrink, vshrink);
}
```