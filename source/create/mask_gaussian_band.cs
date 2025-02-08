```csharp
// vips_mask_gaussian_band_point (from vips_mask_gaussian_band.c)

public double VipsMaskGaussianBandPoint(VipsMask mask, double dx, double dy)
{
    VipsMaskGaussianBand gaussianBand = (VipsMaskGaussianBand)mask;

    double fcx = gaussianBand.FrequencyCutoffX;
    double fcy = gaussianBand.FrequencyCutoffY;
    double r2 = gaussianBand.Radius * gaussianBand.Radius;
    double ac = gaussianBand.AmplitudeCutoff;

    double cnst = Math.Log(ac);

    double d1 = (dx - fcx) * (dx - fcx) + (dy - fcy) * (dy - fcy);
    double d2 = (dx + fcx) * (dx + fcx) + (dy + fcy) * (dy + fcy);

    // Normalise the amplitude at (fcx, fcy) to 1.0.
    double cnsta = 1.0 / 
        (1.0 + Math.Exp(cnst * 4.0 * (fcx * fcx + fcy * fcy) / r2));

    return cnsta * (Math.Exp(cnst * d1 / r2) + Math.Exp(cnst * d2 / r2));
}

// vips_mask_gaussian_band_class_init (from vips_mask_gaussian_band.c)

public class VipsMaskGaussianBandClass : VipsObjectClass
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);

        // Set up properties
        VIPS_ARG_DOUBLE("frequency_cutoff_x", 7, "Frequency cutoff x");
        VIPS_ARG_DOUBLE("frequency_cutoff_y", 8, "Frequency cutoff y");
        VIPS_ARG_DOUBLE("radius", 9, "Radius of circle");
        VIPS_ARG_DOUBLE("amplitude_cutoff", 10, "Amplitude cutoff");

        // Set up methods
        klass.Point = new Func<VipsMask, double, double, double>(VipsMaskGaussianBandPoint);
    }
}

// vips_mask_gaussian_band_init (from vips_mask_gaussian_band.c)

public class VipsMaskGaussianBand : VipsObject
{
    public VipsMaskGaussianBand()
    {
        FrequencyCutoffX = 0.5;
        FrequencyCutoffY = 0.5;
        Radius = 0.1;
        AmplitudeCutoff = 0.5;
    }
}

// vips_mask_gaussian_band (from vips_mask_gaussian_band.c)

public int VipsMaskGaussianBand(VipsImage out, int width, int height,
    double frequency_cutoff_x, double frequency_cutoff_y, double radius,
    double amplitude_cutoff, params)
{
    // Call the split function
    return VipsCallSplit("mask_gaussian_band", out, width, height,
        frequency_cutoff_x, frequency_cutoff_y, radius,
        amplitude_cutoff);
}
```