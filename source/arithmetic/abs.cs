Here is the converted C# code:

```csharp
// Copyright: 1990, N. Dessipris, based on im_powtra()
// Author: Nicos Dessipris
// Written on: 02/05/1990
// Modified on:
// 5/5/93 J.Cupitt
//	- adapted from im_lintra to work with partial images
//	- complex and signed support added
// 30/6/93 JC
//	- adapted for partial v2
//	- ANSI conversion
//	- spe29873r6k3h()**!@lling errors removed
// 9/2/95 JC
//	- adapted for im_wrap...
// 20/6/02 JC
//	- tiny speed up
// 8/12/06
// 	- add liboil support
// 28/8/09
// 	- gtkdoc
// 	- tiny polish
// 31/7/10
// 	- remove liboil
// 6/11/11
// 	- redone as a class
// 3/12/13
// 	- add orc, though the speed improvement vs. gcc's auto-vectorizer
// 	  seems very marginal
// 21/2/19
// 	- move orc init to first use of abs

using System;
using System.Runtime.InteropServices;

public class VipsAbs : VipsUnary
{
    // vips_abs_build
    public override int Build(VipsObject obj)
    {
        VipsUnary unary = (VipsUnary)obj;

        if (unary.in != null && vips_band_format_isuint(unary.in.BandFmt))
            return vips_unary_copy(unary);

        if (base.Build(obj) == -1)
            return -1;

        return 0;
    }

    // Integer abs operation: just test and negate.
    public static void AbsInt(IntPtr inPtr, IntPtr outPtr, int sz)
    {
        signed char* p = (signed char*)inPtr;
        signed char* q = (signed char*)outPtr;
        int x;

        for (x = 0; x < sz; x++)
            q[x] = p[x] < 0 ? 0 - p[x] : p[x];
    }

    // Float abs operation: call fabs().
    public static void AbsFloat(IntPtr inPtr, IntPtr outPtr, int sz)
    {
        float* p = (float*)inPtr;
        float* q = (float*)outPtr;
        int x;

        for (x = 0; x < sz; x++)
            q[x] = Math.Abs(p[x]);
    }

    // Complex abs operation: calculate modulus.
    public static void AbsComplex(IntPtr inPtr, IntPtr outPtr, int sz)
    {
        float* p = (float*)inPtr;
        float* q = (float*)outPtr;
        int x;

        for (x = 0; x < sz / 2; x++)
        {
            q[x] = (float)Math.Sqrt(p[0 * 2 + 0] * p[0 * 2 + 0] + p[0 * 2 + 1] * p[0 * 2 + 1]);
            p += 2;
        }
    }

    // vips_abs_buffer
    public override void ProcessLine(VipsArithmetic arithmetic, VipsPel[] outArray, IntPtr inPtr, int width)
    {
        VipsImage im = arithmetic.ready[0];
        int bands = vips_image_get_bands(im);
        int sz = width * bands;

        switch (vips_image_get_format(im))
        {
            case VIPS_FORMAT_CHAR:
                AbsInt(inPtr, outArray, sz);
                break;
            case VIPS_FORMAT_SHORT:
                AbsInt((IntPtr)((short*)inPtr), (IntPtr)((short*)outArray), sz);
                break;
            case VIPS_FORMAT_INT:
                AbsInt((IntPtr)((int*)inPtr), (IntPtr)((int*)outArray), sz);
                break;
            case VIPS_FORMAT_FLOAT:
                AbsFloat(inPtr, outArray, sz);
                break;
            case VIPS_FORMAT_DOUBLE:
                AbsFloat((IntPtr)((double*)inPtr), (IntPtr)((double*)outArray), sz);
                break;
            case VIPS_FORMAT_COMPLEX:
                AbsComplex(inPtr, outArray, sz);
                break;
            case VIPS_FORMAT_DPCOMPLEX:
                AbsComplex((IntPtr)((double*)inPtr), (IntPtr)((double*)outArray), sz);
                break;

            default:
                throw new ArgumentException("Invalid image format");
        }
    }

    // vips_abs_class_init
    public static void ClassInit(VipsAbsClass class_)
    {
        VipsObjectClass objectClass = (VipsObjectClass)class_;
        VipsArithmeticClass aClass = VIPS_ARITHMETIC_CLASS(class_);

        objectClass.nickname = "abs";
        objectClass.description = "Absolute value of an image";

        aClass.process_line = ProcessLine;

        vips_arithmetic_set_format_table(aClass, new VipsBandFormat[] { 
            VIPS_FORMAT_UCHAR, VIPS_FORMAT_CHAR, VIPS_FORMAT_USHORT, VIPS_FORMAT_SHORT,
            VIPS_FORMAT_UINT, VIPS_FORMAT_INT, VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT,
            VIPS_FORMAT_DOUBLE, VIPS_FORMAT_DOUBLE
        });
    }

    // vips_abs_init
    public override void Init()
    {
    }

    // vips_abs
    [DllImport("vips")]
    private static extern int VipsAbs(VipsImage inImage, ref VipsImage outImage);

    public static int Abs(VipsImage inImage, ref VipsImage outImage)
    {
        return VipsAbs(inImage, ref outImage);
    }
}
```

Note that I've assumed the `VipsUnary` class and other types are already defined elsewhere. Also, I've used the `Math.Abs` method for float abs operation, which is not exactly equivalent to the original C code's use of `fabs()`, but should be close enough.