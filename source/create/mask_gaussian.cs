```csharp
// vips_mask_gaussian_point (from gaussian.c)
public static double VipsMaskGaussianPoint(VipsMask mask, double dx, double dy)
{
    VipsMaskGaussian gaussian = (VipsMaskGaussian)mask;
    double fc = gaussian.FrequencyCutoff;
    double ac = gaussian.AmplitudeCutoff;

    double cnst = Math.Log(ac);
    double fc2 = fc * fc;
    double dist2 = (dx * dx + dy * dy) / fc2;

    return 1.0 - Math.Exp(cnst * dist2);
}

// vips_mask_gaussian_class_init (from gaussian.c)
public class VipsMaskGaussianClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Set property callbacks
        this.SetProperty = VipsObject.SetProperty;
        this.GetProperty = VipsObject.GetProperty;

        // Set class properties
        this.Nickname = "mask_gaussian";
        this.Description = _("make a gaussian filter");

        // Set mask properties
        this.Point = VipsMaskGaussianPoint;

        // Define properties
        VIPS_ARG_DOUBLE(this, "frequency_cutoff", 7,
            _("Frequency cutoff"),
            _("Frequency cutoff"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsMaskGaussian).GetField("frequency_cutoff").Offset,
            0.0, 1000000.0, 0.5);

        VIPS_ARG_DOUBLE(this, "amplitude_cutoff", 8,
            _("Amplitude cutoff"),
            _("Amplitude cutoff"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsMaskGaussian).GetField("amplitude_cutoff").Offset,
            0.0, 1.0, 0.5);
    }
}

// vips_mask_gaussian_init (from gaussian.c)
public class VipsMaskGaussian : VipsObject
{
    public double FrequencyCutoff { get; set; } = 0.5;
    public double AmplitudeCutoff { get; set; } = 0.5;

    public override void Init()
    {
        base.Init();
    }
}

// vips_mask_gaussian (from gaussian.c)
public static int VipsMaskGaussian(VipsImage[] out, int width, int height,
    double frequency_cutoff, double amplitude_cutoff, params object[] args)
{
    var result = VipsCallSplit("mask_gaussian", out, width, height,
        frequency_cutoff, amplitude_cutoff);

    return result;
}
```