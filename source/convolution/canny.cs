Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsCanny : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }

    public double Sigma { get; set; }
    public VipsPrecision Precision { get; set; }

    public override int Build(VipsObject obj)
    {
        VipsCanny canny = (VipsCanny)obj;
        VipsImage[] t = new VipsImage[6];

        VipsImage inImg;

        if (base.Build(obj))
            return -1;

        inImg = canny.In;

        if (VipsGaussblur(inImg, t[0], canny.Sigma,
            "precision", canny.Precision,
            null) != 0)
            return -1;
        inImg = t[0];

        if (VipsCannyGradient(inImg, out t[1], out t[2]) != 0)
            return -1;

        // Form (G, theta).
        canny.Args[0] = t[1];
        canny.Args[1] = t[2];
        canny.Args[2] = null;
        if (VipsCannyPolar(canny.Args, out t[3]) != 0)
            return -1;
        inImg = t[3];

        // Expand by two pixels all around, then thin in the direction of the
        // gradient.
        if (VipsEmbed(inImg, out t[4], 1, 1, inImg.Xsize + 2, inImg.Ysize + 2,
            "extend", VIPS_EXTEND_COPY,
            null) != 0)
            return -1;

        if (VipsCannyThin(t[4], out t[5]) != 0)
            return -1;
        inImg = t[5];

        obj.SetProperty("out", new VipsImage());

        if (VipsImageWrite(inImg, canny.Out) != 0)
            return -1;

        return 0;
    }

    public static int VipsCanny(VipsImage inImg, out VipsImage outImg, params object[] args)
    {
        var result = VipsCallSplit("canny", args, inImg, out outImg);
        return result;
    }
}

public class VipsOperation
{
    // ...
}

public enum VipsPrecision { ... }

public class VipsImage
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }
    public VipsPrecision Precision { get; set; }
    public byte[] Data { get; set; }
}

public static class VipsGaussblur
{
    public static int Gaussblur(VipsImage inImg, out VipsImage outImg, double sigma,
        string precision, VipsPrecision precisionValue,
        params object[] args)
    {
        // ...
    }
}

public static class VipsCannyGradient
{
    public static int CannyGradient(VipsImage inImg, out VipsImage Gx, out VipsImage Gy)
    {
        // ...
    }
}

public static class VipsCannyPolar
{
    public static int CannyPolar(VipsImage[] args, out VipsImage outImg)
    {
        // ...
    }
}

public static class VipsEmbed
{
    public static int Embed(VipsImage inImg, out VipsImage outImg, int xsize, int ysize,
        int newxsize, int newysize,
        string extend, VIPS_EXTEND extendValue,
        params object[] args)
    {
        // ...
    }
}

public static class VipsCannyThin
{
    public static int CannyThin(VipsImage inImg, out VipsImage outImg)
    {
        // ...
    }
}

public static class VipsImageWrite
{
    public static int ImageWrite(VipsImage inImg, VipsImage outImg)
    {
        // ...
    }
}

public static class VipsCallSplit
{
    public static int CallSplit(string name, params object[] args,
        VipsImage inImg, out VipsImage outImg)
    {
        // ...
    }
}
```

Note that I've omitted the implementation of some methods and classes for brevity. You'll need to fill in the missing code based on your specific requirements.

Also, keep in mind that this is a direct translation from C to C#, so you may want to refactor the code to better fit the idioms and conventions of the C# language.