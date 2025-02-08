```csharp
// im_gfadd:
//
// Deprecated.
public static int im_gfadd(double a, Image in1, double b, Image in2, double c, Image out)
{
    // fd, data filename must have been set before the function is called
    // Check whether they are set properly
    if (im_iocheck(in1, out) == -1 || im_iocheck(in2, out) == -1)
    {
        im_error("im_gfadd", " im_iocheck failed");
        return -1;
    }
    // Checks the arguments entered in in and prepares out
    if ((in1.Xsize != in2.Xsize) ||
        (in1.Ysize != in2.Ysize) ||
        (in1.Bands != in2.Bands) ||
        (in1.Coding != in2.Coding))
    {
        im_error("im_gfadd", " Input images differ");
        return -1;
    }
    if (in1.Coding != ImageCoding.None)
    {
        im_error("im_gfadd", " images are coded");
        return -1;
    }

    int first = GetBandFormatIndex(in1.BandFmt);
    int second = GetBandFormatIndex(in2.BandFmt);

    // Define the output
    int result = array[first, second];

    // Prepare output
    if (im_cp_desc(out, in1) == -1)
    {
        im_error("im_gfadd", " im_cp_desc failed");
        return -1;
    }
    out.BandFmt = GetBandFormat(result);
    if (im_setupout(out) == -1)
    {
        im_error("im_gfadd", " im_setupout failed");
        return -1;
    }

    // Order in1 and in2
    Image tmp1, tmp2;
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

    // Define what we do for each band element type.

#define loop(IN1, IN2, OUT) \
{ \
    IN1 *input1 = (IN1 *)tmp1.Data; \
    IN2 *input2 = (IN2 *)tmp2.Data; \

    int os = out.Xsize * out.Bands;
    double[] line = new double[os];

    switch (out.BandFmt)
    {
        case ImageBandFormat.Double:
            switch (tmp2.BandFmt)
            {
                case ImageBandFormat.UChar:
                    select_outdouble(unsigned char, double);
                    break;

                case ImageBandFormat.Char:
                    select_outdouble(signed char, double);
                    break;

                case ImageBandFormat.UShort:
                    select_outdouble(unsigned short, double);
                    break;

                case ImageBandFormat.Short:
                    select_outdouble(signed short, double);
                    break;

                case ImageBandFormat.UInt:
                    select_outdouble(unsigned int, double);
                    break;

                case ImageBandFormat.Int:
                    select_outdouble(signed int, double);
                    break;

                case ImageBandFormat.Float:
                    select_outdouble(float, double);
                    break;

                case ImageBandFormat.Double:
                    select_outdouble(double, double);
                    break;

                default:
                    im_error("im_gfadd", "Wrong tmp2 format(d)");
                    return -1;
            }

            break;

        case ImageBandFormat.Float:
            switch (tmp2.BandFmt)
            {
                case ImageBandFormat.UChar:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2uchar(unsigned char, float);

                    default:
                        im_error("im_gfadd", " Error (a)");
                        return -1;
                    }
                    break;

                case ImageBandFormat.Char:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2char(signed char, float);

                    default:
                        im_error("im_gfadd", " Error (b)");
                        return -1;
                    }
                    break;

                case ImageBandFormat.UShort:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2ushort(unsigned short, float);

                    default:
                        im_error("im_gfadd", " Error (c)");
                        return -1;
                    }
                    break;

                case ImageBandFormat.Short:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2short(signed short, float);

                    default:
                        im_error("im_gfadd", " Error (d)");
                        return -1;
                    }
                    break;

                case ImageBandFormat.UInt:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2uint(unsigned int, float);

                    default:
                        im_error("im_gfadd", " Error (e)");
                        return -1;
                    }
                    break;

                case ImageBandFormat.Int:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2int(signed int, float);

                    default:
                        im_error("im_gfadd", " Error (f)");
                        return -1;
                    }
                    break;

                case ImageBandFormat.Float:
                    switch (tmp1.BandFmt)
                    {
                        outfloat_2float(float, float);

                    default:
                        im_error("im_gfadd", " Error (g)");
                        return -1;
                    }
                    break;

                default:
                    im_error("im_gfadd", " Wrong tmp2 format(f)");
                    return -1;
            }

            break;

        default:
            im_error("im_gfadd", " Impossible output state");
            return -1;
    }

#undef loop

    for (int y = 0; y < out.Ysize; y++)
    {
        double[] cpline = new double[out.Xsize];
        IN1 *input1 = (IN1 *)tmp1.Data;
        IN2 *input2 = (IN2 *)tmp2.Data;

        for (int x = 0; x < os; x++)
            cpline[x] = (a * ((double)input1[x]) + b * ((double)input2[x]) + c);

        if (im_writeline(y, out, cpline))
        {
            im_error("im_gfadd", " im_writeline failed");
            return -1;
        }
    }

    return 0;
}

static int GetBandFormatIndex(ImageBandFormat bandFmt)
{
    switch (bandFmt)
    {
        case ImageBandFormat.UChar:
            return 0;

        case ImageBandFormat.Char:
            return 1;

        case ImageBandFormat.UShort:
            return 2;

        case ImageBandFormat.Short:
            return 3;

        case ImageBandFormat.UInt:
            return 4;

        case ImageBandFormat.Int:
            return 5;

        case ImageBandFormat.Float:
            return 6;

        case ImageBandFormat.Double:
            return 7;
    }

    throw new Exception("Invalid band format");
}

static ImageBandFormat GetBandFormat(int index)
{
    switch (index)
    {
        case 0:
            return ImageBandFormat.UChar;

        case 1:
            return ImageBandFormat.Char;

        case 2:
            return ImageBandFormat.UShort;

        case 3:
            return ImageBandFormat.Short;

        case 4:
            return ImageBandFormat.UInt;

        case 5:
            return ImageBandFormat.Int;

        case 6:
            return ImageBandFormat.Float;

        case 7:
            return ImageBandFormat.Double;
    }

    throw new Exception("Invalid band format index");
}

static void select_outdouble(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.UChar:
            loop(unsigned char, input2, output);
            break;

        case ImageBandFormat.Char:
            loop(signed char, input2, output);
            break;

        case ImageBandFormat.UShort:
            loop(unsigned short, input2, output);
            break;

        case ImageBandFormat.Short:
            loop(signed short, input2, output);
            break;

        case ImageBandFormat.UInt:
            loop(unsigned int, input2, output);
            break;

        case ImageBandFormat.Int:
            loop(signed int, input2, output);
            break;

        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;

        case ImageBandFormat.Double:
            loop(double, input2, output);
            break;
    }
}

static void outfloat_2uchar(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.Char:
            loop(signed char, input2, output);
            break;

        case ImageBandFormat.UShort:
            loop(unsigned short, input2, output);
            break;

        case ImageBandFormat.Short:
            loop(signed short, input2, output);
            break;

        case ImageBandFormat.UInt:
            loop(unsigned int, input2, output);
            break;

        case ImageBandFormat.Int:
            loop(signed int, input2, output);
            break;

        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;
    }
}

static void outfloat_2char(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.UShort:
            loop(unsigned short, input2, output);
            break;

        case ImageBandFormat.Short:
            loop(signed short, input2, output);
            break;

        case ImageBandFormat.UInt:
            loop(unsigned int, input2, output);
            break;

        case ImageBandFormat.Int:
            loop(signed int, input2, output);
            break;

        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;
    }
}

static void outfloat_2ushort(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.Short:
            loop(signed short, input2, output);
            break;

        case ImageBandFormat.UInt:
            loop(unsigned int, input2, output);
            break;

        case ImageBandFormat.Int:
            loop(signed int, input2, output);
            break;

        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;
    }
}

static void outfloat_2short(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.UInt:
            loop(unsigned int, input2, output);
            break;

        case ImageBandFormat.Int:
            loop(signed int, input2, output);
            break;

        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;
    }
}

static void outfloat_2uint(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.Int:
            loop(signed int, input2, output);
            break;

        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;
    }
}

static void outfloat_2int(IN2 input2, OUT output)
{
    switch (input2.BandFmt)
    {
        case ImageBandFormat.Float:
            loop(float, input2, output);
            break;
    }
}

static void outfloat_2float(IN2 input2, OUT output)
{
}
```