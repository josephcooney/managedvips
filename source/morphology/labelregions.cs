```csharp
// labelregions.cs

using System;

public class VipsLabelregions : VipsMorphology
{
    private VipsImage _mask;
    private int _segments;

    public VipsLabelregions(VipsImage inImage) 
        : base(inImage)
    {
    }

    // vips_labelregions_build
    protected override int Build()
    {
        var morphology = (VipsMorphology)this;
        var inImage = morphology.In;
        var t = new VipsImage[2];
        var mask = new VipsImage();

        int segments = 1;
        int[] m;

        if (!base.Build())
            return -1;

        // Create the zero mask image in memory.
        mask = new VipsImage();
        this.Mask = mask;
        if (!VipsBlack(t[0], inImage.Xsize, inImage.Ysize) ||
            !VipsCast(t[0], t[1], VIPS_FORMAT_INT) ||
            !VipsImageWrite(t[1], mask))
            return -1;

        m = (int[])mask.Data;
        for (int y = 0; y < mask.Ysize; y++)
        {
            for (int x = 0; x < mask.Xsize; x++)
            {
                if (!m[x])
                {
                    // Use a direct path for speed.
                    if (!Vips__DrawFloodDirect(mask, inImage, segments, x, y))
                        return -1;

                    segments += 1;
                }
            }

            m += mask.Xsize;
        }

        this.Segments = segments;

        return 0;
    }

    // vips_labelregions_class_init
    public static void ClassInit(Type type)
    {
        var gobjectClass = (GObjectClass)type.GetMethod("GetClass").Invoke(null, null);
        var vobjectClass = (VipsObjectClass)gobjectClass.GetType().GetMethod("GetClass").Invoke(null, null);

        gobjectClass.SetProperty = VipsObject.SetProperty;
        gobjectClass.GetProperty = VipsObject.GetProperty;

        vobjectClass.Nickname = "labelregions";
        vobjectClass.Description = "label regions in an image";
        vobjectClass.Build = Build;

        VIPS_ARG_IMAGE(type, "mask", 2,
            "Mask",
            "Mask of region labels",
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            typeof(VipsLabelregions).GetField("Mask").GetOffset());

        VIPS_ARG_INT(type, "segments", 3,
            "Segments",
            "Number of discrete contiguous regions",
            VIPS_ARGUMENT_OPTIONAL_OUTPUT,
            typeof(VipsLabelregions).GetField("Segments").GetOffset(),
            0, 1000000000, 0);
    }

    // vips_labelregions_init
    public VipsLabelregions()
    {
    }

    // vips_labelregions
    public static int LabelRegions(VipsImage inImage, out VipsImage mask, params object[] args)
    {
        var labelregions = new VipsLabelregions(inImage);
        return labelregions.Build();
    }
}
```