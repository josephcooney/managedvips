```csharp
// vips__avgdxdy

public static int AvgDxDy(TiePoints points, ref int dx, ref int dy)
{
    // Check if there are any points to average.
    if (points.Nopoints == 0)
    {
        throw new ArgumentException("No points to average");
    }

    // Lots of points.
    int sumdx = 0;
    int sumdy = 0;

    for (int i = 0; i < points.Nopoints; i++)
    {
        sumdx += points.XSecondary[i] - points.XReference[i];
        sumdy += points.YSecondary[i] - points.YReference[i];
    }

    dx = (int)Math.Round((double)sumdx / (double)points.Nopoints);
    dy = (int)Math.Round((double)sumdy / (double)points.Nopoints);

    return 0;
}
```