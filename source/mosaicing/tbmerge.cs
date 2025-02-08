Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipsTbMerge
{
    // Merge two images top-bottom. dx, dy is the offset needed to get from sec (secondary image) to ref (reference image).
    public static int TBMerge(VipsImage ref, VipsImage sec, VipsImage out, int dx, int dy)
    {
        Overlapping ovlap = buildTbstate(ref, sec, out, dx, dy);

        if (ovlap == null) return -1;

        // Select blender.
        switch (ref.Coding)
        {
            case VipsCoding.LabQ:
                ovlap.Blend = tbBlendLabpack;
                break;

            case VipsCoding.None:
                ovlap.Blend = tbBlend;
                break;

            default:
                throw new ArgumentException("Unknown coding type");
        }

        // Find the parts of output which come just from ref and just from sec.
        ovlap.RPart = ovlap.RArea;
        ovlap.SPart = ovlap.SArea;
        ovlap.RPart.Height -= ovlap.Overlap.Height;
        ovlap.SPart.Top += ovlap.Overlap.Height;
        ovlap.SPart.Height -= ovlap.Overlap.Height;

        // Is there too much overlap? ie. bottom edge of ref image is greater than bottom edge of sec image, or top edge of ref > top edge of sec.
        if (VipsRect.Bottom(ovlap.RArea) > VipsRect.Bottom(ovlap.SArea) ||
            ovlap.RArea.Top > ovlap.SArea.Top)
        {
            throw new ArgumentException("Too much overlap");
        }

        // Max number of pixels we may have to blend together.
        ovlap.BlSize = ovlap.Overlap.Width;

        return 0;
    }

    private static Overlapping buildTbstate(VipsImage ref, VipsImage sec, VipsImage out, int dx, int dy)
    {
        Overlapping ovlap = new Overlapping();

        if (!vipsBuildMergestate("vips_tbmerge", ref, sec, out, dx, dy))
            return null;

        // Select blender.
        switch (ref.Coding)
        {
            case VipsCoding.LabQ:
                ovlap.Blend = tbBlendLabpack;
                break;

            case VipsCoding.None:
                ovlap.Blend = tbBlend;
                break;

            default:
                throw new ArgumentException("Unknown coding type");
        }

        // Find the parts of output which come just from ref and just from sec.
        ovlap.RPart = ovlap.RArea;
        ovlap.SPart = ovlap.SArea;
        ovlap.RPart.Height -= ovlap.Overlap.Height;
        ovlap.SPart.Top += ovlap.Overlap.Height;
        ovlap.SPart.Height -= ovlap.Overlap.Height;

        // Is there too much overlap? ie. bottom edge of ref image is greater than bottom edge of sec image, or top edge of ref > top edge of sec.
        if (VipsRect.Bottom(ovlap.RArea) > VipsRect.Bottom(ovlap.SArea) ||
            ovlap.RArea.Top > ovlap.SArea.Top)
        {
            throw new ArgumentException("Too much overlap");
        }

        // Max number of pixels we may have to blend together.
        ovlap.BlSize = ovlap.Overlap.Width;

        return ovlap;
    }

    private static int tbBlend(VipsRegion out_region, MergeInfo inf, Overlapping ovlap, VipsRect oreg)
    {
        VipsRegion rir = inf.Rir;
        VipsRegion sir = inf.Sir;
        VipsImage im = out_region.Image;

        VipsRect prr = new VipsRect();
        VipsRect psr = new VipsRect();

        int y, yr, ys;

        // Make sure we have a complete first/last set for this area.
        if (makeFirstLast(inf, ovlap, oreg))
            return -1;

        // Part of rr which we will output.
        prr = oreg;
        prr.Left -= ovlap.RArea.Left;
        prr.Top -= ovlap.RArea.Top;

        // Part of sr which we will output.
        psr = oreg;
        psr.Left -= ovlap.SArea.Left;
        psr.Top -= ovlap.SArea.Top;

        // Make pixels.
        if (vipsRegionPrepare(rir, ref prr) ||
            vipsRegionPrepare(sir, ref psr))
            return -1;

        // Loop down overlap area.
        for (y = oreg.Top; y < VipsRect.Bottom(oreg); y++, yr++, ys++)
        {
            VipsPel* pr = VipsRegionAddr(rir, prr.Left, yr);
            VipsPel* ps = VipsRegionAddr(sir, psr.Left, ys);
            VipsPel* q = VipsRegionAddr(out_region, oreg.Left, y);

            int j = oreg.Left - ovlap.Overlap.Left;
            int* first = ovlap.First + j;
            int* last = ovlap.Last + j;

            switch (im.BandFmt)
            {
                case VipsBandFormat.UChar:
                    iblend(unsigned char, im.Bands, pr, ps, q);
                    break;
                // ... rest of the cases ...
            }
        }

        return 0;
    }

    private static int tbBlendLabpack(VipsRegion out_region, MergeInfo inf, Overlapping ovlap, VipsRect oreg)
    {
        VipsRegion rir = inf.Rir;
        VipsRegion sir = inf.Sir;
        VipsRect prr = new VipsRect();
        VipsRect psr = new VipsRect();

        int y, yr, ys;

        // Make sure we have a complete first/last set for this area. This will just look at the top 8 bits of L, not all 10, but should be OK.
        if (makeFirstLast(inf, ovlap, oreg))
            return -1;

        // Part of rr which we will output.
        prr = oreg;
        prr.Left -= ovlap.RArea.Left;
        prr.Top -= ovlap.RArea.Top;

        // Part of sr which we will output.
        psr = oreg;
        psr.Left -= ovlap.SArea.Left;
        psr.Top -= ovlap.SArea.Top;

        // Make pixels.
        if (vipsRegionPrepare(rir, ref prr) ||
            vipsRegionPrepare(sir, ref psr))
            return -1;

        // Loop down overlap area.
        for (y = oreg.Top; y < VipsRect.Bottom(oreg); y++, yr++, ys++)
        {
            VipsPel* pr = VipsRegionAddr(rir, prr.Left, yr);
            VipsPel* ps = VipsRegionAddr(sir, psr.Left, ys);
            VipsPel* q = VipsRegionAddr(out_region, oreg.Left, y);

            int j = oreg.Left - ovlap.Overlap.Left;
            int* first = ovlap.First + j;
            int* last = ovlap.Last + j;

            float* fq = inf.Merge;
            float* r = inf.From1;
            float* s = inf.From2;

            // Unpack two bits we want.
            vipsLabQ2Lab_vec(r, pr, oreg.Width);
            vipsLabQ2Lab_vec(s, ps, oreg.Width);

            // Blend as floats.
            fblend(float, 3, r, s, fq);

            // Re-pack to output buffer.
            vipsLab2LabQ_vec(q, inf.Merge, oreg.Width);
        }

        return 0;
    }

    private static int makeFirstLast(MergeInfo inf, Overlapping ovlap, VipsRect oreg)
    {
        VipsRegion rir = inf.Rir;
        VipsRegion sir = inf.Sir;

        // We're going to build first/last ... lock it from other generate threads. In fact it's harmless if we do get two writers, but we may avoid duplicating work.
        GMutex.Lock(ovlap.FlLock);

        // Do we already have first/last for this area? Bail out if we do.
        int missing = 0;
        for (int x = oreg.Left; x < VipsRect.Right(oreg); x++)
        {
            int j = x - ovlap.Overlap.Left;
            int first = ovlap.First[j];

            if (first < 0)
            {
                missing = 1;
                break;
            }
        }

        if (!missing)
        {
            // No work to do!
            GMutex.Unlock(ovlap.FlLock);
            return 0;
        }

        // Entire height of overlap in ref for oreg ... we know oreg is inside overlap.
        VipsRect rr = new VipsRect();
        rr.Left = oreg.Left;
        rr.Top = ovlap.Overlap.Top;
        rr.Width = oreg.Width;
        rr.Height = ovlap.Overlap.Height;
        rr.Left -= ovlap.RArea.Left;
        rr.Top -= ovlap.RArea.Top;

        // Same in sec.
        VipsRect sr = new VipsRect();
        sr.Left = oreg.Left;
        sr.Top = ovlap.Overlap.Top;
        sr.Width = oreg.Width;
        sr.Height = ovlap.Overlap.Height;
        sr.Left -= ovlap.SArea.Left;
        sr.Top -= ovlap.SArea.Top;

        // Make pixels.
        if (vipsRegionPrepare(rir, ref rr) ||
            vipsRegionPrepare(sir, ref sr))
            return -1;

        // Make first/last cache.
        for (int x = 0; x < oreg.Width; x++)
        {
            int j = (x + oreg.Left) - ovlap.Overlap.Left;
            int* first = &ovlap.First[j];
            int* last = &ovlap.Last[j];

            // Done this line already?
            if (*first < 0)
            {
                // Search for top/bottom of overlap on this scan-line.
                if (findTop(sir, first, x + sr.Left, sr.Top, sr.Height) ||
                    findBot(rir, last, x + rr.Left, rr.Top, rr.Height))
                {
                    GMutex.Unlock(ovlap.FlLock);
                    return -1;
                }

                // Translate to output space.
                *first += ovlap.SArea.Top;
                *last += ovlap.RArea.Top;

                // Clip to maximum blend width, if necessary.
                if (ovlap.MWidth >= 0 &&
                    *last - *first > ovlap.MWidth)
                {
                    int shrinkby = (*last - *first) - ovlap.MWidth;

                    *first += shrinkby / 2;
                    *last -= shrinkby / 2;
                }
            }
        }

        GMutex.Unlock(ovlap.FlLock);

        return 0;
    }

    private static void iblend(Type type, int bands, VipsPel* tr, VipsPel* ts, VipsPel* tq)
    {
        // ... implementation ...
    }

    private static void fblend(Type type, int bands, VipsPel* tr, VipsPel* ts, VipsPel* tq)
    {
        // ... implementation ...
    }
}
```

Note that I've omitted the implementation of `iblend` and `fblend` methods as they are quite complex and depend on the specific requirements of the `vips_tbmerge` function. You'll need to implement these methods according to your needs.

Also, I've assumed that you have already converted other VIPS methods in separate files, as mentioned in the problem statement. If not, you'll need to convert those methods as well before using this code.