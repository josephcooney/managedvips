```csharp
// vips_black_gen (from black.c)

public int VipsBlackGen(VipsRegion out_region, object seq, object a, object b, bool stop)
{
    // VipsRect *r = &out_region->valid;

    VipsRegion.Black(out_region);

    return 0;
}

// vips_black_build (from black.c)

public int VipsBlackBuild(VipsObject obj)
{
    VipsCreate create = (VipsCreate)obj;
    VipsBlack black = (VipsBlack)obj;

    if (VIPS_OBJECT_CLASS(vips_black_parent_class).build(obj) != 0)
        return -1;

    VipsImage.InitFields(create.Out,
                         black.Width, black.Height, black.Bands,
                         VipsFormat.UChar, VipsCoding.None,
                         VipsInterpretation.Multiband,
                         1.0, 1.0);
    if (VipsImage.Pipelinev(create.Out,
                            VipsDemandStyle.Any, null) != 0)
        return -1;

    if (VipsImage.Generate(create.Out,
                           null, vips_black_gen, null, null, null) != 0)
        return -1;

    return 0;
}

// vips_black_class_init (from black.c)

public class VipsBlackClass : VipsObjectClass
{
    public VipsBlackClass()
    {
        // GObjectClass *gobject_class = G_OBJECT_CLASS(class);
        // VipsObjectClass *vobject_class = VIPS_OBJECT_CLASS(class);

        // gobject_class->set_property = vips_object_set_property;
        // gobject_class->get_property = vips_object_get_property;

        // vobject_class->nickname = "black";
        // vobject_class->description = _("make a black image");
        // vobject_class->build = vips_black_build;

        // VIPS_ARG_INT(class, "width", 4,
        //     _("Width"),
        //     _("Image width in pixels"),
        //     VIPS_ARGUMENT_REQUIRED_INPUT,
        //     G_STRUCT_OFFSET(VipsBlack, width),
        //     1, VIPS_MAX_COORD, 1);

        // VIPS_ARG_INT(class, "height", 5,
        //     _("Height"),
        //     _("Image height in pixels"),
        //     VIPS_ARGUMENT_REQUIRED_INPUT,
        //     G_STRUCT_OFFSET(VipsBlack, height),
        //     1, VIPS_MAX_COORD, 1);

        // VIPS_ARG_INT(class, "bands", 6,
        //     _("Bands"),
        //     _("Number of bands in image"),
        //     VIPS_ARGUMENT_OPTIONAL_INPUT,
        //     G_STRUCT_OFFSET(VipsBlack, bands),
        //     1, VIPS_MAX_COORD, 1);
    }
}

// vips_black_init (from black.c)

public void VipsBlackInit(VipsBlack black)
{
    black.Bands = 1;
}

// vips_black (from black.c)

public int VipsBlack(VipsImage out, int width, int height, params object[] args)
{
    var result = VipsCallSplit("black", args, ref out, width, height);

    return result;
}
```