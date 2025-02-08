Here is the C# equivalent of the provided C code:

```csharp
// im_gaddim: Generalised addition of two vasari images.
// Inputs, outputs are neither float nor double
// Result at each point is a*in1 + b*in2 + c
// Result depends on inputs, rounding is carried out;
// Function im_gaddim() assumes that the both input files
// are either memory mapped or in a buffer.
// Images must have the same no of bands and must not be complex
// No check for overflow is done;

public static int im_gaddim(double a, Image in1, double b, Image in2, double c, Image out)
{
    // array to map input formats to output formats
    static int[] fmt = { (int)ImageBandFormat.UChar, (int)ImageBandFormat.Char,
        (int)ImageBandFormat.UShort, (int)ImageBandFormat.SShort,
        (int)ImageBandFormat.UInt, (int)ImageBandFormat.Int };

    // variables to hold the input and output image formats
    int first, second, result;
    Image tmp1, tmp2;

    // check if the input images are valid
    if (!im_iocheck(in1, out) || !im_iocheck(in2, out))
        return -1;

    // check if the input images have the same size and number of bands
    if (in1.Xsize != in2.Xsize ||
        in1.Ysize != in2.Ysize ||
        in1.Bands != in2.Bands)
    {
        throw new ArgumentException("Input images differ");
    }

    // check if the input images are uncoded
    if (in1.Coding != ImageCoding.None || in2.Coding != ImageCoding.None)
    {
        throw new ArgumentException("Images must be uncoded");
    }

    // determine the input and output image formats
    switch (in1.BandFmt)
    {
        case (int)ImageBandFormat.UChar:
            first = 0;
            break;
        case (int)ImageBandFormat.Char:
            first = 1;
            break;
        case (int)ImageBandFormat.UShort:
            first = 2;
            break;
        case (int)ImageBandFormat.SShort:
            first = 3;
            break;
        case (int)ImageBandFormat.UInt:
            first = 4;
            break;
        case (int)ImageBandFormat.Int:
            first = 5;
            break;
        default:
            throw new ArgumentException("Unable to accept image1");
    }

    switch (in2.BandFmt)
    {
        case (int)ImageBandFormat.UChar:
            second = 0;
            break;
        case (int)ImageBandFormat.Char:
            second = 1;
            break;
        case (int)ImageBandFormat.UShort:
            second = 2;
            break;
        case (int)ImageBandFormat.SShort:
            second = 3;
            break;
        case (int)ImageBandFormat.UInt:
            second = 4;
            break;
        case (int)ImageBandFormat.Int:
            second = 5;
            break;
        default:
            throw new ArgumentException("Unable to accept image2");
    }

    // determine the output format
    result = fmt[first][second];

    // copy the input image description to the output image
    if (!im_cp_desc(out, in1))
        throw new Exception("im_cp_desc failed");

    out.BandFmt = (ImageBandFormat)result;

    // set up the output image
    if (!im_setupout(out))
        throw new Exception("im_setupout failed");

    // order the input images
    if (first >= second)
    {
        tmp1 = in1;
        tmp2 = in2;
    }
    else
    {
        tmp1 = in2;
        tmp2 = in1;
    }

    // perform the addition for each band element type
    switch ((int)out.BandFmt)
    {
        case (int)ImageBandFormat.Int:
            select_tmp2_for_out_int(tmp2, out);
            break;

        case (int)ImageBandFormat.UInt:
            select_tmp2_for_out_int((uint)tmp2.Data[0]);
            break;

        case (int)ImageBandFormat.SShort:
            select_tmp1_for_out_short((short)tmp1.Data[0], out);
            break;

        case (int)ImageBandFormat.UShort:
            select_tmp1_for_out_short((ushort)tmp1.Data[0], out);
            break;
    }

    return 0;
}

// select the temporary image format for output
private static void select_tmp2_for_out_int(Image tmp2, Image out)
{
    switch ((int)out.BandFmt)
    {
        case (int)ImageBandFormat.UChar:
            select_tmp1_for_out_int((uint)tmp2.Data[0], out);
            break;
        case (int)ImageBandFormat.Char:
            select_tmp1_for_out_int((sbyte)tmp2.Data[0], out);
            break;
        case (int)ImageBandFormat.UShort:
            select_tmp1_for_out_int((ushort)tmp2.Data[0], out);
            break;
        case (int)ImageBandFormat.SShort:
            select_tmp1_for_out_int((short)tmp2.Data[0], out);
            break;
        case (int)ImageBandFormat.UInt:
            select_tmp1_for_out_int((uint)tmp2.Data[0], out);
            break;
        case (int)ImageBandFormat.Int:
            select_tmp1_for_out_int((int)tmp2.Data[0], out);
            break;
    }
}

// select the temporary image format for output
private static void select_tmp1_for_out_short(short in2, Image out)
{
    switch ((int)out.BandFmt)
    {
        case (int)ImageBandFormat.UChar:
            loop((uint)in2, out);
            break;
        case (int)ImageBandFormat.Char:
            loop((sbyte)in2, out);
            break;
        case (int)ImageBandFormat.UShort:
            loop((ushort)in2, out);
            break;
        case (int)ImageBandFormat.SShort:
            loop(in2, out);
            break;
    }
}

// perform the addition for each band element type
private static void loop(short in1, Image out)
{
    short[] input1 = (short[])tmp1.Data;
    short[] input2 = (short[])tmp2.Data;

    for (int y = 0; y < out.Ysize; y++)
    {
        double[] cpline = new double[out.Xsize];
        for (int x = 0; x < out.Xsize; x++)
            cpline[x] = (double)(a * input1[y * out.Xsize + x] + b * input2[y * out.Xsize + x] + c + 0.5);

        if (!im_writeline(y, out, cpline))
            throw new Exception("im_writeline failed");
    }
}
```