Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsImage : VipsObject
{
    public enum ImageType { NONE, PARTIAL, MMAPIN, MMAPINRW, OPENOUT, OPENIN, SETBUF, SETBUF_FOREIGN };

    private int Xsize;
    private int Ysize;
    private int Bands;
    private int BandFmt;
    private int Coding;
    private int Type;
    private double Xres;
    private double Yres;
    private int Xoffset;
    private int Yoffset;

    public VipsImage()
    {
        // Initialize image properties
        Xsize = 1;
        Ysize = 1;
        Bands = 1;
        BandFmt = (int)VipsBandFormat.UCHAR;
        Coding = -1; // VIPS_CODING_NONE
        Type = (int)VipsInterpretation.MULTIBAND;
        Xres = 1.0;
        Yres = 1.0;
        Xoffset = 0;
        Yoffset = 0;

        // Initialize image type and demand hint
        DType = ImageType.NONE;
        Dhint = VipsDemandStyle.SMALLTILE;
    }

    public int Width { get { return Xsize; } set { Xsize = value; } }
    public int Height { get { return Ysize; } set { Ysize = value; } }
    public int BandsCount { get { return Bands; } set { Bands = value; } }
    public VipsBandFormat Format { get { return (VipsBandFormat)BandFmt; } set { BandFmt = (int)value; } }
    public VipsCoding CodingValue { get { return (VipsCoding)Coding; } set { Coding = (int)value; } }
    public VipsInterpretation Interpretation { get { return (VipsInterpretation)Type; } set { Type = (int)value; } }
    public double XResolution { get { return Xres; } set { Xres = value; } }
    public double YResolution { get { return Yres; } set { Yres = value; } }
    public int XOffset { get { return Xoffset; } set { Xoffset = value; } }
    public int YOffset { get { return Yoffset; } set { Yoffset = value; } }

    // ... other properties and methods ...

    public static VipsImage New()
    {
        return new VipsImage();
    }

    public static VipsImage NewMode(string filename, string mode)
    {
        var image = new VipsImage();
        image.Filename = filename;
        image.Mode = mode;
        // ... other initialization ...
        return image;
    }

    public static VipsImage NewFromMemory(byte[] data, int size, int width, int height, int bands, VipsBandFormat format)
    {
        var image = new VipsImage();
        image.Filename = "temp";
        image.Mode = "m";
        image.ForeignBuffer = data;
        image.Width = width;
        image.Height = height;
        image.BandsCount = bands;
        image.Format = format;
        // ... other initialization ...
        return image;
    }

    public static VipsImage NewFromMemoryCopy(byte[] data, int size, int width, int height, int bands, VipsBandFormat format)
    {
        var buffer = new byte[size];
        Array.Copy(data, 0, buffer, 0, size);
        var image = NewFromMemory(buffer, size, width, height, bands, format);
        // ... other initialization ...
        return image;
    }

    public static VipsImage NewFromBuffer(byte[] buf, int len, string optionString)
    {
        // ... implementation ...
    }

    public static VipsImage NewFromSource(VipsSource source, string optionString)
    {
        // ... implementation ...
    }

    public static VipsImage NewMatrix(int width, int height)
    {
        var image = new VipsImage();
        image.Filename = "vips_image_new_matrix";
        image.Mode = "t";
        image.Width = width;
        image.Height = height;
        image.BandsCount = 1;
        image.Format = VipsBandFormat.DOUBLE;
        image.Interpretation = VipsInterpretation.MATRIX;
        // ... other initialization ...
        return image;
    }

    public static VipsImage NewFromImage(VipsImage image, double[] c, int n)
    {
        var scope = new VipsObject();
        var t = (VipsImage[])scope.LocalArray(5);
        // ... implementation ...
        return t[4];
    }

    public static VipsImage NewFromImage1(VipsImage image, double c)
    {
        return NewFromImage(image, new double[] { c }, 1);
    }

    public void SetDeleteOnClose(bool deleteOnClose)
    {
        DeleteOnClose = deleteOnClose;
        if (deleteOnClose)
            DeleteOnCloseFilename = Filename;
    }

    public static guint64 GetDiscThreshold()
    {
        // ... implementation ...
    }

    public VipsImage NewTempFile(string format)
    {
        var name = Vips.TempName(format);
        var image = new VipsImage();
        image.Filename = name;
        image.Mode = "w";
        SetDeleteOnClose(true);
        return image;
    }

    public int Write(VipsImage outImage)
    {
        // ... implementation ...
    }

    public int WriteToFile(string filename, string optionString)
    {
        // ... implementation ...
    }

    public int WriteToBuffer(out byte[] buf, out int size)
    {
        // ... implementation ...
    }

    public int Decode(VipsImage outImage)
    {
        // ... implementation ...
    }

    public int Encode(VipsImage inImage, VipsCoding coding, out VipsImage outImage)
    {
        // ... implementation ...
    }

    public bool IsMSBfirst()
    {
        return Magic == VIPS_MAGIC_SPARC;
    }

    public bool IsFile()
    {
        switch (DType)
        {
            case ImageType.MMAPIN:
            case ImageType.MMAPINRW:
            case ImageType.OPENOUT:
            case ImageType.OPENIN:
                return true;

            default:
                return false;
        }
    }

    public bool IsPartial()
    {
        return DType == ImageType.PARTIAL;
    }

    public bool HasAlpha()
    {
        // ... implementation ...
    }

    public int WritePrepare()
    {
        // ... implementation ...
    }

    public int WriteLine(int ypos, byte[] linebuffer)
    {
        // ... implementation ...
    }

    public int WioInput()
    {
        // ... implementation ...
    }

    public int PioInput()
    {
        // ... implementation ...
    }

    public int PioOutput()
    {
        // ... implementation ...
    }
}

public class VipsObject : IDisposable
{
    // ... other properties and methods ...

    public void Dispose()
    {
        // ... implementation ...
    }
}
```

Note that this is not an exhaustive translation, but rather a selection of the most relevant parts. You will need to complete the implementation based on your specific requirements.

Also, please note that C# does not have direct equivalents for some of the C data types and functions used in the original code. For example, `VipsBandFormat` is an enum in C, but it would be a class or struct in C#. Similarly, `vips_image_new_from_file()` is a function in C, but it would be a method on the `VipsImage` class in C#.

You will need to adjust the code accordingly and add any necessary type conversions or modifications to make it work correctly.