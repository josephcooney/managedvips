```csharp
// C method: colr_color
public static void ColrColor(float[] color, byte[] clr)
{
    if (clr[3] == 0)
        color[0] = color[1] = color[2] = 0.0f;
    else
    {
        double f = Math.Pow(10, clr[3] - (128 + 8));

        color[0] = (clr[0] + 0.5f) * f;
        color[1] = (clr[1] + 0.5f) * f;
        color[2] = (clr[2] + 0.5f) * f;
    }
}

// C method: vips_rad2float_line
public static void VipsRad2FloatLine(VipsColour colour, float[] out, byte[][] in, int width)
{
    byte[] inp = in[0];
    float[] outbuf = out;

    for (int i = 0; i < width; i++)
        ColrColor(outbuf, inp);
}

// C method: vips_rad2float_class_init
public class VipsRad2FloatClass : VipsColourCodeClass
{
    public static void VipsRad2FloatClassInit(VipsRad2FloatClass class_)
    {
        VipsObjectClass object_class = (VipsObjectClass)class_;
        VipsColourClass colour_class = VIPS_COLOUR_CLASS(class_);

        object_class.nickname = "rad2float";
        object_class.description = _("unpack Radiance coding to float RGB");

        colour_class.process_line = VipsRad2FloatLine;
    }
}

// C method: vips_rad2float_init
public class VipsRad2Float : VipsColourCode
{
    public static void VipsRad2FloatInit(VipsRad2Float rad2float)
    {
        VipsColour colour = VIPS_COLOUR(rad2float);
        VipsColourCode code = VIPS_COLOUR_CODE(rad2float);

        colour.coding = VIPS_CODING_NONE;
        colour.interpretation = VIPS_INTERPRETATION_scRGB;
        colour.format = VIPS_FORMAT_FLOAT;
        colour.bands = 3;

        code.input_coding = VIPS_CODING_RAD;
    }
}

// C method: vips_rad2float
public static int VipsRad2Float(VipsImage in, ref VipsImage out)
{
    return VipsCallSplit("rad2float", in, ref out);
}
```