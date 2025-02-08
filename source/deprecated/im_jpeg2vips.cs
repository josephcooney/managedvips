Here is the converted C# code:

```csharp
// Convert 1 or 3-band 8-bit VIPS images to/from JPEG.
//
// 30/11/11
//	- now just a stub
// 10/7/12
//	- use jpeg funcs directly rather than going though vips_jpegload()

using System;
using System.IO;

public class Jpeg2VipsConverter
{
    // By default, we ignore any warnings. We want to get as much of the user's data as we can.
    private static bool FailOnWarn = false;

    public static int Jpeg2Vips(string name, VipsImage outImage, bool headerOnly)
    {
        string filename = Path.GetFileName(name);
        string mode = Path.GetExtension(name);

        // Parse the filename.
        var options = new OptionParser();
        if (options.Parse(name))
        {
            var shrink = int.Parse(options["shrink"]);
            if (shrink != 1 && shrink != 2 &&
                shrink != 4 && shrink != 8)
            {
                throw new ArgumentException("bad shrink factor " + shrink);
            }

            FailOnWarn = options.ContainsKey("fail");
            bool sequential = options.ContainsKey("seq");

            // Don't use vips_jpegload() ... we call the jpeg func directly in order to avoid the foreign.c mechanisms for load-via-disc and stuff like that.

            // We need to be compatible with the pre-sequential mode im_jpeg2vips(). This returned a "t" if given a "p" image, since it used writeline.
            //
            // If we're writing the image to a "p", switch it to a "t".
            if (!headerOnly &&
                !sequential &&
                outImage.Dtype == VipsImageType.Partial)
            {
                if (outImage.WioOutput())
                    return -1;
            }
        }

#ifdef HAVE_JPEG
        using (var source = new VipsSource(filename))
        {
            if (VipsJpegReadSource(source, outImage, headerOnly, int.Parse(options["shrink"]), FailOnWarn, false, false))
                return -1;
        }
#else
        throw new ArgumentException("no JPEG support in your libvips");
#endif

        return 0;
    }

    public static int ImJpeg2Vips(string name, VipsImage outImage)
    {
        return Jpeg2Vips(name, outImage, false);
    }

    // By having a separate header func, we get lazy.c to open via disc/mem.
    public static int ImJpeg2VipsHeader(string name, VipsImage outImage)
    {
        return Jpeg2Vips(name, outImage, true);
    }

    public static int ImBufjpeg2vips(byte[] buf, long len, VipsImage outImage, bool headerOnly)
    {
        using (var t = new VipsImage())
        {
            if (VipsJpegLoadBuffer(buf, len, ref t))
                return -1;
            if (t.Write(outImage))
            {
                t.Dispose();
                return -1;
            }
            t.Dispose();

            return 0;
        }
    }

    public static bool Isjpeg(string name)
    {
        var filename = Path.GetFileName(name);
        var mode = Path.GetExtension(name);

        return VipsForeignIsA("jpegload", filename);
    }

    private static readonly string[] JpegSuffs = { ".jpg", ".jpeg", ".jpe" };

    // jpeg format adds no new members.
    public class FormatJpeg : VipsFormat
    {
        protected override void ClassInit(VipsObjectClass objectClass, VipsFormatClass formatClass)
        {
            base.ClassInit(objectClass, formatClass);

            objectClass.Nickname = "jpeg";
            objectClass.Description = "JPEG";

            formatClass.IsA = Isjpeg;
            formatClass.Header = ImJpeg2VipsHeader;
            formatClass.Load = ImJpeg2Vips;
            formatClass.Save = ImVips2jpeg;
            formatClass.Suffs = JpegSuffs;
        }

        protected override void Init(VipsFormatJpeg obj)
        {
            base.Init(obj);
        }
    }

    public static void Main()
    {
        // G_DEFINE_TYPE macro is not needed in C#
    }
}
```

Note that I've assumed the existence of `VipsImage`, `VipsSource`, and other VIPS-related classes, as well as the `OptionParser` class for parsing command-line options. You may need to modify the code to match your specific implementation.

Also, I've used C# 7.x features such as `using` statements with disposable objects, `ref` parameters, and pattern matching. If you're using an earlier version of C#, you may need to adjust the code accordingly.