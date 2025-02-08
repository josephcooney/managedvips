Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class BandJoin : Bandary
{
    public override void Buffer(BandarySequence sequence, Pel[] q, ref Pel[] p, int width)
    {
        Bandary bandary = (Bandary)sequence.Bandary;
        Conversion conversion = (Conversion)bandary;
        Image[] inImages = bandary.Ready;

        // Output pel size.
        int ops = VipsImage.SizeOfPel(conversion.Out);

        for (int i = 0; i < bandary.N; i++)
        {
            // Input pel size.
            int ips = VipsImage.SizeOfPel(inImages[i]);

            Pel[] p1;
            Pel[] q1;
            int x, z;

            q1 = q;
            p1 = p[i];

            if (ips == 1)
            {
                for (x = 0; x < width; x++)
                {
                    q1[0] = p1[x];
                    q1 += ops;
                }
                q += ips;
            }
            else if (ips == 3)
            {
                for (x = 0; x < width; x++)
                {
                    q1[0] = p1[0];
                    q1[1] = p1[1];
                    q1[2] = p1[2];

                    p1 += ips;
                    q1 += ops;
                }
                q += ips;
            }
            else
            {
                for (x = 0; x < width; x++)
                {
                    for (z = 0; z < ips; z++)
                        q1[z] = p1[z];

                    p1 += ips;
                    q1 += ops;
                }
                q += ips;
            }
        }
    }

    public override int Build(VipsObject obj)
    {
        Bandary bandary = (Bandary)obj;
        BandJoin bandjoin = (BandJoin)obj;

        if (bandjoin.In != null)
        {
            bandary.In = VipsArrayImage.Get(bandjoin.In, ref bandary.N);

            if (bandary.N == 1)
                return VipsBandary.Copy(bandary);
            else
            {
                int i;
                bandary.OutBands = 0;

                for (i = 0; i < bandary.N; i++)
                    if (bandary.In[i] != null)
                        bandary.OutBands += bandary.In[i].Bands;
            }
        }

        return base.Build(obj);
    }

    public BandJoin()
    {
        // Init our instance fields.
    }

    public static int BandJoinV(Image[] inImages, ref Image outImage, int n, params object[] args)
    {
        VipsArrayImage array = new VipsArrayImage(inImages, n);

        return VipsCallSplit("bandjoin", args, array, ref outImage);
    }

    public static int BandJoin2(Image in1, Image in2, ref Image outImage, params object[] args)
    {
        Image[] inImages = new Image[2];
        inImages[0] = in1;
        inImages[1] = in2;

        return VipsCallSplit("bandjoin", args, inImages, ref outImage);
    }

    public static int BandJoinConst(Image inImage, ref Image outImage, double[] c, int n, params object[] args)
    {
        VipsArrayDouble array = new VipsArrayDouble(c, n);

        return VipsCallSplit("bandjoin_const", args, inImage, ref outImage, array);
    }

    public static int BandJoinConst1(Image inImage, ref Image outImage, double c, params object[] args)
    {
        double[] cArray = new double[1];
        cArray[0] = c;

        return VipsCallSplit("bandjoin_const", args, inImage, ref outImage, cArray);
    }
}

public class BandJoinConst : Bandary
{
    public override void Buffer(BandarySequence sequence, Pel[] q, ref Pel[] p, int width)
    {
        Bandary bandary = (Bandary)sequence.Bandary;
        Conversion conversion = (Conversion)bandary;
        Image inImage = bandary.Ready[0];

        // Output pel size.
        int ops = VipsImage.SizeOfPel(conversion.Out);

        // Input pel size.
        int ips = VipsImage.SizeOfPel(inImage);

        // Extra bands size.
        int ebs = ops - ips;

        Pel[] p1;
        Pel[] q1;
        int x, z;

        q1 = q;
        p1 = p[0];

        if (ips == 3 && ebs == 1)
        {
            double c = bandjoin.CReady[0];

            for (x = 0; x < width; x++)
            {
                q1[0] = p1[0];
                q1[1] = p1[1];
                q1[2] = p1[2];
                q1[3] = c;

                p1 += 3;
                q1 += 4;
            }
        }
        else
        {
            for (x = 0; x < width; x++)
            {
                for (z = 0; z < ips; z++)
                    q1[z] = p1[z];

                p1 += ips;
                q1 += ips;

                for (z = 0; z < ebs; z++)
                    q1[z] = bandjoin.CReady[z];

                q1 += ebs;
            }
        }
    }

    public override int Build(VipsObject obj)
    {
        Bandary bandary = (Bandary)obj;
        BandJoinConst bandjoin = (BandJoinConst)obj;

        if (bandjoin.In != null && bandjoin.C != null)
        {
            double[] c;
            int n;

            c = VipsArrayDouble.Get(bandjoin.C, ref n);

            if (n == 0)
                return VipsBandary.Copy(bandary);
            else
                bandary.OutBands = inImage.Bands + n;

            bandary.N = 1;
            bandary.In = new Image[] { bandjoin.In };

            if (!(bandjoin.CReady = Vips__VectorToPels("bandjoin_const", n, inImage.BandFmt, inImage.Coding, c, null, n)))
                return -1;
        }

        return base.Build(obj);
    }

    public BandJoinConst()
    {
        // Init our instance fields.
    }
}
```