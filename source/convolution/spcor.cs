```csharp
// spcor
// 
// Copyright: 1990, N. Dessipris; 2006, 2007 Nottingham Trent University.
// 
// Author: Nicos Dessipris
// Written on: 02/05/1990
// Modified on :
// 20/2/95 JC
//	- updated
//	- ANSIfied, a little
// 21/2/95 JC
//	- rewritten
//	- partialed
//	- speed-ups
//	- new correlation coefficient (see above), from Niblack "An
//	  Introduction to Digital Image Processing", Prentice/Hall, pp 138.
// 4/9/97 JC
//	- now does short/ushort as well
// 13/2/03 JC
//	- oops, could segv for short images
// 14/4/04 JC
//	- sets Xoffset / Yoffset
// 8/3/06 JC
//	- use im_embed() with edge stretching on the input, not the output
// 
// 2006-10-24 tcv
//      - add im_spcor2
// 
// 2007-11-12 tcv
//      - make im_spcor a wrapper selecting either im__spcor or im__spcor2
// 2008-09-09 JC
// 	- roll back the windowed version for now, it has some tile edge effects
// 3/2/10
// 	- gtkdoc
// 	- cleanups
// 7/11/13
// 	- redone as a class
// 8/4/15
// 	- avoid /0 for constant reference or zero image

using System;
using System.Collections.Generic;

public class VipsSpcor : VipsCorrelation
{
    private double[] rmean;
    private double[] c1;

    public override int PreGenerate()
    {
        var ref = RefReady;
        var bands = ref.Bands;
        var b = new VipsImage[bands];
        var t = new VipsImage[2];
        var b2 = new VipsImage[bands];

        if (!VipsCheckNonComplex(Nickname, ref))
            return -1;

        // Per-band mean.
        rmean = new double[bands];
        c1 = new double[bands];

        for (var i = 0; i < bands; i++)
        {
            if (!VipsExtractBand(ref, b[i], i, null) ||
                !VipsAvg(b[i], refmean[i], null))
                return -1;
        }

        // Per band sqrt(sumij (ref(i,j)-mean(ref))^2)
        var offset = new double[bands];
        var scale = new double[bands];

        for (var i = 0; i < bands; i++)
        {
            offset[i] = -rmean[i];
            scale[i] = 1.0;
        }

        if (!VipsLinear(ref, t[0], scale, offset, bands, null) ||
            !VipsMultiply(t[0], t[0], t[1], null))
            return -1;

        for (var i = 0; i < bands; i++)
        {
            if (!VipsExtractBand(t[1], b2[i], i, null) ||
                !VipsAvg(b2[i], c1[i], null))
                return -1;
        }

        for (var i = 0; i < bands; i++)
        {
            c1[i] *= ref.Xsize * ref.Ysize;
            c1[i] = Math.Sqrt(c1[i]);
        }

        return 0;
    }

    public override void Correlation(VipsRegion inRegion, VipsRegion outRegion)
    {
        var spcor = this as VipsSpcor;
        var r = outRegion.Valid;
        var ref = RefReady;
        var bands = VipsBandFormatIsComplex(ref.BandFmt) ? ref.Bands * 2 : ref.Bands;
        var sz = ref.Xsize * bands;
        var lsk = VipsRegionLskip(inRegion);

        var x = 0;
        var y = 0;
        var b = 0;
        var j = 0;
        var i = 0;

        double imean;
        double sum1;
        double sum2, sum3;
        double c2, cc;

        for (y = 0; y < r.Height; y++)
        {
            var q = (float[])VipsRegionAddr(outRegion, r.Left, r.Top + y);

            for (x = 0; x < r.Width; x++)
            {
                var p = VipsRegionAddr(inRegion, r.Left + x, r.Top + y);

                for (b = 0; b < bands; b++)
                {
                    switch (VipsImageGetFormat(ref))
                    {
                        case VIPS_FORMAT_UCHAR:
                            LoopUnsignedChar(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_CHAR:
                            LoopSignedChar(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_USHORT:
                            LoopUnsignedShort(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_SHORT:
                            LoopSignedShort(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_UINT:
                            LoopUnsignedInt(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_INT:
                            LoopSignedInt(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_FLOAT:
                        case VIPS_FORMAT_COMPLEX:
                            LoopFloat(ref, inRegion, spcor, p, q);
                            break;

                        case VIPS_FORMAT_DOUBLE:
                        case VIPS_FORMAT_DPCOMPLEX:
                            LoopDouble(ref, inRegion, spcor, p, q);
                            break;

                        default:
                            g_assert_not_reached();

                            // Stop compiler warnings.
                            sum2 = 0;
                            sum3 = 0;
                    }

                    c2 = spcor.c1[b] * Math.Sqrt(sum2);

                    if (c2 == 0.0)
                        /* Something like constant ref.
                         * We regard this as uncorrelated.
                         */
                        cc = 0.0;
                    else
                        cc = sum3 / c2;

                    q[x] = cc;
                }
            }
        }
    }

    private void LoopUnsignedChar(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        var r1 = (byte[])ref.Data + b;
        var p1 = (byte[])p + b;
        var lsk = VipsRegionLskip(inRegion);

        // Mean of area of in corresponding to ref.
        var sum1 = 0.0;

        for (var j = 0; j < ref.Ysize; j++)
        {
            for (var i = 0; i < sz; i += bands)
                sum1 += p1[i];

            p1 += lsk;
        }

        imean = sum1 / VipsImageNPels(ref);

        // Calculate sum-of-squares-of-differences for this window on
        // in, and also sum-of-products-of-differences from mean.
        var r1a = r1;
        var p1a = p1;

        sum2 = 0.0;
        sum3 = 0.0;

        for (var j = 0; j < ref.Ysize; j++)
        {
            for (var i = 0; i < sz; i += bands)
            {
                // Reference pel and input pel.
                var ip = p1a[i];
                var rp = r1a[i];

                // Accumulate sum-of-squares-of-differences for
                // input image.
                var t = ip - imean;
                sum2 += t * t;

                // Accumulate product-of-difference from mean.
                sum3 += (rp - spcor.rmean[b]) * t;
            }

            p1a += lsk;
            r1a += sz;
        }
    }

    private void LoopSignedChar(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }

    private void LoopUnsignedShort(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }

    private void LoopSignedShort(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }

    private void LoopUnsignedInt(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }

    private void LoopSignedInt(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }

    private void LoopFloat(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }

    private void LoopDouble(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q)
    {
        // ... (similar to LoopUnsignedChar)
    }
}

public class VipsCorrelationClass : VipsObjectClass
{
    public override string Nickname { get; }
    public override string Description { get; }

    public VipsBandFormat[] FormatTable { get; }
    public int PreGenerate(VipsCorrelation correlation);
    public void Correlation(VipsRegion inRegion, VipsRegion outRegion);

    public static readonly VipsCorrelationClass Type = new VipsCorrelationClass();
}

public class VipsSpcorClass : VipsCorrelationClass
{
    public override string Nickname { get; }
    public override string Description { get; }

    public VipsBandFormat[] FormatTable { get; }
    public int PreGenerate(VipsCorrelation correlation);
    public void Correlation(VipsRegion inRegion, VipsRegion outRegion);

    public static readonly VipsSpcorClass Type = new VipsSpcorClass();
}

public class VipsImage
{
    public int Bands { get; }
    public int Xsize { get; }
    public int Ysize { get; }

    public byte[] Data { get; }
    public VipsPel[] PelData { get; }

    public int BandFmt { get; }

    public static readonly VipsImage Type = new VipsImage();
}

public class VipsRegion
{
    public int Width { get; }
    public int Height { get; }

    public VipsRect Valid { get; }

    public static readonly VipsRegion Type = new VipsRegion();
}

public class VipsPel
{
    // ... (similar to VipsImage)
}

public class VipsObjectClass : VipsCorrelationClass
{
    public string Nickname { get; }
    public string Description { get; }

    public static readonly VipsObjectClass Type = new VipsObjectClass();
}

public abstract class VipsCorrelation : VipsObject
{
    protected VipsImage RefReady { get; }

    public virtual int PreGenerate();
    public virtual void Correlation(VipsRegion inRegion, VipsRegion outRegion);
}

public class VipsObject : VipsCorrelation
{
    // ... (similar to VipsCorrelation)
}

// Save a bit of typing.
public static readonly VipsBandFormat[] VipsSpcorFormatTable = new VipsBandFormat[]
{
    // Band format:  UC C  US S  UI I  F  X  D  DX
    // Promotion:      F     F     F     F     F     F     F     F     F     F
    VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT,
    VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT,
    VIPS_FORMAT_FLOAT, VIPS_FORMAT_FLOAT
};

public static int VipsCheckNonComplex(string nickname, VipsImage image)
{
    // ... (similar to the C code)
}

public static bool VipsBandFormatIsComplex(VipsBandFormat bandFmt)
{
    // ... (similar to the C code)
}

public static int VipsImageNPels(VipsImage image)
{
    // ... (similar to the C code)
}

public static byte[] VipsRegionAddr(VipsRegion region, int x, int y)
{
    // ... (similar to the C code)
}

public static float[] VipsImageGetFormat(VipsImage image)
{
    // ... (similar to the C code)
}

public static void LoopUnsignedChar(VipsImage ref, VipsRegion inRegion, VipsSpcor spcor, VipsPel[] p, float[] q);
// ... (similar to other loops)

public static int VipsAvg(VipsImage image, double[] avg, string error)
{
    // ... (similar to the C code)
}

public static bool VipsExtractBand(VipsImage image, VipsImage band, int bandIndex, string error)
{
    // ... (similar to the C code)
}

public static bool VipsLinear(VipsImage image, VipsImage output, double[] scale, double[] offset, int bands, string error)
{
    // ... (similar to the C code)
}

public static bool VipsMultiply(VipsImage image1, VipsImage image2, VipsImage output, string error)
{
    // ... (similar to the C code)
}

public class VipsConfig
{
    public static readonly string ConfigFile = "config.h";
}

public class Glib
{
    public static void AssertNotReached()
    {
        // ... (similar to the C code)
    }
}
```