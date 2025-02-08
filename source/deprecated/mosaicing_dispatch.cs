Here is the converted C# code:

```csharp
// lrmosaic_vec
public static int LrmosaicVec(IMAGE ref, IMAGE sec, IMAGE out, int bandno, int xr, int yr, int xs, int ys, int halfcorrelation, int halfarea, int mwidth)
{
    return Vips__Lrmosaic(ref, sec, out, bandno, xr, yr, xs, ys, halfcorrelation, halfarea, mwidth);
}

// lrmosaic1_vec
public static int Lrmosaic1Vec(IMAGE ref, IMAGE sec, IMAGE out, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2, int halfcorrelation, int halfarea, int mwidth)
{
    return Im_Lrmosaic1(ref, sec, out, 0, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, halfcorrelation, halfarea, 0, mwidth);
}

// find_lroverlap_vec
public static int FindLroverlapVec(IMAGE ref, IMAGE sec, double[] dx0, double[] dy0, double[] scale1, double[] angle1, double[] dx1, double[] dy1)
{
    int bandno = 0;
    int xr = 0;
    int yr = 0;
    int xs = 0;
    int ys = 0;
    int halfcorrelation = 0;
    int halfarea = 0;

    IMAGE t;
    int result;

    if (!(t = Im_Open("find_lroverlap_vec", "p")))
        return -1;
    result = Vips__FindLroverlap(ref, sec, t, bandno, xr, yr, xs, ys, halfcorrelation, halfarea, dx0, dy0, scale1, angle1, dx1, dy1);
    Im_Close(t);

    return result;
}

// tbmosaic_vec
public static int TbmosaicVec(IMAGE ref, IMAGE sec, IMAGE out, int bandno, int x1, int y1, int x2, int y2, int halfcorrelation, int halfarea, int mwidth)
{
    return Vips__Tbmosaic(ref, sec, out, bandno, x1, y1, x2, y2, halfcorrelation, halfarea, mwidth);
}

// tbmosaic1_vec
public static int Tbmosaic1Vec(IMAGE ref, IMAGE sec, IMAGE out, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2, int halfcorrelation, int halfarea, int mwidth)
{
    return Im_Tbmosaic1(ref, sec, out, 0, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, halfcorrelation, halfarea, 0, mwidth);
}

// lrmerge_vec
public static int LrmergeVec(IMAGE ref, IMAGE sec, IMAGE out, int dx, int dy, int mwidth)
{
    return Im_Lrmerge(ref, sec, out, dx, dy, mwidth);
}

// lrmerge1_vec
public static int Lrmerge1Vec(IMAGE ref, IMAGE sec, IMAGE out, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2, int mwidth)
{
    return Im_Lrmerge1(ref, sec, out, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, mwidth);
}

// tbmerge_vec
public static int TbmergeVec(IMAGE ref, IMAGE sec, IMAGE out, int dx, int dy, int mwidth)
{
    return Im_Tbmerge(ref, sec, out, dx, dy, mwidth);
}

// tbmerge1_vec
public static int Tbmerge1Vec(IMAGE ref, IMAGE sec, IMAGE out, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2, int mwidth)
{
    return Im_Tbmerge1(ref, sec, out, xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, mwidth);
}

// match_linear_vec
public static int MatchLinearVec(IMAGE ref, IMAGE sec, IMAGE out, int xref1, int yref1, int xsec1, int ysec1, int xref2, int yref2, int xsec2, int ysec2)
{
    return Im_MatchLinear(ref, sec, out, xref1, yref1, xsec1, ysec1, xref2, yref2, xsec2, ysec2);
}

// match_linear_search_vec
public static int MatchLinearSearchVec(IMAGE ref, IMAGE sec, IMAGE out, int xref1, int yref1, int xsec1, int ysec1, int xref2, int yref2, int xsec2, int ysec2, int hwin, int hsrch)
{
    return Im_MatchLinearSearch(ref, sec, out, xref1, yref1, xsec1, ysec1, xref2, yref2, xsec2, ysec2, hwin, hsrch);
}

// correl_vec
public static int CorrelVec(IMAGE ref, IMAGE sec, double[] correlation, int[] x, int[] y)
{
    int xref = 0;
    int yref = 0;
    int xsec = 0;
    int ysec = 0;
    int cor = 0;
    int area = 0;

    return Vips__Correl(ref, sec, xref, yref, xsec, ysec, cor, area, correlation, x, y);
}

// global_balance_vec
public static int GlobalBalanceVec(IMAGE in, IMAGE out, double gamma)
{
    return Im_GlobalBalance(in, out, gamma);
}

// global_balancef_vec
public static int GlobalBalanceFVec(IMAGE in, IMAGE out, double gamma)
{
    return Im_GlobalBalanceF(in, out, gamma);
}

// remosaic_vec
public static int RemosaicVec(IMAGE in, IMAGE out, string old_str, string new_str)
{
    return Im_Remosaic(in, out, old_str, new_str);
}

// align_bands_vec
public static int AlignBandsVec(IMAGE in, IMAGE out)
{
    return Im_AlignBands((VipsImage)in, (VipsImage)out);
}

// maxpos_subpel_vec
public static double MaxPosSubpelVec(IMAGE im, double[] x, double[] y)
{
    return Im_MaxPosSubpel((VipsImage)im, x, y);
}
```

Note that I've assumed the existence of a `VipsImage` class to wrap the `IMAGE` struct from the original C code. You may need to modify this to match your actual implementation.

Also, I've kept the same naming conventions as the original C code, but you may want to consider using more descriptive names in your C# code.

Finally, I've omitted the `im_package` structure and its associated functions, as they seem to be related to the VIPS library's internal workings. If you need help with those, please let me know!