```csharp
// im_line() - draw a line on an image

public static int ImLine(IMAGE image, int x1, int y1, int x2, int y2, int pelval)
{
    // Check if the image is read/write enabled
    if (!image.IsReadWriteEnabled())
        return -1;

    // Check coordinates
    if ((x1 > image.Xsize) || (x1 < 0) ||
        (y1 > image.Ysize) || (y1 < 0) ||
        (x2 > image.Xsize) || (x2 < 0) ||
        (y2 > image.Ysize) || (y2 < 0))
    {
        // Error message
        throw new ArgumentException("Invalid line coordinates");
    }

    if ((pelval > 255) || (pelval < 0))
    {
        // Error message
        throw new ArgumentException("Line intensity between 0 and 255");
    }

    // Check number of bands in the image
    if (image.Bands != 1)
    {
        // Error message
        throw new ArgumentException("Image should have one band only");
    }

    double dx = x2 - x1;
    double dy = y2 - y1;

    double signx, signy;

    if (dx < 0.0)
        signx = -1.0;
    else
        signx = 1.0;

    if (dy < 0.0)
        signy = -1.0;
    else
        signy = 1.0;

    // Handle vertical line
    if (dx == 0.0)
    {
        double x = x1;
        double y = y1;

        while (y != y2)
        {
            int offset = (int)(x + .5) + ((int)(y + .5)) * image.Xsize;
            image.Data[offset] = (byte)pelval;
            y += signy;
        }

        // Draw point (x2, y2)
        int offset = x2 + y2 * image.Xsize;
        image.Data[offset] = (byte)pelval;

        return 0;
    }

    // Handle horizontal line
    if (dy == 0.0)
    {
        double x = x1;
        double y = y1;

        while (x != x2)
        {
            int offset = (int)(x + .5) + ((int)(y + .5)) * image.Xsize;
            image.Data[offset] = (byte)pelval;
            x += signx;
        }

        // Draw point (x2, y2)
        int offset = x2 + y2 * image.Xsize;
        image.Data[offset] = (byte)pelval;

        return 0;
    }

    // Handle diagonal line
    if (Math.Abs(dy) < Math.Abs(dx))
    {
        double m = Math.Abs(dy / dx) * signy;
        double x = x1;
        double y = y1;

        while (x != x2)
        {
            int offset = (int)(x + .5) + ((int)(y + .5)) * image.Xsize;
            image.Data[offset] = (byte)pelval;
            x += signx;
            y += m;
        }
    }
    else
    {
        double m = Math.Abs(dx / dy) * signx;
        double x = x1;
        double y = y1;

        while (y != y2)
        {
            int offset = (int)(x + .5) + ((int)(y + .5)) * image.Xsize;
            image.Data[offset] = (byte)pelval;
            x += m;
            y += signy;
        }
    }

    // Draw point (x2, y2)
    int offset = x2 + y2 * image.Xsize;
    image.Data[offset] = (byte)pelval;

    // Invalidate the image
    image.Invalidate();

    return 0;
}
```