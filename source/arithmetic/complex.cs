Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsComplex : VipsUnary
{
    public enum Operation { Polar, Rect, Conj };

    private Operation cmplx;

    public VipsComplex(Operation cmplx)
    {
        this.cmplx = cmplx;
    }

    public override int ProcessLine(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int sz = width * VipsImage.GetBands(im);

        switch (cmplx)
        {
            case Operation.Polar:
                Polar(outArray, inArray);
                break;
            case Operation.Rect:
                Rect(outArray, inArray);
                break;
            case Operation.Conj:
                Conj(outArray, inArray);
                break;
            default:
                throw new ArgumentException("Invalid operation");
        }

        return 0;
    }

    private void Polar(VipsPel[] outArray, VipsPel[][] inArray)
    {
        double[] re = new double[sz];
        double[] im = new double[sz];

        for (int x = 0; x < sz; x++)
        {
            re[x] = inArray[0][x].ToDouble();
            im[x] = inArray[1][x].ToDouble();

            double am = Math.Sqrt(re[x] * re[x] + im[x] * im[x]);
            double ph = VipsComplex.Atan2(im[x], re[x]);

            if (ph < 0)
                ph += 360;

            outArray[x * 2] = new VipsPel(am);
            outArray[(x * 2) + 1] = new VipsPel(ph);
        }
    }

    private void Rect(VipsPel[] outArray, VipsPel[][] inArray)
    {
        double[] am = new double[sz];
        double[] ph = new double[sz];

        for (int x = 0; x < sz; x++)
        {
            am[x] = inArray[0][x].ToDouble();
            ph[x] = inArray[1][x].ToDouble();

            double re = am[x] * Math.Cos(VipsRad(ph[x]));
            double im = am[x] * Math.Sin(VipsRad(ph[x]));

            outArray[x * 2] = new VipsPel(re);
            outArray[(x * 2) + 1] = new VipsPel(im);
        }
    }

    private void Conj(VipsPel[] outArray, VipsPel[][] inArray)
    {
        double[] re = new double[sz];
        double[] im = new double[sz];

        for (int x = 0; x < sz; x++)
        {
            re[x] = inArray[0][x].ToDouble();
            im[x] = inArray[1][x].ToDouble();

            im[x] *= -1;

            outArray[x * 2] = new VipsPel(re[x]);
            outArray[(x * 2) + 1] = new VipsPel(im[x]);
        }
    }

    private static double Atan2(double a, double b)
    {
        double h = Math.Atan2(b, a);
        if (h < 0)
            h += 360;

        return h;
    }

    public override void ClassInit()
    {
        base.ClassInit();

        VipsObjectClass object_class = (VipsObjectClass)GetType();
        VipsArithmeticClass arithmetic_class = (VipsArithmeticClass)typeof(VipsComplex).GetNestedType("Arithmetic", BindingFlags.NonPublic);

        object_class.Nickname = "complex";
        object_class.Description = "perform a complex operation on an image";

        arithmetic_class.ProcessLine += ProcessLine;
    }
}

public class VipsComplex2 : VipsBinary
{
    public enum Operation { CrossPhase };

    private Operation cmplx;

    public VipsComplex2(Operation cmplx)
    {
        this.cmplx = cmplx;
    }

    public override int ProcessLine(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int sz = width * VipsImage.GetBands(im);

        switch (cmplx)
        {
            case Operation.CrossPhase:
                CrossPhase(outArray, inArray);
                break;
            default:
                throw new ArgumentException("Invalid operation");
        }

        return 0;
    }

    private void CrossPhase(VipsPel[] outArray, VipsPel[][] inArray)
    {
        double[] re1 = new double[sz];
        double[] im1 = new double[sz];
        double[] re2 = new double[sz];
        double[] im2 = new double[sz];

        for (int x = 0; x < sz; x++)
        {
            re1[x] = inArray[0][x].ToDouble();
            im1[x] = inArray[1][x].ToDouble();
            re2[x] = inArray[2][x].ToDouble();
            im2[x] = inArray[3][x].ToDouble();

            if ((re1[x] == 0 && im1[x] == 0) || (re2[x] == 0 && im2[x] == 0) || (im1[x] == 0 && im2[x] == 0))
            {
                outArray[x * 2] = new VipsPel(0);
                outArray[(x * 2) + 1] = new VipsPel(0);
            }
            else if (Math.Abs(im1[x]) > Math.Abs(im2[x]))
            {
                double y1 = im1[x];
                double a = im2[x] / y1;
                double b = im1[x] + im2[x] * a;
                double re = (re1[x] + re2[x] * a) / b;
                double im = (re2[x] - re1[x] * a) / b;

                outArray[x * 2] = new VipsPel(re / Math.Sqrt(re * re + im * im));
                outArray[(x * 2) + 1] = new VipsPel(im / Math.Sqrt(re * re + im * im));
            }
            else
            {
                double y2 = im2[x];
                double a = im1[x] / y2;
                double b = im2[x] + im1[x] * a;
                double re = (re1[x] * a + re2[x]) / b;
                double im = (re2[x] * a - re1[x]) / b;

                outArray[x * 2] = new VipsPel(re / Math.Sqrt(re * re + im * im));
                outArray[(x * 2) + 1] = new VipsPel(im / Math.Sqrt(re * re + im * im));
            }
        }
    }

