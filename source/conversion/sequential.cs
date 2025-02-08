Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

public class VipsSequential : VipsConversion
{
    public VipsImage In { get; private set; }
    public int TileHeight { get; private set; }
    public VipsAccess Access { get; private set; }
    public bool Trace { get; private set; }

    private Mutex Lock;
    private int YPos;
    private int Error;

    public VipsSequential(VipsImage inImage, int tileHeight = 1)
        : base(inImage)
    {
        In = inImage;
        TileHeight = tileHeight;
        Access = VipsAccess.Sequential;
        Trace = false;
        Lock = new Mutex();
        YPos = 0;
        Error = 0;
    }

    public override int Generate(VipsRegion outRegion, object seq, object a, object b, bool[] stop)
    {
        var sequential = (VipsSequential)b;
        var r = outRegion.Valid;
        var ir = (VipsRegion)seq;

        VIPS_GATE_START("vips_sequential_generate: wait");

        Lock.WaitOne();

        VIPS_GATE_STOP("vips_sequential_generate: wait");

        // If we've seen an error, everything must stop.
        if (sequential.Error != 0)
        {
            Lock.ReleaseMutex();
            return -1;
        }

        if (r.Top > sequential.YPos)
        {
            // This is a request for something some way down the image.
            // Probably the operation is something like extract_area and
            // we should skip the initial part of the image. In fact,
            // we read to cache, since it may be useful.

            int y;

            for (y = sequential.YPos; y < r.Top; y += sequential.TileHeight)
            {
                var area = new VipsRect();
                area.Left = 0;
                area.Top = y;
                area.Width = 1;
                area.Height = Math.Min(sequential.TileHeight, r.Top - area.Top);
                if (VipsRegion.Prepare(ir, ref area))
                {
                    sequential.Error = -1;
                    Lock.ReleaseMutex();
                    return -1;
                }

                sequential.YPos += area.Height;
            }
        }

        // This is a request for old pixels, or for pixels exactly at the read
        // point. This might trigger a generate from the thing feeding the cache,
        // eg. a loader.

        if (VipsRegion.Prepare(ir, ref r) || VipsRegion.Region(outRegion, ir, r, r.Left, r.Top))
        {
            sequential.Error = -1;
            Lock.ReleaseMutex();
            return -1;
        }

        sequential.YPos = Math.Max(sequential.YPos, VipsRect.Bottom(r));

        Lock.ReleaseMutex();

        return 0;
    }

    public override int Build(VipsObject obj)
    {
        var conversion = (VipsConversion)obj;
        var sequential = (VipsSequential)obj;

        var t = new VipsImage();
        if (!VipsLinecache(sequential.In, ref t,
            "tile_height", sequential.TileHeight,
            "access", VipsAccess.Sequential,
            null))
        {
            return -1;
        }

        VipsObject.Local(obj, t);

        if (VipsImage.Pipelinev(conversion.Out,
            VipsDemandStyle.ThinStrip, t, null))
        {
            return -1;
        }
        if (VipsImage.Generate(conversion.Out,
            VipsStartOne, sequential.Generate, VipsStopOne,
            t, sequential))
        {
            return -1;
        }

        return 0;
    }

    public static void ClassInit(Type type)
    {
        var gobjectClass = (GObjectClass)type.GetMethod("ClassInit").GetParameters()[0].ParameterType;
        var vobjectClass = (VipsObjectClass)gobjectClass;

        gobjectClass.Dispose += VipsSequential.Dispose;
        gobjectClass.SetProperty += VipsObject.SetProperty;
        gobjectClass.GetProperty += VipsObject.GetProperty;

        vobjectClass.Nickname = "sequential";
        vobjectClass.Description = "check sequential access";
        vobjectClass.Build = VipsSequential.Build;

        VipsArg.Image(type, "in", 1,
            "Input",
            "Input image",
            VipsArgument.RequiredInput);

        VipsArg.Int(type, "tile_height", 3,
            "Tile height",
            "Tile height in pixels",
            VipsArgument.OptionalInput,
            1, 1000000, 1);

        VipsArg.Enum(type, "access", 6,
            "Strategy",
            "Expected access pattern",
            VipsArgument.OptionalInput | VipsArgument.Deprecated,
            VipsType.Access, VipsAccess.Sequential);
    }

    public static void Init(VipsSequential sequential)
    {
        sequential.Lock = new Mutex();
        sequential.TileHeight = 1;
        sequential.Error = 0;
        sequential.Trace = false;
    }
}

public class VipsSequentialClass : VipsConversionClass
{
    public override Type Type => typeof(VipsSequential);
}

public static class VipsSequentialExtensions
{
    public static int Sequential(VipsImage inImage, out VipsImage[] outImages)
    {
        var sequential = new VipsSequential(inImage);
        return VipsCallSplit("sequential", null, inImage, outImages);
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsConversion`, `VipsObject`, `VipsRegion`, etc.) which are not shown here, as they would require a significant amount of additional code to implement.