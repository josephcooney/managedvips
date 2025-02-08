Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsConvf : VipsConvolution
{
    public int Nnz { get; private set; }
    public double[] Coeff { get; private set; }
    public int[] CoeffPos { get; private set; }

    protected override bool Build()
    {
        var convolution = (VipsConvolution)this;
        var t = new VipsImage[4];

        var in_ = convolution.In;
        var M = convolution.M;

        var coeff = (double[])VIPS_IMAGE_ADDR(M, 0, 0);
        var ne = M.Xsize * M.Ysize;

        // Bake the scale into the mask.
        var scale = VipsImage.GetScale(M);
        for (var i = 0; i < ne; i++)
            coeff[i] /= scale;

        if (!(Coeff = new double[ne]) || !(CoeffPos = new int[ne]))
            return false;

        // Find non-zero mask elements.
        Nnz = 0;
        for (var i = 0; i < ne; i++)
            if (coeff[i] != 0)
            {
                Coeff[Nnz] = coeff[i];
                CoeffPos[Nnz] = i;
                Nnz++;
            }

        // Was the whole mask zero? We must have at least 1 element
        // in there: set it to zero.
        if (Nnz == 0)
        {
            Coeff[0] = 0;
            CoeffPos[0] = 0;
            Nnz = 1;
        }

        var result = VipsEmbed(in_, t, M.Xsize / 2, M.Ysize / 2,
            in_.Xsize + M.Xsize - 1, in_.Ysize + M.Ysize - 1,
            "extend", VIPS_EXTEND_COPY, null);
        if (result != 0)
            return false;

        var out_ = new VipsImage();
        result = VipsImagePipelinev(convolution.Out,
            VIPS_DEMAND_STYLE_SMALLTILE, t[0], null);
        if (result != 0)
            return false;

        convolution.Out.Xoffset = 0;
        convolution.Out.Yoffset = 0;

        // Prepare output. Consider a 7x7 mask and a 7x7 image --- the output
        // would be 1x1.
        if (VipsBandFormat.IsInt(in_.BandFmt))
            convolution.Out.BandFmt = VIPS_FORMAT_FLOAT;
        convolution.Out.Xsize -= M.Xsize - 1;
        convolution.Out.Ysize -= M.Ysize - 1;

        result = VipsImageGenerate(convolution.Out,
            VipsConvfStart, VipsConvfGen, null, in_, this);
        if (result != 0)
            return false;

        convolution.Out.Xoffset = -M.Xsize / 2;
        convolution.Out.Yoffset = -M.Ysize / 2;

        return true;
    }
}

public class VipsConvfSequence
{
    public VipsConvf Convf { get; private set; }
    public VipsRegion Ir { get; private set; }

    public int[] Offsets { get; private set; }
    public int LastBpl { get; private set; }

    public void Stop()
    {
        VIPS_UNREF(Ir);
    }
}

public class VipsConvfStart
{
    public object Start(VipsImage out_, object a, object b)
    {
        var in_ = (VipsImage)a;
        var convf = (VipsConvf)b;

        if (!(var seq = new VipsConvfSequence()))
            return null;

        seq.Convf = convf;
        seq.Ir = null;
        seq.LastBpl = -1;

        seq.Ir = VipsRegion.New(in_);
        if (!(seq.Offsets = new int[convf.Nnz]))
            vips_convf_stop(seq, in_, convf);
        return (object)seq;
    }
}

public class VipsConvfGen
{
    public int Gen(VipsRegion out_region,
        object vseq, object a, object b, bool[] stop)
    {
        var seq = (VipsConvfSequence)vseq;
        var convf = (VipsConvf)b;
        var convolution = (VipsConvolution)convf;
        var M = convolution.M;
        var offset = VipsImage.GetOffset(M);
        var t = convf.Coeff;
        var nnz = convf.Nnz;
        var ir = seq.Ir;

        var r = out_region.Valid;
        var le = r.Left;
        var to = r.Top;
        var bo = VIPS_RECT_BOTTOM(r);
        var sz = VipsRegion.NElements(out_region) *
            (VipsBandFormat.IsComplex(in_.BandFmt) ? 2 : 1);

        var s = new VipsRect();
        var x, y, z, i;

        // Prepare the section of the input image we need. A little larger
        // than the section of the output image we are producing.
        s = r;
        s.Width += M.Xsize - 1;
        s.Height += M.Ysize - 1;
        if (VipsRegion.Prepare(ir, ref s))
            return -1;

