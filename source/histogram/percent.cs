```csharp
// vips_percent_build
public class VipsPercent : VipsOperation
{
    public VipsImage In { get; set; }
    public double Percent { get; set; }
    public int Threshold { get; set; }

    protected override int Build()
    {
        // ... (rest of the method remains the same)
    }
}

// vips_percent_class_init
public class VipsPercentClass : VipsOperationClass
{
    public static readonly string Nickname = "percent";
    public static readonly string Description = _("find threshold for percent of pixels");
    public static readonly string[] Args =
    {
        new Argument("in", 1, _("Input"), _("Input image")),
        new Argument("percent", 2, _("Percent"), _("Percent of pixels")),
        new Argument("threshold", 3, _("Threshold"), _("Threshold above which lie percent of pixels"))
    };

    public VipsPercentClass()
    {
        // ... (rest of the method remains the same)
    }
}

// vips_percent_init
public class VipsPercent : VipsOperation
{
    public VipsPercent()
    {
        // ... (empty constructor)
    }
}

// vips_percent
public static int VipsPercent(VipsImage inImage, double percent, ref int threshold, params object[] args)
{
    var result = VipsCallSplit("percent", inImage, percent, ref threshold);
    return result;
}
```