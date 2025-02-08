```csharp
// im_simcontr: creates a pattern showing the similtaneous constrast effect

public static int ImSimContr(IMAGE out, int xsize, int ysize)
{
    int x, y;
    byte[] line1 = new byte[xsize];
    byte[] line2 = new byte[xsize];

    // Check input args
    if (ImOutCheck(out))
        return -1;

    // Set now image properly
    ImInitDesc(out, xsize, ysize, 1, IM_BBITS_BYTE, IM_BANDFMT_UCHAR,
        IM_CODING_NONE, IM_TYPE_B_W, 1.0f, 1.0f, 0, 0);

    // Set up image checking whether the output is a buffer or a file
    if (ImSetupOut(out) == -1)
        return -1;

    // Create data
    for (x = 0; x < xsize; x++)
        line1[x] = 255;
    for (x = 0; x < xsize / 2; x++)
        line1[x] = 0;

    for (x = 0; x < xsize; x++)
        line2[x] = 255;
    int cplineIndex = 0;
    for (x = 0; x < xsize / 8; x++)
        line2[cplineIndex++] = 0;
    for (x = 0; x < xsize / 4; x++)
        line2[cplineIndex++] = 128;
    for (x = 0; x < xsize / 8; x++)
        line2[cplineIndex++] = 0;
    for (x = 0; x < xsize / 8; x++)
        line2[cplineIndex++] = 255;
    for (x = 0; x < xsize / 4; x++)
        line2[cplineIndex++] = 128;

    // Write lines
    for (y = 0; y < ysize / 4; y++)
    {
        if (ImWriteLine(y, out, line1) == -1)
        {
            return -1;
        }
    }
    for (y = ysize / 4; y < (ysize / 4 + ysize / 2); y++)
    {
        if (ImWriteLine(y, out, line2) == -1)
        {
            return -1;
        }
    }
    for (y = (ysize / 4 + ysize / 2); y < ysize; y++)
    {
        if (ImWriteLine(y, out, line1) == -1)
        {
            return -1;
        }
    }

    return 0;
}
```