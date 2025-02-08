```csharp
// vips_clamp_buffer
void vipsClampBuffer(VipsArithmetic arithmetic, VipsPel[] out, VipsPel[][] in, int width)
{
    VipsClamp clamp = (VipsClamp)arithmetic;
    VipsImage im = (VipsImage)arithmetic.Ready[0];
    int bands = im.GetBands();
    int sz = width * bands * (im.IsComplex() ? 2 : 1);

    switch (im.GetFormat())
    {
        case VIPS_FORMAT_CHAR:
            ClampLine((signed char[])in[0], out, sz);
            break;

        case VIPS_FORMAT_UCHAR:
            ClampLine((unsigned char[])in[0], out, sz);
            break;

        case VIPS_FORMAT_SHORT:
            ClampLine((short[])in[0], out, sz);
            break;

        case VIPS_FORMAT_USHORT:
            ClampLine((ushort[])in[0], out, sz);
            break;

        case VIPS_FORMAT_INT:
            ClampLine((int[])in[0], out, sz);
            break;

        case VIPS_FORMAT_UINT:
            ClampLine((uint[])in[0], out, sz);
            break;

        case VIPS_FORMAT_FLOAT:
            ClampLine((float[])in[0], out, sz);
            break;

        case VIPS_FORMAT_DOUBLE:
            ClampLine((double[])in[0], out, sz);
            break;

        case VIPS_FORMAT_COMPLEX:
            ClampLine((float[])in[0], out, sz);
            break;

        case VIPS_FORMAT_DPCOMPLEX:
            ClampLine((double[])in[0], out, sz);
            break;

        default:
            throw new ArgumentException("Invalid image format");
    }
}

// CLAMP_LINE macro
void ClampLine<T>(T[] in, T[] out, int sz) where T : struct
{
    for (int x = 0; x < sz; x++)
    {
        out[x] = VipsClip(clamp.Min, in[x], clamp.Max);
    }
}

// vips_clamp_format_table
VipsBandFormat[] vipsClampFormatTable = new VipsBandFormat[]
{
    // Band format: UC  C  US  S  UI  I  F  X  D  DX
    // Promotion:
    VIPS_FORMAT_UCHAR, VIPS_FORMAT_CHAR, VIPS_FORMAT_USHORT,
    VIPS_FORMAT_SHORT, VIPS_FORMAT_UINT, VIPS_FORMAT_INT,
    VIPS_FORMAT_FLOAT, VIPS_FORMAT_COMPLEX, VIPS_FORMAT_DOUBLE,
    VIPS_FORMAT_DPCOMPLEX
};

// vips_clamp_class_init
void vipsClampClassInit(VipsClampClass class)
{
    GObjectClass gobjectClass = (GObjectClass)class;
    VipsObjectClass objectClass = (VipsObjectClass)class;
    VipsArithmeticClass aClass = VIPS_ARITHMETIC_CLASS(class);

    gobjectClass.SetProperty = vipsObjectSetProperty;
    gobjectClass.GetProperty = vipsObjectGetProperty;

    objectClass.Nickname = "clamp";
    objectClass.Description = "Clamp values of an image";

    aClass.ProcessLine = vipsClampBuffer;

    VIPS_ARG_DOUBLE(class, "min", 10,
        "Min",
        "Minimum value",
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsClamp, min),
        double.MinValue, double.MaxValue, 0.0);

    VIPS_ARG_DOUBLE(class, "max", 11,
        "Max",
        "Maximum value",
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsClamp, max),
        double.MinValue, double.MaxValue, 0.0);
}

// vips_clamp_init
void vipsClampInit(VipsClamp clamp)
{
    clamp.Min = 0.0;
    clamp.Max = 1.0;
}

// vips_clamp
int vipsClamp(VipsImage in, VipsImage[] out, params object[] args)
{
    return vipsCallSplit("clamp", args, in, out);
}
```