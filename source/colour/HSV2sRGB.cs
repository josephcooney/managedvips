```csharp
// Converted from vips_HSV2sRGB_line

public static void HSV2sRGBLine(VipsColour colour, VipsPel[] out, VipsPel[][] in, int width)
{
    byte[] p = (byte[])in[0];
    byte[] q = new byte[out.Length];

    for (int i = 0; i < width; i++)
    {
        float c, x, m;

        c = (p[2] * p[1]) / 255.0f;
        x = c * Math.Abs((p[0] % SIXTH_OF_CHAR) - 1);
        m = p[2] - c;

        if (p[0] < SIXTH_OF_CHAR)
        {
            q[0] = c + m;
            q[1] = x + m;
            q[2] = 0 + m;
        }
        else if (p[0] < (int)(2 * SIXTH_OF_CHAR))
        {
            q[0] = x + m;
            q[1] = c + m;
            q[2] = 0 + m;
        }
        else if (p[0] < (int)(3 * SIXTH_OF_CHAR))
        {
            q[0] = 0 + m;
            q[1] = c + m;
            q[2] = x + m;
        }
        else if (p[0] < (int)(4 * SIXTH_OF_CHAR))
        {
            q[0] = 0 + m;
            q[1] = x + m;
            q[2] = c + m;
        }
        else if (p[0] < (int)(5 * SIXTH_OF_CHAR))
        {
            q[0] = x + m;
            q[1] = 0 + m;
            q[2] = c + m;
        }
        else
        {
            q[0] = c + m;
            q[1] = 0 + m;
            q[2] = x + m;
        }

        p += 3;
        Array.Copy(q, 0, out, i * 3, 3);
    }
}

// Converted from vips_HSV2sRGB_class_init

public class HSV2sRGBClass : VipsColourCodeClass
{
    public static void HSV2sRGBClassInit(VipsHSV2sRGBClass* class)
    {
        VipsObjectClass objectClass = (VipsObjectClass)class;
        VipsColourClass colourClass = VIPS_COLOUR_CLASS(class);

        objectClass.nickname = "HSV2sRGB";
        objectClass.description = "transform HSV to sRGB";

        colourClass.processLine = HSV2sRGBLine;
    }
}

// Converted from vips_HSV2sRGB_init

public class HSV2sRGB : VipsColour
{
    public static void HSV2sRGBInit(VipsHSV2sRGB* hsv2srgb)
    {
        VipsColour colour = VIPS_COLOUR(hsv2srgb);
        VipsColourCode code = VIPS_COLOUR_CODE(hsv2srgb);

        colour.interpretation = VIPS_INTERPRETATION_sRGB;
        colour.format = VIPS_FORMAT_UCHAR;
        colour.bands = 3;
        colour.inputBands = 3;

        code.inputCoding = VIPS_CODING_NONE;
        code.inputFormat = VIPS_FORMAT_UCHAR;
        code.inputInterpretation = VIPS_INTERPRETATION_HSV;
    }
}

// Converted from vips_HSV2sRGB

public class HSV2sRGB : VipsImage
{
    public static int HSV2sRGB(VipsImage in, out VipsImage[] out)
    {
        return vipsCallSplit("HSV2sRGB", in, out);
    }
}
```