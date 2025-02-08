```csharp
// vips__clinear(TiePoints *points)

public int VipsCLinear(TiePoints points)
{
    // VipsImage *mat, *matinv;
    double[] g = new double[4];
    double sx1 = 0.0, sx1x1 = 0.0, sy1 = 0.0, sy1y1 = 0.0;
    double sx2x1 = 0.0, sx2y1 = 0.0, sx2 = 0.0, sy2 = 0.0, sy2y1 = 0.0, sy2x1 = 0.0;

    int i, j;
    int elms = points.Nopoints;
    double scale = 0.0;
    double angle = 0.0;
    double xdelta = 0.0;
    double ydelta = 0.0;

    // xref = &points->x_reference[0];
    int[] xref = new int[elms];
    Array.Copy(points.XReference, xref, elms);
    // yref = &points->y_reference[0];
    int[] yref = new int[elms];
    Array.Copy(points.YReference, yref, elms);
    // xsec = &points->x_secondary[0];
    int[] xsec = new int[elms];
    Array.Copy(points.XSecondary, xsec, elms);
    // ysec = &points->y_secondary[0];
    int[] ysec = new int[elms];
    Array.Copy(points.YSecondary, ysec, elms);
    // dx = &points->dx[0];
    double[] dx = new double[elms];
    // dy = &points->dy[0];
    double[] dy = new double[elms];
    // dev = &points->deviation[0];
    double[] dev = new double[elms];

    for (i = 0; i < elms; i++)
    {
        sx1 += xref[i];
        sx1x1 += xref[i] * xref[i];
        sy1 += yref[i];
        sy1y1 += yref[i] * yref[i];
        sx2x1 += xsec[i] * xref[i];
        sx2y1 += xsec[i] * yref[i];
        sy2y1 += ysec[i] * yref[i];
        sy2x1 += ysec[i] * xref[i];
        sx2 += xsec[i];
        sy2 += ysec[i];
    }

    // mat = vips_image_new_matrix(4, 4)
    double[,] mat = new double[4, 4];

    mat[0, 0] = sx1x1 + sy1y1;
    mat[1, 0] = 0;
    mat[2, 0] = sx1;
    mat[3, 0] = sy1;

    mat[0, 1] = 0;
    mat[1, 1] = sx1x1 + sy1y1;
    mat[2, 1] = -sy1;
    mat[3, 1] = sx1;

    mat[0, 2] = sx1;
    mat[1, 2] = -sy1;
    mat[2, 2] = (double)elms;
    mat[3, 2] = 0.0;

    mat[0, 3] = sy1;
    mat[1, 3] = sx1;
    mat[2, 3] = 0.0;
    mat[3, 3] = (double)elms;

    g[0] = sx2x1 + sy2y1;
    g[1] = -sx2y1 + sy2x1;
    g[2] = sx2;
    g[3] = sy2;

    // matinv = vips_matrixinvert(mat, NULL)
    double[,] matinv = new double[4, 4];
    if (!VipsMatrixInvert(mat, ref matinv))
    {
        return -1;
    }

    for (j = 0; j < 4; j++)
    {
        scale += matinv[j, 0] * g[j];
        angle += matinv[j, 1] * g[j];
        xdelta += matinv[j, 2] * g[j];
        ydelta += matinv[j, 3] * g[j];
    }

    // find the deviation of each point for the estimated variables
    // if it greater than 1 then the solution is not good enough
    // but this is handled by the main program
    for (i = 0; i < elms; i++)
    {
        dx[i] = xsec[i] - ((scale * xref[i]) - (angle * yref[i]) + xdelta);
        dy[i] = ysec[i] - ((angle * xref[i]) + (scale * yref[i]) + ydelta);

        double value = Math.Sqrt(dx[i] * dx[i] + dy[i] * dy[i]);
        dev[i] = value;
    }

    points.LScale = scale;
    points.LAngle = angle;
    points.LDeltax = xdelta;
    points.LDeltay = ydelta;

    return 0;
}

// VipsMatrixInvert(double[,] mat, ref double[,] matinv)
public bool VipsMatrixInvert(double[,] mat, ref double[,] matinv)
{
    // implementation of vips_matrixinvert
    // ...
}
```