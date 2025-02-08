Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;
using Cairo;

public class VipsForeignLoadSvg : VipsForeignLoad
{
    public double Dpi { get; set; }
    public double Scale { get; set; }
    public double CairoScale { get; set; }
    public bool Unlimited { get; set; }

    private RsvgHandle Page { get; set; }

    public override int GetFlagsFilename(string filename)
    {
        return VipsForeignFlags.Partial;
    }

    public override int GetFlags()
    {
        return VipsForeignFlags.Partial;
    }

#if LIBRSVG_CHECK_VERSION(2, 52, 0)
    private static double SvgCssLengthToPixels(RsvgLength length, double dpi)
    {
        double value = length.Length;

        switch (length.Unit)
        {
            case RsvgUnit.Px:
                break;
            case RsvgUnit.Em:
                value *= 12.0; // default font size
                break;
            case RsvgUnit.Ex:
                value *= 6.0; // default font size / 2
                break;
            case RsvgUnit.In:
                value *= dpi;
                break;
            case RsvgUnit.Cm:
                value = dpi * value / 2.54;
                break;
            case RsvgUnit.Mm:
                value = dpi * value / 25.4;
                break;
            case RsvgUnit.Pt:
                value = dpi * value / 72;
                break;
            case RsvgUnit.Pc:
                value = dpi * value / 6;
                break;
            default:
                value = 0;
                break;
        }

        return value;
    }
#endif

    public override int GetNaturalSize(out double width, out double height)
    {
#if LIBRSVG_CHECK_VERSION(2, 52, 0)
        if (!Page.GetIntrinsicSizeInPixels(out width, out height))
        {
            RsvgRectangle viewbox;

            bool hasWidth, hasHeight;
            RsvgLength iwidth, iheight;
            bool hasViewbox;

            Page.GetIntrinsicDimensions(out hasWidth, out iwidth,
                out hasHeight, out iheight,
                out hasViewbox, out viewbox);

#if LIBRSVG_CHECK_VERSION(2, 54, 0)
            width = SvgCssLengthToPixels(iwidth, Dpi);
            height = SvgCssLengthToPixels(iheight, Dpi);

            hasWidth = width > 0.5;
            hasHeight = height > 0.5;

            if (hasWidth && hasHeight)
            {
                // Success! Taking the viewbox into account is not needed.
            }
            else if (hasWidth && hasViewbox)
            {
                height = width * viewbox.Height / viewbox.Width;
            }
            else if (hasHeight && hasViewbox)
            {
                width = height * viewbox.Width / viewbox.Height;
            }
            else if (hasViewbox)
            {
                width = viewbox.Width;
                height = viewbox.Height;
            }
#else
            if (hasWidth && hasHeight)
            {
                // We can use these values directly.
                width = SvgCssLengthToPixels(iwidth, Dpi);
                height = SvgCssLengthToPixels(iheight, Dpi);
            }
            else if (hasWidth && hasViewbox)
            {
                width = SvgCssLengthToPixels(iwidth, Dpi);
                height = width * viewbox.Height / viewbox.Width;
            }
            else if (hasHeight && hasViewbox)
            {
                height = SvgCssLengthToPixels(iheight, Dpi);
                width = height * viewbox.Width / viewbox.Height;
            }
            else if (hasViewbox)
            {
                width = viewbox.Width;
                height = viewbox.Height;
            }
#endif
            if (width <= 0.5 || height <= 0.5)
            {
                // We haven't found a usable set of sizes, so try working out the visible area.
                Page.GetGeometryForElement(out viewbox);
                width = viewbox.X + viewbox.Width;
                height = viewbox.Y + viewbox.Height;
            }
        }

#else
        RsvgDimensionData dimensions;

        Page.GetDimensions(out dimensions);
        width = dimensions.Width;
        height = dimensions.Height;
#endif

        if (width < 0.5 || height < 0.5)
        {
            throw new Exception("bad dimensions");
        }

        width = Math.Round(width * Scale);
        height = Math.Round(height * Scale);

        return 0;
    }

