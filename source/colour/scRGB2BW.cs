Here is the converted C# code:

```csharp
using System;
using VipsDotNet;

// Converted from: vips_scRGB2BW_line_8
private static void vips_scRGB2BWLine8(float[] p, int extraBands, int width)
{
    for (int i = 0; i < width; i++)
    {
        float R = p[0];
        float G = p[1];
        float B = p[2];

        int g;
        int og;

        VipsCol.ScRGB2BW_8(R, G, B, out g, out og);

        p += 3;

        g = (byte)g;

        for (int j = 0; j < extraBands; j++)
            p[j] = (float)VIPS_CLIP(0, (int)(p[j] * 255.0), byte.MaxValue);
    }
}

// Converted from: vips_scRGB2BW_line_16
private static void vips_scRGB2BWLine16(short[] q, float[] p, int extraBands, int width)
{
    for (int i = 0; i < width; i++)
    {
        float R = p[0];
        float G = p[1];
        float B = p[2];

        int g;
        int og;

        VipsCol.ScRGB2BW_16(R, G, B, out g, out og);

        p += 3;

        q[0] = (short)g;

        for (int j = 0; j < extraBands; j++)
            q[j] = (short)VIPS_CLIP(0, (int)(p[j] * 65535.0), short.MaxValue);
    }
}

// Converted from: vips_scRGB2BW_gen
private static int vips_scRGB2BWGen(VipsRegion outRegion, VipsscRGB2BW scRGB2BW)
{
    VipsRegion inRegion = (VipsRegion)scRGB2BW.In;
    VipsRect r = outRegion.Valid;

    if (!vips_region_prepare(inRegion, r))
        return -1;

    int y;

    for (y = 0; y < r.Height; y++)
    {
        float[] p = (float[])VIPS_REGION_ADDR(inRegion, r.Left, r.Top + y);
        byte[] q = (byte[])VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);

        if (scRGB2BW.Depth == 16)
            vips_scRGB2BWLine16(q, p, inRegion.Image.Bands - 3, r.Width);
        else
            vips_scRGB2BWLine8(p, inRegion.Image.Bands - 3, r.Width);
    }

    return 0;
}

// Converted from: vips_scRGB2BW_build
private static int vips_scRGB2BWBuil(VipsscRGB2BW scRGB2BW)
{
    VipsImage in = scRGB2BW.In;

    if (vips_check_bands_atleast(scRGB2BW.Class.Nickname, in, 3))
        return -1;

    switch (scRGB2BW.Depth)
    {
        case 16:
            in.Interpretation = VIPS_INTERPRETATION_GREY16;
            break;
        case 8:
            in.Interpretation = VIPS_INTERPRETATION_B_W;
            break;
        default:
            vips_error(scRGB2BW.Class.Nickname, "%s", "depth must be 8 or 16");
            return -1;
    }

    if (vips_cast_float(in, out VipsImage))
        return -1;

    VipsImage out = in.Generate(VIPS_DEMAND_STYLE_THINSTRIP);

    out.Type = in.Interpretation;
    out.BandFmt = VIPS_FORMAT_UCHAR;
    out.Bands = in.Bands - 2;

    if (vips_image_pipeline(out))
        return -1;

    scRGB2BW.Out = out;

    return 0;
}

// Converted from: vips_scRGB2BW_class_init
private static void vips_scRGB2BWClassInit(VipsscRGB2BWClass class)
{
    VipsObjectClass objectClass = (VipsObjectClass)class;
    VipsOperationClass operationClass = (VipsOperationClass)class;

    objectClass.Nickname = "scRGB2BW";
    objectClass.Description = "convert scRGB to BW";

    operationClass.Flags = VIPS_OPERATION_SEQUENTIAL;

    VipsArg.Image(scRGB2BW.Class, "in", 1, "Input", "Input image");
    VipsArg.Image(scRGB2BW.Class, "out", 100, "Output", "Output image");
    VipsArg.Int(scRGB2BW.Class, "depth", 130, "Depth", "Output device space depth in bits");
}

// Converted from: vips_scRGB2BW_init
private static void vips_scRGB2BWInit(VipsscRGB2BW scRGB2BW)
{
    scRGB2BW.Depth = 8;
}

public class VipsscRGB2BW : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }
    public int Depth { get; set; }

    public override int Build()
    {
        return vips_scRGB2BWBuil(this);
    }

    public override void Gen(VipsRegion outRegion)
    {
        vips_scRGB2BWGen(outRegion, this);
    }
}

public static class VipsScRGB2BW
{
    public static int ScRGB2BW(VipsImage in, ref VipsImage out, params object[] args)
    {
        return VipsscRGB2BW.ScRGB2BW(in, out, args);
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available and contains the necessary classes and methods for working with VIPS images. You may need to modify the code to match your specific use case.