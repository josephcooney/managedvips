```csharp
// Converted from: vips_XYZ2Yxy_line

public class VipsXYZ2YxyLineProcessor : IColourProcessor
{
    public void Process(VipsImage image)
    {
        float[] p = (float[])image.GetPixelData(0);
        float[] q = new float[image.Width * 3];

        for (int i = 0; i < image.Width; i++)
        {
            float X = p[0];
            float Y = p[1];
            float Z = p[2];
            double total = X + Y + Z;

            float x, y;

            if (total == 0.0)
            {
                x = 0;
                y = 0;
            }
            else
            {
                x = X / total;
                y = Y / total;
            }

            q[i * 3] = Y;
            q[i * 3 + 1] = x;
            q[i * 3 + 2] = y;

            p += 3;
        }

        image.SetPixelData(q);
    }
}

// Converted from: vips_XYZ2Yxy_class_init

public class VipsXYZ2YxyClass : ColourTransform
{
    public override string Nickname => "XYZ2Yxy";
    public override string Description => _("transform XYZ to Yxy");

    public override IColourProcessor ProcessLine => new VipsXYZ2YxyLineProcessor();
}

// Converted from: vips_XYZ2Yxy_init

public class VipsXYZ2Yxy : ColourTransform
{
    protected override void Init()
    {
        Interpretation = VIPS_INTERPRETATION_YXY;
    }
}

// Converted from: vips_XYZ2Yxy

public class VipsXYZ2YxyProcessor : IImageProcessor
{
    public int Process(VipsImage image, out VipsImage output)
    {
        return ColourTransform.CallSplit("XYZ2Yxy", image, out output);
    }
}
```