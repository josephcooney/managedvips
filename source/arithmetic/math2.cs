Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

// vips_math2.c --- 2ary math funcs

public class VipsMath2 : VipsBinary
{
    public enum OperationMath2 { Pow, Wop, Atan2 };

    private OperationMath2 _math2;

    public VipsMath2(OperationMath2 math2)
    {
        _math2 = math2;
    }

    // vips_math2_buffer

    public void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int sz = width * im.Bands;

        switch (_math2)
        {
            case OperationMath2.Pow:
                foreach (var format in Enum.GetValues(typeof(VipsFormat)))
                {
                    if ((int)format == VipsFormat.UChar ||
                        (int)format == VipsFormat.Char ||
                        (int)format == VipsFormat.UInt ||
                        (int)format == VipsFormat.Int ||
                        (int)format == VipsFormat.Float)
                    {
                        float[] p1 = new float[sz];
                        float[] p2 = new float[sz];
                        float[] q = new float[sz];

                        for (int x = 0; x < sz; x++)
                        {
                            p1[x] = inArrays[0][x].Float;
                            p2[x] = inArrays[1][x].Float;
                            q[x] = Pow(q[x], p1[x], p2[x]);
                        }

                        outArray.AsSpan(0, sz).CopyTo(q);
                    }
                    else if ((int)format == VipsFormat.Double)
                    {
                        double[] p1 = new double[sz];
                        double[] p2 = new double[sz];
                        double[] q = new double[sz];

                        for (int x = 0; x < sz; x++)
                        {
                            p1[x] = inArrays[0][x].Double;
                            p2[x] = inArrays[1][x].Double;
                            q[x] = Pow(q[x], p1[x], p2[x]);
                        }

                        outArray.AsSpan(0, sz).CopyTo(q);
                    }
                }
                break;

            case OperationMath2.Wop:
                foreach (var format in Enum.GetValues(typeof(VipsFormat)))
                {
                    if ((int)format == VipsFormat.UChar ||
                        (int)format == VipsFormat.Char ||
                        (int)format == VipsFormat.UInt ||
                        (int)format == VipsFormat.Int ||
                        (int)format == VipsFormat.Float)
                    {
                        float[] p1 = new float[sz];
                        float[] p2 = new float[sz];
                        float[] q = new float[sz];

                        for (int x = 0; x < sz; x++)
                        {
                            p1[x] = inArrays[0][x].Float;
                            p2[x] = inArrays[1][x].Float;
                            q[x] = Wop(q[x], p1[x], p2[x]);
                        }

                        outArray.AsSpan(0, sz).CopyTo(q);
                    }
                    else if ((int)format == VipsFormat.Double)
                    {
                        double[] p1 = new double[sz];
                        double[] p2 = new double[sz];
                        double[] q = new double[sz];

                        for (int x = 0; x < sz; x++)
                        {
                            p1[x] = inArrays[0][x].Double;
                            p2[x] = inArrays[1][x].Double;
                            q[x] = Wop(q[x], p1[x], p2[x]);
                        }

                        outArray.AsSpan(0, sz).CopyTo(q);
                    }
                }
                break;

            case OperationMath2.Atan2:
                foreach (var format in Enum.GetValues(typeof(VipsFormat)))
                {
                    if ((int)format == VipsFormat.UChar ||
                        (int)format == VipsFormat.Char ||
                        (int)format == VipsFormat.UInt ||
                        (int)format == VipsFormat.Int ||
                        (int)format == VipsFormat.Float)
                    {
                        float[] p1 = new float[sz];
                        float[] p2 = new float[sz];
                        float[] q = new float[sz];

                        for (int x = 0; x < sz; x++)
                        {
                            p1[x] = inArrays[0][x].Float;
                            p2[x] = inArrays[1][x].Float;
                            q[x] = Atan2(q[x], p1[x], p2[x]);
                        }

                        outArray.AsSpan(0, sz).CopyTo(q);
                    }
                    else if ((int)format == VipsFormat.Double)
                    {
                        double[] p1 = new double[sz];
                        double[] p2 = new double[sz];
                        double[] q = new double[sz];

                        for (int x = 0; x < sz; x++)
                        {
                            p1[x] = inArrays[0][x].Double;
                            p2[x] = inArrays[1][x].Double;
                            q[x] = Atan2(q[x], p1[x], p2[x]);
                        }

                        outArray.AsSpan(0, sz).CopyTo(q);
                    }
                }
                break;

            default:
                throw new Exception("Invalid operation");
        }
    }

    // vips_math2_class_init

    public static void ClassInit(Type type)
    {
        VipsObjectClass objectClass = (VipsObjectClass)type;
        VipsArithmeticClass arithmeticClass = (VipsArithmeticClass)type;

        objectClass.Nickname = "math2";
        objectClass.Description = "binary math operations";

        arithmeticClass.ProcessLine = Buffer;
    }

    // vips_math2_init

    public VipsMath2() { }
}

public class VipsPow : VipsMath2
{
    public VipsPow() : base(VipsMath2.OperationMath2.Pow) { }
}

public class VipsWop : VipsMath2
{
    public VipsWop() : base(VipsMath2.OperationMath2.Wop) { }
}

