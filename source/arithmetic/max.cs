Here is the C# code equivalent to the provided C code:

```csharp
// find image maximum
// 
// Copyright: 1990, J. Cupitt
// 
// Author: J. Cupitt
// Written on: 02/05/1990
// Modified on : 18/03/1991, N. Dessipris
// 	23/11/92:  J.Cupitt - correct result for more than 1 band now.
// 23/7/93 JC
//	- im_incheck() call added
// 20/6/95 JC
//	- now returns double for value, like im_max()
// 4/9/09
// 	- gtkdoc comment
// 8/9/09
// 	- rewrite based on im_max() to get partial
// 	- move im_max() in here as a convenience function
// 6/11/11
// 	- rewrite as a class
// 	- abandon scan if we find maximum possible value
// 24/2/12
// 	- avoid NaN in float/double/complex images
// 	- allow +/- INFINITY as a result
// 4/12/12
// 	- track and return top n values
// 24/1/17
// 	- sort equal values by y then x to make order more consistent

using System;
using System.Collections.Generic;

public class VipsMax : VipsStatistic
{
    public VipsMax()
    {
        size = 1;
    }

    public int size { get; set; }
    public double out { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public VipsArrayDouble out_array { get; set; }
    public VipsArrayInt x_array { get; set; }
    public VipsArrayInt y_array { get; set; }

    private VipsValues values;

    protected override void Build()
    {
        values = new VipsValues();
        vips_values_init(values, this);
    }

    private static void vips_values_init(VipsValues values, VipsMax max)
    {
        values.max = max;
        values.size = max.size;
        values.n = 0;
        values.value = new double[max.size];
        values.x_pos = new int[max.size];
        values.y_pos = new int[max.size];
    }

    public void AddValue(double v, int x, int y)
    {
        int i, j;

        // Find insertion point.
        for (i = 0; i < values.n; i++)
        {
            if (v < values.value[i])
                break;

            if (v == values.value[i])
            {
                if (y < values.y_pos[i])
                    break;

                if (y == values.y_pos[i])
                    if (x <= values.x_pos[i])
                        break;
            }
        }

        // Array full?
        if (values.n == values.size)
        {
            if (i > 0)
            {
                // We need to move stuff to the left to make space,
                // shunting the smallest out.
                for (j = 0; j < i - 1; j++)
                {
                    values.value[j] = values.value[j + 1];
                    values.x_pos[j] = values.x_pos[j + 1];
                    values.y_pos[j] = values.y_pos[j + 1];
                }
                values.value[i - 1] = v;
                values.x_pos[i - 1] = x;
                values.y_pos[i - 1] = y;
            }
        }
        else
        {
            // Not full, move stuff to the right into empty space.
            for (j = values.n; j > i; j--)
            {
                values.value[j] = values.value[j - 1];
                values.x_pos[j] = values.x_pos[j - 1];
                values.y_pos[j] = values.y_pos[j - 1];
            }
            values.value[i] = v;
            values.x_pos[i] = x;
            values.y_pos[i] = y;
            values.n += 1;
        }
    }

    public override void Scan(VipsImage in_image, int x, int y)
    {
        VipsValues values = this.values;

        switch (in_image.Format)
        {
            case VIPS_FORMAT_UCHAR:
                LOOPU(unsigned char, UCHAR_MAX);
                break;
            case VIPS_FORMAT_CHAR:
                LOOPU(sbyte, SBYTE_MAX);
                break;
            case VIPS_FORMAT_USHORT:
                LOOPU(ushort, USHORT_MAX);
                break;
            case VIPS_FORMAT_SHORT:
                LOOPU(short, SHORT_MAX);
                break;
            case VIPS_FORMAT_UINT:
                LOOPU(uint, UINT_MAX);
                break;
            case VIPS_FORMAT_INT:
                LOOPU(int, INT_MAX);
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
                throw new Exception("Invalid image format");
        }
    }

    private static void LOOPU<T>(T[] in_array, T max_val) where T : struct
    {
        int i;

        for (i = 0; i < in_array.Length && values.n < values.size; i++)
            vips_values_add(values, in_array[i], x + i / bands, y);

        double m = values.value[0];

        for (; i < in_array.Length; i++)
        {
            if (in_array[i] > m)
            {
                vips_values_add(values, in_array[i], x + i / bands, y);
                m = values.value[0];

                if (m >= max_val)
                {
                    stop = true;
                    break;
                }
            }
        }
    }

    private static void LOOPF<T>(T[] in_array) where T : struct
    {
        int i;

        for (i = 0; i < in_array.Length && values.n < values.size; i++)
            if (!VIPS_ISNAN(in_array[i]))
                vips_values_add(values, in_array[i], x + i / bands, y);

        double m = values.value[0];

        for (; i < in_array.Length; i++)
            if (in_array[i] > m)
            {
                vips_values_add(values, in_array[i], x + i / bands, y);
                m = values.value[0];
            }
    }

    private static void LOOPC<T>(T[] in_array) where T : struct
    {
        int i;

        for (i = 0; i < in_array.Length && values.n < values.size; i++)
        {
            double mod2 = in_array[i * 2] * in_array[i * 2] + in_array[i * 2 + 1] * in_array[i * 2 + 1];

            if (!VIPS_ISNAN(mod2))
                vips_values_add(values, mod2, x + i / bands, y);

            i++;
        }

        double m = values.value[0];

        for (; i < in_array.Length; i++)
        {
            double mod2 = in_array[i * 2] * in_array[i * 2] + in_array[i * 2 + 1] * in_array[i * 2 + 1];

            if (mod2 > m)
            {
                vips_values_add(values, mod2, x + i / bands, y);
                m = values.value[0];
            }

            i++;
        }
    }

    public override void Stop()
    {
        int i;

        for (i = 0; i < values.n; i++)
            vips_values_add(&values, values.value[i], values.x_pos[i], values.y_pos[i]);

        // Set properties via g_object_set() to stop vips complaining they are unset.
        out = values.value[values.n - 1];
        x = values.x_pos[values.n - 1];
        y = values.y_pos[values.n - 1];

        if (out_array != null)
            out_array.Dispose();

        if (x_array != null)
            x_array.Dispose();

        if (y_array != null)
            y_array.Dispose();
    }

    private static void vips_values_add(VipsValues values, double v, int x, int y)
    {
        int i, j;

        // Find insertion point.
        for (i = 0; i < values.n; i++)
        {
            if (v < values.value[i])
                break;

            if (v == values.value[i])
            {
                if (y < values.y_pos[i])
                    break;

                if (y == values.y_pos[i])
                    if (x <= values.x_pos[i])
                        break;
            }
        }

        // Array full?
        if (values.n == values.size)
        {
            if (i > 0)
            {
                // We need to move stuff to the left to make space,
                // shunting the smallest out.
                for (j = 0; j < i - 1; j++)
                {
                    values.value[j] = values.value[j + 1];
                    values.x_pos[j] = values.x_pos[j + 1];
                    values.y_pos[j] = values.y_pos[j + 1];
                }
                values.value[i - 1] = v;
                values.x_pos[i - 1] = x;
                values.y_pos[i - 1] = y;
            }
        }
        else
        {
            // Not full, move stuff to the right into empty space.
            for (j = values.n; j > i; j--)
            {
                values.value[j] = values.value[j - 1];
                values.x_pos[j] = values.x_pos[j - 1];
                values.y_pos[j] = values.y_pos[j - 1];
            }
            values.value[i] = v;
            values.x_pos[i] = x;
            values.y_pos[i] = y;
            values.n += 1;
        }
    }

    public static VipsMax Create()
    {
        return new VipsMax();
    }
}

public class VipsValues
{
    public VipsMax max;
    public int size;
    public int n;
    public double[] value;
    public int[] x_pos;
    public int[] y_pos;

    public void Init(VipsMax max)
    {
        this.max = max;
        size = max.size;
        n = 0;
        value = new double[size];
        x_pos = new int[size];
        y_pos = new int[size];
    }
}
```

Note that I've assumed the existence of `VIPS_TYPE_ARRAY_DOUBLE` and `VIPS_TYPE_ARRAY_INT` types, which are not defined in the provided C code. You may need to modify the code to match your specific requirements.

Also, I've used `sbyte`, `ushort`, `short`, `uint`, and `int` instead of `signed char`, `unsigned short`, `signed short`, `unsigned int`, and `signed int` respectively, as they are equivalent in C#.

Please note that this is a direct translation from the provided C code, without any modifications or optimizations. You may need to review and modify it to fit your specific use case.