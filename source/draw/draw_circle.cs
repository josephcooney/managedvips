```csharp
// vips__draw_circle_direct
public static void DrawCircleDirect(VipsImage image, int cx, int cy, int r,
    VipsDrawScanline drawScanline, object client)
{
    int x, y, d;

    y = r;
    d = 3 - 2 * r;

    for (x = 0; x < y; x++)
    {
        drawScanline(image, cy + y, cx - x, cx + x, 0, client);
        drawScanline(image, cy - y, cx - x, cx + x, 1, client);
        drawScanline(image, cy + x, cx - y, cx + y, 2, client);
        drawScanline(image, cy - x, cx - y, cx + y, 3, client);

        if (d < 0)
            d += 4 * x + 6;
        else
        {
            d += 4 * (x - y) + 10;
            y--;
        }
    }

    if (x == y)
    {
        drawScanline(image, cy + y, cx - x, cx + x, 0, client);
        drawScanline(image, cy - y, cx - x, cx + x, 1, client);
        drawScanline(image, cy + x, cx - y, cx + y, 2, client);
        drawScanline(image, cy - x, cx - y, cx + y, 3, client);
    }
}

// vips_draw_circle_draw_point
private static void DrawCircleDrawPoint(VipsImage image, int x, int y, object client)
{
    VipsPel ink = (VipsPel)client;
    VipsPel[] q = new VipsPel[VIPS_IMAGE_SIZEOF_PEL(image)];
    Array.Copy(ink, q, VIPS_IMAGE_SIZEOF_PEL(image));

    VipsImageAddr addr = VIPS_IMAGE_ADDR(image, x, y);
    for (int j = 0; j < VIPS_IMAGE_SIZEOF_PEL(image); j++)
        addr[j] = q[j];
}

// vips_draw_circle_draw_endpoints_clip
private static void DrawCircleDrawEndpointsClip(VipsImage image,
    int y, int x1, int x2, int quadrant, object client)
{
    if (y >= 0 && y < image.Ysize)
    {
        if (x1 >= 0 && x1 < image.Xsize)
            DrawCircleDrawPoint(image, x1, y, client);
        if (x2 >= 0 && x2 < image.Xsize)
            DrawCircleDrawPoint(image, x2, y, client);
    }
}

// vips_draw_circle_draw_endpoints_noclip
private static void DrawCircleDrawEndpointsNoclip(VipsImage image,
    int y, int x1, int x2, int quadrant, object client)
{
    DrawCircleDrawPoint(image, x1, y, client);
    DrawCircleDrawPoint(image, x2, y, client);
}

// vips_draw_circle_draw_scanline
private static void DrawCircleDrawScanline(VipsImage image,
    int y, int x1, int x2, int quadrant, object client)
{
    VipsPel ink = (VipsPel)client;
    VipsPel[] q = new VipsPel[VIPS_IMAGE_SIZEOF_PEL(image)];
    Array.Copy(ink, q, VIPS_IMAGE_SIZEOF_PEL(image));

    VipsImageAddr addr = VIPS_IMAGE_ADDR(image, x1, y);
    int len = x2 - x1 + 1;
    for (int i = 0; i < len; i++)
    {
        Array.Copy(q, 0, addr, 0, VIPS_IMAGE_SIZEOF_PEL(image));
        addr += VIPS_IMAGE_SIZEOF_PEL(image);
    }
}

// vips_draw_circle_build
public static int DrawCircleBuild(VipsObject object)
{
    VipsDraw draw = (VipsDraw)object;
    VipsDrawink drawink = (VipsDrawink)object;
    VipsDrawCircle circle = (VipsDrawCircle)object;

    VipsDrawScanline drawScanline;

    if (((VipsObjectClass)vips_draw_circle_parent_class).build(object) != 0)
        return -1;

    if (circle.fill)
        drawScanline = DrawCircleDrawScanline;
    else if (circle.cx - circle.radius >= 0 &&
             circle.cx + circle.radius < draw.image.Xsize &&
             circle.cy - circle.radius >= 0 &&
             circle.cy + circle.radius < draw.image.Ysize)
        drawScanline = DrawCircleDrawEndpointsNoclip;
    else
        drawScanline = DrawCircleDrawEndpointsClip;

    Vips__draw_circle_direct(draw.image,
        circle.cx, circle.cy, circle.radius,
        drawScanline, drawink.pixel_ink);

    return 0;
}

// vips_draw_circlev
public static int DrawCirclev(VipsImage image,
    double[] ink, int n, int cx, int cy, int radius, params object[] args)
{
    VipsArea area_ink = new VipsArea(vips_array_double_new(ink, n));
    int result;

    result = vips_call_split("draw_circle", args,
        image, area_ink, cx, cy, radius);
    vips_area_unref(area_ink);

    return result;
}

// vips_draw_circle
public static int DrawCircle(VipsImage image,
    double[] ink, int n, int cx, int cy, int radius, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, radius);
    result = DrawCirclev(image, ink, n, cx, cy, radius, args);
    va_end(ap);

    return result;
}

// vips_draw_circle1
public static int DrawCircle1(VipsImage image,
    double ink, int cx, int cy, int radius, params object[] args)
{
    double[] array_ink = new double[1];
    array_ink[0] = ink;

    va_list ap;
    int result;

    va_start(ap, radius);
    result = DrawCirclev(image, array_ink, 1, cx, cy, radius, args);
    va_end(ap);

    return result;
}
```