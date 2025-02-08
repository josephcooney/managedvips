Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class Vips
{
    public static int im_lab_morph(IMAGE inImage, IMAGE outImage,
        DOUBLEMASK mask,
        double L_offset, double L_scale,
        double a_scale, double b_scale)
    {
        // Morph a lab image.
        //
        // 8/3/01
        // 	- added
        // 2/11/09
        // 	- cleanups
        // 	- gtkdoc

        Params parm = new Params();

        if (inImage.Coding == IM_CODING_LABQ)
        {
            IMAGE[] t = new IMAGE[2];

            if (!im_open_local_array(outImage, t, 2, "im_lab_morph", "p") &&
                !im_LabQ2Lab(inImage, t[0]) &&
                !im_lab_morph(t[0], t[1],
                    mask, L_offset, L_scale, a_scale, b_scale) &&
                !im_Lab2LabQ(t[1], outImage))
            {
                return -1;
            }

            return 0;
        }

        if (!morph_init(parm,
            inImage, outImage, L_scale, L_offset, mask, a_scale, b_scale))
        {
            return -1;
        }

        return im__colour_unary("im_lab_morph", inImage, outImage, IM_TYPE_LAB,
            (im_wrapone_fn)morph_buffer, parm, null);
    }
}

public class Params
{
    public IMAGE inImage;
    public IMAGE outImage;

    public double L_scale;
    public double L_offset;

    public double[] a_offset = new double[101];
    public double[] b_offset = new double[101];

    public double a_scale;
    public double b_scale;
}

public class DOUBLEMASK
{
    public int xsize;
    public int ysize;
    public double[] coeff;
}

public class IMAGE
{
    public int Coding;
}

// Morph init function
static bool morph_init(Params parm,
    IMAGE inImage, IMAGE outImage,
    double L_scale, double L_offset,
    DOUBLEMASK mask, double a_scale, double b_scale)
{
    for (int i = 0; i < mask.ysize; i++)
    {
        if (mask.coeff[i * 3] < 0 || mask.coeff[i * 3] > 100 ||
            mask.coeff[i * 3 + 1] < -120 || mask.coeff[i * 3 + 1] > 120 ||
            mask.coeff[i * 3 + 2] < -120 || mask.coeff[i * 3 + 2] > 120)
        {
            throw new Exception("bad greyscale mask value, row " + i);
        }
    }

    for (int i = 0; i <= 100; i++)
    {
        double L_low = 0;
        double a_low = 0;
        double b_low = 0;

        double L_high = 100;
        double a_high = 0;
        double b_high = 0;

        for (int j = 0; j < mask.ysize; j++)
        {
            if (mask.coeff[j * 3] < i && mask.coeff[j * 3] > L_low)
            {
                L_low = mask.coeff[j * 3];
                a_low = mask.coeff[j * 3 + 1];
                b_low = mask.coeff[j * 3 + 2];
            }
        }

        for (int j = mask.ysize - 1; j >= 0; j--)
        {
            if (mask.coeff[j * 3] >= i && mask.coeff[j * 3] < L_high)
            {
                L_high = mask.coeff[j * 3];
                a_high = mask.coeff[j * 3 + 1];
                b_high = mask.coeff[j * 3 + 2];
            }
        }

        parm.a_offset[i] = a_low +
            (a_high - a_low) * ((i - L_low) / (L_high - L_low));
        parm.b_offset[i] = b_low +
            (b_high - b_low) * ((i - L_low) / (L_high - L_low));
    }

    return true;
}

// Morph buffer function
static void morph_buffer(float[] in, float[] out, int width, Params parm)
{
    for (int x = 0; x < width; x++)
    {
        double L = in[0];
        double a = in[1];
        double b = in[2];

        L = Math.Min(Math.Max(0, L), 100);
        a -= parm.a_offset[(int)L];
        b -= parm.b_offset[(int)L];

        L = (L + parm.L_offset) * parm.L_scale;
        L = Math.Min(Math.Max(0, L), 100);

        a *= parm.a_scale;
        b *= parm.b_scale;

        out[0] = L;
        out[1] = a;
        out[2] = b;

        in += 3;
        out += 3;
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `IMAGE`, `DOUBLEMASK`, `im_open_local_array`, etc.) which are not defined here, as they were likely defined elsewhere in your codebase. You may need to modify this code to fit your specific use case.