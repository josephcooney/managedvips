```csharp
// vips__tbcalcon

public static int VipsTbCalcon(VipsImage ref, TiePoints points)
{
    // Geometry: border we must leave around each area.
    const int border = points.HalfAreasize;

    // Width of an area.
    const int aWidth = ref.Xsize / AREAS;

    // Number of points we find in each area.
    const int len = points.Nopoints / AREAS;

    int i;
    VipsRect area;

    // Make sure we can read image.
    if (!ref.CanRead())
        return -1;
    if (ref.Bands != 1 || ref.BandFmt != VIPS_FORMAT_UCHAR)
    {
        throw new ArgumentException("vips__tbcalcon", "help!");
        return -1;
    }

    // Define bits to search for high-contrast areas.
    area.Width = aWidth;
    area.Height = ref.Ysize;
    area.Left = 0;
    area.Top = 0;
    VipsRect.MarginAdjust(ref, out area);
    area.Width--;
    area.Height--;
    if (area.Width < 0 || area.Height < 0)
    {
        throw new ArgumentException("vips__tbcalcon", "overlap too small");
        return -1;
    }

    // Loop over areas, finding points.
    for (i = 0; area.Left < ref.Xsize; area.Left += aWidth, i++)
        if (!VipsFindBestContrast(ref,
                area.Left, area.Top, area.Width, area.Height,
                points.XReference + i * len,
                points.YReference + i * len,
                points.Contrast + i * len,
                len,
                points.HalfCorsize))
            return -1;

    return 0;
}
```