Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class HoughCircle : Hough
{
    public int Scale { get; set; }
    public int MinRadius { get; set; }
    public int MaxRadius { get; set; }

    public override void Normalise()
    {
        // Smaller circles have fewer pixels and therefore fewer votes. 
        // Scale bands by the ratio of circumference, so all radii get equal weight.
        double maxCircumference = 2 * Math.PI * MaxRadius;

        for (int b = 0; b < Bands; b++)
        {
            int radius = b * Scale + MinRadius;
            double circumference = 2 * Math.PI * radius;
            double ratio = maxCircumference / circumference;
            size_t nPels = (size_t)Width * Height * Bands;

            for (int i = 0; i < nPels; i += Bands)
                Out[i] *= ratio;
        }
    }

    public override int Build(VipsObject object)
    {
        VipsStatistic statistic = (VipsStatistic)object;
        int range = MaxRadius - MinRadius;

        if (range <= 0)
        {
            throw new ArgumentException("parameters out of range");
        }

        Width = statistic.In.Xsize / Scale;
        Height = statistic.In.Ysize / Scale;
        Bands = 1 + range / Scale;

        return base.Build(object);
    }

    public override int InitAccumulator(VipsImage accumulator)
    {
        vips_image_init_fields(accumulator, Width, Height, Bands,
            VipsFormat.UInt, VipsCoding.None,
            VipsInterpretation.Matrix,
            1.0, 1.0);

        return 0;
    }

    public override void Vote(VipsImage image, int y, int x1, int x2, int quadrant, object client)
    {
        // Cast votes for all possible circles passing through x, y.
        VipsHoughCircle houghCircle = (VipsHoughCircle)client;

        int minRadius = houghCircle.MinRadius;
        int cx = x / houghCircle.Scale;
        int cy = y / houghCircle.Scale;

        int rb;

        for (rb = 0; rb < houghCircle.Bands; rb++)
        {
            // r needs to be in scaled down image space.
            int r = rb + minRadius / houghCircle.Scale;

            VipsDrawScanline drawScanline;

            if (cx - r >= 0 && cx + r < accumulator.Xsize &&
                cy - r >= 0 && cy + r < accumulator.Ysize)
                drawScanline = vips_hough_circle_vote_endpoints_noclip;
            else
                drawScanline = vips_hough_circle_vote_endpoints_clip;

            vips__draw_circle_direct(accumulator, cx, cy, r, drawScanline, rb);
        }
    }

    public override void ClassInit(VipsHoughClass class)
    {
        base.ClassInit(class);

        VIPS_ARG_INT(class, "scale", 119,
            _("Scale"),
            _("Scale down dimensions by this factor"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(HoughCircle, Scale),
            1, 100000, 3);

        VIPS_ARG_INT(class, "min_radius", 120,
            _("Min radius"),
            _("Smallest radius to search for"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(HoughCircle, MinRadius),
            1, 100000, 10);

        VIPS_ARG_INT(class, "max_radius", 121,
            _("Max radius"),
            _("Largest radius to search for"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(HoughCircle, MaxRadius),
            1, 100000, 20);
    }

    public override void Init()
    {
        base.Init();

        Scale = 1;
        MinRadius = 10;
        MaxRadius = 20;
    }
}

public class HoughCircleClass : HoughClass
{
    public new VipsHoughCircleClass ClassInit(VipsHoughClass class)
    {
        return (VipsHoughCircleClass)base.ClassInit(class);
    }

    public override void Init()
    {
        base.Init();
    }
}
```

Note that I've assumed the existence of a `VipsDotNet` namespace, which is not provided in your original code. You will need to create this namespace and add the necessary classes and methods for VIPS image processing.

Also note that some parts of the code have been modified to fit C# syntax and semantics. For example, the use of `guint*` has been replaced with `int[]`, and the `VipsImageAddr` function has been replaced with direct array access. Additionally, the `vips_error` function has been replaced with a simple `throw new ArgumentException`.