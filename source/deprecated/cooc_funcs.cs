Here's the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsImage
{
    public static int im_cooc_sym(VipsImage im, VipsImage m, int xpos, int ypos, int xsize, int ysize, int dx, int dy)
    {
        // @(#) Calculates the cooccurrence matrix of an image and some of its
        //      features.  The 256x256 cooccurrence matrix of im is held by m

        if (im_iocheck(im, m) == -1)
            return -1;

        if ((im.Bands != 1) || (im.BandFmt != VipsBandFormat.UChar))
        {
            throw new ArgumentException("Unable to accept input");
        }

        if ((xpos + xsize + dx > im.Xsize) || (ypos + ysize + dy > im.Ysize))
        {
            throw new ArgumentException("wrong args");
        }

        if (im_cp_desc(m, im) == -1)
            return -1;

        m.Xsize = 256;
        m.Ysize = 256;
        m.BandFmt = VipsBandFormat.Double;
        m.Type = VipsImageType.BW;

        if (im_setupout(m) == -1)
            return -1;

        // malloc space to keep the read values
        int[] buf = new int[m.Xsize * m.Ysize];
        double[] line = new double[m.Xsize];

        for (int y = 0; y < ysize; y++)
        {
            for (int x = 0; x < xsize; x++)
            {
                int tempA = im.data[ypos + y, xpos + x];
                int tempB = im.data[ypos + y + dy, xpos + x + dx];

                buf[tempA + m.Xsize * tempB]++;
                buf[tempB + m.Xsize * tempA]++;
            }
        }

        double norm = xsize * ysize * 2;
        for (int y = 0; y < m.Ysize; y++)
        {
            for (int x = 0; x < m.Xsize; x++)
            {
                buf[x + m.Xsize * y] /= norm;
            }
        }

        if (im_writeline(0, m, line) == -1)
        {
            throw new Exception("unable to im_writeline");
        }

        return 0;
    }

    public static int im_cooc_ord(VipsImage im, VipsImage m, int xpos, int ypos, int xsize, int ysize, int dx, int dy)
    {
        // @(#) Calculates the cooccurrence matrix of an image and some of its
        //      features.  The 256x256 cooccurrence matrix of im is held by m

        if (im_iocheck(im, m) == -1)
            return -1;

        if ((im.Bands != 1) || (im.BandFmt != VipsBandFormat.UChar))
        {
            throw new ArgumentException("Unable to accept input");
        }

        if ((xpos + xsize + dx > im.Xsize) || (ypos + ysize + dy > im.Ysize))
        {
            throw new ArgumentException("wrong args");
        }

        if (im_cp_desc(m, im) == -1)
            return -1;

        m.Xsize = 256;
        m.Ysize = 256;
        m.BandFmt = VipsBandFormat.Double;

        if (im_setupout(m) == -1)
            return -1;

        // malloc space to keep the read values
        int[] buf = new int[m.Xsize * m.Ysize];
        double[] line = new double[m.Xsize];

        for (int y = 0; y < ysize; y++)
        {
            for (int x = 0; x < xsize; x++)
            {
                int tempA = im.data[ypos + y, xpos + x];
                int tempB = im.data[ypos + y + dy, xpos + x + dx];

                buf[tempA + m.Xsize * tempB]++;
            }
        }

        double norm = xsize * ysize;
        for (int y = 0; y < m.Ysize; y++)
        {
            for (int x = 0; x < m.Xsize; x++)
            {
                buf[x + m.Xsize * y] /= norm;
            }
        }

        if (im_writeline(0, m, line) == -1)
        {
            throw new Exception("unable to im_writeline");
        }

        return 0;
    }

    public static int im_cooc_matrix(VipsImage im, VipsImage m,
        int xp, int yp, int xs, int ys, int dx, int dy, int flag)
    {
        if (flag == 0)
            return im_cooc_ord(im, m, xp, yp, xs, ys, dx, dy);
        else if (flag == 1) /* symmetrical cooc */
            return im_cooc_sym(im, m, xp, yp, xs, ys, dx, dy);
        else
        {
            throw new ArgumentException("wrong flag!");
        }
    }

    public static int im_cooc_asm(VipsImage m, double[] asmoment)
    {
        // @(#) Calculate the asmoment of a cooccurrence matrix

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 256) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("unable to accept input");
        }

        double tmpasm = 0.0;
        for (int i = 0; i < m.Xsize * m.Ysize; i++)
        {
            tmpasm += Math.Pow(m.data[i], 2);
        }
        asmoment[0] = tmpasm;

        return 0;
    }

    public static int im_cooc_contrast(VipsImage m, double[] contrast)
    {
        // @(#) Calculate the contrast of a cooccurrence matrix

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 256) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("unable to accept input");
        }

        double tmpcon = 0.0;
        for (int y = 0; y < m.Ysize; y++)
        {
            for (int x = 0; x < m.Xsize; x++)
            {
                int tempA = x - y;
                tmpcon += tempA * tempA * m.data[x + m.Xsize * y];
            }
        }

        contrast[0] = tmpcon;

        return 0;
    }

    public static int im_cooc_correlation(VipsImage m, double[] correlation)
    {
        // @(#) Calculate the correlation of a cooccurrence matrix

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 256) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("unable to accept input");
        }

        double[] row = new double[m.Ysize];
        double[] col = new double[m.Xsize];

        for (int j = 0; j < m.Ysize; j++)
        {
            for (int i = 0; i < m.Xsize; i++)
            {
                row[j] += m.data[i + m.Xsize * j];
            }
        }

        for (int j = 0; j < m.Ysize; j++)
        {
            for (int i = 0; i < m.Xsize; i++)
            {
                col[j] += m.data[i + m.Xsize * j];
            }
        }

        double[] meanRow = new double[1];
        double[] stdRow = new double[1];

        stats(row, m.Ysize, meanRow, stdRow);

        double[] meanCol = new double[1];
        double[] stdCol = new double[1];

        stats(col, m.Xsize, meanCol, stdCol);

        double tmpcor = 0.0;
        for (int j = 0; j < m.Ysize; j++)
        {
            for (int i = 0; i < m.Xsize; i++)
            {
                tmpcor += ((double)i * (double)j * m.data[i + m.Xsize * j]);
            }
        }

        if ((stdCol[0] == 0.0) || (stdRow[0] == 0.0))
        {
            throw new ArgumentException("zero std");
        }

        tmpcor = (tmpcor - (meanCol[0] * meanRow[0])) / (stdCol[0] * stdRow[0]);
        correlation[0] = tmpcor;

        return 0;
    }

    public static int im_cooc_entropy(VipsImage m, double[] entropy)
    {
        // @(#) Calculate the entropy of a cooccurrence matrix

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 256) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("unable to accept input");
        }

        double tmpent = 0.0;
        for (int j = 0; j < m.Ysize; j++)
        {
            for (int i = 0; i < m.Xsize; i++)
            {
                if (m.data[i + m.Xsize * j] != 0)
                {
                    tmpent += Math.Pow(m.data[i + m.Xsize * j], 2) * Math.Log10(m.data[i + m.Xsize * j]);
                }
            }
        }

        entropy[0] = -tmpent / Math.Log10(2.0);

        return 0;
    }

    public static void stats(double[] buffer, int size, double[] pmean, double[] pstd)
    {
        // @(#) Calculate the mean and standard deviation of a distribution

        double mean = 0.0;
        double std = 0.0;

        for (int i = 0; i < size; i++)
        {
            mean += buffer[i] * i;
        }

        double sumf2 = 0.0;
        for (int i = 0; i < size; i++)
        {
            sumf2 += Math.Pow(buffer[i], 2) * Math.Pow(i, 2);
        }

        double correction = mean * mean;

        mean /= size;
        std = Math.Sqrt(sumf2 - correction);

        pmean[0] = mean;
        pstd[0] = std;
    }
}
```

Note that I've assumed the existence of a `VipsImage` class with properties like `Xsize`, `Ysize`, `Bands`, `BandFmt`, and `data`. You may need to modify this code to fit your specific use case. Additionally, some methods like `im_iocheck`, `im_cp_desc`, `im_setupout`, and `im_writeline` are not implemented here as they seem to be part of the VIPS library and their implementation is not provided in the given C code snippet.