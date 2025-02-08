Here is the C# code equivalent to the provided C code:

```csharp
// VipsMath --- call various -lm functions (trig, log etc.) on images
//
// Copyright: 1990, N. Dessipris, based on im_powtra()
// Author: Nicos Dessipris
// Written on: 02/05/1990
// Modified on:
// 5/5/93 JC
//	- adapted from im_lintra to work with partial images
//	- incorrect implementation of complex logs removed
// 1/7/93 JC
//	- adapted for partial v2
//	- ANSIfied
// 24/2/95 JC
//	- im_logtra() adapted to make im_sintra()
//	- adapted for im_wrapone()
// 26/1/96 JC
//	- im_asintra() added
// 30/8/09
// 	- gtkdoc
// 	- tiny cleanups
// 	- use im__math()
// 19/9/09
// 	- im_sintra() adapted to make math.c
// 4/11/11
// 	- redone as a class
// 11/8/15
// 	- log/log10 zero-avoid

using System;
using System.Collections.Generic;

public enum VipsOperationMath
{
    VIPS_OPERATION_MATH_SIN,
    VIPS_OPERATION_MATH_COS,
    VIPS_OPERATION_MATH_TAN,
    VIPS_OPERATION_MATH_ASIN,
    VIPS_OPERATION_MATH_ACOS,
    VIPS_OPERATION_MATH_ATAN,
    VIPS_OPERATION_MATH_SINH,
    VIPS_OPERATION_MATH_COSH,
    VIPS_OPERATION_MATH_TANH,
    VIPS_OPERATION_MATH_ASINH,
    VIPS_OPERATION_MATH_ACOSH,
    VIPS_OPERATION_MATH_ATANH,
    VIPS_OPERATION_MATH_LOG,
    VIPS_OPERATION_MATH_LOG10,
    VIPS_OPERATION_MATH_EXP,
    VIPS_OPERATION_MATH_EXP10
}

public class VipsMath : VipsUnary
{
    public VipsOperationMath math { get; set; }

    public override int Build(VipsObject object)
    {
        VipsUnary unary = (VipsUnary)object;

        if (unary.in != null && vips_check_noncomplex(object.Nickname, unary.in))
            return -1;

        if (base.Build(object) == 0)
            return 0;

        return -1;
    }

    public override void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        VipsMath math = (VipsMath)arithmetic;
        VipsImage im = arithmetic.Ready[0];
        const int sz = width * vips_image_get_bands(im);

        int x;

        switch (math.math)
        {
            case VIPS_OPERATION_MATH_SIN:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_COS:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_TAN:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_ASIN:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_ACOS:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_ATAN:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_SINH:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_COSH:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_TANH:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_ASINH:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_ACOSH:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_ATANH:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_LOG:
                Switch(math, im, sz);
                break;
            case VIPS_OPERATION_MATH_LOG10:
                Switch(math, im, sz);
                break;
            case VipsOperationMath.EXP:
                Switch(math, im, sz);
                break;
            case VipsOperationMath.EXP10:
                Switch(math, im, sz);
                break;

            default:
                throw new Exception("Invalid math operation");
        }
    }

    private void Switch(VipsMath math, VipsImage im, int sz)
    {
        switch (vips_image_get_format(im))
        {
            case VIPS_FORMAT_UCHAR:
                Loop((unsigned char[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_CHAR:
                Loop((sbyte[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_USHORT:
                Loop((ushort[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_SHORT:
                Loop((short[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_UINT:
                Loop((uint[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_INT:
                Loop((int[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_FLOAT:
                Loop((float[])inArrays[0], outArray, math.math);
                break;
            case VIPS_FORMAT_DOUBLE:
                Loop((double[])inArrays[0], outArray, math.math);
                break;

            default:
                throw new Exception("Invalid image format");
        }
    }

    private void Loop<T>(T[] inArray, T[] outArray, VipsOperationMath math)
    {
        for (int x = 0; x < inArray.Length; x++)
        {
            switch (math)
            {
                case VIPS_OPERATION_MATH_SIN:
                    outArray[x] = (float)Math.Sin((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_COS:
                    outArray[x] = (float)Math.Cos((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_TAN:
                    outArray[x] = (float)Math.Tan((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_ASIN:
                    outArray[x] = (float)Math.Asin((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_ACOS:
                    outArray[x] = (float)Math.Acos((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_ATAN:
                    outArray[x] = (float)Math.Atan((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_SINH:
                    outArray[x] = (float)Math.Sinh((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_COSH:
                    outArray[x] = (float)Math.Cosh((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_TANH:
                    outArray[x] = (float)Math.Tanh((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_ASINH:
                    outArray[x] = (float)Math.Asinh((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_ACOSH:
                    outArray[x] = (float)Math.Acosh((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_ATANH:
                    outArray[x] = (float)Math.Atanh((double)inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_LOG:
                    outArray[x] = LOGZ(inArray[x]);
                    break;
                case VIPS_OPERATION_MATH_LOG10:
                    outArray[x] = LOGZ10(inArray[x]);
                    break;
                case VipsOperationMath.EXP:
                    outArray[x] = (float)Math.Exp((double)inArray[x]);
                    break;
                case VipsOperationMath.EXP10:
                    outArray[x] = EXP10(inArray[x]);
                    break;

                default:
                    throw new Exception("Invalid math operation");
            }
        }
    }

    private double LOGZ(double x)
    {
        return (x == 0.0) ? 0.0 : Math.Log(x);
    }

    private double LOGZ10(double x)
    {
        return (x == 0.0) ? 0.0 : Math.Log10(x);
    }

    private double EXP10(double x)
    {
        return Math.Pow(10, x);
    }
}

public class VipsUnary
{
    public virtual int Build(VipsObject object)
    {
        throw new NotImplementedException();
    }

    public virtual void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        throw new NotImplementedException();
    }
}
```