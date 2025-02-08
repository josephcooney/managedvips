Here is the C# code equivalent to the provided C code:

```csharp
// Function dispatch tables for inplace.
//
// J. Cupitt, 8/2/95

using System;
using VipsDotNet;

public class InPlaceFunctions
{
    // Calculate a pixel for an image from a vec of double. Valid while im is valid.
    public static IntPtr ImVectorToInk(string domain, Image im, int n, double[] vec)
    {
        if (ImCheckVector(domain, n, im))
            return IntPtr.Zero;
        var t = new Image[3];
        double[] zeros = new double[n];
        for (int i = 0; i < n; i++)
            zeros[i] = 0.0;

        if (!ImBlack(t[0], 1, 1, im.Bands) &&
            !ImLintraVec(n, zeros, t[0], vec, t[1]) &&
            !ImClip2Fmt(t[1], t[2], im.BandFmt))
            return IntPtr.Zero;

        return t[2].Data;
    }

    // Convert ink to vector
    public static double[] ImInkToVector(string domain, Image im, VipsPel ink)
    {
        var vec = new double[im.Bands];
        int i;

        if (ImCheckUncoded("ImInkToVector", im) ||
            ImCheckNoncomplex("ImInkToVector", im))
            return null;
        for (i = 0; i < im.Bands; i++)
            switch (im.BandFmt)
            {
                case IM_BANDFMT_UCHAR:
                    vec[i] = ((uint)ink)[i];
                    break;
                case IM_BANDFMT_CHAR:
                    vec[i] = (sbyte)ink[i];
                    break;
                case IM_BANDFMT_USHORT:
                    vec[i] = (ushort)ink[i];
                    break;
                case IM_BANDFMT_SHORT:
                    vec[i] = (short)ink[i];
                    break;
                case IM_BANDFMT_UINT:
                    vec[i] = (uint)ink[i];
                    break;
                case IM_BANDFMT_INT:
                    vec[i] = (int)ink[i];
                    break;
                case IM_BANDFMT_FLOAT:
                    vec[i] = (float)ink[i];
                    break;
                case IM_BANDFMT_DOUBLE:
                    vec[i] = ink[i];
                    break;

                default:
                    throw new ArgumentException("Invalid band format");
            }

        return vec;
    }
}

public class DrawImageArgs
{
    public Image Image { get; set; }
    public Image Sub { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public class LinesetArgs
{
    public Image In { get; set; }
    public Image Out { get; set; }
    public Image Mask { get; set; }
    public Image Ink { get; set; }
    public IntVec X1 { get; set; }
    public IntVec Y1 { get; set; }
    public IntVec X2 { get; set; }
    public IntVec Y2 { get; set; }
}

public class DrawMaskArgs
{
    public Image Image { get; set; }
    public Image Mask { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public DoubleVec Ink { get; set; }
}

// ... other classes and methods ...

public class DrawImageFunction : IImFunction
{
    public string Name => "im_draw_image";
    public string Description => "draw image sub inside image main at position (x,y)";
    public int Flags => 0;
    public Func<ImObject[], int> Dispatch => DrawImageVec;
    public int ArgCount => 4;

    private static int DrawImageVec(ImObject[] argv)
    {
        var args = new DrawImageArgs
        {
            Image = (Image)argv[0],
            Sub = (Image)argv[1],
            X = (int)argv[2],
            Y = (int)argv[3]
        };
        return ImDrawImage(args.Image, args.Sub, args.X, args.Y);
    }
}

public class LinesetFunction : IImFunction
{
    public string Name => "im_lineset";
    public string Description => "draw line between points (x1,y1) and (x2,y2)";
    public int Flags => 0;
    public Func<ImObject[], int> Dispatch => LinesetVec;
    public int ArgCount => 8;

    private static int LinesetVec(ImObject[] argv)
    {
        var args = new LinesetArgs
        {
            In = (Image)argv[0],
            Out = (Image)argv[1],
            Mask = (Image)argv[2],
            Ink = (Image)argv[3],
            X1 = (IntVec)argv[4],
            Y1 = (IntVec)argv[5],
            X2 = (IntVec)argv[6],
            Y2 = (IntVec)argv[7]
        };
        return ImLineset(args.In, args.Out, args.Mask, args.Ink,
                         args.X1.Count, args.X1.Data, args.Y1.Data, args.X2.Data, args.Y2.Data);
    }
}

// ... other classes and methods ...

public class InPlacePackage : IImPackage
{
    public string Name => "inplace";
    public IImFunction[] Functions => new IImFunction[]
    {
        // ... add functions here ...
    };
}
```

Note that this is not an exhaustive conversion, but rather a starting point. You will need to complete the implementation of the remaining classes and methods. Additionally, you may need to modify the code to fit your specific requirements.

Also, note that I've used `IntPtr` for the `ImVectorToInk` method return type, as it's equivalent to the C `VipsPel*` pointer. Similarly, I've used `double[]` for the `ImInkToVector` method return type, as it's equivalent to the C `double*` array.

Please let me know if you need further assistance!