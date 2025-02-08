Here is the converted C# code:

```csharp
using System;
using VipsDotNet;

public class VipsColourspace : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }
    public VipsInterpretation Space { get; set; }
    public VipsInterpretation SourceSpace { get; set; }

    protected override int Build()
    {
        // Verify that all input args have been set.
        if (base.Build())
            return -1;

        var x = In;

        // Unpack radiance-coded images. We can't use interpretation for this,
        // since rad images can be scRGB or XYZ.
        if (x.Coding == VipsCoding.RAD)
        {
            if (!VipsRad2Float(x, out var t))
                return -1;
            x = t;
        }

        if (ObjectArgumentIsSet("source_space"))
            Space = SourceSpace;
        else
            Space = VipsImage.GuessInterpretation(x);

        // Treat RGB as sRGB. If you want some other treatment,
        // you'll need to use the icc funcs.
        if (Space == VipsInterpretation.RGB)
            Space = VipsInterpretation.sRGB;

        // No conversion necessary.
        if (Space == this.Space)
        {
            Out = new VipsImage();
            return ImageWrite(In, Out);
        }

        for (int i = 0; i < VipsNumber(VipsColourRoutes); i++)
        {
            if (VipsColourRoutes[i].From == Space && VipsColourRoutes[i].To == this.Space)
                break;
        }
        if (i == VipsNumber(VipsColourRoutes))
        {
            throw new Exception($"No known route from '{VipsEnumNick(VipsType.Interpretation, Space)}' to '{VipsEnumNick(VipsType.Interpretation, this.Space)}'");
        }

        for (int j = 0; VipsColourRoutes[i].Route[j] != null; j++)
        {
            if (!VipsColourRoutes[i].Route[j](x, out var pipe))
                return -1;
            x = pipe;
        }

        Out = new VipsImage();
        if (!ImageWrite(x, Out))
            return -1;

        return 0;
    }
}

public class VipsColourspaceClass : VipsOperationClass
{
    public static readonly string Nickname = "colourspace";
    public static readonly string Description = "convert to a new colorspace";

    protected override void ClassInit(VipsObjectClass class_)
    {
        base.ClassInit(class_);

        VipsArgImage class_ImgIn = new VipsArgImage("in", 1, "Input", "Input image", VipsArgument.RequiredInput);
        AddArgument(class_, class_ImgIn);

        VipsArgImage class_ImgOut = new VipsArgImage("out", 2, "Output", "Output image", VipsArgument.RequiredOutput);
        AddArgument(class_, class_ImgOut);

        VipsArgEnum class_Space = new VipsArgEnum("space", 6, "Space", "Destination color space", VipsArgument.RequiredInput, typeof(VipsInterpretation), VipsInterpretation.sRGB);
        AddArgument(class_, class_Space);

        VipsArgEnum class_SourceSpace = new VipsArgEnum("source_space", 6, "Source space", "Source color space", VipsArgument.OptionalInput, typeof(VipsInterpretation), VipsInterpretation.sRGB);
        AddArgument(class_, class_SourceSpace);
    }
}

public static class VipsColourspaceMethods
{
    public static int Colourspace(VipsImage inImg, out VipsImage outImg, VipsInterpretation space)
    {
        var colourspace = new VipsColourspace { In = inImg, Out = null, Space = space };
        return colourspace.Build();
    }
}

public class VipsColourRoutes
{
    public static readonly VipsColourRoute[] Routes = new[]
    {
        // ... (rest of the routes)
    };

    public static int Number => Routes.Length;
}

public struct VipsColourRoute
{
    public VipsInterpretation From { get; set; }
    public VipsInterpretation To { get; set; }
    public VipsColourTransformFn[] Route { get; set; }
}
```

Note that I've assumed the existence of a `VipsDotNet` namespace and classes, which are not shown here. You'll need to create these yourself or use an existing implementation.

Also, I've omitted some parts of the code for brevity, such as the implementation of `VipsColourTransformFn` and the `VipsNumber` method. You'll need to add those yourself based on your specific requirements.