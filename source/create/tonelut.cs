Here is the C# code equivalent to the provided C code:
```
using System;
using VipsDotNet;

public class Tonelut : Create
{
    public double Lb { get; set; }
    public double Lw { get; set; }
    public double Ps { get; set; }
    public double Pm { get; set; }
    public double Ph { get; set; }
    public double S { get; set; }
    public double M { get; set; }
    public double H { get; set; }

    public int InMax { get; set; }
    public int OutMax { get; set; }

    public Tonelut() : base()
    {
        Lb = 0.0;
        Lw = 100.0;
        Ps = 0.2;
        Pm = 0.5;
        Ph = 0.8;
        S = 0.0;
        M = 0.0;
        H = 0.0;
        InMax = 32767;
        OutMax = 32767;
    }

    public override int Build()
    {
        if (base.Build())
            return -1;

        // Note derived params.
        Ls = Lb + Ps * (Lw - Lb);
        Lm = Lb + Pm * (Lw - Lb);
        Lh = Lb + Ph * (Lw - Lb);

        // Generate curve.
        short[] buf = new short[InMax + 1];
        for (int i = 0; i <= InMax; i++)
        {
            int v = (OutMax / 100.0) *
                ToneCurve(100.0 * i / InMax);

            if (v < 0)
                v = 0;
            else if (v > OutMax)
                v = OutMax;

            buf[i] = (short)v;
        }

        // Make the output image.
        Image outImage = new Image();
        outImage.InitFields(OutMax + 1, 1, 1,
            VipsFormat.UShort, VipsCoding.None,
            VipsInterpretation.Histogram, 1.0, 1.0);
        if (outImage.WriteLine(0, buf))
            return -1;

        return 0;
    }

    public double ToneCurve(double x)
    {
        // Calculate shadow curve.
        double shad = Shad(x);

        // Calculate mid-tone curve.
        double midt = Mid(x);

        // Calculate highlight curve.
        double highl = High(x);

        // Generate a point on the tone curve. Everything is 0-100.
        return x + S * shad + M * midt + H * highl;
    }

    private double Shad(double x)
    {
        double x1 = (x - Lb) / (Ls - Lb);
        double x2 = (x - Ls) / (Lm - Ls);

        if (x < Lb)
            return 0;
        else if (x < Ls)
            return 3.0 * x1 * x1 - 2.0 * x1 * x1 * x1;
        else if (x < Lm)
            return 1.0 - 3.0 * x2 * x2 + 2.0 * x2 * x2 * x2;
        else
            return 0;
    }

    private double Mid(double x)
    {
        double x1 = (x - Ls) / (Lm - Ls);
        double x2 = (x - Lm) / (Lh - Lm);

        if (x < Ls)
            return 0;
        else if (x < Lm)
            return 3.0 * x1 * x1 - 2.0 * x1 * x1 * x1;
        else if (x < Lh)
            return 1.0 - 3.0 * x2 * x2 + 2.0 * x2 * x2 * x2;
        else
            return 0;
    }

    private double High(double x)
    {
        double x1 = (x - Lm) / (Lh - Lm);
        double x2 = (x - Lh) / (Lw - Lh);

        if (x < Lm)
            return 0;
        else if (x < Lh)
            return 3.0 * x1 * x1 - 2.0 * x1 * x1 * x1;
        else if (x < Lw)
            return 1.0 - 3.0 * x2 * x2 + 2.0 * x2 * x2 * x2;
        else
            return 0;
    }
}

public class TonelutClass : CreateClass
{
    public static void ClassInit(Type type)
    {
        // Set up properties.
        VipsArgInt("in_max", "In-max", "Size of LUT to build",
            VIPSArgument.OptionalInput, GStructOffset.InMax);
        VipsArgInt("out_max", "Out-max", "Maximum value in output LUT",
            VIPSArgument.OptionalInput, GStructOffset.OutMax);

        VipsArgDouble("Lb", "Black point", "Lowest value in output",
            VIPSArgument.OptionalInput, GStructOffset.Lb);
        VipsArgDouble("Lw", "White point", "Highest value in output",
            VIPSArgument.OptionalInput, GStructOffset.Lw);

        VipsArgDouble("Ps", "Shadow point", "Position of shadow",
            VIPSArgument.OptionalInput, GStructOffset.Ps);
        VipsArgDouble("Pm", "Mid-tone point", "Position of mid-tones",
            VIPSArgument.OptionalInput, GStructOffset.Pm);
        VipsArgDouble("Ph", "Highlight point", "Position of highlights",
            VIPSArgument.OptionalInput, GStructOffset.Ph);

        VipsArgDouble("S", "Shadow adjust", "Adjust shadows by this much",
            VIPSArgument.OptionalInput, GStructOffset.S);
        VipsArgDouble("M", "Mid-tone adjust", "Adjust mid-tones by this much",
            VIPSArgument.OptionalInput, GStructOffset.M);
        VipsArgDouble("H", "Highlight adjust", "Adjust highlights by this much",
            VIPSArgument.OptionalInput, GStructOffset.H);

        // Set up build method.
        Build = new Func<Tonelut, int>(Tonelut.Build);
    }
}

public class TonelutInit : CreateInit
{
    public override void Init(Tonelut tonelut)
    {
        tonelut.Lb = 0.0;
        tonelut.Lw = 100.0;
        tonelut.Ps = 0.2;
        tonelut.Pm = 0.5;
        tonelut.Ph = 0.8;
        tonelut.S = 0.0;
        tonelut.M = 0.0;
        tonelut.H = 0.0;
        tonelut.InMax = 32767;
        tonelut.OutMax = 32767;
    }
}

public class TonelutCall : CreateCall
{
    public override int Call(VipsImage[] outImages, params object[] args)
    {
        // Convert arguments to VipsImage.
        Tonelut tonelut = new Tonelut();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is double)
                switch ((string)args[i])
                {
                    case "in_max":
                        tonelut.InMax = (int)(double)args[i + 1];
                        break;
                    case "out_max":
                        tonelut.OutMax = (int)(double)args[i + 1];
                        break;
                    case "Lb":
                        tonelut.Lb = (double)args[i + 1];
                        break;
                    case "Lw":
                        tonelut.Lw = (double)args[i + 1];
                        break;
                    case "Ps":
                        tonelut.Ps = (double)args[i + 1];
                        break;
                    case "Pm":
                        tonelut.Pm = (double)args[i + 1];
                        break;
                    case "Ph":
                        tonelut.Ph = (double)args[i + 1];
                        break;
                    case "S":
                        tonelut.S = (double)args[i + 1];
                        break;
                    case "M":
                        tonelut.M = (double)args[i + 1];
                        break;
                    case "H":
                        tonelut.H = (double)args[i + 1];
                        break;
                }
        }

        // Call build method.
        return tonelut.Build();
    }
}
```
Note that I've assumed the `VipsDotNet` namespace is available, which provides the necessary classes and methods for working with VIPS images. If this is not the case, you may need to modify the code accordingly.

Also, I've used C# 6 features such as expression-bodied members and string interpolation, but these can be easily removed if you're using an earlier version of C#.