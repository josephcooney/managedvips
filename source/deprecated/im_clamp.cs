```csharp
// @(#) Function to perform black level correction given black image
// @(#) designed for PAD camera single field black to apply in blocks
// @(#) as each is reused for higher resolution pels (eg: 6 8 for Progres)
// @(#) IM_BANDFMT_UCHAR images only. Always writes UCHAR.
// @(#) int im_clamp(in, w, out, hstep, vstep)

public static int ImClamp(VipsImage in, VipsImage out, VipsImage black, int hstep, int vstep)
{
    // Compute clip(image - (black)) ie subtract black no negatives
    // scales for low res Progres images to replicate black value

    if (!ImIocheck(in, out))
        return -1;

    if ((in.BitsPerSample != 8) ||
        (in.Coding != VipsCoding.None || in.Format != VipsFormat.UChar))
    {
        ImError("im_clamp", "%s", "bad input format");
        return -1;
    }

    if ((black.BitsPerSample != 8) ||
        (black.Coding != VipsCoding.None || black.Format != VipsFormat.UChar))
    {
        ImError("im_clamp", "%s", "bad black format");
        return -1;
    }

    // Set up the output header.
    if (!ImCpDesc(out, in))
        return -1;

    if (!ImSetupout(out))
        return -1;

    // Make buffer for expanded black line
    var bline = new byte[out.Bands * hstep * in.Width];

    // Make buffer we write to.
    var outbuf = new byte[out.Bands * out.Width];

    int blacky = -1;
    var p = (byte[])in.Data;

    for (int y = 0; y < in.Height; y++)
    {
        // calc corresponding black line - get new one if different
        int newblacky = (vstep * black.Height - in.Height + y) / vstep;
        if (newblacky != blacky)
        {
            blacky = newblacky;

            // time to expand a new black line
            var blk = (byte[])black.Data +
                black.Width * black.Bands * blacky;

            for (int bexp = 0; bexp < bline.Length; bexp += in.Bands)
            {
                int rep = 0;
                while (rep < hstep && bexp + in.Bands <= bline.Length)
                {
                    Array.Copy(blk, 0, bline, bexp, in.Bands);
                    blk = new byte[in.Bands];
                    rep++;
                }
            }
        }

        // correct a line of image
        var q = outbuf;
        for (int x = 0; x < outbuf.Length; x++)
        {
            int temp = ((int)p[x] - bline[x]);
            if (temp < 0)
                temp = 0;

            q[x] = (byte)temp;
        }

        p = (byte[])in.Data;
        if (!ImWriteLine(y, out, outbuf))
            return -1;
    } // end of a line

    return 0;
}
```