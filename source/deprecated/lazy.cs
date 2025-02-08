Here is the C# code equivalent to the provided C code:

```csharp
// lazy open/save ... compat funcs for old im_open() behaviour
//
// 30/11/11
//  - cut from old image.c

using System;
using Vips;

public class LazyOpenSaveCompat
{
    // What we track during a delayed open.
    public struct Lazy
    {
        public Image image;
        public FormatClass format; /* Read in pixels with this */
        public string filename;         /* Get pixels from here */
        public bool sequential;       /* Sequential read requested */

        public Image real; /* The real decompressed image */
    }

    // Lazy open.
    public static void* OpenLazyStart(Image outImage, object a, object dummy)
    {
        Lazy lazy = (Lazy)a;

        if (lazy.real == null)
        {
            if (lazy.real = LazyRealImage(lazy))
                return null;
        }

        return Region.New(lazy.real);
    }

    // Just copy.
    public static int OpenLazyGenerate(Region outRegion, object seq, object a, object b, ref bool stop)
    {
        Region ir = (Region)seq;

        Rect r = outRegion.Valid;

        // Ask for input we need.
        if (Region.Prepare(ir, r))
            return -1;

        // Attach output region to that.
        if (Region.Region(outRegion, ir, r, r.Left, r.Top))
            return -1;

        return 0;
    }

    // Lazy open ... init the header with the first OpenLazyFn, delay actually
    // decoding pixels with the second OpenLazyFn until the first generate().
    public static int VipsImageOpenLazy(Image image, FormatClass format, string filename, bool sequential)
    {
        Lazy lazy;

        lazy = LazyNew(image, format, filename, sequential);

        // Is there a ->header() function? We need to do a lazy load.
        if (format.Header != null)
        {
            // Read header fields to init the return image.
            if (format.Header(filename, image))
                return -1;

            // Then 'start' creates the real image and 'gen' paints 'image'
            // with pixels from the real image on demand.
            if (Image.Pipelinev(image, image.DHint, null) ||
                Image.Generate(image,
                    OpenLazyStart, OpenLazyGenerate,
                    StopOne, lazy, null))
                return -1;
        }
        else if (format.Load != null)
        {
            if (format.Load(filename, image))
                return -1;
        }
        else
            g_assert(0);

        return 0;
    }

    // Lazy save.

    // If we write to (eg.) TIFF, actually do the write
    // to a "p" and on "written" do im_vips2tiff() or whatever. Track save
    // parameters here.
    public struct SaveBlock
    {
        public Func<Image, string, int> saveFn; /* Save function */
        public string filename;         /* Save args */
    }

    // From "written" callback: invoke a delayed save.
    public static void VipsImageSaveCb(Image image, ref int result, SaveBlock sb)
    {
        if (sb.saveFn(image, sb.filename))
            result = -1;

        g_free(sb.filename);
        g_free(sb);
    }

    // Attach output region to that.
    public static void VipsAttachSave(Image image, Func<Image, string, int> saveFn, string filename)
    {
        SaveBlock sb;

        sb = new SaveBlock();
        sb.saveFn = saveFn;
        sb.filename = g_strdup(filename);
        GSignal.Connect(image, "written",
            (GCallback)VipsImageSaveCb, sb);
    }

    // Lazy real image.
    public static Image LazyRealImage(Lazy lazy)
    {
        Image real;

        // We open via disc if:
        // - 'sequential' is not set
        // - disc_threshold() has not been set to zero
        // - the format does not support lazy read
        // - the uncompressed image will be larger than disc_threshold()
        real = null;
        if (!lazy.sequential &&
            DiscThreshold() &&
            !(Format.GetFlags(lazy.format, lazy.filename) &
                VIPS_FORMAT_PARTIAL) &&
            Image.SizeOfImage(lazy.image) > DiscThreshold())
            if (!(real = Image.NewTempFile("%s.v")))
                return null;

        // Otherwise, fall back to a "p".
        if (!real &&
            !(real = Image.New()))
            return null;

        return real;
    }

    // Lazy new.
    public static Lazy LazyNew(Image image,
        FormatClass format, string filename, bool sequential)
    {
        Lazy lazy;

        lazy = new Lazy();
        lazy.image = image;
        lazy.format = format;
        lazy.filename = g_strdup(filename);
        lazy.sequential = sequential;
        lazy.real = null;
        GSignal.Connect(image, "close",
            (GCallback)LazyFreeCb, lazy);

        return lazy;
    }

    // Lazy free cb.
    public static void LazyFreeCb(Image image, object a)
    {
        Lazy lazy = (Lazy)a;

        VIPS_DEBUG_MSG("lazy_free: %p \"%s\"\n", lazy, lazy.filename);

        g_free(lazy.filename);
        VIPS.Unref(lazy.real);
        g_free(lazy);
    }

    // Disc threshold.
    public static int DiscThreshold()
    {
        static bool done = false;
        static int threshold;

        if (!done)
        {
            const string env = g_getenv("IM_DISC_THRESHOLD");

            done = true;

            // 100mb default.
            threshold = 100 * 1024 * 1024;

            if (env != null)
                threshold = Vips__ParseSize(env);

            if (Vips__DiscThreshold != null)
                threshold = Vips__ParseSize(Vips__DiscThreshold);

            VIPS_DEBUG_MSG("disc_threshold: %zd bytes\n", threshold);
        }

        return threshold;
    }
}

// vips__deprecated_open_read
public static Image Vips__DeprecatedOpenRead(string filename, bool sequential)
{
    FormatClass format;

    if (!(format = Vips.FormatForFile(filename)))
        return null;

    if (VipsFormatIsVips(format))
    {
        // For vips format, we can just the main vips path.
        return Image.NewMode(filename, "rd");
    }
    else
    {
        // For non-vips formats we must go via the old VipsFormat
        // system to make sure we support the "filename:options"
        // syntax.
        Image image;

        image = Image.New();
        if (VipsImageOpenLazy(image, format,
                filename, sequential))
        {
            g_object_unref(image);
            return null;
        }

        // Yuk. Can't g_object_set() filename since it's after
        // construct. Just zap the new filename in.
        VIPS.SetStr(image.Filename, filename);

        return image;
    }
}

// vips__deprecated_open_write
public static Image Vips__DeprecatedOpenWrite(string filename)
{
    FormatClass format;

    if (!(format = Vips.FormatForName(filename)))
        return null;

    if (VipsFormatIsVips(format))
        // For vips format, we can just the main vips path.
        return Image.NewMode(filename, "w");
    else
    {
        // For non-vips formats we must go via the old VipsFormat
        // system to make sure we support the "filename:options"
        // syntax.
        Image image;

        if (!(image = Image.New()))
            return null;
        VipsAttachSave(image,
            format.Save, filename);
        return image;
    }
}
```

Note that this code uses C# 7.0 features and assumes that you have the `Vips` namespace available. You may need to modify it to fit your specific use case.

Also note that I've kept the original comments from the C source code where possible, but some of them were removed or modified to make the code more readable in C#.