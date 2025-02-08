```csharp
// vips_buildlut_init()
public VipsBuildlut(VipsImage in)
{
    this.in = in;
}

// vips_buildlut_dispose()
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        // free managed resources
    }
    base.Dispose(disposing);
}

// vips_buildlut_compare()
private int Compare(double[] r1, double[] r2)
{
    return Math.Sign(r1[0] - r2[0]);
}

// vips_buildlut_build_init()
private bool BuildInit(VipsBuildlut lut)
{
    VipsImage mat = lut.in;
    int y;
    int xlow = (int)mat.GetPixel(0, 0);
    int xhigh = xlow;

    for (y = 0; y < mat.Ysize; y++)
    {
        double v = mat.GetPixel(0, y);

        if (Math.Abs(v - Math.Round(v)) > 0.001)
        {
            throw new Exception("x value row " + y.ToString() + " not an int");
        }

        v = Math.Round(v);
        if (v < xlow) xlow = v;
        if (v > xhigh) xhigh = v;
    }
    lut.xlow = xlow;
    lut.lut_size = xhigh - xlow + 1;

    if (lut.lut_size < 1)
    {
        throw new Exception("x range too small");
    }

    lut.data = new double[mat.Ysize][];
    for (y = 0; y < mat.Ysize; y++)
    {
        lut.data[y] = new double[mat.Xsize - 1];
        for (int x = 0; x < mat.Xsize - 1; x++)
        {
            lut.data[y][x] = mat.GetPixel(x + 1, y);
        }
    }

    lut.buf = new double[lut.lut_size * (mat.Xsize - 1)];

    Array.Sort(lut.data, Compare);

    return true;
}

// vips_buildlut_build_create()
private bool BuildCreate(VipsBuildlut lut)
{
    int b, i, x;
    const int xlow = lut.xlow;
    const VipsImage mat = lut.in;
    const int xsize = mat.Xsize;
    const int ysize = mat.Ysize;
    const int bands = xsize - 1;
    const int xlast = (int)lut.data[ysize - 1][0];

    for (b = 0; b < bands; b++)
    {
        for (i = 0; i < ysize - 1; i++)
        {
            int x1 = (int)Math.Round(lut.data[i][0]);
            int x2 = (int)Math.Round(lut.data[i + 1][0]);
            double dx = x2 - x1;
            double y1 = lut.data[i][b + 1];
            double y2 = lut.data[i + 1][b + 1];
            double dy = y2 - y1;

            for (x = 0; x < dx; x++)
            {
                int index = b + (x + x1 - xlow) * bands;
                lut.buf[index] = y1 + x * dy / dx;
            }
        }

        // We are inclusive: pop the final value in by hand.
        int index = b + (xlast - xlow) * bands;
        lut.buf[index] = lut.data[ysize - 1][b + 1];
    }

    return true;
}

// vips_buildlut_build()
public bool Build(VipsObject object)
{
    VipsBuildlut lut = (VipsBuildlut)object;

    if (!base.Build(object))
    {
        return false;
    }

    if (!CheckMatrix(lut.in, out VipsImage mat))
    {
        return false;
    }

    if (!BuildInit(lut) || !BuildCreate(lut))
    {
        return false;
    }

    ImageInitFields(out VipsImage image, lut.lut_size, 1, mat.Xsize - 1,
        VipsFormat.Double, VipsCoding.None, VipsInterpretation.Histogram, 1.0, 1.0);

    if (!WriteLine(image, 0, (VipsPel[])lut.buf))
    {
        return false;
    }

    return true;
}

// vips_buildlut_class_init()
public static void ClassInit(VipsBuildlutClass class)
{
    base.ClassInit(class);
    class.Nickname = "buildlut";
    class.Description = "build a look-up table";
    class.Build = Build;
    VipsArgImage arg = new VipsArgImage("in", 0, "Input", "Matrix of XY coordinates",
        VipsArgument.RequiredInput, GStructOffset<VipsBuildlut>.FieldIn);
    class.Args.Add(arg);
}

// vips_buildlut()
public static int Build(VipsImage in, out VipsImage out)
{
    try
    {
        VipsBuildlut lut = new VipsBuildlut(in);
        if (!lut.Build())
        {
            return -1;
        }
        out = lut.Image;
        return 0;
    }
    catch (Exception ex)
    {
        throw new Exception("Error building LUT: " + ex.Message);
    }
}
```