```csharp
// vips_sdf_circle
public float VipsSdfCircle(VipsSdf sdf, int x, int y)
{
    return (float)Math.Sqrt(Math.Pow(x - sdf.A[0], 2) + Math.Pow(y - sdf.A[1], 2)) - sdf.R;
}

// vips_sdf_box
public float VipsSdfBox(VipsSdf sdf, int x, int y)
{
    float px = x - sdf.Cx;
    float py = y - sdf.Cy;

    float dx = Math.Abs(px) - sdf.Sx;
    float dy = Math.Abs(py) - sdf.Sy;

    return (float)Math.Sqrt(Math.Pow(Vips.Max(dx, 0), 2) + Math.Pow(Vips.Max(dy, 0), 2)) +
        Vips.Min(Vips.Max(dx, dy), 0);
}

// vips_sdf_rounded_box
public float VipsSdfRoundedBox(VipsSdf sdf, int x, int y)
{
    float px = x - sdf.Cx;
    float py = y - sdf.Cy;

    // radius of nearest corner
    float r_top = px > 0 ? sdf.Corners[0] : sdf.Corners[2];
    float r_bottom = px > 0 ? sdf.Corners[1] : sdf.Corners[3];
    float r = py > 0 ? r_top : r_bottom;

    float qx = Math.Abs(px) - sdf.Sx + r;
    float qy = Math.Abs(py) - sdf.Sy + r;

    return (float)Math.Sqrt(Math.Pow(Vips.Max(qx, 0), 2) + Math.Pow(Vips.Max(qy, 0), 2)) +
        Vips.Min(Vips.Max(qx, qy), 0) - r;
}

// vips_sdf_line
public float VipsSdfLine(VipsSdf sdf, int px, int py)
{
    float pax = px - sdf.A[0];
    float pay = py - sdf.A[1];

    float dot_paba = pax * sdf.Dx + pay * sdf.Dy;
    float dot_baba = Math.Pow(sdf.Dx, 2) + Math.Pow(sdf.Dy, 2);
    float h = Vips.Clip(0, dot_paba / dot_baba, 1);

    float dx = pax - h * sdf.Dx;
    float dy = pay - h * sdf.Dy;

    return (float)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
}

// vips_sdf_gen
public int VipsSdfGen(VipsRegion out_region, object seq, object a, object b, bool[] stop)
{
    VipsSdf sdf = (VipsSdf)a;
    VipsRect r = out_region.Valid;

    for (int y = 0; y < r.Height; y++)
    {
        int ay = y + r.Top;
        float[] q = (float[])Vips.RegionAddr(out_region, r.Left, ay);

        for (int x = 0; x < r.Width; x++)
            q[x] = sdf.Point(sdf, x + r.Left, ay);
    }

    return 0;
}

// vips_sdf_build
public int VipsSdfBuild(VipsObject object)
{
    VipsObjectClass class = (VipsObjectClass)Vips.ObjectGetClass(object);
    VipsCreate create = (VipsCreate)Vips.Create(object);
    VipsSdf sdf = (VipsSdf)object;

    if (Vips.ObjectClass(vips_sdf_parent_class).Build(object) != 0)
        return -1;

    switch (sdf.Shape)
    {
        case VIPS_SDF_SHAPE_CIRCLE:
            if (!Vips.ObjectArgumentIsSet(object, "a") || !Vips.ObjectArgumentIsSet(object, "r"))
            {
                Vips.Error(class.Nickname, "%s", _("circle needs a, r to be set"));
                return -1;
            }
            if (sdf.AArea.N != 2)
            {
                Vips.Error(class.Nickname, "%s", _("rounded-box needs 2 values for a"));
                return -1;
            }

            sdf.A = (double[])sdf.AArea.Data;
            sdf.Point = VipsSdfCircle;

            break;

        case VIPS_SDF_SHAPE_BOX:
            if (!Vips.ObjectArgumentIsSet(object, "a") || !Vips.ObjectArgumentIsSet(object, "b"))
            {
                Vips.Error(class.Nickname, "%s", _("box needs a, b to be set"));
                return -1;
            }
            if (sdf.AArea.N != 2 || sdf.BArea.N != 2)
            {
                Vips.Error(class.Nickname, "%s", _("box needs 2 values for a, b"));
                return -1;
            }

            sdf.A = (double[])sdf.AArea.Data;
            sdf.B = (double[])sdf.BArea.Data;
            sdf.Point = VipsSdfBox;

            break;

        case VIPS_SDF_SHAPE_ROUNDED_BOX:
            if (!Vips.ObjectArgumentIsSet(object, "a") || !Vips.ObjectArgumentIsSet(object, "b"))
            {
                Vips.Error(class.Nickname, "%s", _("rounded-box needs a, b to be set"));
                return -1;
            }
            if (sdf.AArea.N != 2 || sdf.BArea.N != 2)
            {
                Vips.Error(class.Nickname, "%s", _("rounded-box needs 2 values for a, b"));
                return -1;
            }
            if (sdf.CornersArea.N != 4)
            {
                Vips.Error(class.Nickname, "%s", _("rounded-box needs 4 values for corners"));
                return -1;
            }

            sdf.A = (double[])sdf.AArea.Data;
            sdf.B = (double[])sdf.BArea.Data;
            sdf.Corners = (double[])sdf.CornersArea.Data;
            sdf.Point = VipsSdfRoundedBox;

            break;

        case VIPS_SDF_SHAPE_LINE:
            if (!Vips.ObjectArgumentIsSet(object, "a") || !Vips.ObjectArgumentIsSet(object, "b"))
            {
                Vips.Error(class.Nickname, "%s", _("line needs sx, sy to be set"));
                return -1;
            }
            if (sdf.AArea.N != 2 || sdf.BArea.N != 2)
            {
                Vips.Error(class.Nickname, "%s", _("line needs 2 values for a, b"));
                return -1;
            }

            sdf.A = (double[])sdf.AArea.Data;
            sdf.B = (double[])sdf.BArea.Data;
            sdf.Point = VipsSdfLine;

            break;

        default:
            Vips.Error(class.Nickname, _("unknown SDF %d"), sdf.Shape);
            return -1;
    }

    if (sdf.A != null && sdf.B != null)
    {
        // centre
        sdf.Cx = (sdf.A[0] + sdf.B[0]) / 2.0;
        sdf.Cy = (sdf.A[1] + sdf.B[1]) / 2.0;

        // difference
        sdf.Dx = sdf.B[0] - sdf.A[0];
        sdf.Dy = sdf.B[1] - sdf.A[1];

        // half size
        sdf.Sx = sdf.Dx / 2.0;
        sdf.Sy = sdf.Dy / 2.0;
    }

    Vips.ImageInitFields(create.Out, sdf.Width, sdf.Height, 1,
        VIPS_FORMAT_FLOAT, VIPS_CODING_NONE, VIPS_INTERPRETATION_B_W, 1.0, 1.0);
    if (Vips.ImagePipelinev(create.Out, VIPS_DEMAND_STYLE_ANY, null) != 0 ||
        Vips.ImageGenerate(create.Out, null, VipsSdfGen, null, sdf, null) != 0)
        return -1;

    return 0;
}

// vips_sdf_class_init
public void VipsSdfClassInit(VipsSdfClass class)
{
    GObjectClass gobject_class = (GObjectClass)class;
    VipsObjectClass vobject_class = (VipsObjectClass)class;

    VIPS_DEBUG_MSG("vips_sdf_class_init\n");

    gobject_class.SetProperty = Vips.ObjectSetProperty;
    gobject_class.GetProperty = Vips.ObjectGetProperty;

    vobject_class.Nickname = "sdf";
    vobject_class.Description = _("create an SDF image");
    vobject_class.Build = VipsSdfBuild;

    VIPS_ARG_INT(class, "width", 2,
        _("Width"),
        _("Image width in pixels"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsSdf, Width),
        1, VIPS_MAX_COORD, 1);

    VIPS_ARG_INT(class, "height", 3,
        _("Height"),
        _("Image height in pixels"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsSdf, Height),
        1, VIPS_MAX_COORD, 1);

    VIPS_ARG_ENUM(class, "shape", 8,
        _("Shape"),
        _("SDF shape to create"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsSdf, Shape),
        VIPS_TYPE_SDF_SHAPE, VIPS_SDF_SHAPE_CIRCLE);

    VIPS_ARG_DOUBLE(class, "r", 9,
        _("r"),
        _("Radius"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsSdf, R),
        0.0, VIPS_MAX_COORD, 50);

    VIPS_ARG_BOXED(class, "a", 13,
        _("a"),
        _("Point a"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsSdf, AArea),
        VIPS_TYPE_ARRAY_DOUBLE);

    VIPS_ARG_BOXED(class, "b", 14,
        _("b"),
        _("Point b"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsSdf, BArea),
        VIPS_TYPE_ARRAY_DOUBLE);

    VIPS_ARG_BOXED(class, "corners", 15,
        _("corners"),
        _("Corner radii"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsSdf, CornersArea),
        VIPS_TYPE_ARRAY_DOUBLE);
}

// vips_sdf_init
public void VipsSdfInit(VipsSdf sdf)
{
    sdf.CornersArea = Vips.AreaNewArray(G_TYPE_DOUBLE, sizeof(double), 4);
}

// vips_sdf
public int VipsSdf(VipsImage[] out, int width, int height, VipsSdfShape shape, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, shape);
    result = Vips.CallSplit("sdf", ap, out, width, height, shape);
    va_end(ap);

    return result;
}
```