Here is the C# code equivalent to the provided C code:

```csharp
// vips_hough_new_accumulator
public static VipsImage* VipsHoughNewAccumulator(VipsHough hough)
{
    VipsHoughClass class = (VipsHoughClass)hough.GetType().GetBaseType();
    VipsStatistic statistic = (VipsStatistic)hough;

    VipsImage accumulator;

    // Create a new image in memory
    accumulator = new VipsImage();

    if (!vips_image_pipelinev(accumulator, VIPS_DEMAND_STYLE_ANY, statistic.Ready(), null) ||
        class.InitAccumulator(hough, accumulator) ||
        vips_image_write_prepare(accumulator))
    {
        // Clean up and return null on failure
        accumulator.Dispose();
        return null;
    }

    return accumulator;
}

// vips_hough_build
public int VipsHoughBuild(VipsObject object)
{
    VipsObjectClass class = (VipsObjectClass)object.GetType().GetBaseType();
    VipsStatistic statistic = (VipsStatistic)object;
    VipsHough hough = (VipsHough)object;

    // Check if input image is mono
    if (statistic.In() != null && vips_check_mono(class.Nickname(), statistic.In()))
        return -1;

    // Create a new accumulator
    VipsImage out = VipsHoughNewAccumulator(hough);

    if (out == null)
        return -1;

    object.SetProperty("out", out);
    out.Dispose();

    // Call parent class build method
    if (VIPS_OBJECT_CLASS(class).Build(object) != 0)
        return -1;

    return 0;
}

// vips_hough_start
public void* VipsHoughStart(VipsStatistic statistic)
{
    VipsHough hough = (VipsHough)statistic;

    // Create a new accumulator
    VipsImage accumulator = VipsHoughNewAccumulator(hough);

    if (accumulator == null)
        return null;

    return accumulator;
}

// vips_hough_stop
public int VipsHoughStop(VipsStatistic statistic, void* seq)
{
    VipsImage accumulator = (VipsImage)seq;
    VipsHough hough = (VipsHough)statistic;

    // Draw the accumulator onto the output image
    if (!vips_draw_image(hough.Out(), accumulator, 0, 0,
        "mode", VIPS_COMBINE_MODE_ADD,
        null))
    {
        // Clean up and return -1 on failure
        accumulator.Dispose();
        return -1;
    }

    // Clean up the accumulator
    accumulator.Dispose();

    return 0;
}

// vips_hough_scan
public int VipsHoughScan(VipsStatistic statistic, void* seq, int x, int y, void* in, int n)
{
    VipsHough hough = (VipsHough)statistic;
    VipsHoughClass class = (VipsHoughClass)hough.GetType().GetBaseType();
    VipsImage accumulator = (VipsImage)seq;
    VipsPel[] p = (VipsPel[])in;

    for (int i = 0; i < n; i++)
        if (p[i] != 0)
            class.Vote(hough, accumulator, x + i, y);

    return 0;
}

// vips_hough_format_table
private static readonly VipsBandFormat[] vipsHoughFormatTable = new VipsBandFormat[]
{
    // Band format: UC C US S UI I F X D DX
    // Promotion:
    VipsBandFormat.UC, VipsBandFormat.UC, VipsBandFormat.UC, VipsBandFormat.UC,
    VipsBandFormat.UC, VipsBandFormat.UC, VipsBandFormat.UC, VipsBandFormat.UC,
    VipsBandFormat.UC, VipsBandFormat.UC
};

// vips_hough_class_init
public static void VipsHoughClassInit(VipsHoughClass class)
{
    GObjectClass gobjectClass = (GObjectClass)class;
    VipsObjectClass objectClass = (VipsObjectClass)class;
    VipsStatisticClass sclass = (VipsStatisticClass)class;

    gobjectClass.SetProperty += vips_object_set_property;
    gobjectClass.GetProperty += vips_object_get_property;

    objectClass.Nickname = "hough";
    objectClass.Description = _("find hough transform");
    objectClass.Build = VipsHoughBuild;

    sclass.Start = VipsHoughStart;
    sclass.Scan = VipsHoughScan;
    sclass.Stop = VipsHoughStop;
    sclass.FormatTable = vipsHoughFormatTable;

    VIPS_ARG_IMAGE(class, "out", 100,
        _("Output"),
        _("Output image"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsHough, Out));
}

// vips_hough_init
public void VipsHoughInit(VipsHough hough)
{
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsImage`, `VipsPel`, `VIPS_DEMAND_STYLE_ANY`, etc.) which are not defined in this code snippet, but are likely part of a larger VIPS library.