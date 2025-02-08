```csharp
// Converted from: vips_mapim_region_minmax()

void VipsMapimRegionMinmax(VipsRegion region, VipsRect r, ref VipsRect bounds)
{
    double min_x = 0.0;
    double max_x = 0.0;
    double min_y = 0.0;
    double max_y = 0.0;
    bool first = true;

    for (int y = 0; y < r.Height; y++)
    {
        VipsPel* p = VIPS_REGION_ADDR(region, r.Left, r.Top + y);

        switch (region.Im.BandFmt)
        {
            case VipsFormat.UChar:
                MinmaxUnsignedChar(p);
                break;
            case VipsFormat.Char:
                MinmaxSignedChar(p);
                break;
            case VipsFormat.UShort:
                MinmaxUnsignedShort(p);
                break;
            case VipsFormat.Short:
                MinmaxSignedShort(p);
                break;
            case VipsFormat.UInt:
                MinmaxUnsignedInt(p);
                break;
            case VipsFormat.Int:
                MinmaxSignedInt(p);
                break;
            case VipsFormat.Float:
            case VipsFormat.Complex:
                MinmaxFloat(p);
                break;
            case VipsFormat.Double:
            case VipsFormat.DComplex:
                MinmaxDouble(p);
                break;

            default:
                g_assert_not_reached();
        }
    }

    // bounds is the bounding box -- we must round left/top down and round
    // bottom/right up.
    min_x = Math.Floor(min_x);
    min_y = Math.Floor(min_y);
    max_x = Math.Ceiling(max_x);
    max_y = Math.Ceiling(max_y);

    // bounds uses ints, so we must clip the range down from double.
    // Coordinates can be negative for the antialias edges.
    min_x = VipsClip(-1, min_x, VipsMaxCoord);
    min_y = VipsClip(-1, min_y, VipsMaxCoord);
    max_x = VipsClip(-1, max_x, VipsMaxCoord);
    max_y = VipsClip(-1, max_y, VipsMaxCoord);

    bounds.Left = (int)min_x;
    bounds.Top = (int)min_y;
    bounds.Width = (int)(max_x - min_x + 1);
    bounds.Height = (int)(max_y - min_y + 1);
}

// Converted from: vips_mapim_gen()

int VipsMapimGen(VipsRegion out_region, object seq, object a, object b, ref bool stop)
{
    VipsRect r = out_region.Valid;
    VipsRegion[] ir = (VipsRegion[])seq;
    VipsImage[] in_array = (VipsImage[])a;
    VipsMapim mapim = (VipsMapim)b;
    VipsImage in = in_array[0];
    int window_size = vips_interpolate_get_window_size(mapim.Interpolate);
    int window_offset = vips_interpolate_get_window_offset(mapim.Interpolate);
    VipsInterpolateMethod interpolate = vips_interpolate_get_method(mapim.Interpolate);
    int ps = VipsImage.SizeOfPel(in);
    int clip_width = in.Xsize - window_size;
    int clip_height = in.Ysize - window_size;

    VipsRect bounds, need, image, clipped;
    int x, y, z;

#ifdef DEBUG_VERBOSE
    Console.WriteLine("vips_mapim_gen: generating left={0}, top={1}, width={2}, height={3}",
        r.Left,
        r.Top,
        r.Width,
        r.Height);
#endif /*DEBUG_VERBOSE*/

    // Fetch the chunk of the index image we need, and find the max/min in
    // x and y.
    if (vips_region_prepare(ir[1], r) != 0)
        return -1;

    VIPS_GATE_START("vips_mapim_gen: work");

    vips_mapim_region_minmax(ir[1], r, ref bounds);

    VIPS_GATE_STOP("vips_mapim_gen: work");

    // Enlarge by the stencil size.
    need.Width = bounds.Width + window_size - 1;
    need.Height = bounds.Height + window_size - 1;

    // Offset for the antialias edge we have top and left.
    need.Left = bounds.Left + 1;
    need.Top = bounds.Top + 1;

    // Clip against the expanded image.
    image.Left = 0;
    image.Top = 0;
    image.Width = in.Xsize;
    image.Height = in.Ysize;
    vips_rect_intersectrect(ref need, ref image, ref clipped);

#ifdef DEBUG_VERBOSE
    Console.WriteLine("vips_mapim_gen: preparing left={0}, top={1}, width={2}, height={3}",
        clipped.Left,
        clipped.Top,
        clipped.Width,
        clipped.Height);
#endif /*DEBUG_VERBOSE*/

    if (vips_rect_isempty(ref clipped) != 0)
    {
        vips_region_paint_pel(out_region, r, mapim.Ink);
        return 0;
    }
    if (vips_region_prepare(ir[0], ref clipped) != 0)
        return -1;

    VIPS_GATE_START("vips_mapim_gen: work");

    // Resample! x/y loop over pixels in the output (and index) images.
    for (int y = 0; y < r.Height; y++)
    {
        VipsPel* p = VIPS_REGION_ADDR(ir[1], r.Left, r.Top + y);
        VipsPel* q = VIPS_REGION_ADDR(out_region, r.Left, r.Top + y);

        switch (ir[1].Im.BandFmt)
        {
            case VipsFormat.UChar:
                UlookupUnsignedChar(p);
                break;
            case VipsFormat.Char:
                LookupSignedChar(p);
                break;
            case VipsFormat.UShort:
                UlookupUnsignedShort(p);
                break;
            case VipsFormat.Short:
                LookupSignedShort(p);
                break;
            case VipsFormat.UInt:
                UlookupUnsignedInt(p);
                break;
            case VipsFormat.Int:
                LookupSignedInt(p);
                break;
            case VipsFormat.Float:
            case VipsFormat.Complex:
                FlookupFloat(p);
                break;
            case VipsFormat.Double:
            case VipsFormat.DComplex:
                FlookupDouble(p);
                break;

            default:
                g_assert_not_reached();
        }
    }

    VIPS_GATE_STOP("vips_mapim_gen: work");

    return 0;
}

// Converted from: vips_mapim_build()

int VipsMapimBuild(VipsObject object)
{
    VipsObjectClass class = VIPS_OBJECT_GET_CLASS(object);
    VipsResample resample = (VipsResample)object;
    VipsMapim mapim = (VipsMapim)object;
    VipsImage[] t = new VipsImage[6];

    // TRUE if we've premultiplied and need to unpremultiply.
    bool have_premultiplied;
    VipsBandFormat unpremultiplied_format;

    if (VIPS_OBJECT_CLASS(vips_mapim_parent_class).Build(object) != 0)
        return -1;

    if (vips_check_coding_known(class.Nickname, resample.In) ||
        vips_check_twocomponents(class.Nickname, mapim.Index))
        return -1;

    VipsImage in = resample.In;

    if (vips_image_decode(in, ref t[0]) != 0)
        return -1;
    in = t[0];

    int window_size = vips_interpolate_get_window_size(mapim.Interpolate);
    int window_offset = vips_interpolate_get_window_offset(mapim.Interpolate);

    // Add new pixels around the input so we can interpolate at the edges.
    //
    // We add the interpolate stencil, plus one extra pixel on all the
    // edges. This means when we clip in generate (above) we can be sure
    // we clip outside the real pixels and don't get jaggies on edges.
    //
    // We allow for the +1 in the adjustment to window_offset in generate.
    if (vips_embed(in, ref t[1],
        window_offset + 1, window_offset + 1,
        in.Xsize + window_size - 1 + 2,
        in.Ysize + window_size - 1 + 2,
        "extend", mapim.Extend,
        "background", mapim.Background,
        null) != 0)
        return -1;
    in = t[1];

    // If there's an alpha and we've not premultiplied, we have to
    // premultiply before resampling.
    have_premultiplied = false;
    if (vips_image_hasalpha(in) && !mapim.Premultiplied)
    {
        if (vips_premultiply(in, ref t[2], null) != 0)
            return -1;
        have_premultiplied = true;

        // vips_premultiply() makes a float image. When we
        // vips_unpremultiply() below, we need to cast back to the
        // pre-premultiply format.
        unpremultiplied_format = in.BandFmt;
        in = t[2];
    }

    // Convert the background to the image's format.
    if ((mapim.Ink = vips__vector_to_ink(class.Nickname,
          in,
          VIPS_AREA(mapim.Background).Data, null,
          VIPS_AREA(mapim.Background).N)) == null)
        return -1;

    t[3] = vips_image_new();
    if (vips_image_pipelinev(t[3], VipsDemandStyle.SmallTile,
        in, null) != 0)
        return -1;

    t[3].Xsize = mapim.Index.Xsize;
    t[3].Ysize = mapim.Index.Ysize;

    mapim.InArray[0] = in;
    mapim.InArray[1] = mapim.Index;
    mapim.InArray[2] = null;
    if (vips_image_generate(t[3],
        vips_start_many, VipsMapimGen, vips_stop_many,
        mapim.InArray, mapim) != 0)
        return -1;

    in = t[3];

    if (have_premultiplied)
    {
        if (vips_unpremultiply(in, ref t[4], null) != 0 ||
            vips_cast(t[4], ref t[5], unpremultiplied_format, null) != 0)
            return -1;
        in = t[5];
    }

    if (vips_image_write(in, resample.Out) != 0)
        return -1;

    return 0;
}

// Converted from: vips_mapim_class_init()

void VipsMapimClassInit(VipsMapimClass* class)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(class);

    VIPS_DEBUG_MSG("vips_mapim_class_init\n");

    gobject_class.SetProperty = vips_object_set_property;
    gobject_class.GetProperty = vips_object_get_property;

    vobject_class.Nickname = "mapim";
    vobject_class.Description = _("resample with a map image");
    vobject_class.Build = VipsMapimBuild;

    VIPS_ARG_IMAGE(class, "index", 3,
        _("Index"),
        _("Index pixels with this"),
        VipsArgument.RequiredInput,
        G_STRUCT_OFFSET(VipsMapim, Index));

    VIPS_ARG_INTERPOLATE(class, "interpolate", 4,
        _("Interpolate"),
        _("Interpolate pixels with this"),
        VipsArgument.OptionalInput,
        G_STRUCT_OFFSET(VipsMapim, Interpolate));

    VIPS_ARG_ENUM(class, "extend", 117,
        _("Extend"),
        _("How to generate the extra pixels"),
        VipsArgument.OptionalInput,
        G_STRUCT_OFFSET(VipsMapim, Extend),
        VipsType.Extend, VipsExtend.Background);

    VIPS_ARG_BOXED(class, "background", 116,
        _("Background"),
        _("Background value"),
        VipsArgument.OptionalInput,
        G_STRUCT_OFFSET(VipsMapim, Background),
        VipsType.ArrayDouble);

    VIPS_ARG_BOOL(class, "premultiplied", 117,
        _("Premultiplied"),
        _("Images have premultiplied alpha"),
        VipsArgument.OptionalInput,
        G_STRUCT_OFFSET(VipsMapim, Premultiplied),
        false);
}

// Converted from: vips_mapim_init()

void VipsMapimInit(VipsMapim* mapim)
{
    mapim.Interpolate = vips_interpolate_new("bilinear");
    mapim.Extend = VipsExtend.Background;
    mapim.Background = vips_array_double_newv(1, 0.0);
}

// Converted from: vips_mapim()

int VipsMapim(VipsImage in, ref VipsImage out, VipsImage index, params object[] args)
{
    int result;

    result = vips_call_split("mapim", args, in, ref out, index);

    return result;
}

// Helper functions

void MinmaxUnsignedChar(VipsPel* p)
{
    unsigned char* restrict p1 = (unsigned char*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        unsigned char px = p1[0];
        unsigned char py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxSignedChar(VipsPel* p)
{
    signed char* restrict p1 = (signed char*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        signed char px = p1[0];
        signed char py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxUnsignedShort(VipsPel* p)
{
    unsigned short* restrict p1 = (unsigned short*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        unsigned short px = p1[0];
        unsigned short py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxSignedShort(VipsPel* p)
{
    signed short* restrict p1 = (signed short*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        signed short px = p1[0];
        signed short py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxUnsignedInt(VipsPel* p)
{
    unsigned int* restrict p1 = (unsigned int*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        unsigned int px = p1[0];
        unsigned int py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxSignedInt(VipsPel* p)
{
    signed int* restrict p1 = (signed int*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        signed int px = p1[0];
        signed int py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxFloat(VipsPel* p)
{
    float* restrict p1 = (float*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        float px = p1[0];
        float py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void MinmaxDouble(VipsPel* p)
{
    double* restrict p1 = (double*)p;

    double t_max_x = 0.0;
    double t_min_x = 0.0;
    double t_max_y = 0.0;
    double t_min_y = 0.0;

    for (int x = 0; x < r.Width; x++)
    {
        double px = p1[0];
        double py = p1[1];

        if (first)
        {
            t_min_x = px;
            t_max_x = px;
            t_min_y = py;
            t_max_y = py;

            first = false;
        }
        else
        {
            if (px > t_max_x)
                t_max_x = px;
            else if (px < t_min_x)
                t_min_x = px;

            if (py > t_max_y)
                t_max_y = py;
            else if (py < t_min_y)
                t_min_y = py;
        }

        p1 += 2;
    }

    min_x = t_min_x;
    max_x = t_max_x;
    min_y = t_min_y;
    max_y = t_max_y;
}

void UlookupUnsignedChar(VipsPel* p)
{
    unsigned char* restrict p1 = (unsigned char*)p;

    for (int x = 0; x < r.Width; x++)
    {
        unsigned char px = p1[0];
        unsigned char py = p1[1];

        if (px >= clip_width || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void LookupSignedChar(VipsPel* p)
{
    signed char* restrict p1 = (signed char*)p;

    for (int x = 0; x < r.Width; x++)
    {
        signed char px = p1[0];
        signed char py = p1[1];

        if (px < -1 || px >= clip_width || py < -1 || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void UlookupUnsignedShort(VipsPel* p)
{
    unsigned short* restrict p1 = (unsigned short*)p;

    for (int x = 0; x < r.Width; x++)
    {
        unsigned short px = p1[0];
        unsigned short py = p1[1];

        if (px >= clip_width || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void LookupSignedShort(VipsPel* p)
{
    signed short* restrict p1 = (signed short*)p;

    for (int x = 0; x < r.Width; x++)
    {
        signed short px = p1[0];
        signed short py = p1[1];

        if (px < -1 || px >= clip_width || py < -1 || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void UlookupUnsignedInt(VipsPel* p)
{
    unsigned int* restrict p1 = (unsigned int*)p;

    for (int x = 0; x < r.Width; x++)
    {
        unsigned int px = p1[0];
        unsigned int py = p1[1];

        if (px >= clip_width || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void LookupSignedInt(VipsPel* p)
{
    signed int* restrict p1 = (signed int*)p;

    for (int x = 0; x < r.Width; x++)
    {
        signed int px = p1[0];
        signed int py = p1[1];

        if (px < -1 || px >= clip_width || py < -1 || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void FlookupFloat(VipsPel* p)
{
    float* restrict p1 = (float*)p;

    for (int x = 0; x < r.Width; x++)
    {
        float px = p1[0];
        float py = p1[1];

        if (VIPS_ISNAN(px) || VIPS_ISNAN(py) || px < -1 || px >= clip_width || py < -1 || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}

void FlookupDouble(VipsPel* p)
{
    double* restrict p1 = (double*)p;

    for (int x = 0; x < r.Width; x++)
    {
        double px = p1[0];
        double py = p1[1];

        if (VIPS_ISNAN(px) || VIPS_ISNAN(py) || px < -1 || px >= clip_width || py < -1 || py >= clip_height)
        {
            for (int z = 0; z < ps; z++)
                q[z] = mapim.Ink[z];
        }
        else
            interpolate(mapim.Interpolate, q, ir[0], px + window_offset + 1, py + window_offset + 1);

        p1 += 2;
        q += ps;
    }
}
```