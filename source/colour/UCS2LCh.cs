```csharp
// vips_col_Lcmc2L (vips_col_Lcmc2L)
public static float Lcmc2L(float Lcmc) {
    int known = Math.Min(999, Math.Max(0, (int)(Lcmc * 10)));
    return LI[known] + (LI[known + 1] - LI[known]) * (Lcmc * 10.0 - known);
}

// vips_col_Ccmc2C (vips_col_Ccmc2C)
public static float Ccmc2C(float Ccmc) {
    int known = Math.Min(2999, Math.Max(0, (int)(Ccmc * 10)));
    return CI[known] + (CI[known + 1] - CI[known]) * (Ccmc * 10.0 - known);
}

// vips_col_Chcmc2h (vips_col_Chcmc2h)
public static float Chcmc2h(float C, float hcmc) {
    int r = Math.Min(99, Math.Max(0, (int)((C + 1.0) / 2)));
    int known = Math.Min(359, Math.Max(0, (int)(hcmc)));
    return hI[r][known] + (hI[r][(known + 1) % 360] - hI[r][known]) * (hcmc - known);
}

// vips_col_make_tables_CMC (vips_col_make_tables_CMC)
public static void MakeTablesCMC() {
    make_LI();
    make_CI();
    make_hI();
}

// vips_CMC2LCh_line (vips_CMC2LCh_line)
public static void CMC2LChLine(VipsColour colour, float[] out, float[] in, int width) {
    for (int x = 0; x < width; x++) {
        float Lcmc = in[0];
        float Ccmc = in[1];
        float hcmc = in[2];

        // Turn from CMC.
        float C = Ccmc2C(Ccmc);
        float h = Chcmc2h(C, hcmc);
        float L = Lcmc2L(Lcmc);

        out[x * 3] = L;
        out[x * 3 + 1] = C;
        out[x * 3 + 2] = h;
    }
}

// vips_CMC2LCh_class_init (vips_CMC2LCh_class_init)
public static void CMC2LChClassInit() {
    VipsObjectClass objectClass = new VipsObjectClass();
    VipsColourClass colourClass = new VipsColourClass();

    objectClass.nickname = "CMC2LCh";
    objectClass.description = "transform LCh to CMC";

    colourClass.processLine = CMC2LChLine;
}

// vips_CMC2LCh_init (vips_CMC2LCh_init)
public static void CMC2LChInit(VipsColour colour) {
    MakeTablesCMC();
    colour.interpretation = VIPS_INTERPRETATION_LCH;
}

// vips_CMC2LCh (vips_CMC2LCh)
public static int CMC2LCh(VipsImage in, out VipsImage[] out) {
    return 0; // TODO: implement vips_call_split
}
```