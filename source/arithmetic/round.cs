Here is the C# code equivalent to the provided C code:

```csharp
// round.cs --- various rounding operations

using System;
using System.Collections.Generic;

public class VipsRound : VipsUnary
{
    public enum Round { RINT, CEIL, FLOOR };

    private Round round;

    // vips_round_build
    public override int Build(VipsObject obj)
    {
        VipsUnary unary = (VipsUnary)obj;
        if (unary.in != null && VipsBandFormat.IsInt(unary.in.BandFmt))
            return VipsUnary.Copy(unary);
        if (base.Build(obj) == -1)
            return -1;
        return 0;
    }

    // vips_round_buffer
    public override void ProcessLine(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        VipsRound round = (VipsRound)arithmetic;
        VipsImage im = arithmetic.Ready[0];

        const int sz = width * im.Bands * (VipsBandFormat.IsComplex(im.BandFmt) ? 2 : 1);
        int x;

        switch (round.round)
        {
            case Round.RINT:
                SwitchRound(VIPS_RINT, outArray, inArray, sz);
                break;
            case Round.CEIL:
                SwitchRound(VIPS_CEIL, outArray, inArray, sz);
                break;
            case Round.FLOOR:
                SwitchRound(VIPS_FLOOR, outArray, inArray, sz);
                break;
        }
    }

    // vips_round_format_table
    private static readonly VipsBandFormat[] vipsRoundFormatTable = new VipsBandFormat[]
    {
        VipsBandFormat.UC,
        VipsBandFormat.C,
        VipsBandFormat.US,
        VipsBandFormat.S,
        VipsBandFormat.UI,
        VipsBandFormat.I,
        VipsBandFormat.F,
        VipsBandFormat.X,
        VipsBandFormat.D,
        VipsBandFormat.DX
    };

    // vips_round_class_init
    public static void ClassInit(VipsRoundClass class_)
    {
        base.ClassInit(class_);
        VIPS_ARG_ENUM("round", 200, "Rounding operation", VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsRound, round), typeof(Round), Round.RINT);
    }

    // vips_round_init
    public override void Init()
    {
        base.Init();
    }

    // vips_roundv
    public static int RoundV(VipsImage in_, VipsImage[] out_, VipsOperationRound round, params object[] args)
    {
        return VipsCallSplit("round", args, in_, out_, round);
    }

    // vips_round
    public static int Round(VipsImage in_, VipsImage[] out_, VipsOperationRound round, params object[] args)
    {
        var result = RoundV(in_, out_, round, args);
        return result;
    }

    // vips_floor
    public static int Floor(VipsImage in_, VipsImage[] out_, params object[] args)
    {
        return RoundV(in_, out_, VIPS_OPERATION_ROUND_FLOOR, args);
    }

    // vips_ceil
    public static int Ceil(VipsImage in_, VipsImage[] out_, params object[] args)
    {
        return RoundV(in_, out_, VIPS_OPERATION_ROUND_CEIL, args);
    }

    // vips_rint
    public static int Rint(VipsImage in_, VipsImage[] out_, params object[] args)
    {
        return RoundV(in_, out_, VIPS_OPERATION_ROUND_RINT, args);
    }

    // SwitchRound
    private void SwitchRound(Func<float, float> func, VipsPel[] outArray, VipsPel[][] inArray, int sz)
    {
        for (int x = 0; x < sz; x++)
            outArray[x] = func(inArray[0][x]);
    }
}
```

Note that I've assumed the existence of certain classes and methods from the VIPS library, as they are not provided in the original code snippet. You may need to modify the code to match your specific implementation.

Also, please note that this is a direct translation of the C code to C#, without any optimizations or improvements for performance. If you're looking for a more efficient or idiomatic C# solution, I'd be happy to help with that as well!