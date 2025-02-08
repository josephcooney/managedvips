```csharp
// vips_Yxy2XYZ_line: (method)
// @colour: input image
// @out: output image
// @in: input image
// @width: width of the line to process
public static void vips_Yxy2XYZ_line(VipsColour colour, float[] out, float[][] in, int width)
{
    float[] p = in[0];
    float[] q = new float[3];

    for (int i = 0; i < width; i++)
    {
        // Yxy to XYZ colourspace
        float Y = p[0];
        float x = p[1];
        float y = p[2];

        float X, Z;

        if (x == 0.0 || y == 0.0)
        {
            X = 0.0;
            Z = 0.0;
        }
        else
        {
            double total;

            total = Y / y;
            X = x * total;
            Z = (X - x * X - x * Y) / x;
        }

        q[0] = X;
        q[1] = Y;
        q[2] = Z;

        p += 3;
        out[i] = q[0];
    }
}

// vips_Yxy2XYZ_class_init: (method)
// @class: class to initialize
public static void vips_Yxy2XYZ_class_init(VipsYxy2XYZClass class)
{
    VipsObjectClass object_class = (VipsObjectClass)class;
    VipsColourClass colour_class = VIPS_COLOUR_CLASS(class);

    object_class.nickname = "Yxy2XYZ";
    object_class.description = _("transform Yxy to XYZ");

    colour_class.process_line = vips_Yxy2XYZ_line;
}

// vips_Yxy2XYZ_init: (method)
// @Yxy2XYZ: instance to initialize
public static void vips_Yxy2XYZ_init(VipsYxy2XYZ Yxy2XYZ)
{
    VipsColour colour = VIPS_COLOUR(Yxy2XYZ);

    colour.interpretation = VIPS_INTERPRETATION_XYZ;
}

// vips_Yxy2XYZ: (method)
// @in: input image
// @out: output image
// @...: %NULL-terminated list of optional named arguments
public static int vips_Yxy2XYZ(VipsImage in, ref VipsImage out, params object[] args)
{
    return vips_call_split("Yxy2XYZ", in, ref out);
}
```