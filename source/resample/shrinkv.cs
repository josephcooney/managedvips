```csharp
// vips_shrinkv_gen2 method converted from C code
public void VipsShrinkvGen2(VipsShrinkv shrink, VipsRegion out_region, VipsRegion ir,
    int left, int top, int width)
{
    VipsResample resample = (VipsResample)shrink;
    int bands = resample.In.Bands * 
        (VipsBandFormat.IsComplex(resample.In.BandFmt) ? 2 : 1);
    int ls = VIPS_REGION_LSKIP(ir) / 
        VIPS_IMAGE_SIZEOF_ELEMENT(resample.In);

    int amend = shrink.Vshrink / 2;

    int x, b;

    switch (resample.In.BandFmt)
    {
        case VipsFormat.UChar:
            // Generate a special path for 1, 3 and 4 band uchar data.
            // The compiler will be able to vectorise these.
            switch (bands)
            {
                case 1:
                    UcharShrink(1);
                    break;
                case 3:
                    UcharShrink(3);
                    break;
                case 4:
                    UcharShrink(4);
                    break;
                default:
                    UcharShrink(bands);
                    break;
            }
            break;

        // Integer shrink.
        case VipsFormat.Char:
            IsHrink(int, char, bands);
            break;
        case VipsFormat.UShort:
            IsHrink(int, ushort, bands);
            break;
        case VipsFormat.Short:
            IsHrink(int, short, bands);
            break;
        case VipsFormat.UInt:
            IsHrink(long, uint, bands);
            break;
        case VipsFormat.Int:
            IsHrink(long, int, bands);
            break;

        // Float shrink.
        case VipsFormat.Float:
            FShrink(float);
            break;
        case VipsFormat.Double:
            FShrink(double);
            break;
        case VipsFormat.Complex:
            FShrink(float);
            break;
        case VipsFormat.DpComplex:
            FShrink(double);
            break;

        default:
            g_assert_not_reached();
    }
}

// vips_shrinkv_gen method converted from C code
public int VipsShrinkvGen(VipsRegion out_region,
    object seq, object a, object b, bool[] stop)
{
    VipsShrinkv shrink = (VipsShrinkv)b;
    VipsRegion ir = (VipsRegion)seq;

    // How do we chunk up the output image? We don't want to prepare the
    // whole of the input region corresponding to *r since it could be huge.
    int input_target = VIPS_MAX(shrink.Vshrink, out_region.Valid.Height);
    int dy = input_target / shrink.Vshrink;

    int y, y1;

    for (y = 0; y < out_region.Valid.Height; y += dy)
    {
        int chunk_height = VIPS_MIN(dy, out_region.Valid.Height - y);

        VipsRect s;

        s.Left = out_region.Valid.Left;
        s.Top = (out_region.Valid.Top + y) * shrink.Vshrink;
        s.Width = out_region.Valid.Width;
        s.Height = chunk_height * shrink.Vshrink;

        if (vips_region_prepare(ir, ref s))
            return -1;

        VIPS_GATE_START("VipsShrinkvGen: work");

        for (y1 = 0; y1 < chunk_height; y1++)
            vips_shrinkv_gen2(shrink, out_region, ir,
                out_region.Valid.Left, out_region.Valid.Top + y + y1, out_region.Valid.Width);

        VIPS_GATE_STOP("VipsShrinkvGen: work");
    }

    VIPS_COUNT_PIXELS(out_region, "VipsShrinkvGen");

    return 0;
}

// vips_shrinkv_build method converted from C code
public int VipsShrinkvBuild(VipsObject object)
{
    VipsObjectClass class = (VipsObjectClass)VIPS_OBJECT_GET_CLASS(object);
    VipsResample resample = (VipsResample)object;
    VipsShrinkv shrink = (VipsShrinkv)object;

    // Make the height a multiple of the shrink factor so we don't need to
    // average half pixels.
    if (shrink.Vshrink < 1)
    {
        vips_error(class.Nickname, "%s", _("shrink factors should be >= 1"));
        return -1;
    }

    if (shrink.Vshrink == 1)
        return vips_image_write(resample.In, resample.Out);

    // SMALLTILE or we'll need huge input areas for our output. In seq
    // mode, the linecache above will keep us sequential.
    VipsImage[] t = new VipsImage[4];
    if (vips_embed(resample.In, ref t[1],
        0, 0,
        resample.In.Xsize, VIPS_ROUND_UP(resample.In.Ysize, shrink.Vshrink),
        "extend", VIPS_EXTEND_COPY,
        null))
        return -1;
    resample.In = t[1];

    // Size output.
    //
    // Don't change xres/yres, leave that to the application layer. For
    // example, vipsthumbnail knows the true shrink factor (including the
    // fractional part), we just see the integer part here.
    t[2] = vips_image_new();
    if (vips_image_pipelinev(t[2],
        VIPS_DEMAND_STYLE_SMALLTILE, resample.In, null))
        return -1;

    t[2].Ysize = shrink.Ceil
        ? VIPS_CEIL((double)resample.In.Ysize / shrink.Vshrink)
        : VIPS_ROUND_UINT((double)resample.In.Ysize / shrink.Vshrink);
    if (t[2].Ysize <= 0)
    {
        vips_error(class.Nickname, "%s", _("image has shrunk to nothing"));
        return -1;
    }

    // Large vshrinks will throw off sequential mode. Suppose thread1 is
    // generating tile (0, 0), but stalls. thread2 generates tile
    // (0, 1), 128 lines further down the output. After it has done,
    // thread1 tries to generate (0, 0), but by then the pixels it needs
    // have gone from the input image line cache if the vshrink is large.
    //
    // To fix this, put another seq on the output of vshrink. Now we'll
    // always have the previous XX lines of the shrunk image, and we won't
    // fetch out of order.
    if (vips_image_is_sequential(resample.In))
    {
        g_info("Shrinkv sequential line cache");

        if (vips_sequential(resample.In, ref t[3],
            "tile_height", 10,
            null))
            return -1;
        resample.In = t[3];
    }

    if (vips_image_write(resample.In, resample.Out))
        return -1;

    return 0;
}

// vips_shrinkv_class_init method converted from C code
public void VipsShrinkvClassInit(VipsShrinkvClass class)
{
    GObjectClass gobject_class = (GObjectClass)class;
    VipsObjectClass vobject_class = (VipsObjectClass)class;
    VipsOperationClass operation_class = (VipsOperationClass)class;

    VIPS_DEBUG_MSG("VipsShrinkvClassInit\n");

    gobject_class.SetProperty = vips_object_set_property;
    gobject_class.GetProperty = vips_object_get_property;

    vobject_class.Nickname = "shrinkv";
    vobject_class.Description = _("shrink an image vertically");
    vobject_class.Build = vips_shrinkv_build;

    operation_class.Flags = VIPS_OPERATION_SEQUENTIAL;

    // Argument: vertical shrink
    VipsArgInt arg_vshrink = new VipsArgInt("vshrink", 9,
        _("Vshrink"),
        _("Vertical shrink factor"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsShrinkv, Vshrink),
        1, 1000000, 1);

    // Argument: ceil
    VipsArgBool arg_ceil = new VipsArgBool("ceil", 10,
        _("Ceil"),
        _("Round-up output dimensions"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsShrinkv, Ceil),
        false);

    // The old name .. now use h and v everywhere.
    VipsArgInt arg_yshrink = new VipsArgInt("yshrink", 8,
        _("Yshrink"),
        _("Vertical shrink factor"),
        VIPS_ARGUMENT_REQUIRED_INPUT | VIPS_ARGUMENT_DEPRECATED,
        G_STRUCT_OFFSET(VipsShrinkv, Vshrink),
        1, 1000000, 1);
}

// vips_shrinkv_init method converted from C code
public void VipsShrinkvInit(VipsShrinkv shrink)
{
}

// vips_shrinkv method converted from C code
public int VipsShrinkv(VipsImage in, ref VipsImage out, int vshrink, params object[] args)
{
    // Optional arguments:
    //
    // * ceil: round-up output dimensions

    // Shrink @in vertically by an integer factor. Each pixel in the output is
    // the average of the corresponding column of @vshrink pixels in the input.
    //
    // This is a very low-level operation: see vips_resize() for a more
    // convenient way to resize images.
    //
    // This operation does not change xres or yres. The image resolution needs to
    // be updated by the application.
    //
    // See also: vips_shrinkh(), vips_shrink(), vips_resize(), vips_affine().
    //
    // Returns: 0 on success, -1 on error
    return VipsCallSplit("shrinkv", args, in, ref out, vshrink);
}
```