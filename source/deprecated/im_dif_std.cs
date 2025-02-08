```csharp
// im__mean_std_int_buffer

public static int ImMeanStdIntBuffer(int[] buffer, int size, out double mean, out double std)
{
    if (size <= 0)
    {
        throw new ArgumentException("wrong args");
    }

    double sum = 0;
    double sum2 = 0;

    for (int i = 0; i < size; i++)
    {
        sum += buffer[i];
        sum2 += buffer[i] * buffer[i];
    }

    double meanValue = sum / size;
    double variance = (sum2 - (meanValue * meanValue) * size) / size;
    double stdDev = Math.Sqrt(variance);

    mean = meanValue;
    std = stdDev;

    return 0;
}

// im_dif_std

public static int ImDifStd(IMAGE image, int xpos, int ypos, int xsize, int ysize, int dx, int dy, out double mean, out double std)
{
    if (!image.InCheck())
    {
        return -1;
    }

    if (image.Bands != 1 || image.BandFmt != IM_BANDFMT_UCHAR)
    {
        throw new ArgumentException("Unable to accept input");
    }
    if ((xpos + xsize + dx > image.Xsize) || (ypos + ysize + dy > image.Ysize))
    {
        throw new ArgumentException("wrong args");
    }

    int bufsize = xsize * ysize;
    int[] buf = new int[bufsize];

    PEL input = (PEL)image.Data;
    input += (ypos * image.Xsize + xpos);
    int ofst = dy * image.Xsize + dx;

    for (int y = 0; y < ysize; y++)
    {
        PEL cpinput = input;
        input += image.Xsize;
        for (int x = 0; x < xsize; x++)
        {
            buf[y * xsize + x] = ((int)cpinput[0]) - ((int)(cpinput[ofst]));
            cpinput++;
        }
    }

    double m, s;

    if (ImMeanStdIntBuffer(buf, bufsize, out m, out s))
    {
        return -1;
    }

    mean = m;
    std = s;

    return 0;
}
```

Note: The `IMAGE` class and the `PEL` struct are assumed to be defined elsewhere in your codebase. Also, I've used C#-style naming conventions for the methods and variables.