```csharp
// Converted from: vips_countlines_build()

public class VipsCountlinesBuild : VipsMorphologyBuild
{
    public override int Build(VipsObject object)
    {
        var morphology = (VipsMorphology)object;
        var countlines = (VipsCountlines)object;
        var inImage = morphology.In;
        var tImages = new VipsImage[7];

        double nolines;

        if (base.Build(object) != 0)
            return -1;

        // Compiler warnings.
        nolines = 1.0;

        switch (countlines.Direction)
        {
            case VipsDirection.Horizontal:
                if (!(tImages[0] = new VipsImage(1, 2)) ||
                    !VipsMoreEqConst1(inImage, ref tImages[1], 128) ||
                    !VipsConv(tImages[1], ref tImages[2], tImages[0],
                        "precision", VipsPrecision.Integer,
                        null) ||
                    !VipsProject(tImages[2], ref tImages[3], ref tImages[4], null) ||
                    !VipsAvg(tImages[3], ref nolines, null))
                {
                    return -1;
                }
                break;

            case VipsDirection.Vertical:
                if (!(tImages[0] = new VipsImage(2, 1)) ||
                    !VipsMoreEqConst1(inImage, ref tImages[1], 128) ||
                    !VipsConv(tImages[1], ref tImages[2], tImages[0],
                        "precision", VipsPrecision.Integer,
                        null) ||
                    !VipsProject(tImages[2], ref tImages[3], ref tImages[4], null) ||
                    !VipsAvg(tImages[4], ref nolines, null))
                {
                    return -1;
                }
                break;

            default:
                g_assert_not_reached();
                break;
        }

        object.SetProperty("nolines", nolines / 255.0);

        return 0;
    }
}

// Converted from: vips_countlines_class_init()

public class VipsCountlinesClass : VipsMorphologyClass
{
    public override void ClassInit(VipsCountlinesClass klass)
    {
        base.ClassInit(klass);

        // VIPS_DEBUG_MSG("vips_countlines_class_init\n");

        klass.Nickname = "countlines";
        klass.Description = "count lines in an image";

        VipsObjectClass vobjectClass = (VipsObjectClass)klass;
        vobjectClass.Build = new VipsCountlinesBuild();

        VipsArgDouble argNolines = new VipsArgDouble(
            "nolines",
            2,
            "Number of lines",
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsCountlines, nolines),
            0.0,
            10000000.0);

        VipsArgEnum argDirection = new VipsArgEnum(
            "direction",
            3,
            "Direction",
            "Count lines left-right or up-down",
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsCountlines, direction),
            typeof(VipsDirection),
            VipsDirection.Horizontal);
    }
}

// Converted from: vips_countlines_init()

public class VipsCountlines : VipsMorphology
{
    public override void Init()
    {
        base.Init();
    }
}

// Converted from: vips_countlines()

public static int VipsCountlines(VipsImage inImage, double[] nolines,
    VipsDirection direction)
{
    var countlines = new VipsCountlines();
    countlines.Direction = direction;

    return countlines.Build(inImage);
}
```