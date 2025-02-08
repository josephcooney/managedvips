Here is the converted C# code:

```csharp
// n-dimensional histogram
// Written on: 8/7/03
// 10/11/04
//	- oops, was not checking the bandfmt coming in
// 24/3/10
// 	- gtkdoc
// 	- small celanups
// 17/8/13
// 	- redo as a class
// 28/1/22 travisbell
// 	- better arg checking

using System;
using System.Collections.Generic;

namespace Vips
{
    public class Histogram
    {
        public VipsHistFindNDim NDim { get; set; }
        public int[][][] Data { get; set; }
    }

    public class VipsHistFindNDim : VipsStatistic
    {
        // Number of bins on each axis.
        public int Bins { get; set; }

        // Max pixel value for this format.
        public int MaxVal { get; set; }

        // Main image histogram. Subhists accumulate to this.
        public Histogram Hist { get; set; }

        // Write hist to this output image.
        public VipsImage Out { get; set; }
    }

    public class VipsHistFindNDimClass : VipsStatisticClass
    {
        public override void Initialize()
        {
            base.Initialize();

            // Build a Histogram.
            Hist = new Histogram();
            Hist.NDim = this;
            Hist.Data = new int[][][]
            {
                new int[][] { new int[Bins], new int[Bins] },
                new int[][] { new int[Bins], new int[Bins] },
                new int[][] { new int[Bins], new int[Bins] }
            };
        }

        // Build a Histogram.
        public Histogram HistogramNew()
        {
            VipsImage inImage = Ready;
            int bins = Bins;

            // How many dimensions do we need to allocate?
            int ilimit = inImage.Bands > 2 ? bins : 1;
            int jlimit = inImage.Bands > 1 ? bins : 1;

            int i, j;
            Histogram hist;

            if (!(hist = new Histogram()))
                return null;

            hist.NDim = this;

            for (i = 0; i < ilimit; i++)
            {
                hist.Data[i] = new int[jlimit][];
                for (j = 0; j < jlimit; j++)
                    hist.Data[i][j] = new int[Bins];
            }

            return hist;
        }

        // Accumulate a histogram in one of these.
        public override void Build()
        {
            base.Build();

            if (Ready.Bands > 3)
            {
                VipsError("hist_find_ndim", "%s", _("image is not 1 - 3 bands"));
                return;
            }

            MaxVal = Ready.BandFmt == VIPS_FORMAT_UCHAR ? 256 : 65536;
            if (Bins < 1 || Bins > MaxVal)
            {
                VipsError("hist_find_ndim", _("bins out of range [1,%d]"), MaxVal);
                return;
            }
        }

        // Join a sub-hist onto the main hist.
        public override void Stop(VipsStatistic statistic, object seq)
        {
            Histogram subHist = (Histogram)seq;
            VipsHistFindNDim ndim = (VipsHistFindNDim)statistic;
            Histogram hist = NDim.Hist;

            int i, j, k;

            for (i = 0; i < Bins; i++)
                for (j = 0; j < Bins; j++)
                    for (k = 0; k < Bins; k++)
                        if (hist.Data[i] && hist.Data[i][j])
                        {
                            hist.Data[i][j][k] += subHist.Data[i][j][k];

                            // Zap sub-hist to make sure we
                            // can't add it again.
                            subHist.Data[i][j][k] = 0;
                        }
        }

        // Scan a sub-image of the input image and accumulate its histogram.
        public override void Scan(VipsStatistic statistic, object seq, int x, int y, VipsImage inImage, int n)
        {
            Histogram hist = (Histogram)seq;
            VipsHistFindNDim ndim = (VipsHistFindNDim)statistic;
            VipsImage im = Ready;
            int nb = im.Bands;
            double scale = (double)(MaxVal + 1) / Bins;
            int i, j, k;
            int[] index = new int[3];

            // Fill these with dimensions, backwards.
            index[0] = index[1] = index[2] = 0;

            switch (im.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    LOOP(unsigned char);
                    break;

                case VIPS_FORMAT_USHORT:
                    LOOP(unsigned short);
                    break;

                default:
                    g_assert_not_reached();
                    break;
            }
        }

        // Save a bit of typing.
        #define UC VIPS_FORMAT_UCHAR
        #define US VIPS_FORMAT_USHORT
        #define UI VIPS_FORMAT_UINT

        // Type mapping: go to uchar or ushort.
        private static readonly VipsBandFormat[] vips_hist_find_ndim_format_table = new VipsBandFormat[]
        {
            /* Band format:  UC  C   US  S   UI  I   F   X   D   DX */
            /* Promotion: */ UC, UC, US, US, US, US, US, US, US, US
        };

        // Initialize the class.
        public static void ClassInit(VipsHistFindNDimClass class_)
        {
            base.ClassInit(class_);

            class_.Nickname = "hist_find_ndim";
            class_.Description = _("find n-dimensional image histogram");
            class_.Build = Build;
            class_.Start = Start;
            class_.Scan = Scan;
            class_.Stop = Stop;
            class_.FormatTable = vips_hist_find_ndim_format_table;

            VIPS_ARG_IMAGE(class_, "out", 100,
                _("Output"),
                _("Output histogram"),
                VIPS_ARGUMENT_REQUIRED_OUTPUT);

            VIPS_ARG_INT(class_, "bins", 110,
                _("Bins"),
                _("Number of bins in each dimension"),
                VIPS_ARGUMENT_OPTIONAL_INPUT,
                Bins,
                1, 65536, 10);
        }

        // Initialize the object.
        public override void Init()
        {
            base.Init();

            Bins = 10;
        }
    }

    // vips_hist_find_ndim: (method)
    // @in: input image
    // @out: (out): output image
    // @...: %NULL-terminated list of optional named arguments
    //
    // Optional arguments:
    //
    // * @bins: number of bins to make on each axis
    //
    // Make a one, two or three dimensional histogram of a 1, 2 or
    // 3 band image. Divide each axis into @bins bins .. ie.
    // output is 1 x bins, bins x bins, or bins x bins x bins bands.
    // @bins defaults to 10.
    //
    // char and uchar images are cast to uchar before histogramming, all other
    // image types are cast to ushort.
    //
    // See also: vips_hist_find(), vips_hist_find_indexed().
    //
    // Returns: 0 on success, -1 on error
    public static int VipsHistFindNDim(VipsImage inImage, ref VipsImage outImage, params object[] args)
    {
        return VipsCallSplit("hist_find_ndim", inImage, ref outImage, args);
    }
}
```