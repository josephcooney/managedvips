Here is the converted C# code:

```csharp
// matinv_vec
public static int MatInvVec(object[] argv)
{
    VipsMaskObject in_ = (VipsMaskObject)argv[0];
    VipsMaskObject out = (VipsMaskObject)argv[1];

    if (!out.Mask = Vips.VipsMatInv(in_.Mask, out.Name))
        return -1;

    return 0;
}

// matinv_desc
public static im_function MatInvDesc = new im_function(
    "im_matinv", // Name
    "invert matrix",
    0,           // Flags
    MatInvVec,   // Dispatch function
    2,           // Size of arg list
    new object[] { Vips.VipsArgString("in"), Vips.VipsArgMask("out") }
);

// mattrn_vec
public static int MAttrnVec(object[] argv)
{
    VipsMaskObject in_ = (VipsMaskObject)argv[0];
    VipsMaskObject out = (VipsMaskObject)argv[1];

    if (!out.Mask = Vips.VipsMAttrn(in_.Mask, out.Name))
        return -1;

    return 0;
}

// mattrn_desc
public static im_function MAttrnDesc = new im_function(
    "im_mattrn", // Name
    "transpose matrix",
    0,           // Flags
    MAttrnVec,   // Dispatch function
    2,           // Size of arg list
    new object[] { Vips.VipsArgString("in"), Vips.VipsArgMask("out") }
);

// matcat_vec
public static int MatCatVec(object[] argv)
{
    VipsMaskObject in1 = (VipsMaskObject)argv[0];
    VipsMaskObject in2 = (VipsMaskObject)argv[1];
    VipsMaskObject out = (VipsMaskObject)argv[2];

    if (!out.Mask = Vips.VipsMatCat(in1.Mask, in2.Mask, out.Name))
        return -1;

    return 0;
}

// matcat_desc
public static im_function MatCatDesc = new im_function(
    "im_matcat", // Name
    "append matrix in2 to the end of matrix in1",
    0,           // Flags
    MatCatVec,   // Dispatch function
    3,           // Size of arg list
    new object[] { Vips.VipsArgString("in1"), Vips.VipsArgString("in2"), Vips.VipsArgMask("out") }
);

// matmul_vec
public static int MatMulVec(object[] argv)
{
    VipsMaskObject in1 = (VipsMaskObject)argv[0];
    VipsMaskObject in2 = (VipsMaskObject)argv[1];
    VipsMaskObject out = (VipsMaskObject)argv[2];

    if (!out.Mask = Vips.VipsMatMul(in1.Mask, in2.Mask, out.Name))
        return -1;

    return 0;
}

// matmul_desc
public static im_function MatMulDesc = new im_function(
    "im_matmul", // Name
    "multiply matrix in1 by matrix in2",
    0,           // Flags
    MatMulVec,   // Dispatch function
    3,           // Size of arg list
    new object[] { Vips.VipsArgString("in1"), Vips.VipsArgString("in2"), Vips.VipsArgMask("out") }
);

// read_dmask_vec
public static int ReadDmaskVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[1];

    if (!mo.Mask = Vips.VipsReadDmask(argv[0]))
        return -1;

    return 0;
}

// read_dmask_desc
public static im_function ReadDmaskDesc = new im_function(
    "im_read_dmask", // Name
    "read matrix of double from file",
    0,             // Flags
    ReadDmaskVec,  // Dispatch function
    2,             // Size of arg list
    new object[] { Vips.VipsArgString("filename"), Vips.VipsArgMask("mask") }
);

// gauss_dmask_vec
public static int GaussDMaskVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[0];
    double sigma = (double)argv[1];
    double min_amp = (double)argv[2];

    if (!mo.Mask = Vips.VipsGaussDmask(mo.Name, sigma, min_amp))
        return -1;

    return 0;
}

// gauss_dmask_desc
public static im_function GaussDMaskDesc = new im_function(
    "im_gauss_dmask", // Name
    "generate gaussian DOUBLEMASK",
    0,              // Flags
    GaussDMaskVec,  // Dispatch function
    3,              // Size of arg list
    new object[] { Vips.VipsArgMask("mask"), Vips.VipsArgDouble("sigma"), Vips.VipsArgDouble("min_amp") }
);

// gauss_dmask_sep_vec
public static int GaussDMaskSepVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[0];
    double sigma = (double)argv[1];
    double min_amp = (double)argv[2];

    if (!mo.Mask = Vips.VipsGaussDmaskSep(mo.Name, sigma, min_amp))
        return -1;

    return 0;
}

// gauss_dmask_sep_desc
public static im_function GaussDMaskSepDesc = new im_function(
    "im_gauss_dmask_sep", // Name
    "generate separable gaussian DOUBLEMASK",
    0,                  // Flags
    GaussDMaskSepVec,   // Dispatch function
    3,                  // Size of arg list
    new object[] { Vips.VipsArgMask("mask"), Vips.VipsArgDouble("sigma"), Vips.VipsArgDouble("min_amp") }
);

// gauss_imask_vec
public static int GaussIMaskVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[0];
    double sigma = (double)argv[1];
    double min_amp = (double)argv[2];

    if (!mo.Mask = Vips.VipsGaussIMask(mo.Name, sigma, min_amp))
        return -1;

    return 0;
}

// gauss_imask_desc
public static im_function GaussIMaskDesc = new im_function(
    "im_gauss_imask", // Name
    "generate gaussian INTMASK",
    0,              // Flags
    GaussIMaskVec,  // Dispatch function
    3,              // Size of arg list
    new object[] { Vips.VipsArgMask("mask"), Vips.VipsArgDouble("sigma"), Vips.VipsArgDouble("min_amp") }
);

// gauss_imask_sep_vec
public static int GaussIMaskSepVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[0];
    double sigma = (double)argv[1];
    double min_amp = (double)argv[2];

    if (!mo.Mask = Vips.VipsGaussIMaskSep(mo.Name, sigma, min_amp))
        return -1;

    return 0;
}

// gauss_imask_sep_desc
public static im_function GaussIMaskSepDesc = new im_function(
    "im_gauss_imask_sep", // Name
    "generate separable gaussian INTMASK",
    0,                  // Flags
    GaussIMaskSepVec,   // Dispatch function
    3,                  // Size of arg list
    new object[] { Vips.VipsArgMask("mask"), Vips.VipsArgDouble("sigma"), Vips.VipsArgDouble("min_amp") }
);

// log_imask_vec
public static int LogIMaskVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[0];
    double sigma = (double)argv[1];
    double min_amp = (double)argv[2];

    if (!mo.Mask = Vips.VipsLogIMask(mo.Name, sigma, min_amp))
        return -1;

    return 0;
}

// log_imask_desc
public static im_function LogIMaskDesc = new im_function(
    "im_log_imask", // Name
    "generate laplacian of gaussian INTMASK",
    0,             // Flags
    LogIMaskVec,   // Dispatch function
    3,             // Size of arg list
    new object[] { Vips.VipsArgMask("mask"), Vips.VipsArgDouble("sigma"), Vips.VipsArgDouble("min_amp") }
);

// log_dmask_vec
public static int LogDmaskVec(object[] argv)
{
    VipsMaskObject mo = (VipsMaskObject)argv[0];
    double sigma = (double)argv[1];
    double min_amp = (double)argv[2];

    if (!mo.Mask = Vips.VipsLogDmask(mo.Name, sigma, min_amp))
        return -1;

    return 0;
}

// log_dmask_desc
public static im_function LogDmaskDesc = new im_function(
    "im_log_dmask", // Name
    "generate laplacian of gaussian DOUBLEMASK",
    0,             // Flags
    LogDmaskVec,   // Dispatch function
    3,             // Size of arg list
    new object[] { Vips.VipsArgMask("mask"), Vips.VipsArgDouble("sigma"), Vips.VipsArgDouble("min_amp") }
);

// imask_args
public static object[] ImaskArgs = new object[] {
    Vips.VipsArgIMask("in"),
    Vips.VipsArgIMask("out")
};

// rotate_imask45_vec
public static int RotateImask45Vec(object[] argv)
{
    VipsMaskObject min = (VipsMaskObject)argv[0];
    VipsMaskObject mout = (VipsMaskObject)argv[1];

    if (!mout.Mask = Vips.VipsRotateImask45(min.Mask, mout.Name))
        return -1;

    return 0;
}

// rotate_imask45_desc
public static im_function RotateImask45Desc = new im_function(
    "im_rotate_imask45", // Name
    "rotate INTMASK clockwise by 45 degrees",
    0,                 // Flags
    RotateImask45Vec,   // Dispatch function
    2,                 // Size of arg list
    ImaskArgs
);

// rotate_imask90_vec
public static int RotateImask90Vec(object[] argv)
{
    VipsMaskObject min = (VipsMaskObject)argv[0];
    VipsMaskObject mout = (VipsMaskObject)argv[1];

    if (!mout.Mask = Vips.VipsRotateImask90(min.Mask, mout.Name))
        return -1;

    return 0;
}

// rotate_imask90_desc
public static im_function RotateImask90Desc = new im_function(
    "im_rotate_imask90", // Name
    "rotate INTMASK clockwise by 90 degrees",
    0,                 // Flags
    RotateImask90Vec,   // Dispatch function
    2,                 // Size of arg list
    ImaskArgs
);

// rotate_dmask45_vec
public static int RotateDmask45Vec(object[] argv)
{
    VipsMaskObject min = (VipsMaskObject)argv[0];
    VipsMaskObject mout = (VipsMaskObject)argv[1];

    if (!mout.Mask = Vips.VipsRotateDmask45(min.Mask, mout.Name))
        return -1;

    return 0;
}

// rotate_dmask45_desc
public static im_function RotateDmask45Desc = new im_function(
    "im_rotate_dmask45", // Name
    "rotate DOUBLEMASK clockwise by 45 degrees",
    0,                 // Flags
    RotateDmask45Vec,   // Dispatch function
    2,                 // Size of arg list
    new object[] { Vips.VipsArgMask("in"), Vips.VipsArgMask("out") }
);

// rotate_dmask90_vec
public static int RotateDmask90Vec(object[] argv)
{
    VipsMaskObject min = (VipsMaskObject)argv[0];
    VipsMaskObject mout = (VipsMaskObject)argv[1];

    if (!mout.Mask = Vips.VipsRotateDmask90(min.Mask, mout.Name))
        return -1;

    return 0;
}

// rotate_dmask90_desc
public static im_function RotateDmask90Desc = new im_function(
    "im_rotate_dmask90", // Name
    "rotate DOUBLEMASK clockwise by 90 degrees",
    0,                 // Flags
    RotateDmask90Vec,   // Dispatch function
    2,                 // Size of arg list
    new object[] { Vips.VipsArgMask("in"), Vips.VipsArgMask("out") }
);

// imask_xsize_vec
public static int ImaskXSizeVec(object[] argv)
{
    ((INTMASK)((VipsMaskObject)argv[0]).Mask).xsize = (int)argv[1];
    return 0;
}

// imask_ysize_vec
public static int ImaskYSizeVec(object[] argv)
{
    ((INTMASK)((VipsMaskObject)argv[0]).Mask).ysize = (int)argv[1];
    return 0;
}

// dmask_xsize_vec
public static int DmaskXSizeVec(object[] argv)
{
    ((DOUBLEMASK)((VipsMaskObject)argv[0]).Mask).xsize = (int)argv[1];
    return 0;
}

// dmask_ysize_vec
public static int DmaskYSizeVec(object[] argv)
{
    ((DOUBLEMASK)((VipsMaskObject)argv[0]).Mask).ysize = (int)argv[1];
    return 0;
}

// imask_size_args
public static object[] ImaskSizeArgs = new object[] {
    Vips.VipsArgIMask("mask"),
    Vips.VipsArgInt("size")
};

// dmask_size_args
public static object[] DmaskSizeArgs = new object[] {
    Vips.VipsArgDmask("mask"),
    Vips.VipsArgInt("size")
};

// imask_xsize_desc
public static im_function ImaskXSizeDesc = new im_function(
    "im_imask_xsize", // Name
    "horizontal size of an intmask",
    0,             // Flags
    ImaskXSizeVec,   // Dispatch function
    2,             // Size of arg list
    ImaskSizeArgs
);

// imask_ysize_desc
public static im_function ImaskYSizeDesc = new im_function(
    "im_imask_ysize", // Name
    "vertical size of an intmask",
    0,              // Flags
    ImaskYSizeVec,   // Dispatch function
    2,              // Size of arg list
    ImaskSizeArgs
);

// dmask_xsize_desc
public static im_function DmaskXSizeDesc = new im_function(
    "im_dmask_xsize", // Name
    "horizontal size of a doublemask",
    0,             // Flags
    DmaskXSizeVec,   // Dispatch function
    2,             // Size of arg list
    DmaskSizeArgs
);

// dmask_ysize_desc
public static im_function DmaskYSizeDesc = new im_function(
    "im_dmask_ysize", // Name
    "vertical size of a doublemask",
    0,              // Flags
    DmaskYSizeVec,   // Dispatch function
    2,              // Size of arg list
    DmaskSizeArgs
);

// mask_list
public static im_function[] MaskList = new im_function[] {
    GaussDMaskDesc,
    GaussDMaskSepDesc,
    LogDmaskDesc,
    LogIMaskDesc,
    GaussIMaskDesc,
    GaussIMaskSepDesc,
    DmaskXSizeDesc,
    DmaskYSizeDesc,
    ImaskXSizeDesc,
    ImaskYSizeDesc,
    ReadDmaskDesc,
    RotateDmask45Desc,
    RotateDmask90Desc,
    RotateImask45Desc,
    RotateImask90Desc,
    MatCatDesc,
    MatInvDesc,
    MatMulDesc,
    MAttrnDesc
};

// im__mask
public static VipsPackage ImMask = new VipsPackage(
    "mask",
    MaskList.Length,
    MaskList
);
```