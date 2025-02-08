Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class EdgeDetector : Operation
{
    public Image In { get; set; }
    public Image Out { get; set; }

    protected override int Build()
    {
        if (In.BandFmt == BandFormat.UChar)
        {
            return UcharPath();
        }
        else
        {
            return FloatPath();
        }
    }

    private int UcharPath()
    {
        var t = new Image[20];
        var mask = In.Mask;

        // For uchar, use 128 as the zero and divide the result by 2 to prevent overflow.
        if (Vips.Copy(mask, ref t[1], null))
            return -1;
        Vips.Image.SetDouble(t[1], "offset", 128.0);
        Vips.Image.SetDouble(t[1], "scale", 2.0);

        // Fast uchar path
        if (Vips.Conv(In, ref t[3], t[1],
            "precision", Precision.Integer,
            null))
            return -1;

        if (Vips.Rot90(t[1], ref t[5], null) ||
            Vips.Conv(In, ref t[7], t[5],
                "precision", Precision.Integer,
                null))
            return -1;

        Out = new Image();
        edge.args[0] = t[3];
        edge.args[1] = t[7];
        edge.args[2] = null;
        if (Vips.ImagePipelineArray(Out, DemandStyle.FatStrip, edge.args))
            return -1;

        if (Vips.ImageGenerate(Out,
                Vips.StartMany, UcharGen, Vips.StopMany,
                edge.args, null))
            return -1;

        return 0;
    }

    private int FloatPath()
    {
        var t = new Image[20];
        var mask = In.Mask;

        if (Vips.Rot90(mask, ref t[0], null) ||
            Vips.Conv(In, ref t[1], mask, null) ||
            Vips.Conv(In, ref t[2], t[0], null))
            return -1;

        if (Vips.Multiply(t[1], t[1], ref t[3], null) ||
            Vips.Multiply(t[2], t[2], ref t[4], null) ||
            Vips.Add(t[3], t[4], ref t[5], null) ||
            Vips.PowConst1(t[5], ref t[6], 0.5, null) ||
            Vips.CastUChar(t[6], ref t[7], null))
            return -1;

        Out = new Image();
        if (Vips.ImageWrite(t[7], Out))
            return -1;

        return 0;
    }

    private int UcharGen(VipsRegion out_region,
        void* vseq, void* a, void* b, bool* stop)
    {
        var in = (VipsRegion[])vseq;
        var r = out_region.Valid;
        var sz = r.Width * in[0].Im.Bands;

        int x, y;

        if (Vips.ReorderPrepareMany(out_region.Im, in, r))
            return -1;

        for (y = 0; y < r.Height; y++)
        {
            var p1 = Vips.RegionAddr(in[0], r.Left, r.Top + y);
            var p2 = Vips.RegionAddr(in[1], r.Left, r.Top + y);
            var q = Vips.RegionAddr(out_region, r.Left, r.Top + y);

            for (x = 0; x < sz; x++)
            {
                int v1 = 2 * (p1[x] - 128);
                int v2 = 2 * (p2[x] - 128);
                // Avoid the sqrt() for uchar.
                int v = Vips.Abs(v1) + Vips.Abs(v2);

                q[x] = v > 255 ? 255 : v;
            }
        }

        return 0;
    }
}

public class SobelEdgeDetector : EdgeDetector
{
    public override int Build()
    {
        var mask = new Image(new double[,] { {1, 2, 1}, {0, 0, 0}, {-1, -2, -1} });
        In.Mask = mask;
        return base.Build();
    }
}

public class ScharrEdgeDetector : EdgeDetector
{
    public override int Build()
    {
        var mask = new Image(new double[,] { {-3, 0, 3}, {-10, 0, 10}, {-3, 0, 3} });
        In.Mask = mask;
        return base.Build();
    }
}

public class PrewittEdgeDetector : EdgeDetector
{
    public override int Build()
    {
        var mask = new Image(new double[,] { {-1, 0, 1}, {-1, 0, 1}, {-1, 0, 1} });
        In.Mask = mask;
        return base.Build();
    }
}

public class VipsSobel : SobelEdgeDetector
{
    public static int Call(VipsImage in, ref VipsImage out)
    {
        var edge = new SobelEdgeDetector { In = in };
        if (edge.Build())
            return -1;
        out = edge.Out;
        return 0;
    }
}

public class VipsScharr : ScharrEdgeDetector
{
    public static int Call(VipsImage in, ref VipsImage out)
    {
        var edge = new ScharrEdgeDetector { In = in };
        if (edge.Build())
            return -1;
        out = edge.Out;
        return 0;
    }
}

public class VipsPrewitt : PrewittEdgeDetector
{
    public static int Call(VipsImage in, ref VipsImage out)
    {
        var edge = new PrewittEdgeDetector { In = in };
        if (edge.Build())
            return -1;
        out = edge.Out;
        return 0;
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available, and you'll need to replace it with your actual VIPS .NET wrapper. Also, some methods like `Vips.Copy`, `Vips.Conv`, etc., might have different names or signatures in your specific VIPS implementation.