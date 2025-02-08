```csharp
// C method: rightshift_size_vec
public static int RightShiftSizeVec(IMAGE in, IMAGE out, int xshift, int yshift, int bandFmt)
{
    return Vips.RightShiftSize(in, out, xshift, yshift, bandFmt);
}

// C method: affinei_vec
public static int AffineIVec(IMAGE in, IMAGE out, VipsInterpolate interpolate, double a, double b, double c, double d, double dx, double dy, int x, int y, int w, int h)
{
    return Vips.AffineI(in, out, interpolate, a, b, c, d, dx, dy, x, y, w, h);
}

// C method: affinei_all_vec
public static int AffineIAllVec(IMAGE in, IMAGE out, VipsInterpolate interpolate, double a, double b, double c, double d, double dx, double dy)
{
    return Vips.AffineIAll(in, out, interpolate, a, b, c, d, dx, dy);
}

// C method: shrink_vec
public static int ShrinkVec(IMAGE in, IMAGE out, double xshrink, double yshrink)
{
    return Vips.Shrink(in, out, xshrink, yshrink);
}

// C method: stretch3_vec
public static int Stretch3Vec(IMAGE in, IMAGE out, double xdisp, double ydisp)
{
    return Vips.Stretch3(in, out, xdisp, ydisp);
}

// C method: im__resample
public static im_package ImResample()
{
    im_function[] resampleList = new im_function[]
    {
        new im_function("im_rightshift_size", "decrease size by a power-of-two factor", RightShiftSizeVec),
        new im_function("im_shrink", "shrink image by xfac, yfac times", ShrinkVec),
        new im_function("im_stretch3", "stretch 3%, sub-pixel displace by xdisp/ydisp", Stretch3Vec),
        new im_function("im_affinei", "affine transform", AffineIVec),
        new im_function("im_affinei_all", "affine transform of whole image", AffineIAllVec)
    };

    return new im_package("resample", resampleList.Length, resampleList);
}
```