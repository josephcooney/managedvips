Here is the converted C# code:

```csharp
// Read a Analyze file. Old-style header (so called 7.5 format).
//
// 3/8/05
// 	- dbh.h header from Ralph Myers
// 22/8/05
// 	- better byteswapper
// 12/5/09
//	- fix signed/unsigned warning
// 13/1/09
// 	- try harder not to generate error messages in "isanalyze"
// 4/2/10
// 	- gtkdoc
// 14/12/11
// 	- redo as a set of fns ready for wrapping in a new-style class

using System;
using System.IO;

public enum Type { BYTE, SHORT, INT, FLOAT, STRING }

public struct Field {
    public string name; // Eg. "header_key.sizeof_hdr"
    public Type type;
    public int offset; // Offset in struct
    public int len;     // Sizeof ... useful for string types
}

static readonly Field[] dsr_header = new Field[] {
    { "dsr-header_key.sizeof_hdr", Type.INT, 0, 4 },
    { "dsr-header_key.data_type", Type.STRING, 4, 10 },
    { "dsr-header_key.db_name", Type.STRING, 14, 18 },
    { "dsr-header_key.extents", Type.INT, 32, 4 },
    { "dsr-header_key.session_error", Type.SHORT, 36, 2 },
    { "dsr-header_key.regular", Type.BYTE, 38, 1 },
    { "dsr-header_key.hkey_un0", Type.BYTE, 39, 1 },

    { "dsr-image_dimension.dim[0]", Type.SHORT, 40, 2 },
    { "dsr-image_dimension.dim[1]", Type.SHORT, 42, 2 },
    { "dsr-image_dimension.dim[2]", Type.SHORT, 44, 2 },
    { "dsr-image_dimension.dim[3]", Type.SHORT, 46, 2 },
    { "dsr-image_dimension.dim[4]", Type.SHORT, 48, 2 },
    { "dsr-image_dimension.dim[5]", Type.SHORT, 50, 2 },
    { "dsr-image_dimension.dim[6]", Type.SHORT, 52, 2 },
    { "dsr-image_dimension.dim[7]", Type.SHORT, 54, 2 },
    { "dsr-image_dimension.vox_units[0]", Type.BYTE, 56, 1 },
    { "dsr-image_dimension.vox_units[1]", Type.BYTE, 57, 1 },
    { "dsr-image_dimension.vox_units[2]", Type.BYTE, 58, 1 },
    { "dsr-image_dimension.vox_units[3]", Type.BYTE, 59, 1 },
    { "dsr-image_dimension.cal_units[0]", Type.BYTE, 60, 1 },
    { "dsr-image_dimension.cal_units[1]", Type.BYTE, 61, 1 },
    { "dsr-image_dimension.cal_units[2]", Type.BYTE, 62, 1 },
    { "dsr-image_dimension.cal_units[3]", Type.BYTE, 63, 1 },
    { "dsr-image_dimension.cal_units[4]", Type.BYTE, 64, 1 },
    { "dsr-image_dimension.cal_units[5]", Type.BYTE, 65, 1 },
    { "dsr-image_dimension.cal_units[6]", Type.BYTE, 66, 1 },
    { "dsr-image_dimension.cal_units[7]", Type.BYTE, 67, 1 },
    { "dsr-image_dimension.data_type", Type.SHORT, 68, 2 },
    { "dsr-image_dimension.bitpix", Type.SHORT, 70, 2 },
    { "dsr-image_dimension.dim_un0", Type.SHORT, 72, 2 },
    { "dsr-image_dimension.pixdim[0]", Type.FLOAT, 74, 4 },
    { "dsr-image_dimension.pixdim[1]", Type.FLOAT, 78, 4 },
    { "dsr-image_dimension.pixdim[2]", Type.FLOAT, 82, 4 },
    { "dsr-image_dimension.pixdim[3]", Type.FLOAT, 86, 4 },
    { "dsr-image_dimension.pixdim[4]", Type.FLOAT, 90, 4 },
    { "dsr-image_dimension.pixdim[5]", Type.FLOAT, 94, 4 },
    { "dsr-image_dimension.pixdim[6]", Type.FLOAT, 98, 4 },
    { "dsr-image_dimension.pixdim[7]", Type.FLOAT, 102, 4 },
    { "dsr-image_dimension.vox_offset", Type.FLOAT, 106, 4 },
    { "dsr-image_dimension.cal_max", Type.FLOAT, 110, 4 },
    { "dsr-image_dimension.cal_min", Type.FLOAT, 114, 4 },
    { "dsr-image_dimension.compressed", Type.INT, 118, 4 },
    { "dsr-image_dimension.verified", Type.INT, 122, 4 },
    { "dsr-image_dimension.glmax", Type.INT, 126, 4 },
    { "dsr-image_dimension.glmin", Type.INT, 130, 4 },

    { "dsr-data_history.descrip", Type.STRING, 134, 80 },
    { "dsr-data_history.aux_file", Type.STRING, 214, 24 },
    { "dsr-data_history.orient", Type.BYTE, 238, 1 },
    { "dsr-data_history.originator", Type.STRING, 239, 10 },
    { "dsr-data_history.generated", Type.STRING, 249, 10 },
    { "dsr-data_history.scannum", Type.STRING, 259, 10 },
    { "dsr-data_history.patient_id", Type.STRING, 269, 10 },
    { "dsr-data_history.exp_date", Type.STRING, 279, 10 },
    { "dsr-data_history.exp_time", Type.STRING, 289, 10 },
    { "dsr-data_history.hist_un0", Type.STRING, 299, 3 },
    { "dsr-data_history.views", Type.INT, 302, 4 },
    { "dsr-data_history.vols_added", Type.INT, 306, 4 },
    { "dsr-data_history.start_field", Type.INT, 310, 4 },
    { "dsr-data_history.field_skip", Type.INT, 314, 4 },
    { "dsr-data_history.omax", Type.INT, 318, 4 },
    { "dsr-data_history.omin", Type.INT, 322, 4 },
    { "dsr-data_history.smax", Type.INT, 326, 4 },
    { "dsr-data_history.smin", Type.INT, 330, 4 }
};

public static void GenerateFilenames(string path, out string header, out string image)
{
    const string[] olds = new string[] { ".img", ".hdr" };
    vips__change_suffix(path, ref header, FILENAME_MAX, ".hdr", olds, 2);
    vips__change_suffix(path, ref image, FILENAME_MAX, ".img", olds, 2);
}

public static char[] GetStr(int mx, string str)
{
    char[] buf = new char[mx];
    int i;
    for (i = 0; i < mx && str[i] != '\0'; i++)
        buf[i] = str[i];

    // How annoying, patient_id has some funny ctrlchars in that mess up
    // xml encode later.
    for (i = 0; i < mx && buf[i] != '\0'; i++)
        if (!isascii(buf[i]) || buf[i] < 32)
            buf[i] = '@';

    return buf;
}

public static void PrintDsr(struct dsr d)
{
    int i;
    for (i = 0; i < VIPS_NUMBER(dsr_header); i++) {
        Console.WriteLine($"{dsr_header[i].name} = {GetDsrValue(d, dsr_header[i])}");
    }
}

private static object GetDsrValue(struct dsr d, Field field)
{
    switch (field.type) {
        case Type.BYTE:
            return G_STRUCT_MEMBER(char, d, field.offset);
        case Type.SHORT:
            return G_STRUCT_MEMBER(short, d, field.offset);
        case Type.INT:
            return G_STRUCT_MEMBER(int, d, field.offset);
        case Type.FLOAT:
            return G_STRUCT_MEMBER(float, d, field.offset);
        case Type.STRING:
            return GetStr(field.len, &G_STRUCT_MEMBER(char, d, field.offset));
        default:
            throw new Exception("Invalid field type");
    }
}

public static struct dsr ReadHeader(string header)
{
    using (FileStream file = File.OpenRead(header))
    {
        byte[] data = new byte[sizeof(struct dsr)];
        if (!file.Read(data, 0, sizeof(struct dsr)))
            return default;

        // Ouch! Should check at configure time I guess.
        if (sizeof(struct dsr) != 348)
            throw new Exception("Invalid header size");

        struct dsr d = *Marshal.PtrToStructure<struct dsr>(IntPtr.Zero, data);

        // dsr headers are always SPARC byte order (MSB first). Do we need to
        // swap?
        if (!vips_amiMSBfirst()) {
            int i;
            for (i = 0; i < VIPS_NUMBER(dsr_header); i++) {
                unsigned char* p = &G_STRUCT_MEMBER(unsigned char, d,
                    dsr_header[i].offset);
                vips__copy_2byte(true, p, p);
            }
        }

        return d;
    }
}

public static int GetVipsProperties(struct dsr d, out int width, out int height, out int bands, out VipsBandFormat fmt)
{
    if (d.dime.dim[0] < 2 || d.dime.dim[0] > 7) {
        throw new Exception($"Invalid dimension {d.dime.dim[0]}");
    }

    // Size of base 2d images.
    width = d.dime.dim[1];
    height = d.dime.dim[2];

    for (int i = 3; i <= d.dime.dim[0]; i++)
        height *= d.dime.dim[i];

    // Check it's a datatype we can handle.
    switch (d.dime.datatype) {
        case DT_UNSIGNED_CHAR:
            bands = 1;
            fmt = VipsBandFormat.UCHAR;
            break;
        case DT_SIGNED_SHORT:
            bands = 1;
            fmt = VipsBandFormat.SHORT;
            break;
        case DT_SIGNED_INT:
            bands = 1;
            fmt = VipsBandFormat.INT;
            break;
        case DT_FLOAT:
            bands = 1;
            fmt = VipsBandFormat.FLOAT;
            break;
        case DT_COMPLEX:
            bands = 1;
            fmt = VipsBandFormat.COMPLEX;
            break;
        case DT_DOUBLE:
            bands = 1;
            fmt = VipsBandFormat.DOUBLE;
            break;
        case DT_RGB:
            bands = 3;
            fmt = VipsBandFormat.UCHAR;
            break;
        default:
            throw new Exception($"Unsupported datatype {d.dime.datatype}");
    }

    return 0;
}

public static void AttachMeta(VipsImage out, struct dsr d)
{
    foreach (Field field in dsr_header) {
        switch (field.type) {
            case Type.BYTE:
                vips_image_set_int(out, field.name,
                    G_STRUCT_MEMBER(char, d, field.offset));
                break;
            case Type.SHORT:
                vips_image_set_int(out, field.name,
                    G_STRUCT_MEMBER(short, d, field.offset));
                break;
            case Type.INT:
                vips_image_set_int(out, field.name,
                    G_STRUCT_MEMBER(int, d, field.offset));
                break;
            case Type.FLOAT:
                vips_image_set_double(out, field.name,
                    G_STRUCT_MEMBER(float, d, field.offset));
                break;
            case Type.STRING:
                vips_image_set_string(out, field.name,
                    GetStr(field.len,
                        &G_STRUCT_MEMBER(char, d, field.offset)));
                break;
        }
    }
}

public static int IsAnalyze(string filename)
{
    string header = null;
    string image = null;
    GenerateFilenames(filename, out header, out image);
    if (!File.Exists(header))
        return 0;

    struct dsr d = ReadHeader(header);
    try {
        int width, height, bands;
        VipsBandFormat fmt;
        GetVipsProperties(d, out width, out height, out bands, out fmt);
        // ...
    } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        return 0;
    }

    return 1;
}

public static int ReadHeader(string filename, VipsImage out)
{
    string header = null;
    string image = null;
    GenerateFilenames(filename, out header, out image);
    if (!File.Exists(header))
        return -1;

    struct dsr d = ReadHeader(header);
    try {
        int width, height, bands;
        VipsBandFormat fmt;
        GetVipsProperties(d, out width, out height, out bands, out fmt);

        vips_image_init_fields(out,
            width, height, bands, fmt,
            VIPS_CODING_NONE,
            bands == 1
                ? VIPS_INTERPRETATION_B_W
                : VIPS_INTERPRETATION_sRGB,
            1.0, 1.0);

        AttachMeta(out, d);
    } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        return -1;
    }

    return 0;
}

public static int Read(string filename, VipsImage out)
{
    string header = null;
    string image = null;
    GenerateFilenames(filename, out header, out image);
    if (!File.Exists(header))
        return -1;

    struct dsr d = ReadHeader(header);
    try {
        AttachMeta(out, d);

        int width, height, bands;
        VipsBandFormat fmt;
        GetVipsProperties(d, out width, out height, out bands, out fmt);

        // ...
    } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        return -1;
    }

    return 0;
}
```