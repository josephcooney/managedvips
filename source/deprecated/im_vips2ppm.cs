Here is the C# equivalent of the provided C code:

```csharp
using System;
using VipsDotNet;

public class PpmWriter
{
    // im_vips2ppm
    public static int WritePpmFile(IMAGE inImage, string filename)
    {
        bool ascii = false; // Default to binary output ... much smaller.
        char[] name = new char[FILENAME_MAX];
        char[] mode = new char[FILENAME_MAX];

        // Extract write mode from filename.
        im_filename_split(filename, name, mode);
        if (string.Compare(mode, "") != 0)
        {
            if (im_isprefix("binary", mode))
                ascii = false;
            else if (im_isprefix("ascii", mode))
                ascii = true;
            else
            {
                throw new ArgumentException($"bad mode string, should be \"binary\" or \"ascii\"");
            }
        }

        return vips_ppmsave(inImage, name, "ascii", ascii, null);
    }
}
```

Note that I've assumed `FILENAME_MAX` is a constant defined elsewhere in the codebase. If not, you'll need to define it or replace it with a suitable value.

Also note that `im_filename_split`, `im_isprefix`, and `vips_ppmsave` are assumed to be part of the VIPS library and have been converted separately. 

This C# method does essentially the same thing as the original C code, but with some minor adjustments for C#'s syntax and conventions.