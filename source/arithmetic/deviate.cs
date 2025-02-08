Here is the converted C# code:

```csharp
// VipsDeviate
//
// Copyright: 1990, J. Cupitt
//
// Author: J. Cupitt
// Written on: 02/08/1990
// Modified on:
// 5/5/93 JC
//	- now does partial images
//	- less likely to overflow
//	- adapted from im_deviate
// 1/7/93 JC
//	- adapted for partial v2
//	- ANSIfied
// 21/2/95 JC
//	- modernised again
// 11/5/95 JC
// 	- oops! return( NULL ) in im_deviate(), instead of return( -1 )
// 20/6/95 JC
//	- now returns double, not float
// 13/1/05
//	- use 64 bit arithmetic
// 8/12/06
// 	- add liboil support
// 2/9/09
// 	- gtk-doc comment
// 	- minor reformatting
// 4/9/09
// 	- use im__wrapscan()
// 31/7/10
// 	- remove liboil
// 6/11/11
// 	- rewrite as a class

public class VipsDeviate : VipsStatistic
{
    public VipsDeviate()
    {
        sum = 0.0;
        sum2 = 0.0;
        out = 0.0;
    }

    private double sum;
    private double sum2;
    private double out;

    // Start function: allocate space for an array in which we can accumulate the
    // sum and sum of squares for this thread.
    public override void* Start(VipsStatistic statistic)
    {
        return GCHandle.Alloc(new double[] { 0.0, 0.0 }, GCHandleType.Pinned);
    }

    // Stop function. Add this little sum to the main sum.
    public override int Stop(VipsStatistic statistic, void* seq)
    {
        var deviate = (VipsDeviate)statistic;
        var ss2 = (double[])GCHandle.FromHandle((IntPtr)seq).Target;

        deviate.sum += ss2[0];
        deviate.sum2 += ss2[1];

        GCHandle.Free(GCHandle.FromHandle((IntPtr)seq));

        return 0;
    }

    // vips_deviate_build
    public override int Build(VipsObject object)
    {
        var class = VIPS_OBJECT_GET_CLASS(object);
        var statistic = (VipsStatistic)object;
        var deviate = (VipsDeviate)object;

        if (statistic.in != null && !vips_check_noncomplex(class.nickname, statistic.in))
            return -1;

        if (base.Build(object) != 0)
            return -1;

        // Calculate and return deviation. Add a fabs to stop sqrt(<=0).
        var vals = vips_image_get_width(statistic.in) * vips_image_get_height(statistic.in) * vips_image_get_bands(statistic.in);
        var s = deviate.sum;
        var s2 = deviate.sum2;

        object.SetProperty("out", Math.Sqrt(Math.Abs(s2 - (s * s / vals)) / (vals - 1)));

        return 0;
    }

    // vips_deviate_scan
    public override int Scan(VipsStatistic statistic, void* seq, int x, int y, void* in, int n)
    {
        var sz = n * vips_image_get_bands(statistic.in);
        var ss2 = (double[])GCHandle.FromHandle((IntPtr)seq).Target;

        double sum;
        double sum2;

        sum = ss2[0];
        sum2 = ss2[1];

        // Now generate code for all types.
        switch (vips_image_get_format(statistic.in))
        {
            case VIPS_FORMAT_UCHAR:
                LOOP(unsigned char);
                break;
            case VIPS_FORMAT_CHAR:
                LOOP(signed char);
                break;
            case VIPS_FORMAT_USHORT:
                LOOP(unsigned short);
                break;
            case VIPS_FORMAT_SHORT:
                LOOP(signed short);
                break;
            case VIPS_FORMAT_UINT:
                LOOP(unsigned int);
                break;
            case VIPS_FORMAT_INT:
                LOOP(signed int);
                break;
            case VIPS_FORMAT_FLOAT:
                LOOP(float);
                break;
            case VIPS_FORMAT_DOUBLE:
                LOOP(double);
                break;

            default:
                GDebug.Assert(false, "Unknown format");
                break;
        }

        ss2[0] = sum;
        ss2[1] = sum2;

        return 0;
    }
}

// vips_deviate_class_init
public class VipsDeviateClass : VipsStatisticClass
{
    public VipsDeviateClass()
    {
        base.Nickname = "deviate";
        base.Description = _("find image standard deviation");
        base.Build = new Func<VipsObject, int>(VipsDeviate.Build);
        base.Start = new Func<VipsStatistic, void*>(VipsDeviate.Start);
        base.Scan = new Func<VipsStatistic, void*, int, int, void*, int>(VipsDeviate.Scan);
        base.Stop = new Func<VipsStatistic, void*, int>(VipsDeviate.Stop);

        VIPS_ARG_DOUBLE(base, "out", 2,
            _("Output"),
            _("Output value"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsDeviate, out),
            double.MinValue, double.MaxValue, 0.0);
    }
}

// vips_deviate
public static int VipsDeviate(VipsImage in, ref double out, params object[] args)
{
    var result = VipsCallSplit("deviate", in, ref out, args);

    return result;
}
```

Note that I've assumed the existence of a `VIPS_TYPE_STATISTIC` type and other types used in the original code. You may need to modify the code to match your specific use case.

Also note that this is just one possible way to convert the C code to C#. There are many different ways to do it, and some might be more idiomatic or efficient than others.