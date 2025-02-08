```csharp
// Converted from: vips_avg_build()

public class VipsAvg : VipsStatistic
{
    public double Sum { get; private set; }
    public double Out { get; private set; }

    protected override int Build(VipsObject obj)
    {
        var statistic = (VipsStatistic)obj;
        var avg = (VipsAvg)obj;

        if (base.Build(obj) != 0)
            return -1;

        var vals = (long)vipsImageGetWidth(statistic.In) *
                   vipsImageGetHeight(statistic.In) *
                   vipsImageGetBands(statistic.In);
        Out = Sum / vals;
        gObjectSetProperty(obj, "out", Out);

        return 0;
    }
}

// Converted from: vips_avg_start()

protected override void* Start(VipsStatistic statistic)
{
    return GNew0<double>(1);
}

// Converted from: vips_avg_stop()

protected override int Stop(VipsStatistic statistic, void* seq)
{
    var avg = (VipsAvg)statistic;
    var sum = (double)seq;

    avg.Sum += sum;

    gFree(seq);

    return 0;
}

// Converted from: vips_avg_scan()

protected override int Scan(VipsStatistic statistic, void* seq,
                            int x, int y, void* in, int n)
{
    const int sz = n * vipsImageGetBands(statistic.In);

    var sum = (double)seq;

    int i;
    double m;

    m = sum[0];

    switch (vipsImageGetFormat(statistic.In))
    {
        case VIPS_FORMAT_UCHAR:
            LOOP(unsigned char);
            break;
        case VIPS_FORMAT_CHAR:
            LOOP(signed char);
            break;
        case VIPS_FORMAT_USHORT:
            LOOP(unsigned short);
            break;
        case VIPS_FORMAT_SHORT:
            LOOP(signed short);
            break;
        case VIPS_FORMAT_UINT:
            LOOP(unsigned int);
            break;
        case VIPS_FORMAT_INT:
            LOOP(signed int);
            break;
        case VIPS_FORMAT_FLOAT:
            LOOP(float);
            break;
        case VIPS_FORMAT_DOUBLE:
            LOOP(double);
            break;
        case VIPS_FORMAT_COMPLEX:
            CLOOP(float);
            break;
        case VIPS_FORMAT_DPCOMPLEX:
            CLOOP(double);
            break;

        default:
            gAssertNotReached();
            break;
    }

    sum[0] = m;

    return 0;
}

// Converted from: vips_avg_class_init()

protected override void ClassInit(VipsAvgClass class)
{
    base.ClassInit(class);

    var sclass = (VipsStatisticClass)class;
    sclass.Start = Start;
    sclass.Scan = Scan;
    sclass.Stop = Stop;

    VIPS_ARG_DOUBLE(class, "out", 2,
        _("Output"),
        _("Output value"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsAvg, Out),
        double.MinValue, double.MaxValue, 0.0);
}

// Converted from: vips_avg_init()

protected override void Init(VipsAvg avg)
{
    base.Init(avg);
}

// Converted from: vips_avg()

public static int VipsAvg(VipsImage in, double[] out, params object[] args)
{
    var result = VipsCallSplit("avg", args, in, out);

    return result;
}
```