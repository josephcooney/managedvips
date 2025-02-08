Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipsSimilarityBase : VipsResample
{
    public double Scale { get; set; }
    public double Angle { get; set; }
    public VipsInterpolate Interpolate { get; set; }
    public VipsArrayDouble Background { get; set; }
    public double Odx { get; set; }
    public double Ody { get; set; }
    public double Idx { get; set; }
    public double Idy { get; set; }

    public VipsSimilarityBase()
    {
        Scale = 1;
        Angle = 0;
        Interpolate = null;
        Background = new VipsArrayDouble(1, 0.0);
        Odx = 0;
        Ody = 0;
        Idx = 0;
        Idy = 0;
    }
}

// vips_similarity_base_build
public class VipsSimilarityBaseBuild : VipsObjectBuild
{
    public override int Build(VipsImage inImage, out VipsImage[] output)
    {
        double a = Scale * Math.Cos(Math.PI / 180 * Angle);
        double b = -Scale * Math.Sin(Math.PI / 180 * Angle);
        double c = b;
        double d = a;

        if (base.Build(inImage, output))
            return -1;

        if (VipsAffine.Resample(inImage, output[0], a, b, c, d,
            "interpolate", Interpolate,
            "odx", Odx,
            "ody", Ody,
            "idx", Idx,
            "idy", Idy,
            "background", Background,
            null))
        {
            return -1;
        }

        if (VipsImage.Write(output[0], outImage))
            return -1;

        return 0;
    }
}

// vips_similarity_base_class_init
public class VipsSimilarityBaseClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Add properties here...
    }
}

// vips_similarity_base_init
public class VipsSimilarityBaseInit : VipsObjectInit
{
    public override void Init(VipsSimilarityBase obj)
    {
        base.Init(obj);
    }
}

// vips_similarity
public class VipsSimilarity : VipsResample
{
    // vips_similarity method
    public static int Similarity(VipsImage inImage, out VipsImage[] output, params object[] args)
    {
        return VipsCallSplit("similarity", args, inImage, output);
    }
}

// vips_rotate
public class VipsRotate : VipsSimilarityBase
{
    // vips_rotate method
    public static int Rotate(VipsImage inImage, out VipsImage[] output, double angle, params object[] args)
    {
        return VipsCallSplit("rotate", args, inImage, output, angle);
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available and contains the necessary classes for working with VIPS images. You may need to modify this code to match your specific use case.

Also, I've omitted some details such as property registration and implementation of other methods, as they are not directly related to the conversion from C to C#. If you need help with those parts, please let me know!