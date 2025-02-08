Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsInterpolateBicubic : VipsInterpolate
{
    public override void Interpolate(byte[] outArray, VipsRegion inRegion, double x, double y)
    {
        // Find the mask index. We round-to-nearest, so we need to generate
        // indexes in 0 to VIPS_TRANSFORM_SCALE, 2^n + 1 values. We multiply
        // by 2 more than we need to, add one, mask, then shift down again to
        // get the extra range.
        int sx = (int)(x * VIPS_TRANSFORM_SCALE * 2);
        int sy = (int)(y * VIPS_TRANSFORM_SCALE * 2);

        int six = sx & (VIPS_TRANSFORM_SCALE * 2 - 1);
        int siy = sy & (VIPS_TRANSFORM_SCALE * 2 - 1);

        int tx = (six + 1) >> 1;
        int ty = (siy + 1) >> 1;

        // We know x/y are always positive, so we can just (int) them.
        int ix = (int)x;
        int iy = (int)y;

        // Back and up one to get the top-left of the 4x4.
        byte[] p = inRegion.GetPixelData();

        // Look up the tables we need.
        int[] cxi = vips_bicubic_matrixi[tx];
        int[] cyi = vips_bicubic_matrixi[ty];
        double[] cxf = vips_bicubic_matrixf[tx];
        double[] cyf = vips_bicubic_matrixf[ty];

        // Pel size and line size.
        int bands = inRegion.Bands;
        int lskip = VIPS_REGION_LSKIP(inRegion);

        // Confirm that absolute_x and absolute_y are >= 1, because of
        // window_offset.
        if (x < 1.0)
            throw new ArgumentException("x must be greater than or equal to 1");
        if (y < 1.0)
            throw new ArgumentException("y must be greater than or equal to 1");

        if (ix - 1 < inRegion.Valid.Left || iy - 1 < inRegion.Valid.Top ||
            ix + 2 >= VIPS_RECT_RIGHT(&inRegion.Valid) ||
            iy + 2 >= VIPS_RECT_BOTTOM(&inRegion.Valid))
            throw new ArgumentException("Invalid region");

#ifdef DEBUG
        Console.WriteLine("vips_interpolate_bicubic_interpolate: " + x + " " + y);
        Console.WriteLine("\tleft=" + (ix - 1) + ", top=" + (iy - 1) + ", width=4, height=4");
        Console.WriteLine("\tmaskx=" + tx + ", masky=" + ty);
#endif /*DEBUG*/

        switch (inRegion.Image.BandFmt)
        {
            case VIPS_FORMAT_UCHAR:
                bicubic_unsigned_int_tab<unsigned char>(outArray, p, bands, lskip,
                    cxi, cyi);
                break;

            case VIPS_FORMAT_CHAR:
                bicubic_signed_int_tab<signed char, SCHAR_MIN, SCHAR_MAX>(outArray, p, bands, lskip,
                    cxi, cyi);
                break;

            case VIPS_FORMAT_USHORT:
                bicubic_unsigned_int32_tab<unsigned short, USHRT_MAX>(outArray, p, bands, lskip,
                    cxf, cyf);
                break;

            case VIPS_FORMAT_SHORT:
                bicubic_signed_int32_tab<signed short, SHRT_MIN, SHRT_MAX>(outArray, p, bands, lskip,
                    cxf, cyf);
                break;

            case VIPS_FORMAT_UINT:
                bicubic_unsigned_int32_tab<unsigned int, INT_MAX>(outArray, p, bands, lskip,
                    cxf, cyf);
                break;

            case VIPS_FORMAT_INT:
                bicubic_signed_int32_tab<signed int, INT_MIN, INT_MAX>(outArray, p, bands, lskip,
                    cxf, cyf);
                break;

            case VIPS_FORMAT_FLOAT:
                bicubic_float_tab<float>(outArray, p, bands, lskip,
                    cxf, cyf);
                break;

            case VIPS_FORMAT_DOUBLE:
                bicubic_notab<double>(outArray, p, bands, lskip,
                    x - ix, y - iy);
                break;

            case VIPS_FORMAT_COMPLEX:
                bicubic_float_tab<float>(outArray, p, bands * 2, lskip,
                    cxf, cyf);
                break;

            case VIPS_FORMAT_DPCOMPLEX:
                bicubic_notab<double>(outArray, p, bands * 2, lskip,
                    x - ix, y - iy);
                break;

            default:
                throw new ArgumentException("Unsupported band format");
        }
    }

    public override void ClassInit()
    {
        VipsObjectClass object_class = (VipsObjectClass)GetType();
        VipsInterpolateClass interpolate_class =
            (VipsInterpolateClass)object_class;

        object_class.Nickname = "bicubic";
        object_class.Description = "Bicubic interpolation (Catmull-Rom)";

        interpolate_class.Interpolate = Interpolate;
        interpolate_class.WindowSize = 4;

        // Build the tables of pre-computed coefficients.
        for (int x = 0; x < VIPS_TRANSFORM_SCALE + 1; x++)
        {
            calculate_coefficients_catmull(vips_bicubic_matrixf[x],
                (float)x / VIPS_TRANSFORM_SCALE);

            for (int i = 0; i < 4; i++)
                vips_bicubic_matrixi[x][i] =
                    vips_bicubic_matrixf[x][i] *
                    VIPS_INTERPOLATE_SCALE;
        }
    }

    public override void Init()
    {
#ifdef DEBUG
        Console.WriteLine("vips_interpolate_bicubic_init:");
#endif /*DEBUG*/
    }
}
```

Note that I've assumed the existence of `VipsInterpolate`, `VipsRegion`, and other classes/types, as they are not defined in the provided C code. You may need to modify the code to match your specific implementation.

Also, I've used `Console.WriteLine` for debugging purposes, but you can replace it with your own logging mechanism if needed.

Please note that this is a direct translation of the C code and might require some adjustments to work correctly in a .NET environment.