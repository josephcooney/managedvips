Here is the converted C# code:

```csharp
using System;
using Vips;

public class Relational : BinaryOperation
{
    public enum OperationRelational { Equal, NotEqual, Less, LessEq, More, MoreEq };

    public override int ProcessLine(VipsPel[] outArray, VipsPel[][] inArrays)
    {
        var relational = (OperationRelational)GetProperty("relational");
        var im = GetInputImage(0);
        var sz = im.Width * im.Bands;
        var left = inArrays[0];
        var right = inArrays[1];

        switch (relational)
        {
            case OperationRelational.Equal:
                for (int x = 0; x < sz; x++)
                    outArray[x] = (left[x] == right[x]) ? 255 : 0;
                break;

            case OperationRelational.NotEqual:
                for (int x = 0; x < sz; x++)
                    outArray[x] = (left[x] != right[x]) ? 255 : 0;
                break;

            case OperationRelational.Less:
                for (int x = 0; x < sz; x++)
                    outArray[x] = (left[x] < right[x]) ? 255 : 0;
                break;

            case OperationRelational.LessEq:
                for (int x = 0; x < sz; x++)
                    outArray[x] = (left[x] <= right[x]) ? 255 : 0;
                break;

            case OperationRelational.More:
                for (int x = 0; x < sz; x++)
                    outArray[x] = (left[x] > right[x]) ? 255 : 0;
                break;

            case OperationRelational.MoreEq:
                for (int x = 0; x < sz; x++)
                    outArray[x] = (left[x] >= right[x]) ? 255 : 0;
                break;

            default:
                throw new ArgumentException("Invalid relational operation");
        }

        return 0;
    }
}

public class RelationalConst : UnaryConstantOperation
{
    public override int ProcessLine(VipsPel[] outArray, VipsPel[][] inArrays)
    {
        var relational = (OperationRelational)GetProperty("relational");
        var im = GetInputImage(0);
        var bands = im.Bands;
        var isInt = IsInteger() && im.Format == VipsFormat.UChar;

        switch (relational)
        {
            case OperationRelational.Equal:
                if (isInt)
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] == GetConstant(b)) ? 255 : 0;
                else
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] == GetConstant(b)) ? 255 : 0;
                break;

            case OperationRelational.NotEqual:
                if (isInt)
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] != GetConstant(b)) ? 255 : 0;
                else
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] != GetConstant(b)) ? 255 : 0;
                break;

            case OperationRelational.Less:
                if (isInt)
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] < GetConstant(b)) ? 255 : 0;
                else
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] < GetConstant(b)) ? 255 : 0;
                break;

            case OperationRelational.LessEq:
                if (isInt)
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] <= GetConstant(b)) ? 255 : 0;
                else
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] <= GetConstant(b)) ? 255 : 0;
                break;

            case OperationRelational.More:
                if (isInt)
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] > GetConstant(b)) ? 255 : 0;
                else
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] > GetConstant(b)) ? 255 : 0;
                break;

            case OperationRelational.MoreEq:
                if (isInt)
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] >= GetConstant(b)) ? 255 : 0;
                else
                    for (int i = 0, x = 0; x < im.Width; x++)
                        for (int b = 0; b < bands; b++, i++)
                            outArray[i] = (inArrays[0][i] >= GetConstant(b)) ? 255 : 0;
                break;

            default:
                throw new ArgumentException("Invalid relational operation");
        }

        return 0;
    }
}

public class RelationalOperation
{
    public static int Equal(VipsImage left, VipsImage right, out VipsImage output)
    {
        var op = new Relational { OperationRelational = OperationRelational.Equal };
        return op.Process(left, right, ref output);
    }

    public static int NotEqual(VipsImage left, VipsImage right, out VipsImage output)
    {
        var op = new Relational { OperationRelational = OperationRelational.NotEqual };
        return op.Process(left, right, ref output);
    }

    public static int Less(VipsImage left, VipsImage right, out VipsImage output)
    {
        var op = new Relational { OperationRelational = OperationRelational.Less };
        return op.Process(left, right, ref output);
    }

    public static int LessEq(VipsImage left, VipsImage right, out VipsImage output)
    {
        var op = new Relational { OperationRelational = OperationRelational.LessEq };
        return op.Process(left, right, ref output);
    }

    public static int More(VipsImage left, VipsImage right, out VipsImage output)
    {
        var op = new Relational { OperationRelational = OperationRelational.More };
        return op.Process(left, right, ref output);
    }

    public static int MoreEq(VipsImage left, VipsImage right, out VipsImage output)
    {
        var op = new Relational { OperationRelational = OperationRelational.MoreEq };
        return op.Process(left, right, ref output);
    }
}

public class RelationalConstOperation
{
    public static int EqualConst(VipsImage image, double[] constants, out VipsImage output)
    {
        var op = new RelationalConst { OperationRelational = OperationRelational.Equal };
        return op.Process(image, constants, ref output);
    }

    public static int NotEqualConst(VipsImage image, double[] constants, out VipsImage output)
    {
        var op = new RelationalConst { OperationRelational = OperationRelational.NotEqual };
        return op.Process(image, constants, ref output);
    }

    public static int LessConst(VipsImage image, double[] constants, out VipsImage output)
    {
        var op = new RelationalConst { OperationRelational = OperationRelational.Less };
        return op.Process(image, constants, ref output);
    }

    public static int LessEqConst(VipsImage image, double[] constants, out VipsImage output)
    {
        var op = new RelationalConst { OperationRelational = OperationRelational.LessEq };
        return op.Process(image, constants, ref output);
    }

    public static int MoreConst(VipsImage image, double[] constants, out VipsImage output)
    {
        var op = new RelationalConst { OperationRelational = OperationRelational.More };
        return op.Process(image, constants, ref output);
    }

    public static int MoreEqConst(VipsImage image, double[] constants, out VipsImage output)
    {
        var op = new RelationalConst { OperationRelational = OperationRelational.MoreEq };
        return op.Process(image, constants, ref output);
    }
}
```