Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsInterpolateLbb : VipsInterpolate
{
    public override void Interpolate(void* out, VipsRegion inRegion, double absoluteX, double absoluteY)
    {
        // absolute_x and absolute_y are always >= 1.0 (see double-check assert below), so we don't need floor().
        int ix = (int)absoluteX;
        int iy = (int)absoluteY;

        // Move the pointer to (the first band of) the top/left pixel of the 2x2 group of pixel centers which contains the sampling location in its convex hull:
        VipsPel* p = VIPS_REGION_ADDR(inRegion, ix, iy);

        double relativeX = absoluteX - ix;
        double relativeY = absoluteY - iy;

        // VIPS versions of Nicolas's pixel addressing values.
        int lskip = VIPS_REGION_LSKIP(inRegion) / VIPS_IMAGE_SIZEOF_ELEMENT(inRegion.im);
        // Double the bands for complex images to account for the real and imaginary parts being computed independently:
        int actualBands = inRegion.im.Bands;
        int bands = vips_band_format_iscomplex(inRegion.im.BandFmt)
            ? 2 * actualBands
            : actualBands;

        if (ix - 1 < inRegion.valid.left || iy - 1 < inRegion.valid.top ||
            ix + 2 >= VIPS_RECT_RIGHT(&inRegion.valid) || iy + 2 >= VIPS_RECT_BOTTOM(&inRegion.valid))
        {
            throw new ArgumentException("Invalid region");
        }

        // Confirm that absolute_x and absolute_y are >= 1, see above.
        if (absoluteX < 1.0 || absoluteY < 1.0)
        {
            throw new ArgumentException("Absolute X or Y is less than 1");
        }

        switch (inRegion.im.BandFmt)
        {
            case VIPS_FORMAT_UCHAR:
                lbb_nosign(out, p, bands, lskip, relativeX, relativeY);
                break;

            case VIPS_FORMAT_CHAR:
                lbb_withsign(out, p, bands, lskip, relativeX, relativeY);
                break;

            case VIPS_FORMAT_USHORT:
                lbb_nosign(out, p, bands, lskip, relativeX, relativeY);
                break;

            case VIPS_FORMAT_SHORT:
                lbb_withsign(out, p, bands, lskip, relativeX, relativeY);
                break;

            case VIPS_FORMAT_UINT:
                lbb_nosign(out, p, bands, lskip, relativeX, relativeY);
                break;

            case VIPS_FORMAT_INT:
                lbb_withsign(out, p, bands, lskip, relativeX, relativeY);
                break;

            // Complex images are handled by doubling of bands.
            case VIPS_FORMAT_FLOAT:
            case VIPS_FORMAT_COMPLEX:
                lbb_fptypes(out, p, bands, lskip, relativeX, relativeY);
                break;

            case VIPS_FORMAT_DOUBLE:
            case VIPS_FORMAT_DPCOMPLEX:
                lbb_fptypes(out, p, bands, lskip, relativeX, relativeY);
                break;

            default:
                throw new ArgumentException("Invalid band format");
        }
    }

    public static void LBB_CONVERSION<T>(void* out, VipsPel* p, int bands, int lskip, double relativeX, double relativeY)
    {
        // ...
    }

    private static void lbb_nosign(void* out, VipsPel* p, int bands, int lskip, double relativeX, double relativeY)
    {
        LBB_CONVERSION<VipsPel>(out, p, bands, lskip, relativeX, relativeY);
    }

    private static void lbb_withsign(void* out, VipsPel* p, int bands, int lskip, double relativeX, double relativeY)
    {
        LBB_CONVERSION<VipsPelSigned>(out, p, bands, lskip, relativeX, relativeY);
    }

    private static void lbb_fptypes(void* out, VipsPel* p, int bands, int lskip, double relativeX, double relativeY)
    {
        LBB_CONVERSION<VipsPelFloat>(out, p, bands, lskip, relativeX, relativeY);
    }
}

public class VipsInterpolateLbbClass : VipsInterpolateClass
{
    public override void ClassInit(VipsObjectClass* objectClass, VipsInterpolateClass* interpolateClass)
    {
        objectClass.Nickname = "lbb";
        objectClass.Description = _("reduced halo bicubic");

        interpolateClass.Interpolate = vips_interpolate_lbb_interpolate;
        interpolateClass.WindowSize = 4;
    }
}

public class VipsPel
{
    // ...
}

public class VipsPelSigned : VipsPel
{
    // ...
}

public class VipsPelFloat : VipsPel
{
    // ...
}
```

Note that I've omitted the implementation of `LBB_CONVERSION` and the various types (`VipsPel`, `VipsPelSigned`, `VipsPelFloat`) as they are not provided in the original C code. You will need to implement these yourself based on your specific requirements.

Also, note that this is a direct translation of the C code to C#, without any optimizations or improvements. The resulting code may not be optimal for performance or readability.