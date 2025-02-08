```csharp
// @(#) Thresholds an image.  Works for any non-complex type.
// @(#) Output is a binary image with 0 and 255 only
// @(#) Input is either memory mapped or in a buffer.
// @(#)
// @(#) int im_thresh(imin, imout, threshold)
// @(#) IMAGE *imin, *imout;
// @(#) double threshold;
// @(#)
// @(#) Returns 0 on success and -1 on error

public class Image
{
    public static int ImThresh(Image inImage, Image outImage, double threshold)
    {
        int x, y;
        byte[] bu; // Buffer we write to
        int s, epl; // Size and els per line

        // Check our args.
        if (ImIocheck(inImage, outImage))
            return -1;
        if (inImage.Coding != ImageCoding.None)
        {
            ImError("im_thresh", "%s", "input should be uncoded");
            return -1;
        }

        // Set up the output header.  
        if (ImCpDesc(outImage, inImage))
            return -1;
        outImage.BandFmt = ImageBandFormat.UCHAR;
        if (ImSetupout(outImage))
            return -1;

        // Make buffer for building o/p in.  
        epl = inImage.Xsize * inImage.Bands;
        s = epl * sizeof(byte);
        if ((bu = new byte[s]) == null)
            return -1;

#define ImThreshLoop(TYPE) \
        { \
            TYPE[] a = (TYPE[])inImage.data; \
\
            for (y = 0; y < inImage.Ysize; y++) { \
                byte[] b = bu; \
\
                for (x = 0; x < epl; x++) { \
                    double f = (double)*a++; \
                    if (f >= threshold) \
                        *b++ = (byte)255; \
                    else \
                        *b++ = (byte)0; \
                } \
\
                if (ImWriteLine(y, outImage, bu)) \
                    return -1; \
            } \
        }

        // Do the above for all image types.  
        switch (inImage.BandFmt)
        {
            case ImageBandFormat.UCHAR:
                ImThreshLoop(byte);
                break;
            case ImageBandFormat.CHAR:
                ImThreshLoop(sbyte);
                break;
            case ImageBandFormat.UShort:
                ImThreshLoop(ushort);
                break;
            case ImageBandFormat.Short:
                ImThreshLoop(short);
                break;
            case ImageBandFormat.UInt:
                ImThreshLoop(uint);
                break;
            case ImageBandFormat.Int:
                ImThreshLoop(int);
                break;
            case ImageBandFormat.Float:
                ImThreshLoop(float);
                break;
            case ImageBandFormat.Double:
                ImThreshLoop(double);
                break;
            default:
                ImError("im_thresh", "%s", "Unknown input format");
                return -1;
        }

        return 0;
    }
}

public enum ImageCoding
{
    None,
}

public enum ImageBandFormat
{
    UCHAR,
    CHAR,
    UShort,
    Short,
    UInt,
    Int,
    Float,
    Double,
}

public class Image
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }
    public ImageCoding Coding { get; set; }
    public ImageBandFormat BandFmt { get; set; }
    public object data { get; set; }
}

public class ImIocheck
{
    // implementation omitted for brevity
}

public class ImCpDesc
{
    // implementation omitted for brevity
}

public class ImSetupout
{
    // implementation omitted for brevity
}

public class ImWriteLine
{
    // implementation omitted for brevity
}

public class ImError
{
    // implementation omitted for brevity
}
```