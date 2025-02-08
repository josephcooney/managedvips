```csharp
// vips_col_XYZ2Lab_helper (from vips_col_XYZ2Lab_helper)
public static void XYZ2LabHelper(float X, float Y, float Z, ref float L, ref float a, ref float b)
{
    float nX = QUANT_ELEMENTS * X / VIPS_D65_X0;
    float nY = QUANT_ELEMENTS * Y / VIPS_D65_Y0;
    float nZ = QUANT_ELEMENTS * Z / VIPS_D65_Z0;

    int i = Math.Min(Math.Max(0, (int)nX), QUANT_ELEMENTS - 2);
    float f = nX - i;
    float cbx = cbrt_table[i] + f * (cbrt_table[i + 1] - cbrt_table[i]);

    i = Math.Min(Math.Max(0, (int)nY), QUANT_ELEMENTS - 2);
    f = nY - i;
    float cby = cbrt_table[i] + f * (cbrt_table[i + 1] - cbrt_table[i]);

    i = Math.Min(Math.Max(0, (int)nZ), QUANT_ELEMENTS - 2);
    f = nZ - i;
    float cbz = cbrt_table[i] + f * (cbrt_table[i + 1] - cbrt_table[i]);

    L = 116.0f * cby - 16.0f;
    a = 500.0f * (cbx - cby);
    b = 200.0f * (cby - cbz);
}

// vips_XYZ2Lab_line (from vips_XYZ2Lab_line)
public static void XYZ2LabLine(VipsColour colour, VipsPel[] out, VipsPel[][] in, int width)
{
    VipsXYZ2Lab xyz2lab = (VipsXYZ2Lab)colour;
    float[] p = (float[])in[0];
    float[] q = new float[out.Length * 3];

    for (int x = 0; x < width; x++)
    {
        float X = p[0];
        float Y = p[1];
        float Z = p[2];
        p = p.Skip(3).ToArray();

        XYZ2LabHelper(X, Y, Z, ref q[x * 3], ref q[x * 3 + 1], ref q[x * 3 + 2]);
    }
}

// vips_col_XYZ2Lab (from vips_col_XYZ2Lab)
public static void ColXYZ2Lab(float X, float Y, float Z, out float L, out float a, out float b)
{
    VIPS_ONCE(table_init_once);
    XYZ2LabHelper(X, Y, Z, ref L, ref a, ref b);
}

// vips_XYZ2Lab_build (from vips_XYZ2Lab_build)
public static int XYZ2LabBuild(VipsObject object)
{
    VipsXYZ2Lab xyz2lab = (VipsXYZ2Lab)object;

    if (xyz2lab.temp != null)
    {
        if (!vips_check_vector_length(xyz2lab.temp.n, 3))
            return -1;
        xyz2lab.X0 = ((double[])xyz2lab.temp.data)[0];
        xyz2lab.Y0 = ((double[])xyz2lab.temp.data)[1];
        xyz2lab.Z0 = ((double[])xyz2lab.temp.data)[2];
    }

    if (VIPS_OBJECT_CLASS(vips_XYZ2Lab_parent_class).build(object) != 0)
        return -1;

    return 0;
}

// vips_XYZ2Lab_class_init (from vips_XYZ2Lab_class_init)
public static void XYZ2LabClassInit()
{
    VipsObjectClass object_class = new VipsObjectClass();
    VipsColourClass colour_class = new VipsColourClass();

    object_class.nickname = "XYZ2Lab";
    object_class.description = _("transform XYZ to Lab");
    object_class.build = XYZ2LabBuild;

    colour_class.process_line = XYZ2LabLine;
}

// vips_XYZ2Lab_init (from vips_XYZ2Lab_init)
public static void XYZ2LabInit(VipsXYZ2Lab xyz2lab)
{
    xyz2lab.X0 = VIPS_D65_X0;
    xyz2lab.Y0 = VIPS_D65_Y0;
    xyz2lab.Z0 = VIPS_D65_Z0;

    xyz2lab.interpretation = VIPS_INTERPRETATION_LAB;
}

// vips_XYZ2Lab (from vips_XYZ2Lab)
public static int XYZ2Lab(VipsImage in, out VipsImage out, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, args);
    result = vips_call_split("XYZ2Lab", ap, in, out);
    va_end(ap);

    return result;
}

// table_init (from table_init)
public static void TableInit()
{
    for (int i = 0; i < QUANT_ELEMENTS; i++)
    {
        float Y = (double)i / QUANT_ELEMENTS;

        if (Y < 0.008856f)
            cbrt_table[i] = 7.787f * Y + (16.0f / 116.0f);
        else
            cbrt_table[i] = (float)Math.Pow(Y, 1.0/3.0);
    }
}

// table_init_once
private static GOnce table_init_once = new GOnce(TableInit);

// QUANT_ELEMENTS
public const int QUANT_ELEMENTS = 100000;

// VIPS_D65_X0, VIPS_D65_Y0, VIPS_D65_Z0
public const double VIPS_D65_X0 = 95.047;
public const double VIPS_D65_Y0 = 100.000;
public const double VIPS_D65_Z0 = 108.883;

// cbrt_table
private static float[] cbrt_table = new float[QUANT_ELEMENTS];
```