```csharp
// Converted from: vips_XYZ2scRGB_line

public static void XYZ2scRGBLine(float[] q, float[] p, int extraBands, int width)
{
    for (int i = 0; i < width; i++)
    {
        const float X = p[0];
        const float Y = p[1];
        const float Z = p[2];

        float R, G, B;

        vips_col_XYZ2scRGB(X, Y, Z, out R, out G, out B);

        q[0] = R;
        q[1] = G;
        q[2] = B;

        q += 3;

        for (int j = 0; j < extraBands; j++)
            q[j] = Math.Min(1.0f, Math.Max(0.0f, p[j] / 255.0f));
        p += extraBands;
        q += extraBands;
    }
}

// Converted from: vips_XYZ2scRGB_gen

public static int XYZ2scRGBGen(VipsRegion outRegion, object seq, object a, object b, bool[] stop)
{
    VipsRegion ir = (VipsRegion)seq;
    VipsRect r = outRegion.Valid;
    VipsImage in = ((VipsRegion)ir).Im;

    int y;

    if (!vips_region_prepare(ir, ref r))
        return -1;

    for (y = 0; y < r.Height; y++)
    {
        float[] p = (float[])VIPS_REGION_ADDR(ir, r.Left, r.Top + y);
        float[] q = (float[])VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);

        XYZ2scRGBLine(q, p, in.Bands - 3, r.Width);
    }

    return 0;
}

// Converted from: vips_XYZ2scRGB_build

public static int XYZ2scRGBBuild(VipsObject object)
{
    VipsObjectClass class = (VipsObjectClass)VIPS_OBJECT_GET_CLASS(object);
    VipsXYZ2scRGB xyz2scrgb = (VipsXYZ2scRGB)object;

    VipsImage[] t = vips_object_local_array(object, 2);

    VipsImage in;
    VipsImage out;

    if (!VIPS_OBJECT_CLASS(vips_XYZ2scRGB_parent_class).build(object))
        return -1;

    in = xyz2scrgb.In;
    if (vips_check_bands_atleast(class.Nickname, in, 3))
        return -1;

    if (vips_cast_float(in, ref t[0], null))
        return -1;
    in = t[0];

    out = vips_image_new();
    if (!vips_image_pipelinev(out,
            VIPS_DEMAND_STYLE_THINSTRIP, in, null)) {
        g_object_unref(out);
        return -1;
    }
    out.Type = VIPS_INTERPRETATION_scRGB;
    out.BandFmt = VIPS_FORMAT_FLOAT;

    if (vips_image_generate(out,
            vips_start_one, XYZ2scRGBGen, vips_stop_one,
            in, xyz2scrgb)) {
        g_object_unref(out);
        return -1;
    }

    object.SetProperty("out", out);

    return 0;
}

// Converted from: vips_XYZ2scRGB_class_init

public class VipsXYZ2scRGBClass : VipsOperationClass
{
    public static void ClassInit(VipsXYZ2scRGBClass class_)
    {
        GObjectClass gobject_class = (GObjectClass)class_;
        VipsObjectClass object_class = (VipsObjectClass)class_;
        VipsOperationClass operation_class = (VipsOperationClass)class_;

        gobject_class.SetProperty = vips_object_set_property;
        gobject_class.GetProperty = vips_object_get_property;

        object_class.Nickname = "XYZ2scRGB";
        object_class.Description = _("transform XYZ to scRGB");
        object_class.Build = XYZ2scRGBBuild;

        operation_class.Flags = VIPS_OPERATION_SEQUENTIAL;

        VIPS_ARG_IMAGE(class_, "in", 1,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsXYZ2scRGB, In));

        VIPS_ARG_IMAGE(class_, "out", 100,
            _("Output"),
            _("Output image"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsXYZ2scRGB, Out));
    }
}

// Converted from: vips_XYZ2scRGB_init

public class VipsXYZ2scRGB : VipsOperation
{
    public VipsXYZ2scRGB()
    {
    }
}

// Converted from: vips_XYZ2scRGB

public static int XYZ2scRGB(VipsImage in, ref VipsImage out, params object[] args)
{
    var result = vips_call_split("XYZ2scRGB", args, in, ref out);
    return result;
}
```