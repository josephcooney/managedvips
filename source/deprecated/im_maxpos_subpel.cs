```csharp
// im_maxpos_subpel
public static int ImMaxPosSubPel(IMAGE in, out double x, out double y)
{
    // Define the function name for logging purposes
    string functionName = "im_maxpos_subpel";

    // Allocate arrays to store the best matches and their values
    int[] xs = new int[5];
    int[] ys = new int[5];
    double[] vals = new double[5];

    // Get the best matches and their values
    if (ImMaxPosVec(in, xs, ys, vals, 5) != 0)
        return -1;

    // Define a macro to handle wrap-around cases
    void WrapTestReturn()
    {
        // Check for wrap-around in x dimension
        if (!xs[0] && in.Xsize - 1 == xs[3])
            xs[0] = in.Xsize;
        else if (!xs[3] && in.Xsize - 1 == xs[0])
            xs[3] = in.Xsize;

        // Check for wrap-around in y dimension
        if (!ys[0] && in.Ysize - 1 == ys[3])
            ys[0] = in.Ysize;
        else if (!ys[3] && in.Ysize - 1 == ys[0])
            ys[3] = in.Ysize;

        // Check for a subpixel match
        if (Math.Abs(xs[3] - xs[0]) == 1 && Math.Abs(ys[3] - ys[0]) == 1)
        {
            x = ((double)xs[0]) + ((double)(xs[3] - xs[0])) * (vals[3] / (vals[0] + vals[3]));
            y = ((double)ys[0]) + ((double)(ys[3] - ys[0])) * (vals[4] / (vals[0] + vals[4]));
            return;
        }
    }

    // Check for a subpixel match with the best three matches
    if (xs[0] == xs[1] && ys[0] == ys[2])
    {
        xs[0] = xs[0];
        ys[0] = ys[0];
        xs[3] = xs[1];
        ys[3] = ys[2];
        vals[0] = vals[0];
        vals[3] = vals[1];
        WrapTestReturn();
    }
    else if (xs[0] == xs[2] && ys[0] == ys[1])
    {
        xs[0] = xs[0];
        ys[0] = ys[0];
        xs[3] = xs[2];
        ys[3] = ys[1];
        vals[0] = vals[0];
        vals[3] = vals[2];
        WrapTestReturn();
    }

    // Check for a subpixel match with the best four matches
    if (MOST_OF(vals[1], vals[0]) && MOST_OF(vals[2], vals[0]) &&
        MOST_OF(vals[3], vals[0]) && LITTLE_OF(vals[4], vals[0]))
    {
        // Define another macro to handle this case
        void Test4(int a, int b, int c, int d, int e, int f, int g, int h)
        {
            if (xs[a] == xs[b] && xs[c] == xs[d] &&
                ys[e] == ys[f] && ys[g] == ys[h])
            {
                xs[0] = xs[a];
                xs[3] = xs[c];
                ys[0] = ys[e];
                ys[3] = ys[g];
                vals[0] = vals[a] + vals[b];
                vals[3] = vals[c] + vals[d];
                WrapTestReturn();
            }
        }

        // Check for a subpixel match with the best four matches
        Test4(0, 3, 1, 2, 0, 1, 2, 3);
        Test4(0, 1, 2, 3, 0, 3, 1, 2);
        Test4(0, 3, 1, 2, 0, 2, 1, 3);
        Test4(0, 2, 1, 3, 0, 3, 1, 2);
    }

    // If no subpixel match is found, return the best integer alignment
    x = (double)xs[0];
    y = (double)ys[0];
    return 0;
}
```