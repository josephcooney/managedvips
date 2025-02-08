```csharp
// im_debugim (IMAGE *in)

public static int ImDebugIm(IMAGE in)
{
    // Check our args.
    if (!InCheck(in))
        return -1;
    if (in.Coding != IM_CODING_NONE)
    {
        Error("im_debugim", "input must be uncoded");
        return -1;
    }

    // What type? First define the loop we want to perform for all types.

#define LOOPUC(TYPE) \
    { \
        TYPE[] p = (TYPE[])in.Data; \
        int x, y, z; \
\
        for (y = 0; y < in.Ysize; y++) { \
            for (x = 0; x < in.Xsize; x++) { \
                for (z = 0; z < in.Bands; z++) { \
                    Console.Write("{0,4}", p[z]); \
                } \
            } \
            Console.WriteLine(); \
        } \
    }

#define LOOP(TYPE) \
    { \
        TYPE[] p = (TYPE[])in.Data; \
        int x, y, z; \
\
        for (y = 0; y < in.Ysize; y++) { \
            for (x = 0; x < in.Xsize; x++) { \
                for (z = 0; z < in.Bands; z++) { \
                    Console.Write("{0:g}\t", p[z]); \
                } \
            } \
            Console.WriteLine(); \
        } \
    }

#define LOOPCMPLX(TYPE) \
    { \
        TYPE[] p = (TYPE[])in.Data; \
        int x, y, z; \
\
        for (y = 0; y < in.Ysize; y++) { \
            for (x = 0; x < in.Xsize; x++) { \
                for (z = 0; z < in.Bands; z += 2) { \
                    Console.Write("re={0:g}\t", p[z]); \
                    Console.Write("im={0:g}\t", p[z + 1]); \
                } \
            } \
            Console.WriteLine(); \
        } \
    }

    // Now generate code for all types.
    switch (in.BandFmt)
    {
        case IM_BANDFMT_UCHAR:
            LOOPUC(unsigned char);
            break;
        case IM_BANDFMT_CHAR:
            LOOP(char);
            break;
        case IM_BANDFMT_USHORT:
            LOOP(unsigned short);
            break;
        case IM_BANDFMT_SHORT:
            LOOP(short);
            break;
        case IM_BANDFMT_UINT:
            LOOP(unsigned int);
            break;
        case IM_BANDFMT_INT:
            LOOP(int);
            break;
        case IM_BANDFMT_FLOAT:
            LOOP(float);
            break;
        case IM_BANDFMT_DOUBLE:
            LOOP(double);
            break;
        case IM_BANDFMT_COMPLEX:
            LOOPCMPLX(float);
            break;
        case IM_BANDFMT_DPCOMPLEX:
            LOOPCMPLX(double);
            break;

        default:
            Error("im_debugim", "unknown input format");
            return -1;
    }

    return 0;
}
```