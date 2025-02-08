```csharp
// vips_rot90_gen
public static int vips_rot90_gen(VipsRegion out_region, object seq, object in_image)
{
    // Output area.
    var r = out_region.Valid;
    int le = r.Left;
    int ri = VIPS_RECT_RIGHT(r);
    int to = r.Top;
    int bo = VIPS_RECT_BOTTOM(r);

    int x, y, i;

    // Pixel geometry.
    int ps, ls;

    // Find the area of the input image we need.
    var need = new VipsRect();
    need.Left = to;
    need.Top = in_image.Ysize - ri;
    need.Width = r.Height;
    need.Height = r.Width;
    if (vips_region_prepare(out_region, ref need))
        return -1;

    // Find PEL size and line skip for ir.
    ps = VIPS_IMAGE_SIZEOF_PEL(in_image);
    ls = VIPS_REGION_LSKIP(out_region);

    // Rotate the bit we now have.
    for (y = to; y < bo; y++)
    {
        // Start of this output line.
        var q = VIPS_REGION_ADDR(out_region, le, y);

        // Corresponding position in ir.
        var p = VIPS_REGION_ADDR((VipsRegion)seq, need.Left + y - to, need.Top + need.Height - 1);

        for (x = le; x < ri; x++)
        {
            for (i = 0; i < ps; i++)
                q[i] = p[i];

            q += ps;
            p -= ls;
        }
    }

    return 0;
}

// vips_rot180_gen
public static int vips_rot180_gen(VipsRegion out_region, object seq, object in_image)
{
    // Output area.
    var r = out_region.Valid;
    int le = r.Left;
    int ri = VIPS_RECT_RIGHT(r);
    int to = r.Top;
    int bo = VIPS_RECT_BOTTOM(r);

    int x, y, i;

    // Pixel geometry.
    int ps;

    // Find the area of the input image we need.
    var need = new VipsRect();
    need.Left = in_image.Xsize - ri;
    need.Top = in_image.Ysize - bo;
    need.Width = r.Width;
    need.Height = r.Height;
    if (vips_region_prepare(out_region, ref need))
        return -1;

    // Find PEL size and line skip for ir.
    ps = VIPS_IMAGE_SIZEOF_PEL(in_image);

    // Rotate the bit we now have.
    for (y = to; y < bo; y++)
    {
        // Start of this output line.
        var q = VIPS_REGION_ADDR(out_region, le, y);

        // Corresponding position in ir.
        var p = VIPS_REGION_ADDR((VipsRegion)seq, need.Left + need.Width - 1, need.Top + need.Height - (y - to) - 1);

        // Blap across!
        for (x = le; x < ri; x++)
        {
            for (i = 0; i < ps; i++)
                q[i] = p[i];

            q += ps;
            p -= ps;
        }
    }

    return 0;
}

// vips_rot270_gen
public static int vips_rot270_gen(VipsRegion out_region, object seq, object in_image)
{
    // Output area.
    var r = out_region.Valid;
    int le = r.Left;
    int ri = VIPS_RECT_RIGHT(r);
    int to = r.Top;
    int bo = VIPS_RECT_BOTTOM(r);

    int x, y, i;

    // Pixel geometry.
    int ps, ls;

    // Find the area of the input image we need.
    var need = new VipsRect();
    need.Left = in_image.Xsize - bo;
    need.Top = le;
    need.Width = r.Height;
    need.Height = r.Width;
    if (vips_region_prepare(out_region, ref need))
        return -1;

    // Find PEL size and line skip for ir.
    ps = VIPS_IMAGE_SIZEOF_PEL(in_image);
    ls = VIPS_REGION_LSKIP(out_region);

    // Rotate the bit we now have.
    for (y = to; y < bo; y++)
    {
        // Start of this output line.
        var q = VIPS_REGION_ADDR(out_region, le, y);

        // Corresponding position in ir.
        var p = VIPS_REGION_ADDR((VipsRegion)seq, need.Left + need.Width - (y - to) - 1, need.Top);

        for (x = le; x < ri; x++)
        {
            for (i = 0; i < ps; i++)
                q[i] = p[i];

            q += ps;
            p += ls;
        }
    }

    return 0;
}

// vips_rot_build
public static int vips_rot_build(VipsObject obj)
{
    var conversion = VIPS_CONVERSION(obj);
    var rot = (VipsRot)obj;

    VipsGenerateFn generate_fn;
    VipsDemandStyle hint;

    if (VIPS_OBJECT_CLASS(vips_rot_parent_class).build(obj))
        return -1;

    if (rot.Angle == VIPS_ANGLE_D0)
        return vips_image_write(rot.In, conversion.Out);

    if (vips_image_pio_input(rot.In))
        return -1;

    hint = rot.Angle == VIPS_ANGLE_D180
        ? VIPS_DEMAND_STYLE_THINSTRIP
        : VIPS_DEMAND_STYLE_SMALLTILE;

    if (vips_image_pipelinev(conversion.Out, hint, rot.In, null))
        return -1;

    switch (rot.Angle)
    {
        case VIPS_ANGLE_D90:
            generate_fn = vips_rot90_gen;
            conversion.Out.Xsize = rot.In.Ysize;
            conversion.Out.Ysize = rot.In.Xsize;
            conversion.Out.Xoffset = rot.In.Ysize;
            conversion.Out.Yoffset = 0;
            break;

        case VIPS_ANGLE_D180:
            generate_fn = vips_rot180_gen;
            conversion.Out.Xoffset = rot.In.Xsize;
            conversion.Out.Yoffset = rot.In.Ysize;
            break;

        case VIPS_ANGLE_D270:
            generate_fn = vips_rot270_gen;
            conversion.Out.Xsize = rot.In.Ysize;
            conversion.Out.Ysize = rot.In.Xsize;
            conversion.Out.Xoffset = 0;
            conversion.Out.Yoffset = rot.In.Xsize;
            break;

        default:
            g_assert_not_reached();

            // Stop compiler warnings.
            generate_fn = null;
    }

    if (vips_image_generate(conversion.Out,
        vips_start_one, generate_fn, vips_stop_one,
        rot.In, rot))
        return -1;

    return 0;
}

// vips_rot_class_init
public static void vips_rot_class_init(VipsRotClass class)
{
    var gobject_class = G_OBJECT_CLASS(class);
    var vobject_class = VIPS_OBJECT_CLASS(class);

    VIPS_DEBUG_MSG("vips_rot_class_init\n");

    gobject_class.set_property = vips_object_set_property;
    gobject_class.get_property = vips_object_get_property;

    vobject_class.nickname = "rot";
    vobject_class.description = _("rotate an image");
    vobject_class.build = vips_rot_build;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsRot, in));

    VIPS_ARG_ENUM(class, "angle", 6,
        _("Angle"),
        _("Angle to rotate image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsRot, angle),
        VIPS_TYPE_ANGLE, VIPS_ANGLE_D90);
}

// vips_rot_init
public static void vips_rot_init(VipsRot rot)
{
}

// vips_rotv
public static int vips_rotv(VipsImage in_image, ref VipsImage out_image, VipsAngle angle, params object[] args)
{
    return vips_call_split("rot", args, in_image, ref out_image, angle);
}

// vips_rot
public static int vips_rot(VipsImage in_image, ref VipsImage out_image, VipsAngle angle, params object[] args)
{
    var result = vips_rotv(in_image, ref out_image, angle, args);
    return result;
}

// vips_rot90
public static int vips_rot90(VipsImage in_image, ref VipsImage out_image, params object[] args)
{
    var result = vips_rotv(in_image, ref out_image, VIPS_ANGLE_D90, args);
    return result;
}

// vips_rot180
public static int vips_rot180(VipsImage in_image, ref VipsImage out_image, params object[] args)
{
    var result = vips_rotv(in_image, ref out_image, VIPS_ANGLE_D180, args);
    return result;
}

// vips_rot270
public static int vips_rot270(VipsImage in_image, ref VipsImage out_image, params object[] args)
{
    var result = vips_rotv(in_image, ref out_image, VIPS_ANGLE_D270, args);
    return result;
}
```