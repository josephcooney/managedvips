```csharp
// fastcor
//
// Copyright: 1990, N. Dessipris.
//
// Author: Nicos Dessipris
// Written on: 02/05/1990
// Modified on : 15/03/1991
// 20/2/95 JC
//	- ANSIfied
//	- in1 and in2 swapped, to match order for im_spcor
//	- memory leaks fixed
// 21/2/95 JC
// 	- partialed
//	- speed-ups
// 7/4/04
//	- now uses im_embed() with edge stretching on the output
//	- sets Xoffset / Yoffset
// 8/3/06 JC
//	- use im_embed() with edge stretching on the input, not the output
//	- calculate sum of squares of differences, rather than abs of
//	  difference
// 3/2/10
// 	- gtkdoc
// 	- cleanups
// 7/11/13
// 	- redone as a class

using System;
using System.Collections.Generic;

public class VipsFastcor : VipsCorrelation
{
    public VipsFastcor()
    {
    }

    // vips_fastcor_correlation
    //
    // Calculate the correlation surface.
    protected override void Correlation(VipsRegion inRegion, VipsRegion outRegion)
    {
        var r = outRegion.Valid;
        var refImage = RefReady;
        int bands = BandFormatIsComplex(refImage.BandFmt) ? refImage.Bands * 2 : refImage.Bands;
        int sz = refImage.Xsize * bands;
        int lsk = VipsRegion.LSkip(inRegion) / VipsImage.SizeOfElement(inRegion.Image);

        int x, y, i, j, b;

        switch (VipsImage.GetFormat(refImage))
        {
            case VIPS_FORMAT_CHAR:
                CorrInt(signed char);
                break;

            case VIPS_FORMAT_UCHAR:
                CorrInt(unsigned char);
                break;

            case VIPS_FORMAT_SHORT:
                CorrInt(signed short);
                break;

            case VIPS_FORMAT_USHORT:
                CorrInt(unsigned short);
                break;

            case VIPS_FORMAT_INT:
                CorrInt(signed int);
                break;

            case VIPS_FORMAT_UINT:
                CorrInt(unsigned int);
                break;

            case VIPS_FORMAT_FLOAT:
            case VIPS_FORMAT_COMPLEX:
                CorrFloat(float);
                break;

            case VIPS_FORMAT_DOUBLE:
            case VIPS_FORMAT_DPCOMPLEX:
                CorrFloat(double);
                break;

            default:
                throw new Exception("Invalid image format");
        }
    }

    // CORR_INT
    //
    // Calculate the correlation surface for integer images.
    private void CorrInt(Type type)
    {
        for (int y = 0; y < r.Height; y++)
        {
            var q = (unsigned int[])VipsRegion.Addr(outRegion, r.Left, r.Top + y);
            for (int x = 0; x < r.Width; x++)
                for (int b = 0; b < bands; b++)
                {
                    var p1 = (type[])refImage.Data;
                    var p2 = (type[])VipsRegion.Addr(inRegion, r.Left + x, r.Top + y);

                    unsigned int sum;

                    sum = 0;
                    for (int j = 0; j < refImage.Ysize; j++)
                        for (int i = b; i < sz; i += bands)
                            sum += (p1[i] - p2[i]) * (p1[i] - p2[i]);

                    q[x] = sum;
                }
        }
    }

    // CORR_FLOAT
    //
    // Calculate the correlation surface for float images.
    private void CorrFloat(Type type)
    {
        for (int y = 0; y < r.Height; y++)
        {
            var q = (type[])VipsRegion.Addr(outRegion, r.Left, r.Top + y);
            for (int x = 0; x < r.Width; x++)
                for (int b = 0; b < bands; b++)
                {
                    var p_ref = (type[])refImage.Data;
                    var p_in = (type[])VipsRegion.Addr(inRegion, r.Left + x, r.Top + y);

                    type sum;

                    sum = 0;
                    for (int j = 0; j < refImage.Ysize; j++)
                        for (int i = b; i < sz; i += bands)
                            sum += (p_ref[i] - p_in[i]) * (p_ref[i] - p_in[i]);

                    q[x] = sum;
                }
        }
    }

    // vips_fastcor_class_init
    //
    // Initialize the VipsFastcor class.
    public static void ClassInit(VipsObjectClass* objectClass)
    {
        var cclass = (VipsCorrelationClass)objectClass;

        objectClass.Nickname = "fastcor";
        objectClass.Description = "fast correlation";

        cclass.FormatTable = vips_fastcor_format_table;
        cclass.Correlation = Correlation;
    }

    // vips_fastcor_init
    //
    // Initialize the VipsFastcor instance.
    public void Init()
    {
    }

    // vips_fastcor
    //
    // Calculate a fast correlation surface.
    public static int Fastcor(VipsImage inImage, VipsImage refImage, out VipsImage outImage)
    {
        var result = VipsCallSplit("fastcor", inImage, refImage, out outImage);
        return result;
    }
}

// vips_fastcor_format_table
//
// Type promotion for multiplication.
private static readonly VipsBandFormat[] vips_fastcor_format_table =
{
    // Band format:  UC  C   US  S   UI  I   F  X  D  DX
    // Promotion: */ UI, UI, UI, UI, UI, UI, F, X, D, DX
};
```