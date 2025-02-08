Here is the C# code equivalent to the provided C code:

```csharp
using System;

public abstract class VipsInterpolate : VipsObject
{
    public static readonly Type Type = typeof(VipsInterpolate);

    protected VipsInterpolate()
    {
        // Initialize base class
    }

    public override void Finalize()
    {
        // Finalizer implementation (not used in this example)
    }
}

public abstract class VipsInterpolateClass : VipsObjectClass
{
    public static readonly Type Type = typeof(VipsInterpolateClass);

    protected VipsInterpolateClass()
    {
        // Initialize base class
    }

    public virtual int GetWindowSize(VipsInterpolate interpolate)
    {
        return -1; // Default implementation, override in derived classes
    }

    public virtual int GetWindowOffset(VipsInterpolate interpolate)
    {
        return -1; // Default implementation, override in derived classes
    }
}

public class VipsInterpolateNearest : VipsInterpolate
{
    public static readonly Type Type = typeof(VipsInterpolateNearest);

    protected VipsInterpolateNearest()
    {
        // Initialize base class
    }

    public override void Interpolate(VipsPel[] outArray, VipsRegion inRegion, double x, double y)
    {
        int ps = VipsImage.SizeOfPel(inRegion.Image);
        int xi = (int)x;
        int yi = (int)y;

        VipsPel[] p = new VipsPel[ps];
        Array.Copy(inRegion.Address(xi, yi), p, ps);

        for (int z = 0; z < ps; z++)
            outArray[z] = p[z];
    }

    public static readonly Type ClassType = typeof(VipsInterpolateNearestClass);
}

public class VipsInterpolateBilinear : VipsInterpolate
{
    public static readonly Type Type = typeof(VipsInterpolateBilinear);

    protected VipsInterpolateBilinear()
    {
        // Initialize base class
    }

    public override void Interpolate(VipsPel[] outArray, VipsRegion inRegion, double x, double y)
    {
        int ps = VipsImage.SizeOfPel(inRegion.Image);
        int ls = inRegion.LSkip;
        int b = inRegion.Image.Bands * (VipsBandFormat.IsComplex(inRegion.Image.Format) ? 2 : 1);

        int ix = (int)x;
        int iy = (int)y;

        VipsPel[] p1 = new VipsPel[ps];
        VipsPel[] p2 = new VipsPel[ps];
        VipsPel[] p3 = new VipsPel[ls];
        VipsPel[] p4 = new VipsPel[ls];

        Array.Copy(inRegion.Address(ix, iy), p1, ps);
        Array.Copy(inRegion.Address(ix + 1, iy), p2, ps);
        Array.Copy(inRegion.Address(ix, iy + ls), p3, ls);
        Array.Copy(inRegion.Address(ix + 1, iy + ls), p4, ls);

        int z = 0;

        switch (inRegion.Image.Format)
        {
            case VipsFormat.UChar:
                BilinearInt<unsigned char>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.Char:
                BilinearInt<char>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.UShort:
                BilinearInt<unsigned short>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.Short:
                BilinearInt<short>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.UInt:
                BilinearFloat<unsigned int>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.Int:
                BilinearFloat<int>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.Float:
                BilinearFloat<float>(outArray, p1, p2, p3, p4);
                break;
            case VipsFormat.Double:
                BilinearFloat<double>(outArray, p1, p2, p3, p4);
                break;
            default:
                throw new ArgumentException("Unsupported format");
        }
    }

    private static void BilinearInt<T>(T[] outArray, T[] p1, T[] p2, T[] p3, T[] p4)
    {
        int X = (int)(x - ix) * VipsInterpolate.Scale;
        int Y = (int)(y - iy) * VipsInterpolate.Scale;

        int Yd = VipsInterpolate.Scale - Y;

        int c4 = (Y * X) >> VipsInterpolate.Shift;
        int c2 = (Yd * X) >> VipsInterpolate.Shift;
        int c3 = Y - c4;
        int c1 = Yd - c2;

        for (int z = 0; z < outArray.Length; z++)
            outArray[z] = (T)((c1 * p1[z] + c2 * p2[z] + c3 * p3[z] + c4 * p4[z]) >> VipsInterpolate.Shift);
    }

    private static void BilinearFloat<T>(T[] outArray, T[] p1, T[] p2, T[] p3, T[] p4)
    {
        double X = x - ix;
        double Y = y - iy;

        double Yd = 1.0f - Y;

        double c4 = Y * X;
        double c2 = Yd * X;
        double c3 = Y - c4;
        double c1 = Yd - c2;

        for (int z = 0; z < outArray.Length; z++)
            outArray[z] = (T)(c1 * p1[z] + c2 * p2[z] + c3 * p3[z] + c4 * p4[z]);
    }

    public static readonly Type ClassType = typeof(VipsInterpolateBilinearClass);
}

public class VipsObject
{
    // Base class implementation (not used in this example)
}

public class VipsImage
{
    // Image class implementation (not used in this example)

    public int SizeOfPel()
    {
        return 0; // Default implementation, override in derived classes
    }

    public int BandCount()
    {
        return 0; // Default implementation, override in derived classes
    }
}

public enum VipsFormat
{
    UChar,
    Char,
    UShort,
    Short,
    UInt,
    Int,
    Float,
    Double,
    Complex,
    DComplex
}

public class VipsPel
{
    // Pixel class implementation (not used in this example)
}

public class VipsRegion
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public VipsImage Image { get; set; }

    public VipsPel[] Address(int x, int y)
    {
        // Implementation to address pixels in the image
        return null;
    }

    public int LSkip
    {
        get
        {
            // Implementation to get line skip value
            return 0;
        }
    }

    public int BandCount()
    {
        return Image.BandCount();
    }

    public VipsFormat Format
    {
        get
        {
            // Implementation to get image format
            return VipsFormat.UChar;
        }
    }
}

public class VipsBandFormat
{
    public static bool IsComplex(VipsFormat format)
    {
        switch (format)
        {
            case VipsFormat.Complex:
                return true;
            default:
                return false;
        }
    }
}
```

Note that this is a simplified implementation and may not cover all the details of the original C code. Additionally, some methods and classes are not fully implemented as they were not provided in the original C code.

Also note that I've used `VipsPel` to represent a pixel, but it's not a real class in VIPS library. You should replace it with the actual pixel type used by your image processing library.

You can use this code as a starting point and modify it according to your needs.