```csharp
// vips_col_L2Lcmc
public static float L2Lcmc(float L)
{
    if (L < 16.0f)
        return 1.744f * L;
    else
        return 21.75f * Math.Log(L) + 0.3838f * L - 38.54f;
}

// vips_col_C2Ccmc
public static float C2Ccmc(float C)
{
    float Ccmc = 0.162f * C + 10.92f * Math.Log(0.638f + 0.07216f * C) + 4.907f;
    if (Ccmc < 0)
        Ccmc = 0;
    return Ccmc;
}

// vips_col_Ch2hcmc
public static float Ch2hcmc(float C, float h)
{
    float P, D, f, g;
    float k4, k5, k6, k7, k8;
    float hcmc;

    if (h < 49.1f) {
        k4 = 133.87f;
        k5 = -134.5f;
        k6 = -.924f;
        k7 = 1.727f;
        k8 = 340.0f;
    }
    else if (h < 110.1f) {
        k4 = 11.78f;
        k5 = -12.7f;
        k6 = -.218f;
        k7 = 2.12f;
        k8 = 333.0f;
    }
    else if (h < 269.6f) {
        k4 = 13.87f;
        k5 = 10.93f;
        k6 = 0.14f;
        k7 = 1.0f;
        k8 = -83.0f;
    }
    else {
        k4 = .14f;
        k5 = 5.23f;
        k6 = .17f;
        k7 = 1.61f;
        k8 = 233.0f;
    }

    P = (float)Math.Cos(k7 * h + k8);
    D = k4 + k5 * P * Math.Pow(Math.Abs(P), k6);
    g = C * C * C * C;
    f = (float)Math.Sqrt(g / (g + 1900.0f));
    hcmc = h + D * f;

    return hcmc;
}

// vips_LCh2CMC_line
public static void LCh2CMCLine(VipsColour colour, VipsPel[] out, VipsPel[][] in)
{
    float[] p = (float[])in[0];
    float[] q = new float[out.Length];

    for (int x = 0; x < out.Length; x++)
    {
        float L = p[0];
        float C = p[1];
        float h = p[2];

        p += 3;

        q[0] = L2Lcmc(L);
        q[1] = C2Ccmc(C);
        q[2] = Ch2hcmc(C, h);

        Array.Copy(q, 0, out[x], 0, q.Length);
    }
}

// vips_LCh2CMC_class_init
public static void LCh2CMCClassInit(VipsLCh2CMCClass class)
{
    VipsObjectClass objectClass = (VipsObjectClass)class;
    VipsColourClass colourClass = VIPS_COLOUR_CLASS(class);

    objectClass.nickname = "LCh2CMC";
    objectClass.description = "transform LCh to CMC";

    colourClass.processLine = LCh2CMCLine;
}

// vips_LCh2CMC_init
public static void LCh2CMCInit(VipsLCh2CMC lch2cmc)
{
    VipsColour colour = VIPS_COLOUR(lch2cmc);

    colour.interpretation = VIPS_INTERPRETATION_CMC;
}

// vips_LCh2CMC
public static int LCh2CMCVipsImage(VipsImage in, ref VipsImage out, params object[] args)
{
    return VipsCallSplit("LCh2CMC", args, in, ref out);
}
```