Here is the converted C# code:

```csharp
// vips__tiff_init
void VipsTiffInit()
{
    TIFF.SetErrorHandler(new ErrorHandler(VipsErrorHandler));
    TIFF.SetWarningHandler(new WarningHandler(VipsWarningHandler));
}

// openin_source_read
static long OpenInSourceRead(IntPtr handle, IntPtr data, long size)
{
    VipsSource source = (VipsSource)handle;
    return VipsSource.Read(source, data, size);
}

// openin_source_write
static long OpenInSourceWrite(IntPtr handle, IntPtr buffer, long size)
{
    g_assert_not_reached();
    return 0;
}

// openin_source_seek
static long OpenInSourceSeek(IntPtr handle, long offset, int whence)
{
    VipsSource source = (VipsSource)handle;
    return (long)VipsSource.Seek(source, offset, whence);
}

// openin_source_close
static int OpenInSourceClose(IntPtr handle)
{
    VipsSource source = (VipsSource)handle;
    VIPS.Unref(source);
    return 0;
}

// openin_source_length
static long OpenInSourceLength(IntPtr handle)
{
    VipsSource source = (VipsSource)handle;
    // libtiff will use this to get file size if tags like StripByteCounts are missing.
    return (long)VipsSource.Length(source);
}

// openin_source_map
static int OpenInSourceMap(IntPtr handle, IntPtr start, ref long len)
{
    g_assert_not_reached();
    return 0;
}

// openin_source_unmap
static void OpenInSourceUnmap(IntPtr handle, IntPtr start, long len)
{
    g_assert_not_reached();
}

// vips__tiff_openin_source
public static TIFF VipsTiffOpenInSource(VipsSource source, VipsTiffErrorHandler errorFn, VipsTiffErrorHandler warningFn, object userData, bool unlimited)
{
    TIFF tiff = null;

#ifdef DEBUG
    Console.WriteLine("vips__tiff_openin_source:");
#endif

    if (VipsSource.Rewind(source))
        return null;

    // Disable memory mapped input -- it chews up VM and the performance gain is very small.
    // C enables strip chopping: very large uncompressed strips are chopped into c. 8kb chunks. This can reduce peak memory use for this type of file.

#ifdef HAVE_TIFF_OPEN_OPTIONS
    TIFFOpenOptions opts = new TIFFOpenOptions();
    TIFFOpenOptions.SetErrorHandlerExtR(opts, errorFn, userData);
    TIFFOpenOptions.SetWarningHandlerExtR(opts, warningFn, userData);

    if (!unlimited)
    {
        TIFFOpenOptions.SetMaxCumulatedMemAlloc(opts, 20 * 1024 * 1024);
    }

    tiff = TIFF.ClientOpenExt("source input", "rmC",
        (IntPtr)source,
        OpenInSourceRead,
        OpenInSourceWrite,
        OpenInSourceSeek,
        OpenInSourceClose,
        OpenInSourceLength,
        OpenInSourceMap,
        OpenInSourceUnmap,
        opts);

    TIFFOpenOptions.Free(opts);
#else
    tiff = TIFF.ClientOpen("source input", "rmC",
        (IntPtr)source,
        OpenInSourceRead,
        OpenInSourceWrite,
        OpenInSourceSeek,
        OpenInSourceClose,
        OpenInSourceLength,
        OpenInSourceMap,
        OpenInSourceUnmap);
#endif

    if (tiff == null)
    {
        VipsError("vips__tiff_openin_source", "%s",
            _("unable to open source for input"));
        return null;
    }

    // Unreffed on close(), see above.
    G.ObjectRef(source);

    return tiff;
}

// openout_target_read
static long OpenOutTargetRead(IntPtr handle, IntPtr data, long size)
{
    VipsTarget target = (VipsTarget)handle;

    return VipsTarget.Read(target, data, size);
}

// openout_target_write
static long OpenOutTargetWrite(IntPtr handle, IntPtr data, long size)
{
    VipsTarget target = (VipsTarget)handle;

    if (VipsTarget.Write(target, data, size))
        return -1;

    return size;
}

// openout_target_seek
static long OpenOutTargetSeek(IntPtr handle, long offset, int whence)
{
    VipsTarget target = (VipsTarget)handle;

    return VipsTarget.Seek(target, offset, whence);
}

// openout_target_close
static int OpenOutTargetClose(IntPtr handle)
{
    VipsTarget target = (VipsTarget)handle;

    if (VipsTarget.End(target))
        return -1;

    return 0;
}

// openout_target_length
static long OpenOutTargetLength(IntPtr handle)
{
    g_assert_not_reached();

    return -1;
}

// openout_target_map
static int OpenOutTargetMap(IntPtr handle, IntPtr start, ref long len)
{
    g_assert_not_reached();

    return -1;
}

// openout_target_unmap
static void OpenOutTargetUnmap(IntPtr handle, IntPtr start, long len)
{
    g_assert_not_reached();
}

// vips__tiff_openout_target
public static TIFF VipsTiffOpenOutTarget(VipsTarget target, bool bigtiff, VipsTiffErrorHandler errorFn, VipsTiffErrorHandler warningFn, object userData)
{
    string mode = bigtiff ? "w8" : "w";

    TIFF tiff = null;

#ifdef DEBUG
    Console.WriteLine("vips__tiff_openout_buffer:");
#endif

#ifdef HAVE_TIFF_OPEN_OPTIONS
    TIFFOpenOptions opts = new TIFFOpenOptions();
    TIFFOpenOptions.SetErrorHandlerExtR(opts, errorFn, userData);
    TIFFOpenOptions.SetWarningHandlerExtR(opts, warningFn, userData);

    tiff = TIFF.ClientOpenExt("target output", mode,
        (IntPtr)target,
        OpenOutTargetRead,
        OpenOutTargetWrite,
        OpenOutTargetSeek,
        OpenOutTargetClose,
        OpenOutTargetLength,
        OpenOutTargetMap,
        OpenOutTargetUnmap,
        opts);

    TIFFOpenOptions.Free(opts);
#else
    tiff = TIFF.ClientOpen("target output", mode,
        (IntPtr)target,
        OpenOutTargetRead,
        OpenOutTargetWrite,
        OpenOutTargetSeek,
        OpenOutTargetClose,
        OpenOutTargetLength,
        OpenOutTargetMap,
        OpenOutTargetUnmap);
#endif

    if (tiff == null)
    {
        VipsError("vips__tiff_openout_target", "%s",
            _("unable to open target for output"));
        return null;
    }

    return tiff;
}
```

Note that I've assumed the existence of `VipsSource`, `VipsTarget`, `VipsTiffErrorHandler`, and `G.ObjectRef` classes, as well as the `VipsError` function. You may need to modify the code to match your actual implementation.