Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class ImageConverter
{
    // im_vips2png
    public static int ImVips2Png(IMAGE inImage, string filename)
    {
        int compression = 6;
        int interlace = 0;

        char[] name = new char[FILENAME_MAX];
        char[] mode = new char[FILENAME_MAX];
        char[] buf = new char[FILENAME_MAX];

        // Extract write mode from filename and parse.
        Vips.ImageFilenameSplit(filename, name, mode);
        Array.Copy(mode, buf, mode.Length);
        string p = new string(buf);
        compression = 6;
        interlace = 0;

        if (Vips.ImageGetNextOption(ref p) != null)
            compression = int.Parse(Vips.ImageGetNextOption(ref p));
        if (Vips.ImageGetNextOption(ref p) != null)
            interlace = int.Parse(Vips.ImageGetNextOption(ref p));

        return Vips.PngSave(inImage, name,
            "compression", compression, "interlace", interlace, null);
    }

    // im_vips2bufpng
    public static int ImVips2BufPng(IMAGE inImage, IMAGE outImage,
        int compression, int interlace, ref byte[] obuf, ref long olen)
    {
        if (Vips.PngSaveBuffer(inImage, ref obuf, ref olen,
            "compression", compression,
            "interlace", interlace,
            null))
            return -1;

        if (outImage != null)
            Vips.ImageAddCallback(outImage, "close",
                (im_callback_fn)Vips.Free, obuf, null);

        return 0;
    }
}
```

Note that I've assumed `FILENAME_MAX` is a constant defined elsewhere in the codebase. If not, you'll need to define it or replace it with a suitable value.

Also note that this conversion uses the VipsDotNet library for interacting with VIPS images and functions. This library provides a C# wrapper around the original C API.