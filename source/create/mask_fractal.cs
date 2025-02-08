```csharp
// vips_mask_fractal_point
public double VipsMaskFractalPoint(VipsImage mask, double dx, double dy)
{
    VipsMaskFractal fractal = (VipsMaskFractal)mask;
    double fd = (fractal.FractalDimension - 4.0) / 2.0;

    double d2 = dx * dx + dy * dy;

    return Math.Pow(d2, fd);
}

// vips_mask_fractal_class_init
public class VipsMaskFractalClass : VipsObjectClass
{
    public VipsMaskFractalClass()
    {
        // GObjectClass
        this.SetProperty += VipsObjectSetProperty;
        this.GetProperty += VipsObjectGetProperty;

        // VipsObjectClass
        this.Nickname = "mask_fractal";
        this.Description = _("make fractal filter");

        // VipsMaskClass
        this.Point = VipsMaskFractalPoint;

        // VIPS_ARG_DOUBLE
        VipsArgDouble fractalDimensionArg = new VipsArgDouble("fractal_dimension", 8,
            _("Fractal dimension"),
            _("Fractal dimension"),
            VipsArgument.RequiredInput,
            typeof(VipsMaskFractal).GetField("FractalDimension").Offset,
            2.0, 3.0, 2.5);
    }
}

// vips_mask_fractal_init
public class VipsMaskFractal : VipsObject
{
    public double FractalDimension { get; set; }

    public VipsMaskFractal()
    {
        this.FractalDimension = 2.5;
    }
}

// vips_mask_fractal
public int VipsMaskFractal(VipsImage[] out, int width, int height,
    double fractalDimension, params object[] args)
{
    // va_start and va_end are not needed in C#
    return VipsCallSplit("mask_fractal", out, width, height, fractalDimension);
}
```