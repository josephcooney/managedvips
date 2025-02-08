Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsImage
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Coding { get; set; }
}

public class Overlapping
{
    public VipsImage Ref { get; set; }
    public VipsImage Sec { get; set; }
    public VipsImage Out { get; set; }
    public int Dx { get; set; }
    public int Dy { get; set; }
    public int Mwidth { get; set; }

    public VipsRect Rarea { get; set; }
    public VipsRect Sarea { get; set; }
    public VipsRect Oarea { get; set; }
    public VipsRect Overlap { get; set; }

    public int[] First { get; set; }
    public int[] Last { get; set; }

    public GMutex FlLock { get; set; }

    public Func<VipsRegion, MergeInfo, Overlapping, VipsRect> Blend { get; set; }

    public int Blsize { get; set; }
}

public class MergeInfo
{
    public VipsRegion Rir { get; set; }
    public VipsRegion Sir { get; set; }

    public float[] From1 { get; set; }
    public float[] From2 { get; set; }
    public float[] Merge { get; set; }
}

public class VipsRect
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public static bool Equals(VipsRect a, VipsRect b)
    {
        return a.Left == b.Left && a.Top == b.Top &&
               a.Width == b.Width && a.Height == b.Height;
    }

    public static void Intersectrect(VipsRect a, VipsRect b, VipsRect c)
    {
        c.Left = Math.Max(a.Left, b.Left);
        c.Top = Math.Max(a.Top, b.Top);
        c.Width = Math.Min(a.Right(), b.Right()) - c.Left;
        c.Height = Math.Min(a.Bottom(), b.Bottom()) - c.Top;
    }

    public static void Unionrect(VipsRect a, VipsRect b, VipsRect c)
    {
        c.Left = Math.Min(a.Left, b.Left);
        c.Top = Math.Min(a.Top, b.Top);
        c.Width = Math.Max(a.Right(), b.Right()) - c.Left;
        c.Height = Math.Max(a.Bottom(), b.Bottom()) - c.Top;
    }

    public static bool Isempty(VipsRect rect)
    {
        return rect.Width <= 0 || rect.Height <= 0;
    }
}

public class VipsRegion
{
    public VipsImage Im { get; set; }
    public VipsRect Valid { get; set; }

    public void Black()
    {
        // implementation omitted for brevity
    }

    public bool Prepare(VipsRect rect)
    {
        // implementation omitted for brevity
        return false;
    }

    public bool Region(VipsRegion out_region, VipsRegion ir, VipsRect reg, int left, int top)
    {
        // implementation omitted for brevity
        return false;
    }
}

public class GMutex
{
    public void Lock()
    {
        // implementation omitted for brevity
    }

    public void Unlock()
    {
        // implementation omitted for brevity
    }
}

class Program
{
    static double[] coef1 = null;
    static double[] coef2 = null;
    static int[] icoef1 = null;
    static int[] icoef2 = null;

    const int BLEND_SIZE = 256;
    const int BLEND_SCALE = 255;
    const int BLEND_SHIFT = 8;

    public static void Main(string[] args)
    {
        // implementation omitted for brevity
    }

    public static VipsImage MakeBlendLuts()
    {
        if (coef1 != null && coef2 != null)
            return null;

        coef1 = new double[BLEND_SIZE];
        coef2 = new double[BLEND_SIZE];
        icoef1 = new int[BLEND_SIZE];
        icoef2 = new int[BLEND_SIZE];

        for (int x = 0; x < BLEND_SIZE; x++)
        {
            double a = Math.PI * x / (BLEND_SIZE - 1.0);

            coef1[x] = (Math.Cos(a) + 1.0) / 2.0;
            coef2[x] = 1.0 - coef1[x];
            icoef1[x] = (int)(coef1[x] * BLEND_SCALE);
            icoef2[x] = (int)(coef2[x] * BLEND_SCALE);
        }

        return null;
    }

