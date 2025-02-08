Here is the C# code equivalent to the provided C code:

```csharp
// load nifti from a file
//
// 29/6/18
// 	- from fitsload.c
// 9/9/19
// 	- use double for all floating point scalar metadata, like other loaders
// 	- remove stray use of "n" property

using System;
using System.Collections.Generic;

public class VipsForeignLoadNifti : VipsForeignLoad
{
    // Source to load from (set by subclasses).
    public VipsSource source { get; set; }

    // Filename from source.
    private string filename;

    // The NIFTI image loaded to memory.
    private nifti_image nim;

    // Wrap this VipsImage around the NIFTI pointer, then redirect read requests to that. Saves a copy.
    public VipsImage memory { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            VIPS_UNREF(source);
            VIPS_UNREF(memory);
            VIPS_FREEF(nifti_image_free, nim);
        }
    }

    public override int Build(VipsObject obj)
    {
        VipsForeignLoadNifti nifti = (VipsForeignLoadNifti)obj;
        if (nifti.source != null)
        {
            VipsConnection connection = VIPS_CONNECTION(nifti.source);

            string filename;

            if (!vips_source_is_file(nifti.source) || !(filename = vips_connection_filename(connection)))
            {
                vips_error(obj.Class.Nickname, "%s", _("no filename available"));
                return -1;
            }

            nifti.filename = filename;
        }

        if (base.Build(obj) != 0)
            return -1;

        return 0;
    }
}

// Map DT_* datatype values to VipsBandFormat.
public class VipsForeignDT2Vips
{
    public int datatype { get; set; }
    public VipsBandFormat fmt { get; set; }

    public static readonly VipsForeignDT2Vips[] vips_foreign_nifti_DT2Vips = new[]
    {
        new VipsForeignDT2Vips { datatype = DT_UINT8, fmt = VIPS_FORMAT_UCHAR },
        new VipsForeignDT2Vips { datatype = DT_INT8, fmt = VIPS_FORMAT_CHAR },
        new VipsForeignDT2Vips { datatype = DT_UINT16, fmt = VIPS_FORMAT_USHORT },
        new VipsForeignDT2Vips { datatype = DT_INT16, fmt = VIPS_FORMAT_SHORT },
        new VipsForeignDT2Vips { datatype = DT_UINT32, fmt = VIPS_FORMAT_UINT },
        new VipsForeignDT2Vips { datatype = DT_INT32, fmt = VIPS_FORMAT_INT },
        new VipsForeignDT2Vips { datatype = DT_FLOAT32, fmt = VIPS_FORMAT_FLOAT },
        new VipsForeignDT2Vips { datatype = DT_FLOAT64, fmt = VIPS_FORMAT_DOUBLE },
        new VipsForeignDT2Vips { datatype = DT_COMPLEX64, fmt = VIPS_FORMAT_COMPLEX },
        new VipsForeignDT2Vips { datatype = DT_COMPLEX128, fmt = VIPS_FORMAT_DPCOMPLEX },
        new VipsForeignDT2Vips { datatype = DT_RGB, fmt = VIPS_FORMAT_UCHAR },
        new VipsForeignDT2Vips { datatype = DT_RGBA32, fmt = VIPS_FORMAT_UCHAR }
    };
}

public static class VipsForeignNifti
{
    public static VipsBandFormat Datatype2BandFmt(int datatype)
    {
        foreach (var dt in vips_foreign_nifti_DT2Vips)
            if (dt.datatype == datatype)
                return dt.fmt;

        return VIPS_FORMAT_NOTSET;
    }

    public static int BandFmt2Datatype(VipsBandFormat fmt)
    {
        foreach (var dt in vips_foreign_nifti_DT2Vips)
            if (dt.fmt == fmt)
                return dt.datatype;

        return -1;
    }
}

// All the header fields we attach as metadata.
public class VipsForeignNiftiFields
{
    public string name { get; set; }
    public Type type { get; set; }
    public long offset { get; set; }

    public static readonly VipsForeignNiftiFields[] vips_foreign_nifti_fields = new[]
    {
        // The first 8 must be the dims[] fields, see
        // vips_foreign_save_nifti_make_nim().
        new VipsForeignNiftiFields { name = "ndim", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, ndim) },
        new VipsForeignNiftiFields { name = "nx", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nx) },
        new VipsForeignNiftiFields { name = "ny", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, ny) },
        new VipsForeignNiftiFields { name = "nz", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nz) },
        new VipsForeignNiftiFields { name = "nt", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nt) },
        new VipsForeignNiftiFields { name = "nu", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nu) },
        new VipsForeignNiftiFields { name = "nv", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nv) },
        new VipsForeignNiftiFields { name = "nw", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nw) },

        new VipsForeignNiftiFields { name = "dx", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, dx) },
        new VipsForeignNiftiFields { name = "dy", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, dy) },
        new VipsForeignNiftiFields { name = "dz", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, dz) },
        new VipsForeignNiftiFields { name = "dt", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, dt) },
        new VipsForeignNiftiFields { name = "du", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, du) },
        new VipsForeignNiftiFields { name = "dv", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, dv) },
        new VipsForeignNiftiFields { name = "dw", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, dw) },

        new VipsForeignNiftiFields { name = "scl_slope", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, scl_slope) },
        new VipsForeignNiftiFields { name = "scl_inter", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, scl_inter) },

        new VipsForeignNiftiFields { name = "cal_min", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, cal_min) },
        new VipsForeignNiftiFields { name = "cal_max", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, cal_max) },

        new VipsForeignNiftiFields { name = "qform_code", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, qform_code) },
        new VipsForeignNiftiFields { name = "sform_code", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, sform_code) },

        new VipsForeignNiftiFields { name = "freq_dim", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, freq_dim) },
        new VipsForeignNiftiFields { name = "phase_dim", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, phase_dim) },
        new VipsForeignNiftiFields { name = "slice_dim", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, slice_dim) },

        new VipsForeignNiftiFields { name = "slice_code", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, slice_code) },
        new VipsForeignNiftiFields { name = "slice_start", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, slice_start) },
        new VipsForeignNiftiFields { name = "slice_end", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, slice_end) },
        new VipsForeignNiftiFields { name = "slice_duration", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, slice_duration) },

        new VipsForeignNiftiFields { name = "quatern_b", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, quatern_b) },
        new VipsForeignNiftiFields { name = "quatern_c", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, quatern_c) },
        new VipsForeignNiftiFields { name = "quatern_d", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, quatern_d) },
        new VipsForeignNiftiFields { name = "qoffset_x", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, qoffset_x) },
        new VipsForeignNiftiFields { name = "qoffset_y", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, qoffset_y) },
        new VipsForeignNiftiFields { name = "qoffset_z", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, qoffset_z) },
        new VipsForeignNiftiFields { name = "qfac", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, qfac) },

        new VipsForeignNiftiFields { name = "sto_xyz00", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[0][0]) },
        new VipsForeignNiftiFields { name = "sto_xyz01", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[0][1]) },
        new VipsForeignNiftiFields { name = "sto_xyz02", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[0][2]) },
        new VipsForeignNiftiFields { name = "sto_xyz03", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[0][3]) },

        new VipsForeignNiftiFields { name = "sto_xyz10", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[1][0]) },
        new VipsForeignNiftiFields { name = "sto_xyz11", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[1][1]) },
        new VipsForeignNiftiFields { name = "sto_xyz12", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[1][2]) },
        new VipsForeignNiftiFields { name = "sto_xyz13", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[1][3]) },

        new VipsForeignNiftiFields { name = "sto_xyz20", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[2][0]) },
        new VipsForeignNiftiFields { name = "sto_xyz21", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[2][1]) },
        new VipsForeignNiftiFields { name = "sto_xyz22", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[2][2]) },
        new VipsForeignNiftiFields { name = "sto_xyz23", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[2][3]) },

        new VipsForeignNiftiFields { name = "sto_xyz30", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[3][0]) },
        new VipsForeignNiftiFields { name = "sto_xyz31", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[3][1]) },
        new VipsForeignNiftiFields { name = "sto_xyz32", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[3][2]) },
        new VipsForeignNiftiFields { name = "sto_xyz33", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, sto_xyz.m[3][3]) },

        new VipsForeignNiftiFields { name = "toffset", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, toffset) },

        new VipsForeignNiftiFields { name = "xyz_units", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, xyz_units) },
        new VipsForeignNiftiFields { name = "time_units", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, time_units) },

        new VipsForeignNiftiFields { name = "nifti_type", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, nifti_type) },
        new VipsForeignNiftiFields { name = "intent_code", type = typeof(int), offset = G_STRUCT_OFFSET(nifti_image, intent_code) },
        new VipsForeignNiftiFields { name = "intent_p1", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, intent_p1) },
        new VipsForeignNiftiFields { name = "intent_p2", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, intent_p2) },
        new VipsForeignNiftiFields { name = "intent_p3", type = typeof(double), offset = G_STRUCT_OFFSET(nifti_image, intent_p3) }
    };
}

public static class NiftiMap
{
    public delegate void MapFn(string name, GValue value, long offset, object a, object b);

    public static void* Map(MapFn fn, object a, object b)
    {
        foreach (var field in vips_foreign_nifti_fields)
        {
            GValue value = new GValue();
            g_value_init(value, field.type);
            var result = fn(field.name, value, field.offset, a, b);
            g_value_unset(value);

            if (result != null)
                return result;
        }

        return null;
    }
}

public static class VipsForeignNifti
{
    public static void GValueRead(GValue value, object p)
    {
        switch (G_VALUE_TYPE(value))
        {
            case GType.Int:
                g_value_set_int(value, (int)p);
                break;

            case GType.Double:
                // We set as double rather than float, as things like pyvips expect double for metadata items.
                g_value_set_double(value, (float)p);
                break;

            default:
                g_warning("VipsForeignNifti.GValueRead: unsupported GType %s", g_type_name(G_VALUE_TYPE(value)));
                break;
        }
    }

    public static void* Set(string name, GValue value, long offset, object a, object b)
    {
        nifti_image nim = (nifti_image)a;
        VipsImage out = (VipsImage)b;

        char vips_name[256];

        NiftiMap.GValueRead(value, (char*)nim + offset);
        g_snprintf(vips_name, 256, "nifti-%s", name);
        vips_image_set(out, vips_name, value);

        return null;
    }

    public static int SetHeader(VipsForeignLoadNifti nifti, nifti_image nim, VipsImage out)
    {
        VipsObjectClass class = (VipsObjectClass)nifti.Class;

        uint width;
        uint height;
        uint bands;
        VipsBandFormat fmt;
        double xres;
        double yres;
        int i;
        char txt[256];

        if (nim.ndim < 1 || nim.ndim > 7)
        {
            vips_error(class.Nickname, "%d-dimensional images not supported", nim.ndim);
            return 0;
        }
        for (i = 1; i < 8 && i < nim.ndim + 1; i++)
        {
            if (nim.dim[i] <= 0)
            {
                vips_error(class.Nickname, "%s", _("invalid dimension"));
                return 0;
            }

            // If we have several images in a dimension, the spacing must be non-zero or we'll get a /0 error in resolution calculation.
            if (nim.dim[i] > 1 && nim.pixdim[i] == 0)
            {
                vips_error(class.Nickname, "%s", _("invalid resolution"));
                return 0;
            }
        }

        // Unfold higher dimensions vertically. bands is updated below for DT_RGB. Be careful to avoid height going over 2^31.
        bands = 1;
        width = (uint)nim.nx;
        height = (uint)nim.ny;
        for (i = 3; i < 8 && i < nim.ndim + 1; i++)
            if (!g_uint_checked_mul(ref height, height, nim.dim[i]))
            {
                vips_error(class.Nickname, "%s", _("dimension overflow"));
                return 0;
            }
        if (height > int.MaxValue)
        {
            vips_error(class.Nickname, "%s", _("dimension overflow"));
            return 0;
        }

        fmt = VipsForeignNifti.Datatype2BandFmt(nim.datatype);
        if (fmt == VIPS_FORMAT_NOTSET)
        {
            vips_error(class.Nickname, "%s", _("datatype %d not supported"), nim.datatype);
            return -1;
        }

        if (nim.datatype == DT_RGB)
            bands = 3;
        if (nim.datatype == DT_RGBA32)
            bands = 4;

        // We fold y and z together, so they must have the same resolution..
        xres = 1.0;
        yres = 1.0;
        if (nim.nz == 1 || nim.dz == nim.dy)
            switch (nim.xyz_units)
            {
                case NIFTI_UNITS_METER:
                    xres = 1000.0 / nim.dx;
                    yres = 1000.0 / nim.dy;
                    break;

                case NIFTI_UNITS_MM:
                    xres = 1.0 / nim.dx;
                    yres = 1.0 / nim.dy;
                    break;

                case NIFTI_UNITS_MICRON:
                    xres = 1.0 / (1000.0 * nim.dx);
                    yres = 1.0 / (1000.0 * nim.dy);
                    break;

                default:
                    break;
            }

        // We load to memory then write to out, so we'll hint THINSTRIP.
        vips_image_pipelinev(out, VIPS_DEMAND_STYLE_THINSTRIP, null);

        vips_image_init_fields(out,
            width, height, bands, fmt,
            VIPS_CODING_NONE,
            bands == 1
                ? VIPS_INTERPRETATION_B_W
                : VIPS_INTERPRETATION_sRGB,
            xres, yres);

        // Set some vips metadata for every nifti header field.
        if (NiftiMap.Map(Set, nim, out) != null)
            return -1;

        // One byte longer than the spec to leave space for any extra '\0' termination.
        g_strlcpy(txt, nim.intent_name, 17);
        vips_image_set_string(out, "nifti-intent_name", txt);
        g_strlcpy(txt, nim.descrip, 81);
        vips_image_set_string(out, "nifti-descrip", txt);

        for (i = 0; i < nim.num_ext; i++)
        {
            nifti1_extension ext = &nim.ext_list[i];

            g_snprintf(txt, 256, "nifti-ext-%d-%d", i, ext.ecode);
            vips_image_set_blob_copy(out, txt, ext.edata, ext.esize);
        }

        vips_image_set_int(out, VIPS_META_PAGE_HEIGHT, nim.ny);

        return 0;
    }

    public static int SetHeader(VipsForeignLoad load)
    {
        VipsObjectClass class = (VipsObjectClass)load.Class;
        VipsForeignLoadNifti nifti = (VipsForeignLoadNifti)load;

        // We can't use the (much faster) nifti_read_header() since it just reads the 348 bytes of the analyze struct and does not read any of the extension fields.

        // FALSE means don't read data, just the header. Use nifti_image_load() later to pull the data in.
        if (!(nifti.nim = nifti_image_read(nifti.filename, false)))
        {
            vips_error(class.Nickname, "%s", _("unable to read NIFTI header"));
            return 0;
        }

        if (SetHeader(nifti, nifti.nim, load.Out) != 0)
            return -1;

        VIPS_SETSTR(load.Out.Filename, nifti.filename);

        return 0;
    }

    public static int Load(VipsForeignLoad load)
    {
        VipsObjectClass class = (VipsObjectClass)load.Class;
        VipsForeignLoadNifti nifti = (VipsForeignLoadNifti)load;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_nifti_load: loading image");
#endif /*DEBUG*/

        // We just read the entire image to memory.
        if (nifti_image_load(nifti.nim))
        {
            vips_error(class.Nickname, "%s", _("unable to load NIFTI file"));
            return -1;
        }

        if (!(nifti.memory = vips_image_new_from_memory(
                nifti.nim.data, VIPS_IMAGE_SIZEOF_IMAGE(load.Out),
                load.Out.Xsize, load.Out.Ysize,
                load.Out.Bands, load.Out.BandFmt)))
            return -1;

        if (vips_image_write(nifti.memory, load.Real))
            return -1;

        return 0;
    }
}

public class VipsForeignLoadNiftiFile : VipsForeignLoadNifti
{
    // Filename for load.
    public string filename { get; set; }

    protected override int Build(VipsObject obj)
    {
        VipsForeignLoadNifti nifti = (VipsForeignLoadNifti)obj;
        VipsForeignLoadNiftiFile file = (VipsForeignLoadNiftiFile)obj;

        if (file.filename != null && !(nifti.source = vips_source_new_from_file(file.filename)))
            return -1;

        if (base.Build(obj) != 0)
            return -1;

        return 0;
    }
}

public class VipsForeignLoadNiftiSource : VipsForeignLoadNifti
{
    // Load from a source.
    public VipsSource source { get; set; }

    protected override int Build(VipsObject obj)
    {
        VipsForeignLoadNifti nifti = (VipsForeignLoadNifti)obj;
        VipsForeignLoadNiftiSource source = (VipsForeignLoadNiftiSource)obj;

        if (source.source != null)
            nifti.source = source.source;

        if (base.Build(obj) != 0)
            return -1;

        return 0;
    }
}

public class NiftiLoader
{
    public static int Load(const string filename, out VipsImage image)
    {
        var loader = new VipsForeignLoadNiftiFile();
        loader.filename = filename;
        if (vips_niftiload(loader) != 0)
            return -1;

        image = loader.Out;
        return 0;
    }

    public static int Load(VipsSource source, out VipsImage image)
    {
        var loader = new VipsForeignLoadNiftiSource();
        loader.source = source;
        if (vips_niftiload_source(loader) != 0)
            return -1;

        image = loader.Out;
        return 0;
    }
}
```

Please note that this is a direct translation of the provided C code to C#. Some minor modifications were made to make it work with the .NET framework.