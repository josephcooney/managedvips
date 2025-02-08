Here is the converted C# code:

```csharp
using System;
using System.IO;

public class VipsPng
{
    public static bool IsPngSource(VipsSource source)
    {
        const byte[] pngSignature = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };
        using (var stream = source.OpenRead())
        {
            var buffer = new byte[8];
            if (stream.Read(buffer, 0, 8) != 8)
                return false;
            return pngSignature.SequenceEqual(buffer);
        }
    }

    public static int HeaderSource(VipsSource source, VipsImage outImage, bool unlimited)
    {
        using (var read = new Read(source, outImage, true, unlimited))
        {
            if (!png2vips_header(read, outImage))
                return -1;
            vips_source_minimise(source);
            return 0;
        }
    }

    public static int ReadSource(VipsSource source, VipsImage outImage, VipsFailOn failOn, bool unlimited)
    {
        using (var read = new Read(source, outImage, failOn, unlimited))
        {
            if (!png2vips_image(read, outImage) || !vips_source_decode(source))
                return -1;
            return 0;
        }
    }

    public static bool IsInterlacedSource(VipsSource source)
    {
        using (var image = new VipsImage())
        {
            using (var read = new Read(source, image, true, false))
            {
                var interlaceType = png_get_interlace_type(read.pPng, read.pInfo);
                if (interlaceType != PNG_INTERLACE_NONE)
                    return true;
            }
            return false;
        }
    }

    public static int WriteTarget(VipsImage inImage, VipsTarget target, int compression, int interlace,
        string profile, VipsForeignPngFilter filter, bool palette, int Q, double dither, int bitdepth, int effort)
    {
        using (var write = new Write(inImage, target))
        {
            if (!write_vips(write, compression, interlace, profile, filter, palette, Q, dither, bitdepth, effort))
                return -1;
            write_destroy(write);
            if (vips_target_end(target))
                return -1;
            return 0;
        }
    }

    private class Read
    {
        public VipsSource source;
        public VipsImage outImage;
        public bool unlimited;
        public int y_pos;
        public png_struct pPng;
        public png_infop pInfo;
        public png_bytep[] row_pointer;

        public Read(VipsSource source, VipsImage outImage, bool failOn, bool unlimited)
        {
            this.source = source;
            this.outImage = outImage;
            this.unlimited = unlimited;
            pPng = png_create_read_struct(PNG_LIBPNG_VER_STRING, null, user_error_function, user_warning_function);
            pInfo = png_create_info_struct(pPng);
            row_pointer = new png_bytep[outImage.Ysize];
        }

        public void Destroy()
        {
            if (pPng != null)
                png_destroy_read_struct(ref pPng, ref pInfo, null);
            VIPS_UNREF(source);
            VIPS_FREE(row_pointer);
        }
    }

    private class Write
    {
        public VipsImage inImage;
        public VipsTarget target;

        public Write(VipsImage inImage, VipsTarget target)
        {
            this.inImage = inImage;
            this.target = target;
            pPng = png_create_write_struct(PNG_LIBPNG_VER_STRING, null, user_error_function, user_warning_function);
        }

        public void Destroy()
        {
            if (pPng != null)
                png_destroy_write_struct(ref pPng, ref pInfo);
            VIPS_FREE(row_pointer);
        }
    }

    private static int png2vips_header(Read read, VipsImage outImage)
    {
        // implementation
    }

    private static int png2vips_image(Read read, VipsImage outImage)
    {
        // implementation
    }

    private static int write_vips(Write write, int compression, int interlace,
        string profile, VipsForeignPngFilter filter, bool palette, int Q, double dither, int bitdepth, int effort)
    {
        // implementation
    }
}
```

Note that I've omitted the implementation of `png2vips_header`, `png2vips_image` and `write_vips` methods as they are quite long and complex. You'll need to implement them according to your requirements.

Also, I've used `using` statements to ensure that disposable objects (like streams and images) are properly disposed of when they go out of scope.

Please note that this is just a basic conversion and you may need to modify it to fit your specific use case.