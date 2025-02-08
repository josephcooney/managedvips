```csharp
// vips_mask_gaussian_ring_point
public double VipsMaskGaussianRingPoint(VipsImage mask, double dx, double dy)
{
    VipsMaskGaussian gaussian = (VipsMaskGaussian)mask;
    VipsMaskGaussianRing gaussianRing = (VipsMaskGaussianRing)mask;

    double fc = gaussian.FrequencyCutoff;
    double ac = gaussian.AmplitudeCutoff;
    double ringwidth = gaussianRing.RingWidth;

    double df = ringwidth / 2.0;
    double df2 = df * df;
    double cnst = Math.Log(ac);
    double dist = Math.Sqrt(dx * dx + dy * dy);

    return Math.Exp(cnst * (dist - fc) * (dist - fc) / df2);
}

// vips_mask_gaussian_ring_class_init
public class VipsMaskGaussianRingClass : VipsObjectClass, VipsMaskClass
{
    public override void ClassInit(VipsObjectClass gobject_class)
    {
        base.ClassInit(gobject_class);

        // Set property handlers
        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        // Set class properties
        vobject_class.Nickname = "mask_gaussian_ring";
        vobject_class.Description = _("make a gaussian ring filter");

        mask_class.Point = VipsMaskGaussianRingPoint;

        // Define property: RingWidth
        VIPS_ARG_DOUBLE("ringwidth", 20, _("Ringwidth"), _("Ringwidth"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMaskGaussianRing, RingWidth),
            0.0, 1000000.0, 0.5);
    }
}

// vips_mask_gaussian_ring_init
public void VipsMaskGaussianRingInit(VipsMaskGaussianRing gaussian_ring)
{
    gaussian_ring.RingWidth = 0.5;
}

// vips_mask_gaussian_ring
public int VipsMaskGaussianRing(out VipsImage out, int width, int height,
    double frequency_cutoff, double amplitude_cutoff, double ringwidth,
    params object[] args)
{
    // Call the split function with the given arguments
    return VipsCallSplit("mask_gaussian_ring", args, ref out, width, height,
        frequency_cutoff, amplitude_cutoff, ringwidth);
}
```