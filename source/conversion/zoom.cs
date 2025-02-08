Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsZoom : VipsConversion
{
    public VipsImage In { get; set; }
    public int Xfac { get; set; }
    public int Yfac { get; set; }

    public override int Gen(VipsRegion out_region, object seq, object a, object b, bool stop)
    {
        VipsRegion ir = (VipsRegion)seq;
        VipsZoom zoom = (VipsZoom)b;

        // Output area we are building.
        var r = out_region.Valid;
        int ri = VIPS_RECT_RIGHT(r);
        int bo = VIPS_RECT_BOTTOM(r);

        VipsRect s;
        int left, right, top, bottom;
        int width, height;

        // Area of input we need. We have to round out, as we may have
        // part-pixels all around the edges.
        left = VIPS_ROUND_DOWN(r.Left, zoom.Xfac);
        right = VIPS_ROUND_UP(ri, zoom.Xfac);
        top = VIPS_ROUND_DOWN(r.Top, zoom.Yfac);
        bottom = VIPS_ROUND_UP(bo, zoom.Yfac);
        width = right - left;
        height = bottom - top;
        s.Left = left / zoom.Xfac;
        s.Top = top / zoom.Yfac;
        s.Width = width / zoom.Xfac;
        s.Height = height / zoom.Yfac;

        if (VipsRegion.Prepare(ir, ref s))
            return -1;

        // Find the part of the output (if any) which uses only whole pels.
        left = VIPS_ROUND_UP(r.Left, zoom.Xfac);
        right = VIPS_ROUND_DOWN(ri, zoom.Xfac);
        top = VIPS_ROUND_UP(r.Top, zoom.Yfac);
        bottom = VIPS_ROUND_DOWN(bo, zoom.Yfac);
        width = right - left;
        height = bottom - top;

        // Stage 1: we just paint the whole pels in the centre of the region.
        // As we know they are not clipped, we can do it quickly.
        if (width > 0 && height > 0)
            VipsZoomPaintWhole(out_region, ir, zoom, left, right, top, bottom);

        // Just fractional pixels left. Paint in the top, left, right and
        // bottom parts.
        if (top - r.Top > 0)
            // Some top pixels.
            VipsZoomPaintPart(out_region, ir, zoom, r.Left, ri, r.Top, Math.Min(top, bo));
        if (left - r.Left > 0 && height > 0)
            // Left pixels.
            VipsZoomPaintPart(out_region, ir, zoom, r.Left, VIPS_MAX(right, r.Left), top, bottom);
        if (ri - right > 0 && height > 0)
            // Right pixels.
            VipsZoomPaintPart(out_region, ir, zoom, VIPS_MAX(right, r.Left), ri, top, bottom);
        if (bo - bottom > 0 && height >= 0)
            // Bottom pixels.
            VipsZoomPaintPart(out_region, ir, zoom, r.Left, ri, VIPS_MAX(bottom, r.Top), bo);

        return 0;
    }

    public override int Build(VipsObject obj)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(obj);
        var conversion = (VipsConversion)obj;
        var zoom = (VipsZoom)obj;

        if (base.Build(obj) != 0)
            return -1;

        g_assert(zoom.Xfac > 0);
        g_assert(zoom.Yfac > 0);

        // Make sure we won't get integer overflow.
        if ((double)zoom.In.Xsize * zoom.Xfac > (double)int.MaxValue / 2 ||
            (double)zoom.In.Ysize * zoom.Yfac > (double)int.MaxValue / 2)
        {
            VipsError(class.Nickname, "%s", _("zoom factors too large"));
            return -1;
        }
        if (zoom.Xfac == 1 && zoom.Yfac == 1)
            return VipsImage.Write(zoom.In, conversion.Out);

        if (VipsImage.PioInput(zoom.In) || VipsCheckCodingKnown(class.Nickname, zoom.In))
            return -1;

        // Set demand hints. THINSTRIP will prevent us from using
        // vips_zoom_paint_whole() much ... so go for FATSTRIP.
        if (VipsImage.Pipelinev(conversion.Out, VIPS_DEMAND_STYLE_FATSTRIP, zoom.In, null))
            return -1;
        conversion.Out.Xsize = zoom.In.Xsize * zoom.Xfac;
        conversion.Out.Ysize = zoom.In.Ysize * zoom.Yfac;

        if (VipsImage.Generate(conversion.Out, VipsStartOne, Gen, VipsStopOne, zoom.In, zoom))
            return -1;

        return 0;
    }

    public static void ClassInit(VipsZoomClass class_)
    {
        var gobject_class = (GObjectClass)class_;
        var vobject_class = (VipsObjectClass)class_;
        var operation_class = (VipsOperationClass)class_;

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        vobject_class.Nickname = "zoom";
        vobject_class.Description = _("zoom an image");
        vobject_class.Build = Build;

        operation_class.Flags = VIPS_OPERATION_SEQUENTIAL;

        VipsArgImage(class_, "input", 1, _("Input"), _("Input image"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsZoom, In));
        VipsArgInt(class_, "xfac", 3, _("Xfac"), _("Horizontal zoom factor"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsZoom, Xfac), 1, int.MaxValue, 1);
        VipsArgInt(class_, "yfac", 4, _("Yfac"), _("Vertical zoom factor"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsZoom, Yfac), 1, int.MaxValue, 1);
    }

    public static void Init(VipsZoom zoom)
    {
    }
}

