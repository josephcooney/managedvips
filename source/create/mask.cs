```csharp
// vips_mask_point
public static double VipsMaskPoint(VipsPoint point, int x, int y)
{
    // VIPS_MAX to prevent /0.
    int halfWidth = Math.Max(point.Width / 2, 1);
    int halfHeight = Math.Max(point.Height / 2, 1);

    double result;

    // Move centre for an optical transform mask.
    if (!point.Mask.Optical)
    {
        x = (x + halfWidth) % point.Width;
        y = (y + halfHeight) % point.Height;
    }

    x -= halfWidth;
    y -= halfHeight;

    if (!point.Mask.NoDC && x == 0 && y == 0)
        // DC component is always 1.
        result = 1.0;
    else
    {
        double dx, dy;

        dx = (double)x / halfWidth;
        dy = (double)y / halfHeight;

        result = point.Mask.Class.Point(mask: point.Mask, dx: dx, dy: dy);

        // Invert filter sense for a highpass filter, or to swap band-pass for band-reject.
        if (point.Mask.Reject)
            result = 1.0 - result;
    }

    return result;
}

// vips_mask_class_init
public class VipsMaskClass : VipsPointClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // gobject_class->set_property = vips_object_set_property;
        // gobject_class->get_property = vips_object_get_property;

        Nickname = "mask";
        Description = _("base class for frequency filters");

        Point = VipsMaskPoint;
        Min = 0.0;
        Max = 1.0;
        Interpretation = VIPS_INTERPRETATION_FOURIER;

        // VIPS_ARG_BOOL(class, "optical", 5,
        //     _("Optical"),
        //     _("Rotate quadrants to optical space"),
        //     VIPS_ARGUMENT_OPTIONAL_INPUT,
        //     G_STRUCT_OFFSET(VipsMask, optical),
        //     FALSE);

        // VIPS_ARG_BOOL(class, "reject", 5,
        //     _("Reject"),
        //     _("Invert the sense of the filter"),
        //     VIPS_ARGUMENT_OPTIONAL_INPUT,
        //     G_STRUCT_OFFSET(VipsMask, reject),
        //     FALSE);

        // VIPS_ARG_BOOL(class, "nodc", 5,
        //     _("Nodc"),
        //     _("Remove DC component"),
        //     VIPS_ARGUMENT_OPTIONAL_INPUT,
        //     G_STRUCT_OFFSET(VipsMask, nodc),
        //     FALSE);
    }
}

// vips_mask_init
public class VipsMask : VipsPoint
{
    public override void Init()
    {
        base.Init();
    }
}
```