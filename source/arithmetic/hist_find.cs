Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsHistFind : VipsStatistic
{
    public int Band { get; set; }
    public Histogram Hist { get; private set; }
    public VipsImage Out { get; private set; }
    public bool Large { get; private set; }

    public override void Build()
    {
        // Make the output image.
        if (VIPS_OBJECT_CLASS(VipsHistFind.ParentClass).Build(this))
            return;

        // Interleave for output.
        VipsPel[] obuffer = new VipsPel[VIPS_IMAGE_SIZEOF_LINE(Out)];
        double[] bins = Hist.Bins;
        int i, j;

        for (i = 0; i < Out.Xsize; i++)
        {
            for (j = 0; j < Out.Bands; j++)
                obuffer[i] = bins[j][i];

            if (VipsImage.WriteLine(Out, 0, obuffer))
                return;
        }
    }

    public override void Start()
    {
        // Make the main hist, if necessary.
        Hist = Histogram.New(this,
            Band == -1 ? Ready.Bands : 1,
            Band,
            Ready.BandFmt == VIPS_FORMAT_UCHAR ? 256 : 65536);
    }

    public override int Scan(VipsImage inImage, int x, int y, void seq)
    {
        // Accumulate a histogram in one of these.
        Histogram subHist = (Histogram)seq;
        double[] bins = Hist.Bins;
        double mx = Hist.Mx;

        if (Band < 0)
        {
            switch (Ready.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    for (int i = 0; i < inImage.Xsize; i++)
                        for (int j = 0; j < Ready.Bands; j++)
                            bins[j][i] += ((double[])subHist.Bins[j])[i];
                    mx = 255;
                    break;

                case VIPS_FORMAT_USHORT:
                    for (int i = 0; i < inImage.Xsize; i++)
                        for (int j = 0; j < Ready.Bands; j++)
                            bins[j][i] += ((double[])subHist.Bins[j])[i];
                    break;
            }
        }
        else
        {
            switch (Ready.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    for (int i = 0; i < inImage.Xsize; i++)
                        bins[0][i] += ((double[])subHist.Bins[0])[i];
                    break;

                case VIPS_FORMAT_USHORT:
                    for (int i = 0; i < inImage.Xsize; i++)
                        bins[0][i] += ((double[])subHist.Bins[0])[i];
                    break;
            }
        }

        Hist.Mx = mx;
        return 0;
    }

    public override int Stop(VipsImage inImage, void seq)
    {
        Histogram subHist = (Histogram)seq;
        double[] bins = Hist.Bins;

        for (int i = 0; i < subHist.NBands; i++)
            for (int j = 0; j < subHist.Size; j++)
                bins[i][j] += ((double[])subHist.Bins[i])[j];

        // Blank out sub-hist to make sure we can't add it again.
        subHist.Mx = 0;
        for (int i = 0; i < subHist.NBands; i++)
            subHist.Bins[i] = null;

        return 0;
    }

    public static VipsImage HistFind(VipsImage inImage, VipsImage outImage)
    {
        // Make the output image.
        if (VIPS_OBJECT_CLASS(VipsHistFind.ParentClass).Build(outImage))
            return null;

        // Interleave for output.
        double[] obuffer = new double[VIPS_IMAGE_SIZEOF_LINE(outImage)];
        double[] bins = Hist.Bins;
        int i, j;

        for (i = 0; i < outImage.Xsize; i++)
        {
            for (j = 0; j < outImage.Bands; j++)
                obuffer[i] = bins[j][i];

            if (VipsImage.WriteLine(outImage, 0, obuffer))
                return null;
        }

        return outImage;
    }
}

public class Histogram
{
    public int NBands { get; private set; }
    public int Band { get; private set; }
    public int Size { get; private set; }
    public double Mx { get; private set; }
    public double[] Bins { get; private set; }

    public static Histogram New(VipsHistFind histFind, int nBands, int band, int size)
    {
        // We won't use all of this for uint accumulators.
        int nBytes = size * sizeof(double);

        Histogram hist;
        int i;

        if (!(hist = new Histogram()))
            return null;

        hist.NBands = nBands;
        hist.Band = band;
        hist.Size = size;
        hist.Mx = 0.0;

        for (i = 0; i < nBands; i++)
        {
            if ((hist.Bins[i] = new double[nBytes]) == null)
                return null;
            Array.Clear(hist.Bins[i], 0, nBytes);
        }

        return hist;
    }
}
```