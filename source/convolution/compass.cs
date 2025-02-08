```csharp
// Converted from: vips_compass_build()

public class VipsCompass : VipsConvolution
{
    public int Times { get; set; }
    public VipsAngle45 Angle { get; set; }
    public VipsCombine Combine { get; set; }
    public VipsPrecision Precision { get; set; }
    public int Layers { get; set; }
    public int Cluster { get; set; }

    protected override int Build(VipsObject obj)
    {
        var convolution = (VipsConvolution)obj;
        var compass = (VipsCompass)obj;
        VipsImage[] masks;
        VipsImage mask;
        VipsImage[] images;
        VipsImage[] abs;
        VipsImage[] combine;

        // Create local arrays
        masks = new VipsImage[compass.Times];
        images = new VipsImage[compass.Times];
        abs = new VipsImage[compass.Times];
        combine = new VipsImage[compass.Times];

        // Set output image
        obj.SetProperty("out", VipsImage.New());

        if (base.Build(obj) != 0)
            return -1;

        mask = convolution.M;
        for (int i = 0; i < compass.Times; i++)
        {
            if (VipsConv(convolution.In, ref images[i], mask,
                "precision", compass.Precision,
                "layers", compass.Layers,
                "cluster", compass.Cluster,
                null) != 0)
                return -1;

            if (VipsRot45(mask, ref masks[i],
                "angle", compass.Angle,
                null) != 0)
                return -1;

            mask = masks[i];
        }

        for (int i = 0; i < compass.Times; i++)
            if (VipsAbs(images[i], ref abs[i], null) != 0)
                return -1;

        switch (compass.Combine)
        {
            case VIPS_COMBINE_MAX:
                if (VipsBandRank(abs, ref combine[0], compass.Times,
                    "index", compass.Times - 1,
                    null) != 0)
                    return -1;
                var x = combine[0];
                break;

            case VIPS_COMBINE_MIN:
                if (VipsBandRank(abs, ref combine[0], compass.Times,
                    "index", 0,
                    null) != 0)
                    return -1;
                x = combine[0];
                break;

            case VIPS_COMBINE_SUM:
                if (VipsSum(abs, ref combine[0], compass.Times, null) != 0)
                    return -1;
                x = combine[0];
                break;

            default:
                g_assert_not_reached();

                // Stop compiler warnings.
                x = null;
        }

        if (VipsImage.Write(x, convolution.Out) != 0)
            return -1;

        return 0;
    }
}

// Converted from: vips_compass_class_init()

public class VipsCompassClass : VipsConvolutionClass
{
    public static void ClassInit(VipsCompassClass klass)
    {
        var gobject_class = (GObjectClass)klass;
        var object_class = (VipsObjectClass)klass;

        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        object_class.Nickname = "compass";
        object_class.Description = _("convolve with rotating mask");
        object_class.Build = new Func<VipsObject, int>(VipsCompass.Build);

        VIPS_ARG_INT(klass, "times", 101,
            _("Times"),
            _("Rotate and convolve this many times"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsCompass).GetProperty("Times").GetGetMethod(),
            1, 1000, 2);

        VIPS_ARG_ENUM(klass, "angle", 103,
            _("Angle"),
            _("Rotate mask by this much between convolutions"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsCompass).GetProperty("Angle").GetGetMethod(),
            typeof(VipsAngle45), VIPS_ANGLE45_D90);

        VIPS_ARG_ENUM(klass, "combine", 104,
            _("Combine"),
            _("Combine convolution results like this"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsCompass).GetProperty("Combine").GetGetMethod(),
            typeof(VipsCombine), VIPS_COMBINE_MAX);

        VIPS_ARG_ENUM(klass, "precision", 203,
            _("Precision"),
            _("Convolve with this precision"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsCompass).GetProperty("Precision").GetGetMethod(),
            typeof(VipsPrecision), VIPS_PRECISION_FLOAT);

        VIPS_ARG_INT(klass, "layers", 204,
            _("Layers"),
            _("Use this many layers in approximation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsCompass).GetProperty("Layers").GetGetMethod(),
            1, 1000, 5);

        VIPS_ARG_INT(klass, "cluster", 205,
            _("Cluster"),
            _("Cluster lines closer than this in approximation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsCompass).GetProperty("Cluster").GetGetMethod(),
            1, 100, 1);
    }
}

// Converted from: vips_compass_init()

public class VipsCompass : VipsConvolution
{
    public VipsCompass()
    {
        Times = 2;
        Angle = VIPS_ANGLE45_D90;
        Combine = VIPS_COMBINE_MAX;
        Precision = VIPS_PRECISION_FLOAT;
        Layers = 5;
        Cluster = 1;
    }
}

// Converted from: vips_compass()

public class VipsCompass : VipsConvolution
{
    public static int Compass(VipsImage in, ref VipsImage out, VipsImage mask, params object[] args)
    {
        var result = VipsCallSplit("compass", args, in, ref out, mask);
        return result;
    }
}
```