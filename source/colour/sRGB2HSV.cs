```csharp
// vips_sRGB2HSV_line (from LabS2Lab.c)

public static void vips_sRGB2HSV_line(VipsColour colour, VipsPel[] out, VipsPel[][] in, int width)
{
    byte[] p = (byte[])in[0];
    byte[] q = new byte[out.Length];

    for (int i = 0; i < width; i++)
    {
        byte c_max;
        byte c_min;
        float secondary_diff;
        float wrap_around_hue;

        if (p[1] < p[2])
        {
            if (p[2] < p[0])
            {
                /* Center red (at top).
                 */
                c_max = p[0];
                c_min = p[1];
                secondary_diff = p[1] - p[2];
                wrap_around_hue = 255.0f;
            }
            else
            {
                /* Center blue.
                 */
                c_max = p[2];
                c_min = Math.Min(p[1], p[0]);
                secondary_diff = p[0] - p[1];
                wrap_around_hue = 170.0f;
            }
        }
        else
        {
            if (p[1] < p[0])
            {
                /* Center red (at bottom)
                 */
                c_max = p[0];
                c_min = p[2];
                secondary_diff = p[1] - p[2];
                wrap_around_hue = 0.0f;
            }
            else
            {
                /* Center green
                 */
                c_max = p[1];
                c_min = Math.Min(p[2], p[0]);
                secondary_diff = p[2] - p[0];
                wrap_around_hue = 85.0f;
            }
        }

        if (c_max == 0)
        {
            q[0] = 0;
            q[1] = 0;
            q[2] = 0;
        }
        else
        {
            byte delta;

            q[2] = c_max;
            delta = c_max - c_min;

            if (delta == 0)
                q[0] = 0;
            else
                q[0] = 42.5f * (secondary_diff / delta) + wrap_around_hue;

            q[1] = delta * 255.0f / c_max;
        }

        p = new byte[p.Length + 3];
        p.CopyTo(in[0], 0);
        p[0] = in[0][i * 3 + 0];
        p[1] = in[0][i * 3 + 1];
        p[2] = in[0][i * 3 + 2];

        q = new byte[q.Length + 3];
        q.CopyTo(out, i * 3);
    }
}

// vips_sRGB2HSV_class_init (from LabS2Lab.c)

public class VipssRGB2HSVClass : VipsColourCodeClass
{
    public static void vips_sRGB2HSV_class_init(VipssRGB2HSVClass* class)
    {
        VipsObjectClass object_class = (VipsObjectClass)class;
        VipsColourClass colour_class = VIPS_COLOUR_CLASS(class);

        object_class.nickname = "sRGB2HSV";
        object_class.description = _("transform sRGB to HSV");

        colour_class.process_line = vips_sRGB2HSV_line;
    }
}

// vips_sRGB2HSV_init (from LabS2Lab.c)

public class VipssRGB2HSV : VipsColour
{
    public static void vips_sRGB2HSV_init(VipssRGB2HSV* sRGB2HSV)
    {
        VipsColour colour = VIPS_COLOUR(sRGB2HSV);
        VipsColourCode code = VIPS_COLOUR_CODE(sRGB2HSV);

        colour.interpretation = VIPS_INTERPRETATION_HSV;
        colour.format = VIPS_FORMAT_UCHAR;
        colour.bands = 3;
        colour.input_bands = 3;

        code.input_coding = VIPS_CODING_NONE;
        code.input_format = VIPS_FORMAT_UCHAR;
        code.input_interpretation = VIPS_INTERPRETATION_sRGB;
    }
}

// vips_sRGB2HSV (from LabS2Lab.c)

public class VipssRGB2HSV : VipsColour
{
    public static int vips_sRGB2HSV(VipsImage in, out VipsImage[] out)
    {
        // ... (rest of the method remains the same)
    }
}
```