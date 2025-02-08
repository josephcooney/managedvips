```csharp
// vips_bandunfold_gen (from copy.c)
public static int VipsBandunfoldGen(VipsRegion region, object seq, object a, object b, bool stop)
{
    VipsBandunfold bandunfold = (VipsBandunfold)b;
    VipsRegion ir = (VipsRegion)seq;
    VipsImage in = ir.im;
    VipsImage out = region.im;
    VipsRect r = region.valid;
    int esize = VipsImage.SizeOfElement(in);
    int psize = VipsImage.SizeOfPel(out);

    VipsRect need;
    int y;

    need.left = r.left / bandunfold.factor;
    need.top = r.top;
    need.width = 1 + VipsRect.Right(r) / bandunfold.factor - need.left;
    need.height = r.height;
    if (VipsRegion.Prepare(ir, ref need))
        return -1;

    for (y = 0; y < r.height; y++)
    {
        VipsPel p = VipsRegion.Addr(ir, r.left / bandunfold.factor, r.top + y) +
            (r.left % bandunfold.factor) * esize;
        VipsPel q = VipsRegion.Addr(region, r.left, r.top + y);

        // We can't use vips_region_region() since we change pixel coordinates.
        System.Buffer.BlockCopy(q, 0, p, 0, r.width * psize);
    }

    return 0;
}

// vips_bandunfold_build (from copy.c)
public static int VipsBandunfoldBuild(VipsObject object)
{
    VipsObjectClass class = VipsObject.GetClass(object);
    VipsConversion conversion = VipsConversion.GetObject(object);
    VipsBandunfold bandunfold = (VipsBandunfold)object;

    if (VipsObjectClass.VipsBandunfoldParentClass.Build(object) != 0)
        return -1;

    if (VipsImage.PioInput(bandunfold.in))
        return -1;

    if (bandunfold.factor == 0)
        bandunfold.factor = bandunfold.in.Bands;
    if (bandunfold.in.Bands % bandunfold.factor != 0)
    {
        VipsError.Class(class.nickname, "%s", _("Factor must be a factor of image bands"));
        return -1;
    }

    if (!VipsImage.Pipelinev(conversion.out, VipsDemandStyle.ThinStrip, bandunfold.in, null))
        return -1;

    conversion.out.Xsize *= bandunfold.factor;
    conversion.out.Bands /= bandunfold.factor;

    if (VipsImage.Generate(conversion.out, VipsStart.One, VipsBandunfoldGen, VipsStop.One,
            bandunfold.in, bandunfold) != 0)
        return -1;

    return 0;
}

// vips_bandunfold_class_init (from copy.c)
public static void VipsBandunfoldClassInit(VipsBandunfoldClass class)
{
    GObjectClass gobjectClass = GObject.GetClass(class);
    VipsObjectClass vobjectClass = VipsObjectClass.GetClass(class);
    VipsOperationClass operationClass = VipsOperationClass.GetClass(class);

    VIPS_DEBUG_MSG("vips_bandunfold_class_init\n");

    gobjectClass.SetProperty = VipsObject.SetProperty;
    gobjectClass.GetProperty = VipsObject.GetProperty;

    vobjectClass.Nickname = "bandunfold";
    vobjectClass.Description = _("Unfold image bands into x axis");
    vobjectClass.Build = VipsBandunfoldBuild;

    operationClass.Flags = VipsOperation.Sequential;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsBandunfold, in));

    VIPS_ARG_INT(class, "factor", 11,
        _("Factor"),
        _("Unfold by this factor"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsBandunfold, factor),
        0, 10000000, 0);
}

// vips_bandunfold_init (from copy.c)
public void VipsBandunfoldInit(VipsBandunfold bandunfold)
{
    // 0 means unfold by width, see above.
    bandunfold.factor = 0;
}

// vips_bandunfold (from copy.c)
public static int VipsBandunfold(VipsImage in, ref VipsImage out, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, args);
    result = VipsCallSplit("bandunfold", ap, in, ref out);
    va_end(ap);

    return result;
}
```