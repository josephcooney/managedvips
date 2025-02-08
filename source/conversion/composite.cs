Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public abstract class VipsCompositeBase : VipsConversion
{
    public VipsArrayImage In { get; set; }
    public VipsArrayInt Mode { get; set; }
    public VipsInterpretation CompositingSpace { get; set; }
    public bool Premultiplied { get; set; }
    public int[] XOffset { get; set; }
    public int[] YOffset { get; set; }
    public VipsRect[] Subimages { get; set; }
    public int Bands { get; set; }
    public double[] MaxBand { get; set; }
    public bool Skippable { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (In != null)
            In.Dispose();
        if (Mode != null)
            Mode.Dispose();
        VipsRect[] subimages = Subimages;
        if (subimages != null)
            subimages.Clear();

        base.Dispose(disposing);
    }
}

public class VipsCompositeSequence
{
    public VipsCompositeBase Composite { get; set; }
    public VipsRegion[] InputRegions { get; set; }
    public int N { get; set; }
    public int[] Enabled { get; set; }
    public VipsPel[] P { get; set; }

    ~VipsCompositeSequence()
    {
        if (InputRegions != null)
            foreach (var region in InputRegions)
                region.Dispose();
        InputRegions = null;

        if (Enabled != null)
            Enabled.Clear();
        Enabled = null;

        if (P != null)
            P.Clear();
        P = null;
    }
}

public class VipsCompositeBaseClass : VipsConversionClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public abstract class VipsCompositeSequenceClass : VipsCompositeBaseClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public enum BlendMode
{
    Clear,
    Source,
    Over,
    In,
    Out,
    Atop,
    Dest,
    DestOver,
    DestIn,
    DestOut,
    DestAtop,
    Xor,
    Add,
    Saturate,
    Multiply,
    Screen,
    Overlay,
    Darken,
    Lighten,
    ColourDodge,
    ColourBurn,
    HardLight,
    SoftLight,
    Difference,
    Exclusion
}

public class VipsComposite : VipsCompositeBase
{
    public VipsArrayInt X { get; set; }
    public VipsArrayInt Y { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (X != null)
            X.Dispose();
        if (Y != null)
            Y.Dispose();

        base.Dispose(disposing);
    }
}

public class VipsComposite2 : VipsCompositeBase
{
    public VipsImage Base { get; set; }
    public VipsImage Overlay { get; set; }
    public BlendMode Mode { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (Base != null)
            Base.Dispose();
        if (Overlay != null)
            Overlay.Dispose();

        base.Dispose(disposing);
    }
}

public class VipsComposite2Class : VipsCompositeBaseClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public class VipsCompositeSequenceClass : VipsCompositeSequenceClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public abstract class VipsConversionClass : VipsObjectClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public enum Interpretation
{
    XYZ,
    LAB,
    LCH,
    CMC,
    scRGB,
    sRGB,
    HSV,
    CMYK,
    RGB16,
    GREY16,
    YXY,
    B_W
}

public class VipsCompositeBaseClass : VipsConversionClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public abstract class VipsObjectClass : VipsObjectBaseClass
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public enum DemandStyle
{
    SmallTile,
    LargeTile
}

public class VipsCompositeSequence : IDisposable
{
    private VipsCompositeBase _composite;
    private VipsRegion[] _inputRegions;
    private int _n;
    private int[] _enabled;
    private VipsPel[] _p;

    public VipsCompositeSequence(VipsCompositeBase composite)
    {
        _composite = composite;
        _inputRegions = new VipsRegion[composite.In.Area.N + 1];
        _enabled = new int[composite.In.Area.N];
        _p = new VipsPel[composite.In.Area.N];
    }

    public void Dispose()
    {
        if (_inputRegions != null)
            foreach (var region in _inputRegions)
                region.Dispose();
        _inputRegions = null;

        if (_enabled != null)
            _enabled.Clear();
        _enabled = null;

        if (_p != null)
            _p.Clear();
        _p = null;
    }
}

public class VipsComposite : IDisposable
{
    private VipsCompositeBase _composite;
    private VipsArrayInt _x;
    private VipsArrayInt _y;

    public VipsComposite(VipsCompositeBase composite)
    {
        _composite = composite;
        _x = new VipsArrayInt();
        _y = new VipsArrayInt();
    }

    public void Dispose()
    {
        if (_x != null)
            _x.Dispose();
        if (_y != null)
            _y.Dispose();

        base.Dispose(true);
    }
}

public class VipsComposite2 : IDisposable
{
    private VipsCompositeBase _composite;
    private VipsImage _base;
    private VipsImage _overlay;
    private BlendMode _mode;
    private int _x;
    private int _y;

    public VipsComposite2(VipsCompositeBase composite)
    {
        _composite = composite;
        _base = null;
        _overlay = null;
        _mode = BlendMode.Over;
        _x = 0;
        _y = 0;
    }

    public void Dispose()
    {
        if (_base != null)
            _base.Dispose();
        if (_overlay != null)
            _overlay.Dispose();

        base.Dispose(true);
    }
}

public class VipsCompositeSequenceClass : VipsCompositeSequence
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public class VipsComposite2Class : VipsComposite2
{
    public static void RegisterType(Type type)
    {
        TypeDescriptor.AddAttributes(type, new Attribute[] { new ObsoleteAttribute() });
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
```

Note that this is a direct translation of the provided C code and may not be perfect. Some parts of the original code are complex and might require additional modifications to work correctly in C#. Additionally, some types (like `VipsImage`, `VipsRegion`, etc.) were assumed to exist in the target assembly, but their actual implementation is not shown here.

Also note that this code does not include any error handling or debugging mechanisms. You may need to add those depending on your specific requirements.

Please let me know if you have any further questions or if there's anything else I can help with!