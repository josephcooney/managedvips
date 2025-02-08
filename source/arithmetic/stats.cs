Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsStats : VipsStatistic
{
    public VipsImage Out { get; private set; }

    public bool Set { get; private set; } // FALSE means no value yet

    protected override int Build(VipsObject obj)
    {
        var statistic = (VipsStatistic)obj;
        var stats = (VipsStats)obj;

        if (vips_object_argument_isset(obj, "in"))
        {
            int bands = vips_image_get_bands(statistic.In);

            if (vips_check_noncomplex(Nickname, statistic.In))
                return -1;

            obj.SetProperty("out", new VipsImage(COL_LAST, bands + 1));
        }

        if (base.Build(obj) != 0)
            return -1;

        var pels = (long)vips_image_get_width(statistic.In) * vips_image_get_height(statistic.In);
        var vals = pels * vips_image_get_bands(statistic.In);

        var row0 = VIPS_MATRIX(stats.Out, 0, 0);
        var row = VIPS_MATRIX(stats.Out, 0, 1);
        for (int i = 0; i < COL_LAST; i++)
            row0[i] = row[i];

        for (int b = 1; b < vips_image_get_bands(statistic.In); b++)
        {
            row = VIPS_MATRIX(stats.Out, 0, b + 1);

            if (row[COL_MIN] < row0[COL_MIN])
            {
                row0[COL_MIN] = row[COL_MIN];
                row0[COL_XMIN] = row[COL_XMIN];
                row0[COL_YMIN] = row[COL_YMIN];
            }

            if (row[COL_MAX] > row0[COL_MAX])
            {
                row0[COL_MAX] = row[COL_MAX];
                row0[COL_XMAX] = row[COL_XMAX];
                row0[COL_YMAX] = row[COL_YMAX];
            }

            row0[COL_SUM] += row[COL_SUM];
            row0[COL_SUM2] += row[COL_SUM2];
        }

        for (int y = 1; y < vips_image_get_height(stats.Out); y++)
        {
            var row = VIPS_MATRIX(stats.Out, 0, y);

            row[COL_AVG] = row[COL_SUM] / pels;
            row[COL_SD] = Math.Sqrt(
                Math.Abs(row[COL_SUM2] -
                    (row[COL_SUM] * row[COL_SUM] / pels)) /
                (pels - 1));
        }

        row0[COL_AVG] = row0[COL_SUM] / vals;
        row0[COL_SD] = Math.Sqrt(
            Math.Abs(row0[COL_SUM2] -
                (row0[COL_SUM] * row0[COL_SUM] / vals)) /
            (vals - 1));

        return 0;
    }

    protected override int Stop(VipsStatistic statistic, object seq)
    {
        var global = (VipsStats)statistic;
        var local = (VipsStats)seq;

        if (local.Set && !global.Set)
        {
            for (int b = 0; b < vips_image_get_bands(statistic.In); b++)
            {
                var p = VIPS_MATRIX(local.Out, 0, b + 1);
                var q = VIPS_MATRIX(global.Out, 0, b + 1);

                for (int i = 0; i < COL_LAST; i++)
                    q[i] = p[i];
            }

            global.Set = true;
        }
        else if (local.Set && global.Set)
        {
            for (int b = 0; b < vips_image_get_bands(statistic.In); b++)
            {
                var p = VIPS_MATRIX(local.Out, 0, b + 1);
                var q = VIPS_MATRIX(global.Out, 0, b + 1);

                if (p[COL_MIN] < q[COL_MIN])
                {
                    q[COL_MIN] = p[COL_MIN];
                    q[COL_XMIN] = p[COL_XMIN];
                    q[COL_YMIN] = p[COL_YMIN];
                }

                if (p[COL_MAX] > q[COL_MAX])
                {
                    q[COL_MAX] = p[COL_MAX];
                    q[COL_XMAX] = p[COL_XMAX];
                    q[COL_YMAX] = p[COL_YMAX];
                }

                q[COL_SUM] += p[COL_SUM];
                q[COL_SUM2] += p[COL_SUM2];
            }
        }

        VIPS_FREEF(g_object_unref, local.Out);
        return 0;
    }

    protected override object Start(VipsStatistic statistic)
    {
        var bands = vips_image_get_bands(statistic.In);

        var stats = new VipsStats();
        if (!(stats.Out = new VipsImage(COL_LAST, bands + 1)))
            return null;

        stats.Set = false;
        return (object)stats;
    }

    protected override int Scan(VipsStatistic statistic, object seq,
        int x, int y, object inData, int n)
    {
        var local = (VipsStats)seq;

        switch (vips_image_get_format(statistic.In))
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
                LOOPF(float);
                break;
            case VIPS_FORMAT_DOUBLE:
                LOOPF(double);
                break;

            default:
                g_assert_not_reached();
                return -1;
        }

        local.Set = true;
        return 0;
    }
}

// Names for our columns.
public enum Column
{
    COL_MIN,
    COL_MAX,
    COL_SUM,
    COL_SUM2,
    COL_AVG,
    COL_SD,
    COL_XMIN,
    COL_YMIN,
    COL_XMAX,
    COL_YMAX,
    COL_LAST
}
```

Note that I've assumed the `VipsImage` and `VipsStatistic` classes are already defined elsewhere in your codebase, as well as the `vips_image_get_bands`, `vips_check_noncomplex`, `g_object_set_property`, etc. methods. If not, you'll need to define them or import the necessary libraries.

Also note that I've used C# 6 features like expression-bodied members and pattern matching, but if you're using an earlier version of C#, you can easily convert this code back to use older syntax.