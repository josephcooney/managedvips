Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class ZeroxGen : IGenerate
{
    public int Generate(Region outRegion, object seq, object a, object b)
    {
        Region ir = (Region)seq;
        Image inImage = (Image)a;
        int flag = (int)b;
        Rect irect;
        Rect r = outRegion.Valid;

        // Range of pixels we loop over.
        int le = r.Left;
        int to = r.Top;
        int bo = IM_RECT_BOTTOM(r);
        int ba = inImage.Bands;
        int ne = ba * r.Width;

        int i, y;

        // We need to be able to see one pixel to the right.
        irect.Top = r.Top;
        irect.Left = r.Left;
        irect.Width = r.Width + 1;
        irect.Height = r.Height;
        if (im_prepare(ir, ref irect))
            return -1;

        for (y = to; y < bo; y++)
        {
            VipsPel[] p = IM_REGION_ADDR(ir, le, y);
            VipsPel[] q = IM_REGION_ADDR(outRegion, le, y);

            switch (inImage.BandFmt)
            {
                case IM_BANDFMT_CHAR:
                    LoopSignedChar(p, q, ne, ba);
                    break;
                case IM_BANDFMT_SHORT:
                    LoopSignedShort(p, q, ne, ba);
                    break;
                case IM_BANDFMT_INT:
                    LoopSignedInt(p, q, ne, ba);
                    break;
                case IM_BANDFMT_FLOAT:
                    LoopFloat(p, q, ne, ba);
                    break;
                case IM_BANDFMT_DOUBLE:
                    LoopDouble(p, q, ne, ba);
                    break;

                default:
                    throw new ArgumentException("Invalid band format");
            }
        }

        return 0;
    }

    private void LoopSignedChar(VipsPel[] p, VipsPel[] q, int ne, int ba)
    {
        for (int i = 0; i < ne; i++)
        {
            signed char p1 = p[i];
            signed char p2 = p[i + ba];

            if (flag == 1 && p1 > 0 && p2 <= 0)
                q[i] = 255;
            else if (flag == -1 && p1 < 0 && p2 >= 0)
                q[i] = 255;
            else
                q[i] = 0;
        }
    }

    private void LoopSignedShort(VipsPel[] p, VipsPel[] q, int ne, int ba)
    {
        for (int i = 0; i < ne; i++)
        {
            signed short p1 = p[i];
            signed short p2 = p[i + ba];

            if (flag == 1 && p1 > 0 && p2 <= 0)
                q[i] = 255;
            else if (flag == -1 && p1 < 0 && p2 >= 0)
                q[i] = 255;
            else
                q[i] = 0;
        }
    }

    private void LoopSignedInt(VipsPel[] p, VipsPel[] q, int ne, int ba)
    {
        for (int i = 0; i < ne; i++)
        {
            signed int p1 = p[i];
            signed int p2 = p[i + ba];

            if (flag == 1 && p1 > 0 && p2 <= 0)
                q[i] = 255;
            else if (flag == -1 && p1 < 0 && p2 >= 0)
                q[i] = 255;
            else
                q[i] = 0;
        }
    }

    private void LoopFloat(VipsPel[] p, VipsPel[] q, int ne, int ba)
    {
        for (int i = 0; i < ne; i++)
        {
            float p1 = p[i];
            float p2 = p[i + ba];

            if (flag == 1 && p1 > 0 && p2 <= 0)
                q[i] = 255;
            else if (flag == -1 && p1 < 0 && p2 >= 0)
                q[i] = 255;
            else
                q[i] = 0;
        }
    }

    private void LoopDouble(VipsPel[] p, VipsPel[] q, int ne, int ba)
    {
        for (int i = 0; i < ne; i++)
        {
            double p1 = p[i];
            double p2 = p[i + ba];

            if (flag == 1 && p1 > 0 && p2 <= 0)
                q[i] = 255;
            else if (flag == -1 && p1 < 0 && p2 >= 0)
                q[i] = 255;
            else
                q[i] = 0;
        }
    }

    public class Zerox : IImageOperation
    {
        public int Sign { get; set; }

        public int Operate(Image inImage, Image outImage)
        {
            if (Sign != -1 && Sign != 1)
            {
                throw new ArgumentException("Flag must be -1 or 1");
            }
            if (inImage.Xsize < 2)
            {
                throw new ArgumentException("Image too narrow");
            }

            Image t1 = im_open_local(outImage, "im_zerox", "p");

            if (!im_piocheck(inImage, t1) ||
                !im_check_uncoded("im_zerox", inImage) ||
                !im_check_noncomplex("im_zerox", inImage))
            {
                return -1;
            }

            if (vips_bandfmt_isuint(inImage.BandFmt))
            {
                // Unsigned type, therefore there will be no zero-crossings.
                return im_black(outImage, inImage.Xsize, inImage.Ysize, inImage.Bands);
            }

            // Force output to be BYTE. Output is narrower than input by 1 pixel.
            if (im_cp_desc(t1, inImage))
            {
                return -1;
            }
            t1.BandFmt = IM_BANDFMT_UCHAR;
            t1.Xsize -= 1;

            // Set hints - THINSTRIP is ok with us.
            if (im_demand_hint(t1, IM_THINSTRIP, null))
            {
                return -1;
            }

            // Generate image.
            if (im_generate(t1, im_start_one, new ZeroxGen(), im_stop_one,
                inImage, Sign))
            {
                return -1;
            }

            // Now embed it in a larger image.
            if (im_embed(t1, outImage, 0, 0, 0, inImage.Xsize, inImage.Ysize))
            {
                return -1;
            }

            return 0;
        }
    }
}
```

Note that I've assumed the existence of certain classes and methods (`Region`, `Image`, `IM_REGION_ADDR`, `im_prepare`, `im_open_local`, `im_piocheck`, `im_check_uncoded`, `im_check_noncomplex`, `im_cp_desc`, `im_demand_hint`, `im_generate`, `im_embed`, etc.) which are not defined in the provided C code. These classes and methods should be implemented separately to complete the conversion.