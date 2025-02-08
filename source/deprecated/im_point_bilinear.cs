```csharp
// im_point_bilinear.cs
// Converted from: im_point_bilinear.c

using System;

public class VipsInterpolate
{
    public static double Bilinear(double x1, double y1, double v11, double v12, double v21, double v22)
    {
        return (v11 * (x1 * y1) + v12 * ((1 - x1) * y1) +
                v21 * (x1 * (1 - y1)) + v22 * ((1 - x1) * (1 - y1)));
    }
}

public class Image
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }

    public double this[int band, int x, int y]
    {
        get
        {
            // implement im_extract_band and im_affinei here
            return 0;
        }
    }
}

public class Program
{
    public static int ImPoint(Image im, VipsInterpolate interpolate,
        double x, double y, int band, out double out)
    {
        if (band >= im.Bands ||
            x < 0.0 || y < 0.0 ||
            x > im.Xsize || y > im.Ysize)
        {
            Console.WriteLine("coords outside image");
            return -1;
        }

        // implement im_open, im_open_local_array and im_close here
        Image mem = new Image();
        Image[] t = new Image[2];

        if (im.ExtractBand(band, out t[0]) ||
            VipsInterpolate.Affinei(t[0], t[1],
                interpolate,
                1, 0, 0, 1,
                Math.Floor(x) - x, Math.Floor(y) - y,
                Math.Floor(x), Math.Floor(y), 1, 1) ||
            VipsInterpolate.Avg(t[1], out out))
        {
            return -1;
        }

        return 0;
    }

    public static int ImPointBilinear(Image im, double x, double y, int band, out double out)
    {
        return ImPoint(im, new VipsInterpolate(), x, y, band, out);
    }
}
```