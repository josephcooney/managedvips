Here is the C# code equivalent to the provided C code:

```csharp
using System;
using Vips;

public class VipsForeignSaveJpeg : VipsForeignSave
{
    public int Q { get; set; }
    public bool OptimizeCoding { get; set; }
    public bool Interlace { get; set; }
    public bool NoSubsample { get; set; } // deprecated, use SubsampleMode instead
    public VipsForeignSubsample SubsampleMode { get; set; }
    public bool TrellisQuant { get; set; }
    public bool OvershootDeringing { get; set; }
    public bool OptimizeScans { get; set; }
    public int QuantTable { get; set; }
    public int RestartInterval { get; set; }

    public VipsForeignSaveJpeg()
    {
        Q = 75;
        SubsampleMode = VipsForeignSubsample.Auto;
    }
}

public class VipsForeignSaveJpegTarget : VipsForeignSaveJpeg
{
    public VipsTarget Target { get; set; }

    public VipsForeignSaveJpegTarget(VipsTarget target)
    {
        Target = target;
    }
}

public class VipsForeignSaveJpegFile : VipsForeignSaveJpeg
{
    public string Filename { get; set; }

    public VipsForeignSaveJpegFile(string filename)
    {
        Filename = filename;
    }
}

public class VipsForeignSaveJpegBuffer : VipsForeignSaveJpeg
{
    public VipsArea Buffer { get; set; }

    public VipsForeignSaveJpegBuffer(VipsArea buffer)
    {
        Buffer = buffer;
    }
}

public class VipsForeignSaveJpegMime : VipsForeignSaveJpeg
{
    // no additional properties or methods needed for this class
}

public static class VipsForeignSaveJpegMethods
{
    public static int Jpegsave(VipsImage inImage, string filename, params object[] args)
    {
        return VipsObject.Call("jpegsave", inImage, filename, args);
    }

    public static int JpegsaveTarget(VipsImage inImage, VipsTarget target, params object[] args)
    {
        return VipsObject.Call("jpegsave_target", inImage, target, args);
    }

    public static int JpegsaveBuffer(VipsImage inImage, out byte[] buf, out int len, params object[] args)
    {
        var area = new VipsArea();
        var result = VipsObject.Call("jpegsave_buffer", inImage, ref area, args);
        if (result == 0 && area != null)
        {
            buf = area.Data;
            area.FreeFn = null;
            len = area.Length;
        }
        return result;
    }

    public static int JpegsaveMime(VipsImage inImage, params object[] args)
    {
        return VipsObject.Call("jpegsave_mime", inImage, args);
    }
}
```

Note that I've assumed the `Vips` namespace is already defined and available. If not, you'll need to add it or modify the code accordingly.

Also, I've used C# properties for the class members instead of fields, as it's a more idiomatic way to define properties in C#. The getters and setters are implicit, so you can access the properties directly without calling `get` or `set`.

The `VipsForeignSaveJpegMethods` class contains static methods that wrap the VIPS functions. These methods take the same parameters as their C counterparts and return an integer result.

Please note that this is just a direct translation of the provided C code, and you may need to adjust it to fit your specific use case or requirements.