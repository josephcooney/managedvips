Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsFillNearest : VipsMorphology
{
    public VipsImage Out { get; private set; }
    public VipsImage Distance { get; private set; }

    private int width;
    private int height;
    private List<Seed> seeds = new List<Seed>();

    public override void Finalize()
    {
        foreach (var seed in seeds)
            seed.OctantMask = 0;

        seeds.Clear();
        base.Finalize();
    }

    public class Seed
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int R { get; set; }
        public int OctantMask { get; set; }
    }

    private void FillNearestPixel(Circle circle, int x, int y, int octant)
    {
        if ((circle.Seed.OctantMask & (1 << octant)) == 0)
            return;

        var p = (float[])VipsImage.Addr(Distance, x, y);
        var dx = x - circle.Seed.X;
        var dy = y - circle.Seed.Y;
        var radius = Math.Sqrt(dx * dx + dy * dy);

        if (p[0] == 0 || p[0] > radius)
        {
            VipsMorphology morphology = this as VipsMorphology;
            var inImage = morphology.In;
            int ps = VipsImage.SizeOfPel(inImage);
            var pi = VipsImage.Addr(inImage, circle.Seed.X, circle.Seed.Y);
            var qi = VipsImage.Addr(Out, x, y);

            for (int i = 0; i < ps; i++)
                qi[i] = pi[i];

            p[0] = radius;
            circle.OctantMask |= 1 << octant;
        }
    }

    private void FillNearestPixelClip(Circle circle, int x, int y, int octant)
    {
        if ((circle.Seed.OctantMask & (1 << octant)) == 0)
            return;

        if (x >= 0 && x < width && y >= 0 && y < height)
            FillNearestPixel(circle, x, y, octant);
    }

    private void Scanline(VipsImage image, int y, int x1, int x2, int quadrant, Circle circle)
    {
        FillNearestPixel(circle, x1, y, quadrant);
        FillNearestPixel(circle, x2, y, quadrant + 4);

        if (quadrant == 0)
        {
            FillNearestPixel(circle, x1, y - 1, quadrant);
            FillNearestPixel(circle, x2, y - 1, quadrant + 4);
        }
        else if (quadrant == 1)
        {
            FillNearestPixel(circle, x1, y + 1, quadrant);
            FillNearestPixel(circle, x2, y + 1, quadrant + 4);
        }
        else
        {
            FillNearestPixel(circle, x1 + 1, y, quadrant);
            FillNearestPixel(circle, x2 - 1, y, quadrant + 4);
        }
    }

    private void GrowSeed(Seed seed)
    {
        var circle = new Circle { Seed = seed };

        if (seed.X - seed.R >= 0 && seed.X + seed.R < width && seed.Y - seed.R >= 0 && seed.Y + seed.R < height)
            circle.NearestPixel = FillNearestPixel;
        else
            circle.NearestPixel = FillNearestPixelClip;

        Vips__DrawCircleDirect(Distance, seed.X, seed.Y, seed.R, Scanline, circle);

        // Update the action_mask for this seed. Next time, we can skip any octants where we failed to act this time.
        seed.OctantMask = circle.OctantMask;

        seed.R += 1;
    }

    public override int Build(VipsObject object)
    {
        var morphology = (VipsMorphology)object;
        var inImage = morphology.In;
        width = inImage.Xsize;
        height = inImage.Ysize;

        seeds.Clear();

        for (int y = 0; y < height; y++)
        {
            var p = VipsImage.Addr(inImage, 0, y);

            for (int x = 0; x < width; x++)
            {
                for (int i = 0; i < VipsImage.SizeOfPel(inImage); i++)
                    if (p[i] != 0)
                        break;

                if (i != VipsImage.SizeOfPel(inImage))
                {
                    var seed = new Seed { X = x, Y = y, R = 1, OctantMask = 255 };
                    seeds.Add(seed);
                }

                p += VipsImage.SizeOfPel(inImage);
            }
        }

        // Create the output and distance images in memory.
        Out = VipsImage.NewMemory(width, height);
        Distance = VipsImage.NewMemory(width, height);

        if (VipsBlack(Out) || VipsCast(Out, Distance, VIPS_FORMAT_FLOAT) || VipsImage.Write(Distance))
            return -1;

        while (seeds.Count > 0)
        {
            // Grow all seeds by one pixel.
            foreach (var seed in seeds.ToList())
                GrowSeed(seed);

            // Remove dead seeds.
            for (int i = 0; i < seeds.Count; i++)
            {
                var seed = seeds[i];

                if (seed.OctantMask == 0)
                    seeds.RemoveAt(i);
                else
                    i++;
            }
        }

        return 0;
    }

    public class Circle
    {
        public VipsFillNearest Nearest { get; set; }
        public Seed Seed { get; set; }
        public int OctantMask { get; set; }
        public Action<int, int, int> NearestPixel { get; set; }
    }

    // vips_fill_nearest method
    public static int VipsFillNearest(VipsImage inImage, out VipsImage[] outImages)
    {
        var result = VipsCallSplit("fill_nearest", inImage, outImages);
        return result;
    }
}
```

Note that this code assumes the existence of other classes and methods (e.g., `VipsMorphology`, `VipsObject`, `VipsImage`) which are not provided here. Also, some parts of the original C code were modified or omitted to make it more suitable for C#.