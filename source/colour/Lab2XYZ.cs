Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

public class LabToXyz : ColourTransform
{
    public LabToXyz()
    {
        // default to D65
        X0 = VipsD65.X0;
        Y0 = VipsD65.Y0;
        Z0 = VipsD65.Z0;
    }

    public double X0 { get; set; }
    public double Y0 { get; set; }
    public double Z0 { get; set; }

    public override void ProcessLine(VipsPel[] inArray, VipsPel[] outArray)
    {
        // vips_Lab2XYZ_line
        for (int x = 0; x < inArray.Length; x++)
        {
            float L = inArray[x].L;
            float a = inArray[x].A;
            float b = inArray[x].B;

            float X, Y, Z;
            vips_col_Lab2XYZ_helper(L, a, b, out X, out Y, out Z);

            // Write.
            outArray[x] = new VipsPel(X, Y, Z);
        }
    }

    private void vips_col_Lab2XYZ_helper(float L, float a, float b, out float X, out float Y, out float Z)
    {
        double cby, tmp;

        if (L < 8.0)
        {
            Y = (L * Y0) / 903.3;
            cby = 7.787 * (Y / Y0) + 16.0 / 116.0;
        }
        else
        {
            cby = (L + 16.0) / 116.0;
            Y = Y0 * Math.Pow(cby, 3);
        }

        tmp = a / 500.0 + cby;
        if (tmp < 0.2069)
            X = X0 * (tmp - 0.13793) / 7.787;
        else
            X = X0 * Math.Pow(tmp, 3);

        tmp = cby - b / 200.0;
        if (tmp < 0.2069)
            Z = Z0 * (tmp - 0.13793) / 7.787;
        else
            Z = Z0 * Math.Pow(tmp, 3);
    }

    public override int Build(VipsObject obj)
    {
        // vips_Lab2XYZ_build
        if (temp != null && temp.Length != 3)
            return -1;

        X0 = ((double[])temp.Data)[0];
        Y0 = ((double[])temp.Data)[1];
        Z0 = ((double[])temp.Data)[2];

        return 0;
    }

    public override void ClassInit()
    {
        // vips_Lab2XYZ_class_init
        base.ClassInit();

        VipsObjectClass objClass = (VipsObjectClass)GetType();
        VipsColourClass colourClass = (VipsColourClass)typeof(ColourTransform);

        objClass.Nickname = "LabToXyz";
        objClass.Description = "transform CIELAB to XYZ";

        colourClass.ProcessLine = ProcessLine;

        VipsArgBoxed tempArg = new VipsArgBoxed("temp", 110, "Temperature", "Color temperature", VIPS_ARGUMENT_OPTIONAL_INPUT);
        tempArg.Offset = typeof(LabToXyz).GetField("temp");
        tempArg.Type = typeof(VipsArrayDouble);

        objClass.Args.Add(tempArg);
    }

    public override void Init()
    {
        // vips_Lab2XYZ_init
        base.Init();

        X0 = VipsD65.X0;
        Y0 = VipsD65.Y0;
        Z0 = VipsD65.Z0;

        Colour.Interpretation = VIPS_INTERPRETATION_XYZ;
    }

    public static int LabToXyz(VipsImage inImage, out VipsImage outImage)
    {
        // vips_Lab2XYZ
        return VipsCallSplit("LabToXyz", inImage, out outImage);
    }

    public static void ColLabToXyz(float L, float a, float b, out float X, out float Y, out float Z)
    {
        // vips_col_Lab2XYZ
        LabToXyz lab = new LabToXyz();
        lab.X0 = VipsD65.X0;
        lab.Y0 = VipsD65.Y0;
        lab.Z0 = VipsD65.Z0;

        lab.vips_col_Lab2XYZ_helper(L, a, b, out X, out Y, out Z);
    }
}
```

Note that I've assumed the existence of `VipsPel`, `VipsArrayDouble`, and other types from the original C code. You may need to modify these to match your specific implementation.

Also, I've used the `Math.Pow` method for exponentiation, as it's more readable than the `* *` operator.

Please let me know if you have any questions or need further clarification!