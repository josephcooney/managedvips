```csharp
// vips_msb()

using System;

public class VipsMsb : VipsConversion
{
    // Params.
    public VipsImage In { get; set; }
    public int Band { get; set; }

    // Initial input offset.
    public int Offset { get; set; }

    // Input step.
    public int Instep { get; set; }

    // Need to convert signed to unsgned.
    public bool Sign { get; set; }

    protected override int Gen(VipsRegion out_region, object seq, object a, object b, ref bool stop)
    {
        VipsRegion ir = (VipsRegion)seq;
        VipsMsb msb = (VipsMsb)b;
        VipsConversion conversion = (VipsConversion)msb;
        VipsRect r = out_region.Valid;

        int le = r.Left;
        int to = r.Top;
        int bo = VIPS_RECT_BOTTOM(r);
        int sz = r.Width * conversion.Out.Bands;

        int x, y, i;

        if (vipsRegionPrepare(ir, r) != 0)
            return -1;

        for (y = to; y < bo; y++)
        {
            VipsPel[] p = vipsRegionAddr(ir, le, y);
            VipsPel[] q = vipsRegionAddr(out_region, le, y);

            if (msb.In.Coding == VIPS_CODING_LABQ && msb.Band == -1)
            {
                // LABQ, no sub-band select.
                for (x = 0; x < r.Width; x++)
                {
                    q[0] = p[0];
                    q[1] = 0x80 ^ p[1];
                    q[2] = 0x80 ^ p[2];

                    q += 4;
                    p += 3;
                }
            }
            else if (msb.Sign)
            {
                // Copy, converting signed to unsigned.
                p += msb.Offset;
                for (i = 0; i < sz; i++)
                {
                    q[i] = 0x80 ^ *p;

                    p += msb.Instep;
                }
            }
            else
            {
                // Just pick out bytes.
                p += msb.Offset;
                for (i = 0; i < sz; i++)
                {
                    q[i] = *p;

                    p += msb.Instep;
                }
            }
        }

        return 0;
    }

    protected override int Build(VipsObject object)
    {
        VipsObjectClass class_ = VIPS_OBJECT_GET_CLASS(object);
        VipsConversion conversion = (VipsConversion)object;
        VipsMsb msb = (VipsMsb)object;

        int vbands;

        if (VIPS_OBJECT_CLASS(vips_msb_parent_class).Build(object) != 0)
            return -1;

        if (vipsCheckCodingNoneOrLabq(class_.Nickname, msb.In) ||
            vipsCheckInt(class_.Nickname, msb.In))
            return -1;

        // Effective number of bands this image has.
        vbands = msb.In.Coding == VIPS_CODING_LABQ
            ? 3
            : msb.In.Bands;

        if (msb.Band > vbands - 1)
        {
            vipsError(class_.Nickname, "%s", _("bad band"));
            return -1;
        }

        // Step to next input element.
        msb.Instep = VIPS_IMAGE_SIZEOF_ELEMENT(msb.In);

        // Offset into first band element of high order byte.
        msb.Offset = vipsAmiMSBfirst()
            ? 0
            : VIPS_IMAGE_SIZEOF_ELEMENT(msb.In) - 1;

        // If we're picking out a band, they need scaling up.
        if (msb.Band != -1)
        {
            msb.Offset += VIPS_IMAGE_SIZEOF_ELEMENT(msb.In) * msb.Band;
            msb.Instep *= msb.In.Bands;
        }

        // May need to flip sign if we're picking out a band from labq.
        if (msb.In.Coding == VIPS_CODING_LABQ && msb.Band > 0)
            msb.Sign = true;
        if (msb.In.Coding == VIPS_CODING_NONE &&
            !vipsBandFormatIsUint(msb.In.BandFmt))
            msb.Sign = true;

        if (msb.Band == -1 && msb.In.BandFmt == VIPS_FORMAT_UCHAR)
            return vipsImageWrite(msb.In, conversion.Out);
        if (msb.Band == 0 && msb.In.Bands == 1 && msb.In.BandFmt == VIPS_FORMAT_UCHAR)
            return vipsImageWrite(msb.In, conversion.Out);

        if (vipsImagePipelineV(conversion.Out,
                VIPS_DEMAND_STYLE_THINSTRIP, msb.In, null) != 0)
            return -1;

        if (msb.Band != -1)
            conversion.Out.Bands = 1;
        else
            conversion.Out.Bands = vbands;
        conversion.Out.BandFmt = VIPS_FORMAT_UCHAR;
        conversion.Out.Coding = VIPS_CODING_NONE;
        if (conversion.Out.Bands == 1)
            conversion.Out.Type = VIPS_INTERPRETATION_B_W;
        else
            conversion.Out.Type = VIPS_INTERPRETATION_MULTIBAND;

        if (vipsImageGenerate(conversion.Out,
                vipsStartOne, Gen, vipsStopOne, msb.In, msb) != 0)
            return -1;

        return 0;
    }

    public static void ClassInit(VipsMsbClass class_)
    {
        GObjectClass gobject_class = G_OBJECT_CLASS(class_);
        VipsObjectClass vobject_class = VIPS_OBJECT_CLASS(class_);
        VipsOperationClass operation_class = VIPS_OPERATION_CLASS(class_);

        gobject_class.SetProperty = vipsObjectSetProperty;
        gobject_class.GetProperty = vipsObjectGetProperty;

        vobject_class.Nickname = "msb";
        vobject_class.Description =
            _("pick most-significant byte from an image");
        vobject_class.Build = Build;

        operation_class.Flags = VIPS_OPERATION_SEQUENTIAL;

        VIPS_ARG_IMAGE(class_, "in", 0,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsMsb, In));

        VIPS_ARG_INT(class_, "band", 3,
            _("Band"),
            _("Band to msb"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsMsb, Band),
            0, 100000000, 0);
    }

    public static void Init(VipsMsb msb)
    {
        msb.Band = -1;
    }
}

public class Program
{
    public static int VipsMsb(VipsImage in_, VipsImage[] out, params object[] args)
    {
        va_list ap;
        int result;

        va_start(ap, out);
        result = vipsCallSplit("msb", ap, in_, out);
        va_end(ap);

        return result;
    }
}
```