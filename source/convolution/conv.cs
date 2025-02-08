```csharp
// vips_conv_build
public int VipsConvBuild(VipsObject obj)
{
    VipsConvolution convolution = (VipsConvolution)obj;
    VipsConv conv = (VipsConv)obj;
    VipsImage[] t = new VipsImage[4];

    VipsImage in;

    if (convolution.ParentClass.Build(convolution))
        return -1;

    object.SetProperty(conv, "out", new VipsImage());

    in = convolution.In;

    // printf("vips_conv_build: convolving with:\n");
    // vips_matrixprint(convolution.M, NULL);

    // Unpack for processing.
    if (in.Decode(out, t))
        return -1;
    in = t[0];

    switch (conv.Precision)
    {
        case VipsPrecision.Float:
            if (VipsConvf(in, out, convolution.M) || in.Write(convolution.Out))
                return -1;
            break;

        case VipsPrecision.Integer:
            if (VipsConvi(in, out, convolution.M) || in.Write(convolution.Out))
                return -1;
            break;

        case VipsPrecision.Approximate:
            if (VipsConva(in, out, convolution.M,
                "layers", conv.Layers,
                "cluster", conv.Cluster,
                null) || in.Write(convolution.Out))
                return -1;
            break;

        default:
            Debug.Assert(false);
            break;
    }

    VipsReorderMarginHint(convolution.Out, convolution.M.Xsize * convolution.M.Ysize);

    return 0;
}

// vips_conv_class_init
public class VipsConvClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        ObjectClass.SetProperty = VipsObjectSetProperty;
        ObjectClass.GetProperty = VipsObjectGetProperty;

        ObjectClass.Nickname = "conv";
        ObjectClass.Description = _("convolution operation");
        ObjectClass.Build = VipsConvBuild;

        VIPS_ARG_ENUM("precision", 103,
            _("Precision"),
            _("Convolve with this precision"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsConv),
            "precision",
            typeof(VipsPrecision),
            VipsPrecision.Float);

        VIPS_ARG_INT("layers", 104,
            _("Layers"),
            _("Use this many layers in approximation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsConv),
            "layers",
            1, 1000, 5);

        VIPS_ARG_INT("cluster", 105,
            _("Cluster"),
            _("Cluster lines closer than this in approximation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsConv),
            "cluster",
            1, 100, 1);
    }
}

// vips_conv_init
public class VipsConv : VipsObject
{
    public override void Init()
    {
        Precision = VipsPrecision.Float;
        Layers = 5;
        Cluster = 1;
    }
}

// vips_conv
public int VipsConv(VipsImage in, out VipsImage[] out, VipsImage mask)
{
    var result = VipsCallSplit("conv", in, out, mask);
    return result;
}
```