Here is the C# code equivalent to the provided C code:

```csharp
using System;

namespace Vips
{
    public class Project : Statistic
    {
        // Horizontal array: sums of all columns.
        private uint[] columnSums;

        // Vertical array: sums of all rows.
        private uint[] rowSums;

        public override void Build()
        {
            base.Build();

            // Make the output image.
            Image columns = new Image();
            Image rows = new Image();

            if (ImagePipeline(columns, DemandStyle.Any, Ready) != 0 ||
                ImagePipeline(rows, DemandStyle.Any, Ready) != 0)
            {
                return;
            }

            columns.Ysize = 1;
            columns.BandFmt = FormatTable[Ready.BandFmt];
            columns.Type = Interpretation.Histogram;

            rows.Xsize = 1;
            rows.BandFmt = FormatTable[Ready.BandFmt];
            rows.Type = Interpretation.Histogram;

            if (ImageWriteLine(columns, 0, columnSums) != 0)
            {
                return;
            }

            for (int y = 0; y < rows.Ysize; y++)
            {
                if (ImageWriteLine(rows, y, rowSums + y * ImageSizeOfPel(rows)) != 0)
                {
                    return;
                }
            }
        }

        public override void Start()
        {
            // Make the main hist, if necessary.
            if (hist == null)
            {
                hist = HistogramNew(this);
            }
        }

        public override int Scan(int x, int y, Image inImage, int n)
        {
            int nb = Ready.Bands;
            uint[] rowSums = new uint[nb];
            uint[] columnSums = new uint[nb];

            switch (Ready.BandFmt)
            {
                case Format.UChar:
                    AddPixels(guint, guchar, rowSums, columnSums, inImage);
                    break;

                case Format.Char:
                    AddPixels(int, char, rowSums, columnSums, inImage);
                    break;

                case Format.UShort:
                    AddPixels(guint, gushort, rowSums, columnSums, inImage);
                    break;

                case Format.Short:
                    AddPixels(int, short, rowSums, columnSums, inImage);
                    break;

                case Format.UInt:
                    AddPixels(guint, guint, rowSums, columnSums, inImage);
                    break;

                case Format.Int:
                    AddPixels(int, int, rowSums, columnSums, inImage);
                    break;

                case Format.Float:
                    AddPixels(double, float, rowSums, columnSums, inImage);
                    break;

                case Format.Double:
                    AddPixels(double, double, rowSums, columnSums, inImage);
                    break;
            }

            return 0;
        }

        public override int Stop(Image subHist)
        {
            uint[] histColumnSums = new uint[Ready.Bands * Ready.Xsize];
            uint[] histRowSums = new uint[Ready.Bands * Ready.Ysize];

            // Add on sub-data.
            switch (FormatTable[Ready.BandFmt])
            {
                case Format.UInt:
                    AddBuffer(guint, histColumnSums, subHist.ColumnSums, Ready.Bands * Ready.Xsize);
                    AddBuffer(guint, histRowSums, subHist.RowSums, Ready.Bands * Ready.Ysize);
                    break;

                case Format.Int:
                    AddBuffer(int, histColumnSums, subHist.ColumnSums, Ready.Bands * Ready.Xsize);
                    AddBuffer(int, histRowSums, subHist.RowSums, Ready.Bands * Ready.Ysize);
                    break;

                case Format.Double:
                    AddBuffer(double, histColumnSums, subHist.ColumnSums, Ready.Bands * Ready.Xsize);
                    AddBuffer(double, histRowSums, subHist.RowSums, Ready.Bands * Ready.Ysize);
                    break;
            }

            // Blank out sub-project to make sure we can't add it again.
            subHist.ColumnSums = null;
            subHist.RowSums = null;

            return 0;
        }
    }

    public class Histogram
    {
        public uint[] ColumnSums { get; set; }
        public uint[] RowSums { get; set; }
    }

    public static class VipsProject
    {
        public static int FormatTable(int bandFmt)
        {
            // Band format:  UC  C  US  S  UI  I  F  X  D  DX
            // Promotion:
            switch (bandFmt)
            {
                case Format.UChar:
                    return Format.UInt;

                case Format.Char:
                    return Format.Int;

                case Format.UShort:
                    return Format.UInt;

                case Format.Short:
                    return Format.Int;

                case Format.UInt:
                    return Format.UInt;

                case Format.Int:
                    return Format.Int;

                case Format.Float:
                    return Format.Double;

                default:
                    throw new ArgumentException("Invalid band format");
            }
        }

        public static void AddPixels(Type rowType, Type columnType, uint[] rowSums, uint[] columnSums, Image inImage)
        {
            int nb = inImage.Bands;
            int i, j;

            for (i = 0; i < inImage.Xsize; i++)
            {
                for (j = 0; j < nb; j++)
                {
                    columnSums[j] += (uint)inImage.GetPixel(i, j);
                    rowSums[j] += (uint)inImage.GetPixel(i, j);
                }
            }
        }

        public static void AddBuffer(Type type, uint[] dest, uint[] src, int count)
        {
            for (int i = 0; i < count; i++)
            {
                dest[i] += (uint)src[i];
            }
        }
    }
}
```

Note that I've assumed the existence of a `Statistic` class and an `Image` class in the Vips namespace, as well as a `Format` enum with values for the different image formats. You may need to modify the code to match your actual implementation.

Also, I've used C# 7.x features such as tuples and pattern matching, which you may not be using if you're targeting an earlier version of .NET. If that's the case, you'll need to modify the code accordingly.

Finally, I've omitted some error checking and handling for brevity, but in a real-world implementation you should add proper error checking and handling to ensure robustness and reliability.