```csharp
// vips_mask_ideal_point (from ideal.c)

public double VipsMaskIdealPoint(VipsMask mask, double dx, double dy)
{
    VipsMaskIdeal ideal = (VipsMaskIdeal)mask;
    double fc = ideal.FrequencyCutoff;

    double dist2 = dx * dx + dy * dy;
    double fc2 = fc * fc;

    return dist2 <= fc2 ? 0.0 : 1.0;
}

// vips_mask_ideal_class_init (from ideal.c)

public class VipsMaskIdealClass : VipsMaskClass
{
    public static void VipsMaskIdealClassInit(VipsMaskIdealClass* klass)
    {
        GObjectClass gobject_class = (GObjectClass)klass;
        VipsObjectClass vobject_class = (VipsObjectClass)VIPS_OBJECT_CLASS(klass);
        VipsMaskClass mask_class = (VipsMaskClass)VIPS_MASK_CLASS(klass);

        gobject_class.SetProperty = VipsObjectSetProperty;
        gobject_class.GetProperty = VipsObjectGetProperty;

        vobject_class.Nickname = "mask_ideal";
        vobject_class.Description = _("make an ideal filter");

        mask_class.Point = VipsMaskIdealPoint;

        VIPS_ARG_DOUBLE(klass, "frequency_cutoff", 6,
            _("Frequency cutoff"),
            _("Frequency cutoff"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsMaskIdeal).GetField("FrequencyCutoff").Offset,
            0.0, 1000000.0, 0.5);
    }
}

// vips_mask_ideal_init (from ideal.c)

public class VipsMaskIdeal : VipsMask
{
    public double FrequencyCutoff { get; set; }

    public VipsMaskIdeal()
    {
        this.FrequencyCutoff = 0.5;
    }
}

// vips_mask_ideal (from ideal.c)

public int VipsMaskIdeal(VipsImage out, int width, int height,
    double frequency_cutoff, params object[] args)
{
    return VipsCallSplit("mask_ideal", args, ref out, width, height, frequency_cutoff);
}
```