Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class Multiply : Arithmetic
{
    public Multiply()
        : base("multiply", "multiply two images")
    {
    }

    protected override void ProcessLine(VipsImage image, VipsPel[] outArray, VipsPel[][] inArrays)
    {
        int width = image.Width;
        int sz = width * image.Bands;

        switch (image.Format)
        {
            case VipsFormat.Char:
                MultiplyBuffer(signed char, signed short, outArray, inArrays);
                break;
            case VipsFormat.UChar:
                MultiplyBuffer(unsigned char, signed short, outArray, inArrays);
                break;
            case VipsFormat.Short:
                MultiplyBuffer(signed short, signed int, outArray, inArrays);
                break;
            case VipsFormat.UShort:
                MultiplyBuffer(unsigned short, signed int, outArray, inArrays);
                break;
            case VipsFormat.Int:
                MultiplyBuffer(signed int, signed int, outArray, inArrays);
                break;
            case VipsFormat.UInt:
                MultiplyBuffer(unsigned int, signed int, outArray, inArrays);
                break;
            case VipsFormat.Float:
                MultiplyBuffer(float, float, outArray, inArrays);
                break;
            case VipsFormat.Double:
                MultiplyBuffer(double, double, outArray, inArrays);
                break;

            case VipsFormat.Complex:
                MultiplyComplexBuffer(float, outArray, inArrays);
                break;
            case VipsFormat.DPComplex:
                MultiplyComplexBuffer(double, outArray, inArrays);
                break;

            default:
                throw new ArgumentException("Unsupported format");
        }
    }

    private void MultiplyBuffer<T1, T2>(T1 leftType, T2 rightType, VipsPel[] outArray, VipsPel[][] inArrays)
    {
        int x;
        T1 left = (T1)inArrays[0][0];
        T1 right = (T1)inArrays[1][0];

        for (x = 0; x < outArray.Length; x++)
        {
            outArray[x] = (T2)(left * right);
            left++;
            right++;
        }
    }

    private void MultiplyComplexBuffer<T>(VipsPel[] outArray, VipsPel[][] inArrays)
    {
        int x;
        T leftReal = (T)inArrays[0][0];
        T leftImaginary = (T)inArrays[0][1];
        T rightReal = (T)inArrays[1][0];
        T rightImaginary = (T)inArrays[1][1];

        for (x = 0; x < outArray.Length / 2; x++)
        {
            outArray[x * 2] = (T)(leftReal * rightReal - leftImaginary * rightImaginary);
            outArray[x * 2 + 1] = (T)(leftReal * rightImaginary + leftImaginary * rightReal);

            leftReal++;
            leftImaginary++;
            rightReal++;
            rightImaginary++;
        }
    }

    private static readonly VipsBandFormat[] MultiplyFormatTable = new[]
    {
        VipsBandFormat.US, VipsBandFormat.S, VipsBandFormat.UI, VipsBandFormat.I,
        VipsBandFormat.UI, VipsBandFormat.I, VipsBandFormat.F, VipsBandFormat.X,
        VipsBandFormat.D, VipsBandFormat.DX
    };

    protected override void ClassInit()
    {
        base.ClassInit();

        FormatTable = MultiplyFormatTable;
    }

    public static int Multiply(VipsImage left, VipsImage right, out VipsImage output)
    {
        return Arithmetic.Call("multiply", left, right, out output);
    }
}
```

This code defines a `Multiply` class that inherits from the `Arithmetic` class. It overrides the `ProcessLine` method to perform the multiplication operation and implements the `ClassInit` method to set up the format table for type promotion.

The `MultiplyBuffer` method is used to multiply two arrays of numbers, while the `MultiplyComplexBuffer` method is used to multiply two complex numbers.

The `MultiplyFormatTable` array defines the type promotion rules for multiplication, which are used in the `ProcessLine` method to determine the output format based on the input formats.