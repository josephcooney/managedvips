Here is the C# code equivalent to the provided C code:

```csharp
// boolean.cs --- various bit operations

using System;
using System.Collections.Generic;

public class VipsBoolean : VipsBinary
{
    public enum Operation { AND, OR, EOR, LSHIFT, RSHIFT };

    private Operation operation;

    public VipsBoolean(Operation op)
    {
        this.operation = op;
    }

    // vips_boolean_build()
    public override int Build(VipsObject obj)
    {
        VipsBinary binary = (VipsBinary)obj;
        if (binary.Left != null && !VipsCheckNonComplex(binary.Nickname, binary.Left))
            return -1;
        if (binary.Right != null && !VipsCheckNonComplex(binary.Nickname, binary.Right))
            return -1;

        // Call parent class build method
        int result = base.Build(obj);
        if (result == 0)
            return 0;

        return -1;
    }

    // vips_boolean_buffer()
    public override void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays, int width)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int sz = width * im.Bands;

        switch (operation)
        {
            case Operation.AND:
                SwitchLoop(LOOP, FLOOP, &);
                break;
            case Operation.OR:
                SwitchLoop(LOOP, FLOOP, |);
                break;
            case Operation.EOR:
                SwitchLoop(LOOP, FLOOP, ^);
                break;
            case Operation.LSHIFT:
                switch (im.Format)
                {
                    case VipsFormat.UCHAR:
                        Loop<unsigned char>(<<);
                        break;
                    case VipsFormat.CHAR:
                        FNLoop(signed char, VIPS_LSHIFT_INT);
                        break;
                    case VipsFormat.USHORT:
                        Loop<unsigned short>(<<);
                        break;
                    case VipsFormat.SHORT:
                        FNLoop(signed short, VIPS_LSHIFT_INT);
                        break;
                    case VipsFormat.UINT:
                        Loop<unsigned int>(<<);
                        break;
                    case VipsFormat.INT:
                        FNLoop(signed int, VIPS_LSHIFT_INT);
                        break;
                    case VipsFormat.FLOAT:
                        FLoop<float>(<<);
                        break;
                    case VipsFormat.DOUBLE:
                        FLoop<double>(<<);
                        break;

                    default:
                        throw new ArgumentException("Invalid image format");
                }
                break;
            case Operation.RSHIFT:
                SwitchLoop(LOOP, FLOOP, >>);
                break;

            default:
                throw new ArgumentException("Invalid operation");
        }
    }

    // vips_boolean()
    public static int Boolean(VipsImage left, VipsImage right, ref VipsImage outImage, Operation op)
    {
        return VipsCallSplit("boolean", left, right, ref outImage, op);
    }

    // vips_andimage()
    public static int Andimage(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return Boolean(left, right, ref outImage, Operation.AND);
    }

    // vips_orimage()
    public static int Orimage(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return Boolean(left, right, ref outImage, Operation.OR);
    }

    // vips_eorimage()
    public static int Eorimage(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return Boolean(left, right, ref outImage, Operation.EOR);
    }

    // vips_lshift()
    public static int Lshift(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return Boolean(left, right, ref outImage, Operation.LSHIFT);
    }

    // vips_rshift()
    public static int Rshift(VipsImage left, VipsImage right, ref VipsImage outImage)
    {
        return Boolean(left, right, ref outImage, Operation.RSHIFT);
    }
}

// vips_boolean_const.cs

public class VipsBooleanConst : VipsUnaryConst
{
    private Operation operation;

    public VipsBooleanConst(Operation op)
    {
        this.operation = op;
    }

    // vips_boolean_const_build()
    public override int Build(VipsObject obj)
    {
        VipsUnary unary = (VipsUnary)obj;
        if (unary.In != null && !VipsCheckNonComplex(unary.Nickname, unary.In))
            return -1;

        // Call parent class build method
        int result = base.Build(obj);
        if (result == 0)
            return 0;

        return -1;
    }

    // vips_boolean_const_buffer()
    public override void Buffer(VipsArithmetic arithmetic, VipsPel[] outArray, VipsPel[][] inArrays)
    {
        VipsImage im = (VipsImage)arithmetic.Ready[0];
        int bands = im.Bands;

        switch (operation)
        {
            case Operation.AND:
                SwitchLoop(LOOPC, FLOOPC, &);
                break;
            case Operation.OR:
                SwitchLoop(LOOPC, FLOOPC, |);
                break;
            case Operation.EOR:
                SwitchLoop(LOOPC, FLOOPC, ^);
                break;
            case Operation.LSHIFT:
                SwitchLoop(LOOPC, FLOOPC, <<);
                break;
            case Operation.RSHIFT:
                SwitchLoop(LOOPC, FLOOPC, >>);
                break;

            default:
                throw new ArgumentException("Invalid operation");
        }
    }

    // vips_boolean_const()
    public static int BooleanConst(VipsImage inImage, ref VipsImage outImage, Operation op, double[] c)
    {
        return VipsCallSplit("boolean_const", inImage, ref outImage, op, c);
    }

    // vips_andimage_const()
    public static int AndimageConst(VipsImage inImage, ref VipsImage outImage, double[] c)
    {
        return BooleanConst(inImage, ref outImage, Operation.AND, c);
    }

    // vips_orimage_const()
    public static int OrimageConst(VipsImage inImage, ref VipsImage outImage, double[] c)
    {
        return BooleanConst(inImage, ref outImage, Operation.OR, c);
    }

    // vips_eorimage_const()
    public static int EorimageConst(VipsImage inImage, ref VipsImage outImage, double[] c)
    {
        return BooleanConst(inImage, ref outImage, Operation.EOR, c);
    }

    // vips_lshift_const()
    public static int LshiftConst(VipsImage inImage, ref VipsImage outImage, double[] c)
    {
        return BooleanConst(inImage, ref outImage, Operation.LSHIFT, c);
    }

    // vips_rshift_const()
    public static int RshiftConst(VipsImage inImage, ref VipsImage outImage, double[] c)
    {
        return BooleanConst(inImage, ref outImage, Operation.RSHIFT, c);
    }
}

// Macros

#define LOOP(TYPE, OP) \
{ \
    TYPE *restrict left = (TYPE *) in[0]; \
    TYPE *restrict right = (TYPE *) in[1]; \
    TYPE *restrict q = (TYPE *) out; \
\
    for (x = 0; x < sz; x++) \
        q[x] = left[x] OP right[x]; \
}

#define FLOOP(TYPE, OP) \
{ \
    TYPE *restrict left = (TYPE *) in[0]; \
    TYPE *restrict right = (TYPE *) in[1]; \
    int *restrict q = (int *) out; \
\
    for (x = 0; x < sz; x++) \
        q[x] = ((int) left[x]) OP((int) right[x]); \
}

#define LOOPC(TYPE, OP) \
{ \
    TYPE *restrict p = (TYPE *) in[0]; \
    TYPE *restrict q = (TYPE *) out; \
    int *restrict c = uconst->c_int; \
\
    for (i = 0, x = 0; x < width; x++) \
        for (b = 0; b < bands; b++, i++) \
            q[i] = p[i] OP c[b]; \
}

#define FLOOPC(TYPE, OP) \
{ \
    TYPE *restrict p = (TYPE *) in[0]; \
    int *restrict q = (int *) out; \
    int *restrict c = uconst->c_int; \
\
    for (i = 0, x = 0; x < width; x++) \
        for (b = 0; b < bands; b++, i++) \
            q[i] = ((int) p[i]) OP((int) c[b]); \
}

#define SwitchLoop(I, F, OP) \
switch (im.Format) { \
    case VipsFormat.UCHAR: \
        I(unsigned char, OP); \
        break; \
    case VipsFormat.CHAR: \
        F(signed char, OP); \
        break; \
    case VipsFormat.USHORT: \
        I(unsigned short, OP); \
        break; \
    case VipsFormat.SHORT: \
        F(signed short, OP); \
        break; \
    case VipsFormat.UINT: \
        I(unsigned int, OP); \
        break; \
    case VipsFormat.INT: \
        F(signed int, OP); \
        break; \
    case VipsFormat.FLOAT: \
        F(float, OP); \
        break; \
    case VipsFormat.DOUBLE: \
        F(double, OP); \
        break; \
\
    default: \
        throw new ArgumentException("Invalid image format"); \
}
```