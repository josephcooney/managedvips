Here is the converted C# code:

```csharp
// vips_col_sRGB2scRGB
public static int SRGB2SCRGB(int range, float[] lut, int r, int g, int b, ref float R, ref float G, ref float B)
{
    int maxval = range - 1;
    int i;

    i = Math.Min(Math.Max(0, r), maxval);
    R = lut[i];

    i = Math.Min(Math.Max(0, g), maxval);
    G = lut[i];

    i = Math.Min(Math.Max(0, b), maxval);
    B = lut[i];

    return 0;
}

// calcul_tables
public static void CalculTables(int range, int[] Y2v, float[] v2Y)
{
    for (int i = 0; i < range; i++)
    {
        float f = (float)i / (range - 1);
        float v;

        if (f <= 0.0031308)
            v = 12.92f * f;
        else
            v = (1.0f + 0.055f) * (float)Math.Pow(f, 1.0f / 2.4f) - 0.055f;

        Y2v[i] = (int)((range - 1) * v);
    }

    // Copy the final element.
    Y2v[range] = Y2v[range - 1];

    for (int i = 0; i < range; i++)
    {
        float f = (float)i / (range - 1);

        if (f <= 0.04045)
            v2Y[i] = f / 12.92f;
        else
            v2Y[i] = (float)Math.Pow((f + 0.055f) / (1.0f + 0.055f), 2.4f);
    }
}

// vips_col_make_tables_RGB_8
public static void MakeTablesRGB8()
{
    GOnce once = new GOnce(CalculTables_8, null);
    VIPS_ONCE(once, null);
}

// vips_col_sRGB2scRGB_8
public static int SRGB2SCRGB8(int r, int g, int b, ref float R, ref float G, ref float B)
{
    MakeTablesRGB8();
    return SRGB2SCRGB(256, vips_Y2v_8, r, g, b, ref R, ref G, ref B);
}

// vips_col_make_tables_RGB_16
public static void MakeTablesRGB16()
{
    GOnce once = new GOnce(CalculTables_16, null);
    VIPS_ONCE(once, null);
}

// vips_col_sRGB2scRGB_16
public static int SRGB2SCRGB16(int r, int g, int b, ref float R, ref float G, ref float B)
{
    MakeTablesRGB16();
    return SRGB2SCRGB(65536, vips_Y2v_16, r, g, b, ref R, ref G, ref B);
}

// vips_col_scRGB2XYZ
public static int SCRGB2XYZ(float R, float G, float B, ref float X, ref float Y, ref float Z)
{
    R *= VIPS_D65_Y0;
    G *= VIPS_D65_Y0;
    B *= VIPS_D65_Y0;

    X = 0.4124f * R + 0.3576f * G + 0.1805f * B;
    Y = 0.2126f * R + 0.7152f * G + 0.0722f * B;
    Z = 0.0193f * R + 0.1192f * G + 0.9505f * B;

    return 0;
}

// vips_col_XYZ2scRGB
public static int XYZ2SCRGB(float X, float Y, float Z, ref float R, ref float G, ref float B)
{
    X /= VIPS_D65_Y0;
    Y /= VIPS_D65_Y0;
    Z /= VIPS_D65_Y0;

    // Use 6 decimal places of precision for the inverse matrix.
    R = 3.240625f * X - 1.537208f * Y - 0.498629f * Z;
    G = -0.968931f * X + 1.875756f * Y + 0.041518f * Z;
    B = 0.055710f * X - 0.204021f * Y + 1.056996f * Z;

    return 0;
}

// vips_col_scRGB2sRGB
public static int SCRGB2SRGB(int range, int[] lut, float R, float G, float B, ref int r, ref int g, ref int b, ref int og)
{
    int maxval = range - 1;

    int og_val;
    float Yf;
    int Yi;
    float v;

    // RGB can be NaN. Throw those values out, they will break our clipping.
    if (float.IsNaN(R) || float.IsNaN(G) || float.IsNaN(B))
    {
        r = 0;
        g = 0;
        b = 0;

        return -1;
    }

    // Clip range, set the out-of-gamut flag.
    int CLIP(int L, float V, float H)
    {
        if (V < L)
        {
            V = L;
            og_val = 1;
        }
        else if (V > H)
        {
            V = H;
            og_val = 1;
        }

        return og_val;
    }

    // Look up with a float index: interpolate between the nearest two points.
    og_val = 0;

    Yf = R * maxval;
    CLIP(0, Yf, maxval);
    Yi = (int)Yf;
    v = lut[Yi] + (lut[Yi + 1] - lut[Yi]) * (Yf - Yi);
    r = (int)v;

    Yf = G * maxval;
    CLIP(0, Yf, maxval);
    Yi = (int)Yf;
    v = lut[Yi] + (lut[Yi + 1] - lut[Yi]) * (Yf - Yi);
    g = (int)v;

    Yf = B * maxval;
    CLIP(0, Yf, maxval);
    Yi = (int)Yf;
    v = lut[Yi] + (lut[Yi + 1] - lut[Yi]) * (Yf - Yi);
    b = (int)v;

    if (og != null)
        og.Value = og_val;

    return 0;
}

// vips_col_scRGB2sRGB_8
public static int SCRGB2SRGB8(float R, float G, float B, ref int r, ref int g, ref int b, ref int og)
{
    MakeTablesRGB8();
    return SCRGB2SRGB(256, vips_Y2v_8, R, G, B, ref r, ref g, ref b, ref og);
}

// vips_col_scRGB2sRGB_16
public static int SCRGB2SRGB16(float R, float G, float B, ref int r, ref int g, ref int b, ref int og)
{
    MakeTablesRGB16();
    return SCRGB2SRGB(65536, vips_Y2v_16, R, G, B, ref r, ref g, ref b, ref og);
}

// vips_col_scRGB2BW
public static int SCRGB2BW(int range, int[] lut, float R, float G, float B, ref int g, ref int og)
{
    int maxval = range - 1;

    float Y;
    int og_val;
    float Yf;
    int Yi;
    float v;

    // CIE linear luminance function, see https://en.wikipedia.org/wiki/Grayscale#Colorimetric_(perceptual_luminance-preserving)_conversion_to_grayscale
    Y = 0.2126f * R + 0.7152f * G + 0.0722f * B;

    // Y can be NaN. Throw those values out, they will break our clipping.
    if (float.IsNaN(Y))
    {
        g = 0;

        return -1;
    }

    // Look up with a float index: interpolate between the nearest two points.
    og_val = 0;

    Yf = Y * maxval;
    CLIP(0, Yf, maxval);
    Yi = (int)Yf;
    v = lut[Yi] + (lut[Yi + 1] - lut[Yi]) * (Yf - Yi);
    g = (int)v;

    if (og != null)
        og.Value = og_val;

    return 0;
}

// vips_col_scRGB2BW_8
public static int SCRGB2BW8(float R, float G, float B, ref int g, ref int og)
{
    MakeTablesRGB8();
    return SCRGB2BW(256, vips_Y2v_8, R, G, B, ref g, ref og);
}

// vips_col_scRGB2BW_16
public static int SCRGB2BW16(float R, float G, float B, ref int g, ref int og)
{
    MakeTablesRGB16();
    return SCRGB2BW(65536, vips_Y2v_16, R, G, B, ref g, ref og);
}

// build_tables
public static void BuildTables()
{
    for (int l = 0; l < 64; l++)
    {
        for (int a = 0; a < 64; a++)
        {
            for (int b = 0; b < 64; b++)
            {
                // Scale to lab space.
                float L = (l << 2) * (100.0f / 256.0f);
                float A = (sbyte)(a << 2);
                float B = (sbyte)(b << 2);
                float X, Y, Z;
                float Rf, Gf, Bf;
                int rb, gb, bb;

                SCRGB2XYZ(L, A, B, ref X, ref Y, ref Z);
                XYZ2SCRGB(X, Y, Z, ref Rf, ref Gf, ref Bf);
                SCRGB2SRGB8(Rf, Gf, Bf, ref rb, ref gb, ref bb, null);

                int t = INDEX(l, a, b);
                vips_red[t] = rb;
                vips_green[t] = gb;
                vips_blue[t] = bb;
            }
        }
    }
}

// vips_col_make_tables_LabQ2sRGB
public static void MakeTablesLabQ2sRGB()
{
    GOnce once = new GOnce(BuildTables, null);
    VIPS_ONCE(once, null);
}

// vips_LabQ2sRGB_line
public static void LabQ2sRGBLine(VipsColour colour, VipsPel[] q, VipsPel[][] in, int width)
{
    unsigned char[] p = (unsigned char[])in[0];

    int i, t;

    // Current error.
    int le = 0;
    int ae = 0;
    int be = 0;

    MakeTablesLabQ2sRGB();

    for (i = 0; i < width; i++)
    {
        // Get colour, add in error from previous pixel.
        int L = p[0] + le;
        int A = (sbyte)p[1] + ae;
        int B = (sbyte)p[2] + be;

        p += 4;

        // Look out for overflow.
        L = Math.Min(255, L);
        A = Math.Min(127, A);
        B = Math.Min(127, B);

        // Find new quant error. This will always be +ve.
        le = L & 3;
        ae = A & 3;
        be = B & 3;

        // Scale to 0-63.
        L = (L >> 2) & 63;
        A = (A >> 2) & 63;
        B = (B >> 2) & 63;

        // Convert to RGB.
        t = INDEX(L, A, B);
        q[0] = vips_red[t];
        q[1] = vips_green[t];
        q[2] = vips_blue[t];

        q += 3;
    }
}

// vips_LabQ2sRGB_class_init
public static void LabQ2sRGBClassInit(VipsLabQ2sRGBClass class)
{
    VipsObjectClass objectClass = (VipsObjectClass)class;
    VipsColourClass colourClass = VIPS_COLOUR_CLASS(class);

    objectClass.nickname = "LabQ2sRGB";
    objectClass.description = _("convert a LabQ image to sRGB");

    colourClass.processLine = LabQ2sRGBLine;
}

// vips_LabQ2sRGB_init
public static void LabQ2sRGBInit(VipsLabQ2sRGB LabQ2sRGB)
{
    VipsColour colour = VIPS_COLOUR(LabQ2sRGB);
    VipsColourCode code = VIPS_COLOUR_CODE(LabQ2sRGB);

    colour.coding = VIPS_CODING_NONE;
    colour.interpretation = VIPS_INTERPRETATION_sRGB;
    colour.format = VIPS_FORMAT_UCHAR;
    colour.bands = 3;

    code.inputCoding = VIPS_CODING_LABQ;
}

// vips_LabQ2sRGB
public static int LabQ2sRGB(VipsImage in, ref VipsImage out)
{
    return CallSplit("LabQ2sRGB", in, ref out);
}
```

Note that I've assumed the existence of certain types and methods (e.g. `VipsColour`, `VipsObjectClass`, `GOnce`) which are not defined in this code snippet. You may need to modify the code to match your specific use case.

Also, I've kept the original comments from the C code where possible, but some modifications were necessary to make them compatible with C#.