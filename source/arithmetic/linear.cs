Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsLinear : VipsUnary
{
    public override int Build(VipsObject object)
    {
        VipsArithmetic arithmetic = (VipsArithmetic)object;
        VipsUnary unary = (VipsUnary)object;
        VipsLinear linear = (VipsLinear)object;

        // If we have a three-element vector, we need to bandup the image to match.
        linear.n = 1;
        if (linear.a != null)
            linear.n = Math.Max(linear.n, linear.a.Length);
        if (linear.b != null)
            linear.n = Math.Max(linear.n, linear.b.Length);
        if (unary.in != null)
        {
            int bands;

            vips_image_decode_predict(unary.in, out bands, null);
            linear.n = Math.Max(linear.n, bands);
        }
        arithmetic.base_bands = linear.n;

        // If all elements of the constants are equal, we can shrink them down to a single element.
        if (linear.a != null)
        {
            double[] ary = (double[])linear.a.Data;
            bool all_equal = true;

            for (int i = 1; i < linear.a.Length; i++)
                if (ary[i] != ary[0])
                {
                    all_equal = false;
                    break;
                }

            if (all_equal)
                linear.a.Length = 1;
        }
        if (linear.b != null)
        {
            double[] ary = (double[])linear.b.Data;
            bool all_equal = true;

            for (int i = 1; i < linear.b.Length; i++)
                if (ary[i] != ary[0])
                {
                    all_equal = false;
                    break;
                }

            if (all_equal)
                linear.b.Length = 1;
        }

        // Make up-banded versions of our constants.
        linear.a_ready = new double[linear.n];
        linear.b_ready = new double[linear.n];

        for (int i = 0; i < linear.n; i++)
        {
            if (linear.a != null)
            {
                double[] ary = (double[])linear.a.Data;
                int j = Math.Min(i, linear.a.Length - 1);

                linear.a_ready[i] = ary[j];
            }

            if (linear.b != null)
            {
                double[] ary = (double[])linear.b.Data;
                int j = Math.Min(i, linear.b.Length - 1);

                linear.b_ready[i] = ary[j];
            }
        }

        return base.Build(object);
    }

    public override void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        VipsLinear linear = (VipsLinear)this;
        double[] a = linear.a_ready;
        double[] b = linear.b_ready;

        if (linear.uchar)
            switch (vips_image_get_format(im))
            {
                case VIPS_FORMAT_UCHAR:
                    LOOPuc(unsigned char);
                    break;
                case VIPS_FORMAT_CHAR:
                    LOOPuc(signed char);
                    break;
                case VIPS_FORMAT_USHORT:
                    LOOPuc(unsigned short);
                    break;
                case VIPS_FORMAT_SHORT:
                    LOOPuc(signed short);
                    break;
                case VIPS_FORMAT_UINT:
                    LOOPuc(unsigned int);
                    break;
                case VIPS_FORMAT_INT:
                    LOOPuc(signed int);
                    break;
                case VIPS_FORMAT_FLOAT:
                    LOOPuc(float);
                    break;
                case VIPS_FORMAT_DOUBLE:
                    LOOPuc(double);
                    break;
                case VIPS_FORMAT_COMPLEX:
                    LOOPCMPLXNuc(float);
                    break;
                case VIPS_FORMAT_DPCOMPLEX:
                    LOOPCMPLXNuc(double);
                    break;

                default:
                    throw new Exception("Invalid image format");
            }
        else
            switch (vips_image_get_format(im))
            {
                case VIPS_FORMAT_UCHAR:
                    LOOP(unsigned char, float);
                    break;
                case VIPS_FORMAT_CHAR:
                    LOOP(signed char, float);
                    break;
                case VIPS_FORMAT_USHORT:
                    LOOP(unsigned short, float);
                    break;
                case VIPS_FORMAT_SHORT:
                    LOOP(signed short, float);
                    break;
                case VIPS_FORMAT_UINT:
                    LOOP(unsigned int, float);
                    break;
                case VIPS_FORMAT_INT:
                    LOOP(signed int, float);
                    break;
                case VIPS_FORMAT_FLOAT:
                    LOOP(float, float);
                    break;
                case VIPS_FORMAT_DOUBLE:
                    LOOP(double, double);
                    break;
                case VIPS_FORMAT_COMPLEX:
                    LOOPCMPLXN(float, float);
                    break;
                case VIPS_FORMAT_DPCOMPLEX:
                    LOOPCMPLXN(double, double);
                    break;

                default:
                    throw new Exception("Invalid image format");
            }
    }

    private void LOOPuc(Type inType)
    {
        if (linear.a.Length == 1 && linear.b.Length == 1)
        {
            LOOP1uc(inType);
        }
        else
        {
            LOOPNuc(inType);
        }
    }

    private void LOOP1uc(Type inType)
    {
        unsigned char[] p = (unsigned char[])inArray[0];
        VipsPel[] q = outArray;

        float a1 = linear.a_ready[0];
        float b1 = linear.b_ready[0];

        int sz = width * im.Bands;

        for (int x = 0; x < sz; x++)
        {
            float t = a1 * p[x] + b1;

            q[x] = VIPS_FCLIP(0, t, 255);
        }
    }

    private void LOOPNuc(Type inType)
    {
        unsigned char[] p = (unsigned char[])inArray[0];
        VipsPel[] q = outArray;

        for (int i = 0, x = 0; x < width; x++)
            for (int k = 0; k < im.Bands; k++, i++)
                {
                    double t = linear.a_ready[k] * p[i] + linear.b_ready[k];

                    q[i] = VIPS_FCLIP(0, t, 255);
                }
    }

    private void LOOP(Type inType, Type outType)
    {
        if (linear.a.Length == 1 && linear.b.Length == 1)
        {
            LOOP1(inType, outType);
        }
        else
        {
            LOOPN(inType, outType);
        }
    }

    private void LOOP1(Type inType, Type outType)
    {
        inType p = (inType)inArray[0];
        outType[] q = outArray;

        outType a1 = linear.a_ready[0];
        outType b1 = linear.b_ready[0];

        int sz = width * im.Bands;

        for (int x = 0; x < sz; x++)
            q[x] = a1 * p[x] + b1;
    }

    private void LOOPN(Type inType, Type outType)
    {
        inType[] p = (inType[])inArray[0];
        outType[] q = outArray;

        for (int i = 0, x = 0; x < width; x++)
            for (int k = 0; k < im.Bands; k++, i++)
                {
                    outType t = linear.a_ready[k] * p[i] + linear.b_ready[k];

                    q[i] = t;
                }
    }

    private void LOOPCMPLXNuc(Type inType)
    {
        unsigned char[] p = (unsigned char[])inArray[0];
        VipsPel[] q = outArray;

        for (int i = 0, x = 0; x < width; x++)
            for (int k = 0; k < im.Bands; k++, i++)
                {
                    double t = linear.a_ready[k] * p[0] + linear.b_ready[k];

                    q[i] = VIPS_FCLIP(0, t, 255);
                    p += 2;
                }
    }

    private void LOOPCMPLXN(Type inType, Type outType)
    {
        inType[] p = (inType[])inArray[0];
        outType[] q = outArray;

        for (int x = 0; x < width; x++)
            for (int k = 0; k < im.Bands; k++)
                {
                    q[0] = linear.a_ready[k] * p[0] + linear.b_ready[k];
                    q[1] = p[1];
                    q += 2;
                    p += 2;
                }
    }

    public static int VipsLinearv(VipsImage inImage, VipsImage[] outImages, double[] a, double[] b, int n, params object[] args)
    {
        VipsArea area_a = new VipsArea(vips_array_double_new(a, n));
        VipsArea area_b = new VipsArea(vips_array_double_new(b, n));

        int result = vips_call_split("linear", args, inImage, outImages, area_a, area_b);

        vips_area_unref(area_a);
        vips_area_unref(area_b);

        return result;
    }

    public static int VipsLinear(VipsImage inImage, VipsImage[] outImages, double[] a, double[] b, int n)
    {
        object[] args = new object[0];

        return VipsLinearv(inImage, outImages, a, b, n, args);
    }

    public static int VipsLinear1(VipsImage inImage, VipsImage[] outImages, double a, double b)
    {
        double[] aArray = new double[] { a };
        double[] bArray = new double[] { b };

        return VipsLinear(inImage, outImages, aArray, bArray, 1);
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsUnary`, `VipsArithmetic`, `vips_image_decode_predict`, etc.) which are not defined in this code snippet. You will need to ensure that these classes and methods are properly implemented and available for use.

Also, I've used C# 6 syntax and features throughout the code. If you're using an earlier version of C#, you may need to modify the code accordingly.

Finally, please note that this is a direct translation of the provided C code, and it's possible that there are more efficient or idiomatic ways to implement certain parts of the code in C#.