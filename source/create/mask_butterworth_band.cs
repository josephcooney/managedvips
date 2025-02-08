```csharp
// vips_mask_butterworth_band_point (from vips_mask_butterworth_band.c)
public double VipsMaskButterworthBandPoint(double dx, double dy)
{
    double order = Order;
    double fcx = FrequencyCutoffX;
    double fcy = FrequencyCutoffY;
    double r2 = Radius * Radius;
    double ac = AmplitudeCutoff;

    double cnst = (1.0 / ac) - 1.0;

    // Normalise the amplitude at (fcx, fcy) to 1.0.
    double cnsta = 1.0 / (1.0 + 1.0 / (1.0 +
        cnst * Math.Pow(4.0 * (fcx * fcx + fcy * fcy) / r2, order)));

    double d1 = (dx - fcx) * (dx - fcx) + (dy - fcy) * (dy - fcy);
    double d2 = (dx + fcx) * (dx + fcx) + (dy + fcy) * (dy + fcy);

    return cnsta * (1.0 / (1.0 + cnst * Math.Pow(d1 / r2, order)) +
                    1.0 / (1.0 + cnst * Math.Pow(d2 / r2, order)));
}

// vips_mask_butterworth_band_class_init (from vips_mask_butterworth_band.c)
public class VipsMaskButterworthBandClass : VipsMaskClass
{
    public static Property OrderProperty { get; set; }
    public static Property FrequencyCutoffXProperty { get; set; }
    public static Property FrequencyCutoffYProperty { get; set; }
    public static Property RadiusProperty { get; set; }
    public static Property AmplitudeCutoffProperty { get; set; }

    protected override void InitializeProperties()
    {
        base.InitializeProperties();

        OrderProperty = new DoubleProperty("order", "Order",
            _("Filter order"), 1.0, 1000000.0, 1.0);
        FrequencyCutoffXProperty = new DoubleProperty("frequency_cutoff_x", "Frequency cutoff x",
            _("Frequency cutoff x"), 0.0, 1000000.0, 0.5);
        FrequencyCutoffYProperty = new DoubleProperty("frequency_cutoff_y", "Frequency cutoff y",
            _("Frequency cutoff y"), 0.0, 1000000.0, 0.5);
        RadiusProperty = new DoubleProperty("radius", "Radius",
            _("Radius of circle"), 0.0, 1000000.0, 0.1);
        AmplitudeCutoffProperty = new DoubleProperty("amplitude_cutoff", "Amplitude cutoff",
            _("Amplitude cutoff"), 0.0, 1.0, 0.5);

        Order = 1.0;
        FrequencyCutoffX = 0.5;
        FrequencyCutoffY = 0.5;
        Radius = 0.1;
        AmplitudeCutoff = 0.5;
    }
}

// vips_mask_butterworth_band (from vips_mask_butterworth_band.c)
public int VipsMaskButterworthBand(VipsImage[] out, int width, int height,
    double order, double frequency_cutoff_x, double frequency_cutoff_y,
    double radius, double amplitude_cutoff, params object[] args)
{
    var result = VipsObject.Call("mask_butterworth_band", args);
    return (int)result;
}
```