```csharp
// vips_merge_build (converted from vips_merge.c)

public class VipsMerge : VipsOperation
{
    public VipsImage Ref { get; set; }
    public VipsImage Sec { get; set; }
    public VipsImage Out { get; set; }
    public VipsDirection Direction { get; set; }
    public int Dx { get; set; }
    public int Dy { get; set; }
    public int Mblend { get; set; }

    protected override int Build()
    {
        // Create a new output image
        Out = new VipsImage();

        // Call the parent class's build method
        if (base.Build() != 0)
            return -1;

        switch (Direction)
        {
            case VipsDirection.Horizontal:
                // Call vips__lrmerge to merge the two images horizontally
                if (Vips.__LrMerge(Ref, Sec, Out, Dx, Dy, Mblend) != 0)
                    return -1;
                break;

            case VipsDirection.Vertical:
                // Call vips__tbmerge to merge the two images vertically
                if (Vips.__TbMerge(Ref, Sec, Out, Dx, Dy, Mblend) != 0)
                    return -1;
                break;

            default:
                // Should not reach here
                g_assert_not_reached();
        }

        // Add a mosaic name to the output image
        Vips.AddMosaicName(Out);

        // Set the history of the output image
        if (Vips.ImageHistoryPrintf(Out,
            "#%s <%s> <%s> <%s> <%d> <%d> <%d>",
            Direction == VipsDirection.Horizontal ? "LRJOIN" : "TBJOIN",
            Vips.GetMosaicName(Ref),
            Vips.GetMosaicName(Sec),
            Vips.GetMosaicName(Out),
            -Dx, -Dy, Mblend) != 0)
            return -1;

        return 0;
    }
}

// vips_merge_class_init (converted from vips_merge.c)

public class VipsMergeClass : VipsOperationClass
{
    public static void ClassInit(VipsMergeClass* klass)
    {
        // Set the nickname and description of the operation
        klass.Nickname = "merge";
        klass.Description = _("merge two images");

        // Set the build method of the operation
        klass.Build = new Func<VipsObject, int>(VipsMerge.Build);

        // Define the properties of the operation
        VIPS_ARG_IMAGE(klass, "ref", 1,
            _("Reference"),
            _("Reference image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMerge, Ref));

        VIPS_ARG_IMAGE(klass, "sec", 2,
            _("Secondary"),
            _("Secondary image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMerge, Sec));

        VIPS_ARG_IMAGE(klass, "out", 3,
            _("Output"),
            _("Output image"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsMerge, Out));

        VIPS_ARG_ENUM(klass, "direction", 4,
            _("Direction"),
            _("Horizontal or vertical merge"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMerge, Direction),
            typeof(VipsDirection), VipsDirection.Horizontal);

        VIPS_ARG_INT(klass, "dx", 5,
            _("dx"),
            _("Horizontal displacement from sec to ref"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMerge, Dx),
            -100000000, 1000000000, 1);

        VIPS_ARG_INT(klass, "dy", 6,
            _("dy"),
            _("Vertical displacement from sec to ref"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMerge, Dy),
            -100000000, 1000000000, 1);

        VIPS_ARG_INT(klass, "mblend", 7,
            _("Max blend"),
            _("Maximum blend size"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsMerge, Mblend),
            0, 10000, 10);
    }
}

// vips_merge_init (converted from vips_merge.c)

public class VipsMerge : VipsOperation
{
    public VipsMerge()
    {
        // Initialize the mblend property to 10
        Mblend = 10;
    }
}

// vips_merge (converted from vips_merge.c)

public static int Merge(VipsImage ref, VipsImage sec, out VipsImage out,
    VipsDirection direction, int dx, int dy)
{
    // Call the vips_call_split function to split the arguments
    var args = new object[] { ref, sec, out, direction, dx, dy };
    return Vips.CallSplit("merge", args);
}
```