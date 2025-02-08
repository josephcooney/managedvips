```csharp
// @(#) convert float Lab to packed Lab32 format 10 11 11 bits
// works only on buffers, not IMAGEs
// Copyright 1993 K.Martinez
// Modified: 3/5/93, 16/6/93
public class VipsLab2LabQ : VipsColourCode
{
    public override void ProcessLine(VipsPel[] outArray, float[] inArray, int width)
    {
        // Scale L up to 10 bits.
        for (int i = 0; i < width; i++)
        {
            float fval;
            int lsbs;
            int intv;

            intv = VIPS_ROUND_UINT(10.23 * inArray[0]);
            intv = VIPS_CLIP(0, intv, 1023);
            lsbs = (intv & 0x3) << 6; // 00000011 -> 11000000
            outArray[0] = (VipsPel)(intv >> 2); // drop bot 2 bits and store

            fval = 8.0f * inArray[1]; // do a
            intv = VIPS_RINT(fval);
            intv = VIPS_CLIP(-1024, intv, 1023);
            lsbs |= (intv & 0x7) << 3; // 00000111 -> 00111000
            outArray[1] = (VipsPel)(intv >> 3); // drop bot 3 bits & store

            fval = 8.0f * inArray[2]; // do b
            intv = VIPS_RINT(fval);
            intv = VIPS_CLIP(-1024, intv, 1023);
            lsbs |= (intv & 0x7);
            outArray[2] = (VipsPel)(intv >> 3);

            outArray[3] = (VipsPel)lsbs; // store lsb band

            inArray += 3;
            outArray += 4;
        }
    }

    public static void Vips__Lab2LabQ_vec(VipsPel[] out, float[] in, int width)
    {
        ProcessLine(out, in, width);
    }
}

public class VipsLab2LabQClass : VipsColourCodeClass
{
    public override void ClassInit()
    {
        base.ClassInit();
        VipsObjectClass object_class = (VipsObjectClass)this;
        VipsColourClass colour_class = VIPS_COLOUR_CLASS(this);

        object_class.Nickname = "Lab2LabQ";
        object_class.Description = "transform float Lab to LabQ coding";

        colour_class.ProcessLine = new VipsColourCode.ProcessLineDelegate(VipsLab2LabQ.ProcessLine);
    }

    public override void Init()
    {
        base.Init();
        VipsColour colour = (VipsColour)this;
        VipsColourCode code = (VipsColourCode)this;

        colour.Coding = VIPS_CODING_LABQ;
        colour.Interpretation = VIPS_INTERPRETATION_LABQ;
        colour.Format = VIPS_FORMAT_UCHAR;
        colour.InputBands = 3;
        colour.Bands = 4;

        code.InputCoding = VIPS_CODING_NONE;
        code.InputFormat = VIPS_FORMAT_FLOAT;
    }
}

public class VipsLab2LabQ : VipsObject
{
    public override int Call(VipsImage inImage, out VipsImage[] outImages)
    {
        return VipsCallSplit("Lab2LabQ", inImage, outImages);
    }

    // vips_Lab2LabQ: (method)
    // @in: input image
    // @out: (out): output image
    // @...: %NULL-terminated list of optional named arguments
    //
    // Convert a Lab three-band float image to LabQ (#VIPS_CODING_LABQ).
    //
    // See also: vips_LabQ2Lab().
    //
    // Returns: 0 on success, -1 on error.
}
```