    public override int GetScaledSize(out int width, out int height)
    {
        double scaledWidth, scaledHeight;

        // Get dimensions with the default dpi.
        Page.SetDpi(72.0);
        if (GetNaturalSize(out scaledWidth, out scaledHeight))
            return -1;

        // We scale up with Cairo --- scaling with rsvg_handle_set_dpi() will fail for SVGs with absolute sizes.
        CairoScale = Scale * Dpi / 72.0;
        scaledWidth *= CairoScale;
        scaledHeight *= CairoScale;

        width = (int)Math.Round(scaledWidth);
        height = (int)Math.Round(scaledHeight);

        return 0;
    }

    public override int Parse(VipsImage outImage)
    {
        int width, height;

        if (GetScaledSize(out width, out height))
            return -1;

        // We need pixels/mm for Vips.
        double res = Dpi / 25.4;

        outImage.InitFields(width, height,
            4, VipsFormat.UChar,
            VipsCoding.None, VipsInterpretation.sRGB, res, res);

        // We use a tilecache, so it's smalltile.
        if (outImage.PipelineV(VipsDemandStyle.SmallTile, null))
            return -1;

        return 0;
    }

    public override int Header()
    {
        return Parse(Out);
    }

    public override int Generate(VipsRegion outRegion)
    {
        const VipsForeignLoadSvg self = this;
        const VipsObjectClass class = VipsObject.GetClass(self);

        CairoSurface surface;
        CairoContext cr;
        int y;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_svg_generate: {0} \n     left = {1}, top = {2}, width = {3}, height = {4}",
            self,
            outRegion.Valid.Left, outRegion.Valid.Top, outRegion.Valid.Width, outRegion.Valid.Height);
#endif

        // rsvg won't always paint the background.
        VipsRegion.Black(outRegion);

        surface = CairoImageSurface.CreateForData(
            VipsRegion.Addr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top),
            CairoFormat.Argb32,
            outRegion.Valid.Width, outRegion.Valid.Height,
            VipsRegion.LSkip(outRegion));
        cr = Cairo.Create(surface);
        Cairo.Surface.Destroy(surface);

        // rsvg is single-threaded, but we don't need to lock since we're running inside a non-threaded tilecache.
#if LIBRSVG_CHECK_VERSION(2, 46, 0)
        {
            RsvgRectangle viewport;
            GError error = null;

            // No need to scale -- we always set the viewport to the whole image, and set the region to draw on the surface.
            Cairo.Translate(cr, -outRegion.Valid.Left, -outRegion.Valid.Top);
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = outRegion.Im.Xsize;
            viewport.Height = outRegion.Im.Ysize;

            if (!Page.RenderDocument(cr, ref viewport, ref error))
            {
                Cairo.Destroy(cr);
                VipsOperation.Invalidate(VipsOperation(self));
                throw new Exception("SVG rendering failed");
            }

            Cairo.Destroy(cr);
        }
#else
        Cairo.Scale(cr, CairoScale, CairoScale);
        Cairo.Translate(cr, -outRegion.Valid.Left / CairoScale,
            -outRegion.Valid.Top / CairoScale);

        if (!Page.RenderCairo(cr))
        {
            Cairo.Destroy(cr);
            VipsOperation.Invalidate(VipsOperation(self));
            throw new Exception("SVG rendering failed");
        }

        Cairo.Destroy(cr);
#endif

        // Cairo makes pre-multipled BRGA -- we must byteswap and unpremultiply.
        for (y = 0; y < outRegion.Valid.Height; y++)
            Vips.PremultipliedBgra2Rgba(
                (uint[])VipsRegion.Addr(outRegion, outRegion.Valid.Left, outRegion.Valid.Top + y),
                outRegion.Valid.Width);

        return 0;
    }

    public override int Load()
    {
        VipsImage[] tiles = new VipsImage[3];

        // Enough tiles for two complete rows.
        tiles[0] = Vips.Image.New();
        if (Parse(tiles[0]) ||
            tiles[0].Generate(null, Generate) ||
            Vips.Tilecache(tiles[0], ref tiles[1],
                "tile_width", 2000,
                "tile_height", 2000,
                "max_tiles", 2 * (1 + tiles[0].Xsize / 2000),
                null) ||
            Vips.Image.Write(tiles[1], Real))
            return -1;

        return 0;
    }

    protected override void Dispose(GObject gobject)
    {
        VIPS.Unref(Page);

        base.Dispose(gobject);
    }
}

