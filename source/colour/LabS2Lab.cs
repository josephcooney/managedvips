```csharp
// LabS2Lab()
//
// 12/12/02 JC
// 	- adapted from im_LabS2LabQ()
// 2/11/09
// 	- gtkdoc, cleanup

using System;

public class VipsLabS2Lab : VipsColourCode
{
    public static void LabS2Lab_line(VipsColour colour, float[] out, short[][] in, int width)
    {
        short[] p = in[0];
        float[] q = out;
        int i;

        for (i = 0; i < width; i++)
        {
            // Convert n pels from signed short to Lab.
            q[0] = p[0] / (32767.0f / 100.0f);
            q[1] = p[1] / (32768.0f / 128.0f);
            q[2] = p[2] / (32768.0f / 128.0f);

            p += 3;
            q += 3;
        }
    }

    public static void LabS2Lab_class_init(VipsLabS2LabClass class_)
    {
        VipsObjectClass object_class = (VipsObjectClass)class_;
        VipsColourClass colour_class = VIPS_COLOUR_CLASS(class_);

        object_class.nickname = "LabS2Lab";
        object_class.description = "transform signed short Lab to float";

        colour_class.process_line = LabS2Lab_line;
    }

    public static void LabS2Lab_init(VipsLabS2Lab LabS2Lab)
    {
        VipsColour colour = VIPS_COLOUR(LabS2Lab);
        VipsColourCode code = VIPS_COLOUR_CODE(LabS2Lab);

        colour.interpretation = VIPS_INTERPRETATION_LAB;
        colour.format = VIPS_FORMAT_FLOAT;
        colour.input_bands = 3;
        colour.bands = 3;

        code.input_coding = VIPS_CODING_NONE;
        code.input_format = VIPS_FORMAT_SHORT;
    }

    public static int LabS2Lab(VipsImage in, ref VipsImage out)
    {
        return vips_call_split("LabS2Lab", in, ref out);
    }
}
```