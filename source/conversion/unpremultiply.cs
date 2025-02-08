```csharp
// vips_unpremultiply_gen: (method)
// @in_region: input region
// @vseq: sequence of regions to process
// @a: unused argument
// @b: unpremultiply object
// @stop: stop flag
public static int VipsUnpremultiplyGen(VipsRegion inRegion, IntPtr vseq, IntPtr a, IntPtr b, ref bool stop)
{
    VipsUnpremultiply unpremultiply = (VipsUnpremultiply)b;
    VipsRegion ir = (VipsRegion)vseq;
    VipsImage im = ir.im;
    VipsRect r = inRegion.valid;
    int width = r.width;
    int bands = im.Bands;
    double maxAlpha = unpremultiply.max_alpha;
    int alphaBand = unpremultiply.alpha_band;

    int x, y, i;

    if (VipsRegionPrepare(ir, ref r))
        return -1;

    for (y = 0; y < r.height; y++)
    {
        VipsPel[] inArray = VipsRegionAddr(ir, r.left, r.top + y);
        VipsPel[] outArray = VipsRegionAddr(inRegion, r.left, r.top + y);

        switch (im.BandFmt)
        {
            case VipsFormat.UChar:
                UnpremultiplyUcharFloat(inArray, outArray);
                break;

            case VipsFormat.Char:
                UnpremultiplyCharFloat(inArray, outArray);
                break;

            case VipsFormat.UShort:
                UnpremultiplyUshortFloat(inArray, outArray);
                break;

            case VipsFormat.Short:
                UnpremultiplyShortFloat(inArray, outArray);
                break;

            case VipsFormat.UInt:
                UnpremultiplyUIntFloat(inArray, outArray);
                break;

            case VipsFormat.Int:
                UnpremultiplyIntFloat(inArray, outArray);
                break;

            case VipsFormat.Float:
                FunpremultiplyFloatFloat(inArray, outArray);
                break;

            case VipsFormat.Double:
                FunpremultiplyDoubleDouble(inArray, outArray);
                break;

            default:
                g_assert_not_reached();
        }
    }

    return 0;
}

// vips_unpremultiply_build: (method)
// @object: object to build
public static int VipsUnpremultiplyBuild(VipsObject object)
{
    VipsObjectClass class = VipsObjectGetClass(object);
    VipsConversion conversion = VipsConversion(object);
    VipsUnpremultiply unpremultiply = (VipsUnpremultiply)object;
    VipsImage[] t = new VipsImage[1];

    VipsImage in;

    if (VipsObjectClass.VipsUnpremultiplyParentClass.Build(object) != 0)
        return -1;

    in = unpremultiply.in;

    if (VipsImageDecode(in, ref t[0]) != 0)
        return -1;
    in = t[0];

    // Trivial case: fall back to copy().
    if (in.Bands == 1)
        return VipsImageWrite(in, conversion.out);

    if (VipsCheckNoncomplex(class.nickname, in) != 0)
        return -1;

    if (VipsImagePipelinev(conversion.out, VipsDemandStyle.ThinStrip, in, null) != 0)
        return -1;

    // Is max-alpha unset? Default to the correct value for this interpretation.
    if (!VipsObjectArgumentIsSet(object, "max_alpha"))
        unpremultiply.max_alpha = VipsInterpretationMaxAlpha(in.Type);

    // Is alpha-band unset? Default to the final band for this image.
    if (!VipsObjectArgumentIsSet(object, "alpha_band"))
        unpremultiply.alpha_band = in.Bands - 1;

    if (in.BandFmt == VipsFormat.Double)
        conversion.out.BandFmt = VipsFormat.Double;
    else
        conversion.out.BandFmt = VipsFormat.Float;

    if (VipsImageGenerate(conversion.out, VipsStartOne, VipsUnpremultiplyGen, VipsStopOne, in, unpremultiply) != 0)
        return -1;

    return 0;
}

// vips_unpremultiply_class_init: (method)
public static void VipsUnpremultiplyClassInit(VipsUnpremultiplyClass class)
{
    GObjectClass gobjectClass = GObjectClass(class);
    VipsObjectClass vobjectClass = VipsObjectClass(class);
    VipsOperationClass operationClass = VipsOperationClass(class);

    VIPS_DEBUG_MSG("vips_unpremultiply_class_init\n");

    gobjectClass.SetProperty = VipsObjectSetProperty;
    gobjectClass.GetProperty = VipsObjectGetProperty;

    vobjectClass.Nickname = "unpremultiply";
    vobjectClass.Description = _("unpremultiply image alpha");
    vobjectClass.Build = VipsUnpremultiplyBuild;

    operationClass.Flags = VipsOperationSequencial;

    VIPS_ARG_IMAGE(class, "in", 1,
        _("Input"),
        _("Input image"),
        VipsArgumentRequiredInput,
        G_STRUCT_OFFSET(VipsUnpremultiply, in));

    VIPS_ARG_DOUBLE(class, "max_alpha", 115,
        _("Maximum alpha"),
        _("Maximum value of alpha channel"),
        VipsArgumentOptionalInput,
        G_STRUCT_OFFSET(VipsUnpremultiply, max_alpha),
        0.0, 100000000.0, 255.0);

    VIPS_ARG_INT(class, "alpha_band", 116,
        _("Alpha band"),
        _("Unpremultiply with this alpha"),
        VipsArgumentOptionalInput,
        G_STRUCT_OFFSET(VipsUnpremultiply, alpha_band),
        0, 100000000, 3);
}

// vips_unpremultiply_init: (method)
public static void VipsUnpremultiplyInit(VipsUnpremultiply unpremultiply)
{
    unpremultiply.max_alpha = 255.0;
}

// vips_unpremultiply: (method)
// @in: input image
// @out: output image
// @...: %NULL-terminated list of optional named arguments
public static int VipsUnpremultiply(VipsImage in, ref VipsImage out, params object[] args)
{
    var result = VipsCallSplit("unpremultiply", args, in, ref out);
    return result;
}

void UnpremultiplyUcharFloat(VipsPel[] inArray, VipsPel[] outArray)
{
    for (int x = 0; x < inArray.Length; x++)
    {
        int alpha = inArray[alphaBand];
        float factor = alpha == 0 ? 0 : maxAlpha / alpha;

        for (int i = 0; i < alphaBand; i++)
            outArray[i] = factor * inArray[i];

        outArray[alphaBand] = VipsClip(0, alpha, maxAlpha);
        for (int i = alphaBand + 1; i < bands; i++)
            outArray[i] = inArray[i];
    }
}

void UnpremultiplyCharFloat(VipsPel[] inArray, VipsPel[] outArray)
{
    // ...
}

// ... other unpremultiply functions ...

void FunpremultiplyFloatFloat(VipsPel[] inArray, VipsPel[] outArray)
{
    for (int x = 0; x < inArray.Length; x++)
    {
        float alpha = inArray[alphaBand];
        float factor = Math.Abs(alpha) < 0.01 ? 0 : maxAlpha / alpha;

        for (int i = 0; i < alphaBand; i++)
            outArray[i] = factor * inArray[i];

        outArray[alphaBand] = VipsClip(0, alpha, maxAlpha);
        for (int i = alphaBand + 1; i < bands; i++)
            outArray[i] = inArray[i];
    }
}

void FunpremultiplyDoubleDouble(VipsPel[] inArray, VipsPel[] outArray)
{
    // ...
}
```