```csharp
// remainder.cs
using System;
using VipsDotNet;

public class Remainder : Arithmetic
{
    public override int Build(VipsObject obj)
    {
        var binary = (VipsBinary)obj;
        if (binary.Left != null && !VipsCheckNonComplex(binary.Nickname, binary.Left))
            return -1;
        if (binary.Right != null && !VipsCheckNonComplex(binary.Nickname, binary.Right))
            return -1;

        if (base.Build(obj) == 0)
            return 0;

        return -1;
    }

    public override void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        var im = arithmetic.Ready[0];
        var sz = width * im.Bands;

        switch (im.Format)
        {
            case VipsFormat.Char:
                IRemainder(signed char);
                break;
            case VipsFormat.UChar:
                IRemainder(unsigned char);
                break;
            case VipsFormat.Short:
                IRemainder(signed short);
                break;
            case VipsFormat.UShort:
                IRemainder(unsigned short);
                break;
            case VipsFormat.Int:
                IRemainder(signed int);
                break;
            case VipsFormat.UInt:
                IRemainder(unsigned int);
                break;
            case VipsFormat.Float:
                FRemainder(float);
                break;
            case VipsFormat.Double:
                FRemainder(double);
                break;

            default:
                throw new Exception("Unsupported format");
        }
    }

    // Integer remainder-after-division.
    private void IRemainder<T>() where T : struct
    {
        var p1 = (T[])inArrays[0];
        var p2 = (T[])inArrays[1];
        var q = outArray;

        for (var x = 0; x < sz; x++)
            q[x] = p2[x] != 0 ? p1[x] % p2[x] : -1;
    }

    // Float remainder-after-division.
    private void FRemainder<T>() where T : struct
    {
        var p1 = (T[])inArrays[0];
        var p2 = (T[])inArrays[1];
        var q = outArray;

        for (var x = 0; x < sz; x++)
        {
            var a = p1[x];
            var b = p2[x];

            q[x] = b != 0 ? a - b * Math.Floor(a / b) : -1;
        }
    }

    // Type promotion for remainder.
    private static readonly VipsBandFormat[] RemainderFormatTable = new[]
    {
        VipsFormat.UChar, VipsFormat.Char,
        VipsFormat.UShort, VipsFormat.Short,
        VipsFormat.UInt, VipsFormat.Int,
        VipsFormat.Float, VipsFormat.Double
    };

    public override void ClassInit(VipsRemainderClass class_)
    {
        base.ClassInit(class_);
        var aclass = (VipsArithmeticClass)class_;
        aclass.ProcessLine = Buffer;
        VipsArithmetic.SetFormatTable(aclass, RemainderFormatTable);
    }

    // vips_remainder:
    public static int Remainder(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return CallSplit("remainder", left, right, ref outImage);
    }
}

public class RemainderConst : UnaryConst
{
    public override int Build(VipsObject obj)
    {
        var unary = (VipsUnary)obj;

        if (unary.In != null && !VipsCheckNonComplex(unary.Nickname, unary.In))
            return -1;

        if (base.Build(obj) == 0)
            return 0;

        return -1;
    }

    public override void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        var im = arithmetic.Ready[0];
        var bands = im.Bands;

        switch (im.Format)
        {
            case VipsFormat.Char:
                IRemainderConst(signed char);
                break;
            case VipsFormat.UChar:
                IRemainderConst(unsigned char);
                break;
            case VipsFormat.Short:
                IRemainderConst(signed short);
                break;
            case VipsFormat.UShort:
                IRemainderConst(unsigned short);
                break;
            case VipsFormat.Int:
                IRemainderConst(signed int);
                break;
            case VipsFormat.UInt:
                IRemainderConst(unsigned int);
                break;
            case VipsFormat.Float:
                FRemainderConst(float);
                break;
            case VipsFormat.Double:
                FRemainderConst(double);
                break;

            default:
                throw new Exception("Unsupported format");
        }
    }

    // Integer remainder-after-divide, per-band constant.
    private void IRemainderConst<T>() where T : struct
    {
        var p = (T[])inArrays[0];
        var q = outArray;
        var c = ((VipsUnaryConst)arithmetic).C;

        for (var x = 0; x < width; x++)
            for (var b = 0; b < bands; b++)
                q[x * bands + b] = p[x * bands + b] % c[b];
    }

    // Float remainder-after-divide, per-band constant.
    private void FRemainderConst<T>() where T : struct
    {
        var p = (T[])inArrays[0];
        var q = outArray;
        var c = ((VipsUnaryConst)arithmetic).C;

        for (var x = 0; x < width; x++)
            for (var b = 0; b < bands; b++)
            {
                var left = p[x * bands + b];
                var right = c[b];

                q[x * bands + b] = right != 0 ? left - right * Math.Floor(left / right) : -1;
            }
    }

    public override void ClassInit(VipsRemainderConstClass class_)
    {
        base.ClassInit(class_);
        var aclass = (VipsArithmeticClass)class_;
        aclass.ProcessLine = Buffer;
        VipsArithmetic.SetFormatTable(aclass, RemainderFormatTable);
    }

    // vips_remainder_const:
    public static int RemainderConst(VipsImage inImage, ref VipsImage outImage, double[] c, int n)
    {
        return CallSplit("remainder_const", inImage, ref outImage, c, n);
    }
}

public class RemainderConst1 : UnaryConst
{
    // vips_remainder_const1:
    public static int RemainderConst1(VipsImage inImage, ref VipsImage outImage, double c)
    {
        return CallSplit("remainder_const", inImage, ref outImage, new[] { c }, 1);
    }
}
```