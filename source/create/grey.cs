```csharp
// vips_grey_point
public static float VipsGreyPoint(VipsPoint point, int x)
{
    return (double)x / (point.Width - 1);
}

// vips_grey_class_init
public class VipsGreyClass : VipsObjectClass
{
    public override string Nickname { get; set; } = "grey";
    public override string Description { get; set; } = _("make a grey ramp image");

    public VipsPointClass PointClass { get; }

    public VipsGreyClass()
    {
        PointClass = new VipsPointClass();
    }
}

// vips_grey_init
public class VipsGrey : VipsObject, VipsPoint
{
    public override void Init()
    {
    }
}

// vips_grey
public static int VipsGrey(VipsImage[] out, int width, int height, params object[] args)
{
    // Note: C# does not support variable argument lists like C.
    // We assume the arguments are passed as an array.

    return VipsCallSplit("grey", out, width, height, args);
}
```