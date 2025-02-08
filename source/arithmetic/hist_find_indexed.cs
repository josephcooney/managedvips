Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsHistFindIndexed : VipsStatistic
{
    public VipsImage Index { get; set; }
    public VipsCombine Combine { get; set; }

    private Histogram _histogram;

    public override void Start()
    {
        if (_histogram == null)
            _histogram = new Histogram(this);
    }

    public override int Scan(int x, int y, object inData, int n)
    {
        var hist = _histogram;
        var indexed = this as VipsHistFindIndexed;

        var scanFn = GetScanFunction(indexed.Index.Format);

        if (scanFn != null)
            scanFn(indexed, hist, inData, VIPS_REGION_ADDR(hist.Region, x, y), n);

        return 0;
    }

    public override int Stop(object seq)
    {
        var subHist = seq as Histogram;
        var indexed = this as VipsHistFindIndexed;

        if (subHist == null || indexed == null)
            return -1;

        var hist = _histogram;
        var bands = Indexed.Image.Bands;

        for (int i = 0; i <= subHist.MaxValue; i++)
        {
            if (subHist.Init[i])
            {
                if (hist.Init[i])
                    CombineBins(indexed.Combine, hist.Bins, subHist.Bins);
                else
                {
                    Array.Copy(subHist.Bins, hist.Bins, bands * sizeof(double));
                    hist.Init[i] = true;
                }
            }

            hist.Bins += bands;
            subHist.Bins += bands;
        }

        return 0;
    }

    private void CombineBins(VipsCombine combine, double[] a, double[] b)
    {
        switch (combine)
        {
            case VipsCombine.Max:
                for (int i = 0; i < a.Length; i++)
                    a[i] = Math.Max(a[i], b[i]);
                break;
            case VipsCombine.Sum:
                for (int i = 0; i < a.Length; i++)
                    a[i] += b[i];
                break;
            case VipsCombine.Min:
                for (int i = 0; i < a.Length; i++)
                    a[i] = Math.Min(a[i], b[i]);
                break;
        }
    }

    private void AccumulateUchar(double[] bins, int[] init, object inData, int n)
    {
        var tv = (double[])inData;
        for (int x = 0; x < n; x++)
        {
            int ix = ((unsigned char[])Index.Data)[x];
            double* bin = bins + ix * Indexed.Image.Bands;

            if (init[ix])
                CombineBins(Combine, bin, tv);
            else
            {
                Array.Copy(tv, bin, Indexed.Image.Bands * sizeof(double));
                init[ix] = true;
            }

            tv += Indexed.Image.Bands;
        }
    }

    private void AccumulateUshort(double[] bins, int[] init, object inData, int n)
    {
        var tv = (double[])inData;
        for (int x = 0; x < n; x++)
        {
            int ix = ((unsigned short[])Index.Data)[x];
            double* bin = bins + ix * Indexed.Image.Bands;

            if (ix > MaxValue)
                MaxValue = ix;

            if (init[ix])
                CombineBins(Combine, bin, tv);
            else
            {
                Array.Copy(tv, bin, Indexed.Image.Bands * sizeof(double));
                init[ix] = true;
            }

            tv += Indexed.Image.Bands;
        }
    }

    private VipsHistFindIndexedScanFn GetScanFunction(VipsBandFormat format)
    {
        switch (format)
        {
            case VipsBandFormat.Uchar:
                return AccumulateUchar;
            case VipsBandFormat.Ushort:
                return AccumulateUshort;
            default:
                throw new ArgumentException("Unsupported band format");
        }
    }

    private class Histogram
    {
        public VipsHistFindIndexed Indexed { get; set; }
        public int Size { get; set; }
        public double[] Bins { get; set; }
        public int[] Init { get; set; }
        public int MaxValue { get; set; }

        public VipsRegion Region { get; set; }

        public Histogram(VipsHistFindIndexed indexed)
        {
            Indexed = indexed;
            Size = indexed.Index.Format == VipsBandFormat.Uchar ? 256 : 65536;
            Bins = new double[Size * indexed.Image.Bands];
            Init = new int[Size];
            Region = new VipsRegion(indexed.Index);
        }
    }

    public class VipsHistFindIndexedScanFn
    {
        private readonly Action<VipsHistFindIndexed, Histogram, object, int, int> _scan;

        public VipsHistFindIndexedScanFn(Action<VipsHistFindIndexed, Histogram, object, int, int> scan)
        {
            _scan = scan;
        }

        public void Invoke(VipsHistFindIndexed indexed, Histogram hist, object inData, int x, int n)
        {
            _scan(indexed, hist, inData, x, n);
        }
    }
}
```

Note that this code assumes the existence of a `VipsStatistic` class and its methods, as well as the `VipsImage`, `VipsRegion`, and `VipsCombine` classes. It also uses the `System.Collections.Generic` namespace for the `Histogram` class.

The `vips_hist_find_indexed_build` method is not directly converted to C# because it appears to be a part of the VIPS library's internal implementation, rather than a public API. However, the equivalent functionality can be achieved through the use of the `Start`, `Scan`, and `Stop` methods.

The `vips_hist_find_indexed_uchar_scan` and `vips_hist_find_indexed_ushort_scan` functions are replaced with the `AccumulateUchar` and `AccumulateUshort` methods, respectively. The `GetScanFunction` method is used to determine which accumulation function to use based on the index image's format.

The `Histogram` class represents a histogram object, which accumulates pixel values from an input image using an index image. It has properties for the indexed object, size, bins, initialization array, and maximum value.

The `VipsHistFindIndexedScanFn` delegate is used to represent a scan function that can be invoked with a `VipsHistFindIndexed`, `Histogram`, and other parameters.