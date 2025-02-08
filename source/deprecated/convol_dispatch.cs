Here is the C# code equivalent to the provided C code:

```csharp
// addgnoise_vec
public int AddGNoiseVec(int[] argv)
{
    double sigma = (double)argv[2];
    return ImAddGNoise(argv[0], argv[1], sigma);
}

// addgnoise_desc
public static readonly im_function AddGNoiseDesc = new im_function(
    "im_addgnoise", // Name
    "add gaussian noise with mean 0 and std. dev. sigma",
    IM_FN_PIO, // Flags
    (int[] argv) => AddGNoiseVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        new im_arg_desc("sigma", "std. dev.")
    } // Arg list
);

// contrast_surface_vec
public int ContrastSurfaceVec(int[] argv)
{
    int half_win_size = (int)argv[2];
    int spacing = (int)argv[3];
    return ImContrastSurface(argv[0], argv[1], half_win_size, spacing);
}

// contrast_surface_desc
public static readonly im_function ContrastSurfaceDesc = new im_function(
    "im_contrast_surface", // Name
    "find high-contrast points in an image",
    IM_FN_PIO, // Flags
    (int[] argv) => ContrastSurfaceVec(argv), // Dispatch function
    4, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        new im_arg_desc("half_win_size", "half window size"),
        new im_arg_desc("spacing", "spacing")
    } // Arg list
);

// sharpen_vec
public int SharpenVec(int[] argv)
{
    int mask_size = (int)argv[2];
    double x1 = (double)argv[3];
    double x2 = (double)argv[4];
    double x3 = (double)argv[5];
    double m1 = (double)argv[6];
    double m2 = (double)argv[7];
    return ImSharpen(argv[0], argv[1], mask_size, x1, x2, x3, m1, m2);
}

// sharpen_desc
public static readonly im_function SharpenDesc = new im_function(
    "im_sharpen", // Name
    "sharpen high frequencies of L channel of LabQ",
    IM_FN_PIO, // Flags
    (int[] argv) => SharpenVec(argv), // Dispatch function
    8, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        new im_arg_desc("mask_size", "mask size"),
        new im_arg_desc("x1", "x1"),
        new im_arg_desc("x2", "x2"),
        new im_arg_desc("x3", "x3"),
        new im_arg_desc("m1", "m1"),
        new im_arg_desc("m2", "m2")
    } // Arg list
);

// conv_imask
public static readonly im_arg_desc ConvImask = new im_arg_desc(
    "matrix",
    "convolution matrix"
);

// conv_dmask
public static readonly im_arg_desc ConvDmask = new im_arg_desc(
    "matrix",
    "double convolution matrix"
);

// compass_vec
public int CompassVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImCompass(argv[0], argv[1], mo.mask);
}

// compass_desc
public static readonly im_function CompassDesc = new im_function(
    "im_compass", // Name
    "convolve with 8-way rotating integer mask",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => CompassVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvImask // Arg list
    }
);

// conv_vec
public int ConvVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImConv(argv[0], argv[1], mo.mask);
}

// conv_desc
public static readonly im_function ConvDesc = new im_function(
    "im_conv", // Name
    "convolve",
    IM_FN_TRANSFORM | IM_FN_PIO, // Flags
    (int[] argv) => ConvVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvImask // Arg list
    }
);

// conv_f_vec
public int ConvFVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImConvF(argv[0], argv[1], mo.mask);
}

// conv_f_desc
public static readonly im_function ConvFDesc = new im_function(
    "im_conv_f", // Name
    "convolve, with DOUBLEMASK",
    IM_FN_TRANSFORM | IM_FN_PIO, // Flags
    (int[] argv) => ConvFVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvDmask // Arg list
    }
);

// convsep_vec
public int ConvSepVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImConvSep(argv[0], argv[1], mo.mask);
}

// convsep_desc
public static readonly im_function ConvSepDesc = new im_function(
    "im_convsep", // Name
    "seperable convolution",
    IM_FN_TRANSFORM | IM_FN_PIO, // Flags
    (int[] argv) => ConvSepVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvImask // Arg list
    }
);

// convsep_f_vec
public int ConvSepFVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImConvSepF(argv[0], argv[1], mo.mask);
}

// convsep_f_desc
public static readonly im_function ConvSepFDesc = new im_function(
    "im_convsep_f", // Name
    "seperable convolution, with DOUBLEMASK",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => ConvSepFVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvDmask // Arg list
    }
);

// fastcor_vec
public int FastCorVec(int[] argv)
{
    return ImFastCor(argv[0], argv[1], argv[2]);
}

// fastcor_desc
public static readonly im_function FastCorDesc = new im_function(
    "im_fastcor", // Name
    "fast correlate in2 within in1",
    IM_FN_TRANSFORM | IM_FN_PIO, // Flags
    (int[] argv) => FastCorVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        new im_arg_desc("in2", "second input image")
    }
);

// grad_x_vec
public int GradXVec(int[] argv)
{
    return ImGradX(argv[0], argv[1]);
}

// grad_x_desc
public static readonly im_function GradXDesc = new im_function(
    "im_grad_x", // Name
    "horizontal difference image",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => GradXVec(argv), // Dispatch function
    2, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image")
    }
);

// grad_y_vec
public int GradYVec(int[] argv)
{
    return ImGradY(argv[0], argv[1]);
}

// grad_y_desc
public static readonly im_function GradYDesc = new im_function(
    "im_grad_y", // Name
    "vertical difference image",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => GradYVec(argv), // Dispatch function
    2, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image")
    }
);

// gradcor_vec
public int GradCorVec(int[] argv)
{
    return ImGradCor(argv[0], argv[1], argv[2]);
}

// gradcor_desc
public static readonly im_function GradCorDesc = new im_function(
    "im_gradcor", // Name
    "non-normalised correlation of gradient of in2 within in1",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => GradCorVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        new im_arg_desc("in2", "second input image")
    }
);

// gradient_vec
public int GradientVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImGradient(argv[0], argv[1], mo.mask);
}

// gradient_desc
public static readonly im_function GradientDesc = new im_function(
    "im_gradient", // Name
    "convolve with 2-way rotating mask",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => GradientVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvImask // Arg list
    }
);

// lindetect_vec
public int LindetectVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    return ImLindetect(argv[0], argv[1], mo.mask);
}

// lindetect_desc
public static readonly im_function LindetectDesc = new im_function(
    "im_lindetect", // Name
    "convolve with 4-way rotating mask",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => LindetectVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        ConvImask // Arg list
    }
);

// spcor_vec
public int SpCorVec(int[] argv)
{
    return ImSpCor(argv[0], argv[1], argv[2]);
}

// spcor_desc
public static readonly im_function SpCorDesc = new im_function(
    "im_spcor", // Name
    "normalised correlation of in2 within in1",
    IM_FN_PIO | IM_FN_TRANSFORM, // Flags
    (int[] argv) => SpCorVec(argv), // Dispatch function
    3, // Size of arg list
    new im_arg_desc[]
    {
        new im_arg_desc("in", "input image"),
        new im_arg_desc("out", "output image"),
        new im_arg_desc("in2", "second input image")
    }
);

// aconv_args
public static readonly im_arg_desc[] AConvArgs = new im_arg_desc[]
{
    new im_arg_desc("matrix", "double convolution matrix"),
    new im_arg_desc("n_layers", "number of layers"),
    new im_arg_desc("cluster", "cluster size")
};

// aconv_vec
public int AConvVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    int n_layers = (int)argv[3];
    int cluster = (int)argv[4];
    return ImAConv(argv[0], argv[1], mo.mask, n_layers, cluster);
}

// aconv_desc
public static readonly im_function AConvDesc = new im_function(
    "im_aconv", // Name
    "approximate convolution",
    IM_FN_TRANSFORM | IM_FN_PIO, // Flags
    (int[] argv) => AConvVec(argv), // Dispatch function
    5, // Size of arg list
    AConvArgs // Arg list
);

// aconvsep_args
public static readonly im_arg_desc[] AConvSepArgs = new im_arg_desc[]
{
    new im_arg_desc("matrix", "double convolution matrix"),
    new im_arg_desc("n_layers", "number of layers")
};

// aconvsep_vec
public int AConvSepVec(int[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    int n_layers = (int)argv[3];
    return ImAConvSep(argv[0], argv[1], mo.mask, n_layers);
}

// aconvsep_desc
public static readonly im_function AConvSepDesc = new im_function(
    "im_aconvsep", // Name
    "approximate separable convolution",
    IM_FN_TRANSFORM | IM_FN_PIO, // Flags
    (int[] argv) => AConvSepVec(argv), // Dispatch function
    4, // Size of arg list
    AConvSepArgs // Arg list
);

// convol_list
public static readonly im_function[] ConvolList = new im_function[]
{
    AConvSepDesc,
    AConvDesc,
    AddGNoiseDesc,
    CompassDesc,
    ContrastSurfaceDesc,
    ConvDesc,
    ConvFDesc,
    ConvSepDesc,
    ConvSepFDesc,
    FastCorDesc,
    GradCorDesc,
    GradientDesc,
    GradXDesc,
    GradYDesc,
    LindetectDesc,
    SharpenDesc,
    SpCorDesc
};

// im__convolution
public static readonly im_package ImConvolution = new im_package(
    "convolution",
    ConvolList.Length,
    ConvolList
);
```