Here is the converted C# code:

```csharp
// vips__hist_sizealike_vec
public static int VipsHistogramSizealikeVec(VipsImage[] inArray, VipsImage[] outArray, int n)
{
    int i;
    int maxSize;

    g_assert(n >= 1);

    maxSize = Math.Max(inArray[0].Xsize, inArray[0].Ysize);
    for (i = 1; i < n; i++)
        maxSize = Math.Max(maxSize, Math.Max(inArray[i].Xsize, inArray[i].Ysize));

    for (i = 0; i < n; i++)
    {
        if (inArray[i].Ysize == 1)
        {
            if (!VipsEmbed(inArray[i], outArray[i], 0, 0, maxSize, 1, "extend", VIPS_EXTEND_COPY))
                return -1;
        }
        else
        {
            if (!VipsEmbed(inArray[i], outArray[i], 0, 0, 1, maxSize, "extend", VIPS_EXTEND_COPY))
                return -1;
        }
    }

    return 0;
}

// vips_histogram_build
public static int VipsHistogramBuild(VipsObject obj)
{
    VipsHistogram histogram = (VipsHistogram)obj;
    VipsObjectClass class = VIPS_OBJECT_GET_CLASS(obj);
    VipsHistogramClass hclass = VIPS_HISTOGRAM_GET_CLASS(histogram);

    VipsImage[] decode = new VipsImage[histogram.N];
    VipsImage[] format = new VipsImage[histogram.N];
    VipsImage[] band = new VipsImage[histogram.N];
    VipsImage[] size = new VipsImage[histogram.N];
    VipsImage[] memory = new VipsImage[histogram.N];

    VipsPel[] inbuf;
    int i;

#ifdef DEBUG
    Console.WriteLine("vips_histogram_build: " + obj.Name);
#endif /*DEBUG*/

    if (VIPS_OBJECT_CLASS(vips_histogram_parent_class).Build(obj))
        return -1;

    g_assert(histogram.N > 0);

    // Must be NULL-terminated.
    g_assert(!histogram.In[histogram.N]);

    for (i = 0; i < histogram.N; i++)
    {
        if (!VipsImageDecode(histogram.In[i], ref decode[i]) ||
            VipsCheckHist(class.Nickname, decode[i]))
            return -1;
    }

    // Cast our input images up to a common format, bands and size. If
    // input_format is set, cast to a fixed input type.
    if (hclass.InputFormat != VIPS_FORMAT_NOTSET)
    {
        for (i = 0; i < histogram.N; i++)
            if (!VipsCast(decode[i], ref format[i], hclass.InputFormat))
                return -1;
    }
    else
    {
        if (VipsFormatAlikeVec(decode, format, histogram.N))
            return -1;
    }

    if (VipsBandAlikeVec(class.Nickname, format, band, histogram.N, 1) ||
        VipsHistSizealikeVec(band, size, histogram.N))
        return -1;

    if (!VipsImagePipelineArray(histogram.Out, VIPS_DEMAND_STYLE_THINSTRIP, size))
        return -1;

    // Need a copy of the inputs in memory.
    if ((inbuf = new VipsPel[histogram.N + 1]) == null)
        return -1;
    for (i = 0; i < histogram.N; i++)
    {
        if (!(memory[i] = VipsImageCopyMemory(size[i])))
            return -1;
        inbuf[i] = VIPS_IMAGE_ADDR(memory[i], 0, 0);
    }
    inbuf[i] = null;

    // Keep a copy of the memory images here for subclasses.
    histogram.Ready = memory;

    histogram.Out.Xsize = VIPS_IMAGE_N_PELS(histogram.Ready[0]);
    histogram.Out.Ysize = 1;
    if (hclass.FormatTable != null)
        histogram.Out.BandFmt = hclass.FormatTable[histogram.Ready[0].BandFmt];
    histogram.Out.Type = VIPS_INTERPRETATION_HISTOGRAM;

    // Process the histogram
    hclass.Process(histogram, new VipsPel[VIPS_IMAGE_SIZEOF_LINE(histogram.Out)]);

    if (!VipsImageWriteLine(histogram.Out, 0))
        return -1;

    return 0;
}

// vips_histogram_class_init
public static void VipsHistogramClassInit(VipsHistogramClass class)
{
    GObjectClass gobjectClass = G_OBJECT_CLASS(class);
    VipsObjectClass vobjectClass = VIPS_OBJECT_CLASS(class);

    gobjectClass.SetProperty += VipsObjectSetProperty;
    gobjectClass.GetProperty += VipsObjectGetProperty;

    vobjectClass.Nickname = "histogram";
    vobjectClass.Description = _("histogram operations");
    vobjectClass.Build = VipsHistogramBuild;

    class.InputFormat = VIPS_FORMAT_NOTSET;

    // Inputs set by subclassess.
}

// vips_histogram_init
public static void VipsHistogramInit(VipsHistogram histogram)
{
    // Sanity check this above.
    histogram.N = -1;
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsImage`, `VipsObject`, `GObjectClass`) which are not defined in the provided C code, but are likely part of the VIPS library. You may need to modify the code to match your specific use case.

Also note that I've used the `ref` keyword to pass arrays by reference, as is common in C#. If you're using an older version of .NET that doesn't support this syntax, you can remove the `ref` keywords and use the `out` or `in` keywords instead.