```csharp
// vips__improve (from vips__improve.c)

public static int VipsImprove(TiePoints inpoints, ref TiePoints outpoints)
{
    TiePoints points1 = new TiePoints();
    TiePoints points2 = new TiePoints();
    TiePoints p = points1;
    TiePoints q = points2;

    // p has the current state - make a new state, q, with only those
    // points which have a small deviation.
    while (Copypoints(ref q, ref p) && Copydevpoints(ref q, ref p))
    {
        // If there are only a few left, jump out.
        if (q.Nopoints < 2)
            break;

        // Fit the model to the new set of points.
        if (!VipsClinear(q))
            return -1;

        // And loop.
        Swap(ref p, ref q);
    }

    // q has the output - copy to outpoints.
    Copypoints(ref outpoints, ref q);

    return 0;
}

// copypoints (from vips__improve.c)

public static void Copypoints(ref TiePoints newpoints, ref TiePoints oldpoints)
{
    newpoints.Reference = oldpoints.Reference;
    newpoints.Secondary = oldpoints.Secondary;

    newpoints.Deltax = oldpoints.Deltax;
    newpoints.Deltay = oldpoints.Deltay;
    newpoints.Nopoints = oldpoints.Nopoints;
    newpoints.Halfcorsize = oldpoints.Halfcorsize;
    newpoints.Halfareasize = oldpoints.Halfareasize;

    for (int i = 0; i < oldpoints.Nopoints; i++)
    {
        newpoints.X_Reference[i] = oldpoints.X_Reference[i];
        newpoints.Y_Reference[i] = oldpoints.Y_Reference[i];
        newpoints.X_Secondary[i] = oldpoints.X_Secondary[i];
        newpoints.Y_Secondary[i] = oldpoints.Y_Secondary[i];
        newpoints.Contrast[i] = oldpoints.Contrast[i];
        newpoints.Correlation[i] = oldpoints.Correlation[i];
        newpoints.Deviation[i] = oldpoints.Deviation[i];
        newpoints.Dx[i] = oldpoints.Dx[i];
        newpoints.Dy[i] = oldpoints.Dy[i];
    }

    newpoints.L_Scale = oldpoints.L_Scale;
    newpoints.L_Angle = oldpoints.L_Angle;
    newpoints.L_Deltax = oldpoints.L_Deltax;
    newpoints.L_Deltay = oldpoints.L_Deltay;
}

// copydevpoints (from vips__improve.c)

public static int Copydevpoints(ref TiePoints newpoints, ref TiePoints oldpoints)
{
    int i;
    int j;
    double thresh_dev, max_dev, min_dev;
    double[] corr;

    min_dev = 9999.0;
    max_dev = 0.0;
    corr = new double[oldpoints.Nopoints];

    for (i = 0; i < oldpoints.Nopoints; i++)
        if (corr[i] > 0.01)
            if (oldpoints.Deviation[i] / corr[i] < min_dev)
                min_dev = oldpoints.Deviation[i] / corr[i];
            if (oldpoints.Deviation[i] / corr[i] > max_dev)
                max_dev = oldpoints.Deviation[i] / corr[i];

    thresh_dev = min_dev + (max_dev - min_dev) * 0.3;
    if (thresh_dev <= 1.0)
        thresh_dev = 1.0;

    for (i = 0, j = 0; i < oldpoints.Nopoints; i++)
        if (oldpoints.Correlation[i] > 0.01)
            if (oldpoints.Deviation[i] / corr[i] <= thresh_dev)
            {
                newpoints.X_Reference[j] = oldpoints.X_Reference[i];
                newpoints.Y_Reference[j] = oldpoints.Y_Reference[i];
                newpoints.X_Secondary[j] = oldpoints.X_Secondary[i];
                newpoints.Y_Secondary[j] = oldpoints.Y_Secondary[i];
                newpoints.Contrast[j] = oldpoints.Contrast[i];
                newpoints.Correlation[j] = oldpoints.Correlation[i];
                newpoints.Deviation[j] = oldpoints.Deviation[i];
                newpoints.Dx[j] = oldpoints.Dx[i];
                newpoints.Dy[j] = oldpoints.Dy[i];
                j++;
            }
    newpoints.Nopoints = j;

    for (i = j; i < VIPS_MAXPOINTS; i++)
    {
        newpoints.X_Reference[i] = 0;
        newpoints.Y_Reference[i] = 0;
        newpoints.X_Secondary[i] = 0;
        newpoints.Y_Secondary[i] = 0;
        newpoints.Contrast[i] = 0;
        newpoints.Correlation[i] = 0.0;
        newpoints.Deviation[i] = 0.0;
        newpoints.Dx[i] = 0.0;
        newpoints.Dy[i] = 0.0;
    }

    // Return non-zero if we changed something.
    if (j != oldpoints.Nopoints)
        return -1;

    return 0;
}

// VipsClinear (from vips__improve.c)

public static bool VipsClinear(TiePoints points)
{
    // implementation of VipsClinear
}
```