        // Fill offset array. Only do this if the bpl has changed since the
        // previous VipsRegion.Prepare().
        if (seq.LastBpl != VIPS_REGION_LSKIP(ir))
        {
            seq.LastBpl = VIPS_REGION_LSKIP(ir);

            for (i = 0; i < nnz; i++)
            {
                z = convf.CoeffPos[i];
                x = z % M.Xsize;
                y = z / M.Xsize;

                seq.Offsets[i] =
                    (VipsRegion.Addr(ir, x + le, y + to) -
                        VipsRegion.Addr(ir, le, to)) /
                    VipsImage.SizeOfElement(ir.Im);
            }
        }

        VIPS_GATE_START("vips_convf_gen: work");

        for (y = to; y < bo; y++)
        {
            switch (in_.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    CONV_FLOAT(unsigned char, float);
                    break;

                case VIPS_FORMAT_CHAR:
                    CONV_FLOAT(signed char, float);
                    break;

                case VIPS_FORMAT_USHORT:
                    CONV_FLOAT(unsigned short, float);
                    break;

                case VIPS_FORMAT_SHORT:
                    CONV_FLOAT(signed short, float);
                    break;

                case VIPS_FORMAT_UINT:
                    CONV_FLOAT(unsigned int, float);
                    break;

                case VIPS_FORMAT_INT:
                    CONV_FLOAT(signed int, float);
                    break;

                case VIPS_FORMAT_FLOAT:
                case VIPS_FORMAT_COMPLEX:
                    CONV_FLOAT(float, float);
                    break;

                case VIPS_FORMAT_DOUBLE:
                case VIPS_FORMAT_DPCOMPLEX:
                    CONV_FLOAT(double, double);
                    break;

                default:
                    g_assert_not_reached();
            }
        }

        VIPS_GATE_STOP("vips_convf_gen: work");

        VIPS_COUNT_PIXELS(out_region, "vips_convf_gen");

        return 0;
    }
}

static void ConvFloat<TIn, TOut>(TIn p, TOut q)
{
    var sum = offset;
    for (var i = 0; i < nnz; i++)
        sum += t[i] * p[offsets[i]];

    q[x] = sum;
    p += 1;
}

public class VipsConvf : VipsObject
{
    public static void ClassInit(Type type)
    {
        var object_class = (VipsObjectClass)type;

        object_class.Nickname = "convf";
        object_class.Description = _("float convolution operation");
        object_class.Build = new Func<VipsObject, bool>(vips_convf_build);
    }

    public static void Init(VipsConvf convf)
    {
        convf.Nnz = 0;
        convf.Coeff = null;
        convf.CoeffPos = null;
    }
}

public class VipsConvfBuild
{
    public int Build(VipsObject obj)
    {
        var convolution = (VipsConvolution)obj;
        var t = new VipsImage[4];

        var in_ = convolution.In;
        var M = convolution.M;

        var coeff = (double[])VIPS_IMAGE_ADDR(M, 0, 0);
        var ne = M.Xsize * M.Ysize;

        // Bake the scale into the mask.
        var scale = VipsImage.GetScale(M);
        for (var i = 0; i < ne; i++)
            coeff[i] /= scale;

        if (!(convf.Coeff = new double[ne]) || !(convf.CoeffPos = new int[ne]))
            return -1;

        // Find non-zero mask elements.
        convf.Nnz = 0;
        for (var i = 0; i < ne; i++)
            if (coeff[i] != 0)
            {
                convf.Coeff[convf.Nnz] = coeff[i];
                convf.CoeffPos[convf.Nnz] = i;
                convf.Nnz++;
            }

        // Was the whole mask zero? We must have at least 1 element
        // in there: set it to zero.
        if (convf.Nnz == 0)
        {
            convf.Coeff[0] = 0;
            convf.CoeffPos[0] = 0;
            convf.Nnz = 1;
        }

        var result = VipsEmbed(in_, t, M.Xsize / 2, M.Ysize / 2,
            in_.Xsize + M.Xsize - 1, in_.Ysize + M.Ysize - 1,
            "extend", VIPS_EXTEND_COPY, null);
        if (result != 0)
            return -1;

        var out_ = new VipsImage();
        result = VipsImagePipelinev(convolution.Out,
            VIPS_DEMAND_STYLE_SMALLTILE, t[0], null);
        if (result != 0)
            return -1;

        convolution.Out.Xoffset = 0;
        convolution.Out.Yoffset = 0;

        // Prepare output. Consider a 7x7 mask and a 7x7 image --- the output
        // would be 1x1.
        if (VipsBandFormat.IsInt(in_.BandFmt))
            convolution.Out.BandFmt = VIPS_FORMAT_FLOAT;
        convolution.Out.Xsize -= M.Xsize - 1;
        convolution.Out.Ysize -= M.Ysize - 1;

        result = VipsImageGenerate(convolution.Out,
            VipsConvfStart, VipsConvfGen, null, in_, convf);
        if (result != 0)
            return -1;

        convolution.Out.Xoffset = -M.Xsize / 2;
        convolution.Out.Yoffset = -M.Ysize / 2;

        return 0;
    }
}

