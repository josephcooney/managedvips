Here is the C# code equivalent to the provided C code:

```csharp
// vips_smartcrop_score method
public static int VipsSmartcropScore(VipsSmartcrop smartcrop, VipsImage inImage, int left, int top, int width, int height, out double score)
{
    var t = new VipsImage[2];
    if (VipsExtractArea(inImage, ref t[0], left, top, width, height) ||
        VipsHistFind(t[0], ref t[1]) ||
        VipsHistEntropy(t[1], out score))
    {
        return -1;
    }
    return 0;
}

// vips_smartcrop_entropy method
public static int VipsSmartcropEntropy(VipsSmartcrop smartcrop, VipsImage inImage, out int left, out int top)
{
    var maxSliceSize = Math.Max((inImage.Xsize - smartcrop.Width) / 8.0,
        (inImage.Ysize - smartcrop.Height) / 8.0);

    left = 0;
    top = 0;

    while (inImage.Xsize > smartcrop.Width || inImage.Ysize > smartcrop.Height)
    {
        var sliceWidth = Math.Min(inImage.Xsize - smartcrop.Width, maxSliceSize);
        var sliceHeight = Math.Min(inImage.Ysize - smartcrop.Height, maxSliceSize);

        if (sliceWidth > 0)
        {
            double leftScore;
            double rightScore;

            if (VipsSmartcropScore(smartcrop, inImage,
                left, top,
                sliceWidth, inImage.Ysize, out leftScore))
            {
                return -1;
            }

            if (VipsSmartcropScore(smartcrop, inImage,
                left + inImage.Xsize - sliceWidth, top,
                sliceWidth, inImage.Ysize, out rightScore))
            {
                return -1;
            }

            inImage.Xsize -= sliceWidth;
            if (leftScore < rightScore)
            {
                left += sliceWidth;
            }
        }

        if (sliceHeight > 0)
        {
            double topScore;
            double bottomScore;

            if (VipsSmartcropScore(smartcrop, inImage,
                left, top,
                inImage.Xsize, sliceHeight, out topScore))
            {
                return -1;
            }

            if (VipsSmartcropScore(smartcrop, inImage,
                left, top + inImage.Ysize - sliceHeight,
                inImage.Xsize, sliceHeight, out bottomScore))
            {
                return -1;
            }

            inImage.Ysize -= sliceHeight;
            if (topScore < bottomScore)
            {
                top += sliceHeight;
            }
        }
    }

    return 0;
}

// pythagoras method
public static int Pythagoras(VipsSmartcrop smartcrop, VipsImage inImage, out VipsImage outImage)
{
    var t = new VipsImage[2 * inImage.Bands + 1];

    for (int i = 0; i < inImage.Bands; i++)
    {
        if (VipsExtractBand(inImage, ref t[i], i) ||
            VipsMultiply(t[i], t[i], ref t[inImage.Bands + i]))
        {
            return -1;
        }
    }

    if (VipsSum(ref t[inImage.Bands], ref outImage, inImage.Bands) ||
        VipsPowConst1(outImage, 0.5))
    {
        return -1;
    }

    return 0;
}

// vips_smartcrop_attention method
public static int VipsSmartcropAttention(VipsSmartcrop smartcrop, VipsImage inImage, out int left, out int top, out int attentionX, out int attentionY)
{
    var t = new VipsImage[24];

    double hscale;
    double vscale;
    double sigma;
    double max;
    int x_pos;
    int y_pos;

    // The size we shrink to gives the precision with which we can place
    // the crop
    hscale = 32.0 / inImage.Xsize;
    vscale = 32.0 / inImage.Ysize;
    sigma = Math.Sqrt(Math.Pow(smartcrop.Width * hscale, 2) +
        Math.Pow(smartcrop.Height * vscale, 2));
    sigma = Math.Max(sigma / 10, 1.0);

    if (VipsResize(inImage, ref t[17], hscale,
        "vscale", vscale))
    {
        return -1;
    }

    // Simple edge detect.
    var edgeDetectMatrix = new VipsImage(3, 3);
    edgeDetectMatrix.SetData(new double[]
    {
        0.0, -1.0, 0.0,
        -1.0, 4.0, -1.0,
        0.0, -1.0, 0.0
    });

    // Convert to XYZ and just use the first three bands.
    if (VipsColourspace(t[17], ref t[0], VIPS_INTERPRETATION_XYZ) ||
        VipsExtractBand(t[0], ref t[1], 0, "n", 3))
    {
        return -1;
    }

    // Edge detect on Y.
    if (VipsExtractBand(t[1], ref t[2], 1) ||
        VipsConv(t[2], ref t[3], edgeDetectMatrix,
            "precision", VIPS_PRECISION_INTEGER) ||
        VipsLinear1(t[3], ref t[4], 5.0, 0.0) ||
        VipsAbs(t[4], ref t[14]))
    {
        return -1;
    }

    // Look for skin colours. Taken from smartcrop.js.
    if (
        // Normalise to magnitude of colour in XYZ.
        Pythagoras(smartcrop, t[1], ref t[5]) ||
        VipsDivide(t[1], t[5], ref t[6])
        ||
        // Distance from skin point.
        VipsLinear(t[6], ref t[7], new double[] { 1.0, 1.0, 1.0 }, new double[] { -0.78, -0.57, -0.44 }, 3) ||
        Pythagoras(smartcrop, t[7], ref t[8])
        ||
        // Rescale to 100 - 0 score.
        VipsLinear1(t[8], ref t[9], -100.0, 100.0)
        ||
        // Ignore dark areas.
        VipsMoreConst1(t[2], ref t[10], 5.0) ||
        (t[11] = new VipsImage(inImage))
        ||
        VipsIfThenElse(t[10], t[9], t[11], ref t[15]))
    {
        return -1;
    }

    // Look for saturated areas.
    if (VipsColourspace(t[1], ref t[12],
            VIPS_INTERPRETATION_LAB) ||
        VipsExtractBand(t[12], ref t[13], 1) ||
        VipsIfThenElse(t[10], t[13], t[11], ref t[16]))
    {
        return -1;
    }

    // Sum, blur and find maxpos.
    //
    // The amount of blur is related to the size of the crop
    // area: how large an area we want to consider for the scoring
    // function.

    if (VipsSum(ref t[14], ref t[18], 3) ||
        VipsGaussBlur(t[18], ref outImage, sigma))
    {
        return -1;
    }

    if (VipsMax(outImage, out max, "x", out x_pos, "y", out y_pos))
    {
        return -1;
    }

    // Transform back into image coordinates.
    attentionX = x_pos / hscale;
    attentionY = y_pos / vscale;

    // Centre the crop over the max.
    left = Math.Max(0,
        attentionX - smartcrop.Width / 2);
    top = Math.Max(0,
        attentionY - smartcrop.Height / 2);

    return 0;
}

// vips_smartcrop_build method
public static int VipsSmartcropBuild(VipsObject object)
{
    var conversion = (VipsConversion)object;
    var smartcrop = (VipsSmartcrop)object;

    var t = new VipsImage[2];

    if (VIPS_OBJECT_CLASS(vips_smartcrop_parent_class).build(object))
    {
        return -1;
    }

    if (smartcrop.Width > inImage.Xsize ||
        smartcrop.Height > inImage.Ysize ||
        smartcrop.Width <= 0 || smartcrop.Height <= 0)
    {
        vips_error("vips_smartcrop", "%s", _("bad extract area"));
        return -1;
    }

    var in = smartcrop.in;

    // If there's an alpha, we have to premultiply before searching for
    // content. There could be stuff in transparent areas which we don't
    // want to consider.
    if (VipsImageHasAlpha(in) && !smartcrop.premultiplied)
    {
        if (VipsPremultiply(in, ref t[0]))
        {
            return -1;
        }
        in = t[0];
    }

    switch (smartcrop.interesting)
    {
        case VIPS_INTERESTING_NONE:
        case VIPS_INTERESTING_LOW:
            left = 0;
            top = 0;
            break;

        case VIPS_INTERESTING_CENTRE:
            left = (inImage.Xsize - smartcrop.Width) / 2;
            top = (inImage.Ysize - smartcrop.Height) / 2;
            break;

        case VIPS_INTERESTING_ENTROPY:
            if (VipsSmartcropEntropy(smartcrop, in, out left, out top))
            {
                return -1;
            }
            break;

        case VIPS_INTERESTING_ATTENTION:
            if (VipsSmartcropAttention(smartcrop, in,
                    out left, out top,
                    out attentionX, out attentionY))
            {
                return -1;
            }
            break;

        case VIPS_INTERESTING_HIGH:
            left = inImage.Xsize - smartcrop.Width;
            top = inImage.Ysize - smartcrop.Height;
            break;

        case VIPS_INTERESTING_ALL:
            left = 0;
            top = 0;
            smartcrop.Width = inImage.Xsize;
            smartcrop.Height = inImage.Ysize;
            break;

        default:
            g_assert_not_reached();

            // Stop a compiler warning.
            left = 0;
            top = 0;
            break;
    }

    object.SetProperty(smartcrop,
        "attention_x", attentionX,
        "attention_y", attentionY);

    if (VipsExtractArea(in, ref t[1],
            left, top,
            smartcrop.Width, smartcrop.Height) ||
        VipsImageWrite(t[1], conversion.out))
    {
        return -1;
    }

    return 0;
}

// vips_smartcrop_class_init method
public static void VipsSmartcropClassInit(VipsObjectClass class)
{
    var gobjectClass = G_OBJECT_CLASS(class);
    var vobjectClass = VIPS_OBJECT_CLASS(class);

    VIPS_DEBUG_MSG("vips_smartcrop_class_init\n");

    gobjectClass.set_property = vips_object_set_property;
    gobjectClass.get_property = vips_object_get_property;

    vobjectClass.nickname = "smartcrop";
    vobjectClass.description = _("extract an area from an image");
    vobjectClass.build = VipsSmartcropBuild;

    // Add properties
    VIPS_ARG_IMAGE(class, "input", 0,
        _("Input"),
        _("Input image"),
        VIPS_ARGUMENT_REQUIRED_INPUT);

    VIPS_ARG_INT(class, "width", 4,
        _("Width"),
        _("Width of extract area"),
        VIPS_ARGUMENT_REQUIRED_INPUT);

    VIPS_ARG_INT(class, "height", 5,
        _("Height"),
        _("Height of extract area"),
        VIPS_ARGUMENT_REQUIRED_INPUT);

    VIPS_ARG_ENUM(class, "interesting", 6,
        _("Interesting"),
        _("How to measure interestingness"),
        VIPS_ARGUMENT_OPTIONAL_INPUT);

    VIPS_ARG_INT(class, "attention_x", 2,
        _("Attention x"),
        _("Horizontal position of attention centre"),
        VIPS_ARGUMENT_OPTIONAL_OUTPUT);

    VIPS_ARG_INT(class, "attention_y", 3,
        _("Attention y"),
        _("Vertical position of attention centre"),
        VIPS_ARGUMENT_OPTIONAL_OUTPUT);

    VIPS_ARG_BOOL(class, "premultiplied", 7,
        _("Premultiplied"),
        _("Input image already has premultiplied alpha"));
}

// vips_smartcrop_init method
public static void VipsSmartcropInit(VipsObject object)
{
    var smartcrop = (VipsSmartcrop)object;
    smartcrop.interesting = VIPS_INTERESTING_ATTENTION;
    smartcrop.premultiplied = false;
}

// vips_smartcrop method
public static int VipsSmartcrop(VipsImage inImage, out VipsImage outImage, int width, int height)
{
    var result = VipsCallSplit("smartcrop", inImage, out outImage, width, height);
    return result;
}
```