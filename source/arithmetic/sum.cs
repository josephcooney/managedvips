```csharp
// sum_buffer (from sum.c)
void sum_buffer(VipsArithmetic arithmetic, VipsPel[] out, VipsPel[][] in, int width)
{
    VipsImage im = (VipsImage)arithmetic.ready[0];
    int n = arithmetic.n;

    // Complex just doubles the size.
    const int sz = width * vips_image_get_bands(im) *
        (vips_band_format_iscomplex(vips_image_get_format(im)) ? 2 : 1);

    int x;
    int i;

    // Sum all input types. Keep types here in sync with
    // vips_sum_format_table[] below.
    switch (vips_image_get_format(im))
    {
        case VIPS_FORMAT_UCHAR:
            LOOP(unsigned char, unsigned int);
            break;
        case VIPS_FORMAT_CHAR:
            LOOP(signed char, signed int);
            break;
        case VIPS_FORMAT_USHORT:
            LOOP(unsigned short, unsigned int);
            break;
        case VIPS_FORMAT_SHORT:
            LOOP(signed short, signed int);
            break;
        case VIPS_FORMAT_UINT:
            LOOP(unsigned int, unsigned int);
            break;
        case VIPS_FORMAT_INT:
            LOOP(signed int, signed int);
            break;

        case VIPS_FORMAT_FLOAT:
        case VIPS_FORMAT_COMPLEX:
            LOOP(float, float);
            break;

        case VIPS_FORMAT_DOUBLE:
        case VIPS_FORMAT_DPCOMPLEX:
            LOOP(double, double);
            break;

        default:
            g_assert_not_reached();
    }
}

// vips_sum_class_init (from sum.c)
void vips_sum_class_init(VipsSumClass class)
{
    VipsObjectClass object_class = (VipsObjectClass)class;
    VipsArithmeticClass aclass = VIPS_ARITHMETIC_CLASS(class);

    object_class.nickname = "sum";
    object_class.description = _("sum an array of images");

    aclass.process_line = sum_buffer;

    vips_arithmetic_set_format_table(aclass, vips_sum_format_table);
}

// vips_sum_init (from sum.c)
void vips_sum_init(VipsSum sum)
{
}

// vips_sumv (from sum.c)
int vips_sumv(VipsImage[] in, VipsImage[] out, int n, params)
{
    VipsArrayImage array;
    int result;

    array = vips_array_image_new(in, n);
    result = vips_call_split("sum", params, array, out);
    vips_area_unref(VIPS_AREA(array));

    return result;
}

// vips_sum (from sum.c)
int vips_sum(VipsImage[] in, VipsImage[] out, int n, params)
{
    va_list ap;
    int result;

    va_start(ap, n);
    result = vips_sumv(in, out, n, ap);
    va_end(ap);

    return result;
}

// vips_sum_format_table (from sum.c)
static readonly VipsBandFormat[] vips_sum_format_table =
{
    // Band format:  UC  C  US  S  UI  I  F  X  D  DX
    // Promotion:
    VIPS_FORMAT_UI, VIPS_FORMAT_I, VIPS_FORMAT_UI, VIPS_FORMAT_I,
    VIPS_FORMAT_UI, VIPS_FORMAT_I, VIPS_FORMAT_F, VIPS_FORMAT_X,
    VIPS_FORMAT_D, VIPS_FORMAT_DX
};
```