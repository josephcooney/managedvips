Here is the C# code equivalent to the provided C code:

```csharp
// max of a pair of images
//
// 18/6/24
// 	- from add.c

using System;

namespace Vips
{
    public class Maxpair : Arithmetic
    {
        // Maxpair all input types. Keep types here in sync with vips_maxpair_format_table[] below.
        protected override void ProcessLine(VipsPel[] outArray, VipsImage[] inImages)
        {
            int width = inImages[0].Width;
            int bands = inImages[0].Bands;
            int sz = width * bands * (inImages[0].Format.IsComplex ? 2 : 1);

            switch (inImages[0].Format)
            {
                case VipsBandFormat.UCHAR:
                    Loop<unsigned char>(outArray, inImages);
                    break;

                case VipsBandFormat.CHAR:
                    Loop<signed char>(outArray, inImages);
                    break;

                case VipsBandFormat.USHORT:
                    Loop<unsigned short>(outArray, inImages);
                    break;

                case VipsBandFormat.SHORT:
                    Loop<signed short>(outArray, inImages);
                    break;

                case VipsBandFormat.UINT:
                    Loop<unsigned int>(outArray, inImages);
                    break;

                case VipsBandFormat.INT:
                    Loop<signed int>(outArray, inImages);
                    break;

                case VipsBandFormat.FLOAT:
                case VipsBandFormat.COMPLEX:
                    FLoop<float>(outArray, inImages);
                    break;

                case VipsBandFormat.DOUBLE:
                case VipsBandFormat.DPCOMPLEX:
                    FLoop<double>(outArray, inImages);
                    break;

                default:
                    throw new ArgumentException("Unsupported format");
            }
        }

        private void Loop<T>(VipsPel[] outArray, VipsImage[] inImages) where T : struct
        {
            T[] left = (T[])inImages[0].GetData();
            T[] right = (T[])inImages[1].GetData();
            T[] q = (T[])outArray;

            for (int x = 0; x < outArray.Length; x++)
                q[x] = Math.Max(left[x], right[x]);
        }

        private void FLoop<T>(VipsPel[] outArray, VipsImage[] inImages) where T : struct
        {
            T[] left = (T[])inImages[0].GetData();
            T[] right = (T[])inImages[1].GetData();
            T[] q = (T[])outArray;

            for (int x = 0; x < outArray.Length; x++)
                q[x] = Math.Max(left[x], right[x]);
        }
    }

    public class MaxpairClass : ArithmeticClass
    {
        public override string Nickname => "maxpair";
        public override string Description => "maximum of a pair of images";

        protected override void ClassInit()
        {
            base.ClassInit();

            FormatTable = new VipsBandFormat[]
            {
                // Band format:  UC  C  US  S  UI  I  F  X  D  DX
                // Promotion:
                VipsBandFormat.UCHAR, VipsBandFormat.CHAR, VipsBandFormat.USHORT,
                VipsBandFormat.SHORT, VipsBandFormat.UINT, VipsBandFormat.INT,
                VipsBandFormat.FLOAT, VipsBandFormat.COMPLEX, VipsBandFormat.DOUBLE,
                VipsBandFormat.DPCOMPLEX
            };
        }
    }

    public class MaxpairInit : ArithmeticInit
    {
    }

    public static class MaxpairExtensions
    {
        public static int Maxpair(VipsImage left, VipsImage right, out VipsImage output)
        {
            return Arithmetic.Call("maxpair", left, right, out output);
        }
    }
}
```

Note that I've assumed the existence of a `VipsPel` struct and an `Arithmetic` class, as well as various other classes and methods that are not shown in the provided C code. You will need to modify this code to fit your specific use case.

Also note that I've used the `Math.Max` method instead of defining a custom `FLoop` function for floating-point types. This is because `Math.Max` is a built-in method in C# that can handle both integer and floating-point types.