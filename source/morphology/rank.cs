Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipsRank : VipsMorphology
{
    public VipsImage Out { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Index { get; private set; }

    public bool HistPath { get; private set; }

    protected override void Build(VipsObject obj)
    {
        VipsMorphology morphology = (VipsMorphology)obj;
        VipsImage inImg = morphology.In;

        if (!vips_image_decode(inImg, out Out))
            return;

        if (!vips_check_noncomplex("rank", inImg))
            return;

        if (Width > inImg.Xsize || Height > inImg.Ysize)
        {
            vips_error("rank", "%s", "window too large");
            return;
        }

        n = Width * Height;
        if (Index < 0 || Index > n - 1)
        {
            vips_error("rank", "%s", "index out of range");
            return;
        }

        // Enable the hist path if it'll probably help.
        if (inImg.BandFmt == VipsBandFormat.UChar)
        {
            // The hist path is always faster for windows larger than about
            // 10x10, and faster for >3x3 on the non-max/min case.
            if (n > 90)
                HistPath = true;
            else if (n > 10 && Index != 0 && Index != n - 1)
                HistPath = true;
        }

        // Expand the input.
        VipsImage expandedImg;
        if (!vips_embed(inImg, out expandedImg,
            Width / 2, Height / 2,
            inImg.Xsize + Width - 1, inImg.Ysize + Height - 1,
            "extend", VipsExtendMode.Copy,
            null))
            return;

        Out = vips_image_new();
        if (!vips_image_pipeline(Out, VipsDemandHint.FatStrip, expandedImg, null))
            return;
        Out.Xsize -= Width - 1;
        Out.Ysize -= Height - 1;

        if (!vips_image_generate(Out,
            new VipsRankStart(this),
            new VipsRankGenerate(),
            new VipsRankStop()))
            return;

        Out.Xoffset = 0;
        Out.Yoffset = 0;

        vips_reorder_margin_hint(Out, Width * Height);
    }

    public class VipsRankSequence
    {
        public VipsRegion Ir { get; private set; }
        public VipsPel[] Sort { get; private set; }
        public unsigned int[][] Hist { get; private set; }

        public void Dispose()
        {
            if (Ir != null)
                vips_region_free(Ir);
            if (Sort != null)
                Array.Clear(Sort, 0, Sort.Length);
            if (Hist != null)
            {
                foreach (var hist in Hist)
                    Array.Clear(hist, 0, hist.Length);
                Array.Clear(Hist, 0, Hist.Length);
            }
        }

        public VipsRankSequence(VipsImage img)
        {
            Ir = vips_region_new(img);
            Sort = new VipsPel[Width * Height];
            if (HistPath)
            {
                Hist = new unsigned int[img.Bands][];
                for (int i = 0; i < img.Bands; i++)
                    Hist[i] = new unsigned int[256];
            }
        }
    }

    public class VipsRankStart : VipsImageGenerateStart
    {
        private readonly VipsRank rank;

        public VipsRankStart(VipsRank rank)
        {
            this.rank = rank;
        }

        public override void Dispose()
        {
            if (rank != null)
                rank.Dispose();
        }
    }

    public class VipsRankGenerate : VipsImageGenerate
    {
        private readonly VipsRankSequence seq;

        public VipsRankGenerate(VipsRankSequence seq)
        {
            this.seq = seq;
        }

        public override int Generate(VipsRegion outRegion, object vseq, object a, object b, bool stop)
        {
            VipsRect r = outRegion.Valid;
            VipsImage inImg = (VipsImage)a;
            VipsRank rank = (VipsRank)b;

            if (rank.HistPath)
                GenerateUChar(outRegion, seq, rank);
            else if (rank.Index == 0)
                GenerateMin(outRegion, seq, rank);
            else if (rank.Index == rank.n - 1)
                GenerateMax(outRegion, seq, rank);
            else
                GenerateSelect(outRegion, seq, rank);

            return 0;
        }

        private void GenerateUChar(VipsRegion outRegion, VipsRankSequence seq, VipsRank rank)
        {
            VipsImage inImg = seq.Ir.Im;
            VipsRect r = outRegion.Valid;

            // Get input and output pointers for this line.
            VipsPel[] p = (VipsPel[])VipsRegionAddr(seq.Ir, r.Left, r.Top + r.Height);
            VipsPel[] q = (VipsPel[])VipsRegionAddr(outRegion, r.Left, r.Top + r.Height);

            // Find histogram for the first output pixel.
            for (int b = 0; b < inImg.Bands; b++)
                Array.Clear(seq.Hist[b], 0, seq.Hist[b].Length);
            VipsPel[] p1 = p;
            for (int j = 0; j < rank.Height; j++)
            {
                for (int i = 0, x = 0; x < rank.Width; x++, i++)
                    seq.Hist[i / rank.Width][p1[i]] += 1;

                p1 += VipsRegionLskip(seq.Ir);
            }

            // Loop for output pels.
            for (int x = 0; x < r.Width; x++)
            {
                for (int b = 0; b < inImg.Bands; b++)
                {
                    // Calculate cumulative histogram -- the value is the
                    // index at which we pass the rank.
                    unsigned int[] hist = seq.Hist[b];

                    int sum;
                    int i;

                    sum = 0;
                    for (i = 0; i < 256; i++)
                    {
                        sum += hist[i];
                        if (sum > rank.Index)
                            break;
                    }
                    q[b] = i;

                    // Adapt histogram --- remove the pels from
                    // the left hand column, add in pels for a
                    // new right-hand column.
                    VipsPel[] p2 = p + b;
                    for (int j = 0; j < rank.Height; j++)
                    {
                        hist[p2[0]] -= 1;
                        hist[p2[(rank.Width - 1) * inImg.Bands]] += 1;

                        p2 += VipsRegionLskip(seq.Ir);
                    }
                }

                p += inImg.Bands;
                q += inImg.Bands;
            }
        }

        private void GenerateMin(VipsRegion outRegion, VipsRankSequence seq, VipsRank rank)
        {
            // Loop for find min of window.
            VipsPel[] q = (VipsPel[])VipsRegionAddr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top + outRegion.Valid.Height);
            VipsPel[] p = (VipsPel[])VipsRegionAddr(seq.Ir, seq.Ir.Valid.Left, seq.Ir.Valid.Top + seq.Ir.Valid.Height);

            for (int x = 0; x < outRegion.Valid.Width; x++)
            {
                VipsPel min = p[x];

                for (int j = 0; j < rank.Height; j++)
                {
                    VipsPel[] e = p;

                    for (int i = 0; i < rank.Width; i++)
                    {
                        if (e[i] < min)
                            min = e[i];
                    }

                    p += VipsRegionLskip(seq.Ir);
                }

                q[x] = min;
            }
        }

        private void GenerateMax(VipsRegion outRegion, VipsRankSequence seq, VipsRank rank)
        {
            // Loop for find max of window.
            VipsPel[] q = (VipsPel[])VipsRegionAddr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top + outRegion.Valid.Height);
            VipsPel[] p = (VipsPel[])VipsRegionAddr(seq.Ir, seq.Ir.Valid.Left, seq.Ir.Valid.Top + seq.Ir.Valid.Height);

            for (int x = 0; x < outRegion.Valid.Width; x++)
            {
                VipsPel max = p[x];

                for (int j = 0; j < rank.Height; j++)
                {
                    VipsPel[] e = p;

                    for (int i = 0; i < rank.Width; i++)
                    {
                        if (e[i] > max)
                            max = e[i];
                    }

                    p += VipsRegionLskip(seq.Ir);
                }

                q[x] = max;
            }
        }

        private void GenerateSelect(VipsRegion outRegion, VipsRankSequence seq, VipsRank rank)
        {
            // Inner loop for select-sorting TYPE.
            VipsPel[] q = (VipsPel[])VipsRegionAddr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top + outRegion.Valid.Height);
            VipsPel[] p = (VipsPel[])VipsRegionAddr(seq.Ir, seq.Ir.Valid.Left, seq.Ir.Valid.Top + seq.Ir.Valid.Height);

            for (int x = 0; x < outRegion.Valid.Width; x++)
            {
                VipsPel[] sort = seq.Sort;

                // Copy window into sort[].
                for (int j = 0; j < rank.Height; j++)
                {
                    for (int i = 0, k = 0; i < inImg.Bands * rank.Width; i += inImg.Bands, k++)
                        sort[k] = p[i];

                    p += VipsRegionLskip(seq.Ir);
                }

                // Rearrange sort[] to make the index-th element the index-th
                // smallest, adapted from Numerical Recipes in C.
                int lower = 0;
                int upper = rank.n - 1;

                for (;;)
                {
                    if (upper - lower < 2)
                    {
                        // 1 or 2 elements left.
                        if (upper - lower == 1 && sort[lower] > sort[upper])
                            VipsSwap(sort, lower, upper);
                        break;
                    }
                    else
                    {
                        // Pick mid-point of remaining elements.
                        int mid = (lower + upper) >> 1;

                        // Sort lower/mid/upper elements, hold
                        // midpoint in sort[lower + 1] for
                        // partitioning.
                        VipsSwap(sort, lower + 1, sort[mid]);
                        if (sort[lower] > sort[upper])
                            VipsSwap(sort, lower, sort[upper]);
                        if (sort[lower + 1] > sort[upper])
                            VipsSwap(sort, lower + 1, sort[upper]);
                        if (sort[lower] > sort[lower + 1])
                            VipsSwap(sort, lower, sort[lower + 1]);

                        int i = lower + 1;
                        int j = upper;

                        for (;;)
                        {
                            // Search for out of order elements.
                            do
                                i++;
                            while (sort[i] < sort[lower + 1]);
                            do
                                j--;
                            while (sort[j] > sort[lower + 1]);

                            if (j < i)
                                break;
                            VipsSwap(sort, i, sort[j]);
                        }

                        // Replace mid element.
                        sort[lower + 1] = sort[j];
                        sort[j] = sort[lower + 1];

                        // Move to partition with the kth element.
                        if (j >= rank.Index)
                            upper = j - 1;
                        if (j <= rank.Index)
                            lower = i;
                    }
                }

                q[x] = sort[rank.Index];
            }
        }
    }

    public class VipsRankStop : VipsImageGenerateStop
    {
        private readonly VipsRankSequence seq;

        public VipsRankStop(VipsRankSequence seq)
        {
            this.seq = seq;
        }

        public override void Dispose()
        {
            if (seq != null)
                seq.Dispose();
        }
    }

    public class VipsRankBuild : VipsObjectBuild
    {
        private readonly VipsRank rank;

        public VipsRankBuild(VipsRank rank)
        {
            this.rank = rank;
        }

        public override int Build(VipsObject obj)
        {
            VipsMorphology morphology = (VipsMorphology)obj;
            VipsImage inImg = morphology.In;

            if (!vips_image_decode(inImg, out Out))
                return -1;

            if (!vips_check_noncomplex("rank", inImg))
                return -1;

            if (Width > inImg.Xsize || Height > inImg.Ysize)
            {
                vips_error("rank", "%s", "window too large");
                return -1;
            }

            n = Width * Height;
            if (Index < 0 || Index > n - 1)
            {
                vips_error("rank", "%s", "index out of range");
                return -1;
            }

            // Enable the hist path if it'll probably help.
            if (inImg.BandFmt == VipsBandFormat.UChar)
            {
                // The hist path is always faster for windows larger than about
                // 10x10, and faster for >3x3 on the non-max/min case.
                if (n > 90)
                    HistPath = true;
                else if (n > 10 && Index != 0 && Index != n - 1)
                    HistPath = true;
            }

            // Expand the input.
            VipsImage expandedImg;
            if (!vips_embed(inImg, out expandedImg,
                Width / 2, Height / 2,
                inImg.Xsize + Width - 1, inImg.Ysize + Height - 1,
                "extend", VipsExtendMode.Copy,
                null))
                return -1;

            Out = vips_image_new();
            if (!vips_image_pipeline(Out, VipsDemandHint.FatStrip, expandedImg, null))
                return -1;
            Out.Xsize -= Width - 1;
            Out.Ysize -= Height - 1;

            if (!vips_image_generate(Out,
                new VipsRankStart(this),
                new VipsRankGenerate(),
                new VipsRankStop()))
                return -1;

            Out.Xoffset = 0;
            Out.Yoffset = 0;

            vips_reorder_margin_hint(Out, Width * Height);

            return 0;
        }
    }

    public static int Rank(VipsImage inImg, out VipsImage outImg,
        int width, int height, int index)
    {
        VipsRank rank = new VipsRank();
        rank.Width = width;
        rank.Height = height;
        rank.Index = index;

        if (!vips_rank_build(rank))
            return -1;

        return 0;
    }

    public static int Median(VipsImage inImg, out VipsImage outImg,
        int size)
    {
        return Rank(inImg, out outImg, size, size, (size * size) / 2);
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available and contains the necessary classes for working with VIPS images. You may need to modify the code to match your specific requirements.

Also note that this implementation does not include all the error checking and handling that the original C code has, but it should give you a good starting point for converting the rest of the code.