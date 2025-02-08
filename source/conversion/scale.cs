```csharp
// vips_scale_build
public class ScaleConversion : Conversion
{
    public ScaleConversion(VipsImage image) 
        : base(image)
    {
    }

    protected override int Build()
    {
        VipsImage[] t = new VipsImage[7];
        double mx, mn;

        if (base.Build())
            return -1;

        if (VipsStats(Image, out t[0], null))
            return -1;
        mn = t[0].GetMatrix(0, 0);
        mx = t[0].GetMatrix(1, 0);

        if (mn == mx)
        {
            // Range of zero: just return black.
            VipsImage imageBlack = new VipsImage(Image.Xsize, Image.Ysize, "bands", Image.Bands, null);
            if (!VipsBlack(imageBlack, out t[1], null) || !VipsImageWrite(t[1], Output))
                return -1;
        }
        else if (Log)
        {
            double f = 255.0 / Math.Log10(1.0 + Math.Pow(mx, Exp));

            VipsImage imagePowConst1 = new VipsImage(Image);
            if (!VipsPowConst1(imagePowConst1, out t[2], Exp, null) ||
                !VipsLinear1(t[2], out t[3], 1.0, 1.0, null) ||
                !VipsLog10(t[3], out t[4], null) ||
                // Add 0.5 to get round to nearest.
                !VipsLinear1(t[4], out t[5], f, 0.5, "uchar", true, null) ||
                !VipsImageWrite(t[5], Output))
                return -1;
        }
        else
        {
            double f = 255.0 / (mx - mn);

            // Add .5 to get round-to-nearest.
            double a = -(mn * f) + 0.5;

            VipsImage imageLinear1 = new VipsImage(Image);
            if (!VipsLinear1(imageLinear1, out t[2], f, a, "uchar", true, null) ||
                !VipsImageWrite(t[2], Output))
                return -1;
        }

        return 0;
    }
}

// vips_scale_class_init
public class ScaleConversionClass : ConversionClass
{
    public static void Init(VipsScaleClass* klass)
    {
        GObjectClass gobject_class = (GObjectClass)klass;
        VipsObjectClass vobject_class = (VipsObjectClass)klass;

        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        vobject_class.Nickname = "scale";
        vobject_class.Description = _("scale an image to uchar");
        vobject_class.Build = ScaleConversion.Build;

        VIPS_ARG_IMAGE(klass, "in", 1,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsScale, in));

        VIPS_ARG_BOOL(klass, "log", 3,
            _("Log"),
            _("Log scale"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsScale, log),
            false);

        VIPS_ARG_DOUBLE(klass, "exp", 3,
            _("Exponent"),
            _("Exponent for log scale"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsScale, exp),
            0.00001, 10000, 0.25);
    }
}

// vips_scale_init
public class ScaleConversion : Conversion
{
    public ScaleConversion(VipsImage image) 
        : base(image)
    {
        Exp = 0.25;
    }

    private double _exp;
    public double Exp { get; set; }
}

// vips_scale
public static int VipsScale(VipsImage in, out VipsImage[] out, params object[] args)
{
    ScaleConversion conversion = new ScaleConversion(in);
    return conversion.Build();
}
```