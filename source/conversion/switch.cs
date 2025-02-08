```csharp
// vips_switch_gen
public static int VipsSwitchGen(VipsRegion out_region, object seq, object a, object b, bool stop)
{
    VipsRegion[] ar = (VipsRegion[])seq;
    VipsSwitch swit = (VipsSwitch)b;
    VipsRect r = out_region.Valid;

    int x, y, i;
    byte[] q;
    size_t qls;
    byte[][] p = new byte[256][];
    size_t[] ls = new size_t[256];

    if (!VipsReorderPrepareMany(out_region.Im, ar, r))
        return -1;

    G.Assert(ar[0].Im.BandFmt == VipsFormat.UChar);
    G.Assert(ar[0].Im.Bands == 1);

    for (i = 0; i < swit.N; i++)
    {
        p[i] = new byte[r.Width];
        ls[i] = ar[i].LSkip;
    }

    q = out_region.Address(r.Left, r.Top);
    qls = out_region.LSkip;
    for (y = 0; y < r.Height; y++)
    {
        for (x = 0; x < r.Width; x++)
        {
            for (i = 0; i < swit.N; i++)
                if (p[i][x] != 0)
                    break;

            q[x] = (byte)i;
        }

        q += qls;
        for (i = 0; i < swit.N; i++)
            p[i] = new byte[r.Width];
    }

    return 0;
}

// vips_switch_build
public static int VipsSwitchBuild(VipsObject object)
{
    VipsObjectClass class = VipsObject.GetClass(object);
    VipsSwitch swit = (VipsSwitch)object;

    VipsImage[] tests;
    VipsImage[] decode;
    VipsImage[] format;
    VipsImage[] band;
    VipsImage[] size;
    int i;

    object.SetProperty("out", new VipsImage());

    if (!VipsObjectClass.VipsSwitchParentClass.Build(object))
        return -1;

    // 255 rather than 256, since we want to reserve +1 as the no
    // match value.
    tests = (VipsImage[])swit.Tests.Area.GetData(null, ref swit.N, null, null);
    if (swit.N > 255 || swit.N < 1)
    {
        VipsError(class.Nickname, "%s", _("bad number of tests"));
        return -1;
    }

    decode = new VipsImage[swit.N];
    format = new VipsImage[swit.N];
    band = new VipsImage[swit.N + 1];
    size = new VipsImage[swit.N + 1];

    // Decode RAD/LABQ etc.
    for (i = 0; i < swit.N; i++)
        if (!VipsImage.Decode(tests[i], ref decode[i]))
            return -1;
    tests = decode;

    // Must be uchar.
    for (i = 0; i < swit.N; i++)
        if (!VipsCastUChar(tests[i], ref format[i], null))
            return -1;
    tests = format;

    // Images must match in size and bands.
    if (VipsBandAlikeVec(class.Nickname, tests, band, swit.N, 1) ||
        VipsSizeAlikeVec(band, size, swit.N))
        return -1;
    tests = size;

    if (tests[0].Bands > 1)
    {
        VipsError(class.Nickname, "%s", _("test images not 1-band"));
        return -1;
    }

    if (!VipsImagePipelineArray(swit.Out, VipsDemandStyle.ThinStrip, tests))
        return -1;

    if (!VipsImageGenerate(swit.Out,
            new VipsStartMany(), VipsSwitchGen, new VipsStopMany(),
            tests, swit))
        return -1;

    return 0;
}

// vips_switch_class_init
public static void VipsSwitchClassInit(VipsSwitchClass class)
{
    GObjectClass gobject_class = GType.GetClass(class);
    VipsObjectClass object_class = VipsObjectClass.GetClass(class);
    VipsOperationClass operation_class = VipsOperationClass.GetClass(class);

    gobject_class.SetProperty = VipsObject.SetProperty;
    gobject_class.GetProperty = VipsObject.GetProperty;

    object_class.Nickname = "switch";
    object_class.Description =
        _("find the index of the first non-zero pixel in tests");
    object_class.Build = VipsSwitchBuild;

    operation_class.Flags = VipsOperationFlags.Sequential;

    VipsArgBoxed(class, "tests", 1,
        _("Tests"),
        _("Table of images to test"),
        VipsArgument.RequiredInput,
        GStructOffset(typeof(VipsSwitch), "tests"),
        typeof(VipsArrayImage));

    VipsArgImage(class, "out", 2,
        _("Output"),
        _("Output image"),
        VipsArgument.RequiredOutput,
        GStructOffset(typeof(VipsSwitch), "out"));
}

// vips_switch_init
public static void VipsSwitchInit(VipsSwitch swit)
{
}

// vips_switchv
public static int VipsSwitchV(VipsImage[] tests, VipsImage[] out, int n, params object[] args)
{
    VipsArrayImage tests_array;
    int result;

    tests_array = new VipsArrayImage(tests);
    result = VipsCallSplit("switch", args, tests_array, out);
    VipsArea.Unref(VipsArea.Get(tests_array));

    return result;
}

// vips_switch
public static int VipsSwitch(VipsImage[] tests, VipsImage[] out, int n, params object[] args)
{
    var va = new VaList(args);
    int result;

    result = VipsSwitchV(tests, out, n, va);
    va.Dispose();

    return result;
}
```