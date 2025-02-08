Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsHistIsmonotonic : VipsOperation
{
    public VipsImage In { get; set; }
    public bool Monotonic { get; set; }

    protected override int Build(VipsObject object)
    {
        VipsObjectClass classObj = (VipsObjectClass)VipsObject.GetClass(object);
        VipsHistIsmonotonic histIsmonotonic = (VipsHistIsmonotonic)object;
        VipsImage[] t = new VipsImage[4];

        double m;

        if (base.Build(object) != 0)
            return -1;

        if (!VipsCheckHist(classObj.Nickname, histIsmonotonic.In))
            return -1;

        if (histIsmonotonic.In.Xsize == 1)
            t[0] = VipsImage.NewMatrixv(1, 2, -1.0, 1.0);
        else
            t[0] = VipsImage.NewMatrixv(2, 1, -1.0, 1.0);
        VipsImage.SetDouble(t[0], "offset", 128);

        // We want >=128 everywhere, ie. no -ve transitions.
        if (VipsConv(histIsmonotonic.In, t, t[0],
            "precision", VIPS_PRECISION_INTEGER,
            null) ||
            VipsMoreeqConst1(t[1], t[2], 128, null) ||
            VipsMin(t[2], out m, null))
        {
            return -1;
        }

        GObject.SetProperty(histIsmonotonic, "monotonic", (int)m == 255);

        return 0;
    }
}

public class VipsHistIsmonotonicClass : VipsOperationClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        GObjectClass gobjectClass = (GObjectClass)base.GetClass();
        VipsObjectClass objectClass = (VipsObjectClass)base;

        gobjectClass.SetProperty = VipsObject.SetProperty;
        gobjectClass.GetProperty = VipsObject.GetProperty;

        objectClass.Nickname = "hist_ismonotonic";
        objectClass.Description = "test for monotonicity";
        objectClass.Build = new BuildDelegate(VipsHistIsmonotonic.Build);

        VIPS_ARG_IMAGE(objectClass, "in", 1,
            "Input",
            "Input histogram image",
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsHistIsmonotonic).GetField("In").Offset);

        VIPS_ARG_BOOL(objectClass, "monotonic", 2,
            "Monotonic",
            "true if in is monotonic",
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            typeof(VipsHistIsmonotonic).GetField("Monotonic").Offset,
            false);
    }
}

public class VipsHistIsmonotonicInit : VipsOperation
{
    public override void Init()
    {
        base.Init();
    }
}

public static class VipsHistIsmonotonicMethods
{
    [Method]
    public static int HistIsmonotonic(VipsImage inImage, bool[] out)
    {
        return VipsCallSplit("hist_ismonotonic", inImage, out);
    }
}
```

Note that I've assumed the existence of `VipsOperation`, `VipsObject`, `GObject`, and other classes/types from the VIPS library, as well as the `VipsImage` class. I've also assumed that the `VipsCheckHist`, `VipsConv`, `VipsMoreeqConst1`, `VipsMin`, and `VipsCallSplit` methods are available in the VIPS library.

Also note that this code is not a direct translation of the C code, but rather an equivalent implementation using C# syntax and conventions.