```csharp
// vips_sink_screen() as an operation.

using System;
using VipsDotNet;

public class VipsCache : VipsConversion
{
    public VipsImage In { get; private set; }
    public int TileWidth { get; private set; } = 128;
    public int TileHeight { get; private set; } = 128;
    public int MaxTiles { get; private set; } = 250;

    protected override void Build(VipsObject obj)
    {
        VIPS_DEBUG_MSG("vips_cache_build\n");

        if (base.Build(obj))
            return;

        if (!VipsSinkScreen(In, Out, null, TileWidth, TileHeight, MaxTiles, 0, null, null))
            throw new Exception("Error in vips_sink_screen");
    }
}

public class VipsCacheClass : VipsConversionClass
{
    public static void ClassInit(VipsCacheClass klass)
    {
        GObjectClass gobject_class = (GObjectClass)klass;
        VipsObjectClass vobject_class = (VipsObjectClass)klass;
        VipsOperationClass operation_class = (VipsOperationClass)klass;

        VIPS_DEBUG_MSG("vips_cache_class_init\n");

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        vobject_class.Nickname = "cache";
        vobject_class.Description = _("cache an image");
        vobject_class.Build = new BuildDelegate(VipsCache.Build);

        operation_class.Flags |= VIPS_OPERATION_DEPRECATED;

        VipsArgImage.AddArgument(klass, "in", 1, _("Input"), _("Input image"), VIPS_ARGUMENT_REQUIRED_INPUT);
        VipsArgInt.AddArgument(klass, "tile_width", 3, _("Tile width"), _("Tile width in pixels"), VIPS_ARGUMENT_OPTIONAL_INPUT, ref TileWidth, 1, 1000000, 128);
        VipsArgInt.AddArgument(klass, "tile_height", 3, _("Tile height"), _("Tile height in pixels"), VIPS_ARGUMENT_OPTIONAL_INPUT, ref TileHeight, 1, 1000000, 128);
        VipsArgInt.AddArgument(klass, "max_tiles", 3, _("Max tiles"), _("Maximum number of tiles to cache"), VIPS_ARGUMENT_OPTIONAL_INPUT, ref MaxTiles, -1, 1000000, 1000);
    }

    public VipsCache()
    {
        TileWidth = 128;
        TileHeight = 128;
        MaxTiles = 250;
    }
}

public class VipsCacheOperation : VipsOperation
{
    // vips_cache: (method)
    // @in: input image
    // @out: (out): output image
    // @...: %NULL-terminated list of optional named arguments

    public static int Cache(VipsImage in_image, out VipsImage? out_image, params object[] args)
    {
        var cache = new VipsCache();
        cache.In = in_image;
        cache.Out = out_image;

        return VipsCallSplit("cache", args, ref cache);
    }
}
```