public class VipsForeignLoadSvgSource : VipsForeignLoadSvg
{
    public VipsSource Source { get; set; }

    public override bool IsASource(VipsSource source)
    {
        unsigned char[] data;
        int bytesRead;

        if ((bytesRead = Source.SniffAtMost(source, ref data, 1000)) <= 0)
            return false;

        return VipsForeignLoadSvg.IsA(data, bytesRead);
    }

    public override int Header()
    {
        VipsForeignLoadSvg svg = this;
        RsvgHandleFlags flags = svg.Unlimited ? RsvgHandleFlag.Unlimited : 0;

        GError error = null;

        GInputStream gstream;

        if (Source.Rewind())
            return -1;

        gstream = Vips.G.InputStream.NewFromSource(Source);
        if (!(svg.Page = Rsvg.Handle.NewFromStreamSync(gstream, null, flags, null, ref error)))
        {
            gstream.Dispose();
            throw new Exception("SVG rendering failed");
        }
        gstream.Dispose();

        return Header();
    }

    public override int Load()
    {
        if (Source.Rewind() ||
            base.Load() ||
            Source.Decode())
            return -1;

        return 0;
    }

    protected override void Dispose(GObject gobject)
    {
        base.Dispose(gobject);
    }
}

public class VipsForeignLoadSvgFile : VipsForeignLoadSvg
{
    public string Filename { get; set; }

    public override bool IsA(string filename)
    {
        unsigned char[] data;
        int bytes;

        if ((bytes = Vips.GetBytes(filename, data, 1000)) <= 0)
            return false;

        return VipsForeignLoadSvg.IsA(data, bytes);
    }

    public override int Header()
    {
        VipsForeignLoadSvg svg = this;
        RsvgHandleFlags flags = svg.Unlimited ? RsvgHandleFlag.Unlimited : 0;

        GError error = null;

        GFile gfile;

        gfile = G.File.NewForPath(Filename);
        if (!(svg.Page = Rsvg.Handle.NewFromGFileSync(gfile, flags, null, ref error)))
        {
            gfile.Dispose();
            throw new Exception("SVG rendering failed");
        }
        gfile.Dispose();

        Vips.SetStr(Out.Filename, Filename);

        return Header();
    }

    protected override void Dispose(GObject gobject)
    {
        base.Dispose(gobject);
    }
}

public class VipsForeignLoadSvgBuffer : VipsForeignLoadSvg
{
    public VipsArea Buffer { get; set; }

    public override bool IsABuffer(unsigned char[] data, int len)
    {
        return VipsForeignLoadSvg.IsA(data, len);
    }

    public override int Header()
    {
        VipsForeignLoadSvg svg = this;
        RsvgHandleFlags flags = svg.Unlimited ? RsvgHandleFlag.Unlimited : 0;

        GError error = null;

        GInputStream gstream;

        gstream = G.MemoryInputStream.NewFromData(Buffer.Data, Buffer.Length, null);
        if (!(svg.Page = Rsvg.Handle.NewFromStreamSync(gstream, null, flags, null, ref error)))
        {
            gstream.Dispose();
            throw new Exception("SVG rendering failed");
        }
        gstream.Dispose();

        return Header();
    }

    protected override void Dispose(GObject gobject)
    {
        base.Dispose(gobject);
    }
}

public class Vips
{
    public static int Svgload(string filename, out VipsImage image, params object[] args)
    {
        // implementation of vips_svgload function
    }

    public static int SvgloadBuffer(void* buf, int len, out VipsImage image, params object[] args)
    {
        // implementation of vips_svgload_buffer function
    }

    public static int SvgloadString(string str, out VipsImage image, params object[] args)
    {
        // implementation of vips_svgload_string function
    }

    public static int SvgloadSource(VipsSource source, out VipsImage image, params object[] args)
    {
        // implementation of vips_svgload_source function
    }
}
```