    public static int FindFirst(VipsRegion ir, ref int pos, int x, int y, int w)
    {
        VipsPel[] pr = new VipsPel[w * ir.Im.Bands];
        for (int i = 0; i < w * ir.Im.Bands; i++)
            pr[i] = new VipsPel();

        int ne = w * ir.Im.Bands;
        if (VipsBandFormat.IsComplex(ir.Im.BandFmt))
            ne *= 2;

        switch (ir.Im.BandFmt)
        {
            case VIPS_FORMAT_UCHAR:
                for (int i = 0; i < ne; i++)
                    if (pr[i].Value != 0)
                        break;
                pos = x + i / ir.Im.Bands;
                return 0;

            // implementation omitted for brevity
        }

        return -1;
    }

    public static int FindLast(VipsRegion ir, ref int pos, int x, int y, int w)
    {
        VipsPel[] pr = new VipsPel[w * ir.Im.Bands];
        for (int i = 0; i < w * ir.Im.Bands; i++)
            pr[i] = new VipsPel();

        int ne = w * ir.Im.Bands;
        if (VipsBandFormat.IsComplex(ir.Im.BandFmt))
            ne *= 2;

        switch (ir.Im.BandFmt)
        {
            case VIPS_FORMAT_UCHAR:
                for (int i = ne - 1; i >= 0; i--)
                    if (pr[i].Value != 0)
                        break;
                pos = x + i / ir.Im.Bands;
                return 0;

            // implementation omitted for brevity
        }

        return -1;
    }

    public static int MakeFirstlast(MergeInfo inf, Overlapping ovlap, VipsRect oreg)
    {
        VipsRegion rir = inf.Rir;
        VipsRegion sir = inf.Sir;

        VipsRect rr = new VipsRect();
        VipsRect sr = new VipsRect();

        int y, yr, ys;
        bool missing = false;

        lock (ovlap.FlLock)
        {
            for (y = oreg.Top; y < VIPS_RECT_BOTTOM(oreg); y++)
            {
                const int j = y - ovlap.Overlap.Top;
                const int first = ovlap.First[j];

                if (first < 0)
                {
                    missing = true;
                    break;
                }
            }

            if (!missing)
                return 0;

            rr.Left = ovlap.Overlap.Left;
            rr.Top = oreg.Top;
            rr.Width = ovlap.Overlap.Width;
            rr.Height = oreg.Height;
            rr.Left -= ovlap.Rarea.Left;
            rr.Top -= ovlap.Rarea.Top;

            sr.Left = ovlap.Overlap.Left;
            sr.Top = oreg.Top;
            sr.Width = ovlap.Overlap.Width;
            sr.Height = oreg.Height;
            sr.Left -= ovlap.Sarea.Left;
            sr.Top -= ovlap.Sarea.Top;

            for (y = oreg.Top, yr = rr.Top, ys = sr.Top; y < VIPS_RECT_BOTTOM(oreg); y++, yr++, ys++)
            {
                const int j = y - ovlap.Overlap.Top;
                const int first = ovlap.First[j];
                const int last = ovlap.Last[j];

                if (FindFirst(sir, ref pos, sr.Left, ys, sr.Width) ||
                    FindLast(rir, ref pos, rr.Left, yr, rr.Width))
                {
                    return -1;
                }

                ovlap.First[j] += ovlap.Sarea.Left;
                ovlap.Last[j] += ovlap.Rarea.Left;

                if (ovlap.Mwidth >= 0 && ovlap.Last[j] - ovlap.First[j] > ovlap.Mwidth)
                {
                    int shrinkby = (ovlap.Last[j] - ovlap.First[j]) - ovlap.Mwidth;
                    ovlap.First[j] += shrinkby / 2;
                    ovlap.Last[j] -= shrinkby / 2;
                }
            }

            ovlap.FlLock.Unlock();
        }

        return 0;
    }

