```csharp
// im_gaussnoise

using System;

public class VipsGaussnoise : VipsCreate
{
    public int Width { get; set; }
    public int Height { get; set; }
    public double Mean { get; set; }
    public double Sigma { get; set; }

    // Per-image seed. Each pixel is seeded by this plus the (x, y) coordinate.
    public uint Seed { get; set; }

    public VipsGaussnoise()
    {
        Width = 1;
        Height = 1;
        Mean = 128.0;
        Sigma = 30.0;
        Seed = uint.MaxValue * new Random().NextDouble();
    }
}

public class VipsGaussnoiseClass : VipsCreateClass
{
    public static void RegisterType(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsCreate);
    }
}

// vips_gaussnoise_gen

public int VipsGaussnoiseGen(VipsRegion outRegion, object seq, object a, object b, bool stop)
{
    VipsGaussnoise gaussNoise = (VipsGaussnoise)a;
    int sz = VIPS_REGION_N_ELEMENTS(outRegion);

    for (int y = 0; y < outRegion.Valid.Height; y++)
    {
        float[] q = (float[])VIPS_REGION_ADDR(outRegion, outRegion.Valid.Left, y + outRegion.Valid.Top);

        for (int x = 0; x < sz; x++)
        {
            uint seed;
            double sum;
            int i;

            seed = gaussNoise.Seed;
            seed = Vips__RandomAdd(seed, outRegion.Valid.Left + x);
            seed = Vips__RandomAdd(seed, outRegion.Valid.Top + y);

            sum = 0.0;
            for (i = 0; i < 12; i++)
            {
                seed = Vips__Random(seed);
                sum += (double)seed / uint.MaxValue;
            }

            q[x] = (sum - 6.0) * gaussNoise.Sigma + gaussNoise.Mean;
        }
    }

    return 0;
}

// vips_gaussnoise_build

public int VipsGaussnoiseBuild(VipsObject object)
{
    VipsCreate create = VIPS_CREATE(object);
    VipsGaussnoise gaussNoise = (VipsGaussnoise)object;

    if (base.Build(object) != 0)
        return -1;

    VipsImageInitFields(create.Out, gaussNoise.Width, gaussNoise.Height, 1,
        VipsFormat.Float, VipsCoding.None,
        VipsInterpretation.Multiband, 1.0, 1.0);

    if (VipsImagePipelinev(create.Out, VipsDemandStyle.Any, null) ||
        VipsImageGenerate(create.Out, null, VipsGaussnoiseGen, null, gaussNoise, null))
        return -1;

    return 0;
}

// vips_gaussnoise_class_init

public static void VipsGaussnoiseClassInit(VipsGaussnoiseClass class)
{
    // ...
}

// vips_gaussnoise_init

public void VipsGaussnoiseInit(VipsGaussnoise gaussNoise)
{
    gaussNoise.Mean = 128.0;
    gaussNoise.Sigma = 30.0;
    gaussNoise.Seed = uint.MaxValue * new Random().NextDouble();
}

// vips_gaussnoise

public int VipsGaussnoise(out VipsImage out, int width, int height, params object[] args)
{
    // ...
}
```