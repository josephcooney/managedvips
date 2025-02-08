Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

public class StretchInfo
{
    public Image In { get; set; }
    public double Dx { get; set; }
    public double Dy { get; set; }

    public int Xoff { get; set; }
    public int Yoff { get; set; }

    public int[,] Mask = new int[34, 4];
}

public class SeqInfo
{
    public StretchInfo Sin { get; set; }
    public Region Ir { get; set; }
    public ushort[] Buf { get; set; }
    public int Lsk { get; set; }
}

class Program
{
    static void stretch_stop(object vseq, object a, object b)
    {
        SeqInfo seq = (SeqInfo)vseq;

        if (seq.Ir != null)
            seq.Ir.Dispose();

        return 0;
    }

    static object stretch_start(Image outImage, object a, object b)
    {
        Image inImage = (Image)a;
        StretchInfo sin = (StretchInfo)b;
        SeqInfo seq;

        if ((seq = new SeqInfo()) == null)
            return null;

        seq.Sin = sin;
        seq.Ir = Region.Create(inImage);
        seq.Lsk = outImage.Width * 4;
        seq.Buf = new ushort[seq.Lsk];

        if (seq.Buf == null || seq.Ir == null)
        {
            stretch_stop(seq, null, null);
            return null;
        }

        return seq;
    }

    static void make_xline(StretchInfo sin,
        ushort[] p, ushort[] q, int w, int m)
    {
        int bands = inImage.Bands;
        int tot;
        int x, b;

        // Offsets for subsequent pixels.
        int o1 = 1 * bands;
        int o2 = 2 * bands;
        int o3 = 3 * bands;

        for (x = 0; x < w; x++)
        {
            int[] mask = sin.Mask[m, 0];
            ushort[] p1 = p;

            // Loop for this pel.
            for (b = 0; b < bands; b++)
            {
                tot = p1[0] * mask[0] + p1[o1] * mask[1] +
                    p1[o2] * mask[2] + p1[o3] * mask[3];
                tot = Math.Max(0, tot);
                p1++;
                q[x * bands + b] = (ushort)((tot + 16384) >> 15);
            }

            // Move to next mask.
            m++;
            if (m == 34)
                // Back to mask 0, reuse this input pel.
                m = 0;
            else
                // Move to next input pel.
                p = new ushort[p.Length + bands];
        }
    }

    static void make_yline(StretchInfo sin, int lsk, int boff,
        ushort[] p, ushort[] q, int w, int m)
    {
        int bands = inImage.Bands;
        int we = w * bands;
        int[] mask = sin.Mask[m, 0];
        int tot;
        int x;

        // Offsets for subsequent pixels. Down a line each time.
        int o0 = lsk * boff;
        int o1 = lsk * ((boff + 1) % 4);
        int o2 = lsk * ((boff + 2) % 4);
        int o3 = lsk * ((boff + 3) % 4);

        for (x = 0; x < we; x++)
        {
            tot = p[o0] * mask[0] + p[o1] * mask[1] +
                p[o2] * mask[2] + p[o3] * mask[3];
            tot = Math.Max(0, tot);
            p++;
            q[x] = (ushort)((tot + 16384) >> 15);
        }
    }

