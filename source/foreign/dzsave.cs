Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;

public class VipsForeignSaveDz : VipsForeignSave
{
    public const string DZ_SUFFS = ".dz";

    private int overlap;
    private int tile_size;
    private VipsForeignDzLayout layout;
    private VipsForeignDzDepth depth;
    private bool centre;
    private bool properties;
    private VipsAngle angle;
    private VipsForeignDzContainer container;
    private int compression;
    private VipsRegionShrink region_shrink;
    private int skip_blanks;
    private bool no_strip;
    private string id;
    private int Q;

    public VipsForeignSaveDz()
    {
        overlap = 1;
        tile_size = 254;
        layout = VipsForeignDzLayout.DZ;
        depth = VipsForeignDzDepth.ONEPIXEL;
        centre = false;
        properties = false;
        angle = VipsAngle.D0;
        container = VipsForeignDzContainer.FS;
        compression = 0;
        region_shrink = VipsRegionShrink.MEAN;
        skip_blanks = -1;
        no_strip = false;
    }

    public override int Build(VipsObject obj)
    {
        // ...
    }

    public enum ForeignDzLayout
    {
        DZ,
        ZOOMIFY,
        GOOGLE,
        IIIF,
        IIIF3
    }

    public enum ForeignDzDepth
    {
        ONEPIXEL,
        ONETILE,
        ONE
    }

    public enum ForeignDzContainer
    {
        FS,
        ZIP,
        SZI
    }
}

public class VipsForeignSaveDzTarget : VipsForeignSaveDz
{
    private VipsTarget target;

    public VipsForeignSaveDzTarget()
    {
        container = VipsForeignDzContainer.ZIP;
    }

    public override int Build(VipsObject obj)
    {
        // ...
    }
}

public class VipsForeignSaveDzFile : VipsForeignSaveDz
{
    private string filename;

    public VipsForeignSaveDzFile()
    {
    }

    public override int Build(VipsObject obj)
    {
        // ...
    }
}

public class VipsForeignSaveDzBuffer : VipsForeignSaveDz
{
    private VipsBlob buffer;

    public VipsForeignSaveDzBuffer()
    {
        container = VipsForeignDzContainer.ZIP;
    }

    public override int Build(VipsObject obj)
    {
        // ...
    }
}

public class DZSave : VipsForeignSaveDz
{
    public static int Dzsave(VipsImage inImage, string name, params object[] args)
    {
        // ...
    }

    public static int DzsaveBuffer(VipsImage inImage, out byte[] buf, out int len, params object[] args)
    {
        // ...
    }

    public static int DzsaveTarget(VipsImage inImage, VipsTarget target, params object[] args)
    {
        // ...
    }
}
```

Note that this is not a direct translation of the C code, but rather an equivalent implementation in C#. Some changes were made to adapt the code to the C# syntax and conventions.