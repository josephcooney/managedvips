```csharp
// measure_patches
static int MeasurePatches(VipsImage im, double[] coeff,
    int left, int top, int width, int height,
    int u, int v, int[] sel, int nsel)
{
    VipsImage tmp;
    int patch;
    int i, j;
    int m, n;
    double avg, dev;
    int x, y, w, h;

    // How large are the patches we are to measure?
    double pw = (double)width / (double)u;
    double ph = (double)height / (double)v;

    // Set up sub to be the size we need for a patch.
    w = (pw + 1) / 2;
    h = (ph + 1) / 2;

    // Loop through sel, picking out areas to measure.
    for (j = 0, patch = 0; patch < nsel; patch++)
    {
        // Sanity check. Is the patch number sensible?
        if (sel[patch] <= 0 || sel[patch] > u * v)
        {
            throw new ArgumentException("im_measure: patch " + sel[patch] + " is out of range");
            return 1;
        }

        // Patch coordinates.
        m = (sel[patch] - 1) % u;
        n = (sel[patch] - 1) / u;

        // Move sub to correct position.
        x = left + m * pw + (pw + 2) / 4;
        y = top + n * ph + (ph + 2) / 4;

        // Loop through bands.
        for (i = 0; i < im.Bands; i++, j++)
        {
            // Make temp buffer to extract to.
            if (!(tmp = VipsImage.NewFromBuffer("patch", "t")))
                return -1;

            // Extract and measure.
            if (!im.ExtractAreaBands(im, tmp, x, y, w, h, i, 1) ||
                !VipsImage.Average(tmp, out avg) ||
                !VipsImage.Deviate(tmp, out dev))
            {
                VipsObject.Unref(tmp);
                return -1;
            }
            VipsObject.Unref(tmp);

            // Is the deviation large compared with the average?
            // This could be a clue that our parameters have
            // caused us to miss the patch. Look out for averages
            // <0, or averages near zero (can get these if use
            // im_measure() on IM_TYPE_LAB images).
            if (dev * 5 > Math.Abs(avg) && Math.Abs(avg) > 3)
                throw new ArgumentException("im_measure: patch " + patch + ", band " + i + ": avg = " + avg + ", sdev = " + dev);

            // Save results.
            coeff[j] = avg;
        }
    }

    return 0;
}

// internal_im_measure_area
static VipsDoubleMask InternalImMeasureArea(VipsImage im,
    int left, int top, int width, int height,
    int u, int v,
    int[] sel, int nsel, string name)
{
    VipsDoubleMask mask;

    if (im.CheckUncoded("im_measure", im) ||
        im.CheckNonComplex("im_measure", im))
        return null;

    // Default to all patches if sel == NULL.
    if (sel == null)
    {
        int i;

        nsel = u * v;
        if (!(sel = new int[nsel]))
            return null;
        for (i = 0; i < nsel; i++)
            sel[i] = i + 1;
    }

    // What size mask do we need?
    if (!(mask = VipsDoubleMask.New(name, im.Bands, nsel)))
        return null;

    // Perform measure and return.
    if (MeasurePatches(im, mask.Coeff, left, top, width, height,
            u, v, sel, nsel))
    {
        VipsObject.Unref(mask);
        return null;
    }

    return mask;
}

// im_measure_area
static VipsDoubleMask ImMeasureArea(VipsImage im,
    int left, int top, int width, int height,
    int u, int v,
    int[] sel, int nsel, string name)
{
    VipsDoubleMask mask;
    VipsImage t;

    // The old im_measure() worked on labq.
    if (im.Coding == VipsImage.LabQ)
    {
        if (!(t = VipsImage.NewFromBuffer("measure-temp", "p")))
            return null;
        if (!VipsImage.LabQ2Lab(im, t) ||
            !ImMeasureArea(t,
                left, top, width, height,
                u, v,
                sel, nsel, name))
        {
            VipsObject.Unref(t);
            return null;
        }
        VipsObject.Unref(t);

        return mask;
    }

    if (sel != null)
        return InternalImMeasureArea(im,
            left, top, width, height, u, v, sel, nsel, name);
    else
    {
        if (!Vips.Measure(im, out t, u, v,
                "left", left,
                "top", top,
                "width", width,
                "height", height,
                null))
            return null;
        if (!(mask = Vips.ImageToDoubleMask(t, name)))
        {
            VipsObject.Unref(t);
            return null;
        }
        VipsObject.Unref(t);

        return mask;
    }
}
```