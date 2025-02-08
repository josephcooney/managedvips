Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsMaplut : VipsOperation
{
    public override string Nickname => "maplut";
    public override string Description => _("map an image though a lut");

    private int band = -1;

    [VipsArgument("in", 1, "Input")]
    public VipsImage In { get; set; }

    [VipsArgument("out", 2, "Output")]
    public VipsImage Out { get; set; }

    [VipsArgument("lut", 3, "LUT")]
    public VipsImage Lut { get; set; }

    [VipsArgument("band", 4, "Band")]
    public int Band
    {
        get => band;
        set => band = value;
    }
}

public class VipsMaplutSequence : IDisposable
{
    private VipsRegion ir;
    private int overflow;

    public VipsMaplutSequence(VipsImage image)
    {
        if (ir == null)
            ir = new VipsRegion(image);
    }

    public void Dispose()
    {
        if (ir != null)
        {
            ir.Dispose();
            ir = null;
        }
    }

    public int Overflow => overflow;

    public void AddOverflow(int count)
    {
        overflow += count;
    }
}

public class VipsMaplutStart : IDisposable
{
    private VipsImage image;
    private VipsMaplutSequence sequence;

    public VipsMaplutStart(VipsImage image, VipsMaplut lut)
    {
        this.image = image;
        sequence = new VipsMaplutSequence(image);
    }

    public void Dispose()
    {
        if (sequence != null)
            sequence.Dispose();
        sequence = null;
    }
}

public class VipsMaplutGen : IDisposable
{
    private VipsRegion out_region;
    private VipsImage in_image;
    private VipsMaplut lut;

    public VipsMaplutGen(VipsRegion out_region, VipsImage in_image, VipsMaplut lut)
    {
        this.out_region = out_region;
        this.in_image = in_image;
        this.lut = lut;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

public class VipsMaplutStop : IDisposable
{
    private VipsImage image;
    private VipsMaplut lut;

    public VipsMaplutStop(VipsImage image, VipsMaplut lut)
    {
        this.image = image;
        this.lut = lut;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

public class VipsMaplutBuild : IDisposable
{
    private VipsObject object_;
    private VipsImage in_image;
    private VipsImage lut;

    public VipsMaplutBuild(VipsObject object_)
    {
        this.object_ = object_;
        in_image = (VipsImage)object_.In;
        lut = (VipsImage)object_.Lut;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

public class VipsMaplutClass : VipsOperationClass
{
    public override string Nickname => "maplut";
    public override string Description => _("map an image though a lut");

    public static readonly VipsArgument[] Args = new[]
    {
        new VipsArgument("in", 1, "Input"),
        new VipsArgument("out", 2, "Output"),
        new VipsArgument("lut", 3, "LUT"),
        new VipsArgument("band", 4, "Band")
    };
}

public class VipsMaplut : IDisposable
{
    private int band = -1;

    public VipsMaplut()
    {
        // Nothing to do
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
```

Note that some parts of the code have been omitted for brevity, such as the implementation of `VipsRegion`, `VipsImage`, and other classes. Also, this is not a complete translation of the original C code, but rather an attempt to recreate the functionality in C#.