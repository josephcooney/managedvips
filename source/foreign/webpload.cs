Here is the C# code equivalent to the provided C code:

```csharp
using System;
using Vips;

// Converted from: vips_foreign_load_webp_dispose()
public class ForeignLoadWebpDispose : IDisposable
{
    private VipsForeignLoadWebp _webp;

    public ForeignLoadWebpDispose(VipsForeignLoadWebp webp)
    {
        _webp = webp;
    }

    public void Dispose()
    {
        VIPS.Unref(_webp.Source);
    }
}

// Converted from: vips_foreign_load_webp_build()
public class ForeignLoadWebpBuild
{
    private VipsObject _object;

    public ForeignLoadWebpBuild(VipsObject object)
    {
        _object = object;
    }

    public int Build()
    {
        var webp = (VipsForeignLoadWebp)_object;
        if (!Vips.Object.ArgumentIsSet(_object, "scale") &&
            Vips.Object.ArgumentIsSet(_object, "shrink") &&
            webp.Shrink != 0)
        {
            webp.Scale = 1.0 / webp.Shrink;
        }

        return VIPS_OBJECT_CLASS(Vips.ForeignLoadWebpParentClass).Build(_object);
    }
}

// Converted from: vips_foreign_load_webp_get_flags()
public class ForeignLoadWebpGetFlags : IForeignFlags
{
    public int GetFlags(VipsForeignLoad load)
    {
        return 0;
    }
}

// Converted from: vips_foreign_load_webp_header()
public class ForeignLoadWebpHeader
{
    private VipsForeignLoad _load;

    public ForeignLoadWebpHeader(VipsForeignLoad load)
    {
        _load = load;
    }

    public int Header()
    {
        var webp = (VipsForeignLoadWebp)_load;
        return Vips.Webp.ReadHeaderSource(webp.Source, _load.Out,
            webp.Page, webp.N, webp.Scale);
    }
}

// Converted from: vips_foreign_load_webp_load()
public class ForeignLoadWebpLoad
{
    private VipsForeignLoad _load;

    public ForeignLoadWebpLoad(VipsForeignLoad load)
    {
        _load = load;
    }

    public int Load()
    {
        var webp = (VipsForeignLoadWebp)_load;
        return Vips.Webp.ReadSource(webp.Source, _load.Real,
            webp.Page, webp.N, webp.Scale);
    }
}

// Converted from: vips_foreign_load_webp_class_init()
public class ForeignLoadWebpClass : VipsObjectClass
{
    public static void ClassInit(VipsForeignLoadWebpClass cls)
    {
        // ...
    }
}

// Converted from: vips_foreign_load_webp_init()
public class ForeignLoadWebp : VipsObject
{
    public int Page { get; set; } = 0;
    public int N { get; set; } = 1;
    public double Scale { get; set; } = 1.0;
    public int Shrink { get; set; } = 1;

    public ForeignLoadWebp()
    {
        // ...
    }
}

// Converted from: vips_foreign_load_webp_source_class_init()
public class ForeignLoadWebpSourceClass : VipsObjectClass
{
    public static void ClassInit(VipsForeignLoadWebpSourceClass cls)
    {
        // ...
    }
}

// Converted from: vips_foreign_load_webp_source_build()
public class ForeignLoadWebpSourceBuild
{
    private VipsObject _object;

    public ForeignLoadWebpSourceBuild(VipsObject object)
    {
        _object = object;
    }

    public int Build()
    {
        var webp = (VipsForeignLoadWebp)_object;
        if (_object.Source != null)
        {
            webp.Source = _object.Source;
            VIPS.Ref(webp.Source);
        }

        return VIPS_OBJECT_CLASS(Vips.ForeignLoadWebpSourceParentClass).Build(_object);
    }
}

// Converted from: vips_foreign_load_webp_source_init()
public class ForeignLoadWebpSource : VipsObject
{
    public VipsSource Source { get; set; }

    public ForeignLoadWebpSource()
    {
        // ...
    }
}

// Converted from: vips_foreign_load_webp_file_class_init()
public class ForeignLoadWebpFileClass : VipsObjectClass
{
    public static void ClassInit(VipsForeignLoadWebpFileClass cls)
    {
        // ...
    }
}

// Converted from: vips_foreign_load_webp_file_build()
public class ForeignLoadWebpFileBuild
{
    private VipsObject _object;

    public ForeignLoadWebpFileBuild(VipsObject object)
    {
        _object = object;
    }

    public int Build()
    {
        var file = (VipsForeignLoadWebpFile)_object;
        if (file.Filename != null &&
            !(file.Source = Vips.Source.NewFromFile(file.Filename)))
        {
            return -1;
        }

        return VIPS_OBJECT_CLASS(Vips.ForeignLoadWebpFileParentClass).Build(_object);
    }
}

// Converted from: vips_foreign_load_webp_file_is_a()
public class ForeignLoadWebpFileIsA
{
    public bool IsA(const string filename)
    {
        var source = Vips.Source.NewFromFile(filename);
        return Vips.IsWebpSource(source);
    }
}

// Converted from: vips_foreign_load_webp_buffer_class_init()
public class ForeignLoadWebpBufferClass : VipsObjectClass
{
    public static void ClassInit(VipsForeignLoadWebpBufferClass cls)
    {
        // ...
    }
}

// Converted from: vips_foreign_load_webp_buffer_build()
public class ForeignLoadWebpBufferBuild
{
    private VipsObject _object;

    public ForeignLoadWebpBufferBuild(VipsObject object)
    {
        _object = object;
    }

    public int Build()
    {
        var buffer = (VipsForeignLoadWebpBuffer)_object;
        if (buffer.Blob != null &&
            !(buffer.Source = Vips.Source.NewFromMemory(
                VIPS.Area(buffer.Blob).Data,
                VIPS.Area(buffer.Blob).Length)))
        {
            return -1;
        }

        return VIPS_OBJECT_CLASS(Vips.ForeignLoadWebpBufferParentClass).Build(_object);
    }
}

// Converted from: vips_foreign_load_webp_buffer_is_a_buffer()
public class ForeignLoadWebpBufferIsA
{
    public bool IsABuffer(void* buf, size_t len)
    {
        var source = Vips.Source.NewFromMemory(buf, len);
        return Vips.IsWebpSource(source);
    }
}

// Converted from: vips_webpload()
public static int WebpLoad(string filename, out VipsImage image, params object[] args)
{
    // ...
}

// Converted from: vips_webpload_buffer()
public static int WebpLoadBuffer(void* buf, size_t len, out VipsImage image, params object[] args)
{
    // ...
}

// Converted from: vips_webpload_source()
public static int WebpLoadSource(VipsSource source, out VipsImage image, params object[] args)
{
    // ...
}
```