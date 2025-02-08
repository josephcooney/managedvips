```csharp
// vips_reduceh_uchar_hwy.cpp

using System;
using System.Runtime.InteropServices;

public static class VipsReducehUcharHwy
{
    // HWY_ATTR void vips_reduceh_uchar_hwy(VipsPel *pout, VipsPel *pin,
    //     int32_t n, int32_t width, int32_t bands,
    //     int16_t *HWY_RESTRICT cs[VIPS_TRANSFORM_SCALE + 1],
    //     double X, double hshrink)
    [DllImport("vips")]
    private static extern void VipsReducehUcharHwy(IntPtr pout, IntPtr pin,
        int n, int width, int bands,
        short[] cs, double X, double hshrink);

    public static void ReducehUcharHwy(IntPtr pout, IntPtr pin,
        int n, int width, int bands,
        short[] cs, double X, double hshrink)
    {
#if HWY_ONCE
        VipsReducehUcharHwy(pout, pin,
            n, width, bands,
            cs, X, hshrink);
#else
        // Implementation of the function using dynamic dispatch
        // Not shown here as it's not provided in the original C code
#endif
    }
}
```