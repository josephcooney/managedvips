```csharp
// vips_convsep_build (method)
public int VipsConvsepBuild(VipsObject obj)
{
    // Get the class and convolution object from the parent instance
    VipsObjectClass klass = VIPS_OBJECT_GET_CLASS(obj);
    VipsConvolution conv = (VipsConvolution)obj;
    VipsConvsep convsep = (VipsConvsep)obj;

    // Create a local array for temporary images
    VipsImage[] t = new VipsImage[4];

    // Get the input image
    VipsImage in = conv.in;

    // Set up the output image
    g_object_set(convsep, "out", vips_image_new(), null);

    // Call the parent class's build method
    if (VIPS_OBJECT_CLASS(vips_convsep_parent_class).build(obj) != 0)
        return -1;

    // Check if the mask is separable
    if (vips_check_separable(klass.nickname, conv.M) != 0)
        return -1;

    // Perform the convolution operation
    if (convsep.precision == VIPS_PRECISION_APPROXIMATE)
    {
        // Approximate convolution using vips_convasep
        if (vips_convasep(in, ref t[0], conv.M,
            "layers", convsep.layers,
            null) != 0)
            return -1;
        in = t[0];
    }
    else
    {
        // Rotate the mask by 90 degrees and copy it
        if (vips_rot(conv.M, ref t[0], VIPS_ANGLE_D90, null) != 0 ||
            vips_copy(t[0], ref t[3], null) != 0)
            return -1;
        vips_image_set_double(t[3], "offset", 0);

        // Perform the convolution operation
        if (vips_conv(in, ref t[1], conv.M,
            "precision", convsep.precision,
            "layers", convsep.layers,
            "cluster", convsep.cluster,
            null) != 0 ||
            vips_conv(t[1], ref t[2], t[3],
            "precision", convsep.precision,
            "layers", convsep.layers,
            "cluster", convsep.cluster,
            null) != 0)
            return -1;
        in = t[2];
    }

    // Write the output image
    if (vips_image_write(in, conv.out) != 0)
        return -1;

    return 0;
}

// vips_convsep_class_init (method)
public void VipsConvsepClassInit(VipsConvsepClass klass)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(klass);
    VipsObjectClass object_class = (VipsObjectClass)klass;

    // Set up the class properties
    gobject_class.set_property = vips_object_set_property;
    gobject_class.get_property = vips_object_get_property;

    object_class.nickname = "convsep";
    object_class.description = _("separable convolution operation");
    object_class.build = VipsConvsepBuild;

    // Define the precision property
    VIPS_ARG_ENUM(klass, "precision", 203,
        _("Precision"),
        _("Convolve with this precision"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsConvsep, precision),
        VIPS_TYPE_PRECISION, VIPS_PRECISION_FLOAT);

    // Define the layers property
    VIPS_ARG_INT(klass, "layers", 204,
        _("Layers"),
        _("Use this many layers in approximation"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsConvsep, layers),
        1, 1000, 5);

    // Define the cluster property
    VIPS_ARG_INT(klass, "cluster", 205,
        _("Cluster"),
        _("Cluster lines closer than this in approximation"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsConvsep, cluster),
        1, 100, 1);
}

// vips_convsep_init (method)
public void VipsConvsepInit(VipsConvsep convsep)
{
    // Initialize the properties
    convsep.precision = VIPS_PRECISION_FLOAT;
    convsep.layers = 5;
    convsep.cluster = 1;
}

// vips_convsep (method)
public int VipsConvsep(VipsImage in, ref VipsImage out, VipsImage mask, params object[] args)
{
    // Call the build method
    return VipsConvsepBuild(new VipsObject(in));
}
```