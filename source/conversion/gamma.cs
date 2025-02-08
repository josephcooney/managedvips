```csharp
// vips_gamma_build: (method)
// @object: VipsObject
public int Build(VipsObject object)
{
    // Get the conversion and gamma objects
    VipsConversion conversion = (VipsConversion)object;
    VipsGamma gamma = (VipsGamma)object;

    // Get the input image
    VipsImage in = gamma.In;

    // Calculate the scale factor
    double scale = Math.Pow(VipsGamma.Maxval[in.BandFmt], 1.0 / gamma.Exponent) /
        VipsGamma.Maxval[in.BandFmt];

    // Create a local array of images
    VipsImage[] t = new VipsImage[5];
    vips_object_local_array(object, ref t);

    // Perform the conversion
    if (vips_identity(ref t[0], "ushort", in.BandFmt == VIPS_FORMAT_USHORT, null) ||
        vips_pow_const1(t[0], ref t[1], 1.0 / gamma.Exponent, null) ||
        vips_linear1(t[1], ref t[2], 1.0 / scale, 0, null) ||
        vips_cast(t[2], ref t[3], in.BandFmt, null) ||
        vips_maplut(in, ref t[4], t[3], null) ||
        vips_image_write(t[4], conversion.Out))
    {
        return -1;
    }

    // Perform the conversion for non-integer formats
    else if (vips_pow_const1(in, ref t[1], 1.0 / gamma.Exponent, null) ||
             vips_linear1(t[1], ref t[2], 1.0 / scale, 0, null) ||
             vips_cast(t[2], ref t[3], in.BandFmt, null) ||
             vips_image_write(t[3], conversion.Out))
    {
        return -1;
    }

    // Return success
    return 0;
}

// vips_gamma_class_init: (method)
public class VipsGammaClass : VipsConversionClass
{
    public static void ClassInit(VipsGammaClass klass)
    {
        GObjectClass gobject_class = G_OBJECT_CLASS(klass);
        VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(klass);
        VipsOperationClass operation_class = VIPS_OPERATION_CLASS(klass);

        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        vobject_class.Nickname = "gamma";
        vobject_class.Description = _("gamma an image");
        vobject_class.Build = Build;

        operation_class.Flags = VIPS_OPERATION_SEQUENTIAL;

        // Define the properties
        VipsArgImage("in", 1, _("Input"), _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsGamma, In));

        VipsArgDouble("exponent", 2, _("Exponent"), _("Gamma factor"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsGamma, Exponent),
            0.000001, 1000.0, 2.4);
    }
}

// vips_gamma_init: (method)
public class VipsGamma : VipsConversion
{
    public double Exponent { get; set; }

    public VipsGamma()
    {
        Exponent = 1.0 / 2.4;
    }
}

// vips_gamma: (method)
public static int Gamma(VipsImage in, ref VipsImage out, params object[] args)
{
    // Create a new gamma object
    VipsGamma gamma = new VipsGamma();
    gamma.In = in;

    // Call the build method
    return Build(new VipsObject(gamma));
}
```