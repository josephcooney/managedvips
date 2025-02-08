```csharp
// vips_sign.c

using System;

public class VipsSign : VipsUnary
{
    public override void ProcessLine(VipsArithmetic arithmetic, VipsPel[] out, VipsPel[][] in, int width)
    {
        VipsUnary unary = (VipsUnary)arithmetic;
        int bands = vips_image_get_bands(unary.in);
        int sz = width * bands;

        switch (vips_image_get_format(unary.in))
        {
            case VIPS_FORMAT_UCHAR:
                Sign((unsigned char[])in[0], out, sz);
                break;
            case VIPS_FORMAT_CHAR:
                Sign((signed char[])in[0], out, sz);
                break;
            case VIPS_FORMAT_USHORT:
                Sign((ushort[])in[0], out, sz);
                break;
            case VIPS_FORMAT_SHORT:
                Sign((short[])in[0], out, sz);
                break;
            case VIPS_FORMAT_UINT:
                Sign((uint[])in[0], out, sz);
                break;
            case VIPS_FORMAT_INT:
                Sign((int[])in[0], out, sz);
                break;
            case VIPS_FORMAT_FLOAT:
                Sign((float[])in[0], out, sz);
                break;
            case VIPS_FORMAT_DOUBLE:
                Sign((double[])in[0], out, sz);
                break;
            case VIPS_FORMAT_COMPLEX:
                CSign((float[])in[0], (float[])out, sz);
                break;
            case VIPS_FORMAT_DPCOMPLEX:
                CSign((double[])in[0], (double[])out, sz);
                break;

            default:
                throw new ArgumentException("Invalid format");
        }
    }

    private void Sign<T>(T[] inArray, T[] outArray, int length) where T : struct
    {
        for (int i = 0; i < length; i++)
        {
            if (inArray[i] > 0)
                outArray[i] = 1;
            else if (inArray[i] == 0)
                outArray[i] = 0;
            else
                outArray[i] = -1;
        }
    }

    private void CSign<T>(T[] inArray, T[] outArray, int length) where T : struct
    {
        for (int i = 0; i < length; i++)
        {
            float re = inArray[2 * i];
            float im = inArray[2 * i + 1];
            double fac = Math.Sqrt(re * re + im * im);

            if (fac == 0)
                outArray[2 * i] = 0;
            else
                outArray[2 * i] = re / fac;

            if (fac == 0)
                outArray[2 * i + 1] = 0;
            else
                outArray[2 * i + 1] = im / fac;
        }
    }

    public override VipsBandFormat[] FormatTable => new VipsBandFormat[]
    {
        // Band format:  UC C  US S  UI I  F  X  D  DX
        // Promotion:
        VIPS_FORMAT_CHAR, VIPS_FORMAT_CHAR, VIPS_FORMAT_CHAR, VIPS_FORMAT_CHAR,
        VIPS_FORMAT_CHAR, VIPS_FORMAT_CHAR, VIPS_FORMAT_CHAR, VIPS_FORMAT_COMPLEX,
        VIPS_FORMAT_CHAR, VIPS_FORMAT_DPCOMPLEX
    };
}

public class VipsSignClass : VipsUnaryClass
{
    public override void ClassInit(VipsObjectClass object_class)
    {
        base.ClassInit(object_class);
        VipsArithmeticClass aclass = (VipsArithmeticClass)object_class;
        aclass.ProcessLine = new VipsArithmetic.ProcessLineDelegate((arithmetic, outArray, inArrays, width) => ((VipsSign)arithmetic).ProcessLine(arithmetic, outArray, inArrays, width));
    }

    public override void Init(VipsSign sign)
    {
        base.Init(sign);
    }
}

public class VipsImage
{
    public static int GetBands(VipsImage image)
    {
        // implementation...
    }

    public static int GetFormat(VipsImage image)
    {
        // implementation...
    }
}

public class VipsArithmetic
{
    public delegate void ProcessLineDelegate(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width);

    public ProcessLineDelegate ProcessLine { get; set; }

    public static VipsBandFormat[] FormatTable { get; set; }
}

public class VipsUnary : VipsArithmetic
{
    public VipsImage In => (VipsImage)in;
}
```