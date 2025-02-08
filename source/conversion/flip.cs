Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsFlip : VipsConversion
{
    public VipsImage In { get; set; }
    public VipsDirection Direction { get; set; }

    public override int Build(VipsObject object)
    {
        VipsConversion conversion = (VipsConversion)object;
        VipsFlip flip = (VipsFlip)object;

        if (base.Build(object) != 0)
            return -1;

        if (vips_image_pio_input(flip.In))
            return -1;

        if (vips_image_pipelinev(conversion.Out, VIPS_DEMAND_STYLE_THINSTRIP, flip.In, null) != 0)
            return -1;

        if (flip.Direction == VIPS_DIRECTION_HORIZONTAL)
        {
            GenerateFn generate_fn = vips_flip_horizontal_gen;
            conversion.Out.Xoffset = flip.In.Xsize;
            conversion.Out.Yoffset = 0;
        }
        else
        {
            GenerateFn generate_fn = vips_flip_vertical_gen;
            conversion.Out.Xoffset = 0;
            conversion.Out.Yoffset = flip.In.Ysize;
        }

        if (vips_image_generate(conversion.Out, vips_start_one, generate_fn, vips_stop_one, flip.In, flip) != 0)
            return -1;

        return 0;
    }
}

public class VipsFlipClass : VipsConversionClass
{
    public static void ClassInit(VipsFlipClass klass)
    {
        GObjectClass gobject_class = (GObjectClass)klass;
        VipsObjectClass vobject_class = (VipsObjectClass)klass;

        VIPS_DEBUG_MSG("vips_flip_class_init\n");

        gobject_class.SetProperty += new PropertyHandler(vips_object_set_property);
        gobject_class.GetProperty += new PropertyHandler(vips_object_get_property);

        vobject_class.Nickname = "flip";
        vobject_class.Description = _("flip an image");
        vobject_class.Build = new BuildHandler(vips_flip_build);

        VIPS_ARG_IMAGE(klass, "in", 1,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsFlip).GetField("In").GetOffset());

        VIPS_ARG_ENUM(klass, "direction", 6,
            _("Direction"),
            _("Direction to flip image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipsFlip).GetField("Direction").GetOffset(),
            typeof(VipsDirection), VIPS_DIRECTION_HORIZONTAL);
    }
}

public class VipsFlipInit : VipsConversion
{
    public override void Init()
    {
        // empty implementation
    }
}

public static class VipsFlipMethods
{
    [Method]
    public static int Flip(VipsImage in_image, out VipsImage out_image, VipsDirection direction)
    {
        va_list ap;
        int result;

        va_start(ap, direction);
        result = vips_call_split("flip", ap, in_image, out_image, direction);
        va_end(ap);

        return result;
    }
}

public delegate int GenerateFn(VipsRegion out_region, void* seq, void* a, void* b, bool* stop);
```

Note that the `VIPS_TYPE_DIRECTION` and `VIPS_DEMAND_STYLE_THINSTRIP` constants are not defined in this code snippet. They should be replaced with their actual values or definitions.

Also note that the `vips_object_set_property`, `vips_object_get_property`, `vips_flip_build`, `vips_start_one`, and `vips_stop_one` methods are not implemented here, as they are assumed to be defined elsewhere in the codebase.