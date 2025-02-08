Here is the converted C# code:

```csharp
// im_istifftiled(const char *filename)
public static bool ImIstiffTiled(string filename)
{
    VipsSource source;
    bool result;

    if (!(source = Vips.Source.NewFromFilename(filename)))
        return false;
    result = Vips.__IstiffSource(source);
    Vips.Unref(source);

    return result;
}

// im_tiff_read_header(const char *filename, VipsImage *out,
// int page, int n, gboolean autorotate)
public static int ImTiffReadHeader(string filename, VipsImage out,
    int page, int n, bool autorotate)
{
    VipsSource source;

    if (!(source = Vips.Source.NewFromFilename(filename)))
        return -1;
    if (Vips.__TiffReadHeaderSource(source, out,
            page, n, autorotate, -1, Vips.FailOnError, true)) {
        Vips.Unref(source);
        return -1;
    }
    Vips.Unref(source);

    return 0;
}

// im_tiff_read(const char *filename, VipsImage *out,
// int page, int n, gboolean autorotate)
public static int ImTiffRead(string filename, VipsImage out,
    int page, int n, bool autorotate)
{
    VipsSource source;

    if (!(source = Vips.Source.NewFromFilename(filename)))
        return -1;
    if (Vips.__TiffReadSource(source, out,
            page, n, autorotate, -1, Vips.FailOnError, true)) {
        Vips.Unref(source);
        return -1;
    }
    Vips.Unref(source);

    return 0;
}

// tiff2vips(const char *name, IMAGE *out, gboolean header_only)
public static int Tiff2Vips(string name, VipsImage out, bool headerOnly)
{
#ifdef HAVE_TIFF
    char filename = new char[FILENAME_MAX];
    char mode = new char[FILENAME_MAX];
    char* p;
    int page;
    int seq;

    ImFilenameSplit(name, filename, mode);

    page = 0;
    seq = 0;
    p = &mode[0];
    if ((p = ImGetNextOption(&p)) != null) {
        page = int.Parse(p);
    }
    if ((p = ImGetNextOption(&p)) != null) {
        if (ImIsPrefix("seq", p))
            seq = 1;
    }

    // We need to be compatible with the pre-sequential mode
    // im_tiff2vips(). This returned a "t" if given a "p" image, since it
    // used writeline.
    //
    // If we're writing the image to a "p", switch it to a "t". And only
    // for non-tiled (strip) images which we write with writeline.
    //
    // Don't do this for header read, since we don't want to force a
    // malloc if all we are doing is looking at fields.

    if (!headerOnly &&
        !seq &&
        !ImIstiffTiled(filename) &&
        out.Dtype == Vips.ImagePartial) {
        if (Vips.__ImageWioOutput(out))
            return -1;
    }

    if (headerOnly) {
        if (ImTiffReadHeader(filename, out, page, 1, false))
            return -1;
    }
    else {
        if (ImTiffRead(filename, out, page, 1, false))
            return -1;
    }
#else
    Vips.Error("im_tiff2vips",
        "%s", _("no TIFF support in your libvips"));

    return -1;
#endif /*HAVE_TIFF*/

    return 0;
}

// im_tiff2vips(const char *name, IMAGE *out)
public static int ImTiff2Vips(string name, VipsImage out)
{
    return Tiff2Vips(name, out, false);
}

// By having a separate header func, we get lazy.c to open via disc/mem.
static int ImTiff2VipsHeader(string name, VipsImage out)
{
    return Tiff2Vips(name, out, true);
}

// tiff_flags(const char *name)
public static VipsFormatFlags TiffFlags(string name)
{
    char filename = new char[FILENAME_MAX];
    char mode = new char[FILENAME_MAX];

    ImFilenameSplit(name, filename, mode);

    return (VipsFormatFlags)Vips.ForeignFlags("tiffload", filename);
}

// istiff(const char *name)
public static bool Istiff(string name)
{
    char filename = new char[FILENAME_MAX];
    char mode = new char[FILENAME_MAX];

    ImFilenameSplit(name, filename, mode);

    return Vips.ForeignIsA("tiffload", filename);
}

static string[] TiffSuffs = { ".tif", ".tiff", null };

// vips_format_tiff_class_init(VipsFormatTiffClass *class)
public class VipsFormatTiff : Vips.Format
{
    public static void VipsFormatTiffClassInit(Type type)
    {
        Vips.ObjectClass objectClass = (Vips.ObjectClass)type;
        Vips.FormatClass formatClass = (Vips.FormatClass)type;

        objectClass.Nickname = "tiff";
        objectClass.Description = _("TIFF");

        formatClass.IsA = Istiff;
        formatClass.Header = ImTiff2VipsHeader;
        formatClass.Load = ImTiff2Vips;
        formatClass.Save = ImVips2Tiff;
        formatClass.GetFlags = TiffFlags;
        formatClass.Suffs = TiffSuffs;
    }
}

// vips_format_tiff_init(VipsFormatTiff *object)
public VipsFormatTiff()
{
}
```

Note that I've assumed the existence of certain classes and methods in the `Vips` namespace, as they are not explicitly defined in the provided code. You may need to modify the code to match your specific implementation.