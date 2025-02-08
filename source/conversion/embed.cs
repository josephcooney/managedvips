Here is the C# code equivalent to the provided C code:

```csharp
// VipsEmbedBase

public class VipsEmbedBase : VipsConversion
{
    public VipsImage In { get; set; }
    public VipsExtend Extend { get; set; }
    public double[] Background { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    // Pixel we paint calculated from background.
    private Pel Ink;

    // Geometry calculations.
    private Rect Rout;
    private Rect Rsub;

    // The 8 border pieces. The 4 borders strictly up/down/left/right of
    // the main image, and the 4 corner pieces.
    private Rect[] Border = new Rect[8];

    public VipsEmbedBase()
    {
        Extend = VipsExtend.Black;
        Background = new double[] { 0 };
    }

    protected override int Build(VipsObject object)
    {
        // nip2 can generate this quite often ... just copy.
        if (X == 0 && Y == 0 && Width == In.Xsize && Height == In.Ysize)
            return VipsImage.Write(In, Out);

        if (!VipsObject.ArgumentIsSet(object, "extend") &&
            VipsObject.ArgumentIsSet(object, "background"))
            Extend = VipsExtend.Background;

        if (Extend == VipsExtend.Background)
        {
            // Paint the borders a solid value.
            for (int i = 0; i < 8; i++)
                VipsRegion.Paint(Out, Border[i], Ink);
        }

        switch (Extend)
        {
            case VipsExtend.Repeat:
                // Clock arithmetic: we want negative x/y to wrap around
                // nicely.
                int nx = X < 0 ? -X % In.Xsize : In.Xsize - X % In.Xsize;
                int ny = Y < 0 ? -Y % In.Ysize : In.Ysize - Y % In.Ysize;

                if (VipsReplicate(In, Width / In.Xsize + 2,
                    Height / In.Ysize + 2) ||
                    VipsExtractArea(Width / In.Xsize + 2,
                        Height / In.Ysize + 2, nx, ny, Width, Height))
                    return -1;

                break;

            case VipsExtend.Mirror:
                // As repeat, but the tiles are twice the size because of
                // mirroring.
                int w2 = In.Xsize * 2;
                int h2 = In.Ysize * 2;

                nx = X < 0 ? -X % w2 : w2 - X % w2;
                ny = Y < 0 ? -Y % h2 : h2 - Y % h2;

                if (VipsFlip(In, Width / In.Xsize + 2,
                    VipsDirection.Horizontal) ||
                    VipsJoin(In, Width / In.Xsize + 2,
                        VipsDirection.Horizontal) ||
                    VipsFlip(Width / In.Xsize + 2,
                        VipsDirection.Vertical) ||
                    VipsJoin(Width / In.Xsize + 2,
                        VipsDirection.Vertical) ||
                    VipsReplicate(Width / In.Xsize + 2,
                        Height / In.Ysize + 2, nx, ny, Width, Height))
                    return -1;

                break;

            case VipsExtend.Black:
            case VipsExtend.White:
            case VipsExtend.Background:
            case VipsExtend.Copy:
                // embed is used in many places. We don't really care about
                // geometry, so use ANY to avoid disturbing all pipelines.
                if (VipsImagePipeline(Out,
                    VipsDemandStyle.Any, In))
                    return -1;

                Out.Xsize = Width;
                Out.Ysize = Height;

                // Whole output area.
                Rout.Left = 0;
                Rout.Top = 0;
                Rout.Width = Out.Xsize;
                Rout.Height = Out.Ysize;

                // Rect occupied by image (can be clipped to nothing).
                Rsub.Left = X;
                Rsub.Top = Y;
                Rsub.Width = In.Xsize;
                Rsub.Height = In.Ysize;
                VipsRect.IntersectRect(&Rsub, &Rout, &Rsub);

                if (VipsImageGenerate(Out,
                    VipsStartOne, VipsEmbedBaseGen, VipsStopOne,
                    In))
                    return -1;

                break;

            default:
                g_assert_not_reached();
        }

        return 0;
    }
}

// vips_embed_base_find_edge

private void FindEdge(VipsRect r, int i, out VipsRect out)
{
    // Expand the border by 1 pixel, intersect with the image area, and we
    // get the edge. Usually too much though: eg. we could make the entire
    // right edge.
    out = Border[i];
    VipsRect.MarginAdjust(out, 1);
    VipsRect.IntersectRect(out, &Rsub, out);

    // Usually too much though: eg. we could make the entire
    // right edge. If we're strictly up/down/left/right of the image, we
    // can trim.
    if (i == 0 || i == 2)
    {
        VipsRect extend;

        // Above or below.
        extend = r;
        extend.Top = 0;
        extend.Height = Height;
        VipsRect.IntersectRect(out, &extend, out);
    }
    if (i == 1 || i == 3)
    {
        VipsRect extend;

        // Left or right.
        extend = r;
        extend.Left = 0;
        extend.Width = Width;
        VipsRect.IntersectRect(out, &extend, out);
    }
}

// vips_embed_base_copy_pixel

private void CopyPixel(VipsPel q, VipsPel p, int n)
{
    const int bs = VipsImage.SizeOfPel(In);

    for (int x = 0; x < n; x++)
        for (int b = 0; b < bs; b++)
            *q++ = p[b];
}

// vips_embed_base_paint_edge

private void PaintEdge(VipsRegion out_region, int i, VipsRect r, VipsPel p, int plsk)
{
    const int bs = VipsImage.SizeOfPel(In);

    VipsRect todo;
    VipsPel q;
    int y;

    // Pixels left to paint.
    todo = r;

    // Corner pieces ... copy the single pixel to paint the top line of
    // todo, then use the line copier below to paint the rest of it.
    if (i > 3)
    {
        q = VipsRegion.Addr(out_region, todo.Left, todo.Top);
        CopyPixel(q, p, todo.Width);

        p = q;
        todo.Top += 1;
        todo.Height -= 1;
    }

    if (i == 1 || i == 3)
    {
        // Vertical line of pixels to copy.
        for (y = 0; y < todo.Height; y++)
        {
            q = VipsRegion.Addr(out_region, todo.Left, todo.Top + y);
            CopyPixel(q, p, todo.Width);
            p += plsk;
        }
    }
    else
    {
        // Horizontal line of pixels to copy.
        for (y = 0; y < todo.Height; y++)
        {
            q = VipsRegion.Addr(out_region, todo.Left, todo.Top + y);
            memcpy(q, p, bs * todo.Width);
        }
    }
}

// vips_embed_base_gen

private int Gen(VipsRegion out_region,
    object seq, object a, object b, ref bool stop)
{
    VipsRegion ir = (VipsRegion)seq;
    VipsEmbedBase base = (VipsEmbedBase)b;
    VipsRect r = &out_region.Valid;

    // Entirely within the input image? Generate the subimage and copy
    // pointers.
    if (VipsRect.IncludesRect(&base.Rsub, r))
    {
        VipsRect need;

        need = r;
        need.Left -= base.X;
        need.Top -= base.Y;
        if (VipsRegion.Prepare(ir, &need) ||
            VipsRegion.Region(out_region, ir, r, need.Left, need.Top))
            return -1;

        return 0;
    }

    // Does any of the input image appear in the area we have been asked
    // to make? Paste it in.
    VipsRect ovl;
    VipsRect.IntersectRect(r, &base.Rsub, &ovl);
    if (!VipsRect.IsEmpty(&ovl))
    {
        // Paint the bits coming from the input image.
        ovl.Left -= base.X;
        ovl.Top -= base.Y;
        if (VipsRegion.PrepareTo(ir, out_region, &ovl,
                ovl.Left + base.X, ovl.Top + base.Y))
            return -1;
    }

    switch (base.Extend)
    {
        case VipsExtend.Black:
        case VipsExtend.White:
            // Paint the borders a solid value.
            for (int i = 0; i < 8; i++)
                VipsRegion.Paint(out_region, Border[i], Ink);

            break;

        case VipsExtend.Background:
            // Paint the borders a solid value.
            for (int i = 0; i < 8; i++)
                VipsRegion.PaintPel(out_region, Border[i], base.Ink);

            break;

        case VipsExtend.Copy:
            // Extend the borders.
            for (int i = 0; i < 8; i++)
            {
                VipsRect todo;
                VipsRect edge;

                FindEdge(base, r, &todo);
                if (!VipsRect.IsEmpty(&todo))
                {
                    VipsEmbedBaseFindEdge(base, r, i, &edge);

                    // Did we paint any of the input image? If we
                    // did, we can fetch the edge pixels from
                    // that.
                    if (!VipsRect.IsEmpty(&ovl))
                    {
                        p = VipsRegion.Addr(out_region,
                            edge.Left, edge.Top);
                        plsk = VipsRegion.LSkip(out_region);
                    }
                    else
                    {
                        // No pixels painted ... fetch directly from the input image.
                        edge.Left -= base.X;
                        edge.Top -= base.Y;
                        if (VipsRegion.Prepare(ir, &edge))
                            return -1;
                        p = VipsRegion.Addr(ir,
                            edge.Left, edge.Top);
                        plsk = VipsRegion.LSkip(ir);
                    }

                    PaintEdge(base, out_region, i, todo, p, plsk);
                }
            }

            break;

        default:
            g_assert_not_reached();
    }

    return 0;
}

// vips_embed_base_build

private int Build(VipsObject object)
{
    VipsObjectClass class = (VipsObjectClass)VipsObject.GetClass(object);
    VipsConversion conversion = (VipsConversion)object;
    VipsEmbedBase base = (VipsEmbedBase)object;

    // nip2 can generate this quite often ... just copy.
    if (X == 0 && Y == 0 && Width == In.Xsize && Height == In.Ysize)
        return VipsImage.Write(In, conversion.Out);

    if (!VipsObject.ArgumentIsSet(object, "extend") &&
        VipsObject.ArgumentIsSet(object, "background"))
        base.Extend = VipsExtend.Background;

    if (base.Extend == VipsExtend.Background)
    {
        // Paint the borders a solid value.
        for (int i = 0; i < 8; i++)
            VipsRegion.Paint(Out, Border[i], Ink);
    }

    switch (base.Extend)
    {
        case VipsExtend.Repeat:
            // Clock arithmetic: we want negative x/y to wrap around
            // nicely.
            int nx = X < 0 ? -X % In.Xsize : In.Xsize - X % In.Xsize;
            int ny = Y < 0 ? -Y % In.Ysize : In.Ysize - Y % In.Ysize;

            if (VipsReplicate(In, Width / In.Xsize + 2,
                Height / In.Ysize + 2) ||
                VipsExtractArea(Width / In.Xsize + 2,
                    Height / In.Ysize + 2, nx, ny, Width, Height))
                return -1;

            break;

        case VipsExtend.Mirror:
            // As repeat, but the tiles are twice the size because of
            // mirroring.
            int w2 = In.Xsize * 2;
            int h2 = In.Ysize * 2;

            nx = X < 0 ? -X % w2 : w2 - X % w2;
            ny = Y < 0 ? -Y % h2 : h2 - Y % h2;

            if (VipsFlip(In, Width / In.Xsize + 2,
                VipsDirection.Horizontal) ||
                VipsJoin(In, Width / In.Xsize + 2,
                    VipsDirection.Horizontal) ||
                VipsFlip(Width / In.Xsize + 2,
                    VipsDirection.Vertical) ||
                VipsJoin(Width / In.Xsize + 2,
                    VipsDirection.Vertical) ||
                VipsReplicate(Width / In.Xsize + 2,
                    Height / In.Ysize + 2, nx, ny, Width, Height))
                return -1;

            break;

        case VipsExtend.Black:
        case VipsExtend.White:
        case VipsExtend.Background:
        case VipsExtend.Copy:
            // embed is used in many places. We don't really care about
            // geometry, so use ANY to avoid disturbing all pipelines.
            if (VipsImagePipeline(Out,
                VipsDemandStyle.Any, In))
                return -1;

            Out.Xsize = Width;
            Out.Ysize = Height;

            // Whole output area.
            Rout.Left = 0;
            Rout.Top = 0;
            Rout.Width = Out.Xsize;
            Rout.Height = Out.Ysize;

            // Rect occupied by image (can be clipped to nothing).
            Rsub.Left = X;
            Rsub.Top = Y;
            Rsub.Width = In.Xsize;
            Rsub.Height = In.Ysize;
            VipsRect.IntersectRect(&Rsub, &Rout, &Rsub);

            if (VipsImageGenerate(Out,
                VipsStartOne, VipsEmbedBaseGen, VipsStopOne,
                In))
                return -1;

            break;

        default:
            g_assert_not_reached();
    }

    return 0;
}

// vips_embed

public class VipsEmbed : VipsEmbedBase
{
    public int X { get; set; }
    public int Y { get; set; }

    protected override int Build(VipsObject object)
    {
        // Just pass the specified x, y down.
        base.X = X;
        base.Y = Y;

        return base.Build(object);
    }
}

// vips_gravity

public class VipsGravity : VipsEmbedBase
{
    public CompassDirection Direction { get; set; }

    protected override int Build(VipsObject object)
    {
        if (VipsObject.ArgumentIsSet(object, "in") &&
            VipsObject.ArgumentIsSet(object, "width") &&
            VipsObject.ArgumentIsSet(object, "height") &&
            VipsObject.ArgumentIsSet(object, "direction"))
        {
            switch (Direction)
            {
                case CompassDirection.Centre:
                    base.X = (base.Width - In.Xsize) / 2;
                    base.Y = (base.Height - In.Ysize) / 2;
                    break;

                case CompassDirection.North:
                    base.X = (base.Width - In.Xsize) / 2;
                    base.Y = 0;
                    break;

                case CompassDirection.East:
                    base.X = base.Width - In.Xsize;
                    base.Y = (base.Height - In.Ysize) / 2;
                    break;

                case CompassDirection.South:
                    base.X = (base.Width - In.Xsize) / 2;
                    base.Y = base.Height - In.Ysize;
                    break;

                case CompassDirection.West:
                    base.X = 0;
                    base.Y = (base.Height - In.Ysize) / 2;
                    break;

                case CompassDirection.NorthEast:
                    base.X = base.Width - In.Xsize;
                    base.Y = 0;
                    break;

                case CompassDirection.SouthEast:
                    base.X = base.Width - In.Xsize;
                    base.Y = base.Height - In.Ysize;
                    break;

                case CompassDirection.SouthWest:
                    base.X = 0;
                    base.Y = base.Height - In.Ysize;
                    break;

                case CompassDirection.NorthWest:
                    base.X = 0;
                    base.Y = 0;
                    break;

                default:
                    g_assert_not_reached();
            }
        }

        return base.Build(object);
    }
}
```