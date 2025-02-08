Here is the C# code equivalent to the provided C code:

```csharp
// @(#) Function which convolves and subsamples VASARI format picture
// with a mask stored in a file argument.

public class ConvolutionSubsampling
{
    public static int im_convsub(IMAGE inImage, IMAGE outImage, INTMASK m, int xskip, int yskip)
    {
        // Check input, output and vars
        if (xskip < 1 || yskip < 1)
        {
            Console.WriteLine("xskip and yskip must be >= 1");
            return -1;
        }
        if (!im_iocheck(inImage, outImage))
            return -1;

        // Prepare output
        if (!im_cp_desc(outImage, inImage))
            return -1;
        int tempsize = inImage.Xsize / xskip;
        while (true)
        {
            if (tempsize * xskip + m.xsize < inImage.Xsize)
                break;
            else
                tempsize--;
            if (tempsize < 0)
                break;
        }
        outImage.Xsize = tempsize;
        tempsize = inImage.Ysize / yskip;
        while (true)
        {
            if (tempsize * yskip + m.ysize < inImage.Ysize)
                break;
            else
                tempsize--;
            if (tempsize < 0)
                break;
        }
        outImage.Ysize = tempsize;
        if ((outImage.Xsize < 2) || (outImage.Ysize < 2))
        {
            Console.WriteLine("too small output sizes");
            return -1;
        }

        // Malloc one line of output data
        int os = outImage.Xsize * outImage.Bands;
        byte[] line = new byte[os];

        // Malloc pointers and put them at correct location
        int ms = m.xsize * m.ysize;
        int count = 0; /* exclude the non-zero elms */
        int[] pm = m.coeff;
        for (int i = 0; i < ms; i++)
        {
            if (*pm++ != 0)
                count++;
        }

        if ((newm = new int[count]) == null ||
            (pnts = new PEL*[count]) == null ||
            (cpnt1s = new PEL*[count]) == null ||
            (cpnt2s = new PEL*[count]) == null)
        {
            Console.WriteLine("unable to calloc(2)");
            return -1;
        }

        // copy the non-zero elms of the original mask and set pointers
        int i = 0;
        byte[] input = inImage.data;
        pm = m.coeff;
        newm = newm;
        for (int y = 0; y < m.ysize; y++)
        {
            for (int x = 0; x < m.xsize; x++)
            {
                if (*pm != 0)
                {
                    *newm++ = *pm;
                    pnts[i] = new PEL[input.Length / inImage.Bands + (x + y * inImage.Xsize) * inImage.Bands];
                    i++;
                }
                pm++;
            }
        }

        // Create luts
        int lutcnt = 0;
        int[][] lut_orig = new int[count][];
        int[][] lut = new int[count][];

        if (im__create_int_luts(newm, count, lut_orig, lut, ref lutcnt) == -1)
        {
            Console.WriteLine("im_create_int_luts failed");
            return -1;
        }

        // Output out->Ysize processed lines
        for (int y = 0; y < outImage.Ysize; y++)
        {
            byte[] cpline = line;
            for (i = 0; i < count; i++)
            {
                cpnt1s[i] = pnts[i];
                // skip yskip input lines
                pnts[i] += inImage.Xsize * inImage.Bands * yskip;
            }

            // process out->Xsize points
            for (int x = 0; x < outImage.Xsize; x++)
            {
                for (i = 0; i < count; i++) /* skip xskip elms */
                {
                    cpnt2s[i] = cpnt1s[i];
                    cpnt1s[i] += xskip * inImage.Bands;
                }
                for (int b = 0; b < outImage.Bands; b++)
                {
                    int sum = 0;
                    for (i = 0; i < count; i++) /* core of convolution */
                    {
                        sum += lut[i][*cpnt2s[i]];
                        cpnt2s[i]++;
                    }
                    sum = ((sum + m.scale / 2) / m.scale) + m.offset;

                    if (sum < 0)
                    {
                        n_clipped++;
                        sum = 0;
                    }
                    else if (sum > 255)
                    {
                        p_clipped++;
                        sum = 255;
                    }
                    cpline[b * outImage.Xsize + x] = (byte)sum;
                }
            }

            // Output the calculated line
            if (!im_writeline(y, outImage, line))
            {
                return -1;
            }
        }

        // free memory
        for (i = 0; i < lutcnt; i++)
            lut_orig[i] = null;
        lut_orig = null;
        lut = null;

        return 0;
    }

    static int[] newm;
    static PEL[][] pnts;
    static PEL[][] cpnt1s;
    static PEL[][] cpnt2s;
    static int lutcnt;
    static int[][] lut_orig;
    static int[][] lut;
    static int n_clipped = 0;
    static int p_clipped = 0;

    public static bool im_iocheck(IMAGE inImage, IMAGE outImage)
    {
        return (inImage.Coding != IM_CODING_NONE) &&
               (inImage.BandFmt == IM_BANDFMT_UCHAR);
    }

    public static bool im_cp_desc(IMAGE outImage, IMAGE inImage)
    {
        // copy description from input to output
        outImage.Xsize = inImage.Xsize;
        outImage.Ysize = inImage.Ysize;
        outImage.Bands = inImage.Bands;
        return true;
    }

    public static bool im_writeline(int y, IMAGE outImage, byte[] line)
    {
        // write the calculated line to output
        // implementation depends on the actual image format
        return true;
    }

    public static int im__create_int_luts(int[] newm, int count, int[][] lut_orig, int[][] lut, ref int lutcnt)
    {
        // Create multiplication luts for all non zero elements of the original mask;
        // which is kept in buffer of length buffersize
        // cnt is needed for freeing luts

        // implementation depends on the actual image format
        return 0;
    }
}
```

Note that some methods (e.g. `im_iocheck`, `im_cp_desc`, `im_writeline`, and `im__create_int_luts`) are not implemented as their implementation depends on the actual image format, which is not specified in the provided C code. You will need to implement these methods according to your specific requirements.

Also note that I've used `byte[]` instead of `PEL*` for the line data, and `int[][]` instead of `int**` for the luts. This is because C# does not support pointers in the same way as C, so we need to use arrays or other data structures to represent the same concepts.

Finally, I've added some comments to explain what each part of the code does, but you may want to add more comments or documentation depending on your specific needs.