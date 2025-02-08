```csharp
// im_spatres

public static int ImSpatRes(IMAGE in, IMAGE out, int step)
{
    // Check args
    if (step < 1)
    {
        throw new ArgumentException("Invalid step " + step);
    }

    if ((in.Xsize / step == 0) || (in.Ysize / step == 0))
    {
        throw new ArgumentException("Invalid step " + step);
    }

    if (ImIoCheck(in, out) == -1)
    {
        return -1;
    }

    // Prepare output
    if (ImCpDesc(out, in) == -1)
    {
        return -1;
    }
    out.Xsize = in.Xsize - in.Xsize % step;
    out.Ysize = in.Ysize - in.Ysize % step;

    if (ImSetupOut(out) == -1)
    {
        return -1;
    }

    // Malloc buffer for one 'line' of input data
    int os = in.Xsize * in.Bands;
    byte[] line = new byte[os];
    // Malloc space for values
    byte[] values = new byte[out.Bands];

    if (line == null || values == null)
    {
        throw new OutOfMemoryException("calloc failed");
    }

    int step2 = step * step;
    int rounding = step2 / 2;

    for (int y = 0; y < out.Ysize; y += step)
    {
        byte[] cpinput = in.data;
        cpinput = cpinput.Skip(os * step).ToArray();
        // do the x loop out->Xsize / step times
        byte[] cpline = line;

        for (int x = 0; x < out.Xsize; x += step)
        {
            byte[] cp2input = cpinput;
            cpinput = cpinput.Skip(step * out.Bands).ToArray();
            for (int z = 0; z < out.Bands; z++)
            {
                byte[] pnt = cp2input;
                int sum = 0;

                for (int j = 0; j < step; j++)
                {
                    byte[] cpnt = pnt;
                    pnt = pnt.Skip(os).ToArray();
                    for (int i = 0; i < step; i++)
                    {
                        sum += (int)pnt[i];
                    }
                }

                values[z] = (byte)((sum + rounding) / step2);
            }

            // for this x, write step*bands data
            for (int j = 0; j < step; j++)
                for (int z = 0; z < out.Bands; z++)
                    cpline[j * out.Bands + z] = values[z];
        }

        // line is now ready. Write now step lines
        for (int j = 0; j < step; j++)
            if (ImWriteLine(y + j, out, line) == -1)
            {
                return -1;
            }
    }

    return 0;
}
```