```csharp
// vips_replicate_gen
public static int VipsReplicateGen(VipsRegion out_region, object seq, object a, object b, bool[] stop)
{
    VipsRegion ir = (VipsRegion)seq;
    VipsImage in = (VipsImage)a;
    VipsRect r = out_region.Valid;
    int twidth = in.Xsize;
    int theight = in.Ysize;

    int x, y;
    VipsRect tile;

    // Find top left of tiles we need.
    int xs = (r.Left / twidth) * twidth;
    int ys = (r.Top / theight) * theight;

    // The tile enclosing the top-left corner of the requested area.
    tile.Left = xs;
    tile.Top = ys;
    tile.Width = twidth;
    tile.Height = theight;

    // If the request fits inside a single tile, we can just pointer-copy.
    if (VipsRect.IncludesRect(tile, r))
    {
        VipsRect irect;

        // Translate request to input space.
        irect = r;
        irect.Left -= xs;
        irect.Top -= ys;
        if (VipsRegion.Prepare(ir, ref irect))
            return -1;

        if (VipsRegion.Region(out_region, ir, r, irect.Left, irect.Top))
            return -1;

        return 0;
    }

    for (y = ys; y < VipsRect.Bottom(r); y += theight)
        for (x = xs; x < VipsRect.Right(r); x += twidth)
        {
            VipsRect paint;

            // Whole tile at x, y
            tile.Left = x;
            tile.Top = y;
            tile.Width = twidth;
            tile.Height = theight;

            // Which parts touch the area of the output we are building.
            VipsRect.IntersectRect(tile, r, ref paint);

            // Translate back to ir coordinates.
            paint.Left -= x;
            paint.Top -= y;

            g_assert(!VipsRect.IsEmpty(paint));

            // Render into out_region.
            if (VipsRegion.PrepareTo(ir, out_region, ref paint, paint.Left + x, paint.Top + y))
                return -1;
        }

    return 0;
}

// vips_replicate_build
public static int VipsReplicateBuild(VipsObject object)
{
    VipsConversion conversion = (VipsConversion)object;
    VipsReplicate replicate = (VipsReplicate)object;

    if (((VipsConversionClass)vips_replicate_parent_class).build(object))
        return -1;

    if (vips_image_pio_input(replicate.in))
        return -1;

    if (vips_image_pipelinev(conversion.out, VIPS_DEMAND_STYLE_SMALLTILE, replicate.in, null))
        return -1;

    conversion.out.Xsize *= replicate.across;
    conversion.out.Ysize *= replicate.down;

    if (vips_image_generate(conversion.out, vips_start_one, VipsReplicateGen, vips_stop_one, replicate.in, replicate))
        return -1;

    return 0;
}

// vips_replicate_class_init
public class VipsReplicateClass : VipsConversionClass
{
    public static void VipsReplicateClassInit(VipsReplicateClass* class)
    {
        GObjectClass gobject_class = (GObjectClass)class;
        VipsObjectClass vobject_class = (VipsObjectClass)class;

        VIPS_DEBUG_MSG("vips_replicate_class_init\n");

        gobject_class.set_property = vips_object_set_property;
        gobject_class.get_property = vips_object_get_property;

        vobject_class.nickname = "replicate";
        vobject_class.description = _("replicate an image");
        vobject_class.build = VipsReplicateBuild;

        VIPS_ARG_IMAGE(class, "in", 0,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsReplicate, in));

        VIPS_ARG_INT(class, "across", 4,
            _("Across"),
            _("Repeat this many times horizontally"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsReplicate, across),
            1, 1000000, 1);

        VIPS_ARG_INT(class, "down", 5,
            _("Down"),
            _("Repeat this many times vertically"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsReplicate, down),
            1, 1000000, 1);
    }
}

// vips_replicate_init
public class VipsReplicate : VipsConversion
{
    public VipsReplicate()
    {
    }
}

// vips_replicate
public static int VipsReplicate(VipsImage in, ref VipsImage out, int across, int down)
{
    var args = new object[] { null };
    return VipsCallSplit("replicate", args, in, ref out, across, down);
}
```