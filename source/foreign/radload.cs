Here is the C# code equivalent to the provided C code:

```csharp
// load radlab from a file
// 5/12/11 - from tiffload.c

using System;
using VipsDotNet;

public class RadLoadBase : ForeignLoad
{
    public RadLoadBase()
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Unref(Source);
        }
        base.Dispose(disposing);
    }

    public override VipsForeignFlags GetFlags(VipsObject load)
    {
        return VipsForeignFlags.Sequential;
    }

    public override VipsForeignFlags GetFlagsFilename(string filename)
    {
        return VipsForeignFlags.Sequential;
    }

    public int Header(VipsObject load)
    {
        if (VipsRadHeader(Source, Load.Out))
            return -1;

        return 0;
    }

    public int Load(VipsObject load)
    {
        if (VipsRadLoad(Source, Load.Real))
            return -1;

        return 0;
    }
}

public class RadLoadSource : RadLoadBase
{
    public VipsSource Source { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public RadLoadSource()
    {
    }

    public static bool IsASource(VipsSource source)
    {
        return VipsRadIsrad(source);
    }
}

public class RadLoadFile : RadLoadBase
{
    public string Filename { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public RadLoadFile()
    {
    }

    public static bool IsA(string filename)
    {
        VipsSource source = VipsSource.NewFromFile(filename);

        if (source == null)
            return false;

        bool result = RadLoadSource.IsASource(source);
        Unref(source);

        return result;
    }
}

public class RadLoadBuffer : RadLoadBase
{
    public VipsBlob Blob { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    public RadLoadBuffer()
    {
    }

    public static bool IsABuffer(byte[] buf, int len)
    {
        VipsSource source = VipsSource.NewFromMemory(buf, len);

        if (source == null)
            return false;

        bool result = RadLoadSource.IsASource(source);
        Unref(source);

        return result;
    }
}

public class Program
{
    public static int Radload(string filename, out VipsImage image, params object[] args)
    {
        var load = new RadLoadFile { Filename = filename };
        return VipsCallSplit("radload", load, image);
    }

    public static int RadloadBuffer(byte[] buf, int len, out VipsImage image, params object[] args)
    {
        var blob = new VipsBlob(buf, len);
        var load = new RadLoadBuffer { Blob = blob };
        int result = VipsCallSplit("radload_buffer", load, image);

        Unref(blob);

        return result;
    }

    public static int RadloadSource(VipsSource source, out VipsImage image, params object[] args)
    {
        var load = new RadLoadSource { Source = source };
        return VipsCallSplit("radload_source", load, image);
    }
}
```

Note that this code assumes the existence of a `VipsDotNet` library, which is not included in the .NET Framework. You will need to install this library separately.

Also note that some methods and classes have been renamed or reorganized for better clarity and consistency with C# conventions.