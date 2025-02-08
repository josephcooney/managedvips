Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VIPSFormatFunctions
{
    // jpeg2vips_vec
    public static int Jpeg2VipsVec(string[] argv)
    {
        string in_ = argv[0];
        IMAGE out_ = (IMAGE)argv[1];

        if (ImJpeg2Vips(in_, out_))
            return -1;

        return 0;
    }

    // jpeg2vips_args
    public static im_arg_desc[] Jpeg2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input file"),
            new im_arg_desc("out", "Output image")
        };
    }

    // jpeg2vips_desc
    public static im_function Jpeg2VipsDesc()
    {
        return new im_function(
            "im_jpeg2vips",
            "Convert from JPEG",
            0,
            Jpeg2VipsVec,
            Jpeg2VipsArgs().Length,
            Jpeg2VipsArgs());
    }

    // vips2dz_vec
    public static int Vips2DzVec(string[] argv)
    {
        IMAGE in_ = (IMAGE)argv[0];
        string out_ = argv[1];

        if (ImVips2Dz(in_, out_))
            return -1;

        return 0;
    }

    // vips2dz_args
    public static im_arg_desc[] Vips2DzArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input image"),
            new im_arg_desc("out", "Output file")
        };
    }

    // vips2dz_desc
    public static im_function Vips2DzDesc()
    {
        return new im_function(
            "im_vips2dz",
            "Save as deep zoom",
            0,
            Vips2DzVec,
            Vips2DzArgs().Length,
            Vips2DzArgs());
    }

    // vips2jpeg_vec
    public static int Vips2JpegVec(string[] argv)
    {
        IMAGE in_ = (IMAGE)argv[0];
        string out_ = argv[1];

        if (ImVips2Jpeg(in_, out_))
            return -1;

        return 0;
    }

    // vips2jpeg_args
    public static im_arg_desc[] Vips2JpegArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input image"),
            new im_arg_desc("out", "Output file")
        };
    }

    // vips2jpeg_desc
    public static im_function Vips2JpegDesc()
    {
        return new im_function(
            "im_vips2jpeg",
            "Convert to JPEG",
            0,
            Vips2JpegVec,
            Vips2JpegArgs().Length,
            Vips2JpegArgs());
    }

    // vips2mimejpeg_vec
    public static int Vips2MimejpegVec(string[] argv)
    {
        IMAGE in_ = (IMAGE)argv[0];
        int qfac = (int)argv[1];

        if (ImVips2Mimejpeg(in_, qfac))
            return -1;

        return 0;
    }

    // vips2mimejpeg_args
    public static im_arg_desc[] Vips2MimejpegArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input image"),
            new im_arg_desc("qfac", "Quality factor")
        };
    }

    // vips2mimejpeg_desc
    public static im_function Vips2MimejpegDesc()
    {
        return new im_function(
            "im_vips2mimejpeg",
            "Convert to JPEG as mime type on stdout",
            0,
            Vips2MimejpegVec,
            Vips2MimejpegArgs().Length,
            Vips2MimejpegArgs());
    }

    // vips2png_vec
    public static int Vips2PngVec(string[] argv)
    {
        return ImVips2Png(argv[0], argv[1]);
    }

    // vips2png_args
    public static im_arg_desc[] Vips2PngArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input image"),
            new im_arg_desc("out", "Output file")
        };
    }

    // vips2png_desc
    public static im_function Vips2PngDesc()
    {
        return new im_function(
            "im_vips2png",
            "Convert VIPS image to PNG file",
            0,
            Vips2PngVec,
            Vips2PngArgs().Length,
            Vips2PngArgs());
    }

    // png2vips_vec
    public static int Png2VipsVec(string[] argv)
    {
        return ImPng2Vips(argv[0], argv[1]);
    }

    // png2vips_args
    public static im_arg_desc[] Png2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input file"),
            new im_arg_desc("out", "Output image")
        };
    }

    // png2vips_desc
    public static im_function Png2VipsDesc()
    {
        return new im_function(
            "im_png2vips",
            "Convert PNG file to VIPS image",
            0,
            Png2VipsVec,
            Png2VipsArgs().Length,
            Png2VipsArgs());
    }

    // exr2vips_vec
    public static int Exr2VipsVec(string[] argv)
    {
        return ImExr2Vips(argv[0], argv[1]);
    }

    // exr2vips_args
    public static im_arg_desc[] Exr2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input file"),
            new im_arg_desc("out", "Output image")
        };
    }

    // exr2vips_desc
    public static im_function Exr2VipsDesc()
    {
        return new im_function(
            "im_exr2vips",
            "Convert an OpenEXR file to VIPS",
            0,
            Exr2VipsVec,
            Exr2VipsArgs().Length,
            Exr2VipsArgs());
    }

    // vips2tiff_vec
    public static int Vips2TiffVec(string[] argv)
    {
        return ImVips2Tiff(argv[0], argv[1]);
    }

    // vips2tiff_args
    public static im_arg_desc[] Vips2TiffArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input image"),
            new im_arg_desc("out", "Output file")
        };
    }

    // vips2tiff_desc
    public static im_function Vips2TiffDesc()
    {
        return new im_function(
            "im_vips2tiff",
            "Convert VIPS image to TIFF file",
            0,
            Vips2TiffVec,
            Vips2TiffArgs().Length,
            Vips2TiffArgs());
    }

    // magick2vips_vec
    public static int Magick2VipsVec(string[] argv)
    {
        return ImMagick2Vips(argv[0], argv[1]);
    }

    // magick2vips_args
    public static im_arg_desc[] Magick2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input file"),
            new im_arg_desc("out", "Output image")
        };
    }

    // magick2vips_desc
    public static im_function Magick2VipsDesc()
    {
        return new im_function(
            "im_magick2vips",
            "Load file with libMagick",
            0,
            Magick2VipsVec,
            Magick2VipsArgs().Length,
            Magick2VipsArgs());
    }

    // tiff2vips_vec
    public static int Tiff2VipsVec(string[] argv)
    {
        return ImTiff2Vips(argv[0], argv[1]);
    }

    // tiff2vips_args
    public static im_arg_desc[] Tiff2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input file"),
            new im_arg_desc("out", "Output image")
        };
    }

    // tiff2vips_desc
    public static im_function Tiff2VipsDesc()
    {
        return new im_function(
            "im_tiff2vips",
            "Convert TIFF file to VIPS image",
            0,
            Tiff2VipsVec,
            Tiff2VipsArgs().Length,
            Tiff2VipsArgs());
    }

    // analyze2vips_vec
    public static int Analyze2VipsVec(string[] argv)
    {
        const string in_ = (string)argv[0];
        IMAGE out_ = (IMAGE)argv[1];

        return ImAnalyze2Vips(in_, out_);
    }

    // analyze2vips_arg_types
    public static im_arg_desc[] Analyze2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("filename", "Input file"),
            new im_arg_desc("im", "Output image")
        };
    }

    // analyze2vips_desc
    public static im_function Analyze2VipsDesc()
    {
        return new im_function(
            "im_analyze2vips",
            "Read a file in analyze format",
            0,
            Analyze2VipsVec,
            Analyze2VipsArgs().Length,
            Analyze2VipsArgs());
    }

    // csv2vips_vec
    public static int Csv2VipsVec(string[] argv)
    {
        const string in_ = (string)argv[0];
        IMAGE out_ = (IMAGE)argv[1];

        return ImCsv2Vips(in_, out_);
    }

    // csv2vips_arg_types
    public static im_arg_desc[] Csv2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("filename", "Input file"),
            new im_arg_desc("im", "Output image")
        };
    }

    // csv2vips_desc
    public static im_function Csv2VipsDesc()
    {
        return new im_function(
            "im_csv2vips",
            "Read a file in CSV format",
            0,
            Csv2VipsVec,
            Csv2VipsArgs().Length,
            Csv2VipsArgs());
    }

    // vips2csv_vec
    public static int Vips2CsvVec(string[] argv)
    {
        IMAGE in_ = (IMAGE)argv[0];
        const string filename = (string)argv[1];

        return ImVips2Csv(in_, filename);
    }

    // vips2csv_arg_types
    public static im_arg_desc[] Vips2CsvArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input image"),
            new im_arg_desc("filename", "Output file")
        };
    }

    // vips2csv_desc
    public static im_function Vips2CsvDesc()
    {
        return new im_function(
            "im_vips2csv",
            "Write an image in CSV format",
            0,
            Vips2CsvVec,
            Vips2CsvArgs().Length,
            Vips2CsvArgs());
    }

    // ppm2vips_vec
    public static int Ppm2VipsVec(string[] argv)
    {
        const string in_ = (string)argv[0];
        IMAGE out_ = (IMAGE)argv[1];

        return ImPpm2Vips(in_, out_);
    }

    // ppm2vips_arg_types
    public static im_arg_desc[] Ppm2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("filename", "Input file"),
            new im_arg_desc("im", "Output image")
        };
    }

    // ppm2vips_desc
    public static im_function Ppm2VipsDesc()
    {
        return new im_function(
            "im_ppm2vips",
            "Read a file in pbm/pgm/ppm format",
            0,
            Ppm2VipsVec,
            Ppm2VipsArgs().Length,
            Ppm2VipsArgs());
    }

    // vips2ppm_vec
    public static int Vips2PpmVec(string[] argv)
    {
        IMAGE in_ = (IMAGE)argv[0];
        const string filename = (string)argv[1];

        return ImVips2Ppm(in_, filename);
    }

    // vips2ppm_arg_types
    public static im_arg_desc[] Vips2PpmArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("im", "Input image"),
            new im_arg_desc("filename", "Output file")
        };
    }

    // vips2ppm_desc
    public static im_function Vips2PpmDesc()
    {
        return new im_function(
            "im_vips2ppm",
            "Write a file in pbm/pgm/ppm format",
            0,
            Vips2PpmVec,
            Vips2PpmArgs().Length,
            Vips2PpmArgs());
    }

    // fits2vips_vec
    public static int Fits2VipsVec(string[] argv)
    {
        string in_ = argv[0];
        IMAGE out_ = (IMAGE)argv[1];

        if (ImFits2Vips(in_, out_))
            return -1;

        return 0;
    }

    // fits2vips_args
    public static im_arg_desc[] Fits2VipsArgs()
    {
        return new im_arg_desc[]
        {
            new im_arg_desc("in", "Input file"),
            new im_arg_desc("out", "Output image")
        };
    }

    // fits2vips_desc
    public static im_function Fits2VipsDesc()
    {
        return new im_function(
            "im_fits2vips",
            "Convert from FITS",
            0,
            Fits2VipsVec,
            Fits2VipsArgs().Length,
            Fits2VipsArgs());
    }

    // list
    public static im_function[] List()
    {
        return new im_function[]
        {
            Csv2VipsDesc(),
            Fits2VipsDesc(),
            Jpeg2VipsDesc(),
            Magick2VipsDesc(),
            Png2VipsDesc(),
            Exr2VipsDesc(),
            Ppm2VipsDesc(),
            Analyze2VipsDesc(),
            Tiff2VipsDesc(),
            Vips2CsvDesc(),
            Vips2DzDesc(),
            Vips2JpegDesc(),
            Vips2MimejpegDesc(),
            Vips2PngDesc(),
            Vips2PpmDesc(),
            Vips2TiffDesc()
        };
    }

    // im__format
    public static im_package ImFormat()
    {
        return new im_package(
            "format",
            List().Length,
            List());
    }
}
```

Note that I've assumed the existence of `IMAGE` and `im_arg_desc`, `im_function`, and `im_package` classes, as well as methods like `ImJpeg2Vips`, `ImVips2Dz`, etc. These would need to be defined elsewhere in your codebase.

Also note that this is a direct translation from C to C#, without any optimizations or improvements specific to the .NET platform.