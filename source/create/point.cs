```csharp
// vips_point_gen (from vips_point.c)

public abstract class VipsPoint : VipsCreate
{
    public override int Gen(VipsRegion out_region, object seq, object a, object b, bool[] stop)
    {
        VipsPoint point = (VipsPoint)a;
        VipsPointClass class_ = GetClass(point);
        VipsRect r = out_region.Valid;

        for (int y = 0; y < r.Height; y++)
        {
            int ay = r.Top + y;
            float[] q = (float[])VIPS_REGION_ADDR(out_region, r.Left, ay);

            for (int x = 0; x < r.Width; x++)
                q[x] = class_.Point(point, r.Left + x, ay);
        }

        return 0;
    }
}

// vips_point_build (from vips_point.c)

public abstract class VipsPoint : VipsCreate
{
    public override int Build(VipsObject object)
    {
        VipsCreate create = (VipsCreate)object;
        VipsPoint point = (VipsPoint)object;
        VipsPointClass class_ = GetClass(point);
        VipsImage[] t = (VipsImage[])vips_object_local_array(object, 4);

        VipsImage in;

        if (base.Build(object) != 0)
            return -1;

        t[0] = vips_image_new();
        vips_image_init_fields(t[0],
            point.Width, point.Height, 1,
            VIPS_FORMAT_FLOAT, VIPS_CODING_NONE, class_.Interpretation,
            1.0, 1.0);
        if (vips_image_pipelinev(t[0], VIPS_DEMAND_STYLE_ANY, null) ||
            vips_image_generate(t[0],
                null, vips_point_gen, null, point, null))
            return -1;
        in = t[0];

        if (point.Uchar)
        {
            float min = class_.Min;
            float max = class_.Max;
            float range = max - min;

            if (vips_linear1(in, ref t[2],
                    255.0 / range, -min * 255.0 / range,
                    "uchar", true,
                    null))
                return -1;
            in = t[2];

            /* We don't want FOURIER or whatever in this case.
             */
            in.Type = VIPS_INTERPRETATION_MULTIBAND;
        }

        if (vips_image_write(in, create.Out))
            return -1;

        return 0;
    }
}

// vips_point_class_init (from vips_point.c)

public abstract class VipsPointClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // nickname = "point";
        // description = _("make a point image");
        // build = vips_point_build;

        Point = null;
        Min = -1.0f;
        Max = 1.0f;
        Interpretation = VIPS_INTERPRETATION_MULTIBAND;

        VIPS_ARG_INT(this, "width", 2,
            _("Width"),
            _("Image width in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsPoint).GetField("Width").Offset);

        VIPS_ARG_INT(this, "height", 3,
            _("Height"),
            _("Image height in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsPoint).GetField("Height").Offset);

        VIPS_ARG_BOOL(this, "uchar", 4,
            _("Uchar"),
            _("Output an unsigned char image"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsPoint).GetField("Uchar").Offset);
    }
}

// vips_point_init (from vips_point.c)

public class VipsPoint : VipsCreate
{
    public VipsPoint()
    {
        // point->width = 1;
        // point->height = 1;
        // point->uchar = FALSE;
    }
}
```