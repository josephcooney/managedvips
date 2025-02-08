Here is the C# code equivalent to the provided C code:

```csharp
// min of a pair of images
//
// 18/6/24
//     - from maxpair.c

using System;

namespace Vips {
    public class MinPair : Arithmetic {
        // Minpair all input types. Keep types here in sync with
        // vips_minpair_format_table[] below.
        private static readonly BandFormat[] formatTable = new BandFormat[] {
            BandFormat.UChar,
            BandFormat.Char,
            BandFormat.UShort,
            BandFormat.Short,
            BandFormat.UInt,
            BandFormat.Int,
            BandFormat.Float,
            BandFormat.Complex,
            BandFormat.Double,
            BandFormat.DComplex
        };

        public override void ProcessLine(Arithmetic arithmetic, Pel[] outArray, Image[] inImages) {
            Image image = inImages[0];
            int bands = image.GetBands();
            int sz = width * bands * (image.Format.IsComplex ? 2 : 1);

            switch (image.Format) {
                case BandFormat.UChar:
                    Loop<unsigned char>(outArray, (unsigned char[])inImages[0].GetData(), (unsigned char[])inImages[1].GetData());
                    break;

                case BandFormat.Char:
                    Loop<signed char>(outArray, (signed char[])inImages[0].GetData(), (signed char[])inImages[1].GetData());
                    break;

                case BandFormat.UShort:
                    Loop<unsigned short>(outArray, (unsigned short[])inImages[0].GetData(), (unsigned short[])inImages[1].GetData());
                    break;

                case BandFormat.Short:
                    Loop<short>(outArray, (short[])inImages[0].GetData(), (short[])inImages[1].GetData());
                    break;

                case BandFormat.UInt:
                    Loop<uint>(outArray, (uint[])inImages[0].GetData(), (uint[])inImages[1].GetData());
                    break;

                case BandFormat.Int:
                    Loop<int>(outArray, (int[])inImages[0].GetData(), (int[])inImages[1].GetData());
                    break;

                case BandFormat.Float:
                case BandFormat.Complex:
                    FLoop<float>(outArray, (float[])inImages[0].GetData(), (float[])inImages[1].GetData());
                    break;

                case BandFormat.Double:
                case BandFormat.DComplex:
                    FLoop<double>(outArray, (double[])inImages[0].GetData(), (double[])inImages[1].GetData());
                    break;

                default:
                    throw new ArgumentException("Unsupported image format");
            }
        }

        private static void Loop<T>(Pel[] outArray, T[] leftData, T[] rightData) where T : struct {
            for (int x = 0; x < width * GetBands(); x++) {
                outArray[x] = Math.Min(leftData[x], rightData[x]);
            }
        }

        private static void FLoop<T>(Pel[] outArray, T[] leftData, T[] rightData) where T : struct {
            for (int x = 0; x < width * GetBands(); x++) {
                outArray[x] = Math.Min(leftData[x], rightData[x]);
            }
        }

        private static int width;
        private static int GetBands() => width * formatTable.Length;

        public MinPair() : base("minpair", "minimum of a pair of images") { }

        protected override void ClassInit() {
            base.ClassInit();
            SetFormatTable(formatTable);
        }
    }

    // vips_minpair:
    //
    // @left: input image
    // @right: input image
    // @out: (out): output image
    // @...: %NULL-terminated list of optional named arguments
    //
    // For each pixel, pick the minimum of a pair of images.
    //
    // See also: vips_minpair().
    //
    // Returns: 0 on success, -1 on error
    public static int MinPair(Image left, Image right, out Image outImage) {
        return CallSplit("minpair", left, right, out outImage);
    }
}
```

Note that I've assumed the existence of `Vips` namespace and its classes (`Arithmetic`, `Image`, etc.) as they are not provided in the original C code. Also, some methods like `GetData()` and `GetBands()` are assumed to be part of the `Image` class.

Also note that this is a direct translation from C to C# and might need some adjustments based on your specific requirements and the actual implementation of the `Vips` namespace.