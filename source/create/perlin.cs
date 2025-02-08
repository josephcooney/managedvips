Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsPerlin : VipsCreate
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int CellSize { get; set; }
    public bool Uchar { get; set; }
    public uint Seed { get; set; }

    public override void Build()
    {
        base.Build();

        // Be careful if width is a multiple of cell_size.
        CellSize = VipsRoundUp(Width, CellSize) / CellSize;
        int cellsDown = VipsRoundUp(Height, CellSize) / CellSize;

        VipsImageInitFields(Out, Width, Height, 1,
            Uchar ? VipsFormat.UChar : VipsFormat.Float,
            VipsCoding.None, VipsInterpretation.Multiband,
            1.0f, 1.0f);

        if (VipsImagePipelinev(Out, VipsDemandStyle.Any, null) ||
            VipsImageGenerate(Out, PerlinStart, PerlinGen, PerlinStop, this, null))
        {
            return;
        }
    }

    public override void Init()
    {
        CellSize = 256;
        Seed = (uint)(Math.Max(int.MinValue, int.MaxValue) * Random.NextDouble());
    }

    private static float[] VipsPerlinCos = new float[256];
    private static float[] VipsPerlinSin = new float[256];

    public void MakeTables()
    {
        for (int i = 0; i < 256; i++)
        {
            double angle = 2 * Math.PI * i / 256.0;

            VipsPerlinCos[i] = (float)Math.Cos(angle);
            VipsPerlinSin[i] = (float)Math.Sin(angle);
        }
    }

    public static void PerlinClassInit(Type type)
    {
        GObjectClass gobject_class = (GObjectClass)type;
        VipsObjectClass vobject_class = (VipsObjectClass)gobject_class;

        MakeTables();

        gobject_class.SetProperty += VipsObjectSetProperty;
        gobject_class.GetProperty += VipsObjectGetProperty;

        vobject_class.Nickname = "perlin";
        vobject_class.Description = "make a perlin noise image";
        vobject_class.Build = PerlinBuild;

        VipsArgInt("width", 2, "Width", "Image width in pixels",
            VipsArgument.RequiredInput, typeof(VipsPerlin), "Width", 1, int.MaxValue, 1);
        VipsArgInt("height", 3, "Height", "Image height in pixels",
            VipsArgument.RequiredInput, typeof(VipsPerlin), "Height", 1, int.MaxValue, 1);
        VipsArgInt("cell_size", 3, "Cell size", "Size of Perlin cells",
            VipsArgument.OptionalInput, typeof(VipsPerlin), "CellSize", 1, int.MaxValue, 256);
        VipsArgBool("uchar", 4, "Uchar", "Output an unsigned char image",
            VipsArgument.OptionalInput, typeof(VipsPerlin), "Uchar", false);
        VipsArgInt("seed", 5, "Seed", "Random number seed",
            VipsArgument.OptionalInput, typeof(VipsPerlin), "Seed", int.MinValue, int.MaxValue, 0);
    }

    public static void PerlinInit(VipsPerlin perlin)
    {
        perlin.CellSize = 256;
        perlin.Seed = (uint)(Math.Max(int.MinValue, int.MaxValue) * Random.NextDouble());
    }
}

public class Sequence
{
    public VipsPerlin Perlin { get; set; }
    public int CellX { get; set; }
    public int CellY { get; set; }

    private float[] Gx = new float[4];
    private float[] Gy = new float[4];

    public void CreateCells(int cell_x, int cell_y)
    {
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++)
            {
                int ci = x + y * 2;

                uint seed;
                int cx;
                int cy;
                int angle;

                seed = Perlin.Seed;

                cx = cell_x + x;
                cy = cell_y + y;

                // When we calculate the seed for this cell, we wrap
                // around so that our output will tessellate.

                if (cy >= Perlin.CellsDown)
                    cy = 0;
                seed = VipsRandomAdd(seed, cy);

                if (cx >= Perlin.CellsAcross)
                    cx = 0;
                seed = VipsRandomAdd(seed, cx);

                angle = (seed ^ (seed >> 8) ^ (seed >> 16)) & 0xff;

                Gx[ci] = VipsPerlinCos[angle];
                Gy[ci] = VipsPerlinSin[angle];
            }
    }

    public static int PerlinStop(object vseq, object a, object b)
    {
        Sequence seq = (Sequence)vseq;
        VIPS.Free(seq);
        return 0;
    }

    public static object PerlinStart(VipsImage out_image, object a, object b)
    {
        VipsPerlin perlin = (VipsPerlin)b;

        Sequence seq;

        if (!(seq = new Sequence()))
            return null;

        seq.Perlin = perlin;
        seq.CellX = -1;
        seq.CellY = -1;

        return seq;
    }

    public static int PerlinGen(VipsRegion out_region, object vseq, object a, object b, bool[] stop)
    {
        VipsPerlin perlin = (VipsPerlin)a;
        Sequence seq = (Sequence)vseq;

        int x, y;

        for (y = 0; y < out_region.Valid.Height; y++)
        {
            float[] fq = (float[])VIPS_REGION_ADDR(out_region, out_region.Valid.Left, out_region.Valid.Top + y);
            VipsPel[] q = (VipsPel[])fq;

            for (x = 0; x < out_region.Valid.Width; x++)
            {
                int cs = perlin.CellSize;
                int cell_x = (out_region.Valid.Left + x) / cs;
                int cell_y = (out_region.Valid.Top + y) / cs;
                float dx = (x + out_region.Valid.Left - cell_x * cs) / (float)cs;
                float dy = (y + out_region.Valid.Top - cell_y * cs) / (float)cs;
                float sx = Smootherstep(dx);
                float sy = Smootherstep(dy);

                float n0, n1;
                float ix0, ix1;
                float p;

                if (cell_x != seq.CellX || cell_y != seq.CellY)
                {
                    seq.CreateCells(cell_x, cell_y);
                    seq.CellX = cell_x;
                    seq.CellY = cell_y;
                }

                n0 = -dx * seq.Gx[0] + -dy * seq.Gy[0];
                n1 = (1 - dx) * seq.Gx[1] + -dy * seq.Gy[1];
                ix0 = n0 + sx * (n1 - n0);

                n0 = -dx * seq.Gx[2] + (1 - dy) * seq.Gy[2];
                n1 = (1 - dx) * seq.Gx[3] + (1 - dy) * seq.Gy[3];
                ix1 = n0 + sx * (n1 - n0);

                p = ix0 + sy * (ix1 - ix0);

                if (perlin.Uchar)
                    q[x] = 128 * p + 128;
                else
                    fq[x] = p;
            }
        }

        return 0;
    }

    public static float Smootherstep(float x)
    {
        return x * x * x * (x * (x * 6 - 15) + 10);
    }
}
```

Note that I've assumed the existence of certain classes and methods from the VIPS library, as they were not provided in the original code. Also, some types like `VipsPel` are not defined in this code snippet, you should replace them with your actual type.

Also note that the `MakeTables()` method is called only once when the class is initialized, which might be different from the C version where it's called every time a new instance of `VipsPerlin` is created.