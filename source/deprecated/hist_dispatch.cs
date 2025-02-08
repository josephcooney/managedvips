Here is the C# code equivalent to the provided C code:

```csharp
using System;

// gammacorrect_vec
public static int GammacorrectVec(int[] argv)
{
    double exp = BitConverter.ToDouble(argv, 2);
    return Vips.GammaCorrect(argv[0], argv[1], exp);
}

// heq_vec
public static int HeqVec(int[] argv)
{
    int bn = argv[2];
    return Vips.Heq(argv[0], argv[1], bn);
}

// histindexed_vec
public static int HistIndexedVec(int[] argv)
{
    return Vips.HistIndexed(argv[0], argv[1], argv[2]);
}

// hist_vec
public static int HistVec(int[] argv)
{
    int bn = argv[2];
    return Vips.Hist(argv[0], argv[1], bn);
}

// histcum_vec
public static int HistCumVec(int[] argv)
{
    return Vips.HistCum(argv[0], argv[1]);
}

// histnorm_vec
public static int HistNormVec(int[] argv)
{
    return Vips.HistNorm(argv[0], argv[1]);
}

// histeq_vec
public static int HisteqVec(int[] argv)
{
    return Vips.Histeq(argv[0], argv[1]);
}

// histgr_vec
public static int HistGrVec(int[] argv)
{
    int bn = argv[2];
    return Vips.HistGr(argv[0], argv[1], bn);
}

// histnD_vec
public static int HistNDVec(int[] argv)
{
    int bins = argv[2];
    return Vips.HistND(argv[0], argv[1], bins);
}

// histplot_vec
public static int HistPlotVec(int[] argv)
{
    return Vips.HistPlot(argv[0], argv[1]);
}

// histspec_vec
public static int HistspecVec(int[] argv)
{
    return Vips.Histspec(argv[0], argv[1], argv[2]);
}

// hsp_vec
public static int HspVec(int[] argv)
{
    return Vips.Hsp(argv[0], argv[1], argv[2]);
}

// identity_vec
public static int IdentityVec(int[] argv)
{
    int nb = argv[1];
    return Vips.Identity(argv[0], nb);
}

// identity_ushort_vec
public static int IdentityUshortVec(int[] argv)
{
    int nb = argv[1];
    int sz = argv[2];
    return Vips.IdentityUshort(argv[0], nb, sz);
}

// lhisteq_vec
public static int LhistEqVec(int[] argv)
{
    int xw = argv[2];
    int yw = argv[3];
    return Vips.LHistEq(argv[0], argv[1], xw, yw);
}

// maplut_vec
public static int MapLutVec(int[] argv)
{
    return Vips.MapLut(argv[0], argv[1], argv[2]);
}

// project_vec
public static int ProjectVec(int[] argv)
{
    return Vips.Project(argv[0], argv[1], argv[2]);
}

// stdif_vec
public static int StdifVec(int[] argv)
{
    double a = BitConverter.ToDouble(argv, 2);
    double m0 = BitConverter.ToDouble(argv, 3);
    double b = BitConverter.ToDouble(argv, 4);
    double s0 = BitConverter.ToDouble(argv, 5);
    int xw = argv[6];
    int yw = argv[7];
    return Vips.Stdif(argv[0], argv[1], a, m0, b, s0, xw, yw);
}

// buildlut_vec
public static int BuildLutVec(int[] argv)
{
    im_mask_object mi = (im_mask_object)argv[0];
    return Vips.BuildLut(mi.mask, argv[1]);
}

// invertlut_vec
public static int InvertLutVec(int[] argv)
{
    im_mask_object mi = (im_mask_object)argv[0];
    int lut_size = argv[2];
    return Vips.InvertLut(mi.mask, argv[1], lut_size);
}

// tone_build_vec
public static int ToneBuildVec(int[] argv)
{
    double Lb = BitConverter.ToDouble(argv, 1);
    double Lw = BitConverter.ToDouble(argv, 2);
    double Ps = BitConverter.ToDouble(argv, 3);
    double Pm = BitConverter.ToDouble(argv, 4);
    double Ph = BitConverter.ToDouble(argv, 5);
    double S = BitConverter.ToDouble(argv, 6);
    double M = BitConverter.ToDouble(argv, 7);
    double H = BitConverter.ToDouble(argv, 8);
    return Vips.ToneBuild(argv[0], Lb, Lw, Ps, Pm, Ph, S, M, H);
}

// tone_build_range_vec
public static int ToneBuildRangeVec(int[] argv)
{
    int in_max = argv[1];
    int out_max = argv[2];
    double Lb = BitConverter.ToDouble(argv, 3);
    double Lw = BitConverter.ToDouble(argv, 4);
    double Ps = BitConverter.ToDouble(argv, 5);
    double Pm = BitConverter.ToDouble(argv, 6);
    double Ph = BitConverter.ToDouble(argv, 7);
    double S = BitConverter.ToDouble(argv, 8);
    double M = BitConverter.ToDouble(argv, 9);
    double H = BitConverter.ToDouble(argv, 10);
    return Vips.ToneBuildRange(argv[0], in_max, out_max,
        Lb, Lw, Ps, Pm, Ph, S, M, H);
}

// tone_analyse_vec
public static int ToneAnalyseVec(int[] argv)
{
    double Ps = BitConverter.ToDouble(argv, 2);
    double Pm = BitConverter.ToDouble(argv, 3);
    double Ph = BitConverter.ToDouble(argv, 4);
    double S = BitConverter.ToDouble(argv, 5);
    double M = BitConverter.ToDouble(argv, 6);
    double H = BitConverter.ToDouble(argv, 7);
    return Vips.ToneAnalyse(argv[0], argv[1], Ps, Pm, Ph, S, M, H);
}

// ismonotonic_vec
public static int IsMonotonicVec(int[] argv)
{
    int res = (int)argv[1];
    return Vips.IsMonotonic(argv[0], ref res);
}

// tone_map_vec
public static int ToneMapVec(int[] argv)
{
    return Vips.ToneMap(argv[0], argv[1], argv[2]);
}

// mpercent_vec
public static int MPercentVec(int[] argv)
{
    double percent = BitConverter.ToDouble(argv, 1);
    return Vips.MPercent(argv[0], percent, argv[2]);
}
```

Note that this code assumes the existence of a `Vips` class with methods corresponding to the C functions. The actual implementation of these methods is not provided here.

Also note that I've used `BitConverter.ToDouble` to convert the `int[]` arguments to `double` values where necessary, as there is no direct equivalent in C# for the C-style pointer arithmetic used in the original code.