public class VipsAtan2 : VipsMath2
{
    public VipsAtan2() : base(VipsMath2.OperationMath2.Atan2) { }
}

// vips_math2_const.c

public class VipsMath2Const : VipsUnaryConst
{
    private OperationMath2 _math2;

    public VipsMath2Const(OperationMath2 math2)
    {
        _math2 = math2;
    }

    // vips_math2_const_buffer

    public void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int bands = im.Bands;

        switch (_math2)
        {
            case OperationMath2.Pow:
                foreach (var format in Enum.GetValues(typeof(VipsFormat)))
                {
                    if ((int)format == VipsFormat.UChar ||
                        (int)format == VipsFormat.Char ||
                        (int)format == VipsFormat.UInt ||
                        (int)format == VipsFormat.Int ||
                        (int)format == VipsFormat.Float)
                    {
                        float[] p = new float[width * bands];
                        float[] q = new float[width * bands];

                        for (int i = 0, x = 0; x < width; x++)
                            for (int b = 0; b < bands; b++, i++)
                                Pow(q[i], inArrays[0][x].Float, p[i]);

                        outArray.AsSpan(0, width * bands).CopyTo(q);
                    }
                    else if ((int)format == VipsFormat.Double)
                    {
                        double[] p = new double[width * bands];
                        double[] q = new double[width * bands];

                        for (int i = 0, x = 0; x < width; x++)
                            for (int b = 0; b < bands; b++, i++)
                                Pow(q[i], inArrays[0][x].Double, p[i]);

                        outArray.AsSpan(0, width * bands).CopyTo(q);
                    }
                }
                break;

            case OperationMath2.Wop:
                foreach (var format in Enum.GetValues(typeof(VipsFormat)))
                {
                    if ((int)format == VipsFormat.UChar ||
                        (int)format == VipsFormat.Char ||
                        (int)format == VipsFormat.UInt ||
                        (int)format == VipsFormat.Int ||
                        (int)format == VipsFormat.Float)
                    {
                        float[] p = new float[width * bands];
                        float[] q = new float[width * bands];

                        for (int i = 0, x = 0; x < width; x++)
                            for (int b = 0; b < bands; b++, i++)
                                Wop(q[i], inArrays[0][x].Float, p[i]);

                        outArray.AsSpan(0, width * bands).CopyTo(q);
                    }
                    else if ((int)format == VipsFormat.Double)
                    {
                        double[] p = new double[width * bands];
                        double[] q = new double[width * bands];

                        for (int i = 0, x = 0; x < width; x++)
                            for (int b = 0; b < bands; b++, i++)
                                Wop(q[i], inArrays[0][x].Double, p[i]);

                        outArray.AsSpan(0, width * bands).CopyTo(q);
                    }
                }
                break;

            case OperationMath2.Atan2:
                foreach (var format in Enum.GetValues(typeof(VipsFormat)))
                {
                    if ((int)format == VipsFormat.UChar ||
                        (int)format == VipsFormat.Char ||
                        (int)format == VipsFormat.UInt ||
                        (int)format == VipsFormat.Int ||
                        (int)format == VipsFormat.Float)
                    {
                        float[] p = new float[width * bands];
                        float[] q = new float[width * bands];

                        for (int i = 0, x = 0; x < width; x++)
                            for (int b = 0; b < bands; b++, i++)
                                Atan2(q[i], inArrays[0][x].Float, p[i]);

                        outArray.AsSpan(0, width * bands).CopyTo(q);
                    }
                    else if ((int)format == VipsFormat.Double)
                    {
                        double[] p = new double[width * bands];
                        double[] q = new double[width * bands];

                        for (int i = 0, x = 0; x < width; x++)
                            for (int b = 0; b < bands; b++, i++)
                                Atan2(q[i], inArrays[0][x].Double, p[i]);

                        outArray.AsSpan(0, width * bands).CopyTo(q);
                    }
                }
                break;

            default:
                throw new Exception("Invalid operation");
        }
    }

    // vips_math2_const_class_init

    public static void ClassInit(Type type)
    {
        VipsObjectClass objectClass = (VipsObjectClass)type;
        VipsArithmeticClass arithmeticClass = (VipsArithmeticClass)type;

        objectClass.Nickname = "math2_const";
        objectClass.Description = "binary math operations with a constant";

        arithmeticClass.ProcessLine = Buffer;
    }

    // vips_math2_const_init

    public VipsMath2Const() { }
}

public class VipsPowConst : VipsMath2Const
{
    public VipsPowConst() : base(VipsMath2Const.OperationMath2.Pow) { }
}

public class VipsWopConst : VipsMath2Const
{
    public VipsWopConst() : base(VipsMath2Const.OperationMath2.Wop) { }
}

public class VipsAtan2Const : VipsMath2Const
{
    public VipsAtan2Const() : base(VipsMath2Const.OperationMath2.Atan2) { }
}
```

Note that I've assumed the existence of a `VipsImage` class with properties like `Bands`, `Float`, and `Double`. You may need to modify the code to match your actual image class. Additionally, I've used the `foreach` loop to iterate over the enum values, which is a C#-specific feature. If you're using an older version of .NET that doesn't support this syntax, you can use a traditional `for` loop instead.

Also note that I've omitted some error handling and edge cases for brevity. You should add proper error checking and handling to make the code more robust.