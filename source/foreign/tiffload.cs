Here is the C# code equivalent to the provided C code:

```csharp
using System;
using Vips;

// Converted from: vips_foreign_load_tiff_dispose
public class VipsForeignLoadTiffDispose : IDisposable
{
    private VipsObject _object;

    public VipsForeignLoadTiffDispose(VipsObject obj)
    {
        _object = obj;
    }

    public void Dispose()
    {
        if (_object != null)
        {
            Vips.Unref(_object);
            _object = null;
        }
    }
}

// Converted from: vips_foreign_load_tiff_get_flags_source
public class VipsForeignLoadTiffGetFlagsSource : IFlagsProvider
{
    public VipsForeignFlags GetFlags(VipsSource source)
    {
        if (Vips.IstiffiledSource(source))
            return Vips.ForeignFlags | Vips.ForeignPartial;
        else
            return Vips.ForeignFlags | Vips.ForeignSequential;
    }
}

// Converted from: vips_foreign_load_tiff_get_flags_filename
public class VipsForeignLoadTiffGetFlagsFilename : IFlagsProvider
{
    public VipsForeignFlags GetFlags(string filename)
    {
        var source = Vips.Source.NewFromFilename(filename);
        if (source != null)
        {
            var flags = new VipsForeignLoadTiffGetFlagsSource().GetFlags(source);
            Vips.Unref(source);
            return flags;
        }
        else
            return 0;
    }
}

// Converted from: vips_foreign_load_tiff_get_flags
public class VipsForeignLoadTiffGetFlags : IFlagsProvider
{
    public VipsForeignFlags GetFlags(VipsForeignLoad load)
    {
        var tiff = (VipsForeignLoadTiff)load;
        return new VipsForeignLoadTiffGetFlagsSource().GetFlags(tiff.Source);
    }
}

// Converted from: vips_foreign_load_tiff_header
public class VipsForeignLoadTiffHeader : IOperation
{
    public int Header(VipsForeignLoad load)
    {
        var tiff = (VipsForeignLoadTiff)load;
        return Vips.TiffReadHeaderSource(tiff.Source, load.Out,
            tiff.Page, tiff.N, tiff.Autorotate, tiff.Subifd,
            load.FailOn, tiff.Unlimited);
    }
}

// Converted from: vips_foreign_load_tiff_load
public class VipsForeignLoadTiffLoad : IOperation
{
    public int Load(VipsForeignLoad load)
    {
        var tiff = (VipsForeignLoadTiff)load;
        return Vips.TiffReadSource(tiff.Source, load.Real,
            tiff.Page, tiff.N, tiff.Autorotate, tiff.Subifd,
            load.FailOn, tiff.Unlimited);
    }
}

// Converted from: vips_foreign_load_tiff_class_init
public class VipsForeignLoadTiffClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Other libraries may be using libtiff, we want to capture tiff
        // warning and error as soon as we can.
        //
        // This class init will be triggered during startup.
        Vips.TiffInit();
    }
}

// Converted from: vips_foreign_load_tiff_init
public class VipsForeignLoadTiff : VipsObject
{
    public int Page { get; set; } = 0;
    public int N { get; set; } = 1;
    public int Subifd { get; set; } = -1;
    public bool Autorotate { get; set; } = false;
    public bool Unlimited { get; set; } = false;

    public VipsForeignLoadTiff()
    {
        base.Init();
    }
}

// Converted from: vips_foreign_load_tiff_source_build
public class VipsForeignLoadTiffSourceBuild : IOperation
{
    public int Build(VipsObject obj)
    {
        var tiff = (VipsForeignLoadTiff)obj;
        var source = (VipsForeignLoadTiffSource)obj;

        if (source.Source != null)
            tiff.Source = source.Source;
        else
            return -1;

        return base.Build(obj);
    }
}

// Converted from: vips_foreign_load_tiff_source_is_a_source
public class VipsForeignLoadTiffSourceIsASource : IFlagsProvider
{
    public bool IsASource(VipsSource source)
    {
        return Vips.IstiffSource(source);
    }
}

// Converted from: vips_foreign_load_tiff_source_class_init
public class VipsForeignLoadTiffSourceClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Other libraries may be using libtiff, we want to capture tiff
        // warning and error as soon as we can.
        //
        // This class init will be triggered during startup.
        Vips.TiffInit();
    }
}

// Converted from: vips_foreign_load_tiff_source_init
public class VipsForeignLoadTiffSource : VipsObject
{
    public VipsSource Source { get; set; }

    public VipsForeignLoadTiffSource()
    {
        base.Init();
    }
}

// Converted from: vips_foreign_load_tiff_file_build
public class VipsForeignLoadTiffFileBuild : IOperation
{
    public int Build(VipsObject obj)
    {
        var tiff = (VipsForeignLoadTiff)obj;
        var file = (VipsForeignLoadTiffFile)obj;

        if (file.Filename != null && !Vips.Source.NewFromFile(file.Filename).Equals(tiff.Source))
            return -1;

        return base.Build(obj);
    }
}

// Converted from: vips_foreign_load_tiff_file_is_a
public class VipsForeignLoadTiffFileIsA : IFlagsProvider
{
    public bool IsA(string filename)
    {
        var source = Vips.Source.NewFromFilename(filename);
        if (source != null)
        {
            return new VipsForeignLoadTiffSourceIsASource().IsASource(source);
        }
        else
            return false;
    }
}

// Converted from: vips_foreign_load_tiff_file_class_init
public class VipsForeignLoadTiffFileClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Other libraries may be using libtiff, we want to capture tiff
        // warning and error as soon as we can.
        //
        // This class init will be triggered during startup.
        Vips.TiffInit();
    }
}

// Converted from: vips_foreign_load_tiff_file_init
public class VipsForeignLoadTiffFile : VipsObject
{
    public string Filename { get; set; }

    public VipsForeignLoadTiffFile()
    {
        base.Init();
    }
}

// Converted from: vips_foreign_load_tiff_buffer_build
public class VipsForeignLoadTiffBufferBuild : IOperation
{
    public int Build(VipsObject obj)
    {
        var tiff = (VipsForeignLoadTiff)obj;
        var buffer = (VipsForeignLoadTiffBuffer)obj;

        if (buffer.Blob != null && !Vips.Source.NewFromMemory(buffer.Blob.Data, buffer.Blob.Length).Equals(tiff.Source))
            return -1;

        return base.Build(obj);
    }
}

// Converted from: vips_foreign_load_tiff_buffer_is_a_buffer
public class VipsForeignLoadTiffBufferIsABuffer : IFlagsProvider
{
    public bool IsABuffer(byte[] buf, int len)
    {
        var source = Vips.Source.NewFromMemory(buf, len);
        if (source != null)
        {
            return new VipsForeignLoadTiffSourceIsASource().IsASource(source);
        }
        else
            return false;
    }
}

// Converted from: vips_foreign_load_tiff_buffer_class_init
public class VipsForeignLoadTiffBufferClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Other libraries may be using libtiff, we want to capture tiff
        // warning and error as soon as we can.
        //
        // This class init will be triggered during startup.
        Vips.TiffInit();
    }
}

// Converted from: vips_foreign_load_tiff_buffer_init
public class VipsForeignLoadTiffBuffer : VipsObject
{
    public VipsBlob Blob { get; set; }

    public VipsForeignLoadTiffBuffer()
    {
        base.Init();
    }
}

// Converted from: vips_tiffload
public static class VipsTiffload
{
    public static int Tiffload(string filename, out VipsImage image, params object[] args)
    {
        return Vips.CallSplit("tiffload", filename, out image, args);
    }
}

// Converted from: vips_tiffload_buffer
public static class VipsTiffloadBuffer
{
    public static int TiffloadBuffer(byte[] buf, int len, out VipsImage image, params object[] args)
    {
        var blob = new VipsBlob(buf, len);
        return Vips.CallSplit("tiffload_buffer", blob, out image, args);
    }
}

// Converted from: vips_tiffload_source
public static class VipsTiffloadSource
{
    public static int TiffloadSource(VipsSource source, out VipsImage image, params object[] args)
    {
        return Vips.CallSplit("tiffload_source", source, out image, args);
    }
}
```