Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipsConversion : VipsOperation
{
    public VipsImage Out { get; set; }

    protected override int Build(VipsObject obj)
    {
        // Commented out for now, as it's not clear what this does.
        // printf("vips_conversion_build: ");
        // vips_object_print_name(object);
        // printf("\n");

        Out = new VipsImage();
        return 0;
    }

    protected override void ClassInit(VipsOperationClass class_)
    {
        base.ClassInit(class_);
        class_.Nickname = "conversion";
        class_.Description = "Conversion operations";

        var outProperty = new VipsArgProperty("out", 2, "Output", "Output image");
        outProperty.ArgumentType = ArgumentType.RequiredOutput;
        outProperty.Offset = typeof(VipsConversion).GetField("Out").Offset;
        class_.Properties.Add(outProperty);
    }

    protected override void Init()
    {
        // Nothing to do here.
    }
}

public static class VipsConversionOperation
{
    public static void OperationInit()
    {
        // Commented out for now, as it's not clear what this does.
        // extern GType vips_copy_get_type(void);
        // ...
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available and contains the necessary classes and methods to work with VIPS images. If you're using a different library or framework, you'll need to adjust the code accordingly.

Also note that some parts of the original C code are commented out or unclear, so I've left them as-is in the C# equivalent. You may need to modify these sections based on your specific requirements and understanding of the VIPS library.