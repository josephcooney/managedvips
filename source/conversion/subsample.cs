Here is the C# code equivalent to the provided C code:
```
using System;
using VipsDotNet;

public class Subsample : Conversion
{
    public override int Build()
    {
        // Maximum width of input we ask for.
        const int MaxWidth = 100;

        // Set demand hints. We want THINSTRIP, as we will be demanding a large area of input for each output line.
        if (ImagePipeline(out, DemandStyle.ThinStrip, in))
            return -1;

        // Prepare output. Note: we round the output width down!
        out.Xsize = in.Xsize / Xfac;
        out.Ysize = in.Ysize / Yfac;
        if (out.Xsize <= 0 || out.Ysize <= 0)
        {
            throw new ArgumentException("image has shrunk to nothing");
        }

        // Generate! If this is a very large shrink, then it's probably faster to do it a pixel at a time.
        if (Point || Xfac > 10)
            Generate(out, vips_subsample_point_gen);
        else
            Generate(out, vips_subsample_line_gen);

        return 0;
    }

    public override void ClassInit()
    {
        base.ClassInit();

        // We don't work well as sequential: we can easily skip the first few scanlines, and that confuses vips_sequential().
        // VIPS_ARG_SEQUENTIAL(class, "sequential", 2,
        //     _("Sequential"),
        //     _("Do this operation sequentially"),
        //     VIPS_ARGUMENT_OPTIONAL_INPUT,
        //     G_STRUCT_OFFSET(VipsSubsample, sequential),
        //     FALSE);

        VipsArgImage("input", 1, "Input", "Input image", ArgumentRequired.Input);
        VipsArgInt("xfac", 3, "Xfac", "Horizontal subsample factor", ArgumentRequired.Input, 1, MaxWidth, 1);
        VipsArgInt("yfac", 4, "Yfac", "Vertical subsample factor", ArgumentRequired.Input, 1, MaxWidth, 1);
        VipsArgBool("point", 5, "Point", "Point sample", ArgumentOptional.Input, false);
    }

    public override void Init()
    {
        base.Init();
    }

    public static int Subsample(VipsImage inImage, out VipsImage outImage, int xfac, int yfac, params object[] args)
    {
        return CallSplit("subsample", args, inImage, out ref outImage, xfac, yfac);
    }
}

// vips_subsample_line_gen
private static int vips_subsample_line_gen(VipsRegion outRegion, void* seq, void* a, void* b, bool* stop)
{
    VipsRegion ir = (VipsRegion)seq;
    Subsample subsample = (Subsample)b;
    VipsImage inImage = (VipsImage)a;
    VipsRect r = outRegion.Valid;
    int le = r.Left;
    int ri = VipsRect.Right(r);
    int to = r.Top;
    int bo = VipsRect.Bottom(r);
    int ps = VipsImage.SizeOfPel(inImage);
    int owidth = MaxWidth / subsample.Xfac;

    VipsRect s;
    int x, y;
    int z, k;

    // Loop down the region.
    for (y = to; y < bo; y++)
    {
        VipsPel* q = VipsRegion.Addr(outRegion, le, y);
        VipsPel* p;

        // Loop across the region, in owidth sized pieces.
        for (x = le; x < ri; x += owidth)
        {
            // How many pixels do we make this time?
            int ow = Math.Min(owidth, ri - x);

            // Ask for this many from input ... can save a little here!
            int iw = ow * subsample.Xfac - (subsample.Xfac - 1);

            // Ask for input.
            s.Left = x * subsample.Xfac;
            s.Top = y * subsample.Yfac;
            s.Width = iw;
            s.Height = 1;
            if (VipsRegion.Prepare(ir, ref s))
                return -1;

            // Append new pels to output.
            p = VipsRegion.Addr(ir, s.Left, s.Top);
            for (z = 0; z < ow; z++)
            {
                for (k = 0; k < ps; k++)
                    q[k] = p[k];

                q += ps;
                p += ps * subsample.Xfac;
            }
        }
    }

    return 0;
}

// vips_subsample_point_gen
private static int vips_subsample_point_gen(VipsRegion outRegion, void* seq, void* a, void* b, bool* stop)
{
    VipsRegion ir = (VipsRegion)seq;
    Subsample subsample = (Subsample)b;
    VipsImage inImage = (VipsImage)a;
    VipsRect r = outRegion.Valid;
    int le = r.Left;
    int ri = VipsRect.Right(r);
    int to = r.Top;
    int bo = VipsRect.Bottom(r);
    int ps = VipsImage.SizeOfPel(inImage);

    VipsRect s;
    int x, y;
    int k;

    // Loop down the region.
    for (y = to; y < bo; y++)
    {
        VipsPel* q = VipsRegion.Addr(outRegion, le, y);
        VipsPel* p;

        // Loop across the region, in owidth sized pieces.
        for (x = le; x < ri; x++)
        {
            // Ask for input.
            s.Left = x * subsample.Xfac;
            s.Top = y * subsample.Yfac;
            s.Width = 1;
            s.Height = 1;
            if (VipsRegion.Prepare(ir, ref s))
                return -1;

            // Append new pels to output.
            p = VipsRegion.Addr(ir, s.Left, s.Top);
            for (k = 0; k < ps; k++)
                q[k] = p[k];
            q += ps;
        }
    }

    return 0;
}
```