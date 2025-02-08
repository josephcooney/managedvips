Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsImageMagickSave
{
    // vips_magicksave: (method)
    // @in: image to save
    // @filename: file to write to
    // @...: %NULL-terminated list of optional named arguments

    public static int MagickSave(VipsImage inImage, string filename, params object[] args)
    {
        return VipsCallSplit("magicksave", inImage, filename, args);
    }

    // vips_magicksave_buffer: (method)
    // @in: image to save
    // @buf: (array length=len) (element-type guint8): return output buffer here
    // @len: (type gsize): return output length here
    // @...: %NULL-terminated list of optional named arguments

    public static int MagickSaveBuffer(VipsImage inImage, out byte[] buf, out long len, params object[] args)
    {
        VipsArea area = null;
        int result = VipsCallSplit("magicksave_buffer", inImage, ref area);

        if (!result && area != null)
        {
            if (buf != null)
                buf = new byte[area.Length];
            if (len != 0)
                len = area.Length;

            vips_area_unref(area);
        }

        return result;
    }
}

public class VipsCallSplit
{
    public static int CallSplit(string method, object[] args, params object[] kwargs)
    {
        // implementation of vips_call_split function
        // ...
        return 0; // or -1 on error
    }
}
```