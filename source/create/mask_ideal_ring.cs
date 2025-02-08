```csharp
// vips_mask_ideal_ring_point
public double VipsMaskIdealRingPoint(VipsMask mask, double dx, double dy)
{
    // from ideal.c
    VipsMaskIdeal ideal = (VipsMaskIdeal)mask;
    VipsMaskIdealRing idealRing = (VipsMaskIdealRing)mask;
    double fc = ideal.FrequencyCutoff;
    double ringwidth = idealRing.RingWidth;

    double df = ringwidth / 2.0;
    double dist2 = dx * dx + dy * dy;
    double fc2_1 = (fc - df) * (fc - df);
    double fc2_2 = (fc + df) * (fc + df);

    return dist2 > fc2_1 && dist2 < fc2_2 ? 1.0 : 0.0;
}

// vips_mask_ideal_ring_class_init
public class VipsMaskIdealRingClass : VipsObjectClass, VipsMaskClass
{
    public VipsMaskIdealRingClass()
    {
        // from ideal.c
        this.Nickname = "mask_ideal_ring";
        this.Description = _("make an ideal ring filter");

        this.Point = new Func<VipsMask, double, double, double>(VipsMaskIdealRingPoint);
    }
}

// vips_mask_ideal_ring_init
public VipsMaskIdealRing Init(VipsMaskIdealRing idealRing)
{
    // from ideal.c
    idealRing.RingWidth = 0.5;
    return idealRing;
}

// vips_mask_ideal_ring
public int VipsMaskIdealRing(VipsImage[] out, int width, int height,
    double frequencyCutoff, double ringwidth, params object[] args)
{
    // from ideal.c
    var result = VipsCallSplit("mask_ideal_ring", out, width, height,
        frequencyCutoff, ringwidth);
    return result;
}
```