    public static int LRBlend(VipsRegion out_region, MergeInfo inf, Overlapping ovlap, VipsRect oreg)
    {
        VipsRegion rir = inf.Rir;
        VipsRegion sir = inf.Sir;

        VipsRect prr = new VipsRect();
        VipsRect psr = new VipsRect();

        int y, yr, ys;

        if (MakeFirstlast(inf, ovlap, oreg) != 0)
            return -1;

        prr.Left = oreg.Left - ovlap.Rarea.Left;
        prr.Top = oreg.Top - ovlap.Rarea.Top;
        prr.Width = ovlap.Rarea.Width;
        prr.Height = oreg.Height;

        psr.Left = oreg.Left - ovlap.Sarea.Left;
        psr.Top = oreg.Top - ovlap.Sarea.Top;
        psr.Width = ovlap.Sarea.Width;
        psr.Height = oreg.Height;

        if (VipsRegion.Prepare(rir, ref prr) ||
            VipsRegion.Prepare(sir, ref psr))
            return -1;

        for (y = oreg.Top, yr = prr.Top, ys = psr.Top; y < VIPS_RECT_BOTTOM(oreg); y++, yr++, ys++)
        {
            VipsPel[] pr = new VipsPel[ovlap.Blsize * inf.Rir.Im.Bands];
            VipsPel[] ps = new VipsPel[ovlap.Blsize * inf.Sir.Im.Bands];
            VipsPel[] q = new VipsPel[ovlap.Blsize * out_region.Im.Bands];

            for (int i = 0; i < ovlap.Blsize * inf.Rir.Im.Bands; i++)
                pr[i] = new VipsPel();

            for (int i = 0; i < ovlap.Blsize * inf.Sir.Im.Bands; i++)
                ps[i] = new VipsPel();

            for (int i = 0; i < ovlap.Blsize * out_region.Im.Bands; i++)
                q[i] = new VipsPel();

            const int j = y - ovlap.Overlap.Top;
            const int first = ovlap.First[j];
            const int last = ovlap.Last[j];

            switch (out_region.Im.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    IBLEND(unsigned char, inf.Rir.Im.Bands, pr, ps, q);
                    break;

                // implementation omitted for brevity
            }
        }

        return 0;
    }

    public static int LRBlendLabpack(VipsRegion out_region, MergeInfo inf, Overlapping ovlap, VipsRect oreg)
    {
        VipsRegion rir = inf.Rir;
        VipsRegion sir = inf.Sir;

        VipsRect prr = new VipsRect();
        VipsRect psr = new VipsRect();

        int y, yr, ys;

        if (MakeFirstlast(inf, ovlap, oreg) != 0)
            return -1;

        prr.Left = oreg.Left - ovlap.Rarea.Left;
        prr.Top = oreg.Top - ovlap.Rarea.Top;
        prr.Width = ovlap.Rarea.Width;
        prr.Height = oreg.Height;

        psr.Left = oreg.Left - ovlap.Sarea.Left;
        psr.Top = oreg.Top - ovlap.Sarea.Top;
        psr.Width = ovlap.Sarea.Width;
        psr.Height = oreg.Height;

        if (VipsRegion.Prepare(rir, ref prr) ||
            VipsRegion.Prepare(sir, ref psr))
            return -1;

        for (y = oreg.Top, yr = prr.Top, ys = psr.Top; y < VIPS_RECT_BOTTOM(oreg); y++, yr++, ys++)
        {
            float[] r = new float[ovlap.Blsize * 3];
            float[] s = new float[ovlap.Blsize * 3];

            vips_LabQ2Lab_vec(r, pr, oreg.Width);
            vips_LabQ2Lab_vec(s, ps, oreg.Width);

            for (int i = 0; i < ovlap.Blsize * 3; i++)
                r[i] += s[i];

            FBLEND(float, 3, r, q);
        }

        return 0;
    }

    public static Overlapping BuildMergestate(string domain, VipsImage ref_image, VipsImage sec_image, VipsImage out_image, int dx, int dy, int mwidth)
    {
        // implementation omitted for brevity
        return null;
    }

    public static Overlapping BuildLRState(VipsImage ref_image, VipsImage sec_image, VipsImage out_image, int dx, int dy, int mwidth)
    {
        // implementation omitted for brevity
        return null;
    }

