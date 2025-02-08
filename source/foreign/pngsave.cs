Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignSavePng : VipsForeignSave
{
    public int Compression { get; set; }
    public bool Interlace { get; set; }
    public VipsForeignPngFilter Filter { get; set; }
    public bool Palette { get; set; }
    public int Q { get; set; }
    public double Dither { get; set; }
    public int Bitdepth { get; set; }
    public int Effort { get; set; }

    public VipsTarget Target { get; set; }

    // Set by subclasses.
}

public class VipsForeignSavePngTarget : VipsForeignSavePng
{
    public VipsTarget Target { get; set; }

    public override bool Build(VipsObject obj)
    {
        base.Build(obj);
        this.Target = ((VipsForeignSavePngTarget)obj).Target;
        return true;
    }
}

public class VipsForeignSavePngFile : VipsForeignSavePng
{
    public string Filename { get; set; }

    public override bool Build(VipsObject obj)
    {
        base.Build(obj);
        this.Target = vips_target_new_to_file(this.Filename);
        return true;
    }
}

public class VipsForeignSavePngBuffer : VipsForeignSavePng
{
    public VipsArea Buffer { get; set; }

    public override bool Build(VipsObject obj)
    {
        base.Build(obj);
        this.Target = vips_target_new_to_memory();
        g_object_get(this.Target, "blob", out var blob, null);
        this.Buffer = new VipsArea(blob.Data, blob.Length);
        return true;
    }
}

public class VipsForeignSavePngClass : VipsForeignSaveClass
{
    public static void Register(VipsObjectClass type)
    {
        // ...
    }

    public override bool Build(VipsObject obj)
    {
        var png = (VipsForeignSavePng)obj;
        if (!base.Build(obj))
            return false;

        if (!vips_object_argument_isset(obj, "bitdepth"))
            png.Bitdepth =
                ((VipsImage)obj).Type == VIPS_INTERPRETATION_RGB16 ||
                    ((VipsImage)obj).Type == VIPS_INTERPRETATION_GREY16
                ? 16
                : 8;

        if (vips_object_argument_isset(obj, "colours"))
            png.Bitdepth = (int)Math.Ceiling(Math.Log(png.Colours, 2));

        if (png.Bitdepth <= 8)
        {
            var x = new VipsImage();
            if (!vips_cast((VipsImage)obj, out x, VIPS_FORMAT_UCHAR, null))
                return false;
            ((VipsImage)obj) = x;
        }

        if (((VipsImage)obj).Bands > 2 && png.Bitdepth < 8)
            png.Palette = true;

        if (png.Bitdepth > 8)
            png.Palette = false;

        var result = vips__png_write_target((VipsImage)obj, png.Target,
            png.Compression, png.Interlace, null, png.Filter,
            png.Palette, png.Q, png.Dither,
            png.Bitdepth, png.Effort);
        if (result)
            return false;
        if (!vips_target_end(png.Target))
            return false;

        return true;
    }
}

public class VipsForeignSavePngTargetClass : VipsForeignSavePngClass
{
    public override bool Build(VipsObject obj)
    {
        base.Build(obj);
        var png = (VipsForeignSavePng)obj;
        png.Target = ((VipsForeignSavePngTarget)obj).Target;
        return true;
    }
}

public class VipsForeignSavePngFileClass : VipsForeignSavePngClass
{
    public override bool Build(VipsObject obj)
    {
        base.Build(obj);
        var file = (VipsForeignSavePngFile)obj;
        file.Target = vips_target_new_to_file(file.Filename);
        return true;
    }
}

public class VipsForeignSavePngBufferClass : VipsForeignSavePngClass
{
    public override bool Build(VipsObject obj)
    {
        base.Build(obj);
        var buffer = (VipsForeignSavePngBuffer)obj;
        buffer.Target = vips_target_new_to_memory();
        g_object_get(buffer.Target, "blob", out var blob, null);
        buffer.Buffer = new VipsArea(blob.Data, blob.Length);
        return true;
    }
}

public class VipsImage
{
    public int Bands { get; set; }
    public int Type { get; set; }
}

public enum VipsInterpretation
{
    RGB16,
    GREY16,
    // ...
}

public enum VipsForeignPngFilter
{
    None,
    FilterType0,
    FilterType1,
    // ...
}

public class VipsTarget
{
    public void* Blob { get; set; }
}

public class VipsArea : IDisposable
{
    public byte[] Data { get; private set; }
    public int Length { get; private set; }

    public VipsArea(byte[] data, int length)
    {
        this.Data = data;
        this.Length = length;
    }

    public void Dispose()
    {
        // ...
    }
}

public class VipsBlob : IDisposable
{
    public byte[] Data { get; private set; }
    public int Length { get; private set; }

    public VipsBlob(byte[] data, int length)
    {
        this.Data = data;
        this.Length = length;
    }

    public void Dispose()
    {
        // ...
    }
}

public class VipsForeignSave
{
    public virtual bool Build(VipsObject obj)
    {
        return true;
    }
}

public class VipsObjectClass : VipsObject
{
    public string Nickname { get; set; }
    public string Description { get; set; }

    public virtual void Dispose()
    {
        // ...
    }
}

public class VipsObject : IDisposable
{
    public bool Build(VipsObject obj)
    {
        return true;
    }

    public void Dispose()
    {
        // ...
    }
}
```

Note that this is not a complete implementation, but rather an excerpt of the code. You will need to add more functionality and error handling as needed.

Also note that some types (e.g., `VipsImage`, `VipsTarget`) are assumed to be defined elsewhere in your project, while others (e.g., `VipsArea`, `VipsBlob`) are new classes created for this example.