public class VipsZoomPaintWhole
{
    public static void Paint(VipsRegion out_region, VipsRegion ir, VipsZoom zoom, int left, int right, int top, int bottom)
    {
        var ps = VIPS_IMAGE_SIZEOF_PEL(ir.Im);
        var ls = VIPS_REGION_LSKIP(out_region);
        var rs = ps * (right - left);

        // Transform to ir coordinates.
        var ileft = left / zoom.Xfac;
        var iright = right / zoom.Xfac;
        var itop = top / zoom.Yfac;
        var ibottom = bottom / zoom.Yfac;

        int x, y, z, i;

        g_assert(right > left && bottom > top &&
            right % zoom.Xfac == 0 &&
            left % zoom.Xfac == 0 &&
            top % zoom.Yfac == 0 &&
            bottom % zoom.Yfac == 0);

        // Loop over input, as we know we are all whole.
        for (y = itop; y < ibottom; y++)
        {
            var p = VIPS_REGION_ADDR(ir, ileft, y);
            var q = VIPS_REGION_ADDR(out_region, left, y * zoom.Yfac);
            var r;

            // Expand the first line of pels.
            r = q;
            for (x = ileft; x < iright; x++)
            {
                // Copy each pel xfac times.
                for (z = 0; z < zoom.Xfac; z++)
                {
                    for (i = 0; i < ps; i++)
                        r[i] = p[i];

                    r += ps;
                }

                p += ps;
            }

            // Copy the expanded line yfac-1 times.
            r = q + ls;
            for (z = 1; z < zoom.Yfac; z++)
            {
                VipsMemCopy(r, q, rs);
                r += ls;
            }
        }
    }
}

public class VipsZoomPaintPart
{
    public static void Paint(VipsRegion out_region, VipsRegion ir, VipsZoom zoom, int left, int right, int top, int bottom)
    {
        var ps = VIPS_IMAGE_SIZEOF_PEL(ir.Im);
        var ls = VIPS_REGION_LSKIP(out_region);
        var rs = ps * (right - left);

        // Start position in input.
        var ix = left / zoom.Xfac;
        var iy = top / zoom.Yfac;

        // Pels down to yfac boundary, pels down to bottom. Do the smallest of
        // these for first y loop.
        var ptbound = (iy + 1) * zoom.Yfac - top;
        var ptbot = bottom - top;

        int yt = Math.Min(ptbound, ptbot);

        int x, y, z, i;

        g_assert(right - left >= 0 && bottom - top >= 0);

        // Have to loop over output.
        for (y = top; y < bottom;)
        {
            var p = VIPS_REGION_ADDR(ir, ix, y / zoom.Yfac);
            var q = VIPS_REGION_ADDR(out_region, left, y);
            var r;

            // Output pels until we jump the input pointer.
            int xt = (ix + 1) * zoom.Xfac - left;

            // Loop for this output line.
            r = q;
            for (x = left; x < right; x++)
            {
                // Copy 1 pel.
                for (i = 0; i < ps; i++)
                    r[i] = p[i];
                r += ps;

                // Move input if on boundary.
                --xt;
                if (xt == 0)
                {
                    xt = zoom.Xfac;
                    p += ps;
                }
            }

            // Repeat that output line until the bottom of this pixel
            // boundary, or we hit bottom.
            r = q + ls;
            for (z = 1; z < yt; z++)
            {
                VipsMemCopy(r, q, rs);
                r += ls;
            }

            // Move y on by the number of lines we wrote.
            y += yt;

            // Reset yt for next iteration.
            yt = zoom.Yfac;
        }
    }
}
```

Note that I've assumed some methods and classes are already converted to C# (e.g., `VipsImage`, `VipsRegion`, `VipsConversion`, etc.) and have not included their implementation here. Also, I've used the `GObject` class from the GLib library for property handling, as it is commonly used in GObject-based applications.

Please note that this code may require some adjustments to fit your specific use case and environment.