public class VipsConvfStart
{
    public object Start(VipsImage out_, object a, object b)
    {
        var in_ = (VipsImage)a;
        var convf = (VipsConvf)b;

        if (!(var seq = new VipsConvfSequence()))
            return null;

        seq.Convf = convf;
        seq.Ir = null;
        seq.LastBpl = -1;

        seq.Ir = VipsRegion.New(in_);
        if (!(seq.Offsets = new int[convf.Nnz]))
            vips_convf_stop(seq, in_, convf);
        return (object)seq;
    }
}

public class VipsConvfGen
{
    public int Gen(VipsRegion out_region,
        object vseq, object a, object b, bool[] stop)
    {
        var seq = (VipsConvfSequence)vseq;
        var convf = (VipsConvf)b;
        var convolution = (VipsConvolution)convf;
        var M = convolution.M;
        var offset = VipsImage.GetOffset(M);
        var t = convf.Coeff;
        var nnz = convf.Nnz;
        var ir = seq.Ir;

        var r = out_region.Valid;
        var le = r.Left;
        var to = r.Top;
        var bo = VIPS_RECT_BOTTOM(r);
        var sz = VipsRegion.NElements(out_region) *
            (VipsBandFormat.IsComplex(in_.BandFmt) ? 2 : 1);

        var s = new VipsRect();
        var x, y, z, i;

        // Prepare the section of the input image we need. A little larger
        // than the section of the output image we are producing.
        s = r;
        s.Width += M.Xsize - 1;
        s.Height += M.Ysize - 1;
        if (VipsRegion.Prepare(ir, ref s))
            return -1;

        // Fill offset array. Only do this if the bpl has changed since the
        // previous VipsRegion.Prepare().
        if (seq.LastBpl != VIPS_REGION_LSKIP(ir))
        {
            seq.LastBpl = VIPS_REGION_LSKIP(ir);

            for (i = 0; i < nnz; i++)
            {
                z = convf.CoeffPos[i];
                x = z % M.Xsize;
                y = z / M.Xsize;

                seq.Offsets[i] =
                    (VipsRegion.Addr(ir, x + le, y + to) -
                        VipsRegion.Addr(ir, le, to)) /
                    VipsImage.SizeOfElement(ir.Im);
            }
        }

        VIPS_GATE_START("vips_convf_gen: work");

        for (y = to; y < bo; y++)
        {
            switch (in_.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    CONV_FLOAT(unsigned char, float);
                    break;

                case VIPS_FORMAT_CHAR:
                    CONV_FLOAT(signed char, float);
                    break;

                case VIPS_FORMAT_USHORT:
                    CONV_FLOAT(unsigned short, float);
                    break;

                case VIPS_FORMAT_SHORT:
                    CONV_FLOAT(signed short, float);
                    break;

                case VIPS_FORMAT_UINT:
                    CONV_FLOAT(unsigned int, float);
                    break;

                case VIPS_FORMAT_INT:
                    CONV_FLOAT(signed int, float);
                    break;

                case VIPS_FORMAT_FLOAT:
                case VIPS_FORMAT_COMPLEX:
                    CONV_FLOAT(float, float);
                    break;

                case VIPS_FORMAT_DOUBLE:
                case VIPS_FORMAT_DPCOMPLEX:
                    CONV_FLOAT(double, double);
                    break;

                default:
                    g_assert_not_reached();
            }
        }

        VIPS_GATE_STOP("vips_convf_gen: work");

        VIPS_COUNT_PIXELS(out_region, "vips_convf_gen");

        return 0;
    }
}

public class VipsConvfStop
{
    public int Stop(object vseq, object a, object b)
    {
        var seq = (VipsConvfSequence)vseq;

        VIPS_UNREF(seq.Ir);

        return 0;
    }
}
```

Note that I've assumed the existence of certain classes and methods in the `Vips` namespace, such as `VipsImage`, `VipsRegion`, `VipsObject`, etc. These are likely part of a larger library or framework, and may need to be implemented separately.

Also note that this code is not compiled or tested, and may contain errors or omissions. It's intended only as a rough guide for converting the C code to C#.