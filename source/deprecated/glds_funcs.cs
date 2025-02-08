Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsImage
{
    public static int im_glds_matrix(VipsImage im, VipsImage m,
        int xpos, int ypos, int xsize, int ysize, int dx, int dy)
    {
        // @(#)  Calculates the spatial grey level differnce
        // @(#) matrix of an image and some of its
        // @(#) features.  The 256x1 difference matrix of im is held by m

        if (im_iocheck(im, m) == -1)
            return -1;

        if ((im.Bands != 1) || (im.BandFmt != VipsBandFormat.UChar))
        {
            throw new ArgumentException("Wrong input");
        }

        if ((xpos + xsize + dx > im.Xsize) || (ypos + ysize + dy > im.Ysize))
        {
            throw new ArgumentException("wrong args");
        }

        if (im_cp_desc(m, im) == -1)
            return -1;
        m.Xsize = 256;
        m.Ysize = 1;
        m.BandFmt = VipsBandFormat.Double;
        m.Type = VipsImageType.B_W;

        if (im_setupout(m) == -1)
            return -1;

        int[] b = new int[m.Xsize];
        double[] l = new double[m.Xsize];

        for (int y = 0; y < ysize; y++)
        {
            PEL cpin = im.data[y * im.Xsize + xpos];
            for (int x = 0; x < xsize; x++)
            {
                int tmp = Math.Abs((int)cpin - (int)(cpin + ofs));
                b[tmp]++;
                cpin++;
            }
        }

        int norm = xsize * ysize;
        double sum = 0.0;
        for (int i = 0; i < m.Xsize; i++)
        {
            sum += ((double)b[i]) / (double)norm;
        }

        if (im_writeline(0, m, l) == -1)
            return -1;

        return 0;
    }

    public static int im_glds_asm(VipsImage m, double[] asmoment)
    {
        // @(#)  Calculates the asmoment of the sglds matrix held by m

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 1) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("unable to accept input");
        }

        double tmpasm = 0.0;
        double[] in = m.data;
        for (int i = 0; i < m.Xsize; i++)
        {
            tmpasm += Math.Pow(in[i], 2);
        }
        asmoment[0] = tmpasm;

        return 0;
    }

    public static int im_glds_contrast(VipsImage m, double[] contrast)
    {
        // @(#)     Calculates the contrast of the coocurence matrix passed in buffer

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 1) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("wrong input");
        }

        double tmpcon = 0.0;
        double[] in = m.data;
        for (int i = 0; i < m.Xsize; i++)
        {
            tmpcon += Math.Pow(i, 2) * in[i];
        }
        contrast[0] = tmpcon;

        return 0;
    }

    public static int im_glds_entropy(VipsImage m, double[] entropy)
    {
        // @(#)     Calculates the entropy of the glds vector passed in buffer
        // @(#) Function returns the entropy based on log base 2.

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 1) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("wrong input");
        }

        double tmpent = 0.0;
        double[] in = m.data;
        for (int i = 0; i < m.Xsize; i++)
        {
            if (in[i] != 0)
            {
                tmpent += in[i] * Math.Log10(in[i]);
            }
        }
        entropy[0] = (-1) * tmpent / Math.Log10(2.0);

        return 0;
    }

    public static int im_glds_mean(VipsImage m, double[] mean)
    {
        // @(#)     Calculates the mean of the sglds matrix passed in m

        if (im_incheck(m))
            return -1;

        if ((m.Xsize != 256) || (m.Ysize != 1) ||
            (m.Bands != 1) || (m.BandFmt != VipsBandFormat.Double))
        {
            throw new ArgumentException("wrong input");
        }

        double tmpmean = 0.0;
        double[] in = m.data;
        for (int i = 0; i < m.Xsize; i++)
        {
            tmpmean += i * in[i];
        }
        mean[0] = tmpmean / m.Xsize;

        return 0;
    }

    // ... other VipsImage methods ...
}
```

Note that I've assumed the existence of a `VipsImage` class with properties and methods similar to those found in the original C code. You may need to modify this code to fit your specific requirements.

Also, note that I've used the `Math.Abs`, `Math.Pow`, and `Math.Log10` functions from the .NET Framework's `System.Math` namespace instead of the corresponding C functions.