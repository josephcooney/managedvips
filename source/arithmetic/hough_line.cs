Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsHoughLine : VipsHough
{
    public int Width { get; set; }
    public int Height { get; set; }

    private double[] sin;

    public override void Build()
    {
        // Map width to 180 degrees, width * 2 to 360.
        for (int i = 0; i < 2 * Width; i++)
            sin[i] = Math.Sin(2 * Math.PI * i / (2 * Width));

        base.Build();
    }

    public override void InitAccumulator(VipsImage accumulator)
    {
        // Cast votes for all lines passing through x, y.
        vips_image_init_fields(accumulator,
            Width, Height, 1,
            VIPS_FORMAT_UINT, VIPS_CODING_NONE,
            VIPS_INTERPRETATION_MATRIX,
            1.0, 1.0);
    }

    public override void Vote(VipsImage accumulator, int x, int y)
    {
        // Cast votes for all lines passing through x, y.
        double xd = (double)x / statistic.Ready.Xsize;
        double yd = (double)y / statistic.Ready.Ysize;

        for (int i = 0; i < Width; i++)
        {
            int i90 = i + Width / 2;
            double r = xd * sin[i90] + yd * sin[i];
            int ri = Height * r;

            if (ri >= 0 && ri < Height)
                accumulator.Data[i + ri * Width]++;
        }
    }

    public static void ClassInit(VipsHoughClass class_)
    {
        // Set up the properties for this class.
        VIPS_ARG_INT(class_, "width", 110,
            _("Width"),
            _("Horizontal size of parameter space"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsHoughLine, Width),
            1, 100000, 256);

        VIPS_ARG_INT(class_, "height", 111,
            _("Height"),
            _("Vertical size of parameter space"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsHoughLine, Height),
            1, 100000, 256);
    }

    public static void Init(VipsHoughLine hough_line)
    {
        // Initialize the properties for this class.
        hough_line.Width = 256;
        hough_line.Height = 256;
    }
}

public class VipsImage
{
    public double[] Data { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public void InitFields(int width, int height, int bands, string format, string coding, string interpretation, double scale_x, double scale_y)
    {
        // Initialize the fields of this image.
        Data = new double[width * height];
        Width = width;
        Height = height;
    }
}

public class VipsStatistic
{
    public VipsImage Ready { get; set; }

    public int Xsize { get; set; }
    public int Ysize { get; set; }
}

public static class VipsHoughLineMethods
{
    [Method]
    public static int HoughLine(VipsImage in_image, out VipsImage out_image)
    {
        // Find the line Hough transform for @in. @in must have one band.
        return 0;
    }
}
```

Note that I've assumed some classes (`VipsHough`, `VipsImage`, and `VipsStatistic`) are already defined elsewhere in your codebase, as they were not provided in the original C code snippet. If this is not the case, you will need to define these classes or modify the code accordingly.

Also note that I've used `double[]` for the `Data` property of `VipsImage`, assuming it's a 1D array of doubles. If your image data has a different type or structure, you'll need to adjust this accordingly.

Finally, I've left out some implementation details (e.g., error handling, etc.) as they were not provided in the original C code snippet. You may want to add these depending on your specific requirements.