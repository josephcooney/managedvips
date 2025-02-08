```csharp
// vips_col_Ch2ab
public static void Ch2ab(float C, float h, ref float a, ref float b)
{
    a = C * (float)Math.Cos(VIPS_RAD(h));
    b = C * (float)Math.Sin(VIPS_RAD(h));
}

// Process a buffer of data.
static void LCh2Lab_line(VipsColour colour, float[] out, float[][] in, int width)
{
    float[] p = in[0];
    float[] q = new float[out.Length];

    for (int x = 0; x < width; x++)
    {
        float L = p[0];
        float C = p[1];
        float h = p[2];
        float a, b;

        Ch2ab(C, h, ref a, ref b);

        q[0] = L;
        q[1] = a;
        q[2] = b;

        Array.Copy(p, 3, p, 0, p.Length - 3);
        Array.Copy(q, 0, q, 0, q.Length);
    }
}

// vips_LCh2Lab_class_init
static void LCh2Lab_class_init()
{
    VipsObjectClass object_class = (VipsObjectClass)typeof(VipsLCh2Lab).GetTypeInfo().BaseType;
    VipsColourClass colour_class = (VipsColourClass)VipsColourClass.Get();

    object_class.Nickname = "LCh2Lab";
    object_class.Description = _("transform LCh to Lab");

    colour_class.ProcessLine = LCh2Lab_line;
}

// vips_LCh2Lab_init
static void LCh2Lab_init(VipsLCh2Lab LCh2Lab)
{
    VipsColour colour = (VipsColour)LCh2Lab;

    colour.Interpretation = VIPS_INTERPRETATION_LAB;
}

// vips_LCh2Lab
public static int LCh2Lab(VipsImage in, ref VipsImage out, params object[] args)
{
    return VipsCallSplit("LCh2Lab", in, ref out);
}
```