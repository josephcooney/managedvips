```csharp
// vips__find_tboverlap

public static int FindTbOverlap(VipsImage ref_in, VipsImage sec_in, VipsImage out,
    int bandno_in,
    int xref, int yref, int xsec, int ysec,
    int halfcorrelation, int halfarea,
    out int dx0, out int dy0,
    out double scale1, out double angle1, out double dx1, out double dy1)
{
    VipsImage[] t = new VipsImage[6];

    // ...

    return 0;
}

// vips__tbmosaic

public static int TbMosaic(VipsImage ref, VipsImage sec, VipsImage out,
    int bandno,
    int xref, int yref, int xsec, int ysec,
    int hwindowsize, int hsearchsize,
    int mwidth)
{
    int dx0, dy0;
    double scale1, angle1, dx1, dy1;

    // ...

    return 0;
}
```