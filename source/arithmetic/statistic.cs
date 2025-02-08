Here is the C# code equivalent to the provided C code:

```csharp
// vips_statistic_scan_start method from im_avg.c
public abstract class VipsStatistic : VipsOperation
{
    public override void* ScanStart(VipsImage inImage, object a, object b)
    {
        VipsStatistic statistic = (VipsStatistic)a;
        VipsStatisticClass classType = (VipsStatisticClass)statistic.GetType().GetInterface(typeof(VipsStatisticClass));

        return classType.Start(statistic);
    }
}

// vips_statistic_scan method from im_avg.c
public abstract class VipsStatistic : VipsOperation
{
    public override int Scan(VipsRegion region, object seq, object a, object b, ref bool stop)
    {
        VipsStatistic statistic = (VipsStatistic)a;
        VipsStatisticClass classType = (VipsStatisticClass)statistic.GetType().GetInterface(typeof(VipsStatisticClass));

        VipsRect r = region.Valid;
        int lsk = VIPS_REGION_LSKIP(region);

        int y;
        VipsPel* p;

        VIPS_DEBUG_MSG("vips_statistic_scan: {0} x {1} @ {2} x {3}", r.Width, r.Height, r.Left, r.Top);

        p = (VipsPel*)VIPS_REGION_ADDR(region, r.Left, r.Top);
        for (y = 0; y < r.Height; y++)
        {
            if (classType.Scan(statistic, seq, r.Left, r.Top + y, p, r.Width))
                return -1;
            p += lsk;
        }

        // If we've requested stop, pass the message on.
        if (statistic.Stop)
            stop = true;

        return 0;
    }
}

// vips_statistic_scan_stop method from im_avg.c
public abstract class VipsStatistic : VipsOperation
{
    public override int ScanStop(object seq, object a, object b)
    {
        VipsStatistic statistic = (VipsStatistic)a;
        VipsStatisticClass classType = (VipsStatisticClass)statistic.GetType().GetInterface(typeof(VipsStatisticClass));

        return classType.Stop(statistic, seq);
    }
}

// vips_statistic_build method from im_avg.c
public abstract class VipsStatistic : VipsOperation
{
    public override int Build()
    {
        VipsStatistic statistic = (VipsStatistic)this;
        VipsStatisticClass sclass = (VipsStatisticClass)statistic.GetType().GetInterface(typeof(VipsStatisticClass));
        VipsImage[] t = new VipsImage[2];

#ifdef DEBUG
        Console.WriteLine("vips_statistic_build: " + this.Name);
#endif /*DEBUG*/

        if (base.Build())
            return -1;

        statistic.Ready = statistic.In;

        if (VipsImage.Decode(statistic.Ready, ref t[0]))
            return -1;
        statistic.Ready = t[0];

        // If there's a format table, cast the input.
        if (sclass.FormatTable != null)
        {
            if (VipsCast(statistic.Ready, ref t[1], sclass.FormatTable[statistic.In.BandFmt], null))
                return -1;
            statistic.Ready = t[1];
        }

        if (VipsSink(statistic.Ready,
            new VipsStatisticScanStart(statistic),
            new VipsStatisticScan(this),
            new VipsStatisticScanStop(this),
            statistic, null))
            return -1;

        return 0;
    }
}

// vips_statistic_class_init method from im_avg.c
public abstract class VipsStatisticClass : VipsOperationClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        VIPS_ARG_IMAGE(this, "in", 0,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsStatistic, In));
    }
}

// vips_statistic_init method from im_avg.c
public abstract class VipsStatistic : VipsOperation
{
    public override void Init()
    {
        base.Init();
    }
}
```

Note that I've assumed the existence of some classes and methods (e.g., `VipsImage`, `VipsRegion`, `VIPS_DEBUG_MSG`, etc.) which are not defined in this code snippet. You may need to modify the code to match your specific requirements.

Also, please note that C# does not have direct equivalents for all the C constructs used in the original code (e.g., `void*` pointers, `struct` types, etc.). I've tried to translate the code as closely as possible while still following good C# coding practices.