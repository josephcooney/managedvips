Here is the C# code equivalent to the provided C code:

```csharp
using System;
using Vips;

public class PpmSave : ForeignSave
{
    public enum Format { Pbm, Pgm, Ppm, Pfm, Pnm };

    [Flags]
    public enum Saveable { Any = 0x01, Bmp = 0x02, Jpeg = 0x04, Png = 0x08, Tiff = 0x10, Ppm = 0x20 };

    private Format format;
    private bool ascii;
    private int bitdepth;

    public PpmSave()
    {
        this.format = Format.Ppm;
        this.ascii = false;
        this.bitdepth = 0;
    }

    public override int Build(VipsObject obj)
    {
        VipsImage image = (VipsImage)obj.Image;
        VipsTarget target = null;

        if (obj.Target != null)
        {
            target = (VipsTarget)obj.Target;
        }
        else
        {
            string filename = obj.Filename;
            if (!string.IsNullOrEmpty(filename))
            {
                target = new TargetToFilename(filename);
            }
        }

        if (target == null)
        {
            return -1;
        }

        VipsImage imageCopy = image.Copy();

        // We don't allow alpha. Just flatten it out.
        if (imageCopy.HasAlpha())
        {
            VipsImage[] bands = new VipsImage[5];
            if (!Flatten(imageCopy, bands, "background", obj.Background))
            {
                return -1;
            }
            imageCopy = bands[0];
        }

        // ppm types to set the defaults for bitdepth etc.
        switch (this.format)
        {
            case Format.Pbm:
                if (!obj.ArgumentIsSet("bitdepth"))
                {
                    this.bitdepth = 1;
                }
                target.Format = VipsFormat.Uchar;
                target.Interpretation = VipsInterpretation.BW;
                target.Bands = 1;
                break;

            case Format.Pgm:
                if (imageCopy.Format == VipsFormat.Ushort)
                {
                    target.Interpretation = VipsInterpretation.Grey16;
                    target.Format = VipsFormat.Ushort;
                }
                else
                {
                    target.Interpretation = VipsInterpretation.BW;
                    target.Format = VipsFormat.Uchar;
                }
                target.Bands = 1;
                break;

            case Format.Ppm:
                if (imageCopy.Format == VipsFormat.Ushort)
                {
                    target.Interpretation = VipsInterpretation.RGB16;
                    target.Format = VipsFormat.Ushort;
                }
                else
                {
                    target.Interpretation = VipsInterpretation.sRGB;
                    target.Format = VipsFormat.Uchar;
                }
                target.Bands = 3;
                break;

            case Format.Pfm:
                target.Format = VipsFormat.Float;
                target.Interpretation = VipsInterpretation.scRGB;
                if (imageCopy.Bands > 1)
                {
                    target.Bands = 3;
                }
                else
                {
                    // can have mono pfm, curiously
                    target.Bands = 1;
                }
                break;

            case Format.Pnm:
            default:
                // Just use the input format and interpretation.
                target.Format = imageCopy.Format;
                target.Interpretation = imageCopy.Type;
                target.Bands = imageCopy.Bands;
                break;
        }

        if (!obj.ArgumentIsSet("bitdepth") || this.bitdepth == 0)
        {
            if (imageCopy.Format == VipsFormat.Uchar)
            {
                target.MaxValue = (uint)UCHAR_MAX;
            }
            else if (imageCopy.Format == VipsFormat.Ushort)
            {
                target.MaxValue = (ushort)USHRT_MAX;
            }
            else if (imageCopy.Format == VipsFormat.UInt)
            {
                target.MaxValue = (uint)UINT_MAX;
            }
            else if (imageCopy.Format == VipsFormat.Float)
            {
                double scale = 1.0;
                if (imageCopy.GetDouble("pfm-scale", out scale))
                {
                    string buf = new string(' ', G_ASCII_DTOSTR_BUF_SIZE);
                    GAsciiDToString(buf, G_ASCII_DTOSTR_BUF_SIZE, scale);
                    target.Write(buf);

                    // Add some secret trailing spaces to the scale field to try to
                    // get the data to land on a 4 byte (the largest pixel ppm can
                    // handle) boundary.
                    if (!this.ascii)
                    {
                        int padding = 4 - ((int)target.Position + 1) % 4;
                        target.Write(new char[padding], padding);
                    }
                }
            }
        }

        if (this.bitdepth > 0)
        {
            this.fn = this.ascii ? LineAscii_1bit : LineBinary_1bit;
        }
        else
        {
            this.fn = this.ascii ? LineAscii : LineBinary;
        }

        // 16 and 32-bit binary write might need byteswapping.
        if (!this.ascii && (imageCopy.Format == VipsFormat.Ushort || imageCopy.Format == VipsFormat.UInt || imageCopy.Format == VipsFormat.Float))
        {
            if (!Byteswap(imageCopy, out VipsImage swappedImage))
            {
                return -1;
            }
            imageCopy = swappedImage;
        }

        // Save the image.
        int result = SinkDisc(imageCopy, this.fn);

        // Clean up.
        target.Close();

        return result;
    }

    private bool LineAscii(VipsImage image, VipsPel[] p)
    {
        int n_elements = image.Xsize * image.Bands;

        for (int i = 0; i < n_elements; i++)
        {
            switch (image.Format)
            {
                case VipsFormat.Uchar:
                    target.Write("{0} ", p[i]);
                    break;

                case VipsFormat.Ushort:
                    target.Write("{0} ", ((ushort)p[i]));
                    break;

                case VipsFormat.UInt:
                    target.Write("{0} ", ((uint)p[i]));
                    break;

                default:
                    throw new ArgumentException("Invalid format");
            }
        }

        if (target.WriteLine())
        {
            return false;
        }

        return true;
    }

    private bool LineAscii_1bit(VipsImage image, VipsPel[] p)
    {
        for (int x = 0; x < image.Xsize; x++)
        {
            target.Write("{0} ", p[x] > 127 ? 0 : 1);
        }

        if (target.WriteLine())
        {
            return false;
        }

        return true;
    }

    private bool LineBinary(VipsImage image, VipsPel[] p)
    {
        if (!target.Write(p, VipsImage.SizeOfLine(image)))
        {
            return false;
        }

        return true;
    }

    private bool LineBinary_1bit(VipsImage image, VipsPel[] p)
    {
        int x;
        int bits = 0;
        int n_bits = 0;

        for (x = 0; x < image.Xsize; x++)
        {
            bits = BitsShiftInt(bits, 1);
            n_bits += 1;
            bits |= p[x] > 127 ? 0 : 1;

            if (n_bits == 8)
            {
                if (!target.Write((byte)bits))
                {
                    return false;
                }

                bits = 0;
                n_bits = 0;
            }
        }

        // Flush any remaining bits in this line.
        if (n_bits > 0 && !target.Write((byte)bits))
        {
            return false;
        }

        return true;
    }

    private bool Byteswap(VipsImage image, out VipsImage swappedImage)
    {
        swappedImage = new VipsImage(image);

        // Swap bytes.
        for (int i = 0; i < image.SizeOfLine(); i++)
        {
            byte[] data = new byte[image.Format == VipsFormat.Ushort ? 2 : 4];
            image.GetBytes(i, data);
            Array.Reverse(data);
            swappedImage.SetBytes(i, data);
        }

        return true;
    }
}

public class PpmSaveFile : ForeignSave
{
    public override int Build(VipsObject obj)
    {
        VipsTarget target = new TargetToFilename(obj.Filename);

        if (obj.ArgumentIsSet("format"))
        {
            string formatStr = obj.GetArgument("format");
            switch (formatStr.ToLower())
            {
                case "pbm":
                    this.format = PpmSave.Format.Pbm;
                    break;

                case "pgm":
                    this.format = PpmSave.Format.Pgm;
                    break;

                case "pfm":
                    this.format = PpmSave.Format.Pfm;
                    break;

                case "pnm":
                    this.format = PpmSave.Format.Pnm;
                    break;

                default:
                    throw new ArgumentException("Invalid format");
            }
        }

        return base.Build(obj);
    }
}

public class PpmSaveTarget : ForeignSave
{
    public override int Build(VipsObject obj)
    {
        VipsTarget target = (VipsTarget)obj.Target;

        if (obj.ArgumentIsSet("format"))
        {
            string formatStr = obj.GetArgument("format");
            switch (formatStr.ToLower())
            {
                case "pbm":
                    this.format = PpmSave.Format.Pbm;
                    break;

                case "pgm":
                    this.format = PpmSave.Format.Pgm;
                    break;

                case "pfm":
                    this.format = PpmSave.Format.Pfm;
                    break;

                case "pnm":
                    this.format = PpmSave.Format.Pnm;
                    break;

                default:
                    throw new ArgumentException("Invalid format");
            }
        }

        return base.Build(obj);
    }
}

public class PbmSaveTarget : PpmSaveTarget
{
    public override void ClassInit()
    {
        base.ClassInit();

        this.format = PpmSave.Format.Pbm;
    }
}

public class PgmSaveTarget : PpmSaveTarget
{
    public override void ClassInit()
    {
        base.ClassInit();

        this.format = PpmSave.Format.Pgm;
    }
}

public class PfmSaveTarget : PpmSaveTarget
{
    public override void ClassInit()
    {
        base.ClassInit();

        this.format = PpmSave.Format.Pfm;
    }
}

public class PnmSaveTarget : PpmSaveTarget
{
    public override void ClassInit()
    {
        base.ClassInit();

        this.format = PpmSave.Format.Pnm;
    }
}
```

Note that some methods and classes have been omitted for brevity, such as the `ForeignSave` class and its methods. Also, some types and constants are not defined in this code snippet, such as `VipsFormat`, `VipsInterpretation`, etc.

This code should be used as a starting point and may need to be modified to fit your specific requirements.