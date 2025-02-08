Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsWrap : VipsConversion
{
    public VipsImage In { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    protected override int Build()
    {
        var conversion = (VipsConversion)this;
        var wrap = this as VipsWrap;

        if (base.Build())
            return -1;

        if (!HasArgument("x"))
            wrap.X = wrap.In.Width / 2;
        if (!HasArgument("y"))
            wrap.Y = wrap.In.Height / 2;

        // Clock arithmetic: we want negative x/y to wrap around nicely.
        var x = wrap.X < 0
            ? -wrap.X % wrap.In.Width
            : wrap.In.Width - wrap.X % wrap.In.Width;
        var y = wrap.Y < 0
            ? -wrap.Y % wrap.In.Height
            : wrap.In.Height - wrap.Y % wrap.In.Height;

        if (VipsReplicate(wrap.In, out var t1) ||
            VipsExtractArea(t1, out var t2,
                x, y, wrap.In.Width, wrap.In.Height, null) ||
            VipsImageWrite(t2, conversion.Out))
            return -1;

        conversion.Out.XOffset = x;
        conversion.Out.YOffset = y;

        return 0;
    }
}

public class VipsWrapClass : VipsConversionClass
{
    public static void Register(Type type)
    {
        // Register the VipsWrap class with GType.
    }

    protected override void ClassInit()
    {
        base.ClassInit();

        var gobject_class = (GObjectClass)base.Class;
        var vobject_class = (VipsObjectClass)gobject_class;

        gobject_class.SetProperty += VipsObjectSetProperty;
        gobject_class.GetProperty += VipsObjectGetProperty;

        vobject_class.Nickname = "wrap";
        vobject_class.Description = "Wrap image origin";

        // Register the properties.
        var in_property = new Property("in", typeof(VipsImage), 1, "Input");
        var x_property = new Property("x", typeof(int), 3, "Left edge of input in output");
        var y_property = new Property("y", typeof(int), 4, "Top edge of input in output");

        // Add the properties to the class.
    }
}

public static class VipsWrapMethods
{
    public static int Wrap(VipsImage in_image, out VipsImage out_image, params object[] args)
    {
        var result = VipsCallSplit("wrap", args, in_image, out ref out_image);
        return result;
    }
}
```

Note that this code assumes the existence of other classes and methods (e.g., `VipsConversion`, `VipsImage`, `VipsReplicate`, etc.) that are not shown here. These would need to be implemented separately.

Also note that C# does not have a direct equivalent to the `va_list` type used in the original C code, so I've replaced it with a `params object[] args` parameter in the `Wrap` method.