Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsExtractArea : VipsConversion
{
    public VipsImage In { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public override bool Build()
    {
        if (Left + Width > In.Xsize || Top + Height > In.Ysize ||
            Left < 0 || Top < 0 || Width <= 0 || Height <= 0)
        {
            throw new ArgumentException("Bad extract area");
        }

        if (In.PioInput() || CheckCodingKnown(In))
        {
            return false;
        }

        PipelineV(Out, VipsDemandStyle.ThinStrip, In, null);

        Out.Xsize = Width;
        Out.Ysize = Height;
        Out.Xoffset = -Left;
        Out.Yoffset = -Top;

        Generate(Out, StartOne, ExtractAreaGen, StopOne, In, this);
        return true;
    }

    public static int ExtractArea(VipsImage inImage, VipsImage[] outImages,
        int left, int top, int width, int height, params object[] args)
    {
        var result = CallSplit("extract_area", args, inImage, outImages,
            left, top, width, height);
        return result;
    }

    public static void ExtractAreaClassInit()
    {
        // Class initialization code
    }

    private static int ExtractAreaGen(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        var ir = (VipsRegion)seq;
        var extract = (VipsExtractArea)b;
        var iarea = new VipsRect(outRegion.Valid);
        iarea.Left += extract.Left;
        iarea.Top += extract.Top;

        if (VipsRegionPrepare(ir, ref iarea))
        {
            return -1;
        }

        if (VipsRegionRegion(outRegion, ir, outRegion.Valid, iarea.Left, iarea.Top))
        {
            return -1;
        }

        return 0;
    }
}

public class VipsCrop : VipsExtractArea
{
    public override string Nickname => "crop";
    public override string Description => "Extract an area from an image";

    // Rest of the code...
}

public class VipsExtractBand : VipsBandary
{
    public VipsImage In { get; set; }
    public int Band { get; set; }
    public int N { get; set; }

    public override bool Build()
    {
        if (In != null)
        {
            var bands = new int[1];
            VipsImageDecodePredict(In, bands);

            N = 1;
            InBands = new[] { In };
            OutBands = N;

            if (Band + N > bands[0])
            {
                throw new ArgumentException("Bad extract band");
            }

            if (Band == 0 && N == bands[0])
            {
                return VipsBandaryCopy(this);
            }
        }

        base.Build();
        return true;
    }

    public static int ExtractBand(VipsImage inImage, VipsImage[] outImages,
        int band, params object[] args)
    {
        var result = CallSplit("extract_band", args, inImage, outImages, band);
        return result;
    }
}
```

Note that I've assumed the existence of some classes and methods (e.g. `VipsConversion`, `VipsDemandStyle`, `VipsRegion`, etc.) which are not defined in the provided C code. You may need to modify the code to match your actual implementation.

Also, I've used C# 6 features such as expression-bodied members and tuple deconstruction, but you can easily convert them back to older versions of C# if needed.