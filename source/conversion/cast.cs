Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Runtime.InteropServices;

public class VipsCast : VipsConversion
{
    public VipsImage In { get; set; }
    public VipsBandFormat Format { get; set; }
    public bool Shift { get; set; }

    public override int Build(VipsObject obj)
    {
        VipsConversion conversion = (VipsConversion)obj;
        VipsImage[] t = new VipsImage[2];

        if (base.Build(obj))
            return -1;

        In = Cast.In;

        // Trivial case: fall back to copy().
        if (In.BandFmt == Format)
            return vips_image_write(In, conversion.Out);

        if (vips_image_decode(In, t))
            return -1;
        In = t[0];

        // If @shift is on but we're not in an int format and we're going to
        // an int format, we need to cast to int first. For example, what
        // about a float image tagged as rgb16 being cast to uint8? We need
        // to cast to ushort before we do the final cast to uint8.
        if (Shift &&
            !VipsBandFormat.IsInt(In.BandFmt) &&
            VipsBandFormat.IsInt(Format))
        {
            if (vips_cast(In, t[1], VipsImage.GuessFormat(In), null))
                return -1;
            In = t[1];
        }

        if (vips_image_pipelinev(conversion.Out,
            VipsDemandStyle.ThinStrip, In, null))
            return -1;

        conversion.Out.BandFmt = Format;

        if (vips_image_generate(conversion.Out,
            vips_start_one, vips_cast_gen, vips_stop_one,
            In, this))
            return -1;

        return 0;
    }

    public override int CastGen(VipsRegion out_region,
        void* vseq, void* a, void* b, bool* stop)
    {
        VipsRegion ir = (VipsRegion)vseq;
        VipsCast cast = (VipsCast)b;
        VipsConversion conversion = (VipsConversion)b;
        VipsRect r = out_region.Valid;
        int sz = VipsRegion.NElements(out_region);

        if (vips_region_prepare(ir, r))
            return -1;

        VIPS_GATE_START("vips_cast_gen: work");

        for (int y = 0; y < r.Height; y++)
        {
            VipsPel[] inArray = VipsRegion.Addr(ir, r.Left, r.Top + y);
            VipsPel[] outArray = VipsRegion.Addr(out_region, r.Left, r.Top + y);

            switch (ir.Im.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    BandSwitchInner(unsigned char,
                        IntInt,
                        CastRealFloat,
                        CastRealComplex);
                    break;

                // ... rest of the cases ...
            }
        }

        VIPS_GATE_STOP("vips_cast_gen: work");

        return 0;
    }

    public override int CastBuild(VipsObject obj)
    {
        VipsConversion conversion = (VipsConversion)obj;
        VipsCast cast = (VipsCast)obj;

        // ... rest of the code ...
    }
}

public class VipsBandFormat
{
    public static bool IsInt(VipsBandFormat fmt)
    {
        return fmt == VIPS_FORMAT_UCHAR ||
            fmt == VIPS_FORMAT_CHAR ||
            fmt == VIPS_FORMAT_USHORT ||
            fmt == VIPS_FORMAT_SHORT ||
            fmt == VIPS_FORMAT_UINT ||
            fmt == VIPS_FORMAT_INT;
    }
}

public class VipsConversion
{
    public VipsImage Out { get; set; }

    // ... rest of the code ...
}

// ... rest of the code ...

[DllImport("vips")]
private static extern int vips_image_write(VipsImage inImg, VipsImage outImg);

[DllImport("vips")]
private static extern int vips_image_decode(VipsImage inImg, VipsImage[] t);

[DllImport("vips")]
private static extern int vips_image_pipelinev(VipsImage outImg,
    VipsDemandStyle style, VipsImage inImg, IntPtr callback);

[DllImport("vips")]
private static extern int vips_image_generate(VipsImage outImg,
    VipsStartFunc start, VipsGenFunc gen, VipsStopFunc stop,
    VipsImage inImg, object data);

// ... rest of the code ...
```

Note that this is not a complete implementation and you will need to add the missing parts. Also, some types like `VipsRegion`, `VipsPel`, `VipsRect` are assumed to be defined elsewhere.

Also note that I've used the `DllImport` attribute to import the native functions from the VIPS library. You may need to adjust this depending on your specific setup.

The `BandSwitchInner` method is not implemented here, you will need to implement it according to the C code.

Please let me know if you have any questions or need further clarification.