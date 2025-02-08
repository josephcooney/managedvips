Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsDivide : VipsArithmetic
{
    public override void ProcessLine(VipsImage image)
    {
        int x;
        int width = image.Width;
        int height = image.Height;
        int bands = image.Bands;

        // Keep types here in sync with vips_divide_format_table[]
        switch (image.Format)
        {
            case VipsFormat.Char:
                RLoop((char[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.UChar:
                RLoop((ushort[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.Short:
                RLoop((short[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.UShort:
                RLoop((ushort[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.Int:
                RLoop((int[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.UInt:
                RLoop((uint[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.Float:
                RLoop((float[])image.Data, (float[])image.Data);
                break;
            case VipsFormat.Double:
                RLoop((double[])image.Data, (double[])image.Data);
                break;
            case VipsFormat.Complex:
                CLoop((float[])image.Data);
                break;
            case VipsFormat.DComplex:
                CLoop((double[])image.Data);
                break;

            default:
                throw new ArgumentException("Invalid image format");
        }
    }

    private void RLoop<T>(T[] left, T[] right)
    {
        int x;
        for (x = 0; x < left.Length; x++)
        {
            if (right[x] == 0)
            {
                left[x] = 0;
            }
            else
            {
                left[x] = (T)(left[x] / right[x]);
            }
        }
    }

    private void CLoop(float[] data)
    {
        int x;
        for (x = 0; x < data.Length; x += 2)
        {
            float real1 = data[x];
            float imag1 = data[x + 1];

            if (data[x + 2] == 0 && data[x + 3] == 0)
            {
                data[x] = 0;
                data[x + 1] = 0;
            }
            else
            {
                double a = data[x + 3] / data[x + 2];
                double b = data[x + 2] + data[x + 3] * a;

                data[x] = (float)((real1 + imag1 * a) / b);
                data[x + 1] = (float)((imag1 - real1 * a) / b);
            }
        }
    }

    private static readonly VipsBandFormat[] vips_divide_format_table =
    {
        // Band format: UC C US S UI I F X D DX
        // Promotion:
        VipsFormat.Float, VipsFormat.Float, VipsFormat.Float,
        VipsFormat.Float, VipsFormat.Float, VipsFormat.Float,
        VipsFormat.Float, VipsFormat.Complex, VipsFormat.Double,
        VipsFormat.DComplex
    };

    public override void ClassInit()
    {
        base.ClassInit();
        this.Nickname = "divide";
        this.Description = "Divide two images";

        this.ProcessLine = ProcessLine;
        this.FormatTable = vips_divide_format_table;
    }

    public static int Divide(VipsImage left, VipsImage right, out VipsImage output)
    {
        return VipsCallSplit("divide", left, right, out output);
    }
}
```

Note that I've assumed the existence of a `VipsArithmetic` class and its methods, as well as a `VipsImage` class with properties like `Width`, `Height`, `Bands`, and `Data`. You may need to modify the code to fit your specific implementation.