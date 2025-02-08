```csharp
// dE76.cs

using System;

public class VipsdE76 : VipsColourDifference
{
    // Pythagorean distance between two points in colour space. Lab/XYZ/CMC etc.
    public float vips_pythagoras(float L1, float a1, float b1, float L2, float a2, float b2)
    {
        float dL = L1 - L2;
        float da = a1 - a2;
        float db = b1 - b2;

        return (float)Math.Sqrt(dL * dL + da * da + db * db);
    }

    // Find the difference between two buffers of LAB data.
    public void vips__pythagoras_line(VipsColour colour, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        float[] p1 = (float[])inArray[0];
        float[] p2 = (float[])inArray[1];
        float[] q = (float[])outArray;

        for (int x = 0; x < width; x++)
        {
            float dL = p1[0] - p2[0];
            float da = p1[1] - p2[1];
            float db = p1[2] - p2[2];

            q[x] = (float)Math.Sqrt(dL * dL + da * da + db * db);

            p1 += 3;
            p2 += 3;
        }
    }

    // Class initialization
    public static void vips_dE76_class_init(VipsdE76Class class)
    {
        VipsObjectClass object_class = (VipsObjectClass)class;
        VipsColourClass colour_class = VIPS_COLOUR_CLASS(class);

        object_class.nickname = "dE76";
        object_class.description = "calculate dE76";

        colour_class.process_line = vips__pythagoras_line;
    }

    // Object initialization
    public void vips_dE76_init()
    {
        VipsColourDifference difference = (VipsColourDifference)this;

        difference.interpretation = VIPS_INTERPRETATION_LAB;
    }
}

// Calculate dE 76.
public int vips_dE76(VipsImage left, VipsImage right, ref VipsImage out)
{
    return VipsCallSplit("dE76", left, right, ref out);
}
```