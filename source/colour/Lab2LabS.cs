```csharp
// vips_Lab2LabS_line: convert a line of float Lab to signed short format
public static void vips_Lab2LabS_line(VipsImage colour, VipsPel[] out, VipsPel[][] in)
{
    float[] p = (float[])in[0];
    short[] q = (short[])out;
    int i;

    for (i = 0; i < colour.Width; i++)
    {
        // Lab to LabS conversion
        q[0] = (short)VIPS_CLIP(0, (int)(p[0] * (32767.0 / 100.0)), short.MaxValue);
        q[1] = (short)VIPS_CLIP(short.MinValue, (int)(p[1] * (32768.0 / 128.0)), short.MaxValue);
        q[2] = (short)VIPS_CLIP(short.MinValue, (int)(p[2] * (32768.0 / 128.0)), short.MaxValue);

        q += 3;
        p += 3;
    }
}

// vips_Lab2LabS_class_init: class initialization
public static void vips_Lab2LabS_class_init(VipsColourClass colourClass)
{
    VipsObjectClass objectClass = (VipsObjectClass)colourClass;

    objectClass.Nickname = "Lab2LabS";
    objectClass.Description = "transform float Lab to signed short";

    colourClass.ProcessLine = vips_Lab2LabS_line;
}

// vips_Lab2LabS_init: instance initialization
public static void vips_Lab2LabS_init(VipsColour colour)
{
    VipsImage image = (VipsImage)colour;

    image.Interpretation = VIPS_INTERPRETATION_LABS;
    image.Format = VIPS_FORMAT_SHORT;
    image.InputBands = 3;
    image.Bands = 3;
}

// vips_Lab2LabS: transform Lab to LabS
public static int vips_Lab2LabS(VipsImage in, out VipsImage[] out)
{
    return vips_call_split("Lab2LabS", in, out);
}
```