    public static int AttachInput(VipsRegion out_region, VipsRegion ir, VipsRect area)
    {
        // implementation omitted for brevity
        return 0;
    }

    public static int CopyInput(VipsRegion out_region, VipsRegion ir, VipsRect area, VipsRect reg)
    {
        // implementation omitted for brevity
        return 0;
    }

    public static int MergeGen(VipsRegion out_region, object seq, object a, object b, ref bool stop)
    {
        MergeInfo inf = (MergeInfo)seq;
        Overlapping ovlap = (Overlapping)a;

        VipsRect rreg = new VipsRect();
        VipsRect sreg = new VipsRect();

        vips_rect_intersectrect(out_region.Valid, ovlap.Rpart, ref rreg);
        vips_rect_intersectrect(out_region.Valid, ovlap.Spart, ref sreg);

        if (vips_rect_equalsrect(out_region.Valid, ref rreg))
            return AttachInput(out_region, inf.Rir, ref ovlap.Rarea);

        if (vips_rect_equalsrect(out_region.Valid, ref sreg))
            return AttachInput(out_region, inf.Sir, ref ovlap.Sarea);

        VipsRect oreg = new VipsRect();
        vips_rect_intersectrect(out_region.Valid, ovlap.Overlap, ref oreg);

        if (vips_region_black(out_region) != 0)
            return -1;

        if (!vips_rect_isempty(ref rreg))
            CopyInput(out_region, inf.Rir, ref ovlap.Rarea, ref rreg);

        if (!vips_rect_isempty(ref sreg))
            CopyInput(out_region, inf.Sir, ref ovlap.Sarea, ref sreg);

        // implementation omitted for brevity
        return 0;
    }

    public static int StopMerge(object seq, object a, object b)
    {
        MergeInfo inf = (MergeInfo)seq;

        VIPS_UNREF(inf.Rir);
        VIPS_UNREF(inf.Sir);
        VIPS_FREE(inf.From1);
        VIPS_FREE(inf.From2);
        VIPS_FREE(inf.Merge);

        return 0;
    }

    public static object StartMerge(VipsImage out_image, object a, object b)
    {
        Overlapping ovlap = (Overlapping)a;

        MergeInfo inf = new MergeInfo();

        // implementation omitted for brevity
        return inf;
    }

    public static int LRmerge(VipsImage ref_image, VipsImage sec_image, VipsImage out_image, int dx, int dy, int mwidth)
    {
        Overlapping ovlap;

        if (dx > 0 || dx < 1 - ref_image.Xsize)
            return Insert(ref_image, sec_image, out_image);

        if (!(ovlap = BuildLRState(ref_image, sec_image, out_image, dx, dy, mwidth)))
            return -1;

        if (vips_image_pipelinev(out_image, VIPS_DEMAND_STYLE_THINSTRIP, ovlap.Ref, ovlap.Sec, null) != 0)
            return -1;

        out_image.Xsize = ovlap.Oarea.Width;
        out_image.Ysize = ovlap.Oarea.Height;
        out_image.Xoffset = -dx;
        out_image.Yoffset = -dy;

        if (vips_image_generate(out_image, StartMerge, MergeGen, StopMerge, ovlap, null) != 0)
            return -1;

        return 0;
    }

    public static string GetMosaicName(VipsImage image)
    {
        // implementation omitted for brevity
        return null;
    }

    public static void AddMosaicName(VipsImage image)
    {
        // implementation omitted for brevity
    }
}
```

Note that this is not a complete translation, as some parts of the code are missing or have been simplified. Additionally, C# does not support direct memory management like C does, so some modifications were necessary to accommodate this difference.

Also note that `VIPS_FORMAT_UCHAR`, `VIPS_FORMAT_CHAR`, etc. are assumed to be enums defined elsewhere in the codebase. Similarly, `vips_image_pipelinev`, `vips_image_generate`, and other functions are assumed to be part of a larger library or framework.

Please let me know if you'd like me to clarify any specific parts of this translation!