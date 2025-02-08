Here is the C# code equivalent to the provided C code:

```csharp
// vips__quantise_attr_create()
public VipsQuantiseAttr CreateQuantiseAttribute()
{
    return new VipsQuantiseAttr();
}

// vips__quantise_set_max_colors()
public VipsQuantiseError SetMaxColors(VipsQuantiseAttr attr, int colors)
{
    return liq.SetMaxColors(attr, colors);
}

// vips__quantise_set_quality()
public VipsQuantiseError SetQuality(VipsQuantiseAttr attr, int minimum, int maximum)
{
    return liq.SetQuality(attr, minimum, maximum);
}

// vips__quantise_set_speed()
public VipsQuantiseError SetSpeed(VipsQuantiseAttr attr, int speed)
{
    return liq.SetSpeed(attr, speed);
}

// vips__quantise_image_create_rgba()
public VipsQuantiseImage CreateQuantiseImageRGBA(VipsQuantiseAttr attr, IntPtr bitmap, int width, int height, double gamma)
{
    return liq.ImageCreateRGBA(attr, bitmap, width, height, gamma);
}

// vips__quantise_image_quantize()
public VipsQuantiseError QuantiseImage(VipsQuantiseImage inputImage, VipsQuantiseAttr options, out VipsQuantiseResult resultOutput)
{
    return liq.ImageQuantize(inputImage, options, out resultOutput);
}

// vips__quantise_image_quantize_fixed()
public VipsQuantiseError QuantiseImageFixed(VipsQuantiseImage inputImage, VipsQuantiseAttr options, out VipsQuantiseResult resultOutput)
{
    int i;
    liq.Result result;
    const liq.Palette palette;
    liq.Error err;
    liq.Image fakeImage;
    byte[] fakeImagePixels = new byte[4];

    // First, quantize the image and get its palette.
    err = liq.ImageQuantize(inputImage, options, out result);
    if (err != liq.OK)
        return err;

    palette = liq.GetPalette(result);

    // Now, we need a fake 1 pixel image that will be quantized on the next step. Its pixel color doesn't matter since we'll add all the colors from the palette further.
    fakeImage = liq.ImageCreateRGBA(options, fakeImagePixels, 1, 1, 0);
    if (fakeImage == null)
    {
        liq.ResultDestroy(result);
        return liq.OUT_OF_MEMORY;
    }

    // Add all the colors from the palette as fixed colors to the fake image. Since the fixed colors number is the same as required colors number, no new colors will be added.
    for (i = 0; i < palette.Count; i++)
        liq.ImageAddFixedColor(fakeImage, palette.Entries[i]);

    liq.ResultDestroy(result);

    // Finally, quantize the fake image with fixed colors to make a VipsQuantiseResult with a fixed palette.
    err = liq.ImageQuantize(fakeImage, options, out resultOutput);

    liq.ImageDestroy(fakeImage);

    return err;
}

// vips__quantise_set_dithering_level()
public VipsQuantiseError SetDitheringLevel(VipsQuantiseResult res, float ditherLevel)
{
    return liq.SetDitheringLevel(res, ditherLevel);
}

// vips__quantise_get_palette()
public VipsQuantisePalette GetPalette(VipsQuantiseResult result)
{
    return liq.GetPalette(result);
}

// vips__quantise_write_remapped_image()
public VipsQuantiseError WriteRemappedImage(VipsQuantiseResult result, VipsQuantiseImage inputImage, IntPtr buffer, int bufferSize)
{
    return liq.WriteRemappedImage(result, inputImage, buffer, bufferSize);
}

// vips__quantise_result_destroy()
public void DestroyQuantiseResult(VipsQuantiseResult result)
{
    liq.ResultDestroy(result);
}

// vips__quantise_image_destroy()
public void DestroyQuantiseImage(VipsQuantiseImage img)
{
    liq.ImageDestroy(img);
}

// vips__quantise_attr_destroy()
public void DestroyQuantiseAttribute(VipsQuantiseAttr attr)
{
    liq.AttrDestroy(attr);
}

// Quantise struct
[StructLayout(LayoutKind.Sequential)]
public struct Quantise
{
    public VipsImage In;
    public VipsImage[] IndexOut;
    public VipsImage[] PaletteOut;
    public int Colours;
    public int Q;
    public double Dither;
    public int Effort;

    public VipsQuantiseAttr Attr;
    public VipsQuantiseImage InputImage;
    public VipsQuantiseResult QuantisationResult;
    public VipsImage[] T;
}

// vips__quantise_free()
public void FreeQuantise(Quantise quantise)
{
    if (quantise.IndicesOut != null)
        foreach (VipsImage index in quantise.IndexOut)
            VIPS.Unref(index);

    if (quantise.PaletteOut != null)
        foreach (VipsImage palette in quantise.PaletteOut)
            VIPS.Unref(palette);

    VIPS.Unref(quantise.InputImage);
    VIPS.Unref(quantise.Attr);
    VIPS.Unref(quantise.QuantisationResult);

    for (int i = 0; i < VIPS.Number(quantise.T); i++)
        VIPS.Unref(quantise.T[i]);

    GCHandle handle = GCHandle.Alloc(quantise, GCHandleType.Pinned);
    Marshal.FreeHGlobal(handle.AddrOfPinnedObject());
}

// vips__quantise_new()
public Quantise NewQuantise(VipsImage inImage, out VipsImage[] indexOut, out VipsImage[] paletteOut, int colours, int Q, double dither, int effort)
{
    Quantise quantise = new Quantise();
    quantise.In = inImage;
    quantise.IndexOut = indexOut;
    quantise.PaletteOut = paletteOut;
    quantise.Colours = colours;
    quantise.Q = Q;
    quantise.Dither = dither;
    quantise.Effort = effort;

    return quantise;
}

// vips__quantise_image()
public int QuantiseImage(VipsImage inImage, out VipsImage[] indexOut, out VipsImage[] paletteOut, int colours, int Q, double dither, int effort, bool thresholdAlpha)
{
    Quantise quantise = NewQuantise(inImage, out indexOut, out paletteOut, colours, Q, dither, effort);

    // Ensure input is sRGB.
    if (inImage.Type != VIPS.Interpretation.sRGB)
    {
        VipsImage newIn;
        if (!VIPS.Colourspace(inImage, out newIn, VIPS.Interpretation.sRGB))
        {
            FreeQuantise(quantise);
            return -1;
        }
        inImage = newIn;
    }

    // Add alpha channel if missing.
    bool addedAlpha = false;
    if (!inImage.HasAlpha)
    {
        VipsImage newIn;
        if (!VIPS.BandJoinConst1(inImage, out newIn, 255))
        {
            FreeQuantise(quantise);
            return -1;
        }
        inImage = newIn;
        addedAlpha = true;
    }

    // Threshold alpha channel.
    if (thresholdAlpha && !addedAlpha)
    {
        const ulong nPels = VIPS.ImageNPels(inImage);

        byte[] pixels = new byte[nPels * 4];
        GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
        for (ulong i = 0; i < nPels; i++)
        {
            pixels[i * 4 + 3] = pixels[i * 4 + 3] > 128 ? 255 : 0;
        }
    }

    quantise.Attr = CreateQuantiseAttribute();
    SetMaxColors(quantise.Attr, colours);
    SetQuality(quantise.Attr, 0, Q);
    SetSpeed(quantise.Attr, 11 - effort);

    quantise.InputImage = CreateQuantiseImageRGBA(quantise.Attr, VIPS.ImageAddr(inImage, 0, 0), inImage.Xsize, inImage.Ysize, 0);

    if (QuantiseImage(quantise.InputImage, quantise.Attr, out quantise.QuantisationResult))
    {
        FreeQuantise(quantise);
        return -1;
    }

    SetDitheringLevel(quantise.QuantisationResult, dither);

    VipsImage index = quantise.T[3] = VIPS.ImageNewMemory();
    if (!VIPS.ImageWritePrepare(index))
    {
        FreeQuantise(quantise);
        return -1;
    }

    if (WriteRemappedImage(quantise.QuantisationResult, quantise.InputImage, VIPS.ImageAddr(index, 0, 0), VIPS.ImageNPels(index)))
    {
        FreeQuantise(quantise);
        return -1;
    }

    VipsQuantisePalette lp = GetPalette(quantise.QuantisationResult);

    VipsImage palette = quantise.T[4] = VIPS.ImageNewMemory();
    if (!VIPS.ImageWritePrepare(palette))
    {
        FreeQuantise(quantise);
        return -1;
    }

    byte[] pixels = new byte[lp.Count * 4];
    GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
    for (int i = 0; i < lp.Count; i++)
    {
        pixels[i * 4 + 0] = lp.Entries[i].R;
        pixels[i * 4 + 1] = lp.Entries[i].G;
        pixels[i * 4 + 2] = lp.Entries[i].B;
        pixels[i * 4 + 3] = lp.Entries[i].A;

        ptr += 4;
    }

    indexOut[0] = index;
    GCHandle handleIndex = GCHandle.Alloc(index, GCHandleType.Pinned);
    paletteOut[0] = palette;
    GCHandle handlePalette = GCHandle.Alloc(palette, GCHandleType.Pinned);

    FreeQuantise(quantise);

    return 0;
}
```

Note that this code assumes the existence of certain classes and methods (e.g. `VipsImage`, `VIPS.ImageAddr`, `VIPS.ImageNPels`, etc.) which are not defined in the provided C code. These classes and methods would need to be implemented separately.

Also, note that I've used `GCHandle` to pin the memory of the arrays, as required by the `Marshal.UnsafeAddrOfPinnedArrayElement` method. This is necessary because we're using pointers to access the array elements.

Finally, please keep in mind that this code is a direct translation from C to C#, and may not be optimal or idiomatic C# code. It's intended to provide a clear understanding of how the original C code works, rather than being a production-ready implementation.