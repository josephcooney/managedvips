Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

// Converted from: vips_scRGB2sRGB_line_8()
void vips_scRGB2sRGBLine8(float[] p, int extraBands, int width)
{
    for (int i = 0; i < width; i++)
    {
        float R = p[0];
        float G = p[1];
        float B = p[2];

        int r, g, b;

        vips_col_scRGB2sRGB_8(R, G, B, out r, out g, out b);

        p += 3;

        VipsPel[] q = new VipsPel[3] { (VipsPel)r, (VipsPel)g, (VipsPel)b };

        for (int j = 0; j < extraBands; j++)
            q[j] = (VipsPel)VIPS_CLIP(0, (int)(p[j] * 255.0), UCHAR_MAX);

        p += extraBands;
    }
}

// Converted from: vips_scRGB2sRGB_line_16()
void vips_scRGB2sRGBLine16(short[] q, float[] p, int extraBands, int width)
{
    for (int i = 0; i < width; i++)
    {
        float R = p[0];
        float G = p[1];
        float B = p[2];

        int r, g, b;

        vips_col_scRGB2sRGB_16(R, G, B, out r, out g, out b);

        p += 3;

        q[0] = (short)r;
        q[1] = (short)g;
        q[2] = (short)b;

        for (int j = 0; j < extraBands; j++)
            q[j] = (short)VIPS_CLIP(0, (int)(p[j] * 65535.0), USHRT_MAX);

        p += extraBands;
    }
}

// Converted from: vips_scRGB2sRGB_gen()
int vips_scRGB2sRGBGen(VipsRegion outRegion, VipsscRGB2sRGB scRGB2sRGB)
{
    VipsRegion ir = (VipsRegion)scRGB2sRGB.In;
    VipsRect r = &outRegion.Valid;

    if (vips_region_prepare(ir, r))
        return -1;

    for (int y = 0; y < r.Height; y++)
    {
        float[] p = (float[])VIPS_REGION_ADDR(ir, r.Left, r.Top + y);
        VipsPel[] q = (VipsPel[])VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);

        if (scRGB2sRGB.Depth == 16)
            vips_scRGB2sRGBLine16((short[])q, p, ir.Image.Bands - 3, r.Width);
        else
            vips_scRGB2sRGBLine8(p, ir.Image.Bands - 3, r.Width);
    }

    return 0;
}

// Converted from: vips_scRGB2sRGB_build()
int vips_scRGB2sRGBBuild(VipsscRGB2sRGB scRGB2sRGB)
{
    VipsImage in = scRGB2sRGB.In;

    if (vips_check_bands_atleast(scRGB2sRGB.Class.Nickname, in, 3))
        return -1;

    // we are changing the gamma, so any profile on the image can no longer
    // work (and will cause horrible problems in any downstream colour
    // handling)
    if (vips_copy(in, out VipsImage[] t, null))
        return -1;
    in = t[0];
    vips_image_remove(in, VIPS_META_ICC_NAME);

    switch (scRGB2sRGB.Depth)
    {
        case 16:
            in.Interpretation = VIPS_INTERPRETATION_RGB16;
            in.Format = VIPS_FORMAT_USHORT;
            break;

        case 8:
            in.Interpretation = VIPS_INTERPRETATION_sRGB;
            in.Format = VIPS_FORMAT_UCHAR;
            break;

        default:
            vips_error(scRGB2sRGB.Class.Nickname, "%s", _("depth must be 8 or 16"));
            return -1;
    }

    if (vips_cast_float(in, out VipsImage[] t, null))
        return -1;
    in = t[1];

    VipsImage out = vips_image_new();
    if (vips_image_pipelinev(out,
            VIPS_DEMAND_STYLE_THINSTRIP, in, null)) {
        g_object_unref(out);
        return -1;
    }
    out.Type = in.Interpretation;
    out.BandFmt = in.Format;

    if (vips_image_generate(out,
            vips_start_one, vips_scRGB2sRGBGen, vips_stop_one,
            in, scRGB2sRGB)) {
        g_object_unref(out);
        return -1;
    }

    scRGB2sRGB.Out = out;

    return 0;
}

// Converted from: vips_scRGB2sRGB_class_init()
void VipsscRGB2sRGBClassInit(VipsscRGB2sRGBClass* class)
{
    GObjectClass gobjectClass = (GObjectClass)class;
    VipsObjectClass objectClass = (VipsObjectClass)class;
    VipsOperationClass operationClass = (VipsOperationClass)class;

    gobjectClass.SetProperty = vips_object_set_property;
    gobjectClass.GetProperty = vips_object_get_property;

    objectClass.Nickname = "scRGB2sRGB";
    objectClass.Description = _("convert an scRGB image to sRGB");
    objectClass.Build = vips_scRGB2sRGBBuild;

    operationClass.Flags = VIPS_OPERATION_SEQUENTIAL;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsscRGB2sRGB, In));

    VIPS_ARG_IMAGE(class, "out", 100,
        _("Output"),
        _("Output image"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsscRGB2sRGB, Out));

    VIPS_ARG_INT(class, "depth", 130,
        _("Depth"),
        _("Output device space depth in bits"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsscRGB2sRGB, Depth),
        8, 16, 8);
}

// Converted from: vips_scRGB2sRGB_init()
void VipsscRGB2sRGBInit(VipsscRGB2sRGB* scRGB2sRGB)
{
    scRGB2sRGB.Depth = 8;
}

public class VipsscRGB2sRGB : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }
    public int Depth { get; set; }

    public override int Build()
    {
        // ...
    }

    public override void ClassInit(VipsscRGB2sRGBClass* class)
    {
        // ...
    }

    public override void Init()
    {
        Depth = 8;
    }
}

public static class VipsScRGB2sRGB
{
    public static int ScRGB2sRGB(VipsImage in, out VipsImage[] out, ...)
    {
        // ...
    }
}
```

Note that this code assumes the existence of a `VipsDotNet` library, which is not included in the standard .NET Framework. You will need to install this library separately.

Also note that some parts of the original C code have been omitted or simplified for brevity and clarity.