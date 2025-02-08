```csharp
// vips_invertlut_dispose: (skip)
public class VipsInvertlutDispose : IDisposable
{
    public void Dispose()
    {
        // VIPS_FREE(lut->data);
        // VIPS_FREE(lut->buf);
        // VIPS_UNREF(lut->mat);
    }
}

// vips_invertlut_build_init: (skip)
public class VipsInvertlutBuildInit
{
    public static int Build(VipsInvertlut lut)
    {
        VipsObjectClass class = VipsObject.GetClass(lut);

        if (!lut.Mat || lut.Mat.Xsize < 2 || lut.Mat.Ysize < 1)
        {
            // vips_error(class->nickname, "%s", _("bad input matrix"));
            return -1;
        }
        if (lut.Size < 1 || lut.Size > 65536)
        {
            // vips_error(class->nickname, "%s", _("bad size"));
            return -1;
        }

        double[] buf = new double[lut.Size * (lut.Mat.Xsize - 1)];
        double[][] data = new double[lut.Mat.Ysize][];

        for (int y = 0; y < lut.Mat.Ysize; y++)
            data[y] = VipsMatrix.GetRow(lut.Mat, 0, y);

        // Sanity check for data range.
        for (int y = 0; y < lut.Mat.Ysize; y++)
            for (int x = 0; x < lut.Mat.Xsize; x++)
                if (data[y][x] > 1.0 || data[y][x] < 0.0)
                {
                    // vips_error(class->nickname, _("element (%d, %d) is %g, outside range [0,1]"), x, y, lut->data[y][x]);
                    return -1;
                }

        // Sort by 1st column in input.
        Array.Sort(data, (a, b) => a[0].CompareTo(b[0]));

        return 0;
    }
}

// vips_invertlut_build_create: (skip)
public class VipsInvertlutBuildCreate
{
    public static int Build(VipsInvertlut lut)
    {
        int bands = lut.Mat.Xsize - 1;
        int height = lut.Mat.Ysize;

        for (int b = 0; b < bands; b++)
        {
            // The first and last lut positions we know real values for.
            int first = (int)(lut.Data[0][b + 1] * (lut.Size - 1));
            int last = (int)(lut.Data[height - 1][b + 1] * (lut.Size - 1));

            // Extrapolate bottom and top segments to (0,0) and (1,1).
            for (int k = 0; k < first; k++)
            {
                double fac = lut.Data[0][0] / first;
                lut.Buf[b + k * bands] = k * fac;
            }

            for (int k = last; k < lut.Size; k++)
            {
                double fac = (1 - lut.Data[height - 1][0]) /
                    ((lut.Size - 1) - last);
                lut.Buf[b + k * bands] =
                    lut.Data[height - 1][0] + (k - last) * fac;
            }

            // Interpolate the data sections.
            for (int k = first; k < last; k++)
            {
                double ki = (double)k / (lut.Size - 1);

                double irange, orange;
                int j;

                // Search for the lowest real value < ki. There may
                // not be one: if not, just use 0. Tiny error.
                for (j = height - 1; j >= 0; j--)
                    if (lut.Data[j][b + 1] < ki)
                        break;
                if (j == -1)
                    j = 0;

                // Interpolate k as being between row data[j] and row
                // data[j + 1].
                irange = lut.Data[j + 1][b + 1] - lut.Data[j][b + 1];
                orange = lut.Data[j + 1][0] - lut.Data[j][0];

                lut.Buf[b + k * bands] = lut.Data[j][0] +
                    orange * ((ki - lut.Data[j][b + 1]) / irange);
            }
        }

        return 0;
    }
}

// vips_invertlut_build: (skip)
public class VipsInvertlutBuild
{
    public static int Build(VipsObject object)
    {
        VipsObjectClass class = VipsObject.GetClass(object);
        VipsCreate create = VipsCreate.GetObject(object);
        VipsInvertlut lut = (VipsInvertlut)object;

        if (VIPS_OBJECT_CLASS(vips_invertlut_parent_class).Build(object))
            return -1;

        if (!vips_check_matrix(class.Nickname, lut.In, ref lut.Mat))
            return -1;

        if (vips_invertlut_build_init(lut) || vips_invertlut_build_create(lut))
            return -1;

        VipsImage.InitFields(create.Out,
            lut.Size, 1, lut.Mat.Xsize - 1,
            VIPS_FORMAT_DOUBLE, VIPS_CODING_NONE,
            VIPS_INTERPRETATION_HISTOGRAM, 1.0, 1.0);
        if (vips_image_write_line(create.Out, 0, lut.Buf))
            return -1;

        return 0;
    }
}

// vips_invertlut_class_init: (skip)
public class VipsInvertlutClass : VipsObjectClass
{
    public static void ClassInit()
    {
        GObjectClass gobject_class = G_OBJECT_CLASS(VipsInvertlutClass);
        VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(VipsInvertlutClass);

        gobject_class.Dispose += new DisposeEventHandler(vips_invertlut_dispose);
        gobject_class.SetProperty += new SetPropertyEventHandler(vips_object_set_property);
        gobject_class.GetProperty += new GetPropertyEventHandler(vips_object_get_property);

        vobject_class.Nickname = "invertlut";
        vobject_class.Description = _("build an inverted look-up table");
        vobject_class.Build = new BuildEventHandler(vips_invertlut_build);

        VIPS_ARG_IMAGE(VipsInvertlutClass, "in", 0,
            _("Input"),
            _("Matrix of XY coordinates"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsInvertlut, in));

        VIPS_ARG_INT(VipsInvertlutClass, "size", 5,
            _("Size"),
            _("LUT size to generate"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsInvertlut, size),
            1, 1000000, 256);
    }
}

// vips_invertlut_init: (skip)
public class VipsInvertlutInit
{
    public static void Init(VipsInvertlut lut)
    {
        lut.Size = 256;
    }
}

// vips_invertlut: (method)
public class VipsInvertlutMethod : VipsObject
{
    public int Invoke(VipsImage in_image, ref VipsImage out_image, params object[] args)
    {
        // va_list ap;
        // int result;

        // va_start(ap, out);
        // result = vips_call_split("invertlut", ap, in_image, ref out_image);
        // va_end(ap);

        return 0;
    }
}
```