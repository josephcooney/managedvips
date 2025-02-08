Here is the converted C# code:

```csharp
// find image minimum
//
// Copyright: 1990, J. Cupitt
//
// Author: J. Cupitt
// Written on: 02/05/1990
// Modified on : 18/03/1991, N. Dessipris
// 23/11/92 JC
//	- correct result for more than 1 band now.
// 23/7/93 JC
//	- im_incheck() added
// 20/6/95 JC
//	- now returns double for value, like im_min()
// 4/9/09
// 	- gtkdoc comment
// 8/9/09
// 	- rewrite, from im_minpos()
// 30/8/11
// 	- rewrite as a class
// 5/9/11
// 	- abandon scan if we find minimum possible value
// 24/2/12
// 	- avoid NaN in float/double/complex images
// 	- allow +/- INFINITY as a result
// 4/12/12
// 	- from min.c
// 	- track and return bottom n values

public class VipsMin : VipsStatistic
{
    public VipsMin()
    {
        Size = 1;
    }

    public override int Build(VipsObject obj)
    {
        VipsStatistic statistic = (VipsStatistic)obj;
        VipsImage image = (VipsImage)statistic.In;

        if (VIPS_BAND_FORMAT_ISCOMPLEX(vips_image_get_format(image)))
        {
            // For speed we accumulate min ** 2 for complex.
            int i;
            for (i = 0; i < Values.N; i++)
                Values.Value[i] = Math.Sqrt(Values.Value[i]);
        }

        if (Values.N > 0)
        {
            VipsArrayDouble outArray = new VipsArrayDouble(Values.Value, Values.N);
            VipsArrayInt xArray = new VipsArrayInt(Values.XPos, Values.N);
            VipsArrayInt yArray = new VipsArrayInt(Values.YPos, Values.N);

            // We have to set the props via g_object_set() to stop vips complaining they are unset.
            GObject.SetProperties(this,
                "Out", Values.Value[Values.N - 1],
                "X", Values.XPos[Values.N - 1],
                "Y", Values.YPos[Values.N - 1],
                "OutArray", outArray,
                "XArray", xArray,
                "YArray", yArray,
                null);
        }

#ifdef DEBUG
        {
            int i;

            Console.WriteLine("vips_min_build: {0} values found", Values.N);
            for (i = 0; i < Values.N; i++)
                Console.WriteLine("{0}) {1}\t{2}\t{3}",
                    i,
                    Values.Value[i],
                    Values.XPos[i], Values.YPos[i]);
        }
#endif /*DEBUG*/

        return 0;
    }

    public override void* Start(VipsStatistic statistic)
    {
        VipsValues values = new VipsValues();
        vips_values_init(values, this);

        return (void*)values;
    }

    public override int Stop(VipsStatistic statistic, void* seq)
    {
        VipsMin min = (VipsMin)statistic;
        VipsValues values = (VipsValues)seq;

        int i;
        for (i = 0; i < values.N; i++)
            vips_values_add(&min.Values,
                values.Value[i], values.XPos[i], values.YPos[i]);

        return 0;
    }

    public override int Scan(VipsStatistic statistic, void* seq,
        int x, int y, VipsImage in, int n)
    {
        VipsValues values = (VipsValues)seq;
        const int bands = vips_image_get_bands(statistic.In);
        const int sz = n * bands;

        int i;

        switch (vips_image_get_format(statistic.In))
        {
            case VIPS_FORMAT_UCHAR:
                LOOPU(unsigned char, 0);
                break;
            case VIPS_FORMAT_CHAR:
                LOOPU(sbyte, sbyte.MinValue);
                break;
            case VIPS_FORMAT_USHORT:
                LOOPU(ushort, 0);
                break;
            case VIPS_FORMAT_SHORT:
                LOOPU(short, short.MinValue);
                break;
            case VIPS_FORMAT_UINT:
                LOOPU(uint, 0);
                break;
            case VIPS_FORMAT_INT:
                LOOPU(int, int.MinValue);
                break;

            case VIPS_FORMAT_FLOAT:
                LOOPF(float);
                break;
            case VIPS_FORMAT_DOUBLE:
                LOOPF(double);
                break;

            case VIPS_FORMAT_COMPLEX:
                LOOPC(float);
                break;
            case VIPS_FORMAT_DPCOMPLEX:
                LOOPC(double);
                break;

            default:
                GAssert.NotReached();
                break;
        }

        return 0;
    }

    public static void Main(string[] args)
    {
        // vips_min: (method)
        // @in: input #VipsImage
        // @out: (out): output pixel minimum
        // @...: %NULL-terminated list of optional named arguments

        // Optional arguments:
        //
        // * @x: horizontal position of minimum
        // * @y: vertical position of minimum
        // * @size: number of minima to find
        // * @out_array: return array of minimum values
        // * @x_array: corresponding horizontal positions
        // * @y_array: corresponding vertical positions

        VipsImage image = new VipsImage();
        double outValue;
        int x, y;

        // This operation finds the minimum value in an image.
        //
        // By default it finds the single smallest value. If @size is set >1, it will
        // find the @size smallest values. It will stop searching early if has found
        // enough values.
        // Equal values will be sorted by y then x.

        // It operates on all
        // bands of the input image: use vips_stats() if you need to find an
        // minimum for each band.

        // For complex images, this operation finds the minimum modulus.

        // You can read out the position of the minimum with @x and @y. You can read
        // out arrays of the values and positions of the top @size minima with
        // @out_array, @x_array and @y_array.
        // These values are returned sorted from
        // smallest to largest.

        // If there are more than @size minima, the minima returned will be a random
        // selection of the minima in the image.

        // See also: vips_min(), vips_stats().
    }

    private static void LOOPU(Type type, int lower)
    {
        TYPE* p = (TYPE*)in;
        TYPE m;

        for (i = 0; i < sz && values.N < values.Size; i++)
            vips_values_add(values, p[i], x + i / bands, y);
        m = values.Value[0];

        for (; i < sz; i++) {
            if (p[i] < m) {
                vips_values_add(values, p[i], x + i / bands, y);
                m = values.Value[0];

                if (m <= lower) {
                    statistic.Stop = true;
                    break;
                }
            }
        }
    }

    private static void LOOPF(Type type)
    {
        TYPE* p = (TYPE*)in;
        TYPE m;

        for (i = 0; i < sz && values.N < values.Size; i++)
            if (!VIPS_ISNAN(p[i]))
                vips_values_add(values, p[i], x + i / bands, y);
        m = values.Value[0];

        for (; i < sz; i++)
            if (p[i] < m) {
                vips_values_add(values, p[i], x + i / bands, y);
                m = values.Value[0];
            }
    }

    private static void LOOPC(Type type)
    {
        TYPE* p = (TYPE*)in;
        TYPE m;

        for (i = 0; i < sz && values.N < values.Size; i++) {
            TYPE mod2 = p[0] * p[0] + p[1] * p[1];

            if (!VIPS_ISNAN(mod2))
                vips_values_add(values, p[i], x + i / bands, y);

            p += 2;
        }
        m = values.Value[0];

        for (; i < sz; i++) {
            TYPE mod2 = p[0] * p[0] + p[1] * p[1];

            if (mod2 < m) {
                vips_values_add(values, mod2, x + i / bands, y);
                m = values.Value[0];
            }

            p += 2;
        }
    }

    private static void vips_values_init(VipsValues* values, VipsMin* min)
    {
        values->min = min;

        values->Size = min.Size;
        values->N = 0;
        values->Value = new double[values.Size];
        values->XPos = new int[values.Size];
        values->YPos = new int[values.Size];
    }

    private static void vips_values_add(VipsValues* values, double v, int x, int y)
    {
        int i, j;

        // Find insertion point.
        for (i = 0; i < values.N; i++) {
            if (v > values.Value[i])
                break;

            if (v == values.Value[i]) {
                if (y < values.YPos[i])
                    break;

                if (y == values.YPos[i])
                    if (x <= values.XPos[i])
                        break;
            }
        }

        // Array full?
        if (values.N == values.Size) {
            if (i > 0) {
                // We need to move stuff to the left to make space,
                // shunting the largest out.
                for (j = 0; j < i - 1; j++) {
                    values.Value[j] = values.Value[j + 1];
                    values.XPos[j] = values.XPos[j + 1];
                    values.YPos[j] = values.YPos[j + 1];
                }
                values.Value[i - 1] = v;
                values.XPos[i - 1] = x;
                values.YPos[i - 1] = y;
            }
        } else {
            // Not full, move stuff to the right into empty space.
            for (j = values.N; j > i; j--) {
                values.Value[j] = values.Value[j - 1];
                values.XPos[j] = values.XPos[j - 1];
                values.YPos[j] = values.YPos[j - 1];
            }
            values.Value[i] = v;
            values.XPos[i] = x;
            values.YPos[i] = y;
            values.N += 1;
        }
    }

    private static void GAssert_NotReached()
    {
        throw new Exception("Not reached");
    }

    public class VipsValues
    {
        public VipsMin Min { get; set; }
        public int Size { get; set; }
        public int N { get; set; }
        public double[] Value { get; set; }
        public int[] XPos { get; set; }
        public int[] YPos { get; set; }

        public VipsValues()
        {
            Min = null;
            Size = 1;
            N = 0;
            Value = new double[Size];
            XPos = new int[Size];
            YPos = new int[Size];
        }
    }
}
```