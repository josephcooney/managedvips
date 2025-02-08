Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsFalsecolour : VipsConversion
{
    public VipsImage In { get; set; }

    protected override int Build(VipsObject obj)
    {
        VipsConversion conversion = (VipsConversion)obj;
        VipsFalsecolour falsecolour = (VipsFalsecolour)obj;
        VipsImage[] t = new VipsImage[5];

        if (base.Build(obj) != 0)
            return -1;

        // Create a new image from the falsecolour scale
        t[0] = new VipsImage(new byte[][] { vips_falsecolour_pet }, 3, 1);

        // Force input to mono 8-bit
        if (VipsImage.Decode(falsecolour.In, out t[1]) != 0 ||
            VipsExtractBand(t[1], out t[2], 0, null) != 0 ||
            VipsCast(t[2], out t[3], VipsFormat.UChar, null) != 0 ||
            VipsMaplut(t[3], out t[4], t[0], null) != 0 ||
            VipsImage.Write(t[4], conversion.Out) != 0)
        {
            return -1;
        }

        return 0;
    }
}

public class VipsFalsecolourClass : VipsConversionClass
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);

        // Set up the properties for this operation
        VIPS_ARG_IMAGE(klass, "in", 0,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsFalsecolour).GetProperty("In"));
    }
}

public class VipsFalsecolourOperation : VipsOperation
{
    public override int Call(VipsImage inImg, out VipsImage outImg)
    {
        return vips_call_split("falsecolour", new object[] { inImg }, out outImg);
    }
}
```

Note: The `vips_falsecolour_pet` array is assumed to be defined elsewhere in the codebase. Also, this implementation assumes that the necessary VIPS classes and methods have been converted separately.

The `VipsFalsecolour` class represents a falsecolour operation on an image. It has a single property `In`, which is the input image. The `Build` method performs the actual operation of transforming the input image to a 3-band 8-bit image with a false colour map.

The `VipsFalsecolourClass` class is used to set up the properties for this operation, and the `VipsFalsecolourOperation` class represents the operation itself, which can be called using the `Call` method.