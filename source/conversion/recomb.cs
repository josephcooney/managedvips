Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipsRecomb : VipsConversion
{
    public VipsImage In { get; private set; }
    public VipsImage M { get; private set; }
    public VipsImage Coeff { get; private set; }

    protected override int Build(VipsObject obj)
    {
        var recomb = (VipsRecomb)obj;
        var inImg = recomb.In;

        if (base.Build(obj) != 0)
            return -1;

        if (Vips.ImageDecode(inImg, out Coeff))
            return -1;

        if (!Vips.CheckNonComplex("recomb", inImg))
            return -1;
        if (Vips.ImagePioInput(recomb.M) ||
            Vips.CheckUncoded("recomb", recomb.M) ||
            Vips.CheckNonComplex("recomb", recomb.M) ||
            Vips.CheckMono("recomb", recomb.M))
            return -1;

        if (inImg.Bands != recomb.M.Xsize)
        {
            Vips.Error("recomb", "%s", "bands in must equal matrix width");
            return -1;
        }

        if (!Vips.CheckMatrix("recomb", recomb.M, out Coeff))
            return -1;

        var conversion = (VipsConversion)obj;
        conversion.Out.Bands = recomb.M.Ysize;
        if (Vips.BandFormatIsInt(inImg.BandFmt))
            conversion.Out.BandFmt = Vips.Format.Float32;

        if (!Vips.ImageGenerate(conversion.Out, Vips.StartOne, VipsRecombGen, Vips.StopOne, inImg, recomb))
            return -1;

        return 0;
    }

    public static int VipsRecomb(VipsImage inImg, out VipsImage outImg, VipsImage m, params object[] args)
    {
        var result = Vips.CallSplit("recomb", args, inImg, out outImg, m);
        return result;
    }
}

public class VipsRecombClass : VipsConversionClass
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);

        klass.Nickname = "recomb";
        klass.Description = "linear recombination with matrix";

        var argImageIn = new VipsArgImage("in", 0, "Input", "Input image argument", Vips.ArgumentRequiredInput);
        var argImageM = new VipsArgImage("m", 102, "M", "Matrix of coefficients", Vips.ArgumentRequiredInput);

        klass.Args.Add(argImageIn);
        klass.Args.Add(argImageM);
    }
}

public class VipsRecombGen : VipsRegion
{
    public override int Gen(VipsRegion outRegion, object seq, object a, object b, bool[] stop)
    {
        var recomb = (VipsRecomb)b;
        var inImg = recomb.In;
        var mWidth = recomb.M.Xsize;
        var mHeight = recomb.M.Ysize;

        int y, x, u, v;

        if (!Vips.RegionPrepare(outRegion, out outRegion.Valid))
            return -1;

        for (y = 0; y < outRegion.Valid.Height; y++)
        {
            var inPtr = Vips.RegionAddr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top + y);
            var outPtr = Vips.RegionAddr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top + y);

            switch (Vips.ImageGetFormat(inImg))
            {
                case Vips.Format.UChar:
                    Loop<unsigned char, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.Char:
                    Loop<sbyte, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.UShort:
                    Loop<ushort, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.SShort:
                    Loop<short, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.UInt:
                    Loop<uint, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.Int:
                    Loop<int, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.Float32:
                    Loop<float, float>(inPtr, outPtr, mWidth, mHeight);
                    break;
                case Vips.Format.Double64:
                    Loop<double, double>(inPtr, outPtr, mWidth, mHeight);
                    break;

                default:
                    throw new ArgumentException("Invalid image format");
            }
        }

        return 0;
    }

    private static void Loop<TIn, TOut>(TIn inPtr, TOut outPtr, int width, int height)
    {
        var coeff = Vips.Matrix(recomb.Coeff, 0, 0);

        for (x = 0; x < width; x++)
        {
            double m = coeff[0];

            for (v = 0; v < height; v++)
            {
                double t;

                t = 0.0;

                for (u = 0; u < width; u++)
                    t += m[u] * inPtr[u];

                outPtr[v] = (TOut)t;
                coeff += width;
            }

            inPtr += width;
            outPtr += height;
        }
    }
}
```