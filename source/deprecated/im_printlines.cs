```csharp
// im_printlines function converted from C source code
public static int ImPrintLines(IMAGE in)
{
    // Check if the image is valid
    if (ImInCheck(in) != 0)
        return -1;

    // Check if the image is uncoded
    if (in.Coding != IM_CODING_NONE)
    {
        ImError("im_printlines", "%s", _("input must be uncoded"));
        return -1;
    }

    // Check if the data is valid
    if (in.Data == null)
    {
        ImError("im_debugim", "%s", _("unsuitable image type"));
        return -1;
    }

    // Define loops for all types
#define loopuc(TYPE) \
{ \
    TYPE[] p = (TYPE[]) in.Data; \
    int x, y, z; \
\
    for (y = 0; y < in.Ysize; y++) { \
        Console.WriteLine("line:" + y.ToString("D5")); \
        for (x = 0; x < in.Xsize; x++) { \
            Console.Write(x.ToString("D5") + "\t"); \
            for (z = 0; z < in.Bands; z++) { \
                Console.Write(p[z * in.Xsize + x].ToString("D4") + "\t"); \
            } \
            Console.WriteLine(); \
        } \
    } \
}

#define loop(TYPE) \
{ \
    TYPE[] p = (TYPE[]) in.Data; \
    int x, y, z; \
\
    for (y = 0; y < in.Ysize; y++) { \
        Console.WriteLine("line:" + y.ToString("D5")); \
        for (x = 0; x < in.Xsize; x++) { \
            Console.Write(x.ToString("D5") + "\t"); \
            for (z = 0; z < in.Bands; z++) { \
                Console.Write(((double)p[z * in.Xsize + x]).ToString("F4") + "\t"); \
            } \
            Console.WriteLine(); \
        } \
    } \
}

#define loopcmplx(TYPE) \
{ \
    TYPE[] p = (TYPE[]) in.Data; \
    int x, y, z; \
\
    for (y = 0; y < in.Ysize; y++) { \
        Console.WriteLine("line:" + y.ToString("D5")); \
        for (x = 0; x < in.Xsize; x++) { \
            Console.Write(x.ToString("D5") + "\t"); \
            for (z = 0; z < in.Bands; z++) { \
                Console.Write(((double)p[z * in.Xsize * 2 + x]).ToString("F4") + "\t"); \
                Console.Write(((double)p[z * in.Xsize * 2 + x + 1]).ToString("F4") + "\t"); \
            } \
            Console.WriteLine(); \
        } \
    } \
}

    // Generate code for all types
    switch (in.BandFmt)
    {
        case IM_BANDFMT_UCHAR:
            loopuc(unsigned char);
            break;
        case IM_BANDFMT_CHAR:
            loop(char);
            break;
        case IM_BANDFMT_USHORT:
            loop(unsigned short);
            break;
        case IM_BANDFMT_SHORT:
            loop(short);
            break;
        case IM_BANDFMT_UINT:
            loop(unsigned int);
            break;
        case IM_BANDFMT_INT:
            loop(int);
            break;
        case IM_BANDFMT_FLOAT:
            loop(float);
            break;
        case IM_BANDFMT_DOUBLE:
            loop(double);
            break;
        case IM_BANDFMT_COMPLEX:
            loopcmplx(float);
            break;
        case IM_BANDFMT_DPCOMPLEX:
            loopcmplx(double);
            break;
        default:
            ImError("im_printlines", "%s", _("unknown input format"));
            return -1;
    }

    return 0;
}
```