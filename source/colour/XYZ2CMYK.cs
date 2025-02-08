Here is the converted C# code:

```csharp
// Converted from: vips_XYZ2CMYK_process

public class VipsXYZ2CMYK : VipsColourCode
{
    public override int Process(VipsImage inImage, out VipsImage[] outImages)
    {
        return VipsIccExport(inImage, out outImages,
            "output_profile", "cmyk",
            "pcs", VipsPcs.XYZ,
            null);
    }
}

// Converted from: vips_XYZ2CMYK_build

public class VipsXYZ2CMYK : VipsColourCode
{
    public override int Build(VipsObject obj)
    {
        VipsImage outImage;
        VipsImage t;

        if (base.Build(obj) != 0)
            return -1;

        outImage = new VipsImage();
        obj.SetProperty("out", outImage);

        if (VipsColourspaceProcessN("XYZ2CMYK",
                ((VipsXYZ2CMYK)obj).In, ref t, 3, Process))
            return -1;
        if (!t.Write(outImage))
        {
            VipsObject.Unref(t);
            return -1;
        }
        VipsObject.Unref(t);

        return 0;
    }

    public override string Nickname => "XYZ2CMYK";
    public override string Description => _("transform XYZ to CMYK");

    [VipsArg("in", 1, "Input")]
    public VipsImage In { get; set; }

    [VipsArg("out", 100, "Output")]
    public VipsImage Out { get; set; }
}

// Converted from: vips_XYZ2CMYK_line

public class VipsXYZ2CMYK : VipsColourCode
{
    public override void ProcessLine(VipsColour colour, VipsPel[] outPel, VipsPel[][] inPels, int width)
    {
        float[] p = (float[])inPels[0];
        byte[] q = new byte[outPel.Length];

        const float epsilon = 0.00001;

        for (int i = 0; i < width; i++)
        {
            float r = p[0] / VipsD65_X0;
            float g = p[1] / VipsD65_Y0;
            float b = p[2] / VipsD65_Z0;

            float c = 1.0f - r;
            float m = 1.0f - g;
            float y = 1.0f - b;
            float k = Math.Min(c, Math.Min(m, y));
            float ik = 1.0f - k;

            if (ik < epsilon)
            {
                q[0] = 255;
                q[1] = 255;
                q[2] = 255;
                q[3] = 255;
            }
            else
            {
                q[0] = (byte)Math.Min(255 * (c - k) / ik, 255);
                q[1] = (byte)Math.Min(255 * (m - k) / ik, 255);
                q[2] = (byte)Math.Min(255 * (y - k) / ik, 255);
                q[3] = (byte)Math.Min(255 * k, 255);
            }

            p += 3;
            q += 4;
        }
    }

    public override string Nickname => "XYZ2CMYK";
    public override string Description => _("transform XYZ to CMYK");
}

// Converted from: vips_XYZ2CMYK

public class VipsXYZ2CMYK : VipsColourCode
{
    public static int Process(VipsImage inImage, out VipsImage[] outImages)
    {
        var ap = new va_list();
        return VipsCallSplit("XYZ2CMYK", ap, inImage, out outImages);
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsIccExport`, `VipsColourspaceProcessN`, `VipsObject`, etc.) which are not defined here. You will need to ensure these exist and are correctly implemented in your code for this conversion to work.