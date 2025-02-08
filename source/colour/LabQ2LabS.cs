```csharp
// vips_LabQ2LabS_line (from vips_LabQ2LabS_line())

public static void LabQ2LabSLine(VipsColour colour, short[] out, byte[][] in, int width)
{
    byte[] p = in[0];
    short[] q = out;

    int i;
    byte ext;
    short l, a, b;

    for (i = 0; i < width; i++)
    {
        // Get most significant 8 bits of lab.
        l = (short)(p[0] << 7);
        a = (short)(p[1] << 8);
        b = (short)(p[2] << 8);

        // Get x-tra bits.
        ext = p[3];
        p += 4;

        // Shift and mask in to lab.
        l |= (byte)((ext & 0xc0) >> 1);
        a |= (byte)((ext & 0x38) << 2);
        b |= (byte)((ext & 0x7) << 5);

        // Write!
        q[0] = l;
        q[1] = a;
        q[2] = b;
        q += 3;
    }
}

// vips_LabQ2LabS_class_init (from vips_LabQ2LabS_class_init())

public class LabQ2LabSClass : VipsColourClass
{
    public static void ClassInit()
    {
        VipsObjectClass object_class = (VipsObjectClass)typeof(LabQ2LabSClass).GetBaseType();
        VipsColourClass colour_class = typeof(VipsColourClass);

        object_class.Nickname = "LabQ2LabS";
        object_class.Description = _("unpack a LabQ image to short Lab");

        colour_class.ProcessLine = new Func<VipsColour, byte[][], short[], int>(LabQ2LabSLine);
    }
}

// vips_LabQ2LabS_init (from vips_LabQ2LabS_init())

public class LabQ2LabS : VipsColour
{
    public static void Init()
    {
        VipsColour colour = new VipsColour();
        VipsColourCode code = new VipsColourCode();

        colour.Coding = VIPS_CODING_NONE;
        colour.Interpretation = VIPS_INTERPRETATION_LABS;
        colour.Format = VIPS_FORMAT_SHORT;
        colour.Bands = 3;

        code.InputCoding = VIPS_CODING_LABQ;
    }
}

// vips_LabQ2LabS (from vips_LabQ2LabS())

public static int LabQ2LabS(VipsImage in, out VipsImage[] out)
{
    return VipsCallSplit("LabQ2LabS", in, out);
}
```