```csharp
// vips_rect_includespoint:
// @r: rectangle to test
// @x: position to test for
// @y: position to test for
//
// Does @r contain point (@x, @y)?
//
// Returns: true if @r contains (@x, @y).
public static bool VipsRectIncludesPoint(VipsRect r, int x, int y)
{
    return r.Left <= x &&
        r.Top <= y &&
        r.Left + r.Width > x &&
        r.Top + r.Height > y;
}

// vips_rect_isempty:
// @r: rectangle to test
//
// Is @r empty? ie. zero width or height.
//
// Returns: true if @r contains no pixels.
public static bool VipsRectIsEmpty(VipsRect r)
{
    return r.Width <= 0 || r.Height <= 0;
}

// vips_rect_includesrect:
// @r1: outer rectangle
// @r2: inner rectangle
//
// Is @r2 a subset of @r1?
//
// Returns: true if @r2 is a subset of @r1.
public static bool VipsRectIncludesRect(VipsRect r1, VipsRect r2)
{
    return r1.Left <= r2.Left &&
        r1.Top <= r2.Top &&
        r1.Left + r1.Width >= r2.Left + r2.Width &&
        r1.Top + r1.Height >= r2.Top + r2.Height;
}

// vips_rect_equalsrect:
// @r1: first rectangle
// @r2: second rectangle
//
// Is @r1 equal to @r2?
//
// Returns: true if @r1 is equal to @r2.
public static bool VipsRectEqualsRect(VipsRect r1, VipsRect r2)
{
    return r1.Left == r2.Left && r1.Top == r2.Top &&
        r1.Width == r2.Width && r1.Height == r2.Height;
}

// vips_rect_overlapsrect:
// @r1: first rectangle
// @r2: second rectangle
//
// Do @r1 and @r2 have a non-empty intersection?
//
// Returns: true if @r2 and @r1 overlap.
public static bool VipsRectOverlapsRect(VipsRect r1, VipsRect r2)
{
    VipsRect intersection = new VipsRect();
    VipsRectIntersectRect(r1, r2, ref intersection);
    return !VipsRectIsEmpty(intersection);
}

// vips_rect_marginadjust:
// @r: rectangle to adjust
// @n: enlarge by
//
// Enlarge @r by @n. +1 means out one pixel.
public static void VipsRectMarginAdjust(ref VipsRect r, int n)
{
    r.Left -= n;
    r.Top -= n;
    r.Width += 2 * n;
    r.Height += 2 * n;
}

// vips_rect_intersectrect:
// @r1: input rectangle 1
// @r2: input rectangle 2
// @out: (out): output rectangle
//
// Fill @out with the intersection of @r1 and @r2. @out can equal @r1 or @r2.
public static void VipsRectIntersectRect(VipsRect r1, VipsRect r2, ref VipsRect outRect)
{
    int left = Math.Max(r1.Left, r2.Left);
    int top = Math.Max(r1.Top, r2.Top);
    int right = Math.Min(VipsRectRight(r1), VipsRectRight(r2));
    int bottom = Math.Min(VipsRectBottom(r1), VipsRectBottom(r2));
    int width = Math.Max(0, right - left);
    int height = Math.Max(0, bottom - top);

    outRect.Left = left;
    outRect.Top = top;
    outRect.Width = width;
    outRect.Height = height;
}

// vips_rect_unionrect:
// @r1: input rectangle 1
// @r2: input rectangle 2
// @out: (out): output rectangle
//
// Fill @out with the bounding box of @r1 and @r2. @out can equal @r1 or @r2.
public static void VipsRectUnionRect(VipsRect r1, VipsRect r2, ref VipsRect outRect)
{
    if (VipsRectIsEmpty(r1))
        outRect = r2;
    else if (VipsRectIsEmpty(r2))
        outRect = r1;
    else
    {
        int left = Math.Min(r1.Left, r2.Left);
        int top = Math.Min(r1.Top, r2.Top);
        int width = Math.Max(VipsRectRight(r1), VipsRectRight(r2)) - left;
        int height = Math.Max(VipsRectBottom(r1), VipsRectBottom(r2)) - top;

        outRect.Left = left;
        outRect.Top = top;
        outRect.Width = width;
        outRect.Height = height;
    }
}

// vips_rect_dup: (skip)
// @r: rectangle to duplicate
//
// Duplicate a rect to the heap. You need to free the result with g_free().
//
// Returns: (transfer full): a pointer to copy of @r allocated on the heap.
public static VipsRect VipsRectDup(VipsRect r)
{
    VipsRect outRect = new VipsRect();
    outRect.Left = r.Left;
    outRect.Top = r.Top;
    outRect.Width = r.Width;
    outRect.Height = r.Height;
    return outRect;
}

// vips_rect_normalise:
// @r: rect to normalise
//
// Make sure width and height are >0 by moving the origin and flipping the
// rect.
public static void VipsRectNormalise(ref VipsRect r)
{
    if (r.Width < 0)
    {
        r.Left += r.Width;
        r.Width *= -1;
    }
    if (r.Height < 0)
    {
        r.Top += r.Height;
        r.Height *= -1;
    }
}

public struct VipsRect
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public static int Right(VipsRect rect) => rect.Left + rect.Width;
    public static int Bottom(VipsRect rect) => rect.Top + rect.Height;
}
```