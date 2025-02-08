```csharp
// vips_CMYK2XYZ_process: (method)
// @in: input image
// @out: (out): output image
// @...: %NULL-terminated list of optional named arguments
//
// Turn CMYK to XYZ. If the image has an embedded ICC profile this will be
// used for the conversion. If there is no embedded profile, a generic
// fallback profile will be used.
//
// Conversion is to D65 XYZ with relative intent. If you need more control
// over the process, use vips_icc_import() instead.
//
// Returns: 0 on success, -1 on error
public static int CMYK2XYZ_process(VipsImage in, VipsImage[] out)
{
    return ICCImport(in, out,
        "input_profile", "cmyk",
        "embedded", true,
        "pcs", VIPS_PCS_XYZ,
        null);
}

// vips_CMYK2XYZ_build: (method)
// @object: object to build
//
// Build the CMYK2XYZ operation.
//
// Returns: 0 on success, -1 on error
public static int CMYK2XYZ_build(VipsObject object)
{
    VipsCMYK2XYZ cmyk2xyz = (VipsCMYK2XYZ)object;

    VipsImage out;
    VipsImage t;

    if (VIPS_OBJECT_CLASS(vips_CMYK2XYZ_parent_class).build(object) != 0)
        return -1;

    out = new VipsImage();
    object.SetProperty("out", out);

    if (colourspace_process_n("CMYK2XYZ",
            cmyk2xyz.in, ref t, 4, CMYK2XYZ_process))
        return -1;
    if (image_write(t, out) != 0)
    {
        object.Unref(t);
        return -1;
    }
    object.Unref(t);

    return 0;
}

// vips_CMYK2XYZ_class_init: (method)
// @class: class to initialise
//
// Class initialisation for CMYK2XYZ.
public static void CMYK2XYZ_class_init(VipsCMYK2XYZClass class)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass object_class = (VipsObjectClass)class;
    VipsOperationClass operation_class = VIPS_OPERATION_CLASS(class);

    gobject_class.set_property = vips_object_set_property;
    gobject_class.get_property = vips_object_get_property;

    object_class.nickname = "CMYK2XYZ";
    object_class.description = _("transform CMYK to XYZ");

    object_class.build = CMYK2XYZ_build;
    operation_class.flags = VIPS_OPERATION_SEQUENTIAL;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsCMYK2XYZ, in));

    VIPS_ARG_IMAGE(class, "out", 100,
        _("Output"),
        _("Output image"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsCMYK2XYZ, out));
}

// vips_CMYK2XYZ_init: (method)
// @cmyk2xyz: object to initialise
//
// Object initialisation for CMYK2XYZ.
public static void CMYK2XYZ_init(VipsCMYK2XYZ cmyk2xyz)
{
    VipsColour colour = VIPS_COLOUR(cmyk2xyz);
    VipsColourCode code = VIPS_COLOUR_CODE(cmyk2xyz);

    colour.interpretation = VIPS_INTERPRETATION_XYZ;
    colour.format = VIPS_FORMAT_FLOAT;
    colour.bands = 3;
    colour.input_bands = 4;

    code.input_coding = VIPS_CODING_NONE;
    code.input_format = VIPS_FORMAT_UCHAR;
    code.input_interpretation = VIPS_INTERPRETATION_CMYK;
}

// vips_CMYK2XYZ: (method)
// @in: input image
// @out: (out): output image
// @...: %NULL-terminated list of optional named arguments
//
// Turn CMYK to XYZ. If the image has an embedded ICC profile this will be
// used for the conversion. If there is no embedded profile, a generic
// fallback profile will be used.
//
// Conversion is to D65 XYZ with relative intent. If you need more control
// over the process, use vips_icc_import() instead.
//
// Returns: 0 on success, -1 on error
public static int CMYK2XYZ(VipsImage in, ref VipsImage out)
{
    return call_split("CMYK2XYZ", in, ref out);
}
```