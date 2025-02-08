```csharp
// dECMC.cs
// Converted from: dECMC.c

using System;

public class VipsdECMC : VipsColourDifference
{
    public static void ClassInit(VipsdECMCClass @class)
    {
        VipsObjectClass object_class = (VipsObjectClass)@class;
        VipsColourClass colour_class = VIPS_COLOUR_CLASS(@class);

        object_class.Nickname = "dECMC";
        object_class.Description = "calculate dECMC";

        colour_class.ProcessLine = vips__pythagoras_line;
    }

    public static void Init(VipsdECMC dECMC)
    {
        VipsColourDifference difference = (VipsColourDifference)dECMC;

        difference.Interpretation = VIPS_INTERPRETATION_CMC;
    }
}

public class VipsImage
{
    // ... other methods and properties ...
}

public static class Vips
{
    public static int dECMC(VipsImage left, VipsImage right, out VipsImage @out)
    {
        return vips_call_split("dECMC", left, right, out);
    }
}
```

```csharp
// vips.cs
// Converted from: vips.c

using System;

public class VipsObjectClass : VipsObject
{
    public string Nickname { get; set; }
    public string Description { get; set; }

    // ... other properties and methods ...
}

public class VipsColourClass : VipsColour
{
    public Func<VipsImage, int> ProcessLine { get; set; }

    // ... other properties and methods ...
}
```