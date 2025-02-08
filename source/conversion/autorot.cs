```csharp
// vips_autorot_remove_angle_sub
public static void VipsAutorotRemoveAngleSub(VipsImage image, string field, GValue value, object myData)
{
    if (field == "exif-ifd0-Orientation")
    {
        // printf("vips_autorot_remove_angle: %s\n", field);
        vips_image_remove(image, field);
    }
}

// vips_autorot_remove_angle
public static void VipsAutorotRemoveAngle(VipsImage image)
{
    vips_image_remove(image, VIPS_META_ORIENTATION);
    vips_image_map(image, VipsAutorotRemoveAngleSub, null);
}

// vips_autorot_build
public static int VipsAutorotBuild(VipsObject object)
{
    VipsConversion conversion = (VipsConversion)object;
    VipsAutorot autorot = (VipsAutorot)object;
    VipsImage[] t = new VipsImage[3];

    if (VIPS_OBJECT_CLASS(vips_autorot_parent_class).build(object) != 0)
        return -1;

    VipsAngle angle;
    bool flip;
    VipsImage in;

    in = autorot.in;

    switch (vips_image_get_orientation(in))
    {
        case 2:
            angle = VIPS_ANGLE_D0;
            flip = true;
            break;

        case 3:
            angle = VIPS_ANGLE_D180;
            flip = false;
            break;

        case 4:
            angle = VIPS_ANGLE_D180;
            flip = true;
            break;

        case 5:
            angle = VIPS_ANGLE_D90;
            flip = true;
            break;

        case 6:
            angle = VIPS_ANGLE_D90;
            flip = false;
            break;

        case 7:
            angle = VIPS_ANGLE_D270;
            flip = true;
            break;

        case 8:
            angle = VIPS_ANGLE_D270;
            flip = false;
            break;

        case 1:
        default:
            angle = VIPS_ANGLE_D0;
            flip = false;
            break;
    }

    g_object_set(object,
        "angle", angle,
        "flip", flip,
        null);

    if (angle != VIPS_ANGLE_D0)
    {
        if (vips_rot(in, ref t[0], angle, null) != 0)
            return -1;
        in = t[0];
    }

    if (flip)
    {
        if (vips_flip(in, ref t[1], VIPS_DIRECTION_HORIZONTAL, null) != 0)
            return -1;
        in = t[1];
    }

    // We must copy before modifying metadata.
    if (vips_copy(in, ref t[2], null) != 0)
        return -1;
    in = t[2];

    vips_autorot_remove_angle(in);

    if (vips_image_write(in, conversion.out) != 0)
        return -1;

    return 0;
}

// vips_autorot_class_init
public static void VipsAutorotClassInit(VipsAutorotClass class)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(class);

    gobject_class.set_property = vips_object_set_property;
    gobject_class.get_property = vips_object_get_property;

    vobject_class.nickname = "autorot";
    vobject_class.description = _("autorotate image by exif tag");
    vobject_class.build = VipsAutorotBuild;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsAutorot, in));

    VIPS_ARG_ENUM(class, "angle", 6,
        _("Angle"),
        _("Angle image was rotated by"),
        VIPS_ARGUMENT_OPTIONAL_OUTPUT,
        G_STRUCT_OFFSET(VipsAutorot, angle),
        VIPS_TYPE_ANGLE, VIPS_ANGLE_D0);

    VIPS_ARG_BOOL(class, "flip", 7,
        _("Flip"),
        _("Whether the image was flipped or not"),
        VIPS_ARGUMENT_OPTIONAL_OUTPUT,
        G_STRUCT_OFFSET(VipsAutorot, flip),
        false);
}

// vips_autorot_init
public static void VipsAutorotInit(VipsAutorot autorot)
{
    autorot.angle = VIPS_ANGLE_D0;
    autorot.flip = false;
}

// vips_autorot
public static int VipsAutorot(VipsImage in, ref VipsImage out, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, args);
    result = vips_call_split("autorot", ap, in, ref out);
    va_end(ap);

    return result;
}
```