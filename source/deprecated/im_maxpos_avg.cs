Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class MaxPosAvg
{
    public int XPos { get; set; }
    public int YPos { get; set; }
    public double Max { get; set; }
    public int Occurences { get; set; }

    public static void Start(IMAGE image, object a, object b)
    {
        var globalMaxPosAvg = (MaxPosAvg)b;
        var maxPosAvg = new MaxPosAvg();

        maxPosAvg.XPos = globalMaxPosAvg.XPos;
        maxPosAvg.YPos = globalMaxPosAvg.YPos;
        maxPosAvg.Max = globalMaxPosAvg.Max;
        maxPosAvg.Occurences = globalMaxPosAvg.Occurences;

        return maxPosAvg;
    }

    public static int Stop(object seq, object a, object b)
    {
        var globalMaxPosAvg = (MaxPosAvg)b;
        var maxPosAvg = (MaxPosAvg)seq;

        if (maxPosAvg.Occurences == 0)
            return 0;

        if (maxPosAvg.Max > globalMaxPosAvg.Max)
            *globalMaxPosAvg = *maxPosAvg;
        else if (maxPosAvg.Max == globalMaxPosAvg.Max)
        {
            globalMaxPosAvg.XPos += maxPosAvg.XPos;
            globalMaxPosAvg.YPos += maxPosAvg.YPos;
            globalMaxPosAvg.Occurences += maxPosAvg.Occurences;
        }

        return 0;
    }
}

public class MaxPosAvgScan
{
    public static int Scan(REGION region, object seq, object a, object b, ref bool stop)
    {
        var r = region.Valid;
        var sz = IM_REGION_N_ELEMENTS(region);
        var maxPosAvg = (MaxPosAvg)seq;

        double max;
        int xpos, ypos, occurences;

        xpos = maxPosAvg.XPos;
        ypos = maxPosAvg.YPos;
        max = maxPosAvg.Max;
        occurences = maxPosAvg.Occurences;

        for (int y = 0; y < r.Height; y++)
        {
            VipsPel[] inArray = VIPS_REGION_ADDR(region, r.Left, r.Top + y);

            switch (region.Image.BandFmt)
            {
                case IM_BANDFMT_UCHAR:
                    ILoop(inArray);
                    break;
                case IM_BANDFMT_CHAR:
                    ILoop((signed char[])inArray);
                    break;
                case IM_BANDFMT_USHORT:
                    ILoop((unsigned short[])inArray);
                    break;
                case IM_BANDFMT_SHORT:
                    ILoop((short[])inArray);
                    break;
                case IM_BANDFMT_UINT:
                    ILoop((uint[])inArray);
                    break;
                case IM_BANDFMT_INT:
                    ILoop((int[])inArray);
                    break;
                case IM_BANDFMT_FLOAT:
                    FLoop(inArray);
                    break;
                case IM_BANDFMT_DOUBLE:
                    FLoop((double[])inArray);
                    break;
                case IM_BANDFMT_COMPLEX:
                    CLoop(inArray);
                    break;
                case IM_BANDFMT_DPCOMPLEX:
                    CLoop((double[])inArray);
                    break;

                default:
                    throw new Exception("Invalid band format");
            }
        }

        maxPosAvg.XPos = xpos;
        maxPosAvg.YPos = ypos;
        maxPosAvg.Max = max;
        maxPosAvg.Occurences = occurences;

        return 0;
    }
}

public class ILoop
{
    public static void Loop(VipsPel[] inArray)
    {
        var p = inArray;
        double m;

        m = MaxPosAvg.Max;

        for (int x = 0; x < inArray.Length; x++)
        {
            VipsPel v = p[x];

            if (MaxPosAvg.Occurences == 0 || v > m)
            {
                m = v;
                MaxPosAvg.XPos = region.Valid.Left + x / region.Image.Bands;
                MaxPosAvg.YPos = region.Valid.Top + y;
                MaxPosAvg.Occurences = 1;
            }
            else if (v == m)
            {
                MaxPosAvg.XPos += region.Valid.Left + x / region.Image.Bands;
                MaxPosAvg.YPos += region.Valid.Top + y;
                MaxPosAvg.Occurences++;
            }
        }

        MaxPosAvg.Max = m;
    }
}

public class FLoop
{
    public static void Loop(VipsPel[] inArray)
    {
        var p = inArray;
        double m;

        m = MaxPosAvg.Max;

        for (int x = 0; x < inArray.Length; x++)
        {
            VipsPel v = p[x];

            if (double.IsNaN(v))
                continue;

            if (MaxPosAvg.Occurences == 0 || v > m)
            {
                m = v;
                MaxPosAvg.XPos = region.Valid.Left + x / region.Image.Bands;
                MaxPosAvg.YPos = region.Valid.Top + y;
                MaxPosAvg.Occurences = 1;
            }
            else if (v == m)
            {
                MaxPosAvg.XPos += region.Valid.Left + x / region.Image.Bands;
                MaxPosAvg.YPos += region.Valid.Top + y;
                MaxPosAvg.Occurences++;
            }
        }

        MaxPosAvg.Max = m;
    }
}

public class CLoop
{
    public static void Loop(VipsPel[] inArray)
    {
        for (int x = 0; x < inArray.Length; x++)
        {
            double re, im;

            re = p[0];
            im = p[1];
            p += 2;
            double mod = re * re + im * im;

            if (double.IsNaN(mod))
                continue;

            if (MaxPosAvg.Occurences == 0 || mod > MaxPosAvg.Max)
            {
                MaxPosAvg.Max = mod;
                MaxPosAvg.XPos = region.Valid.Left + x / region.Image.Bands;
                MaxPosAvg.YPos = region.Valid.Top + y;
                MaxPosAvg.Occurences = 1;
            }
            else if (mod == MaxPosAvg.Max)
            {
                MaxPosAvg.XPos += region.Valid.Left + x / region.Image.Bands;
                MaxPosAvg.YPos += region.Valid.Top + y;
                MaxPosAvg.Occurences++;
            }
        }
    }
}

public class ImMaxPosAvg
{
    public static int FindMaxPosAvg(IMAGE image, ref double xpos, ref double ypos, ref double outValue)
    {
        if (im_PinCheck(image) || im_CheckUncoded("im_maxpos_avg", image))
            return -1;

        var globalMaxPosAvg = new MaxPosAvg();
        globalMaxPosAvg.Occurences = 0;

        if (vips_Sink(image, MaxPosAvg.Start, MaxPosAvgScan.Scan, MaxPosAvg.Stop,
            image, globalMaxPosAvg))
            return -1;

        if (globalMaxPosAvg.Occurences == 0)
        {
            xpos = double.NaN;
            ypos = double.NaN;
            outValue = double.NaN;
        }
        else
        {
            // Back to modulus.
            if (vips_BandFormatIsComplex(image.BandFmt))
                globalMaxPosAvg.Max = Math.Sqrt(globalMaxPosAvg.Max);

            if (xpos != null)
                xpos = (double)globalMaxPosAvg.XPos / globalMaxPosAvg.Occurences;
            if (ypos != null)
                ypos = (double)globalMaxPosAvg.YPos / globalMaxPosAvg.Occurences;
            if (outValue != null)
                outValue = globalMaxPosAvg.Max;
        }

        return 0;
    }
}
```

Note that I've assumed the existence of certain classes and methods from the VipsDotNet library, which is a .NET wrapper for the VIPS image processing library. You may need to modify the code to match your specific use case.

Also note that this code does not include any error handling or debugging mechanisms, as they were not present in the original C code. You should add these features according to your needs.