    static int stretch_gen(Region outRegion, object vseq, object a, object b)
    {
        SeqInfo seq = (SeqInfo)vseq;
        StretchInfo sin = (StretchInfo)b;
        Region ir = seq.Ir;
        Rect r = outRegion.Valid;
        Rect r1;

        // What mask do we start with?
        int xstart = (int)((r.Left + sin.Xoff) % 34);

        // What part of input do we need for this output?
        r1.Left = r.Left - (r.Left + sin.Xoff) / 34;
        r1.Top = r.Top;
        int x = IM_RECT_RIGHT(r);
        x = x - (x + sin.Xoff) / 34 + 3;
        r1.Width = x - r1.Left;
        r1.Height = r.Height + 3;

        if (!ir.Prepare(&r1))
            return -1;

        // Fill the first three lines of the buffer.
        for (int y = 0; y < 3; y++)
        {
            ushort[] p = new ushort[IM_REGION_ADDR(ir, r1.Left, y + r1.Top)];
            ushort[] q = seq.Buf + seq.Lsk * y;

            make_xline(sin, p, q, r.Width, xstart);
        }

        // Loop for subsequent lines: stretch a new line of x pels, and
        // interpolate a line of output from the 3 previous xes plus this new
        // one.
        for (int y = 0; y < r.Height; y++)
        {
            // Next line of fresh input pels.
            ushort[] p = new ushort[IM_REGION_ADDR(ir, r1.Left, y + r1.Top + 3)];

            // Next line we fill in the buffer.
            int boff = (y + 3) % 4;
            ushort[] q = seq.Buf + boff * seq.Lsk;

            // Line we write in output.
            ushort[] q1 = new ushort[IM_REGION_ADDR(outRegion, r.Left, y + r.Top)];

            // Process this new xline.
            make_xline(sin, p, q, r.Width, xstart);

            // Generate new output line.
            make_yline(sin, seq.Lsk, boff,
                seq.Buf, q1, r.Width, sin.Yoff);
        }

        return 0;
    }

    static int im_stretch3(Image inImage, Image outImage, double dx, double dy)
    {
        StretchInfo sin;
        int i;

        // Check our args.
        if (inImage.Coding != IM_CODING_NONE || inImage.BandFmt != IM_BANDFMT_USHORT)
        {
            Console.WriteLine("im_stretch3: not uncoded unsigned short");
            return -1;
        }
        if (dx < 0 || dx >= 1.0 || dy < 0 || dy >= 1.0)
        {
            Console.WriteLine("im_stretch3: displacements out of range [0,1)");
            return -1;
        }

        // Prepare the output image.
        if (!Image.CopyDescription(outImage, inImage))
            return -1;

        outImage.Xsize = (int)(34 * (inImage.Xsize / 33) + inImage.Xsize % 33 - 3);
        outImage.Ysize = inImage.Ysize - 3;

        if (!Image.DemandHint(outImage, IM_FATSTRIP, inImage, null))
            return -1;

        if ((sin = new StretchInfo()) == null)
            return -1;

        // Save parameters.
        sin.In = inImage;
        sin.Dx = dx;
        sin.Dy = dy;

        // Generate masks.
        for (i = 0; i < 34; i++)
        {
            double d = (34.0 - i) / 34.0;

            double y0 = 2.0 * d * d - d - d * d * d;
            double y1 = 1.0 - 2.0 * d * d + d * d * d;
            double y2 = d + d * d - d * d * d;
            double y3 = -d * d + d * d * d;

            sin.Mask[i, 0] = (int)(y0 * 32768);
            sin.Mask[i, 1] = (int)(y1 * 32768);
            sin.Mask[i, 2] = (int)(y2 * 32768);
            sin.Mask[i, 3] = (int)(y3 * 32768);
        }

        // Which mask do we start with to apply these offsets?
        sin.Xoff = (int)((dx * 33.0) + 0.5);
        sin.Yoff = (int)((dy * 33.0) + 0.5);

        if (!Image.Generate(outImage,
            stretch_start, stretch_gen, stretch_stop, inImage, sin))
            return -1;

        return 0;
    }

    static void Main(string[] args)
    {
        // Example usage:
        Image inImage = new Image();
        Image outImage = new Image();

        double dx = 0.03; // 3% displacement
        double dy = 0.02; // 2% displacement

        int result = im_stretch3(inImage, outImage, dx, dy);

        if (result != 0)
            Console.WriteLine("Error: " + result);
    }
}
```

Note that I've assumed the existence of `Image`, `Region`, and other classes/types as they are not provided in your original C code. You may need to modify the code to match your actual implementation.

Also, please note that this is a direct translation from C to C#, without any optimizations or improvements. The performance might be different compared to the original C code.