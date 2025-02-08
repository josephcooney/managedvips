Here is the C# code equivalent to the provided C code:
```
// vips_draw_rect_build
public int Build(VipsObject obj)
{
    VipsDraw draw = (VipsDraw)obj;
    VipsDrawink drawink = (VipsDrawink)obj;
    VipsArea ink = (VipsArea)drawink.Ink;
    VipsDrawRect draw_rect = (VipsDrawRect)obj;

    int left = draw_rect.Left;
    int top = draw_rect.Top;
    int width = draw_rect.Width;
    int height = draw_rect.Height;

    if (base.Build(obj))
        return -1;

    // Also use a solid fill for very narrow unfilled rects.
    if (!draw_rect.Fill &&
        width > 2 &&
        height > 2)
        return Draw(draw.Image,
            ink.Data, ink.N,
            left, top, width, 1, null) ||
            Draw(draw.Image,
                ink.Data, ink.N,
                left + width - 1, top, 1, height, null) ||
            Draw(draw.Image,
                ink.Data, ink.N,
                left, top + height - 1, width, 1, null) ||
            Draw(draw.Image,
                ink.Data, ink.N,
                left, top, 1, height, null);

    VipsRect image = new VipsRect();
    image.Left = 0;
    image.Top = 0;
    image.Width = draw.Image.Xsize;
    image.Height = draw.Image.Ysize;

    VipsRect rect = new VipsRect();
    rect.Left = left;
    rect.Top = top;
    rect.Width = width;
    rect.Height = height;

    VipsRect clip = new VipsRect();
    vips_rect_intersectrect(ref rect, ref image, ref clip);

    if (!vips_rect_isempty(ref clip))
    {
        VipsPel[] to = new VipsPel[clip.Width * draw.Psize];
        VipsPel q;
        int x;

        // We plot the first line pointwise, then memcpy() it for the
        // subsequent lines.

        q = to;
        for (x = 0; x < clip.Width; x++)
            vips__drawink_pel(drawink, ref q);
        q += draw.Psize;

        VipsPel[] src = new VipsPel[clip.Width * draw.Psize];
        Array.Copy(to, src, clip.Width * draw.Psize);

        for (int y = 1; y < clip.Height; y++)
            Array.Copy(src, to, clip.Width * draw.Psize);
    }

    return 0;
}

// vips_draw_rect_class_init
public static void ClassInit(VipsDrawRectClass class)
{
    GObjectClass gobject_class = (GObjectClass)class;
    VipsObjectClass vobject_class = (VipsObjectClass)class;

    gobject_class.SetProperty = Object.SetProperty;
    gobject_class.GetProperty = Object.GetProperty;

    vobject_class.Nickname = "draw_rect";
    vobject_class.Description = _("paint a rectangle on an image");
    vobject_class.Build = Build;

    VIPS_ARG_INT(class, "left", 6,
        _("Left"),
        _("Rect to fill"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        typeof(VipsDrawRect).GetField("Left").Offset,
        -1000000000, 1000000000, 0);

    VIPS_ARG_INT(class, "top", 7,
        _("Top"),
        _("Rect to fill"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        typeof(VipsDrawRect).GetField("Top").Offset,
        -1000000000, 1000000000, 0);

    VIPS_ARG_INT(class, "width", 8,
        _("Width"),
        _("Rect to fill"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        typeof(VipsDrawRect).GetField("Width").Offset,
        -1000000000, 1000000000, 0);

    VIPS_ARG_INT(class, "height", 9,
        _("Height"),
        _("Rect to fill"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        typeof(VipsDrawRect).GetField("Height").Offset,
        -1000000000, 1000000000, 0);

    VIPS_ARG_BOOL(class, "fill", 10,
        _("Fill"),
        _("Draw a solid object"),
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        typeof(VipsDrawRect).GetField("Fill").Offset,
        false);
}

// vips_draw_rect_init
public void Init()
{
}

// vips_draw_rectv
public int DrawRectV(VipsImage image, double[] ink, int n, int left, int top, int width, int height, params object[] args)
{
    VipsArea area_ink = new VipsArea(vips_array_double_new(ink, n));
    int result;

    result = CallSplit("draw_rect", args,
        image, area_ink, left, top, width, height);
    vips_area_unref(area_ink);

    return result;
}

// vips_draw_rect
public static int DrawRect(VipsImage image, double[] ink, int n, int left, int top, int width, int height, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, height);
    result = DrawRectV(image,
        ink, n, left, top, width, height, args);
    va_end(ap);

    return result;
}

// vips_draw_rect1
public static int DrawRect1(VipsImage image, double ink, int left, int top, int width, int height, params object[] args)
{
    double[] array_ink = new double[1];
    array_ink[0] = ink;

    va_list ap;
    int result;

    va_start(ap, height);
    result = DrawRectV(image,
        array_ink, 1, left, top, width, height, args);
    va_end(ap);

    return result;
}

// vips_draw_point
public static int DrawPoint(VipsImage image, double[] ink, int n, int x, int y, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, y);
    result = DrawRectV(image,
        ink, n, x, y, 1, 1, args);
    va_end(ap);

    return result;
}

// vips_draw_point1
public static int DrawPoint1(VipsImage image, double ink, int x, int y, params object[] args)
{
    double[] array_ink = new double[1];
    array_ink[0] = ink;

    va_list ap;
    int result;

    va_start(ap, y);
    result = DrawRectV(image,
        array_ink, 1, x, y, 1, 1, args);
    va_end(ap);

    return result;
}
```
Note that I've assumed the existence of certain classes and methods (e.g. `VipsObject`, `VipsDraw`, `VipsArea`, etc.) which are not defined in this code snippet. You may need to modify the code to match your specific implementation.