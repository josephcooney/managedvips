Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsBandbool : VipsBandary
{
    public VipsImage inImage { get; set; }
    public VipsOperationBoolean operation { get; set; }

    public VipsBandbool()
    {
        operation = VipsOperationBoolean.And;
    }

    public override int Build(VipsObject object)
    {
        VipsObjectClass classType = (VipsObjectClass)object.GetType();
        VipsBandary bandary = (VipsBandary)object;
        VipsBandbool bandbool = (VipsBandbool)object;

        // << and >> don't work over bands.
        if (bandbool.operation == VipsOperationBoolean.LShift ||
            bandbool.operation == VipsOperationBoolean.RShift)
        {
            throw new Exception($"operator {GetEnumName(bandbool.operation)} not supported across image bands");
        }

        if (inImage != null)
        {
            if (!VipsCheckNonComplex(classType.Nickname, inImage))
                return -1;

            bandary.Bands = 1;
            bandary.Input = new[] { inImage };

            if (inImage.Bands == 1)
                return VipsBandaryCopy(bandary);
        }

        bandary.OutputBands = 1;

        if (base.Build(object) != 0)
            return -1;

        return 0;
    }

    public override void Buffer(VipsBandarySequence sequence, VipsPel[] output, VipsPel[][] input, int width)
    {
        VipsBandary bandary = sequence.Bandary;
        VipsBandbool bandbool = (VipsBandbool)bandary;

        switch (bandbool.operation)
        {
            case VipsOperationBoolean.And:
                SwitchFormat(input[0][0], LoopB, FLoopB, &);
                break;

            case VipsOperationBoolean.Or:
                SwitchFormat(input[0][0], LoopB, FLoopB, |);
                break;

            case VipsOperationBoolean.Eor:
                SwitchFormat(input[0][0], LoopB, FLoopB, ^);
                break;

            default:
                throw new Exception("Unknown operation");
        }
    }

    private void SwitchFormat(object value, Action<object, char> loopB, Action<object, char> fLoopB, char op)
    {
        switch (VipsImage.GetFormat(value))
        {
            case VipsFormat.UChar:
                LoopB((object)Convert.ChangeType(value, typeof(unsigned char)), op);
                break;

            case VipsFormat.Char:
                LoopB((object)Convert.ChangeType(value, typeof(signed char)), op);
                break;

            case VipsFormat.UInt:
                LoopB((object)Convert.ChangeType(value, typeof(unsigned int)), op);
                break;

            case VipsFormat.Int:
                LoopB((object)Convert.ChangeType(value, typeof(int)), op);
                break;

            case VipsFormat.Float:
                FLoopB((object)Convert.ChangeType(value, typeof(float)), op);
                break;

            case VipsFormat.Double:
                FLoopB((object)Convert.ChangeType(value, typeof(double)), op);
                break;

            default:
                throw new Exception("Unknown format");
        }
    }

    private void LoopB(object value, char op)
    {
        unsigned char[] p = (unsigned char[])value;
        unsigned char[] q = (unsigned char[])output;

        for (int x = 0; x < width; x++)
        {
            unsigned char acc = p[0];

            for (int b = 1; b < bands; b++)
                acc = (unsigned char)(acc op p[b]);

            q[x] = acc;
            p += bands;
        }
    }

    private void FLoopB(object value, char op)
    {
        float[] p = (float[])value;
        int[] q = (int[])output;

        for (int x = 0; x < width; x++)
        {
            int acc = (int)p[0];

            for (int b = 1; b < bands; b++)
                acc = (int)(acc op (int)p[b]);

            q[x] = acc;
            p += bands;
        }
    }

    public static string GetEnumName(Enum value)
    {
        return value.ToString().Replace("_", " ");
    }

    private const int VipsFormat_UChar = 0;
    private const int VipsFormat_Char = 1;
    private const int VipsFormat_UInt = 2;
    private const int VipsFormat_Int = 3;
    private const int VipsFormat_Float = 4;
    private const int VipsFormat_Double = 5;

    private static readonly VipsBandFormat[] vips_bandbool_format_table = new[]
    {
        // Band format:  UC  C  US  S  UI  I  F  X  D  DX
        // Promotion:
        VipsFormat.UChar, VipsFormat.Char, VipsFormat.UInt, VipsFormat.Int,
        VipsFormat.UInt, VipsFormat.Int, VipsFormat.Int, VipsFormat.Int,
        VipsFormat.Int, VipsFormat.Int
    };

    public class VipsBandboolClass : VipsObjectClass
    {
        public override string Nickname { get; } = "bandbool";
        public override string Description { get; } = "boolean operation across image bands";

        public override int Build(VipsObject object)
        {
            return base.Build(object);
        }

        public override void Buffer(VipsBandarySequence sequence, VipsPel[] output, VipsPel[][] input, int width)
        {
            base.Buffer(sequence, output, input, width);
        }
    }
}

public enum VipsOperationBoolean
{
    And,
    Or,
    Eor,
    LShift,
    RShift
}
```