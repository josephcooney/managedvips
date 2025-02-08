```csharp
// vips_foreign_load_jpeg.c: load jpeg from a file

public class VipsForeignLoadJpeg : VipsForeignLoad
{
    public VipsSource Source { get; set; }
    public bool Unlimited { get; set; }
    public int Shrink { get; set; }
    public bool Autorotate { get; set; }

    protected override void Dispose(GObject gobject)
    {
        if (Source != null)
            VIPS_UNREF(Source);
        base.Dispose(gobject);
    }

    protected override int Build(VipsObject object)
    {
        var jpeg = (VipsForeignLoadJpeg)object;
        if (jpeg.Shrink != 1 && jpeg.Shrink != 2 && jpeg.Shrink != 4 && jpeg.Shrink != 8)
        {
            vips_error("VipsFormatLoadJpeg", _("bad shrink factor %d"), jpeg.Shrink);
            return -1;
        }
        if (base.Build(object) == -1)
            return -1;
        return 0;
    }

    protected override VipsForeignFlags GetFlags(VipsForeignLoad load)
    {
        return VIPS_FOREIGN_SEQUENTIAL;
    }

    protected override VipsForeignFlags GetFlagsFilename(string filename)
    {
        return VIPS_FOREIGN_SEQUENTIAL;
    }

    protected override int Header(VipsForeignLoad load)
    {
        var jpeg = (VipsForeignLoadJpeg)load;
        if (vips__jpeg_read_source(jpeg.Source, load.Out, true, jpeg.Shrink, load.FailOn, jpeg.Autorotate, jpeg.Unlimited))
            return -1;
        return 0;
    }

    protected override int Load(VipsForeignLoad load)
    {
        var jpeg = (VipsForeignLoadJpeg)load;
        if (vips__jpeg_read_source(jpeg.Source, load.Real, false, jpeg.Shrink, load.FailOn, jpeg.Autorotate, jpeg.Unlimited))
            return -1;
        return 0;
    }
}

// vips_foreign_load_jpeg_source.c: load jpeg from a source

public class VipsForeignLoadJpegSource : VipsForeignLoadJpeg
{
    public VipsSource Source { get; set; }

    protected override int Build(VipsObject object)
    {
        var jpeg = (VipsForeignLoadJpeg)object;
        if (Source != null)
            jpeg.Source = Source;
        return base.Build(object);
    }

    protected override bool IsASource(VipsSource source)
    {
        return vips__isjpeg_source(source);
    }
}

// vips_foreign_load_jpeg_file.c: load jpeg from a file

public class VipsForeignLoadJpegFile : VipsForeignLoadJpeg
{
    public string Filename { get; set; }

    protected override int Build(VipsObject object)
    {
        var file = (VipsForeignLoadJpegFile)object;
        if (file.Filename != null && !(file.Source = vips_source_new_from_file(file.Filename)))
            return -1;
        return base.Build(object);
    }

    protected override bool IsA(string filename)
    {
        VipsSource source;
        bool result;
        if (!(source = vips_source_new_from_file(filename)))
            return false;
        result = vips_foreign_load_jpeg_source_is_a_source(source);
        VIPS_UNREF(source);
        return result;
    }
}

// vips_foreign_load_jpeg_buffer.c: load jpeg from a buffer

public class VipsForeignLoadJpegBuffer : VipsForeignLoadJpeg
{
    public VipsBlob Blob { get; set; }

    protected override int Build(VipsObject object)
    {
        var buffer = (VipsForeignLoadJpegBuffer)object;
        if (buffer.Blob != null && !(buffer.Source = vips_source_new_from_memory(VIPS_AREA(buffer.Blob).Data, VIPS_AREA(buffer.Blob).Length)))
            return -1;
        return base.Build(object);
    }

    protected override bool IsABuffer(byte[] buf, size_t len)
    {
        VipsSource source;
        bool result;
        if (!(source = vips_source_new_from_memory(buf, len)))
            return false;
        result = vips_foreign_load_jpeg_source_is_a_source(source);
        VIPS_UNREF(source);
        return result;
    }
}

// vips_jpegload.c: load jpeg from a file

public class VipsJpegLoad
{
    public static int JpegLoad(string filename, out VipsImage image, params object[] args)
    {
        var va = new VaList(args);
        return VipsCallSplit("jpegload", va, filename, out image);
    }
}

// vips_jpegload_buffer.c: load jpeg from a buffer

public class VipsJpegLoadBuffer
{
    public static int JpegLoadBuffer(byte[] buf, size_t len, out VipsImage image, params object[] args)
    {
        var blob = new VipsBlob(null, buf, len);
        var va = new VaList(args);
        return VipsCallSplit("jpegload_buffer", va, blob, out image);
    }
}

// vips_jpegload_source.c: load jpeg from a source

public class VipsJpegLoadSource
{
    public static int JpegLoadSource(VipsSource source, out VipsImage image, params object[] args)
    {
        var va = new VaList(args);
        return VipsCallSplit("jpegload_source", va, source, out image);
    }
}
```