Here is the C# code equivalent to the provided C code:

```csharp
// VIPS function dispatch tables for morphology.
//
// J. Cupitt, 19/9/95

using System;

namespace Vips
{
    // Args to im_profile.
    public static class ProfileArgs
    {
        public const int InputImage = 0;
        public const int OutputImage = 1;
        public const int Direction = 2;
    }

    // Call im_profile via arg vector.
    public static class ProfileVec
    {
        public static int Dispatch(int[] argv)
        {
            int dir = argv[ProfileArgs.Direction];
            return Vips.Image.Profile(argv[ProfileArgs.InputImage], argv[ProfileArgs.OutputImage], dir);
        }
    }

    // Description of im_profile.
    public static class ProfileDesc
    {
        public const string Name = "im_profile";
        public const string Descr = "find first horizontal/vertical edge";
        public const int Flags = Vips.ImageFlags.Transform;
        public static readonly Func<int[], int> Dispatch = ProfileVec.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[ProfileArgs.ArgCount] { ProfileArgs.InputImage, ProfileArgs.OutputImage, ProfileArgs.Direction };
    }

    // Args to im_erode.
    public static class ErodeArgs
    {
        public const int InputImage = 0;
        public const int OutputImage = 1;
        public const int Mask = 2;
    }

    // Call im_dilate via arg vector.
    public static class DilateVec
    {
        public static int Dispatch(int[] argv)
        {
            Vips.MaskObject mo = (Vips.MaskObject)argv[ErodeArgs.Mask];
            return Vips.Image.Dilate(argv[ErodeArgs.InputImage], argv[ErodeArgs.OutputImage], mo.Mask);
        }
    }

    // Description of im_dilate.
    public static class DilateDesc
    {
        public const string Name = "im_dilate";
        public const string Descr = "dilate image with mask, adding a black border";
        public const int Flags = Vips.ImageFlags.PIO | Vips.ImageFlags.Transform;
        public static readonly Func<int[], int> Dispatch = DilateVec.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[ErodeArgs.ArgCount] { ErodeArgs.InputImage, ErodeArgs.OutputImage, ErodeArgs.Mask };
    }

    // Call im_erode via arg vector.
    public static class ErodeVec
    {
        public static int Dispatch(int[] argv)
        {
            Vips.MaskObject mo = (Vips.MaskObject)argv[ErodeArgs.Mask];
            return Vips.Image.Erode(argv[ErodeArgs.InputImage], argv[ErodeArgs.OutputImage], mo.Mask);
        }
    }

    // Description of im_erode.
    public static class ErodeDesc
    {
        public const string Name = "im_erode";
        public const string Descr = "erode image with mask, adding a black border";
        public const int Flags = Vips.ImageFlags.PIO | Vips.ImageFlags.Transform;
        public static readonly Func<int[], int> Dispatch = ErodeVec.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[ErodeArgs.ArgCount] { ErodeArgs.InputImage, ErodeArgs.OutputImage, ErodeArgs.Mask };
    }

    // Args to im_cntlines.
    public static class CntLinesArgs
    {
        public const int InputImage = 0;
        public const int OutputDouble = 1;
        public const int Direction = 2;
    }

    // Call im_cntlines via arg vector.
    public static class CntLinesVec
    {
        public static int Dispatch(int[] argv)
        {
            double[] out = (double[])argv[CntLinesArgs.OutputDouble];
            int dir = argv[CntLinesArgs.Direction];
            return Vips.Image.CntLines(argv[CntLinesArgs.InputImage], out, dir);
        }
    }

    // Description of im_cntlines.
    public static class CntLinesDesc
    {
        public const string Name = "im_cntlines";
        public const string Descr = "count horizontal or vertical lines";
        public const int Flags = 0;
        public static readonly Func<int[], int> Dispatch = CntLinesVec.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[CntLinesArgs.ArgCount] { CntLinesArgs.InputImage, CntLinesArgs.OutputDouble, CntLinesArgs.Direction };
    }

    // Args to im_rank.
    public static class RankArgs
    {
        public const int InputImage = 0;
        public const int OutputImage = 1;
        public const int XSize = 2;
        public const int YSize = 3;
        public const int N = 4;
    }

    // Call im_rank via arg vector.
    public static class RankVec
    {
        public static int Dispatch(int[] argv)
        {
            int xsize = argv[RankArgs.XSize];
            int ysize = argv[RankArgs.YSize];
            int n = argv[RankArgs.N];
            return Vips.Image.Rank(argv[RankArgs.InputImage], argv[RankArgs.OutputImage], xsize, ysize, n);
        }
    }

    // Description of im_rank.
    public static class RankDesc
    {
        public const string Name = "im_rank";
        public const string Descr = "rank filter nth element of xsize/ysize window";
        public const int Flags = Vips.ImageFlags.PIO;
        public static readonly Func<int[], int> Dispatch = RankVec.Dispatch;
        public const int ArgCount = 5;
        public static readonly int[] Args = new int[RankArgs.ArgCount] { RankArgs.InputImage, RankArgs.OutputImage, RankArgs.XSize, RankArgs.YSize, RankArgs.N };
    }

    // Args for im_zerox.
    public static class ZeroXArgs
    {
        public const int InputImage = 0;
        public const int OutputImage = 1;
        public const int Flag = 2;
    }

    // Call im_zerox via arg vector.
    public static class ZeroXVec
    {
        public static int Dispatch(int[] argv)
        {
            int flag = argv[ZeroXArgs.Flag];
            return Vips.Image.ZeroX(argv[ZeroXArgs.InputImage], argv[ZeroXArgs.OutputImage], flag);
        }
    }

    // Description of im_zerox.
    public static class ZeroXDesc
    {
        public const string Name = "im_zerox";
        public const string Descr = "find +ve or -ve zero crossings in image";
        public const int Flags = Vips.ImageFlags.PIO | Vips.ImageFlags.Transform;
        public static readonly Func<int[], int> Dispatch = ZeroXVec.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[ZeroXArgs.ArgCount] { ZeroXArgs.InputImage, ZeroXArgs.OutputImage, ZeroXArgs.Flag };
    }

    // Args to im_maxvalue.
    public static class MaxValueArgs
    {
        public const int InputImageVec = 0;
        public const int OutputImage = 1;
    }

    // Call im_maxvalue via arg vector.
    public static class MaxValueVec
    {
        public static int Dispatch(int[] argv)
        {
            Vips.ImageVecObject iv = (Vips.ImageVecObject)argv[MaxValueArgs.InputImageVec];
            return Vips.Image.MaxValue(iv.Vec, argv[MaxValueArgs.OutputImage], iv.N);
        }
    }

    // Description of im_maxvalue.
    public static class MaxValueDesc
    {
        public const string Name = "im_maxvalue";
        public const string Descr = "point-wise maximum value";
        public const int Flags = Vips.ImageFlags.PIO;
        public static readonly Func<int[], int> Dispatch = MaxValueVec.Dispatch;
        public const int ArgCount = 2;
        public static readonly int[] Args = new int[MaxValueArgs.ArgCount] { MaxValueArgs.InputImageVec, MaxValueArgs.OutputImage };
    }

    // Args to im_rank_image.
    public static class RankImageArgs
    {
        public const int InputImageVec = 0;
        public const int OutputImage = 1;
        public const int Index = 2;
    }

    // Call im_rank_image via arg vector.
    public static class RankImageView
    {
        public static int Dispatch(int[] argv)
        {
            Vips.ImageVecObject iv = (Vips.ImageVecObject)argv[RankImageArgs.InputImageVec];
            int index = argv[RankImageArgs.Index];
            return Vips.Image.RankImage(iv.Vec, argv[RankImageArgs.OutputImage], iv.N, index);
        }
    }

    // Description of im_rank_image.
    public static class RankImageDesc
    {
        public const string Name = "im_rank_image";
        public const string Descr = "point-wise pixel rank";
        public const int Flags = Vips.ImageFlags.PIO;
        public static readonly Func<int[], int> Dispatch = RankImageView.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[RankImageArgs.ArgCount] { RankImageArgs.InputImageVec, RankImageArgs.OutputImage, RankImageArgs.Index };
    }

    // Args for im_label_regions().
    public static class LabelRegionsArgs
    {
        public const int InputImage = 0;
        public const int OutputImage = 1;
        public const int Segments = 2;
    }

    // Call im_label_regions() via arg vector.
    public static class LabelRegionsVec
    {
        public static int Dispatch(int[] argv)
        {
            Vips.Image test = argv[LabelRegionsArgs.InputImage];
            Vips.Image mask = argv[LabelRegionsArgs.OutputImage];
            int[] serial = (int[])argv[LabelRegionsArgs.Segments];
            return Vips.Image.LabelRegions(test, mask, serial);
        }
    }

    // Description of im_label_regions().
    public static class LabelRegionsDesc
    {
        public const string Name = "im_label_regions";
        public const string Descr = "number continuous regions in an image";
        public const int Flags = 0;
        public static readonly Func<int[], int> Dispatch = LabelRegionsVec.Dispatch;
        public const int ArgCount = 3;
        public static readonly int[] Args = new int[LabelRegionsArgs.ArgCount] { LabelRegionsArgs.InputImage, LabelRegionsArgs.OutputImage, LabelRegionsArgs.Segments };
    }

    // Package up all these functions.
    public static class MorphList
    {
        public const int Count = 9;
        public static readonly Func<int[], int>[] Dispatches =
        {
            ProfileVec.Dispatch,
            DilateVec.Dispatch,
            RankVec.Dispatch,
            RankImageView.Dispatch,
            MaxValueVec.Dispatch,
            LabelRegionsVec.Dispatch,
            ZeroXVec.Dispatch,
            ErodeVec.Dispatch,
            CntLinesVec.Dispatch
        };
    }

    // Package of functions.
    public static class MorphologyPackage
    {
        public const string Name = "morphology";
        public const int Count = MorphList.Count;
        public static readonly Func<int[], int>[] Dispatches = MorphList.Dispatches;
    }
}
```