```csharp
// vips__draw_line_direct
public static void DrawLineDirect(VipsImage image, int x1, int y1, int x2, int y2, VipsDrawPoint draw_point, object client)
{
    int dx = x2 - x1;
    int dy = y2 - y1;

    // Swap endpoints to reduce number of cases.
    if (Math.Abs(dx) >= Math.Abs(dy) && dx < 0)
    {
        // Swap to get all x greater or equal cases going to the
        // right. Do diagonals here .. just have up and right and down
        // and right now.
        int temp = x1;
        x1 = x2;
        x2 = temp;

        temp = y1;
        y1 = y2;
        y2 = temp;
    }
    else if (Math.Abs(dx) < Math.Abs(dy) && dy < 0)
    {
        // Swap to get all y greater cases going down the screen.
        int temp = x1;
        x1 = x2;
        x2 = temp;

        temp = y1;
        y1 = y2;
        y2 = temp;
    }

    dx = x2 - x1;
    dy = y2 - y1;

    int x = x1;
    int y = y1;

    // Special case: zero width and height is single point.
    if (dx == 0 && dy == 0)
        draw_point(image, x, y, client);
    // Special case vertical and horizontal lines for speed.
    else if (dx == 0)
    {
        // Vertical line going down.
        for (; y <= y2; y++)
            draw_point(image, x, y, client);
    }
    else if (dy == 0)
    {
        // Horizontal line to the right.
        for (; x <= x2; x++)
            draw_point(image, x, y, client);
    }
    // Special case diagonal lines.
    else if (Math.Abs(dy) == Math.Abs(dx) && dy > 0)
    {
        // Diagonal line going down and right.
        for (; x <= x2; x++, y++)
            draw_point(image, x, y, client);
    }
    else if (Math.Abs(dy) == Math.Abs(dx) && dy < 0)
    {
        // Diagonal line going up and right.
        for (; x <= x2; x++, y--)
            draw_point(image, x, y, client);
    }
    else if (Math.Abs(dy) < Math.Abs(dx) && dy > 0)
    {
        // Between -45 and 0 degrees.
        int err = 0;
        for (; x <= x2; x++)
        {
            draw_point(image, x, y, client);

            err += dy;
            if (err >= dx)
            {
                err -= dx;
                y++;
            }
        }
    }
    else if (Math.Abs(dy) < Math.Abs(dx) && dy < 0)
    {
        // Between 0 and 45 degrees.
        int err = 0;
        for (; x <= x2; x++)
        {
            draw_point(image, x, y, client);

            err -= dy;
            if (err >= dx)
            {
                err -= dx;
                y--;
            }
        }
    }
    else if (Math.Abs(dy) > Math.Abs(dx) && dx > 0)
    {
        // Between -45 and -90 degrees.
        int err = 0;
        for (; y <= y2; y++)
        {
            draw_point(image, x, y, client);

            err += dx;
            if (err >= dy)
            {
                err -= dy;
                x++;
            }
        }
    }
    else if (Math.Abs(dy) > Math.Abs(dx) && dx < 0)
    {
        // Between -90 and -135 degrees.
        int err = 0;
        for (; y <= y2; y++)
        {
            draw_point(image, x, y, client);

            err -= dx;
            if (err >= dy)
            {
                err -= dy;
                x--;
            }
        }
    }
    else
        throw new Exception("Invalid line direction");
}

// vips_draw_line_draw_point_noclip
public static void DrawPointNoClip(VipsImage image, int x, int y, object client)
{
    VipsPel ink = (VipsPel)client;
    VipsPel[] q = image.GetPixel(x, y);
    int psize = image.GetSizeOfPel();

    for (int j = 0; j < psize; j++)
        q[j] = ink[j];
}

// vips_draw_line_draw_point_clip
public static void DrawPointClip(VipsImage image, int x, int y, object client)
{
    if (x >= 0 && x < image.Xsize && y >= 0 && y < image.Ysize)
        DrawPointNoClip(image, x, y, client);
}

// vips_draw_line_build
public static int Build(VipsObject obj)
{
    VipsDraw draw = (VipsDraw)obj;
    VipsDrawink drawink = (VipsDrawink)obj;
    VipsDrawLine line = (VipsDrawLine)obj;

    VipsDrawPoint draw_point;

    if (line.x1 < draw.image.Xsize && line.x1 >= 0 &&
        line.x2 < draw.image.Xsize && line.x2 >= 0 &&
        line.y1 < draw.image.Ysize && line.y1 >= 0 &&
        line.y2 < draw.image.Ysize && line.y2 >= 0)
        draw_point = DrawPointNoClip;
    else
        draw_point = DrawPointClip;

    DrawLineDirect(draw.image, line.x1, line.y1, line.x2, line.y2,
                   draw_point, drawink.pixel_ink);

    return 0;
}

// vips_draw_line_class_init
public static void ClassInit(Type type)
{
    // ...
}

// vips_draw_line_init
public static void Init(VipsDrawLine obj)
{
    // ...
}

// vips_draw_linev
public static int DrawLineV(VipsImage image, double[] ink, int n, int x1, int y1, int x2, int y2, params object[] args)
{
    VipsArea area_ink = new VipsArrayDouble(ink);
    int result;

    result = CallSplit("draw_line", args,
                       image, area_ink, x1, y1, x2, y2);

    area_ink.Dispose();

    return result;
}

// vips_draw_line
public static int DrawLine(VipsImage image, double[] ink, int n, int x1, int y1, int x2, int y2, params object[] args)
{
    return DrawLineV(image, ink, n, x1, y1, x2, y2, args);
}

// vips_draw_line1
public static int DrawLine1(VipsImage image, double ink, int x1, int y1, int x2, int y2, params object[] args)
{
    double[] array_ink = new double[1];
    array_ink[0] = ink;

    return DrawLineV(image, array_ink, 1, x1, y1, x2, y2, args);
}
```