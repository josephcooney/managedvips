```csharp
// Converted from: vips_mask_butterworth_ring_point()

public double VipsMaskButterworthRingPoint(VipsImage mask, double dx, double dy)
{
    VipsMaskButterworth butterworth = (VipsMaskButterworth)mask;
    VipsMaskButterworthRing butterworthRing = (VipsMaskButterworthRing)mask;

    double order = butterworth.Order;
    double fc = butterworth.FrequencyCutoff;
    double ac = butterworth.AmplitudeCutoff;
    double ringwidth = butterworthRing.RingWidth;

    double df = ringwidth / 2.0;
    double cnst = (1.0 / ac) - 1.0;
    double df2 = df * df;
    double dist = Math.Sqrt(dx * dx + dy * dy);

    return 1.0 /
        (1.0 + cnst * Math.Pow((dist - fc) * (dist - fc) / df2, order));
}

// Converted from: vips_mask_butterworth_ring_class_init()

public class VipsMaskButterworthRingClass : VipsObjectClass
{
    public override void ClassInit(VipsMaskButterworthRingClass klass)
    {
        base.ClassInit(klass);

        GObjectClass gobjectClass = (GObjectClass)klass;
        VipsObjectClass vobjectClass = (VipsObjectClass)klass;
        VipsMaskClass maskClass = (VipsMaskClass)klass;

        gobjectClass.SetProperty = VipsObject.SetProperty;
        gobjectClass.GetProperty = VipsObject.GetProperty;

        vobjectClass.Nickname = "mask_butterworth_ring";
        vobjectClass.Description = _("make a butterworth ring filter");

        maskClass.Point = VipsMaskButterworthRingPoint;

        VIPS_ARG_DOUBLE(klass, "ringwidth", 20,
            _("Ringwidth"),
            _("Ringwidth"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsMaskButterworthRing.RingWidth),
            0.0, 1000000.0, 0.1);
    }
}

// Converted from: vips_mask_butterworth_ring_init()

public class VipsMaskButterworthRing : VipsObject
{
    public double RingWidth { get; set; }

    public VipsMaskButterworthRing()
    {
        RingWidth = 0.1;
    }
}

// Converted from: vips_mask_butterworth_ring()

public int VipsMaskButterworthRing(VipsImage @out, int width, int height,
    double order, double frequencyCutoff, double amplitudeCutoff,
    double ringwidth, params object[] args)
{
    var result = VipsCallSplit("mask_butterworth_ring", args,
        out, width, height,
        order, frequencyCutoff, amplitudeCutoff, ringwidth);

    return result;
}
```