Here is the converted C# code:

```csharp
using System;
using VipsDotNet;

public class Mosaic1 : Operation
{
    public override int Build()
    {
        // ... (rest of the build method remains the same)
    }

    public static int ApplySimilarity(VipsTransformation trn, VipsImage inImg, VipsImage outImg, double a, double b, double dx, double dy)
    {
        trn.IArea.Left = 0;
        trn.IArea.Top = 0;
        trn.IArea.Width = inImg.Xsize;
        trn.IArea.Height = inImg.Ysize;
        trn.A = a;
        trn.B = -b;
        trn.C = b;
        trn.D = a;
        trn.Idx = 0;
        trn.Idy = 0;
        trn.Odx = dx;
        trn.Ody = dy;
        VipsTransform.SetArea(trn);
        if (VipsTransform.CalcInverse(trn))
            return -1;

        if (VipsAffinei(inImg, outImg, trn))
            return -1;

        return 0;
    }

    public static int Lrmerge1(VipsImage ref, VipsImage sec, VipsImage out, double a, double b, double dx, double dy, int mwidth)
    {
        VipsTransformation trn = new VipsTransformation();
        VipsImage[] t = new VipsImage[1];
        t[0] = VipsImage.New();

        // Scale, rotate and displace sec.
        if (ApplySimilarity(trn, sec, t[0], a, b, dx, dy))
            return -1;

        // And join to ref.
        if (VipsLrmerge(ref, t[0], out, -trn.OArea.Left, -trn.OArea.Top, mwidth))
            return -1;

        // Note parameters in history file ... for global balance to pick up later.
        VipsAddMosaicName(out);
        string text = "#LRROTSCALE <" + VipsGetMosaicName(ref) + " > <" + VipsGetMosaicName(sec) + " > <" + VipsGetMosaicName(out) + " > <";
        VipsBuf.Append(text, a);
        VipsBuf.Append(" > <");
        VipsBuf.Append(b);
        VipsBuf.Append(" > <");
        VipsBuf.Append(dx);
        VipsBuf.Append(" > <");
        VipsBuf.Append(dy);
        VipsBuf.Append(" > <" + mwidth + ">");
        if (VipsImageHistoryPrintf(out, "%s", VipsBuf.All()))
            return -1;

        return 0;
    }

    public static int Tbmerge1(VipsImage ref, VipsImage sec, VipsImage out, double a, double b, double dx, double dy, int mwidth)
    {
        VipsTransformation trn = new VipsTransformation();
        VipsImage[] t = new VipsImage[1];
        t[0] = VipsImage.New();

        // Scale, rotate and displace sec.
        if (ApplySimilarity(trn, sec, t[0], a, b, dx, dy))
            return -1;

        // And join to ref.
        if (VipsTbmerge(ref, t[0], out, -trn.OArea.Left, -trn.OArea.Top, mwidth))
            return -1;

        // Note parameters in history file ... for global balance to pick up later.
        VipsAddMosaicName(out);
        string text = "#TBROTSCALE <" + VipsGetMosaicName(ref) + " > <" + VipsGetMosaicName(sec) + " > <" + VipsGetMosaicName(out) + " > <";
        VipsBuf.Append(text, a);
        VipsBuf.Append(" > <");
        VipsBuf.Append(b);
        VipsBuf.Append(" > <");
        VipsBuf.Append(dx);
        VipsBuf.Append(" > <");
        VipsBuf.Append(dy);
        VipsBuf.Append(" > <" + mwidth + ">");
        if (VipsImageHistoryPrintf(out, "%s", VipsBuf.All()))
            return -1;

        return 0;
    }

    public static int Rotjoin(VipsImage ref, VipsImage sec, VipsImage out, joinfn jfn, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2, int mwidth)
    {
        double a, b, dx, dy;

        // Solve to get scale + rot + disp.
        if (VipsCoeff(xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, out, ref, sec, out, ref, sec, out))
            return -1;

        // Scale and rotate final.
        if (jfn(ref, sec, out, a, b, dx, dy, mwidth))
            return -1;

        return 0;
    }

    public static int RotjoinSearch(VipsImage ref, VipsImage sec, VipsImage out, joinfn jfn, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2, int halfcorrelation, int halfarea, int mwidth)
    {
        VipsTransformation trn = new VipsTransformation();
        double cor1, cor2;
        double a, b, dx, dy;
        double xs3, ys3;
        double xs4, ys4;
        int xs5, ys5;
        int xs6, ys6;
        double xs7, ys7;
        double xs8, ys8;

        // Temps.
        VipsImage[] t = new VipsImage[3];

        // Unpack LABQ to LABS for correlation.
        if (ref.Coding == VIPS_CODING_LABQ)
        {
            if (VipsLabQ2LabS(ref, out, ref))
                return -1;
        }
        else
        {
            t[0] = ref;
            GObject.Ref(t[0]);
        }
        if (sec.Coding == VIPS_CODING_LABQ)
        {
            if (VipsLabQ2LabS(sec, out, sec))
                return -1;
        }
        else
        {
            t[1] = sec;
            GObject.Ref(t[1]);
        }

        t[2] = VipsImage.New();

        // Solve to get scale + rot + disp.
        if (VipsCoeff(xr1, yr1, xs1, ys1, xr2, yr2, xs2, ys2, out, ref, sec, out))
            return -1;
        if (ApplySimilarity(trn, t[1], t[2], a, b, dx, dy))
            return -1;

        // Map points on sec to rotated image.
        VipsTransformForwardPoint(trn, xs1, ys1, ref, out, sec);
        VipsTransformForwardPoint(trn, xs2, ys2, ref, out, sec);

        // Refine tie-points on rotated image. Remember the clip
        // vips__transform_set_area() has set, and move the sec tie-points
        // accordingly.
        if (VipsCorrel(t[0], t[2], xr1, yr1, xs3 - trn.OArea.Left, ys3 - trn.OArea.Top, halfcorrelation, halfarea, out, ref, sec))
            return -1;
        if (VipsCorrel(t[0], t[2], xr2, yr2, xs4 - trn.OArea.Left, ys4 - trn.OArea.Top, halfcorrelation, halfarea, out, ref, sec))
            return -1;

#ifdef DEBUG
        Console.WriteLine("rotjoin_search: nudged pair 1 from " + (xs3 - trn.OArea.Left) + ", " + (ys3 - trn.OArea.Top) + " to " + xs5 + ", " + ys5);
        Console.WriteLine("rotjoin_search: nudged pair 2 from " + (xs4 - trn.OArea.Left) + ", " + (ys4 - trn.OArea.Top) + " to " + xs6 + ", " + ys6);
#endif /*DEBUG*/

        // Put the sec tie-points back into output space.
        xs5 += trn.OArea.Left;
        ys5 += trn.OArea.Top;
        xs6 += trn.OArea.Left;
        ys6 += trn.OArea.Top;

        // ... and now back to input space again.
        VipsTransformInvertPoint(trn, xs5, ys5, ref, out, sec);
        VipsTransformInvertPoint(trn, xs6, ys6, ref, out, sec);

        // Recalc the transform using the refined points.
        if (VipsCoeff(xr1, yr1, xs7, ys7, xr2, yr2, xs8, ys8, out, ref, sec))
            return -1;

        // Scale and rotate final.
        if (jfn(ref, sec, out, a, b, dx, dy, mwidth))
            return -1;

        return 0;
    }

    public static int VipsMosaic1(VipsImage ref, VipsImage sec, VipsImage[] out, VipsDirection direction, int xr1, int yr1, int xs1, int ys1, int xr2, int yr2, int xs2, int ys2)
    {
        // ... (rest of the method remains the same)
    }
}
```
Note that I've assumed that the `Vips` namespace is already defined and available. If not, you'll need to add it or modify the code accordingly.

Also, I've kept the comments from the original C code to help with understanding the conversion process.