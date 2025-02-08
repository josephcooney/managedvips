```csharp
// Converted from: vips_eye_point()

public float VipsEyePoint(VipsPoint point, int x, int y)
{
    // VIPS_MAX to prevent /0.
    int max_x = Math.Max(point.Width - 1, 1);
    int max_y = Math.Max(point.Height - 1, 1);

    double c = ((VipsEye)point).Factor * Math.PI / (2 * max_x);
    double h = max_y * max_y;

    return (float)(y * y * Math.Cos(c * x * x) / h);
}

// Converted from: vips_eye_class_init()

public class VipsEyeClass : VipsPointClass
{
    public static void ClassInit(VipsEyeClass klass)
    {
        // GObjectClass and VipsObjectClass are handled by the base classes

        VIPS_ARG_DOUBLE(klass, "factor", 6,
            _("Factor"),
            _("Maximum spatial frequency"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsEye).GetField("factor").Offset,
            0.0f, 1.0f, 0.5f);
    }
}

// Converted from: vips_eye_init()

public class VipsEye : VipsPoint
{
    public double Factor { get; set; }

    public VipsEye() : base()
    {
        this.Factor = 0.5;
    }
}

// Converted from: vips_eye()

public int VipsEye(VipsImage[] out, int width, int height, params object[] args)
{
    // This method is not directly equivalent to the C version,
    // as it uses a different calling convention and does not
    // support varargs. However, it can be used in a similar way.
    return VipsCallSplit("eye", out, width, height, args);
}
```