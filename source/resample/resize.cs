```csharp
// vips_resize_build: (method)
// @object: VipsObject to build
// Returns: 0 on success, -1 on error
int vips_resize_build(VipsObject object)
{
    VipsResample resample = (VipsResample)object;
    VipsResize resize = (VipsResize)object;

    VipsImage[] t = new VipsImage[5];

    // ... (rest of the method remains the same)

    return 0;
}

// vips_resize_class_init: (method)
// @class: VipsResizeClass to initialize
void vips_resize_class_init(VipsResizeClass class)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(class);
    VipsOperationClass operation_class = VIPS_OPERATION_CLASS(class);

    // ... (rest of the method remains the same)

    VIPS_ARG_DOUBLE(class, "scale", 113,
        _("Scale factor"),
        _("Scale image by this factor"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsResize, scale),
        0.0, 10000000.0, 0.0);

    // ... (rest of the method remains the same)

    // vips_resize: (method)
    // @in: input image
    // @out: (out): output image
    // @scale: scale factor
    // @...: %NULL-terminated list of optional named arguments
    //
    // Optional arguments:
    //
    // * @vscale: %gdouble vertical scale factor
    // * @kernel: #VipsKernel to reduce with
    // * @gap: reducing gap to use (default: 2.0)
    //
    // Resize an image.
    //
    // Set @gap to speed up downsizing by having vips_shrink() to shrink
    // with a box filter first. The bigger @gap, the closer the result
    // to the fair resampling. The smaller @gap, the faster resizing.
    // The default value is 2.0 (very close to fair resampling
    // while still being faster in many cases).
    //
    // vips_resize() normally uses #VIPS_KERNEL_LANCZOS3 for the final reduce, you
    // can change this with @kernel. Downsizing is done with centre convention.
    //
    // When upsizing (@scale > 1), the operation uses vips_affine() with
    // a #VipsInterpolate selected depending on @kernel. It will use
    // #VipsInterpolateBicubic for #VIPS_KERNEL_CUBIC and above. It adds a
    // 0.5 pixel displacement to the input pixels to get centre convention scaling.
    //
    // vips_resize() normally maintains the image aspect ratio. If you set
    // @vscale, that factor is used for the vertical scale and @scale for the
    // horizontal.
    //
    // If either axis would drop below 1px in size, the shrink in that dimension
    // is limited. This breaks the image aspect ratio, but prevents errors due to
    // fractional pixel sizes.
    //
    // This operation does not change xres or yres. The image resolution needs to
    // be updated by the application.
    //
    // This operation does not premultiply alpha. If your image has an alpha
    // channel, you should use vips_premultiply() on it first.
    //
    // See also: vips_premultiply(), vips_shrink(), vips_reduce().
    //
    // Returns: 0 on success, -1 on error
    int vips_resize(VipsImage in, VipsImage[] out,
        double scale, ...)
    {
        va_list ap;
        int result;

        va_start(ap, scale);
        result = vips_call_split("resize", ap, in, out, scale);
        va_end(ap);

        return result;
    }
}
```