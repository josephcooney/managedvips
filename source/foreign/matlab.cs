Here is the converted C# code:

```csharp
// read_new
public static Read* read_new(const string filename, VipsImage out)
{
    Read* read;

    if (!(read = new Read()))
        return null;

    read.filename = vips_strdup(filename);
    read.out = out;
    read.mat = null;
    read.var = null;

    if (!(read.mat = Mat_Open(filename, MAT_ACC_RDONLY)))
    {
        vips_error("mat2vips", $"unable to open \"{filename}\"");
        read_destroy(read);
        return null;
    }

    for (;;)
    {
        if (!(read.var = Mat_VarReadNextInfo(read.mat)))
        {
            vips_error("mat2vips", $"no matrix variables in \"{filename}\"");
            read_destroy(read);
            return null;
        }

#ifdef DEBUG
        Console.WriteLine($"mat2vips: seen:\n");
        Console.WriteLine($"var->name == {read.var.name}\n");
        Console.WriteLine($"var->class_type == {read.var.class_type}\n");
        Console.WriteLine($"var->rank == {read.var.rank}\n");
#endif /*DEBUG*/

        // Vector to colour image is OK for us.
        if (read.var.rank >= 1 && read.var.rank <= 3)
            break;

        VIPS_FREEF(Mat_VarFree, read.var);
    }

    return read;
}

// mat2vips_formats
private static readonly int[][] mat2vips_formats = new[]
{
    new[] { MAT_C_UINT8, VIPS_FORMAT_UCHAR },
    new[] { MAT_C_INT8, VIPS_FORMAT_CHAR },
    new[] { MAT_C_UINT16, VIPS_FORMAT_USHORT },
    new[] { MAT_C_INT16, VIPS_FORMAT_SHORT },
    new[] { MAT_C_UINT32, VIPS_FORMAT_UINT },
    new[] { MAT_C_INT32, VIPS_FORMAT_INT },
    new[] { MAT_C_SINGLE, VIPS_FORMAT_FLOAT },
    new[] { MAT_C_DOUBLE, VIPS_FORMAT_DOUBLE }
};

// mat2vips_pick_interpretation
private static VipsInterpretation mat2vips_pick_interpretation(int bands, VipsBandFormat format)
{
    if (bands == 3 && vips_band_format_is8bit(format))
        return VIPS_INTERPRETATION_sRGB;
    if (bands == 3 && (format == VIPS_FORMAT_USHORT || format == VIPS_FORMAT_SHORT))
        return VIPS_INTERPRETATION_RGB16;
    if (bands == 1 && (format == VIPS_FORMAT_USHORT || format == VIPS_FORMAT_SHORT))
        return VIPS_INTERPRETATION_GREY16;
    if (bands > 1)
        return VIPS_INTERPRETATION_MULTIBAND;

    return VIPS_INTERPRETATION_MULTIBAND;
}

// mat2vips_get_header
private static int mat2vips_get_header(matvar_t var, VipsImage im)
{
    int width = 1;
    int height = 1;
    int bands = 1;
    VipsBandFormat format;
    VipsInterpretation interpretation;
    int i;

    switch (var.rank)
    {
        case 3:
            bands = var.dims[2];

        case 2:
            width = var.dims[1];

        case 1:
            height = var.dims[0];
            break;

        default:
            vips_error("mat2vips", $"unsupported rank {var.rank}");
            return -1;
    }

    for (i = 0; i < mat2vips_formats.Length; i++)
        if (mat2vips_formats[i][0] == var.class_type)
            break;

    if (i == mat2vips_formats.Length)
    {
        vips_error("mat2vips", $"unsupported class type {var.class_type}");
        return -1;
    }

    format = mat2vips_formats[i][1];
    interpretation = mat2vips_pick_interpretation(bands, format);

    vips_image_init_fields(im,
        width, height, bands,
        format,
        VIPS_CODING_NONE, interpretation, 1.0, 1.0);

    // We read to a huge memory area.
    if (vips_image_pipelinev(im, VIPS_DEMAND_STYLE_ANY, null))
        return -1;

    return 0;
}

// vips__mat_header
public static int vips__mat_header(string filename, VipsImage out)
{
    Read* read;

#ifdef DEBUG
    Console.WriteLine($"mat2vips_header: reading \"{filename}\"");
#endif /*DEBUG*/

    if (!(read = read_new(filename, out)))
        return -1;
    if (mat2vips_get_header(read.var, read.out))
    {
        read_destroy(read);
        return -1;
    }
    read_destroy(read);

    return 0;
}

// mat2vips_get_data
private static int mat2vips_get_data(mat_t mat, matvar_t var, VipsImage im)
{
    int y;
    VipsPel* buffer;
    const int es = VIPS_IMAGE_SIZEOF_ELEMENT(im);

    // Matlab images are plane-separate, so we have to assemble bands in
    // image-size chunks.
    const guint64 is = es * VIPS_IMAGE_N_PELS(im);

    if (Mat_VarReadDataAll(mat, var))
    {
        vips_error("mat2vips", "%s");
        return -1;
    }

    // Matlab images are in columns, so we have to transpose into
    // scanlines with this buffer.
    if (!(buffer = VIPS_ARRAY(im,
              VIPS_IMAGE_SIZEOF_LINE(im), VipsPel)))
        return -1;

    for (y = 0; y < im.Ysize; y++)
    {
        VipsPel* p = (VipsPel*)var.data + y * es;
        int x;
        VipsPel* q;

        q = buffer;
        for (x = 0; x < im.Xsize; x++)
        {
            int b;

            for (b = 0; b < im.Bands; b++)
            {
                VipsPel* p2 = p + b * is;
                int z;

                for (z = 0; z < es; z++)
                    q[z] = p2[z];

                q += es;
            }

            p += es * im.Ysize;
        }

        if (vips_image_write_line(im, y, buffer))
            return -1;
    }

    return 0;
}

// vips__mat_load
public static int vips__mat_load(string filename, VipsImage out)
{
    Read* read;

#ifdef DEBUG
    Console.WriteLine($"mat2vips: reading \"{filename}\"");
#endif /*DEBUG*/

    if (!(read = read_new(filename, out)))
        return -1;
    if (mat2vips_get_header(read.var, read.out) || mat2vips_get_data(read.mat, read.var, read.out))
    {
        read_destroy(read);
        return -1;
    }
    read_destroy(read);

    return 0;
}

// vips__mat_ismat
public static bool vips__mat_ismat(string filename)
{
    var buf = new byte[15];

    if (vips_get_bytes(filename, buf, 10) == 10 && vips_isprefix("MATLAB 5.0", buf))
        return true;

    return false;
}

// vips__mat_suffs
private static readonly string[] vips__mat_suffs = new[] { ".mat", null };
```