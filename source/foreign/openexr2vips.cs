Here is the C# code equivalent to the provided C code:

```csharp
// Converted from: vips__openexr_isexr

public static bool IsExr(string filename)
{
    byte[] buf = new byte[4];
    if (GetBytes(filename, buf, 4) == 4)
        return buf[0] == 0x76 && buf[1] == 0x2f &&
               buf[2] == 0x31 && buf[3] == 0x01;
    return false;
}

// Converted from: get_imf_error

private static void GetImfError()
{
    throw new Exception("EXR error");
}

// Converted from: read_close

private static void ReadClose(Read read)
{
    if (read.Tiles != null) Imf.CloseTiledInputFile(read.Tiles);
    if (read.Lines != null) Imf.CloseInputFile(read.Lines);
}

// Converted from: read_destroy

private static void ReadDestroy(VipsImage out, Read read)
{
    VIPS.Free(read.Filename);

    ReadClose(read);

    GCHandle handle = GCHandle.Alloc(read, GCHandleType.Pinned);
    Marshal.DestroyStructure(handle.AddrOfPinnedObject(), typeof(Read));
    handle.Free();
}

// Converted from: read_new

public static Read ReadNew(string filename, VipsImage out)
{
    Read read;
    int xmin, ymin, xmax, ymax;

    if ((read = new Read()) == null) return null;
    read.Filename = VIPS.StrDup(filename);
    read.Out = out;
    read.Tiles = null;
    read.Lines = null;
    if (out != null)
        out.Close += (sender, e) => ReadDestroy(out, read);

    // Try to open tiled first ...
    if ((read.Tiles = Imf.OpenTiledInputFile(read.Filename)) == null)
    {
        if ((read.Lines = Imf.OpenInputFile(read.Filename)) == null)
        {
            GetImfError();
            return null;
        }
    }

#ifdef DEBUG
    if (read.Tiles != null) Console.WriteLine("exr2vips: opening in tiled mode");
    else Console.WriteLine("exr2vips: opening in scanline mode");
#endif /*DEBUG*/

    if (read.Tiles != null)
    {
        read.Header = Imf.TiledInputHeader(read.Tiles);
        read.TileWidth = Imf.TiledInputTileXSize(read.Tiles);
        read.TileHeight = Imf.TiledInputTileYSize(read.Tiles);
    }
    else
        read.Header = Imf.InputHeader(read.Lines);

    Imf.HeaderDataWindow(read.Header, out xmin, out ymin, out xmax, out ymax);
    read.Window.Left = xmin;
    read.Window.Top = ymin;
    read.Window.Width = xmax - xmin + 1;
    read.Window.Height = ymax - ymin + 1;

    return read;
}

// Converted from: vips__openexr_istiled

public static bool IsTiled(string filename)
{
    Read read;
    if ((read = ReadNew(filename, null)) == null) return false;
    var tiled = read.Tiles != null;
    ReadDestroy(null, read);
    return tiled;
}

// Converted from: read_header

private static void ReadHeader(Read read, VipsImage out)
{
    VipsDemandStyle hint;

    // FIXME ... not really scRGB, you should get the chromaticities
    // from the header and transform
    vips_image_init_fields(out,
        read.Window.Width, read.Window.Height, 4,
        VIPS_FORMAT_FLOAT,
        VIPS_CODING_NONE, VIPS_INTERPRETATION_scRGB, 1.0, 1.0);

    if (read.Tiles != null)
        // Even though this is a tiled reader, we hint thinstrip
        // since with the cache we are quite happy serving that if
        // anything downstream would like it.
        hint = VIPS_DEMAND_STYLE_THINSTRIP;
    else
        hint = VIPS_DEMAND_STYLE_FATSTRIP;
    vips_image_pipelinev(out, hint, null);
}

// Converted from: vips__openexr_read_header

public static int ReadHeader(string filename, VipsImage out)
{
    Read read;

    if ((read = ReadNew(filename, out)) == null) return -1;
    ReadHeader(read, out);
    ReadClose(read);

    return 0;
}

// Converted from: vips__openexr_start

private static IntPtr Start(VipsImage out, IntPtr a, IntPtr b)
{
    Read read = (Read)a;
    Imf.Rgba[] imf_buffer;

    if ((imf_buffer = new Imf.Rgba[read.TileWidth * read.TileHeight]) == null) return IntPtr.Zero;

    return Marshal.AllocHGlobal(imf_buffer.Length * sizeof(Imf.Rgba));
}

// Converted from: vips__openexr_generate

private static int Generate(VipsRegion out,
    IntPtr seq, IntPtr a, IntPtr b, ref bool top)
{
    Imf.Rgba[] imf_buffer = (Imf.Rgba[])Marshal.PtrToStructure(seq, typeof(Imf.Rgba[]));
    Read read = (Read)a;
    VipsRect r = out.Valid;

    const int tw = read.TileWidth;
    const int th = read.TileHeight;

    // Find top left of tiles we need.
    const int xs = (r.Left / tw) * tw;
    const int ys = (r.Top / th) * th;

    int x, y, z;
    VipsRect image;

    // Area of image.
    image.Left = 0;
    image.Top = 0;
    image.Width = read.Out.Xsize;
    image.Height = read.Out.Ysize;

    for (y = ys; y < VIPS_RECT_BOTTOM(r); y += th)
        for (x = xs; x < VIPS_RECT_RIGHT(r); x += tw)
        {
            VipsRect tile;
            VipsRect hit;
            int result;

            if (!Imf.TiledInputSetFrameBuffer(read.Tiles,
                    imf_buffer -
                        (read.Window.Left + x) -
                        (read.Window.Top + y) * tw,
                    1, tw))
            {
                vips_foreign_load_invalidate(read.Out);
                GetImfError();
                return -1;
            }

#ifdef DEBUG
            Console.WriteLine("exr2vips: requesting tile " + x / tw + " x " + y / th);
#endif /*DEBUG*/

            result = Imf.TiledInputReadTile(read.Tiles,
                x / tw, y / th, 0, 0);

            if (!result)
            {
                GetImfError();
                return -1;
            }

            // The tile in the file, in VIPS coordinates.
            tile.Left = x;
            tile.Top = y;
            tile.Width = tw;
            tile.Height = th;
            vips_rect_intersectrect(&tile, &image, &tile);

            // The part of this tile that hits the region.
            vips_rect_intersectrect(&tile, r, &hit);

            // Convert to float and write to the region.
            for (z = 0; z < hit.Height; z++)
            {
                Imf.Rgba* p = imf_buffer +
                    (hit.Left - tile.Left) +
                    (hit.Top - tile.Top + z) * tw;
                float[] q = new float[4];
                Imf.HalfToFloatArray(4 * hit.Width,
                    (Imf.Half*)p, q);

                vips_image_write_region(out, hit.Left, hit.Top + z,
                    (Vips.Pel*)q);
            }
        }

    return 0;
}

// Converted from: vips__openexr_read

public static int Read(string filename, VipsImage out)
{
    Read read;

    if ((read = ReadNew(filename, out)) == null) return -1;

    if (read.Tiles != null)
    {
        VipsImage raw;
        VipsImage t;

        // Tile cache: keep enough for two complete rows of tiles,
        // plus 50%.
        raw = new VipsImage();
        vips_object_local(out, raw);

        ReadHeader(read, raw);

        if (vips_image_generate(raw,
                Start, Generate, null,
                read, null))
            return -1;

        if (vips_tilecache(raw, out,
                "tile_width", read.TileWidth,
                "tile_height", read.TileHeight,
                "max_tiles", 2.5 * (1 + raw.Xsize / read.TileWidth),
                null))
            return -1;
    }
    else
    {
        const int left = read.Window.Left;
        const int top = read.Window.Top;
        const int width = read.Window.Width;
        const int height = read.Window.Height;

        Imf.Rgba[] imf_buffer;
        float[] vips_buffer;
        int y;

        if ((imf_buffer = new Imf.Rgba[width]) == null ||
            (vips_buffer = new float[4 * width]) == null)
            return -1;

        ReadHeader(read, out);

        for (y = 0; y < height; y++)
        {
            if (!Imf.InputSetFrameBuffer(read.Lines,
                    imf_buffer -
                        left -
                        (top + y) * width,
                    1, width))
            {
                GetImfError();
                return -1;
            }
            if (!Imf.InputReadPixels(read.Lines,
                    top + y, top + y))
            {
                GetImfError();
                return -1;
            }

            Imf.HalfToFloatArray(4 * width,
                (Imf.Half*)imf_buffer, vips_buffer);

            vips_image_write_line(out, y,
                (Vips.Pel*)vips_buffer);
        }
    }

    ReadClose(read);

    return 0;
}
```

This code uses the `System.Runtime.InteropServices` namespace to interact with native libraries and structures. The `Imf` class is assumed to be a wrapper around the OpenEXR library, providing methods for reading and writing EXR files.

Note that this implementation does not include any error handling or debugging statements from the original C code. You may need to add these depending on your specific requirements.