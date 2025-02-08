Here is the C# code equivalent to the provided C code:

```csharp
// vips_foreign_load_png_dispose
public class VipsForeignLoadPng : VipsForeignLoad
{
    public VipsSource Source { get; private set; }
    public bool Unlimited { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Source != null)
        {
            Source.Dispose();
        }
        base.Dispose(disposing);
    }
}

// vips_foreign_load_png_get_flags_source
public class VipsForeignLoadPngFlags : VipsForeignFlags
{
    public static VipsForeignFlags GetFlags(VipsSource source)
    {
        var flags = new VipsForeignFlags();

        if (Vips.Png.IsInterlaced(source))
        {
            flags |= Vips.ForeignFlags.Partial;
        }
        else
        {
            flags |= Vips.ForeignFlags.Sequential;
        }

        return flags;
    }
}

// vips_foreign_load_png_get_flags
public class VipsForeignLoadPngFlags : VipsForeignFlags
{
    public static VipsForeignFlags GetFlags(VipsForeignLoad load)
    {
        var png = (VipsForeignLoadPng)load;

        return VipsForeignLoadPngFlags.GetFlags(png.Source);
    }
}

// vips_foreign_load_png_get_flags_filename
public class VipsForeignLoadPngFlags : VipsForeignFlags
{
    public static VipsForeignFlags GetFlagsFilename(string filename)
    {
        var source = Vips.Source.NewFromFilename(filename);

        if (source == null) return 0;

        var flags = VipsForeignLoadPngFlags.GetFlags(source);
        Vips.Unref(source);

        return flags;
    }
}

// vips_foreign_load_png_header
public class VipsForeignLoadPng : VipsForeignLoad
{
    public override int Header()
    {
        if (!Vips.Png.Header(Source, Out, Unlimited))
            return -1;

        return 0;
    }
}

// vips_foreign_load_png_load
public class VipsForeignLoadPng : VipsForeignLoad
{
    public override int Load()
    {
        if (!Vips.Png.Read(Source, Real, FailOn, Unlimited))
            return -1;

        return 0;
    }
}

// vips_foreign_load_png_class_init
public static void VipsForeignLoadPngClassInit(Type type)
{
    var gobjectClass = (GObjectClass)type.GetMethod("Dispose");
    gobjectClass.Dispose += new DisposeEventHandler(VipsForeignLoadPng.Dispose);

    var objectClass = (VipsObjectClass)type;
    objectClass.Nickname = "pngload_base";
    objectClass.Description = "load png base class";

    var foreignClass = (VipsForeignClass)type;
    foreignClass.Priority = 200;

    var loadClass = (VipsForeignLoadClass)type;
    loadClass.GetFlagsFilename += new GetFlagsFilenameEventHandler(VipsForeignLoadPngFlags.GetFlagsFilename);
    loadClass.GetFlags += new GetFlagsEventHandler(VipsForeignLoadPngFlags.GetFlags);
    loadClass.Header += new HeaderEventHandler(VipsForeignLoadPng.Header);
    loadClass.Load += new LoadEventHandler(VipsForeignLoadPng.Load);

#ifndef FUZZING_BUILD_MODE_UNSAFE_FOR_PRODUCTION
    Vips.Arg.Bool("unlimited", 23, "Unlimited", "Remove all denial of service limits");
#endif
}

// vips_foreign_load_png_init
public class VipsForeignLoadPng : VipsForeignLoad
{
    public VipsForeignLoadPng()
    {
    }
}

// vips_foreign_load_png_source_build
public class VipsForeignLoadPngSource : VipsForeignLoadPng
{
    public override int Build(VipsObject obj)
    {
        var png = (VipsForeignLoadPng)obj;
        var source = (VipsForeignLoadPngSource)obj;

        if (source.Source != null)
            png.Source = source.Source;

        return base.Build(obj);
    }
}

// vips_foreign_load_png_source_is_a_source
public class VipsForeignLoadPngSource : VipsForeignLoadPng
{
    public static bool IsASource(VipsSource source)
    {
        return Vips.Png.IsPng(source);
    }
}

// vips_foreign_load_png_source_class_init
public static void VipsForeignLoadPngSourceClassInit(Type type)
{
    var gobjectClass = (GObjectClass)type.GetMethod("Dispose");
    gobjectClass.Dispose += new DisposeEventHandler(VipsForeignLoadPngSource.Dispose);

    var objectClass = (VipsObjectClass)type;
    objectClass.Nickname = "pngload_source";
    objectClass.Description = "load png from source";

    var operationClass = (VipsOperationClass)type;
    operationClass.Flags |= Vips.OperationFlags.NoCache;

    var loadClass = (VipsForeignLoadClass)type;
    loadClass.IsASource += new IsASourceEventHandler(VipsForeignLoadPngSource.IsASource);

    Vips.Arg.Object("source", 1, "Source", "Source to load from");
}

// vips_foreign_load_png_source_init
public class VipsForeignLoadPngSource : VipsForeignLoadPng
{
    public VipsForeignLoadPngSource()
    {
    }
}

// vips_foreign_load_png_file_build
public class VipsForeignLoadPngFile : VipsForeignLoadPng
{
    public override int Build(VipsObject obj)
    {
        var png = (VipsForeignLoadPng)obj;
        var file = (VipsForeignLoadPngFile)obj;

        if (file.Filename != null && !(png.Source = Vips.Source.NewFromFilename(file.Filename)))
            return -1;

        return base.Build(obj);
    }
}

// vips_foreign_load_png_file_is_a
public class VipsForeignLoadPngFile : VipsForeignLoadPng
{
    public static bool IsA(string filename)
    {
        var source = Vips.Source.NewFromFilename(filename);

        if (source == null) return false;

        var result = VipsForeignLoadPngSource.IsASource(source);
        Vips.Unref(source);

        return result;
    }
}

// vips_foreign_load_png_file_class_init
public static void VipsForeignLoadPngFileClassInit(Type type)
{
    var gobjectClass = (GObjectClass)type.GetMethod("Dispose");
    gobjectClass.Dispose += new DisposeEventHandler(VipsForeignLoadPngFile.Dispose);

    var objectClass = (VipsObjectClass)type;
    objectClass.Nickname = "pngload";
    objectClass.Description = "load png from file";

    var foreignClass = (VipsForeignClass)type;
    foreignClass.Suffs = Vips.Png.Suffs;

    var loadClass = (VipsForeignLoadClass)type;
    loadClass.IsA += new IsAEventHandler(VipsForeignLoadPngFile.IsA);

    Vips.Arg.String("filename", 1, "Filename", "Filename to load from");
}

// vips_foreign_load_png_file_init
public class VipsForeignLoadPngFile : VipsForeignLoadPng
{
    public VipsForeignLoadPngFile()
    {
    }
}

// vips_foreign_load_png_buffer_build
public class VipsForeignLoadPngBuffer : VipsForeignLoadPng
{
    public override int Build(VipsObject obj)
    {
        var png = (VipsForeignLoadPng)obj;
        var buffer = (VipsForeignLoadPngBuffer)obj;

        if (buffer.Blob != null && !(png.Source = Vips.Source.NewFromMemory(buffer.Blob.Data, buffer.Blob.Length)))
            return -1;

        return base.Build(obj);
    }
}

// vips_foreign_load_png_buffer_is_a_buffer
public class VipsForeignLoadPngBuffer : VipsForeignLoadPng
{
    public static bool IsABuffer(byte[] buf, size_t len)
    {
        var source = Vips.Source.NewFromMemory(buf, len);

        if (source == null) return false;

        var result = VipsForeignLoadPngSource.IsASource(source);
        Vips.Unref(source);

        return result;
    }
}

// vips_foreign_load_png_buffer_class_init
public static void VipsForeignLoadPngBufferClassInit(Type type)
{
    var gobjectClass = (GObjectClass)type.GetMethod("Dispose");
    gobjectClass.Dispose += new DisposeEventHandler(VipsForeignLoadPngBuffer.Dispose);

    var objectClass = (VipsObjectClass)type;
    objectClass.Nickname = "pngload_buffer";
    objectClass.Description = "load png from buffer";

    var loadClass = (VipsForeignLoadClass)type;
    loadClass.IsABuffer += new IsABufferEventHandler(VipsForeignLoadPngBuffer.IsABuffer);

    Vips.Arg.Boxed("buffer", 1, "Buffer", "Buffer to load from");
}

// vips_foreign_load_png_buffer_init
public class VipsForeignLoadPngBuffer : VipsForeignLoadPng
{
    public VipsForeignLoadPngBuffer()
    {
    }
}

// vips_pngload
public static int VipsPngload(string filename, out VipsImage image, params object[] args)
{
    var result = Vips.CallSplit("pngload", filename, out image, args);

    return result;
}

// vips_pngload_buffer
public static int VipsPngloadBuffer(byte[] buf, size_t len, out VipsImage image, params object[] args)
{
    var blob = new VipsBlob(buf, len);
    var result = Vips.CallSplit("pngload_buffer", blob, out image, args);

    Vips.Unref(blob);

    return result;
}

// vips_pngload_source
public static int VipsPngloadSource(VipsSource source, out VipsImage image, params object[] args)
{
    var result = Vips.CallSplit("pngload_source", source, out image, args);

    return result;
}
```