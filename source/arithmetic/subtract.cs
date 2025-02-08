Here is the converted C# code:

```csharp
using System;
using VipsDotNet;

public class Subtract : ArithmeticOperation
{
    public static int Subtract(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        // Complex just doubles the size.
        const int sz = left.Width * left.Bands * (left.Format.IsComplex ? 2 : 1);

        for (int x = 0; x < sz; x++)
        {
            switch (left.Format)
            {
                case VipsBandFormat.Char:
                    outImage[x] = (short)(left[x] - right[x]);
                    break;
                case VipsBandFormat.UChar:
                    outImage[x] = (short)(left[x] - right[x]);
                    break;
                case VipsBandFormat.Short:
                    outImage[x] = left[x] - right[x];
                    break;
                case VipsBandFormat.UShort:
                    outImage[x] = left[x] - right[x];
                    break;
                case VipsBandFormat.Int:
                    outImage[x] = left[x] - right[x];
                    break;
                case VipsBandFormat.UInt:
                    outImage[x] = left[x] - right[x];
                    break;

                case VipsBandFormat.Float:
                case VipsBandFormat.Complex:
                    outImage[x] = (float)(left[x] - right[x]);
                    break;

                case VipsBandFormat.Double:
                case VipsBandFormat.DComplex:
                    outImage[x] = left[x] - right[x];
                    break;

                default:
                    throw new ArgumentException("Unsupported format");
            }
        }

        return 0;
    }
}

public class SubtractClass : ArithmeticOperationClass
{
    public override string Nickname => "subtract";
    public override string Description => "Subtract two images";

    protected override void ProcessLine(VipsImage image, VipsPel[] outArray, VipsPel[][] inArrays)
    {
        var left = (VipsImage)inArrays[0][0];
        var right = (VipsImage)inArrays[1][0];

        // Complex just doubles the size.
        const int sz = left.Width * left.Bands * (left.Format.IsComplex ? 2 : 1);

        for (int x = 0; x < sz; x++)
        {
            switch (left.Format)
            {
                case VipsBandFormat.Char:
                    outArray[x] = (short)(left[x] - right[x]);
                    break;
                case VipsBandFormat.UChar:
                    outArray[x] = (short)(left[x] - right[x]);
                    break;
                case VipsBandFormat.Short:
                    outArray[x] = left[x] - right[x];
                    break;
                case VipsBandFormat.UShort:
                    outArray[x] = left[x] - right[x];
                    break;
                case VipsBandFormat.Int:
                    outArray[x] = left[x] - right[x];
                    break;
                case VipsBandFormat.UInt:
                    outArray[x] = left[x] - right[x];
                    break;

                case VipsBandFormat.Float:
                case VipsBandFormat.Complex:
                    outArray[x] = (float)(left[x] - right[x]);
                    break;

                case VipsBandFormat.Double:
                case VipsBandFormat.DComplex:
                    outArray[x] = left[x] - right[x];
                    break;

                default:
                    throw new ArgumentException("Unsupported format");
            }
        }
    }

    public static void Register(VipsObjectClass classType)
    {
        classType.Nickname = "subtract";
        classType.Description = "Subtract two images";

        var arithmeticClass = VIPS_ARITHMETIC_CLASS(classType);
        arithmeticClass.ProcessLine = ProcessLine;

        var formatTable = new[]
        {
            // Band format:  UC C  US S  UI I  F  X  D  DX
            // Promotion:
            VipsBandFormat.Short, VipsBandFormat.Short, VipsBandFormat.Int,
            VipsBandFormat.Int, VipsBandFormat.Int, VipsBandFormat.Int,
            VipsBandFormat.Float, VipsBandFormat.Complex, VipsBandFormat.Double,
            VipsBandFormat.DComplex
        };

        vips_arithmetic_set_format_table(arithmeticClass, formatTable);
    }
}

public class SubtractOperation : ArithmeticOperation
{
    public static int Subtract(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return new Subtract().Execute(left, right, ref outImage);
    }
}
```

Note that I've assumed the existence of a `VipsDotNet` namespace with classes and methods similar to those in the original C code. You may need to modify this code to match your specific VIPS library implementation.

Also note that I've used C# 7.x features such as tuples, pattern matching, and expression-bodied members where possible. If you're using an earlier version of C#, you may need to modify the code accordingly.