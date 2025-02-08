```csharp
// im_fav4: Average four identical images.
public static int im_fav4(IMAGE[] in, IMAGE out)
{
    // check IMAGEs parameters
    if (im_iocheck(in[1], out))
        return -1;

    // BYTE images only!
    if ((in[0].BandFmt != IM_BANDFMT_CHAR) &&
        (in[0].BandFmt != IM_BANDFMT_UCHAR))
        return -1;

    if (im_cp_desc(out, in[1]) == -1) /* copy image descriptors */
        return -1;
    if (im_setupout(out) == -1)
        return -1;

    int linebytes = in[0].Xsize * in[0].Bands;
    int PICY = in[0].Ysize;
    byte[] buffer = new byte[linebytes];

    // average 4 pels with rounding, for whole line
    for (int y = 0; y < PICY; y++)
    {
        int result = 0;
        for (int x = 0; x < linebytes; x++)
        {
            result += (in[0].data[x] + in[1].data[x] + in[2].data[x] + in[3].data[x]) >> 2;
        }
        im_writeline(y, out, buffer);
    }

    return 0;
}
```