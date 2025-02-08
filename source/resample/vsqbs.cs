Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsInterpolateVsqbs : VipsInterpolate
{
    public override void Interpolate(byte[] outArray, VipsRegion inRegion, double absoluteX, double absoluteY)
    {
        // absolute_x and absolute_y are always >= 1.0 (see double-check assert below), so we don't need floor().
        int ix = (int)(absoluteX + 0.5);
        int iy = (int)(absoluteY + 0.5);

        // Move the pointer to (the first band of) the top/left pixel of the 2x2 group of pixel centers which contains the sampling location in its convex hull:
        byte[] pArray = inRegion.GetPixel(ix, iy);

        double relativeX = absoluteX - ix;
        double relativeY = absoluteY - iy;

        // VIPS versions of Nicolas's pixel addressing values.
        int lskip = inRegion.LSkip / VipsImage.SizeOfElement(inRegion.Image);

        // Double the bands for complex images to account for the real and imaginary parts being computed independently:
        int actualBands = inRegion.Image.Bands;
        int bands = (VipsBandFormat.IsComplex(inRegion.Image.BandFmt)) ? 2 * actualBands : actualBands;

        // Confirm that absolute_x and absolute_y are >= 1, see above.
        if (absoluteX < 1.0 || absoluteY < 1.0)
            throw new ArgumentException("absolute_x and absolute_y must be >= 1");

        switch (inRegion.Image.BandFmt)
        {
            case VipsBandFormat.UCHAR:
                Call(nosign, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            case VipsBandFormat.CHAR:
                Call(withsign, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            case VipsBandFormat.USHORT:
                Call(nosign, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            case VipsBandFormat.SHORT:
                Call(withsign, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            case VipsBandFormat.UINT:
                Call(nosign, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            case VipsBandFormat.INT:
                Call(withsign, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            // Complex images are handled by doubling bands:
            case VipsBandFormat.FLOAT:
            case VipsBandFormat.COMPLEX:
                Call(fptypes, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            case VipsBandFormat.DOUBLE:
            case VipsBandFormat.DPCOMPLEX:
                Call(fptypes, outArray, pArray, bands, lskip, relativeX, relativeY);
                break;

            default:
                throw new ArgumentException("Invalid band format");
        }
    }

    private void Call(string conversion, byte[] outArray, byte[] pArray, int bands, int lskip, double relativeX, double relativeY)
    {
        // THE STENCIL OF INPUT VALUES:

        // Pointer arithmetic is used to implicitly reflect the input stencil about dos_two---assumed closer to the sampling location than other pixels (ties are OK)---in such a way that after reflection the sampling point is to the bottom right of dos_two.

        int unoTwoShift = -lskip;
        int unoThrShift = lskip + -lskip;
        int dosOneShift = -bands;
        int dosTwoShift = 0;
        int dosThrShift = bands;
        int treOneShift = -bands + lskip;
        int treTwoShift = lskip;
        int treThrShift = bands + lskip;

        double twiceAbsX_0 = (2 * ((relativeX >= 0) ? 1 : -1)) * relativeX;
        double twiceAbsY_0 = (2 * ((relativeY >= 0) ? 1 : -1)) * relativeY;
        double x = twiceAbsX_0 + -0.5;
        double y = twiceAbsY_0 + -0.5;
        double cent = 0.75 - x * x;
        double mid = 0.75 - y * y;
        double left = -0.5 * (x + cent) + 0.5;
        double top = -0.5 * (y + mid) + 0.5;
        double left_p_cent = left + cent;
        double top_p_mid = top + mid;
        double cent_p_rite = 1.0 - left;
        double mid_p_bot = 1.0 - top;
        double rite = 1.0 - left_p_cent;
        double bot = 1.0 - top_p_mid;

        double fourCUnoTwo = left_p_cent * top;
        double fourCDosOne = left * top_p_mid;
        double fourCDosTwo = left_p_cent + top_p_mid;
        double fourCDosThr = cent_p_rite * top_p_mid + rite;
        double fourCTreTwo = mid_p_bot * left_p_cent + bot;
        double fourCTreThr = mid_p_bot * rite + cent_p_rite * bot;
        double fourCUnoThr = top - fourCUnoTwo;

        int band = bands;

        do
        {
            double doubleResult = (
                (((fourCUnoTwo * pArray[unoTwoShift] + fourCDosOne * pArray[dosOneShift]) +
                  (fourCDosTwo * pArray[dosTwoShift] + fourCDosThr * pArray[dosThrShift])) +
                 ((fourCTreTwo * pArray[treTwoShift] + fourCTreThr * pArray[treThrShift]) +
                  (fourCUnoThr * pArray[unoThrShift] + fourCTreOne * pArray[treOneShift]))) *
                0.25;

            byte result = ToConversion(doubleResult, conversion);
            Array.Copy(pArray, lskip, outArray, bands * band, lskip);
            Array.Copy(pArray, bands, outArray, bands * band + lskip, bands - lskip);

            pArray = new byte[pArray.Length];
            Array.Copy(outArray, bands * band, pArray, 0, lskip);
            Array.Copy(outArray, bands * band + lskip, pArray, lskip, bands - lskip);

            outArray[bands * band] = result;
            band--;
        } while (band > 0);
    }

    private byte ToConversion(double value, string conversion)
    {
        switch (conversion)
        {
            case "nosign":
                return (byte)value;

            case "withsign":
                return (byte)(value < 0 ? -1 : 1);

            default:
                throw new ArgumentException("Invalid conversion");
        }
    }

    public class VipsInterpolateVsqbsClass : VipsInterpolateClass
    {
        public override string Nickname { get; set; } = "vsqbs";
        public override string Description { get; set; } = _("B-Splines with antialiasing smoothing");

        public override void Interpolate(byte[] outArray, VipsRegion inRegion, double absoluteX, double absoluteY)
        {
            new VipsInterpolateVsqbs().Interpolate(outArray, inRegion, absoluteX, absoluteY);
        }

        public override int WindowSize { get; set; } = 4;
        public override int WindowOffset { get; set; } = 1;
    }
}

public class VipsInterpolate
{
    public virtual void Interpolate(byte[] outArray, VipsRegion inRegion, double absoluteX, double absoluteY) { }

    public virtual string Nickname { get; set; }
    public virtual string Description { get; set; }
    public virtual int WindowSize { get; set; }
    public virtual int WindowOffset { get; set; }
}

public class VipsRegion
{
    public byte[] GetPixel(int x, int y)
    {
        // implementation of GetPixel method
    }

    public int LSkip { get; set; }
    public VipsImage Image { get; set; }
}

public enum VipsBandFormat
{
    UCHAR,
    CHAR,
    USHORT,
    SHORT,
    UINT,
    INT,
    FLOAT,
    COMPLEX,
    DOUBLE,
    DPCOMPLEX
}

public class VipsImage
{
    public int Bands { get; set; }
    public VipsBandFormat BandFmt { get; set; }

    public static bool IsComplex(VipsBandFormat bandFmt)
    {
        return bandFmt == VipsBandFormat.COMPLEX || bandFmt == VipsBandFormat.DPCOMPLEX;
    }

    public static int SizeOfElement(VipsImage image)
    {
        // implementation of SizeOfElement method
    }
}
```

This C# code defines the `VipsInterpolateVsqbs` class, which implements the `VipsInterpolate` interface. The `Call` method is used to perform the vertex-split subdivision followed by quadratic B-spline smoothing. The `ToConversion` method is used to convert the result of the computation to a byte value.

The code also defines the `VipsRegion`, `VipsImage`, and `VipsBandFormat` classes, which are used to represent the input region, image, and band format, respectively.

Note that this implementation assumes that the `VipsInterpolate` interface is defined elsewhere in the codebase.