    public override void ClassInit()
    {
        base.ClassInit();

        VipsObjectClass object_class = (VipsObjectClass)GetType();
        VipsArithmeticClass arithmetic_class = (VipsArithmeticClass)typeof(VipsComplex2).GetNestedType("Arithmetic", BindingFlags.NonPublic);

        object_class.Nickname = "complex2";
        object_class.Description = "perform a complex binary operation on two images";

        arithmetic_class.ProcessLine += ProcessLine;
    }
}

public class VipsComplexget : VipsUnary
{
    public enum Operation { Real, Imag };

    private Operation get;

    public VipsComplexget(Operation get)
    {
        this.get = get;
    }

    public override int ProcessLine(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int sz = width * VipsImage.GetBands(im);

        switch (get)
        {
            case Operation.Real:
                Real(outArray, inArray);
                break;
            case Operation.Imag:
                Imag(outArray, inArray);
                break;
            default:
                throw new ArgumentException("Invalid operation");
        }

        return 0;
    }

    private void Real(VipsPel[] outArray, VipsPel[][] inArray)
    {
        for (int x = 0; x < sz; x++)
        {
            double re = inArray[0][x].ToDouble();
            double im = inArray[1][x].ToDouble();

            outArray[x] = new VipsPel(re);
        }
    }

    private void Imag(VipsPel[] outArray, VipsPel[][] inArray)
    {
        for (int x = 0; x < sz; x++)
        {
            double re = inArray[0][x].ToDouble();
            double im = inArray[1][x].ToDouble();

            outArray[x] = new VipsPel(im);
        }
    }

    public override void ClassInit()
    {
        base.ClassInit();

        VipsObjectClass object_class = (VipsObjectClass)GetType();
        VipsArithmeticClass arithmetic_class = (VipsArithmeticClass)typeof(VipsComplexget).GetNestedType("Arithmetic", BindingFlags.NonPublic);

        object_class.Nickname = "complexget";
        object_class.Description = "get a component from a complex image";

        arithmetic_class.ProcessLine += ProcessLine;
    }
}

public class VipsComplexform : VipsBinary
{
    public override int Build(VipsObject object)
    {
        if (object.Left != null && !VipsImage.IsNoncomplex(object.Nickname, object.Left))
            return -1;

        if (object.Right != null && !VipsImage.IsNoncomplex(object.Nickname, object.Right))
            return -1;

        return base.Build(object);
    }

    public override int ProcessLine(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int sz = width * VipsImage.GetBands(im);

        switch (im.Format)
        {
            case VipsFormat.UChar:
                CForm(outArray, inArray);
                break;
            case VipsFormat.Char:
                CForm(outArray, inArray);
                break;
            case VipsFormat.UInt:
                CForm(outArray, inArray);
                break;
            case VipsFormat.Int:
                CForm(outArray, inArray);
                break;
            case VipsFormat.Float:
                CForm(outArray, inArray);
                break;
            case VipsFormat.Double:
                CForm(outArray, inArray);
                break;
            default:
                throw new ArgumentException("Invalid format");
        }

        return 0;
    }

    private void CForm(VipsPel[] outArray, VipsPel[][] inArray)
    {
        for (int x = 0; x < sz; x++)
        {
            double re1 = inArray[0][x].ToDouble();
            double im1 = inArray[1][x].ToDouble();

            outArray[x * 2] = new VipsPel(re1);
            outArray[(x * 2) + 1] = new VipsPel(im1);
        }
    }

    public override void ClassInit()
    {
        base.ClassInit();

        VipsObjectClass object_class = (VipsObjectClass)GetType();
        VipsArithmeticClass arithmetic_class = (VipsArithmeticClass)typeof(VipsComplexform).GetNestedType("Arithmetic", BindingFlags.NonPublic);

        object_class.Nickname = "complexform";
        object_class.Description = "form a complex image from two real images";

        arithmetic_class.ProcessLine += ProcessLine;
    }
}
```