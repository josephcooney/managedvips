```csharp
// Converted from: vips_xyz_gen

public static int VipsXyzGen(VipsRegion out_region, object seq, object a, object b, bool[] stop)
{
    VipsXyz xyz = (VipsXyz)a;
    VipsRect r = out_region.Valid;
    int le = r.Left;
    int to = r.Top;
    int ri = VIPS_RECT_RIGHT(r);
    int bo = VIPS_RECT_BOTTOM(r);

    for (int y = to; y < bo; y++)
    {
        uint[] q = (uint[])VIPS_REGION_ADDR(out_region, le, y);

        uint[] dims = new uint[5];
        int r;
        int h;

        h = xyz.Height * xyz.Csize * xyz.Dsize;
        dims[4] = y / h;
        r = y % h;

        h /= xyz.Dsize;
        dims[3] = r / h;
        r %= h;

        h /= xyz.Csize;
        dims[2] = r / h;
        r %= h;

        dims[1] = r;

        for (int x = le; x < ri; x++)
        {
            dims[0] = x;
            for (int i = 0; i < xyz.Dimensions; i++)
                q[i] = dims[i];

            q += xyz.Dimensions;
        }
    }

    return 0;
}

// Converted from: vips_xyz_build

public class VipsXyz : VipsObject
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Csize { get; set; }
    public int Dsize { get; set; }
    public int Esize { get; set; }
    public int Dimensions { get; set; }

    protected override bool Build()
    {
        VipsObjectClass class = VIPS_OBJECT_GET_CLASS(this);
        VipsCreate create = (VipsCreate)VipsObject.GetParent(this);
        VipsXyz xyz = this;

        double d;
        int ysize;

        if (base.Build())
            return false;

        if ((VipsObject.ArgumentIsSet(this, "Dsize") && !VipsObject.ArgumentIsSet(this, "Csize")) ||
            (VipsObject.ArgumentIsSet(this, "Esize") && !VipsObject.ArgumentIsSet(this, "Dsize")))
        {
            VipsError(class.Nickname, "%s", _("lower dimensions not set"));
            return false;
        }

        if (VipsObject.ArgumentIsSet(this, "Csize"))
        {
            xyz.Dimensions += 1;

            if (VipsObject.ArgumentIsSet(this, "Dsize"))
            {
                xyz.Dimensions += 1;

                if (VipsObject.ArgumentIsSet(this, "Esize"))
                    xyz.Dimensions += 1;
            }
        }

        d = (double)xyz.Height * xyz.Csize * xyz.Dsize * xyz.Esize;
        if (d > int.MaxValue)
        {
            VipsError(class.Nickname, "%s", _("image too large"));
            return false;
        }
        ysize = (int)d;

        VipsImage.InitFields(create.Out,
            xyz.Width, ysize, xyz.Dimensions,
            VIPS_FORMAT_UINT, VIPS_CODING_NONE,
            VIPS_INTERPRETATION_MULTIBAND,
            1.0, 1.0);
        if (VipsImage.Pipelinev(create.Out, VIPS_DEMAND_STYLE_ANY, null) ||
            VipsImage.Generate(create.Out,
                null, VipsXyzGen, null, xyz, null))
            return false;

        return true;
    }
}

// Converted from: vips_xyz_class_init

public class VipsXyzClass : VipsObjectClass
{
    public static void ClassInit(VipsXyzClass* class)
    {
        GObjectClass gobject_class = G_OBJECT_CLASS(class);
        VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(class);

        VIPS_DEBUG_MSG("vips_xyz_class_init\n");

        gobject_class.SetProperty = VipsObject.SetProperty;
        gobject_class.GetProperty = VipsObject.GetProperty;

        vobject_class.Nickname = "xyz";
        vobject_class.Description =
            _("make an image where pixel values are coordinates");
        vobject_class.Build = VipsXyz.Build;

        VIPS_ARG_INT(class, "width", 4,
            _("Width"),
            _("Image width in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsXyz, Width),
            1, VIPS_MAX_COORD, 64);

        VIPS_ARG_INT(class, "height", 5,
            _("Height"),
            _("Image height in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsXyz, Height),
            1, VIPS_MAX_COORD, 64);

        VIPS_ARG_INT(class, "csize", 6,
            _("Csize"),
            _("Size of third dimension"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsXyz, Csize),
            1, VIPS_MAX_COORD, 1);

        VIPS_ARG_INT(class, "dsize", 7,
            _("Dsize"),
            _("Size of fourth dimension"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsXyz, Dsize),
            1, VIPS_MAX_COORD, 1);

        VIPS_ARG_INT(class, "esize", 8,
            _("Esize"),
            _("Size of fifth dimension"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsXyz, Esize),
            1, VIPS_MAX_COORD, 1);
    }
}

// Converted from: vips_xyz_init

public class VipsXyz : VipsObject
{
    public int Width { get; set; } = 64;
    public int Height { get; set; } = 64;
    public int Dimensions { get; set; } = 2;
    public int Csize { get; set; } = 1;
    public int Dsize { get; set; } = 1;
    public int Esize { get; set; } = 1;

    protected override void Init()
    {
        Width = 64;
        Height = 64;
        Dimensions = 2;
        Csize = 1;
        Dsize = 1;
        Esize = 1;
    }
}

// Converted from: vips_xyz

public class VipsXyz : VipsObject
{
    public static int Create(VipsImage[] out, int width, int height, params object[] args)
    {
        var result = VipsCallSplit("xyz", args, out, width, height);
        return result;
    }
}
```