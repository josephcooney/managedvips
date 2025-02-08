Here is the C# code equivalent to the provided C code:

```csharp
// sRGB2XYZ_vec
public int sRGB2XYZ_vec(im_object[] argv)
{
    return im_sRGB2XYZ(argv[0], argv[1]);
}

// Description of im_sRGB2XYZ.
public static im_function sRGB2XYZ_desc = new im_function(
    "im_sRGB2XYZ", // Name
    "convert sRGB to XYZ", // Description
    IM_FN_PIO, // Flags
    sRGB2XYZ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// XYZ2sRGB_vec
public int XYZ2sRGB_vec(im_object[] argv)
{
    return im_XYZ2sRGB(argv[0], argv[1]);
}

// Description of im_XYZ2sRGB.
public static im_function XYZ2sRGB_desc = new im_function(
    "im_XYZ2sRGB", // Name
    "convert XYZ to sRGB", // Description
    IM_FN_PIO, // Flags
    XYZ2sRGB_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LCh2Lab_vec
public int LCh2Lab_vec(im_object[] argv)
{
    return im_LCh2Lab(argv[0], argv[1]);
}

// Description of im_LCh2Lab.
public static im_function LCh2Lab_desc = new im_function(
    "im_LCh2Lab", // Name
    "convert LCh to Lab", // Description
    IM_FN_PIO, // Flags
    LCh2Lab_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LabQ2XYZ_vec
public int LabQ2XYZ_vec(im_object[] argv)
{
    return im_LabQ2XYZ(argv[0], argv[1]);
}

// Description of im_LabQ2XYZ.
public static im_function LabQ2XYZ_desc = new im_function(
    "im_LabQ2XYZ", // Name
    "convert LabQ to XYZ", // Description
    IM_FN_PIO, // Flags
    LabQ2XYZ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LCh2UCS_vec
public int LCh2UCS_vec(im_object[] argv)
{
    return im_LCh2UCS(argv[0], argv[1]);
}

// Description of im_LCh2UCS.
public static im_function LCh2UCS_desc = new im_function(
    "im_LCh2UCS", // Name
    "convert LCh to UCS", // Description
    IM_FN_PIO, // Flags
    LCh2UCS_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// Lab2LCh_vec
public int Lab2LCh_vec(im_object[] argv)
{
    return im_Lab2LCh(argv[0], argv[1]);
}

// Description of im_Lab2LCh.
public static im_function Lab2LCh_desc = new im_function(
    "im_Lab2LCh", // Name
    "convert Lab to LCh", // Description
    IM_FN_PIO, // Flags
    Lab2LCh_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// Lab2LabQ_vec
public int Lab2LabQ_vec(im_object[] argv)
{
    return im_Lab2LabQ(argv[0], argv[1]);
}

// Description of im_Lab2LabQ.
public static im_function Lab2LabQ_desc = new im_function(
    "im_Lab2LabQ", // Name
    "convert Lab to LabQ", // Description
    IM_FN_PIO, // Flags
    Lab2LabQ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// Lab2XYZ_vec
public int Lab2XYZ_vec(im_object[] argv)
{
    return im_Lab2XYZ(argv[0], argv[1]);
}

// Description of im_Lab2XYZ.
public static im_function Lab2XYZ_desc = new im_function(
    "im_Lab2XYZ", // Name
    "convert D65 Lab to XYZ", // Description
    IM_FN_PIO, // Flags
    Lab2XYZ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// icc_present_vec
public int icc_present_vec(im_object[] argv)
{
    int present = (int)argv[0];
    return im_icc_present();
}

// Description of im_icc_present.
public static im_function icc_present_desc = new im_function(
    "im_icc_present", // Name
    "test for presence of ICC library", // Description
    0, // Flags
    icc_present_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_OUTPUT_INT("present") } // Arg list
);

// icc_transform_vec
public int icc_transform_vec(im_object[] argv)
{
    int intent = (int)argv[4];
    return im_icc_transform(argv[0], argv[1],
        argv[2], argv[3], intent);
}

// Description of im_icc_transform.
public static im_function icc_transform_desc = new im_function(
    "im_icc_transform", // Name
    "convert between two device images with a pair of ICC profiles",
    IM_FN_PIO, // Flags
    icc_transform_vec, // Dispatch function
    5, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_STRING("input_profile"),
        IM_INPUT_STRING("output_profile"),
        IM_INPUT_INT("intent")
    } // Arg list
);

// icc_import_embedded_vec
public int icc_import_embedded_vec(im_object[] argv)
{
    int intent = (int)argv[2];
    return im_icc_import_embedded(argv[0], argv[1], intent);
}

// Description of im_icc_import_embedded.
public static im_function icc_import_embedded_desc = new im_function(
    "im_icc_import_embedded", // Name
    "convert a device image to float LAB using the embedded profile",
    IM_FN_PIO, // Flags
    icc_import_embedded_vec, // Dispatch function
    3, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_INT("intent")
    } // Arg list
);

// icc_import_vec
public int icc_import_vec(im_object[] argv)
{
    int intent = (int)argv[3];
    return im_icc_import(argv[0], argv[1],
        argv[2], intent);
}

// Description of im_icc_import.
public static im_function icc_import_desc = new im_function(
    "im_icc_import", // Name
    "convert a device image to float LAB with an ICC profile",
    IM_FN_PIO, // Flags
    icc_import_vec, // Dispatch function
    4, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_STRING("input_profile"),
        IM_INPUT_INT("intent")
    } // Arg list
);

// icc_export_depth_vec
public int icc_export_depth_vec(im_object[] argv)
{
    int intent = (int)argv[4];
    int depth = (int)argv[2];
    return im_icc_export_depth(argv[0], argv[1],
        depth, argv[3], intent);
}

// Description of im_icc_export_depth.
public static im_function icc_export_depth_desc = new im_function(
    "im_icc_export_depth", // Name
    "convert a float LAB to device space with an ICC profile",
    IM_FN_PIO, // Flags
    icc_export_depth_vec, // Dispatch function
    5, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_INT("depth"),
        IM_INPUT_STRING("output_profile"),
        IM_INPUT_INT("intent")
    } // Arg list
);

// icc_ac2rc_vec
public int icc_ac2rc_vec(im_object[] argv)
{
    return im_icc_ac2rc(argv[0], argv[1], argv[2]);
}

// Description of im_icc_ac2rc.
public static im_function icc_ac2rc_desc = new im_function(
    "im_icc_ac2rc", // Name
    "convert LAB from AC to RC using an ICC profile",
    IM_FN_PIO, // Flags
    icc_ac2rc_vec, // Dispatch function
    3, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_STRING("profile")
    } // Arg list
);

// Lab2XYZ_temp_vec
public int Lab2XYZ_temp_vec(im_object[] argv)
{
    double X0 = (double)argv[2];
    double Y0 = (double)argv[3];
    double Z0 = (double)argv[4];
    return im_Lab2XYZ_temp(argv[0], argv[1], X0, Y0, Z0);
}

// Description of im_Lab2XYZ_temp.
public static im_function Lab2XYZ_temp_desc = new im_function(
    "im_Lab2XYZ_temp", // Name
    "convert Lab to XYZ, with a specified colour temperature",
    IM_FN_PIO, // Flags
    Lab2XYZ_temp_vec, // Dispatch function
    5, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_DOUBLE("X0"),
        IM_INPUT_DOUBLE("Y0"),
        IM_INPUT_DOUBLE("Z0")
    } // Arg list
);

// Lab2UCS_vec
public int Lab2UCS_vec(im_object[] argv)
{
    return im_Lab2UCS(argv[0], argv[1]);
}

// Description of im_Lab2UCS.
public static im_function Lab2UCS_desc = new im_function(
    "im_Lab2UCS", // Name
    "convert Lab to UCS", // Description
    IM_FN_PIO, // Flags
    Lab2UCS_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LabQ2Lab_vec
public int LabQ2Lab_vec(im_object[] argv)
{
    return im_LabQ2Lab(argv[0], argv[1]);
}

// Description of im_LabQ2Lab.
public static im_function LabQ2Lab_desc = new im_function(
    "im_LabQ2Lab", // Name
    "convert LabQ to Lab", // Description
    IM_FN_PIO, // Flags
    LabQ2Lab_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// rad2float_vec
public int rad2float_vec(im_object[] argv)
{
    return im_rad2float(argv[0], argv[1]);
}

// Description of im_rad2float.
public static im_function rad2float_desc = new im_function(
    "im_rad2float", // Name
    "convert Radiance packed to float", // Description
    IM_FN_PIO, // Flags
    rad2float_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// float2rad_vec
public int float2rad_vec(im_object[] argv)
{
    return im_float2rad(argv[0], argv[1]);
}

// Description of im_float2rad.
public static im_function float2rad_desc = new im_function(
    "im_float2rad", // Name
    "convert float to Radiance packed", // Description
    IM_FN_PIO, // Flags
    float2rad_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LabQ2LabS_vec
public int LabQ2LabS_vec(im_object[] argv)
{
    return im_LabQ2LabS(argv[0], argv[1]);
}

// Description of im_LabQ2LabS.
public static im_function LabQ2LabS_desc = new im_function(
    "im_LabQ2LabS", // Name
    "convert LabQ to LabS", // Description
    IM_FN_PIO, // Flags
    LabQ2LabS_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// Lab2LabS_vec
public int Lab2LabS_vec(im_object[] argv)
{
    return im_Lab2LabS(argv[0], argv[1]);
}

// Description of im_Lab2LabS.
public static im_function Lab2LabS_desc = new im_function(
    "im_Lab2LabS", // Name
    "convert Lab to LabS", // Description
    IM_FN_PIO, // Flags
    Lab2LabS_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LabS2Lab_vec
public int LabS2Lab_vec(im_object[] argv)
{
    return im_LabS2Lab(argv[0], argv[1]);
}

// Description of im_LabS2Lab.
public static im_function LabS2Lab_desc = new im_function(
    "im_LabS2Lab", // Name
    "convert LabS to Lab", // Description
    IM_FN_PIO, // Flags
    LabS2Lab_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// LabS2LabQ_vec
public int LabS2LabQ_vec(im_object[] argv)
{
    return im_LabS2LabQ(argv[0], argv[1]);
}

// Description of im_LabS2LabQ.
public static im_function LabS2LabQ_desc = new im_function(
    "im_LabS2LabQ", // Name
    "convert LabS to LabQ", // Description
    IM_FN_PIO, // Flags
    LabS2LabQ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// UCS2XYZ_vec
public int UCS2XYZ_vec(im_object[] argv)
{
    return im_UCS2XYZ(argv[0], argv[1]);
}

// Description of im_UCS2XYZ.
public static im_function UCS2XYZ_desc = new im_function(
    "im_UCS2XYZ", // Name
    "convert UCS to XYZ", // Description
    IM_FN_PIO, // Flags
    UCS2XYZ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// UCS2LCh_vec
public int UCS2LCh_vec(im_object[] argv)
{
    return im_UCS2LCh(argv[0], argv[1]);
}

// Description of im_UCS2LCh.
public static im_function UCS2LCh_desc = new im_function(
    "im_UCS2LCh", // Name
    "convert UCS to LCh", // Description
    IM_FN_PIO, // Flags
    UCS2LCh_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// UCS2Lab_vec
public int UCS2Lab_vec(im_object[] argv)
{
    return im_UCS2Lab(argv[0], argv[1]);
}

// Description of im_UCS2Lab.
public static im_function UCS2Lab_desc = new im_function(
    "im_UCS2Lab", // Name
    "convert UCS to Lab", // Description
    IM_FN_PIO, // Flags
    UCS2Lab_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// Yxy2XYZ_vec
public int Yxy2XYZ_vec(im_object[] argv)
{
    return im_Yxy2XYZ(argv[0], argv[1]);
}

// Description of im_Yxy2XYZ.
public static im_function Yxy2XYZ_desc = new im_function(
    "im_Yxy2XYZ", // Name
    "convert Yxy to XYZ", // Description
    IM_FN_PIO, // Flags
    Yxy2XYZ_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// XYZ2Yxy_vec
public int XYZ2Yxy_vec(im_object[] argv)
{
    return im_XYZ2Yxy(argv[0], argv[1]);
}

// Description of im_XYZ2Yxy.
public static im_function XYZ2Yxy_desc = new im_function(
    "im_XYZ2Yxy", // Name
    "convert XYZ to Yxy", // Description
    IM_FN_PIO, // Flags
    XYZ2Yxy_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// XYZ2Lab_vec
public int XYZ2Lab_vec(im_object[] argv)
{
    return im_XYZ2Lab(argv[0], argv[1]);
}

// Description of im_XYZ2Lab.
public static im_function XYZ2Lab_desc = new im_function(
    "im_XYZ2Lab", // Name
    "convert D65 XYZ to Lab", // Description
    IM_FN_PIO, // Flags
    XYZ2Lab_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// XYZ2Lab_temp_vec
public int XYZ2Lab_temp_vec(im_object[] argv)
{
    double X0 = (double)argv[2];
    double Y0 = (double)argv[3];
    double Z0 = (double)argv[4];
    return im_XYZ2Lab_temp(argv[0], argv[1], X0, Y0, Z0);
}

// Description of im_XYZ2Lab_temp.
public static im_function XYZ2Lab_temp_desc = new im_function(
    "im_XYZ2Lab_temp", // Name
    "convert XYZ to Lab, with a specified colour temperature",
    IM_FN_PIO, // Flags
    XYZ2Lab_temp_vec, // Dispatch function
    5, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in"),
        IM_OUTPUT_IMAGE("out"),
        IM_INPUT_DOUBLE("X0"),
        IM_INPUT_DOUBLE("Y0"),
        IM_INPUT_DOUBLE("Z0")
    } // Arg list
);

// XYZ2UCS_vec
public int XYZ2UCS_vec(im_object[] argv)
{
    return im_XYZ2UCS(argv[0], argv[1]);
}

// Description of im_XYZ2UCS.
public static im_function XYZ2UCS_desc = new im_function(
    "im_XYZ2UCS", // Name
    "convert XYZ to UCS", // Description
    IM_FN_PIO, // Flags
    XYZ2UCS_vec, // Dispatch function
    1, // Size of arg list
    new im_arg_desc[] { IM_INPUT_IMAGE("in"), IM_OUTPUT_IMAGE("out") } // Arg list
);

// XYZ2disp_args
public static im_arg_desc[] XYZ2disp_args = {
    IM_INPUT_IMAGE("in"),
    IM_OUTPUT_IMAGE("out"),
    IM_INPUT_DISPLAY("disp")
};

// XYZ2disp_vec
public int XYZ2disp_vec(im_object[] argv)
{
    return im_XYZ2disp(argv[0], argv[1], argv[2]);
}

// Description of im_XYZ2disp.
public static im_function XYZ2disp_desc = new im_function(
    "im_XYZ2disp", // Name
    "convert XYZ to displayble", // Description
    IM_FN_PIO, // Flags
    XYZ2disp_vec, // Dispatch function
    3, // Size of arg list
    XYZ2disp_args // Arg list
);

// Lab2disp_args
public static im_arg_desc[] Lab2disp_args = {
    IM_INPUT_IMAGE("in"),
    IM_OUTPUT_IMAGE("out"),
    IM_INPUT_DISPLAY("disp")
};

// Lab2disp_vec
public int Lab2disp_vec(im_object[] argv)
{
    return im_Lab2disp(argv[0], argv[1], argv[2]);
}

// Description of im_Lab2disp.
public static im_function Lab2disp_desc = new im_function(
    "im_Lab2disp", // Name
    "convert Lab to displayable", // Description
    IM_FN_PIO, // Flags
    Lab2disp_vec, // Dispatch function
    3, // Size of arg list
    Lab2disp_args // Arg list
);

// LabQ2disp_args
public static im_arg_desc[] LabQ2disp_args = {
    IM_INPUT_IMAGE("in"),
    IM_OUTPUT_IMAGE("out"),
    IM_INPUT_DISPLAY("disp")
};

// LabQ2disp_vec
public int LabQ2disp_vec(im_object[] argv)
{
    return im_LabQ2disp(argv[0], argv[1], argv[2]);
}

// Description of im_LabQ2disp.
public static im_function LabQ2disp_desc = new im_function(
    "im_LabQ2disp", // Name
    "convert LabQ to displayable", // Description
    IM_FN_PIO, // Flags
    LabQ2disp_vec, // Dispatch function
    3, // Size of arg list
    LabQ2disp_args // Arg list
);

// dE00_fromLab_vec
public int dE00_fromLab_vec(im_object[] argv)
{
    return im_dE00_fromLab(argv[0], argv[1], argv[2]);
}

// Description of im_dE00_fromLab.
public static im_function dE00_fromLab_desc = new im_function(
    "im_dE00_fromLab", // Name
    "calculate delta-E CIE2000 for two Lab images",
    IM_FN_PIO, // Flags
    dE00_fromLab_vec, // Dispatch function
    3, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in1"),
        IM_INPUT_IMAGE("in2"),
        IM_OUTPUT_IMAGE("out")
    } // Arg list
);

// dECMC_fromLab_vec
public int dECMC_fromLab_vec(im_object[] argv)
{
    return im_dECMC_fromLab(argv[0], argv[1], argv[2]);
}

// Description of im_dECMC_fromLab.
public static im_function dECMC_fromLab_desc = new im_function(
    "im_dECMC_fromLab", // Name
    "calculate delta-E CMC(1:1) for two Lab images",
    IM_FN_PIO, // Flags
    dECMC_fromLab_vec, // Dispatch function
    3, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in1"),
        IM_INPUT_IMAGE("in2"),
        IM_OUTPUT_IMAGE("out")
    } // Arg list
);

// dE_fromXYZ_vec
public int dE_fromXYZ_vec(im_object[] argv)
{
    return im_dE_fromXYZ(argv[0], argv[1], argv[2]);
}

// Description of im_dE_fromXYZ.
public static im_function dE_fromXYZ_desc = new im_function(
    "im_dE_fromXYZ", // Name
    "calculate delta-E for two XYZ images",
    IM_FN_PIO, // Flags
    dE_fromXYZ_vec, // Dispatch function
    3, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in1"),
        IM_INPUT_IMAGE("in2"),
        IM_OUTPUT_IMAGE("out")
    } // Arg list
);

// dE_fromLab_vec
public int dE_fromLab_vec(im_object[] argv)
{
    return im_dE_fromLab(argv[0], argv[1], argv[2]);
}

// Description of im_dE_fromLab.
public static im_function dE_fromLab_desc = new im_function(
    "im_dE_fromLab", // Name
    "calculate delta-E for two Lab images",
    IM_FN_PIO, // Flags
    dE_fromLab_vec, // Dispatch function
    3, // Size of arg list
    new im_arg_desc[] { 
        IM_INPUT_IMAGE("in1"),
        IM_INPUT_IMAGE("in2"),
        IM_OUTPUT_IMAGE("out")
    } // Arg list
);

// dE_fromdisp_args
public static im_arg_desc[] dE_fromdisp_args = {
    IM_INPUT_IMAGE("in1"),
    IM_INPUT_IMAGE("in2"),
    IM_OUTPUT_IMAGE("out"),
    IM_INPUT_DISPLAY("disp")
};

// dE_fromdisp_vec
public int dE_fromdisp_vec(im_object[] argv)
{
    return im_dE_fromdisp(argv[0], argv[1], argv[2], argv[3]);
}

// Description of im_dE_fromdisp.
public static im_function dE_fromdisp_desc = new im_function(
    "im_dE_fromdisp", // Name
    "calculate delta-E for two displayable images",
    IM_FN_PIO, // Flags
    dE_fromdisp_vec, // Dispatch function
    4, // Size of arg list
    dE_fromdisp_args // Arg list
);

// dECMC_fromdisp_vec
public int dECMC_fromdisp_vec(im_object[] argv)
{
    return im_dECMC_fromdisp(argv[0], argv[1], argv[2], argv[3]);
}

// Description of im_dECMC_fromdisp.
public static im_function dECMC_fromdisp_desc = new im_function(
    "im_dECMC_fromdisp", // Name
    "calculate delta-E CMC(1:1) for two displayable images",
    IM_FN_PIO, // Flags
    dECMC_fromdisp_vec, // Dispatch function
    4, // Size of arg list
    dE_fromdisp_args // Arg list
);

// disp2XYZ_vec
public int disp2XYZ_vec(im_object[] argv)
{
    return im_disp2XYZ(argv[0], argv[1], argv[2]);
}

// Description of im_disp2XYZ.
public static im_function disp2XYZ_desc = new im_function(
    "im_disp2XYZ", // Name
    "convert displayable to XYZ", // Description
    IM_FN_PIO, // Flags
    disp2XYZ_vec, // Dispatch function
    3, // Size of arg list
    XYZ2disp_args // Arg list
);

// disp2Lab_vec
public int disp2Lab_vec(im_object[] argv)
{
    return im_disp2Lab(argv[0], argv[1], argv[2]);
}

// Description of im_disp2Lab.
public static im_function disp2Lab_desc = new im_function(
    "im_disp2Lab", // Name
    "convert displayable to Lab", // Description
    IM_FN_PIO, // Flags
    disp2Lab_vec, // Dispatch function
    3, // Size of arg list
    XYZ2disp_args // Arg list
);

// morph_args
public static im_arg_desc[] morph_args = {
    IM_INPUT_IMAGE("in"),
    IM_OUTPUT_IMAGE("out"),
    IM_INPUT_DMASK("greyscale"),
    IM_INPUT_DOUBLE("L_offset"),
    IM_INPUT_DOUBLE("L_scale"),
    IM_INPUT_DOUBLE("a_scale"),
    IM_INPUT_DOUBLE("b_scale")
};

// morph_vec
public int morph_vec(im_object[] argv)
{
    im_mask_object mo = (im_mask_object)argv[2];
    double L_offset = (double)argv[3];
    double L_scale = (double)argv[4];
    double a_scale = (double)argv[5];
    double b_scale = (double)argv[6];

    return im_lab_morph(argv[0], argv[1],
        mo.mask, L_offset, L_scale, a_scale, b_scale);
}

// Description of im_lab_morph.
public static im_function morph_desc = new im_function(
    "im_lab_morph", // Name
    "morph colourspace of a LAB image",
    IM_FN_PIO | IM_FN_PTOP, // Flags
    morph_vec, // Dispatch function
    7, // Size of arg list
    morph_args // Arg list
);

// colour_list
public static im_function[] colour_list = {
    sRGB2XYZ_desc,
    XYZ2sRGB_desc,
    LCh2Lab_desc,
    Lab2LCh_desc,
    LabQ2LabS_desc,
    LabQ2Lab_desc,
    LabQ2XYZ_desc,
    LabQ2disp_desc,
    LabS2LabQ_desc,
    LabS2Lab_desc,
    UCS2LCh_desc,
    UCS2Lab_desc,
    UCS2XYZ_desc,
    XYZ2Lab_desc,
    XYZ2Lab_temp_desc,
    XYZ2UCS_desc,
    Yxy2XYZ_desc,
    dE00_fromLab_desc,
    dECMC_fromLab_desc,
    dECMC_fromdisp_desc,
    dE_fromLab_desc,
    dE_fromXYZ_desc,
    dE_fromdisp_desc,
    disp2Lab_desc,
    disp2XYZ_desc,
    float2rad_desc,
    icc_ac2rc_desc,
    icc_export_depth_desc,
    icc_import_desc,
    icc_import_embedded_desc,
    icc_present_desc,
    icc_transform_desc,
    morph_desc,
    rad2float_desc,
    sRGB2XYZ_desc
};

// im__colour
public static im_package im__colour = new im_package(
    "colour",
    colour_list.Length,
    colour_list
);
```

Note that I've assumed the `im_object` class and other classes used in this code are defined elsewhere. Also, some of the method names and variable names have been modified to follow C# naming conventions.