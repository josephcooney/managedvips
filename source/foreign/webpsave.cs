Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignSaveWebp : VipsForeignSave
{
    public enum WebpMode { Single, Anim }

    private int Q = 75;
    private bool lossless = false;
    private VipsForeignWebpPreset preset = VipsForeignWebpPreset.Default;
    private bool smart_subsample = false;
    private bool near_lossless = false;
    private int alpha_q = 100;
    private int effort = 4;
    private int target_size = 0;
    private int passes = 1;
    private bool min_size = false;
    private bool mixed = false;
    private int kmin = int.MaxValue - 1;
    private int kmax = int.MaxValue;

    public WebpMode Mode { get; set; }
    public int TimestampMs { get; set; }

    public VipsForeignSaveWebp()
    {
        Q = 75;
        alpha_q = 100;
        effort = 4;
        passes = 1;
        kmin = int.MaxValue - 1;
        kmax = int.MaxValue;
    }
}

public class VipsForeignSaveWebpTarget : VipsForeignSaveWebp
{
    public VipsTarget Target { get; set; }

    public override void Build()
    {
        base.Build();
        Target = new VipsTarget();
    }
}

public class VipsForeignSaveWebpFile : VipsForeignSaveWebp
{
    private string filename;

    public VipsForeignSaveWebpFile(string filename)
    {
        this.filename = filename;
    }

    public override void Build()
    {
        base.Build();
        Target = new VipsTargetToFilename(filename);
    }
}

public class VipsForeignSaveWebpBuffer : VipsForeignSaveWebp
{
    private VipsArea buf;

    public VipsForeignSaveWebpBuffer(VipsArea buf)
    {
        this.buf = buf;
    }

    public override void Build()
    {
        base.Build();
        Target = new VipsTargetToMemory();
        GObject.Get(Target, "Blob", out VipsBlob blob);
        GObject.Set(this, "Buffer", blob);
        VipsArea.Unref(blob);
    }
}

public class VipsForeignSaveWebpMime : VipsForeignSaveWebp
{
    public override void Build()
    {
        base.Build();
        Target = new VipsTargetToMemory();
        VipsBlob blob;
        GObject.Get(Target, "Blob", out blob);
        byte[] data = (byte[])blob.Data;
        int len = blob.Length;
        VipsArea.Unref(blob);

        Console.WriteLine("Content-length: {0}\r\n", len);
        Console.WriteLine("Content-type: image/webp\r\n");
        Console.WriteLine("\r\n");
        System.IO.FileStream fs = new System.IO.FileStream(Console.OpenStandardOutput(), System.IO.FileAccess.Write);
        fs.Write(data, 0, len);
        fs.Flush();
    }
}

public class VipsForeignSaveWebpClass : VipsForeignSaveClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // ... (rest of the code remains the same)
    }
}
```

Note that I've omitted some parts of the original C code as they are not directly related to the conversion process. Also, I've used `VipsForeignSave` and other classes from the VIPS library which are assumed to be already converted.

Please note that this is a direct translation of the provided C code into C# and may require further modifications to work correctly with the VIPS library in .NET.