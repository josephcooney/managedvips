Here is the C# equivalent of the provided C code:

```csharp
using System;

public class ImageResizer
{
    // im_lowpass()
    public static int ImLowPass(IMAGE inImage, IMAGE outImage)
    {
        double dx, dy, xscale, yscale;
        double Xnew, Ynew; /* inv. coord. of the interpolated pt */

        int x, y;
        int Xint, Yint;
        int bb;

        PEL input = (PEL)inImage.data;
        PEL opline = new PEL[outImage.Xsize * IM_IMAGE_SIZEOF_ELEMENT(outImage)];
        PEL q, p;

        int ils, ips, ies; /* Input and output line, pel and */
        int ols, oes;       /* element sizes */

        if (ImIoCheck(inImage, outImage))
            return -1;
        if (inImage.BandFmt != IM_BANDFMT_COMPLEX)
        {
            ImError("im_lowpass", "%s", "non-complex input only");
            return -1;
        }
        if (inImage.Coding != IM_CODING_NONE)
        {
            ImError("im_lowpass: ", "%s", "put should be uncoded");
            return -1;
        }
        if (ImCpDesc(outImage, inImage))
            return -1;

        outImage.Xsize = inImage.Xsize;
        outImage.Ysize = inImage.Ysize;

        if (ImSetupOut(outImage))
            return -1;

        ils = IM_IMAGE_SIZEOF_LINE(inImage);
        ips = IM_IMAGE_SIZEOF_PEL(inImage);
        ies = IM_IMAGE_SIZEOF_ELEMENT(inImage);

        ols = IM_IMAGE_SIZEOF_LINE(outImage);
        oes = IM_IMAGE_SIZEOF_ELEMENT(outImage);

        // buffer lines
        for (y = 0; y < outImage.Ysize; y++)
        {
            q = opline;
            for (x = 0; x < outImage.Xsize; x++)
            {
                Xnew = x * ((double)inImage.Xsize - 1) / (outImage.Xsize - 1);
                Ynew = y * ((double)inImage.Ysize - 1) / (outImage.Ysize - 1);
                Xint = (int)Math.Floor(Xnew);
                Yint = (int)Math.Floor(Ynew);
                dx = Xnew - Xint;
                dy = Ynew - Yint;
                p = input + Xint * ips + Yint * ils;

                switch (inImage.BandFmt)
                {
                    case IM_BANDFMT_UCHAR:
                        Loop<unsigned char>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_USHORT:
                        Loop<ushort>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_UINT:
                        Loop<uint>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_CHAR:
                        Loop<sbyte>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_SHORT:
                        Loop<short>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_INT:
                        Loop<int>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_FLOAT:
                        Loop<float>(p, q, ols, oes);
                        break;
                    case IM_BANDFMT_DOUBLE:
                        Loop<double>(p, q, ols, oes);
                        break;

                    default:
                        ImError("im_lowpass", "%s", "unsupported image type");
                        return -1;
                }
            }

            if (ImWriteLine(y, outImage, opline))
                return -1;
        }
        return 0;
    }

    private static void Loop<T>(PEL p, PEL q, int ols, int oes) where T : struct
    {
        for (int bb = 0; bb < inImage.Bands; bb++)
        {
            T s1 = *((T*)p);
            T s2 = *((T*)(p + ips));
            T s3 = *((T*)(p + ils));
            T s4 = *((T*)(p + ips + ils));

            T t = (T)q;
            t = (1 - dx) * (1 - dy) * s1 + dx * (1 - dy) * s2 + dy * (1 - dx) * s3 + dx * dy * s4;

            p += ies;
            q += oes;
        }
    }

    // im_iocheck()
    public static bool ImIoCheck(IMAGE inImage, IMAGE outImage)
    {
        return false; // Not implemented
    }

    // im_error()
    public static void ImError(string method, string message)
    {
        Console.WriteLine($"Error in {method}: {message}");
    }

    // im_cp_desc()
    public static bool ImCpDesc(IMAGE outImage, IMAGE inImage)
    {
        return false; // Not implemented
    }

    // im_setupout()
    public static bool ImSetupOut(IMAGE outImage)
    {
        return false; // Not implemented
    }

    // im_writeline()
    public static bool ImWriteLine(int y, IMAGE outImage, PEL opline)
    {
        return false; // Not implemented
    }
}
```

Note that I've assumed the existence of certain methods and classes (e.g. `IMAGE`, `PEL`, `IM_IMAGE_SIZEOF_LINE`, etc.) which are not defined in this code snippet. You will need to implement these yourself or modify the code to match your specific requirements.

Also, please note that C# does not have direct equivalents for some of the C types and functions used in the original code (e.g. `unsigned char`, `sbyte`, etc.). I've replaced them with their C# equivalents where possible.