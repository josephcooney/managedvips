```csharp
// Converted from: vips_hist_plot_vert_gen()

public static int VipsHistPlotVertGen(VipsRegion out_region,
    void* seq, void* a, void* b, bool* stop)
{
    VipsImage in = (VipsImage)a;
    VipsRect r = out_region.Valid;
    int le = r.Left;
    int to = r.Top;
    int ri = VIPS_RECT_RIGHT(r);
    int bo = VIPS_RECT_BOTTOM(r);
    int nb = in.Bands;

    for (int y = to; y < bo; y++)
    {
        VipsPel[] q = new VipsPel[VIPS_REGION_LSKIP(out_region)];
        VipsPel[] p = new VipsPel[in.Xsize];

        switch (in.Format)
        {
            case VipsFormat.UChar:
                Vert<unsigned char>(p, q, le, ri, nb);
                break;
            case VipsFormat.Char:
                Vert<sbyte>(p, q, le, ri, nb);
                break;
            case VipsFormat.UInt16:
                Vert<ushort>(p, q, le, ri, nb);
                break;
            case VipsFormat.Int16:
                Vert<short>(p, q, le, ri, nb);
                break;
            case VipsFormat.UInt32:
                Vert<uint>(p, q, le, ri, nb);
                break;
            case VipsFormat.Int32:
                Vert<int>(p, q, le, ri, nb);
                break;
            case VipsFormat.Float32:
                Vert<float>(p, q, le, ri, nb);
                break;
            case VipsFormat.Float64:
                Vert<double>(p, q, le, ri, nb);
                break;

            default:
                throw new Exception("Invalid format");
        }
    }

    return 0;
}

// Converted from: vips_hist_plot_horz_gen()

public static int VipsHistPlotHorzGen(VipsRegion out_region,
    void* seq, void* a, void* b, bool* stop)
{
    VipsImage in = (VipsImage)a;
    VipsRect r = out_region.Valid;
    int le = r.Left;
    int to = r.Top;
    int ri = VIPS_RECT_RIGHT(r);
    int bo = VIPS_RECT_BOTTOM(r);
    int nb = in.Bands;
    int lsk = VIPS_REGION_LSKIP(out_region);
    int ht = out_region.Image.Ysize;

    for (int x = le; x < ri; x++)
    {
        VipsPel[] q = new VipsPel[VIPS_REGION_LSKIP(out_region)];
        VipsPel[] p = new VipsPel[in.Bands];

        switch (in.Format)
        {
            case VipsFormat.UChar:
                Horz<unsigned char>(p, q, to, bo, nb);
                break;
            case VipsFormat.Char:
                Horz<sbyte>(p, q, to, bo, nb);
                break;
            case VipsFormat.UInt16:
                Horz<ushort>(p, q, to, bo, nb);
                break;
            case VipsFormat.Int16:
                Horz<short>(p, q, to, bo, nb);
                break;
            case VipsFormat.UInt32:
                Horz<uint>(p, q, to, bo, nb);
                break;
            case VipsFormat.Int32:
                Horz<int>(p, q, to, bo, nb);
                break;
            case VipsFormat.Float32:
                Horz<float>(p, q, to, bo, nb);
                break;
            case VipsFormat.Float64:
                Horz<double>(p, q, to, bo, nb);
                break;

            default:
                throw new Exception("Invalid format");
        }
    }

    return 0;
}

// Converted from: vips_hist_plot_build()

public class VipsHistPlot : VipsObject
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }

    protected override int Build()
    {
        // ...
    }
}

// Converted from: vips_hist_plot_class_init()

public class VipsHistPlotClass : VipsObjectClass
{
    public static void ClassInit(VipsHistPlotClass* class)
    {
        // ...
    }
}

// Converted from: vips_hist_plot_init()

public class VipsHistPlot : VipsObject
{
    public VipsHistPlot()
    {
        // ...
    }
}

// Converted from: vips_hist_plot()

public static int VipsHistPlot(VipsImage in, ref VipsImage out)
{
    // ...
}
```