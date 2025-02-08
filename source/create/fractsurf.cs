```csharp
// Converted from: vips_fractsurf_build()

public class VipsFractalsurfBuild : VipsObject
{
    public int Build(VipsImage[] t)
    {
        // Check if the parent build method returns an error
        if (VIPS_OBJECT_CLASS(vips_fractsurf_parent_class).build(this))
            return -1;

        // Call vips_gaussnoise()
        if (!vips_gaussnoise(t[0], Width, Height, "mean", 0.0, "sigma", 1.0, null))
            return -1;

        // Call vips_mask_fractal()
        if (!vips_mask_fractal(t[1], Width, Height, FractalDimension, null))
            return -1;

        // Call vips_freqmult()
        if (!vips_freqmult(t[0], t[1], t[2], null))
            return -1;

        // Call vips_image_write()
        if (!vips_image_write(t[2], Create.Out))
            return -1;

        return 0;
    }
}

// Converted from: vips_fractsurf_class_init()

public class VipsFractalsurfClass : VipsObjectClass
{
    public VipsFractalsurfClass()
    {
        // Set the nickname and description of the object
        Nickname = "fractsurf";
        Description = _("make a fractal surface");

        // Set the build method of the object
        Build = new VipsFractalsurfBuild();

        // Define the properties of the object
        VIPS_ARG_INT(this, "width", 4,
            _("Width"),
            _("Image width in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsFractalsurf, Width),
            1, VIPS_MAX_COORD, 64);

        VIPS_ARG_INT(this, "height", 5,
            _("Height"),
            _("Image height in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsFractalsurf, Height),
            1, VIPS_MAX_COORD, 64);

        VIPS_ARG_DOUBLE(this, "fractal_dimension", 8,
            _("Fractal dimension"),
            _("Fractal dimension"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsFractalsurf, FractalDimension),
            2.0, 3.0, 2.5);
    }
}

// Converted from: vips_fractsurf_init()

public class VipsFractalsurf : VipsObject
{
    public VipsFractalsurf()
    {
        // Initialize the properties of the object
        Width = 64;
        Height = 64;
        FractalDimension = 2.5;
    }
}

// Converted from: vips_fractsurf()

public class VipsFractalsurf : VipsObject
{
    public int Fractalsurf(VipsImage[] out, int width, int height, double fractal_dimension)
    {
        // Call the build method of the object
        return Build(out);
    }
}
```