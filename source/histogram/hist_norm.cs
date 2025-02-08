```csharp
// vips_hist_norm_build method converted from C source code

public class VipsHistNorm : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }

    protected override int Build()
    {
        // Need max for each channel.
        if (VipsStats(In, out t[0], null))
            return -1;

        // Scale each channel by px / channel max
        ulong newMax = VIPS_IMAGE_N_PELS(In) - 1;
        int bands = In.Bands;
        double[] a = new double[bands];
        double[] b = new double[bands];

        for (int y = 0; y < bands; y++)
        {
            a[y] = newMax / *VIPS_MATRIX(t[0], 1, y + 1);
            b[y] = 0;
        }

        // Make output format as small as we can.
        VipsBandFormat fmt;

        if (newMax <= 255)
            fmt = VipsFormat.UChar;
        else if (newMax <= 65535)
            fmt = VipsFormat.UInt16;
        else
            fmt = VipsFormat.UInt32;

        // Linear transformation of the image
        if (VipsLinear(In, out t[1], a, b, bands, null))
            return -1;

        // Cast to output format and write to file
        if (VipsCast(t[1], out t[2], fmt, null) || VipsImageWrite(t[2], Out))
            return -1;

        return 0;
    }
}

// vips_hist_norm_class_init method converted from C source code

public class VipsHistNormClass : VipsOperationClass
{
    public static void ClassInit(VipsHistNormClass *class)
    {
        GObjectClass gobject_class = (GObjectClass)class;
        VipsObjectClass object_class = (VipsObjectClass)class;

        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        object_class.Nickname = "hist_norm";
        object_class.Description = _("normalise histogram");
        object_class.Build = new Func<VipsObject, int>(VipsHistNorm.Build);

        VipsArgImage(class, "in", 1,
            _("Input"),
            _("Input image"),
            VipsArgument.RequiredInput,
            G_STRUCT_OFFSET(VipsHistNorm, In));

        VipsArgImage(class, "out", 2,
            _("Output"),
            _("Output image"),
            VipsArgument.RequiredOutput,
            G_STRUCT_OFFSET(VipsHistNorm, Out));
    }
}

// vips_hist_norm_init method converted from C source code

public class VipsHistNorm : VipsOperation
{
    public VipsHistNorm()
    {
        // Initialize the object
    }
}

// vips_hist_norm method converted from C source code

public static int VipsHistNorm(VipsImage in, out VipsImage[] out)
{
    return VipsCallSplit("hist_norm", in, out);
}
```