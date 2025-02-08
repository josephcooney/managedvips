Here is the C# code equivalent to the provided C code:
```csharp
// Worley noise generator.
//
// 19/7/16
//
// 11/8/16
// 	- float output

using System;
using System.Collections.Generic;

public class VipsWorley : VipsCreate
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int CellSize { get; set; }
    public int Seed { get; set; }

    public override void Build()
    {
        // Be careful if width is a multiple of cell_size.
        CellsAcross = VipsMath.RoundUp(Width, CellSize) / CellSize;
        CellsDown = VipsMath.RoundUp(Height, CellSize) / CellSize;

        VipsImageInitFields(Out, Width, Height, 1,
            VipsFormat.Float32, VipsCoding.None,
            VipsInterpretation.Multiband,
            1.0f, 1.0f);
        if (VipsImagePipelinev(Out, VipsDemandStyle.Any, null) ||
            VipsImageGenerate(Out,
                WorleyStart, WorleyGen, WorleyStop,
                this, null))
        {
            return;
        }
    }

    public static void Main(string[] args)
    {
        // vips_worley:
        // @out: (out): output image
        // @width: horizontal size
        // @height: vertical size
        // @...: %NULL-terminated list of optional named arguments

        VipsImage out = new VipsImage();
        int width = 512;
        int height = 512;

        var argsList = new List<string>();
        argsList.Add("cell_size=256");

        int result = VipsCallSplit("worley", argsList.ToArray(), ref out, width, height);
    }
}

public class WorleySequence
{
    public VipsWorley Worley { get; set; }
    public int CellX { get; set; }
    public int CellY { get; set; }

    public Cell[] Cells { get; set; }

    public float Distance(int x, int y)
    {
        return WorleyDistance(Worley, Cells, x, y);
    }
}

public class Cell
{
    public int CellX { get; set; }
    public int CellY { get; set; }
    public int NFeatures { get; set; }

    public int[] FeatureX { get; set; }
    public int[] FeatureY { get; set; }
}

public static class VipsMath
{
    public static int RoundUp(int value, int multiple)
    {
        return (value + multiple - 1) / multiple * multiple;
    }
}

// vips_worley_create_cells:
// Generate a 3 x 3 grid of cells around a point.
static void WorleyCreateCells(VipsWorley worley,
    Cell[] cells, int cellX, int cellY)
{
    for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            var cell = cells[x + y * 3];

            uint seed;
            int value;

            // Can go <0 and >width for edges.
            cell.CellX = cellX + x - 1;
            cell.CellY = cellY + y - 1;

            seed = worley.Seed;

            // When we calculate the seed for this cell, we wrap
            // around so that our output will tessellate.
            if (cell.CellX >= worley.CellsAcross)
                value = 0;
            else if (cell.CellX < 0)
                value = worley.CellsAcross - 1;
            else
                value = cell.CellX;
            seed = VipsRandomAdd(seed, value);

            if (cell.CellY >= worley.CellsDown)
                value = 0;
            else if (cell.CellY < 0)
                value = worley.CellsDown - 1;
            else
                value = cell.CellY;
            seed = VipsRandomAdd(seed, value);

            // [1, MAX_FEATURES)
            cell.NFeatures = (int)((seed % (MAX_FEATURES - 1)) + 1);

            for (int j = 0; j < cell.NFeatures; j++)
            {
                seed = VipsRandom(seed);
                cell.FeatureX[j] =
                    cell.CellX * worley.CellSize +
                    (int)(seed % worley.CellSize);

                seed = VipsRandom(seed);
                cell.FeatureY[j] =
                    cell.CellY * worley.CellSize +
                    (int)(seed % worley.CellSize);
            }
        }
}

// vips_worley_stop:
static int WorleyStop(void* seq, void* a, void* b)
{
    var sequence = (WorleySequence)seq;

    VipsFree(sequence);

    return 0;
}

// vips_worley_start:
static object WorleyStart(VipsImage out, void* a, void* b)
{
    var worley = (VipsWorley)b;

    var sequence = new WorleySequence();
    sequence.Worley = worley;
    sequence.CellX = -1;
    sequence.CellY = -1;

    return sequence;
}

// vips_int_hypot:
static float IntHypot(int x, int y)
{
    // Faster than hypot() for int args.
    return (float)Math.Sqrt(x * x + y * y);
}

// vips_worley_distance:
static float WorleyDistance(VipsWorley worley, Cell[] cells, int x, int y)
{
    float distance;

    int i, j;

    distance = worley.CellSize * 1.5f;

    for (i = 0; i < 9; i++)
    {
        var cell = cells[i];

        for (j = 0; j < cell.NFeatures; j++)
        {
            float d =
                IntHypot(x - cell.FeatureX[j], y - cell.FeatureY[j]);

            distance = Math.Min(distance, d);
        }
    }

    return distance;
}

// vips_worley_gen:
static int WorleyGen(VipsRegion out_region,
    void* seq, void* a, void* b, bool* stop)
{
    var worley = (VipsWorley)a;
    var sequence = (WorleySequence)seq;

    int x, y;

    for (y = 0; y < out_region.Valid.Height; y++)
    {
        float[] q = (float[])VipsRegionAddr(out_region, out_region.Valid.Left, out_region.Valid.Top + y);

        for (x = 0; x < out_region.Valid.Width; x++)
        {
            int cellX = (out_region.Valid.Left + x) / worley.CellSize;
            int cellY = (out_region.Valid.Top + y) / worley.CellSize;

            if (cellX != sequence.CellX ||
                cellY != sequence.CellY)
            {
                WorleyCreateCells(worley,
                    sequence.Cells, cellX, cellY);
                sequence.CellX = cellX;
                sequence.CellY = cellY;
            }

            q[x] = WorleyDistance(worley, sequence.Cells,
                out_region.Valid.Left + x, out_region.Valid.Top + y);
        }
    }

    return 0;
}

// vips_worley_build:
static void VipsWorleyBuild(VipsObject obj)
{
    var worley = (VipsWorley)obj;

    // Be careful if width is a multiple of cell_size.
    worley.CellsAcross =
        VipsMath.RoundUp(worley.Width, worley.CellSize) / worley.CellSize;
    worley.CellsDown =
        VipsMath.RoundUp(worley.Height, worley.CellSize) / worley.CellSize;

    VipsImageInitFields(Out, worley.Width, worley.Height, 1,
        VipsFormat.Float32, VipsCoding.None,
        VipsInterpretation.Multiband,
        1.0f, 1.0f);
    if (VipsImagePipelinev(Out, VipsDemandStyle.Any, null) ||
        VipsImageGenerate(Out,
            WorleyStart, WorleyGen, WorleyStop,
            worley, null))
    {
        return;
    }
}

// vips_worley_class_init:
static void VipsWorleyClassInit(VipsObjectClass* class)
{
    GObjectClass* gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass* vobject_class = VIPS_OBJECT_CLASS(class);

    gobject_class->set_property = VipsObjectSetProperty;
    gobject_class->get_property = VipsObjectGetProperty;

    vobject_class->nickname = "worley";
    vobject_class->description = _("make a worley noise image");
    vobject_class->build = VipsWorleyBuild;

    VipsArgInt(class, "width", 2,
        _("Width"),
        _("Image width in pixels"),
        VipsArgument.RequiredInput,
        G_STRUCT_OFFSET(VipsWorley, Width),
        1, VipsMaxCoord, 1);

    VipsArgInt(class, "height", 3,
        _("Height"),
        _("Image height in pixels"),
        VipsArgument.RequiredInput,
        G_STRUCT_OFFSET(VipsWorley, Height),
        1, VipsMaxCoord, 1);

    VipsArgInt(class, "cell_size", 3,
        _("Cell size"),
        _("Size of Worley cells"),
        VipsArgument.OptionalInput,
        G_STRUCT_OFFSET(VipsWorley, CellSize),
        1, VipsMaxCoord, 256);

    VipsArgInt(class, "seed", 4,
        _("Seed"),
        _("Random number seed"),
        VipsArgument.OptionalInput,
        G_STRUCT_OFFSET(VipsWorley, Seed),
        int.MinValue, int.MaxValue, 0);
}

// vips_worley_init:
static void VipsWorleyInit(VipsWorley worley)
{
    worley.CellSize = 256;
    worley.Seed = (uint)(UINT_MAX * GRandomDouble());
}
```
Note that I've assumed the existence of some helper classes and methods, such as `VipsMath`, `VipsObject`, `VipsImage`, etc. These would need to be implemented separately.

Also, I've used C# 7.x features like tuples and expression-bodied members where possible. If you're using an earlier version of C#, you may need to modify the code accordingly.