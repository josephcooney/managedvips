Here is the converted C# code:

```csharp
using System;

public class VipsMaskButterworth : VipsMask
{
    public double Order { get; set; }
    public double FrequencyCutoff { get; set; }
    public double AmplitudeCutoff { get; set; }

    public VipsMaskButterworth()
    {
        Order = 1.0;
        FrequencyCutoff = 0.5;
        AmplitudeCutoff = 0.5;
    }

    public override double Point(double dx, double dy)
    {
        // vips_mask_butterworth_point
        double cnst = (1.0 / AmplitudeCutoff) - 1.0;
        double fc2 = FrequencyCutoff * FrequencyCutoff;
        double d = dx * dx + dy * dy;

        if (d == 0)
            return 0;
        else
            return 1.0 / (1.0 + cnst * Math.Pow(fc2 / d, Order));
    }
}

public class VipsMaskButterworthClass : VipsMaskClass
{
    public static void Register(VipsObjectClass type)
    {
        // vips_mask_butterworth_class_init
        GObjectClass gobject_class = (GObjectClass)type;
        VipsObjectClass vobject_class = (VipsObjectClass)type;
        VipsMaskClass mask_class = (VipsMaskClass)type;

        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        vobject_class.Nickname = "mask_butterworth";
        vobject_class.Description = "make a butterworth filter";

        mask_class.Point = new Func<double, double, double>(VipsMaskButterworth.Point);

        // VIPS_ARG_DOUBLE
        VipsArgDouble arg_order = new VipsArgDouble("order", 6,
            "Order",
            "Filter order",
            VipsArgument.RequiredInput,
            typeof(VipsMaskButterworth).GetProperty("Order"),
            1.0, 1000000.0, 1.0);

        VipsArgDouble arg_frequency_cutoff = new VipsArgDouble("frequency_cutoff", 7,
            "Frequency cutoff",
            "Frequency cutoff",
            VipsArgument.RequiredInput,
            typeof(VipsMaskButterworth).GetProperty("FrequencyCutoff"),
            0.0, 1000000.0, 0.5);

        VipsArgDouble arg_amplitude_cutoff = new VipsArgDouble("amplitude_cutoff", 8,
            "Amplitude cutoff",
            "Amplitude cutoff",
            VipsArgument.RequiredInput,
            typeof(VipsMaskButterworth).GetProperty("AmplitudeCutoff"),
            0.0, 1.0, 0.5);
    }
}

public class Program
{
    public static int Main(string[] args)
    {
        // vips_mask_butterworth
        VipsImage out_image = new VipsImage();
        int result = VipsMaskButterworth.MaskButterworth(out_image, width, height,
            order, frequency_cutoff, amplitude_cutoff);

        return result;
    }

    public static void MaskButterworth(VipsImage out, int width, int height,
        double order, double frequency_cutoff, double amplitude_cutoff)
    {
        // vips_mask_butterworth
        va_list ap;
        int result;

        va_start(ap, amplitude_cutoff);
        result = VipsCall.Split("mask_butterworth", ap,
            out, width, height,
            order, frequency_cutoff, amplitude_cutoff);
        va_end(ap);

        return result;
    }
}
```

Note that I've assumed the existence of a `VipsImage` class and a `VipsMask` base class, as well as various other VIPS-related classes and methods. You may need to modify the code to fit your specific use case.

Also note that this is just one possible way to convert the C code to C#. There are many different approaches you could take, depending on your specific needs and preferences.