```csharp
// Converted from: vips_addalpha_build()

public class VipsAddAlpha : VipsConversion
{
    public VipsImage In { get; set; }

    protected override int Build()
    {
        // Get the input image and create a local array for output images
        var t = new VipsImage[2];
        var max_alpha = VipsInterpretation.MaxAlpha(In.Type);

        if (base.Build())
            return -1;

        // Join the alpha band to the input image
        if (!VipsBandJoinConst1(In, ref t[0], max_alpha, null))
            return -1;

        // Write the output image
        if (!VipsImageWrite(t[0]))
            return -1;

        return 0;
    }
}

// Converted from: vips_addalpha_class_init()

public class VipsAddAlphaClass : VipsConversionClass
{
    public static void ClassInit()
    {
        // Set up GObject and VipsObject properties
        base.ClassInit();

        // Set up the nickname, description, and build method for this operation
        base.Nickname = "addalpha";
        base.Description = _("append an alpha channel");
        base.Build = new BuildDelegate(VipsAddAlpha.Build);

        // Add arguments to the class
        VIPS_ARG_IMAGE(base, "in", 0,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsAddAlpha, In));
    }
}

// Converted from: vips_addalpha_init()

public class VipsAddAlpha : VipsConversion
{
    public VipsAddAlpha()
    {
        // Initialize the object
    }
}

// Converted from: vips_addalpha()

public static int VipsAddAlpha(VipsImage in_image, out VipsImage[] out_images)
{
    var result = VipsCallSplit("addalpha", in_image, out_images);
    return result;
}
```