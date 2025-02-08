```csharp
// Converted from vips__coeff in pmosaicing.c

public static int VipsCoeff(int xr1, int yr1, int xs1, int ys1,
                            int xr2, int yr2, int xs2, int ys2,
                            out double a, out double b, out double dx, out double dy)
{
    var t = new double[4, 4];

    if (!VipsMatrixInvert(t, out var invertedT))
        return -1;

    a = t[0, 0] * xr1 + t[0, 1] * yr1 +
        t[0, 2] * xr2 + t[0, 3] * yr2;
    b = t[1, 0] * xr1 + t[1, 1] * yr1 +
        t[1, 2] * xr2 + t[1, 3] * yr2;
    dx = t[2, 0] * xr1 + t[2, 1] * yr1 +
        t[2, 2] * xr2 + t[2, 3] * yr2;
    dy = t[3, 0] * xr1 + t[3, 1] * yr1 +
        t[3, 2] * xr2 + t[3, 3] * yr2;

    return 0;
}

// Converted from vips_match_build in pmosaicing.c

public class VipsMatch : VipsOperation
{
    public VipsImage Ref { get; set; }
    public VipsImage Sec { get; set; }
    public VipsImage Out { get; set; }
    public int Xr1 { get; set; }
    public int Yr1 { get; set; }
    public int Xs1 { get; set; }
    public int Ys1 { get; set; }
    public int Xr2 { get; set; }
    public int Yr2 { get; set; }
    public int Xs2 { get; set; }
    public int Ys2 { get; set; }
    public int Hwindow { get; set; }
    public int Harea { get; set; }
    public bool Search { get; set; }
    public VipsInterpolate Interpolate { get; set; }

    protected override int Build()
    {
        double a, b, dx, dy;
        var oarea = new VipsArrayInt(4);

        if (!VipsImageWrite(VipsAffine(Sec, out var x,
            a, -b, b, a,
            "interpolate", Interpolate,
            "odx", dx,
            "ody", dy,
            "oarea", oarea,
            null), Out))
        {
            VipsAreaUnref(VIPS_AREA(oarea));
            return -1;
        }
        VipsAreaUnref(VIPS_AREA(oarea));

        return 0;
    }

    protected override void ClassInit()
    {
        base.ClassInit();

        var gobjectClass = (GObjectClass)base.GetType().GetInterface(typeof(GObjectClass));
        var objectClass = (VipsObjectClass)base.GetType().GetInterface(typeof(VipsObjectClass));

        gobjectClass.SetProperty = VipsObjectSetProperty;
        gobjectClass.GetProperty = VipsObjectGetProperty;

        objectClass.Nickname = "match";
        objectClass.Description = _("first-order match of two images");
        objectClass.Build = Build;

        // Add properties
        var args = new[]
        {
            new VIPSArg("ref", 1, _("Reference"), _("Reference image"), VIPSArgument.RequiredInput),
            new VIPSArg("sec", 2, _("Secondary"), _("Secondary image"), VIPSArgument.RequiredInput),
            new VIPSArg("out", 3, _("Output"), _("Output image"), VIPSArgument.RequiredOutput),
            new VIPSArg("xr1", 5, _("xr1"), _("Position of first reference tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("yr1", 6, _("yr1"), _("Position of first reference tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("xs1", 7, _("xs1"), _("Position of first secondary tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("ys1", 8, _("ys1"), _("Position of first secondary tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("xr2", 9, _("xr2"), _("Position of second reference tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("yr2", 10, _("yr2"), _("Position of second reference tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("xs2", 11, _("xs2"), _("Position of second secondary tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("ys2", 12, _("ys2"), _("Position of second secondary tie-point"), VIPSArgument.RequiredInput),
            new VIPSArg("hwindow", 13, _("hwindow"), _("Half window size"), VIPSArgument.OptionalInput),
            new VIPSArg("harea", 14, _("harea"), _("Half search size"), VIPSArgument.OptionalInput),
            new VIPSArg("search", 15, _("Search"), _("Search to improve tie-points"), VIPSArgument.OptionalInput),
            new VIPSArg("interpolate", 16, _("Interpolate"), _("Interpolate pixels with this"), VIPSArgument.OptionalInput)
        };

        foreach (var arg in args)
        {
            objectClass.AddProperty(arg);
        }
    }

    protected override void Init()
    {
        base.Init();

        Hwindow = 5;
        Harea = 15;
        Search = false;
    }
}

// Converted from vips_match in pmosaicing.c

public static int VipsMatch(VipsImage Ref, VipsImage Sec, out VipsImage Out,
                            int Xr1, int Yr1, int Xs1, int Ys1,
                            int Xr2, int Yr2, int Xs2, int Ys2)
{
    var args = new object[] { Xr1, Yr1, Xs1, Ys1, Xr2, Yr2, Xs2, Ys2 };
    return VipsCallSplit("match", Ref, Sec, out Out, args);
}
```