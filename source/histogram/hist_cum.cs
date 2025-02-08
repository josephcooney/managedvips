```csharp
// vips_hist_cum_process (from vips_hist_cum.c)
public static void Process(VipsHistogram histogram, VipsPel[] out, VipsPel[][] in, int width)
{
    const int bands = histogram.Ready[0].Bands;
    const int nb =
        VipsBandFormat.IsComplex(histogram.Ready[0].BandFmt)
        ? bands * 2
        : bands;
    int mx = width * nb;

    int x, b;

    switch (histogram.Ready[0].Format)
    {
        case VIPS_FORMAT_CHAR:
            Accumulate(signed char, signed int);
            break;
        case VIPS_FORMAT_UCHAR:
            Accumulate(unsigned char, unsigned int);
            break;
        case VIPS_FORMAT_SHORT:
            Accumulate(signed short, signed int);
            break;
        case VIPS_FORMAT_USHORT:
            Accumulate(unsigned short, unsigned int);
            break;
        case VIPS_FORMAT_INT:
            Accumulate(signed int, signed int);
            break;
        case VIPS_FORMAT_UINT:
            Accumulate(unsigned int, unsigned int);
            break;

        case VIPS_FORMAT_FLOAT:
        case VIPS_FORMAT_COMPLEX:
            Accumulate(float, float);
            break;
        case VIPS_FORMAT_DOUBLE:
        case VIPS_FORMAT_DPCOMPLEX:
            Accumulate(double, double);
            break;

        default:
            throw new ArgumentException("Unsupported format");
    }
}

// ACCUMULATE macro (from vips_hist_cum.c)
#define ACCUMULATE(ITYPE, OTYPE) \
{ \
    for (b = 0; b < nb; b++) { \
        ITYPE *p = (ITYPE *) in[0]; \
        OTYPE *q = (OTYPE *) out; \
        OTYPE total; \

static void Accumulate<T1, T2>(T1 p, T2 q)
{
    int x;
    for (x = 0; x < p.Length; x++)
    {
        q[x] += p[x];
    }
}

// vips_hist_cum_class_init (from vips_hist_cum.c)
public static void ClassInit(VipsHistCumClass class)
{
    VipsObjectClass object_class = (VipsObjectClass)class;
    VipsHistogramClass hclass = VIPS_HISTOGRAM_CLASS(class);

    object_class.Nickname = "hist_cum";
    object_class.Description = "form cumulative histogram";

    hclass.FormatTable = new VipsBandFormat[] {
        // Band format:  UC  C  US  S  UI  I  F  X  D  DX
        // Promotion:
        VIPS_FORMAT_UINT, VIPS_FORMAT_INT, VIPS_FORMAT_UINT,
        VIPS_FORMAT_INT, VIPS_FORMAT_UINT, VIPS_FORMAT_INT,
        VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT, VIPS_FORMAT_DOUBLE,
        VIPS_FORMAT_DOUBLE
    };
    hclass.Process = Process;
}

// vips_hist_cum_init (from vips_hist_cum.c)
public static void Init(VipsHistCum hist_cum)
{
}

// vips_hist_cum (from vips_hist_cum.c)
public static int HistCum(VipsImage in, VipsImage[] out)
{
    return VipsCallSplit("hist_cum", in, out);
}
```