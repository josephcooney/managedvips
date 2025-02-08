```csharp
// vips_bandfold_gen (from copy.c)

public int VipsBandfoldGen(VipsRegion region, object seq, object a, object b, bool stop)
{
    VipsBandfold bandfold = (VipsBandfold)b;
    VipsRegion ir = (VipsRegion)seq;
    VipsImage out = region.Image;
    VipsRect r = &region.Valid;
    int psize = VipsPel.SizeOf(out);

    VipsRect need;
    int y;

    need.Left = r.Left * bandfold.Factor;
    need.Top = r.Top;
    need.Width = r.Width * bandfold.Factor;
    need.Height = r.Height;
    if (VipsRegion.Prepare(ir, ref need))
        return -1;

    for (y = 0; y < r.Height; y++)
    {
        VipsPel[] p = VipsRegion.Addr(ir, r.Left * bandfold.Factor, r.Top + y);
        VipsPel[] q = VipsRegion.Addr(region, r.Left, r.Top + y);

        // We can't use VipsRegion.Region() since we change pixel coordinates.
        Array.Copy(p, 0, q, 0, psize * r.Width);
    }

    return 0;
}

// vips_bandfold_build (from copy.c)

public int VipsBandfoldBuild(VipsObject object)
{
    VipsObjectClass class = VipsObject.GetClass(object);
    VipsConversion conversion = VipsConversion.GetObject(object);
    VipsBandfold bandfold = (VipsBandfold)object;

    if (VipsObjectClass.VipsBandfoldParentClass.Build(object) != 0)
        return -1;

    if (VipsImage.PioInput(bandfold.In))
        return -1;

    if (bandfold.Factor == 0)
        bandfold.Factor = bandfold.In.Xsize;
    if (bandfold.In.Xsize % bandfold.Factor != 0)
    {
        VipsError.Class(class.Nickname, "%s", _("Factor must be a factor of image width"));
        return -1;
    }

    if (VipsImage.Pipelinev(conversion.Out, VipsDemandStyle.ThinStrip, bandfold.In, null) != 0)
        return -1;

    conversion.Out.Xsize /= bandfold.Factor;
    conversion.Out.Bands *= bandfold.Factor;

    if (VipsImage.Generate(conversion.Out, VipsStartOne, VipsBandfoldGen, VipsStopOne, bandfold.In, bandfold) != 0)
        return -1;

    return 0;
}

// vips_bandfold_class_init (from copy.c)

public void VipsBandfoldClassInit(VipsBandfoldClass class)
{
    GObjectClass gobjectClass = GObject.GetClass(class);
    VipsObjectClass vobjectClass = VipsObject.GetClass(class);
    VipsOperationClass operationClass = VipsOperation.GetClass(class);

    VIPS_DEBUG_MSG("VipsBandfoldClassInit\n");

    gobjectClass.SetProperty = VipsObject.SetProperty;
    gobjectClass.GetProperty = VipsObject.GetProperty;

    vobjectClass.Nickname = "bandfold";
    vobjectClass.Description = _("Fold up x axis into bands");
    vobjectClass.Build = VipsBandfoldBuild;

    operationClass.Flags = VipsOperation.Sequential;

    VIPS_ARG_IMAGE(class, "in", 1, _("Input"), _("Input image"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsBandfold, in));
    VIPS_ARG_INT(class, "factor", 11, _("Factor"), _("Fold by this factor"), VIPS_ARGUMENT_OPTIONAL_INPUT, G_STRUCT_OFFSET(VipsBandfold, factor), 0, 10000000, 0);
}

// vips_bandfold_init (from copy.c)

public void VipsBandfoldInit(VipsBandfold bandfold)
{
    // 0 means fold by width, see above.
    bandfold.Factor = 0;
}

// vips_bandfold (from copy.c)

public int VipsBandfold(VipsImage in, ref VipsImage out, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, args);
    result = VipsCallSplit("bandfold", ap, in, ref out);
    va_end(ap);

    return result;
}
```