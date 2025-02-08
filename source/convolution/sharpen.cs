Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsSharpen : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }

    public double Sigma { get; set; } = 0.5;
    public double X1 { get; set; } = 2.0;
    public double Y2 { get; set; } = 10.0;
    public double Y3 { get; set; } = 20.0;
    public double M1 { get; set; } = 0.0;
    public double M2 { get; set; } = 3.0;

    private int[] Lut { get; set; }

    protected override int Build(VipsObject object)
    {
        VipsImage t = new VipsImage();
        VipsImage args = new VipsImage();

        // We used to have a radius control. If that's set but sigma isn't,
        // use it to set a reasonable value for sigma.
        if (!VipsArgument.IsSet(object, "sigma") && VipsArgument.IsSet(object, "radius"))
            Sigma = 1 + Radius / 2;

        In = object.In;

        // Extract L and the rest, convolve L.
        if (VipsExtractBand(In, args, 0, null) ||
            VipsExtractBand(In, t, 1, "n", In.Bands - 1, null) ||
            VipsConvSep(args, t, new VipsImage(), 
                "precision", VipsPrecision.Integer,
                null))
        {
            return -1;
        }

        // Make sure we're short (need this for the LUT) and not eg. float LABS.
        if (VipsCastShort(In, t, null))
        {
            return -1;
        }
        In = t;

        // Index with the signed difference between two 0 - 32767 images.
        if (!(Lut = new int[65536]))
        {
            return -1;
        }

        for (int i = 0; i < 65536; i++)
        {
            double v = (i - 32767) / 327.67;

            if (v < -X1)
                // Left of -x1.
                Lut[i] = VipsRINT((v + X1) * M2 - X1 * M1);
            else if (v < X1)
                // Centre section.
                Lut[i] = VipsRINT(v * M1);
            else
                // Right of x1.
                Lut[i] = VipsRINT((v - X1) * M2 + X1 * M1);

            if (Lut[i] < -Y3)
                Lut[i] = -Y3;
            if (Lut[i] > Y2)
                Lut[i] = Y2;
        }

        // Reattach the rest.
        VipsImage t6 = new VipsImage();
        if (VipsBandJoin2(t, t, t6, null) ||
            VipsColourspace(t6, t, In.Type, null) ||
            VipsImageWrite(t, Out))
        {
            return -1;
        }

        return 0;
    }
}

public class VipsSharpenClass : VipsOperationClass
{
    public static void Register(VipsObjectClass class_)
    {
        // ...
    }
}

public class VipsSharpenInit : VipsOperationInit
{
    public override void Init(VipsSharpen sharpen)
    {
        sharpen.Sigma = 0.5;
        sharpen.X1 = 2.0;
        sharpen.Y2 = 10.0;
        sharpen.Y3 = 20.0;
        sharpen.M1 = 0.0;
        sharpen.M2 = 3.0;
    }
}

public class VipsSharpenGenerate : VipsOperationGenerate
{
    public override int Generate(VipsRegion out_region, object vseq, object a, object b, bool stop)
    {
        // ...
    }
}
```

Note that this is not an exact translation of the C code, but rather a reimplementation in C#. Some changes were made to adapt the code to the C# syntax and conventions.