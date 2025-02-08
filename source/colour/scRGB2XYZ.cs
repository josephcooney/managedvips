```csharp
// Converted from: vips_scRGB2XYZ_line

public static void VipsscRGB2XYZLine(float[] q, float[] p, int extraBands, int width)
{
    for (int i = 0; i < width; i++)
    {
        const float R = p[0] * VIPS_D65_Y0;
        const float G = p[1] * VIPS_D65_Y0;
        const float B = p[2] * VIPS_D65_Y0;

        // Manually inlined logic from the vips_col_scRGB2XYZ function
        q[0] = 0.4124f * R + 0.3576f * G + 0.1805f * B;
        q[1] = 0.2126f * R + 0.7152f * G + 0.0722f * B;
        q[2] = 0.0193f * R + 0.1192f * G + 0.9505f * B;

        p += 3;
        q += 3;

        for (int j = 0; j < extraBands; j++)
            q[j] = Math.Min(Math.Max(0, p[j] * 255.0f), 255.0f);
        p += extraBands;
        q += extraBands;
    }
}

// Converted from: vips_scRGB2XYZ_gen

public static int VipsscRGB2XYZGen(VipsRegion outRegion, object seq, object a, object b, bool[] stop)
{
    VipsRegion ir = (VipsRegion)seq;
    VipsRect r = outRegion.Valid;
    VipsImage inImage = ir.Im;

    int y;

    if (vips_region_prepare(ir, ref r))
        return -1;

    VIPS_GATE_START("vips_scRGB2XYZ_gen: work");

    for (y = 0; y < r.Height; y++)
    {
        float[] p = (float[])VIPS_REGION_ADDR(ir, r.Left, r.Top + y);
        float[] q = (float[])VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);

        VipsscRGB2XYZLine(q, p, inImage.Bands - 3, r.Width);
    }

    VIPS_GATE_STOP("vips_scRGB2XYZ_gen: work");

    return 0;
}

// Converted from: vips_scRGB2XYZ_build

public static int VipsscRGB2XYZBuild(VipsObject object)
{
    VipsObjectClass class = (VipsObjectClass)VIPS_OBJECT_GET_CLASS(object);
    VipsscRGB2XYZ scRGB2XYZ = (VipsscRGB2XYZ)object;

    VipsImage[] t = (VipsImage[])vips_object_local_array(object, 2);

    VipsImage inImage;
    VipsImage outImage;

    if (VIPS_OBJECT_CLASS(vips_scRGB2XYZ_parent_class).build(object))
        return -1;

    inImage = scRGB2XYZ.in;
    if (vips_check_bands_atleast(class.nickname, inImage, 3))
        return -1;

    if (vips_cast_float(inImage, ref t[0], null))
        return -1;
    inImage = t[0];

    outImage = vips_image_new();
    if (vips_image_pipelinev(outImage,
            VIPS_DEMAND_STYLE_THINSTRIP, inImage, null))
    {
        g_object_unref(outImage);
        return -1;
    }
    outImage.Type = VIPS_INTERPRETATION_XYZ;
    outImage.BandFmt = VIPS_FORMAT_FLOAT;

    if (vips_image_generate(outImage,
            vips_start_one, VipsscRGB2XYZGen, vips_stop_one,
            inImage, scRGB2XYZ))
    {
        g_object_unref(outImage);
        return -1;
    }

    object.SetProperty("out", outImage);

    return 0;
}

// Converted from: vips_scRGB2XYZ_class_init

public class VipsscRGB2XYZClass : VipsObjectClass
{
    public override void SetProperty(string name, object value)
    {
        base.SetProperty(name, value);
    }

    public override object GetProperty(string name)
    {
        return base.GetProperty(name);
    }
}

// Converted from: vips_scRGB2XYZ_init

public class VipsscRGB2XYZ : VipsObject
{
    public VipsscRGB2XYZ()
    {
    }
}

// Converted from: vips_scRGB2XYZ

public static int VipsscRGB2XYZ(VipsImage inImage, ref VipsImage outImage)
{
    va_list ap;
    int result;

    va_start(ap, outImage);
    result = vips_call_split("scRGB2XYZ", ap, inImage, ref outImage);
    va_end(ap);

    return result;
}
```