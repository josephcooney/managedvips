Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipssRGB2scRGB : VipsOperation
{
    public VipsImage in_;
    public VipsImage out_;

    protected override int Build(VipsObject obj)
    {
        var t = new VipsImage[2];
        var in_ = (VipsImage)VipsObject.GetLocalArray(obj, 0)[0];
        if (VipsObject.CheckBandsAtLeast(Nickname, in_, 3))
            return -1;

        // we are changing the gamma, so any profile on the image can no longer
        // work (and will cause horrible problems in any downstream colour
        // handling)
        if (VipsImage.Copy(in_, t, null))
            return -1;
        in_ = t[0];
        VipsImage.Remove(in_, "icc_name");

        var format = in_.Type == VIPS_INTERPRETATION_RGB16 ? VIPS_FORMAT_USHORT : VIPS_FORMAT_UCHAR;
        if (in_.BandFmt != format)
        {
            if (VipsImage.Cast(in_, t, format, null))
                return -1;
            in_ = t[0];
        }

        var out = new VipsImage();
        if (VipsImage.Pipeline(out, VIPS_DEMAND_STYLE_THINSTRIP, in_, null))
        {
            out.Dispose();
            return -1;
        }
        out.Type = VIPS_INTERPRETATION_scRGB;
        out.BandFmt = VIPS_FORMAT_FLOAT;

        if (VipsImage.Generate(out, StartOne, SRGB2scRGBGen, StopOne, in_, this))
        {
            out.Dispose();
            return -1;
        }

        Object.SetProperty(obj, "out", out);

        return 0;
    }

    public override void ClassInit(VipssRGB2scRGBClass klass)
    {
        var gobject_class = (GObjectClass)klass;
        var object_class = (VipsObjectClass)klass;
        var operation_class = (VipsOperationClass)klass;

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        object_class.Nickname = "sRGB2scRGB";
        object_class.Description = "convert an sRGB image to scRGB";
        object_class.Build = Build;

        operation_class.Flags = VIPS_OPERATION_SEQUENTIAL;

        VipsArgImage.AddClassArgument(klass, "in", 1,
            "Input",
            "Input image",
            VIPS_ARGUMENT_REQUIRED_INPUT,
            typeof(VipssRGB2scRGB).GetProperty("in_"));

        VipsArgImage.AddClassArgument(klass, "out", 100,
            "Output",
            "Output image",
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            typeof(VipssRGB2scRGB).GetProperty("out_"));
    }

    public override void Init()
    {
    }

    private static int SRGB2scRGBGen(VipsRegion out_region, object seq, object a, object b, bool* stop)
    {
        var ir = (VipsRegion)seq;
        var r = &out_region.Valid;
        var in_ = ((VipsImage)ir.Im);

        if (VipsRegion.Prepare(ir, r))
            return -1;

        VIPS_GATE_START("vips_sRGB2scRGB_gen: work");

        if (in_.BandFmt == VIPS_FORMAT_UCHAR)
        {
            VipsCol.MakeTables_RGB_8();

            for (int y = 0; y < r.Height; y++)
            {
                var p = VipsRegion.Addr(ir, r.Left, r.Top + y);
                float[] q = (float[])VipsRegion.Addr(out_region, r.Left, r.Top + y);

                SRGB2scRGBLine_8(q, p, in_.Bands - 3, r.Width);
            }
        }
        else
        {
            VipsCol.MakeTables_RGB_16();

            for (int y = 0; y < r.Height; y++)
            {
                var p = VipsRegion.Addr(ir, r.Left, r.Top + y);
                float[] q = (float[])VipsRegion.Addr(out_region, r.Left, r.Top + y);

                SRGB2scRGBLine_16(q, (ushort[])p, in_.Bands - 3, r.Width);
            }
        }

        VIPS_GATE_STOP("vips_sRGB2scRGB_gen: work");

        return 0;
    }

    private static void SRGB2scRGBLine_8(float[] q, byte[] p, int extra_bands, int width)
    {
        if (extra_bands == 0)
        {
            for (int i = 0; i < width; i++)
            {
                q[0] = VipsV2Y_8[p[0]];
                q[1] = VipsV2Y_8[p[1]];
                q[2] = VipsV2Y_8[p[2]];

                p += 3;
                q += 3;
            }
        }
        else if (extra_bands == 1)
        {
            for (int i = 0; i < width; i++)
            {
                q[0] = VipsV2Y_8[p[0]];
                q[1] = VipsV2Y_8[p[1]];
                q[2] = VipsV2Y_8[p[2]];
                q[3] = p[3] / 255.0;

                p += 4;
                q += 4;
            }
        }
        else
        {
            for (int i = 0; i < width; i++)
            {
                q[0] = VipsV2Y_8[p[0]];
                q[1] = VipsV2Y_8[p[1]];
                q[2] = VipsV2Y_8[p[2]];

                p += 3;
                q += 3;

                for (int j = 0; j < extra_bands; j++)
                    q[j] = p[j] / 255.0;
                p += extra_bands;
                q += extra_bands;
            }
        }
    }

    private static void SRGB2scRGBLine_16(float[] q, ushort[] p, int extra_bands, int width)
    {
        if (extra_bands == 0)
        {
            for (int i = 0; i < width; i++)
            {
                q[0] = VipsV2Y_16[p[0]];
                q[1] = VipsV2Y_16[p[1]];
                q[2] = VipsV2Y_16[p[2]];

                p += 3;
                q += 3;
            }
        }
        else if (extra_bands == 1)
        {
            for (int i = 0; i < width; i++)
            {
                q[0] = VipsV2Y_16[p[0]];
                q[1] = VipsV2Y_16[p[1]];
                q[2] = VipsV2Y_16[p[2]];
                q[3] = p[3] / 65535.0;

                p += 4;
                q += 4;
            }
        }
        else
        {
            for (int i = 0; i < width; i++)
            {
                q[0] = VipsV2Y_16[p[0]];
                q[1] = VipsV2Y_16[p[1]];
                q[2] = VipsV2Y_16[p[2]];

                p += 3;
                q += 3;

                for (int j = 0; j < extra_bands; j++)
                    q[j] = p[j] / 65535.0;
                p += extra_bands;
                q += extra_bands;
            }
        }
    }

    public static int VipsSRGB2scRGB(VipsImage in_, VipsImage[] out, params object[] args)
    {
        var result = VipsCallSplit("sRGB2scRGB", args, in_, out);
        return result;
    }
}
```