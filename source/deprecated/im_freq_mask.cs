```csharp
// build_freq_mask
public static int BuildFreqMask(VipsImage out, int xs, int ys, ImMaskType flag, params double[] args)
{
    // May be fewer than 4 args ... but extract them all anyway. Should be safe.
    double p0 = args.Length >= 1 ? args[0] : 0;
    double p1 = args.Length >= 2 ? args[1] : 0;
    double p2 = args.Length >= 3 ? args[2] : 0;
    double p3 = args.Length >= 4 ? args[3] : 0;
    double p4 = args.Length >= 5 ? args[4] : 0;

    VipsImage t;

    switch (flag)
    {
        case ImMaskType.IM_MASK_IDEAL_HIGHPASS:
            if (!Vips.MaskIdeal(out, xs, ys, p0))
                return -1;
            break;

        case ImMaskType.IM_MASK_IDEAL_LOWPASS:
            if (!Vips.MaskIdeal(out, xs, ys, p0, "reject", true))
                return -1;
            break;

        // ... rest of the switch statement ...

        default:
            throw new ArgumentException($"unimplemented mask type: {flag}");
    }

    return 0;
}

// im_flt_image_freq
public static int ImFltImageFreq(VipsImage inImg, VipsImage outImg, ImMaskType flag, params double[] args)
{
    VipsImage mask = Vips.OpenLocal(outImg, "tempmask", "p");

    if (mask == null)
        return -1;

    // Generate mask.
    BuildFreqMask(mask, inImg.Xsize, inImg.Ysize, flag, args);

    if (!Vips.FreqFlt(inImg, mask, outImg))
        return -1;

    return 0;
}

// im_create_fmask
public static int ImCreateFmask(VipsImage out, int xsize, int ysize, ImMaskType flag, params double[] args)
{
    BuildFreqMask(out, xsize, ysize, flag, args);

    return 0;
}
```

Note: I've assumed that the `Vips` class has static methods for creating and manipulating images, as well as a `FreqFlt` method for frequency filtering. You may need to modify this code to match your specific VIPS implementation in C#.