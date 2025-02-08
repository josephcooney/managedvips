Here is the converted C# code:

```csharp
// im_vips2mask
public static DOUBLEMASK* im_vips2mask(IMAGE in, string filename)
{
    int width, height;
    DOUBLEMASK* out;

    // double* only: cast if necessary.
    if (in.BandFmt != IM_BANDFMT_DOUBLE)
    {
        IMAGE t = im_open("im_vips2mask", "p");
        if (t == null)
            return null;
        if (!im_clip2fmt(in, t, IM_BANDFMT_DOUBLE) || !(out = im_vips2mask(t, filename)))
        {
            im_close(t);
            return null;
        }
        im_close(t);

        return out;
    }

    // Check the image.
    if (im_incheck(in) || im_check_uncoded("im_vips2mask", in))
        return null;

    if (in.Bands == 1)
    {
        width = in.Xsize;
        height = in.Ysize;
    }
    else if (in.Xsize == 1)
    {
        width = in.Bands;
        height = in.Ysize;
    }
    else if (in.Ysize == 1)
    {
        width = in.Xsize;
        height = in.Bands;
    }
    else
    {
        im_error("im_vips2mask", "%s", _("one band, nx1, or 1xn images only"));
        return null;
    }

    if (out = im_create_dmask(filename, width, height) == null)
        return null;

    if (in.Bands > 1 && in.Ysize == 1)
    {
        double[] data = (double[])in.data;
        int x, y;

        // Need to transpose: the image is RGBRGBRGB, we need RRRGGGBBB.
        for (y = 0; y < height; y++)
            for (x = 0; x < width; x++)
                out.coeff[x + y * width] = data[x * height + y];
    }
    else
        Array.Copy(in.data, out.coeff, width * height * sizeof(double));

    out.scale = vips_image_get_scale(in);
    out.offset = vips_image_get_offset(in);

    return out;
}

// im_vips2imask
public static INTMASK* im_vips2imask(IMAGE in, string filename)
{
    int width, height;
    INTMASK* out;

    double[] data;
    int x, y;
    double double_result;
    int int_result;

    // double* only: cast if necessary.
    if (in.BandFmt != IM_BANDFMT_DOUBLE)
    {
        IMAGE t = im_open("im_vips2imask", "p");
        if (t == null)
            return null;
        if (!im_clip2fmt(in, t, IM_BANDFMT_DOUBLE) || !(out = im_vips2imask(t, filename)))
        {
            im_close(t);
            return null;
        }
        im_close(t);

        return out;
    }

    // Check the image.
    if (im_incheck(in) || im_check_uncoded("im_vips2imask", in))
        return null;

    if (in.Bands == 1)
    {
        width = in.Xsize;
        height = in.Ysize;
    }
    else if (in.Xsize == 1)
    {
        width = in.Bands;
        height = in.Ysize;
    }
    else if (in.Ysize == 1)
    {
        width = in.Xsize;
        height = in.Bands;
    }
    else
    {
        im_error("im_vips2imask", "%s", _("one band, nx1, or 1xn images only"));
        return null;
    }

    data = (double[])in.data;

    if (!(out = im_create_imask(filename, width, height)))
        return null;

    // We want to make an intmask which has the same input to output ratio
    // as the double image.
    //
    // Imagine convolving with the double image, what's the ratio of
    // brightness between input and output? We want the same ratio for the
    // int version, if we can.
    //
    // Imagine an input image where every pixel is 1, what will the output
    // be?
    double_result = 0;
    for (y = 0; y < height; y++)
        for (x = 0; x < width; x++)
            double_result += data[x + width * y];
    double_result /= vips_image_get_scale(in);

    for (y = 0; y < height; y++)
        for (x = 0; x < width; x++)
            if (in.Bands > 1 && in.Ysize == 1)
                // Need to transpose: the image is RGBRGBRGB,
                // we need RRRGGGBBB.
                out.coeff[x + y * width] = VIPS_RINT(data[x * height + y]);
            else
                out.coeff[x + y * width] = VIPS_RINT(data[x + y * width]);

    out.scale = VIPS_RINT(vips_image_get_scale(in));
    if (out.scale == 0)
        out.scale = 1;
    out.offset = VIPS_RINT(vips_image_get_offset(in));

    // Now convolve a 1 everywhere image with the int version we've made,
    // what do we get?
    int_result = 0;
    for (y = 0; y < height; y++)
        for (x = 0; x < width; x++)
            int_result += out.coeff[x + width * y];
    int_result /= out.scale;

    // And adjust the scale to get as close to a match as we can.
    out.scale = VIPS_RINT(out.scale + (int_result - double_result));
    if (out.scale == 0)
        out.scale = 1;

    return out;
}
```