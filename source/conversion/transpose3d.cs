```csharp
// vips_transpose3d_gen
static int vips_transpose3d_gen(VipsRegion out_region, VipsImage in_image, VipsTranspose3d transpose3d)
{
    VipsRect r = out_region.Valid;
    int output_page_height = in_image.Ysize / transpose3d.page_height;

    int y;
    VipsRect tile;

    tile = r;
    tile.Height = 1;

    for (y = 0; y < r.Height; y++)
    {
        // y in output.
        int yo = r.Top + y;

        // On output page.
        int yop = yo / output_page_height;

        // Line on output page.
        int yol = yo % output_page_height;

        // y of input page.
        int yip = yol * transpose3d.page_height;

        // y of input line.
        int yi = yip + yop;

        tile.Top = yi;

        // Render into out_region.
        if (vips_region_prepare_to(out_region, r, &tile, tile.Left, yo))
            return -1;
    }

    return 0;
}

// vips_transpose3d_build
static int vips_transpose3d_build(VipsObject object)
{
    VipsObjectClass class = VIPS_OBJECT_GET_CLASS(object);
    VipsConversion conversion = VIPS_CONVERSION(object);
    VipsTranspose3d transpose3d = (VipsTranspose3d)object;

    VipsImage in_image;

    if (VIPS_OBJECT_CLASS(vips_transpose3d_parent_class)->build(object))
        return -1;

    in_image = transpose3d.in;

    if (vips_check_coding_known(class.Nickname, in_image) || vips_image_pio_input(in_image))
        return -1;

    if (!vips_object_argument_isset(object, "page_height"))
    {
        if (vips_image_get_int(in_image, VIPS_META_PAGE_HEIGHT, &transpose3d.page_height))
            return -1;
    }

    if (transpose3d.page_height <= 0 || in_image.Ysize % transpose3d.page_height != 0)
    {
        vips_error(class.Nickname, "%s", _("bad page_height"));
        return -1;
    }

    if (vips_image_pipelinev(conversion.Out, VIPS_DEMAND_STYLE_SMALLTILE, in_image, null))
        return -1;

    vips_image_set_int(conversion.Out, VIPS_META_PAGE_HEIGHT, in_image.Ysize / transpose3d.page_height);

    if (vips_image_generate(conversion.Out, vips_start_one, vips_transpose3d_gen, vips_stop_one, in_image, transpose3d))
        return -1;

    return 0;
}

// vips_transpose3d_class_init
static void vips_transpose3d_class_init(VipsTranspose3dClass class)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(class);

    gobject_class.SetProperty = vips_object_set_property;
    gobject_class.GetProperty = vips_object_get_property;

    vobject_class.Nickname = "transpose3d";
    vobject_class.Description = _("transpose3d an image");
    vobject_class.Build = vips_transpose3d_build;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsTranspose3d, in));

    VIPS_ARG_INT(class, "page_height", 3,
        _("Page height"),
        _("Height of each input page"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsTranspose3d, page_height),
        0, 10000000, 0);
}

// vips_transpose3d_init
static void vips_transpose3d_init(VipsTranspose3d transpose3d)
{
}

// vips_transpose3d
public static int VipsTranspose3d(VipsImage in_image, ref VipsImage out_image, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, args);
    result = vips_call_split("transpose3d", ap, in_image, ref out_image);
    va_end(ap);

    return result;
}
```