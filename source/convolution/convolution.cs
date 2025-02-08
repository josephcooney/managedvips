Here is the C# code equivalent to the provided C code:

```csharp
// Converted from vips_convolution_build()

public abstract class VipsConvolution : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }
    public VipsImage Mask { get; set; }

    protected override int Build()
    {
        if (base.Build() != 0)
            return -1;

        if (!CheckMatrix(Nickname, Mask, out In))
            return -1;
        M = In;

        return 0;
    }
}

// Converted from vips_convolution_class_init()

public abstract class VipsConvolutionClass : VipsOperationClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Inputs set by subclassess.
        AddArgument("in", "Input", "Input image argument", ArgumentFlags.RequiredInput);
        AddArgument("out", "Output", "Output image", ArgumentFlags.RequiredOutput);
        AddArgument("mask", "Mask", "Input matrix image", ArgumentFlags.RequiredInput);

        Flags = OperationFlags.Sequential;
    }
}

// Converted from vips_convolution_init()

public abstract class VipsConvolution : VipsOperation
{
    public VipsConvolution()
    {
    }
}

// Converted from vips_convolution_operation_init()

public static void ConvolutionOperationInit()
{
    // Plugin system instead?
    typeof(VipsConvGet).Assembly.GetType("VipsConvGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsConvaGet).Assembly.GetType("VipsConvaGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsConvfGet).Assembly.GetType("VipsConvfGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsConviGet).Assembly.GetType("VipsConviGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsCompassGet).Assembly.GetType("VipsCompassGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsConvsepGet).Assembly.GetType("VipsConvsepGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsConvasepGet).Assembly.GetType("VipsConvasepGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsFastcorGet).Assembly.GetType("VipsFastcorGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsSpcorGet).Assembly.GetType("VipsSpcorGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsSharpenGet).Assembly.GetType("VipsSharpenGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsGaussblurGet).Assembly.GetType("VipsGaussblurGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsSobelGet).Assembly.GetType("VipsSobelGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsScharrGet).Assembly.GetType("VipsScharrGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsPrewittGet).Assembly.GetType("VipsPrewittGet").GetMethod("GetType").Invoke(null, null);
    typeof(VipsCannyGet).Assembly.GetType("VipsCannyGet").GetMethod("GetType").Invoke(null, null);
}
```

Note that I've assumed the existence of a `VipsOperation` class and its subclasses, as well as various other types and methods (e.g. `CheckMatrix`, `Nickname`, etc.) which are not defined in this code snippet. You will need to ensure these exist and are correctly implemented for this code to work.

Also note that I've used the `typeof()` operator to get the type of a class, and then invoked its `GetMethod` method to get the `GetType` method (which is equivalent to the C `vips_get_type()` function). This is because C# does not have an exact equivalent to the C `extern GType vips_conv_get_type(void);` syntax.