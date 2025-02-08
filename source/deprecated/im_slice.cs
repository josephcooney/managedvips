```csharp
// Converted from: int im_slice(IMAGE *in, IMAGE *out, double t1, double t2)

public static int ImSlice(IntPtr inPtr, IntPtr outPtr, double t1, double t2)
{
    int x, y, z;
    byte[] bu;  // Buffer we write to
    int s, epl; // Size and els per line
    double thresh1, thresh2;

    // Check our args.
    if (ImIoCheck(inPtr, outPtr))
    {
        throw new Exception("im_slice: im_iocheck failed");
    }
    if (((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Coding != VipsCoding.None)
    {
        throw new Exception("im_slice: input should be uncoded");
    }

    // Set up the output header.
    if (ImCpDesc(outPtr, inPtr))
    {
        throw new Exception("im_slice: im_cp_desc failed");
    }
    ((VipsImage)Marshal.PtrToStructure(outPtr, typeof(VipsImage))).BandFmt = VipsBandFmt.UChar;
    if (ImSetupOut(outPtr))
    {
        throw new Exception("im_slice: im_setupout failed");
    }

    if (t1 <= t2)
    {
        thresh1 = t1;
        thresh2 = t2;
    }
    else
    {
        thresh1 = t2;
        thresh2 = t1;
    }
    // Make buffer for building o/p in.
    epl = ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Xsize * ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Bands;
    s = epl * sizeof(byte);
    if ((bu = (byte[])ImMalloc(outPtr, (uint)s)) == null)
        return -1;

    // Define what we do for each band element type.
#define ImSliceLoop(TYPE) \
    { \
        TYPE[] a = (TYPE[])Marshal.PtrToStructure(inPtr, typeof(byte[])); \
\
        for (y = 0; y < ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Ysize; y++) { \
            byte[] b = bu; \
\
            for (x = 0; x < ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Xsize; x++) \
                for (z = 0; z < ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Bands; z++) { \
                    double f = (double)a[z * ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Xsize + x]; \
                    if (f <= thresh1) \
                        b[z * ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Xsize + x] = 0; \
                    else if (f > thresh2) \
                        b[z * ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Xsize + x] = 255; \
                    else \
                        b[z * ((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).Xsize + x] = 128; \
                } \
\
            if (ImWriteLine(y, outPtr, bu)) \
                return -1; \
        } \
    }

    // Do the above for all image types.
    switch (((VipsImage)Marshal.PtrToStructure(inPtr, typeof(VipsImage))).BandFmt)
    {
        case VipsBandFmt.UChar:
            ImSliceLoop(byte);
            break;
        case VipsBandFmt.Char:
            ImSliceLoop(sbyte);
            break;
        case VipsBandFmt.UShort:
            ImSliceLoop(ushort);
            break;
        case VipsBandFmt.Short:
            ImSliceLoop(short);
            break;
        case VipsBandFmt.UInt:
            ImSliceLoop(uint);
            break;
        case VipsBandFmt.Int:
            ImSliceLoop(int);
            break;
        case VipsBandFmt.Float:
            ImSliceLoop(float);
            break;
        default:
            throw new Exception("im_slice: Unknown input format");
    }

    return 0;
}

// Helper methods

public static bool ImIoCheck(IntPtr inPtr, IntPtr outPtr)
{
    // Implementation of im_iocheck
}

public static bool ImCpDesc(IntPtr outPtr, IntPtr inPtr)
{
    // Implementation of im_cp_desc
}

public static bool ImSetupOut(IntPtr outPtr)
{
    // Implementation of im_setupout
}

public static byte[] ImMalloc(IntPtr outPtr, uint size)
{
    // Implementation of im_malloc
}

public static bool ImWriteLine(int y, IntPtr outPtr, byte[] bu)
{
    // Implementation of im_writeline
}
```