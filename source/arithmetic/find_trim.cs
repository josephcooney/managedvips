Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class FindTrim : Operation
{
    public FindTrim()
    {
        Threshold = 10.0;
    }

    [VipsArgument("in", 1, "Input")]
    public Image In { get; set; }

    [VipsArgument("threshold", 2, "Threshold")]
    public double Threshold { get; set; }

    [VipsArgument("background", 3, "Background")]
    public ArrayDouble Background { get; set; }

    [VipsArgument("line_art", 4, "Line art mode")]
    public bool LineArt { get; set; }

    [VipsArgument("left", 5, "Left")]
    public int Left { get; set; }

    [VipsArgument("top", 11, "Top")]
    public int Top { get; set; }

    [VipsArgument("width", 12, "Width")]
    public int Width { get; set; }

    [VipsArgument("height", 13, "Height")]
    public int Height { get; set; }
}

public class FindTrimClass : OperationClass
{
    public static void Register(Type type)
    {
        VipsOperation.Register(type);
    }

    protected override void Initialize()
    {
        base.Initialize();

        AddProperty("in", typeof(Image), 1, "Input");
        AddProperty("threshold", typeof(double), 2, "Threshold");
        AddProperty("background", typeof(ArrayDouble), 3, "Background");
        AddProperty("line_art", typeof(bool), 4, "Line art mode");
        AddProperty("left", typeof(int), 5, "Left");
        AddProperty("top", typeof(int), 11, "Top");
        AddProperty("width", typeof(int), 12, "Width");
        AddProperty("height", typeof(int), 13, "Height");
    }
}

public class FindTrimOperation : Operation
{
    public static int Build(VipsObject obj)
    {
        var find_trim = (FindTrim)obj;
        var t = new Image[20];

        var inImage = find_trim.In;
        var background = find_trim.Background;

        if (!background.HasValue)
            background = ArrayDouble.New(1, VipsInterpretation.MaxAlpha(inImage.Type));

        // Flatten out alpha, if any
        if (inImage.HasAlpha)
        {
            if (VipsFlatten(inImage, t[0], "background", background.Value, null) != 0)
                return -1;
            inImage = t[0];
        }

        var backgroundArray = background.Value.GetValues();
        var negBg = new double[backgroundArray.Length];
        var ones = new double[backgroundArray.Length];

        for (int i = 0; i < backgroundArray.Length; i++)
        {
            negBg[i] = -1 * backgroundArray[i];
            ones[i] = 1.0;
        }

        // Filter out noise, unless we're in line_art mode
        if (!find_trim.LineArt)
        {
            if (VipsMedian(inImage, t[1], 3, null) != 0)
                return -1;
            inImage = t[1];
        }

        // Find difference from bg, abs, threshold
        if (VipsLinear(inImage, t[2], ones, negBg, backgroundArray.Length, null) != 0 ||
            VipsAbs(t[2], t[3], null) != 0 ||
            VipsMoreConst1(t[3], t[4], find_trim.Threshold, null) != 0 ||
            VipsBandor(t[4], t[5], null) != 0)
            return -1;
        inImage = t[5];

        // t[6] == column sums, t[7] == row sums
        if (VipsProject(inImage, t[6], t[7], null) != 0)
            return -1;

        // t[8] == search column sums in from left
        if (VipsProfile(t[6], t[8], t[9], null) != 0 ||
            VipsAvg(t[9], out double left, null) != 0)
            return -1;
        if (VipsFlip(t[6], t[10], VipsDirection.Horizontal, null) != 0 ||
            VipsProfile(t[10], t[11], t[12], null) != 0 ||
            VipsAvg(t[12], out double right, null) != 0)
            return -1;

        // t[8] == search column sums in from left
        if (VipsProfile(t[7], t[13], t[14], null) != 0 ||
            VipsAvg(t[13], out double top, null) != 0)
            return -1;
        if (VipsFlip(t[7], t[15], VipsDirection.Vertical, null) != 0 ||
            VipsProfile(t[15], t[16], t[17], null) != 0 ||
            VipsAvg(t[16], out double bottom, null) != 0)
            return -1;

        find_trim.Left = (int)left;
        find_trim.Top = (int)top;
        find_trim.Width = (int)VipsMax(0, (t[6].Xsize - right) - left);
        find_trim.Height = (int)VipsMax(0, (t[7].Ysize - bottom) - top);

        return 0;
    }
}

public class FindTrimMethod : Method
{
    public static int Call(VipsObject obj, params object[] args)
    {
        var inImage = (Image)obj;

        var left = new int?(0);
        var top = new int?(0);
        var width = new int?(0);
        var height = new int?(0);

        return FindTrimOperation.Build(obj);
    }
}
```

Note that this code assumes the existence of a `VipsDotNet` library, which is not included in the standard .NET framework. You will need to install this library separately.

Also note that some methods and classes (e.g., `VipsImage`, `ArrayDouble`, `VipsInterpretation`) are assumed to be part of the `VipsDotNet` library and may require additional setup or configuration to work correctly.

This code is a direct translation of the provided C code, with minimal modifications to accommodate the differences between C and C#. Some minor changes were made to improve readability and consistency with standard .NET coding practices.