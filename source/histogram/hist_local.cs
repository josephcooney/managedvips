```csharp
// Converted from vips_hist_local_generate()

public int Generate(VipsRegion out_region, object vseq, VipsImage in_image, VipsHistLocal local)
{
    VipsHistLocalSequence seq = (VipsHistLocalSequence)vseq;
    VipsRect r = out_region.Valid;
    const int bands = in_image.Bands;
    const int max_slope = local.MaxSlope;

    VipsRect irect;
    int y;
    int lsk; // line skip
    int centre; // offset to move to centre of window

    // What part of ir do we need?
    irect.Left = r.Left;
    irect.Top = r.Top;
    irect.Width = r.Width + local.Width;
    irect.Height = r.Height + local.Height;

    if (vips_region_prepare(seq.Ir, ref irect))
        return -1;

    lsk = VIPS_REGION_LSKIP(seq.Ir);
    centre = lsk * (local.Height / 2) + bands * (local.Width / 2);

    for (y = 0; y < r.Height; y++)
    {
        // Get input and output pointers for this line.
        VipsPel[] p = VIPS_REGION_ADDR(seq.Ir, r.Left, r.Top + y);
        VipsPel[] q = VIPS_REGION_ADDR(out_region, r.Left, r.Top + y);

        VipsPel[] p1;
        int x, i, j, b;

        // Find histogram for the start of this line.
        for (b = 0; b < bands; b++)
            Array.Clear(seq.Hist[b], 0, 256);
        p1 = p;
        for (j = 0; j < local.Height; j++)
        {
            for (i = 0, x = 0; x < local.Width; x++, i++)
                seq.Hist[b][p1[i]] += 1;

            p1 += lsk;
        }

        // Loop for output pels.
        for (x = 0; x < r.Width; x++)
        {
            for (b = 0; b < bands; b++)
            {
                // Sum histogram up to current pel.
                unsigned int[] hist = seq.Hist[b];
                int target = p[centre + b];

                int sum;

                sum = 0;

                // For CLAHE we need to limit the height of the
                // hist to limit the amount we boost the
                // contrast by.
                if (max_slope > 0)
                {
                    int sum_over;

                    sum_over = 0;

                    // Must be <= target, since a cum hist
                    // always includes the current element.
                    for (i = 0; i <= target; i++)
                    {
                        if (hist[i] > max_slope)
                        {
                            sum_over += hist[i] - max_slope;
                            sum += max_slope;
                        }
                        else
                            sum += hist[i];
                    }

                    for (; i < 256; i++)
                    {
                        if (hist[i] > max_slope)
                            sum_over += hist[i] - max_slope;
                    }

                    // The extra clipped off bit from the
                    // top of the hist is spread over all
                    // bins equally, then summed to target.
                    sum += (target + 1) * sum_over / 256;
                }
                else
                {
                    sum = 0;
                    for (i = 0; i <= target; i++)
                        sum += hist[i];
                }

                // This can't overflow, even in
                // contrast-limited mode.
                //
                // Scale by 255, not 256, or we'll get
                // overflow.
                q[b] = (int)(255 * sum / (local.Width * local.Height));

                // Adapt histogram --- remove the pels from
                // the left hand column, add in pels for a
                // new right-hand column.
                p1 = p + b;
                for (j = 0; j < local.Height; j++)
                {
                    hist[p1[0]] -= 1;
                    hist[p1[bands * local.Width]] += 1;

                    p1 += lsk;
                }
            }

            p += bands;
            q += bands;
        }
    }

    return 0;
}

// Converted from vips_hist_local_build()

public int Build(VipsObject obj)
{
    VipsHistLocal local = (VipsHistLocal)obj;
    VipsImage[] t = new VipsImage[3];

    VipsImage in_image;

    if (VIPS_OBJECT_CLASS(vips_hist_local_parent_class).Build(obj))
        return -1;

    in_image = local.In;

    if (vips_image_decode(in_image, ref t[0]))
        return -1;
    in_image = t[0];

    if (vips_check_format(VIPS_OPERATION_NICKNAME(local), in_image, VIPS_FORMAT_UCHAR))
        return -1;

    if (local.Width > in_image.Xsize || local.Height > in_image.Ysize)
    {
        vips_error(VIPS_OPERATION_NICKNAME(local), "%s", _("window too large"));
        return -1;
    }

    // Expand the input.
    if (vips_embed(in_image, ref t[1], local.Width / 2, local.Height / 2,
            in_image.Xsize + local.Width - 1, in_image.Ysize + local.Height - 1,
            "extend", VIPS_EXTEND_MIRROR, null))
        return -1;
    in_image = t[1];

    g_object_set(obj, "out", vips_image_new(), null);

    // Set demand hints. FATSTRIP is good for us, as THINSTRIP will cause
    // too many recalculations on overlaps.
    if (vips_image_pipelinev(local.Out,
            VIPS_DEMAND_STYLE_FATSTRIP, in_image, null))
        return -1;
    local.Out.Xsize -= local.Width - 1;
    local.Out.Ysize -= local.Height - 1;

    if (vips_image_generate(local.Out,
            vips_hist_local_start,
            vips_hist_local_generate,
            vips_hist_local_stop,
            in_image, local))
        return -1;

    local.Out.Xoffset = 0;
    local.Out.Yoffset = 0;

    vips_reorder_margin_hint(local.Out, local.Width * local.Height);

    return 0;
}

// Converted from vips_hist_local_class_init()

public void ClassInit(VipsHistLocalClass class)
{
    VipsObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsOperationClass operation_class = (VipsOperationClass)class;

    gobject_class.SetProperty = vips_object_set_property;
    gobject_class.GetProperty = vips_object_get_property;

    operation_class.Nickname = "hist_local";
    operation_class.Description = _("local histogram equalisation");
    operation_class.Build = vips_hist_local_build;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsHistLocal, in));

    VIPS_ARG_IMAGE(class, "out", 2,
        _("Output"),
        _("Output image"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsHistLocal, out));

    VIPS_ARG_INT(class, "width", 4,
        _("Width"),
        _("Window width in pixels"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsHistLocal, width),
        1, VIPS_MAX_COORD, 1);

    VIPS_ARG_INT(class, "height", 5,
        _("Height"),
        _("Window height in pixels"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsHistLocal, height),
        1, VIPS_MAX_COORD, 1);

    VIPS_ARG_INT(class, "max_slope", 6,
        _("Max slope"),
        _("Maximum slope (CLAHE)"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsHistLocal, max_slope),
        0, 100, 0);
}

// Converted from vips_hist_local_init()

public void Init(VipsHistLocal local)
{
}

// Converted from vips_hist_local()

public int HistLocal(VipsImage in_image, VipsImage[] out_image, int width, int height, ...)
{
    va_list ap;
    int result;

    va_start(ap, height);
    result = vips_call_split("hist_local", ap, in_image, out_image, width, height);
    va_end(ap);

    return result;
}
```