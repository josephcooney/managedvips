```csharp
// @(#) int im_gfadd(double a, IMAGE *in1, double b, IMAGE *in2, double c, IMAGE *out);

public static int im_gfadd(double a, Image in1, double b, Image in2, double c, Image out)
{
    // Select output routines
    if (in1.BandFmt == BandFormat.Float || in1.BandFmt == BandFormat.Double)
    {
        return im_gfadd_impl(a, in1, b, in2, c, out);
    }
    else if (in1.BandFmt == BandFormat.Int ||
             in1.BandFmt == BandFormat.UInt ||
             in1.BandFmt == BandFormat.UShort ||
             in1.BandFmt == BandFormat.Short)
    {
        return im_gaddim(a, in1, b, in2, c, out);
    }
    else
    {
        throw new ArgumentException("Unable to accept image1");
    }
}

// @(#) int im_gfadd_impl(double a, IMAGE *in1, double b, IMAGE *in2, double c, IMAGE *out);

private static int im_gfadd_impl(double a, Image in1, double b, Image in2, double c, Image out)
{
    // implementation of im_gfadd
}

// @(#) int im_gaddim(double a, IMAGE *in1, double b, IMAGE *in2, double c, IMAGE *out);

public static int im_gaddim(double a, Image in1, double b, Image in2, double c, Image out)
{
    // implementation of im_gaddim
}

// @(#) int im_gadd(double a, IMAGE *in1, double b, IMAGE *in2, double c, IMAGE *out);

public static int im_gadd(double a, Image in1, double b, Image in2, double c, Image out)
{
    // implementation of im_gadd
}
```

Note: The `Image` class is assumed to have the following properties:

* `BandFmt`: an enum representing the band format (e.g. Float, Double, Int, UInt, UShort, Short)
* Other properties and methods as needed for image processing

The `im_error` function has been replaced with a `ArgumentException`. The implementation of `im_gfadd_impl`, `im_gaddim`, and `im_gadd` is left out as it depends on the specific requirements of the VIPS library.