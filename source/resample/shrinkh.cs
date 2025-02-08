```csharp
// C method: vips_shrinkh_gen2
void VipsShrinkhGen2(VipsShrinkh* shrink, VipsRegion* out_region, VipsRegion* ir,
    int left, int top, int width)
{
    VipsResample* resample = VIPS_RESAMPLE(shrink);
    const int bands = resample->in->Bands *
        (vips_band_format_iscomplex(resample->in->BandFmt) ? 2 : 1);
    const int ne = shrink->hshrink * bands;
    VipsPel* out = VIPS_REGION_ADDR(out_region, left, top);
    VipsPel* in = VIPS_REGION_ADDR(ir, left * shrink->hshrink, top);

    int amend = shrink->hshrink / 2;

    int x;
    int x1, b;

    switch (resample->in->BandFmt)
    {
        case VipsFormat.UChar:
            {
                uint multiplier = (uint.MaxValue << 32) /
                    ((1 << 8) * shrink->hshrink);

                // Generate a special path for 1, 3 and 4 band uchar data.
                switch (bands)
                {
                    case 1:
                        UCHAR_SHRINK(1);
                        break;
                    case 3:
                        UCHAR_SHRINK(3);
                        break;
                    case 4:
                        UCHAR_SHRINK(4);
                        break;
                    default:
                        UCHAR_SHRINK(bands);
                        break;
                }
            }
            break;
        case VipsFormat.Char:
            ISHRINK(int, char, bands);
            break;
        case VipsFormat.UShort:
            ISHRINK(int, ushort, bands);
            break;
        case VipsFormat.Short:
            ISHRINK(int, short, bands);
            break;
        case VipsFormat.UInt:
            ISHRINK(long, uint, bands);
            break;
        case VipsFormat.Int:
            ISHRINK(long, int, bands);
            break;
        case VipsFormat.Float:
            FSHRINK(float);
            break;
        case VipsFormat.Double:
            FSHRINK(double);
            break;
        case VipsFormat.Complex:
            FSHRINK(float);
            break;
        case VipsFormat.DpComplex:
            FSHRINK(double);
            break;

        default:
            g_assert_not_reached();
    }
}

// C method: vips_shrinkh_gen
int VipsShrinkhGen(VipsRegion* out_region,
    void* seq, void* a, void* b, bool* stop)
{
    const int dy = vips__fatstrip_height;

    VipsShrinkh* shrink = (VipsShrinkh*)b;
    VipsRegion* ir = (VipsRegion*)seq;
    VipsRect* r = &out_region->valid;

    int y, y1;

#ifdef DEBUG
    Console.WriteLine("vips_shrinkh_gen: generating {0} x {1} at {2} x {3}",
        r->width, r->height, r->left, r->top);
#endif

    for (y = 0; y < r->height; y += dy)
    {
        int chunk_height = Math.Min(dy, r->height - y);

        VipsRect s;

        s.left = r->left * shrink->hshrink;
        s.top = r->top + y;
        s.width = r->width * shrink->hshrink;
        s.height = chunk_height;
#ifdef DEBUG
        Console.WriteLine("vips_shrinkh_gen: requesting {0} lines from {1}",
            s.height, s.top);
#endif
        if (vips_region_prepare(ir, ref s))
            return -1;

        VIPS_GATE_START("vips_shrinkh_gen: work");

        for (y1 = 0; y1 < chunk_height; y1++)
            vips_shrinkh_gen2(shrink, out_region, ir,
                r->left, r->top + y + y1, r->width);

        VIPS_GATE_STOP("vips_shrinkh_gen: work");
    }

    VIPS_COUNT_PIXELS(out_region, "vips_shrinkh_gen");

    return 0;
}

// C method: vips_shrinkh_build
int VipsShrinkhBuild(VipsObject* object)
{
    VipsObjectClass* class = VIPS_OBJECT_GET_CLASS(object);
    VipsResample* resample = VIPS_RESAMPLE(object);
    VipsShrinkh* shrink = (VipsShrinkh*)object;
    VipsImage** t = (VipsImage**)vips_object_local_array(object, 2);

    VipsImage* in;

    if (VIPS_OBJECT_CLASS(vips_shrinkh_parent_class)->build(object))
        return -1;

    in = resample->in;

    if (shrink->hshrink < 1)
    {
        vips_error(class->nickname,
            "%s", _("shrink factors should be >= 1"));
        return -1;
    }

    if (shrink->hshrink == 1)
        return vips_image_write(in, resample->out);

    // We need new pixels at the right so that we don't have small chunks
    // to average down the right edge.
    if (vips_embed(in, ref t[1],
            0, 0,
            in->Xsize + shrink->hshrink, in->Ysize,
            "extend", VIPS_EXTEND_COPY,
            null))
        return -1;
    in = t[1];

    if (vips_image_pipelinev(resample->out,
            VIPS_DEMAND_STYLE_THINSTRIP, in, null))
        return -1;

    // Size output.
    //
    // Don't change xres/yres, leave that to the application layer. For
    // example, vipsthumbnail knows the true shrink factor (including the
    // fractional part), we just see the integer part here.
    resample->out->Xsize = shrink->ceil
        ? Math.Ceiling((double)resample->in->Xsize / shrink->hshrink)
        : Math.Round((double)resample->in->Xsize / shrink->hshrink);
    if (resample->out->Xsize <= 0)
    {
        vips_error(class->nickname,
            "%s", _("image has shrunk to nothing"));
        return -1;
    }

#ifdef DEBUG
    Console.WriteLine("vips_shrinkh_build: shrinking {0} x {1} image to {2} x {3}",
        in->Xsize, in->Ysize,
        resample->out->Xsize, resample->out->Ysize);
#endif

    if (vips_image_generate(resample->out,
            vips_start_one, vips_shrinkh_gen, vips_stop_one,
            in, shrink))
        return -1;

    return 0;
}

// C method: vips_shrinkh_class_init
void VipsShrinkhClassInit(VipsShrinkhClass* class)
{
    GObjectClass* gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass* vobject_class = VIPS_OBJECT_CLASS(class);
    VipsOperationClass* operation_class = VIPS_OPERATION_CLASS(class);

    VIPS_DEBUG_MSG("vips_shrinkh_class_init\n");

    gobject_class->set_property = vips_object_set_property;
    gobject_class->get_property = vips_object_get_property;

    vobject_class->nickname = "shrinkh";
    vobject_class->description = _("shrink an image horizontally");
    vobject_class->build = vips_shrinkh_build;

    operation_class->flags = VIPS_OPERATION_SEQUENTIAL;

    // C method: vips_shrinkh_args
    VipsArgInt(vips_shrinkh_args, "hshrink", 8,
        _("Hshrink"),
        _("Horizontal shrink factor"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsShrinkh, hshrink),
        1, 1000000, 1);

    VipsArgBool(vips_shrinkh_args, "ceil", 10,
        _("Ceil"),
        _("Round-up output dimensions"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsShrinkh, ceil),
        false);

    // The old name .. now use h and v everywhere.
    VipsArgInt(vips_shrinkh_args, "xshrink", 8,
        _("Xshrink"),
        _("Horizontal shrink factor"),
        VIPS_ARGUMENT_REQUIRED_INPUT | VIPS_ARGUMENT_DEPRECATED,
        G_STRUCT_OFFSET(VipsShrinkh, hshrink),
        1, 1000000, 1);
}

// C method: vips_shrinkh_init
void VipsShrinkhInit(VipsShrinkh* shrink)
{
}

// C method: vips_shrinkh
int VipsShrinkh(VipsImage* in, ref VipsImage out, int hshrink, params object[] args)
{
    var result = vips_call_split("shrinkh", args, in, ref out, hshrink);

    return result;
}
```