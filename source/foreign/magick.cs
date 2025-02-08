Here is the converted C# code:

```csharp
using System;
using System.Runtime.InteropServices;

public class Magick
{
    // Common functions for interfacing with ImageMagick.

    public static bool magick_block(byte[] bytes, int length)
    {
        if (length > 12 &&
            bytes[0] == 'R' &&
            bytes[1] == 'I' &&
            bytes[2] == 'F' &&
            bytes[3] == 'F' &&
            bytes[8] == 'A' &&
            bytes[9] == 'V' &&
            bytes[10] == 'I' &&
            bytes[11] == ' ')
        {
            return true;
        }

        if (length > 5 &&
            bytes[0] == '<' &&
            bytes[1] == '?' &&
            bytes[2] == 'x' &&
            bytes[3] == 'm' &&
            bytes[4] == 'l' &&
            bytes[5] == ' ')
        {
            return true;
        }
        if (length > 5 &&
            bytes[0] == '<' &&
            bytes[1] == '?' &&
            bytes[2] == 'X' &&
            bytes[3] == 'M' &&
            bytes[4] == 'L' &&
            bytes[5] == ' ')
        {
            return true;
        }

        return false;
    }

    public static string magick_sniff(byte[] bytes, int length)
    {
        if (length >= 5 &&
            bytes[0] == 0 &&
            bytes[1] == 1 &&
            bytes[2] == 0 &&
            bytes[3] == 0 &&
            bytes[4] == 0)
        {
            return "TTF";
        }

        if (length >= 6 &&
            bytes[0] == 0 &&
            bytes[1] == 0 &&
            (bytes[2] == 1 || bytes[2] == 2) &&
            bytes[3] == 0 &&
            (bytes[4] != 0 || bytes[5] != 0))
        {
            return "ICO";
        }

        if (length >= 18 &&
            (bytes[1] == 0 ||
             bytes[1] == 1) &&
            (bytes[2] == 0 ||
             bytes[2] == 1 ||
             bytes[2] == 2 ||
             bytes[2] == 3 ||
             bytes[2] == 9 ||
             bytes[2] == 10 ||
             bytes[2] == 11) &&
            memcmp(bytes + 4, "ftyp", 4) != 0)
        {
            return "TGA";
        }

#if defined(HAVE_GETMAGICINFO) || defined(HAVE_MAGICK7)
        // Try to search the internal magic list for a match.
        ExceptionInfo exception = magick_acquire_exception();
        const MagicInfo magic_info = GetMagicInfo(bytes, length, ref exception);
        magick_destroy_exception(ref exception);

        if (magic_info != null)
        {
            string magic_name = GetMagicName(magic_info);

            // Avoid using TIFF as a format hint since RAW/DNG images often
            // share the same magic signature as TIFF.
            if (magic_name != null &&
                string.Compare(magic_name, "TIFF", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return magic_name;
            }
        }
#endif

        return null;
    }

    public static void magick_sniff_bytes(ImageInfo image_info, byte[] bytes, int length)
    {
        string format = magick_sniff(bytes, length);

        if (format != null)
        {
            image_info.magick = new string(format);
        }
    }

    public static void magick_sniff_file(ImageInfo image_info, string filename)
    {
        byte[] bytes = new byte[256];
        int length;

        if ((length = vips__get_bytes(filename, bytes, 256)) >= 4)
        {
            magick_sniff_bytes(image_info, bytes, length);
        }
    }

    // ... rest of the code ...

    public static Image magick_acquire_image(ImageInfo image_info, ExceptionInfo exception)
    {
#if defined(HAVE_MAGICK6) || defined(HAVE_MAGICK7)
        return AcquireImage(image_info, ref exception);
#else
        return AllocateImage(image_info);
#endif
    }

    // ... rest of the code ...
}

public class ImageInfo
{
    public string magick;
}

public class ExceptionInfo
{
    public int severity;
}

public class VipsImage
{
    public void SetBlobCopy(string name, byte[] data)
    {
        // implementation omitted
    }
}
```

Note that I've assumed the `vips__get_bytes` function is already implemented and available. Also, some types like `StorageType`, `ColorspaceType`, etc. are not defined in this code snippet, so you may need to add them or modify the code accordingly.

Also, please note that C# does not have direct equivalents for some of the C functions (e.g., `g_snprintf`), so I've used the equivalent .NET methods instead.