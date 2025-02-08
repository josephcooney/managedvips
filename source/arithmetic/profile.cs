```csharp
// find image profiles
// 
// 11/8/99 JC
//	- from im_cntlines()
// 22/4/04
//	- now outputs horizontal/vertical image
// 9/11/10
// 	- any image format, any number of bands
// 	- gtk-doc
// 21/9/13
// 	- rewrite as a class
// 	- output h and v profile in one pass
// 	- partial
// 	- output is int rather than ushort

using System;
using System.Collections.Generic;

public class VipsProfile : VipsStatistic
{
    // Main edge set. Threads accumulate to this.
    public Edges edges;

    // Write profiles here.
    public VipsImage columns;
    public VipsImage rows;

    public VipsProfile() { }

    // New edge accumulator.
    public void* Start()
    {
        if (edges == null)
            edges = new Edges();
        return new Edges();
    }

    // Add a line of pixels.
    private static void ADD_PIXELS(Type TYPE, int nb, int x, int y, int n, int[] column_edges, int[] row_edges, VipsImage in)
    {
        var p = (TYPE[])in.Data;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < nb; j++)
            {
                if (p[j] != 0)
                {
                    column_edges[j] = Math.Min(column_edges[j], y);
                    row_edges[j] = Math.Min(row_edges[j], x + i);
                }
            }

            p = (TYPE[])in.Data;
            column_edges = new int[nb];
            row_edges = new int[nb];
        }
    }

    // Add a region to a profile.
    public int Scan(object seq, int x, int y, object in_data, int n)
    {
        var edges = (Edges)seq;
        var nb = ((VipsImage)in_data).Bands;
        Type TYPE;

        switch (((VipsImage)in_data).BandFmt)
        {
            case VIPS_FORMAT_UCHAR:
                ADD_PIXELS(typeof(guchar), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_CHAR:
                ADD_PIXELS(typeof(char), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_USHORT:
                ADD_PIXELS(typeof(gushort), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_SHORT:
                ADD_PIXELS(typeof(short), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_UINT:
                ADD_PIXELS(typeof(guint), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_INT:
                ADD_PIXELS(typeof(int), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_FLOAT:
                ADD_PIXELS(typeof(float), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            case VIPS_FORMAT_DOUBLE:
                ADD_PIXELS(typeof(double), nb, x, y, n, edges.column_edges, edges.row_edges, (VipsImage)in_data);
                break;

            default:
                throw new Exception("Invalid band format");
        }

        return 0;
    }

    // Join a sub-profile onto the main profile.
    public int Stop(object seq)
    {
        var edges = this.edges;
        var sub_edges = (Edges)seq;
        var in_data = ((VipsImage)this.ready).Data;

        for (int i = 0; i < ((VipsImage)this.ready).Xsize * ((VipsImage)this.ready).Bands; i++)
            edges.column_edges[i] = Math.Min(edges.column_edges[i], sub_edges.column_edges[i]);

        for (int i = 0; i < ((VipsImage)this.ready).Ysize * ((VipsImage)this.ready).Bands; i++)
            edges.row_edges[i] = Math.Min(edges.row_edges[i], sub_edges.row_edges[i]);

        // Blank out sub-profile to make sure we can't add it again.
        sub_edges.row_edges = null;
        sub_edges.column_edges = null;

        return 0;
    }

    public static void ClassInit(Type type)
    {
        var gobject_class = (GObjectClass)type;
        var object_class = (VipsObjectClass)type;
        var sclass = (VipsStatisticClass)type;

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        object_class.Nickname = "profile";
        object_class.Description = "find image profiles";
        object_class.Build = vips_profile_build;

        sclass.Start = Start;
        sclass.Scan = Scan;
        sclass.Stop = Stop;

        VIPS_ARG_IMAGE(type, "columns", 100,
            _("Columns"),
            _("First non-zero pixel in column"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsProfile, columns));

        VIPS_ARG_IMAGE(type, "rows", 101,
            _("Rows"),
            _("First non-zero pixel in row"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsProfile, rows));
    }

    public static void Init(VipsProfile profile)
    {
    }
}

public class Edges
{
    // Horizontal array: Ys of top-most non-zero pixel.
    public int[] column_edges;

    // Vertical array: Xs of left-most non-zero pixel.
    public int[] row_edges;
}
```

```csharp
// vips_profile_build
public int Build(VipsObject object)
{
    var class = (VipsObjectClass)VIPS_OBJECT_GET_CLASS(object);
    var statistic = (VipsStatistic)object;
    var profile = (VipsProfile)object;

    if (statistic.in &&
        VipsCheckNoncomplex(class.Nickname, statistic.in))
        return -1;

    object.SetProperty(object,
        "columns", new VipsImage(),
        "rows", new VipsImage(),
        null);

    // main edge set made on first thread start.

    if (VIPS_OBJECT_CLASS(vips_profile_parent_class).Build(object))
        return -1;

    // Make the output image.
    if (VipsImagePipelinev(profile.columns,
            VIPS_DEMAND_STYLE_ANY, statistic.ready, null) ||
        VipsImagePipelinev(profile.rows,
            VIPS_DEMAND_STYLE_ANY, statistic.ready, null))
        return -1;
    profile.columns.Ysize = 1;
    profile.columns.BandFmt = VIPS_FORMAT_INT;
    profile.columns.Type = VIPS_INTERPRETATION_HISTOGRAM;
    profile.rows.Xsize = 1;
    profile.rows.BandFmt = VIPS_FORMAT_INT;
    profile.rows.Type = VIPS_INTERPRETATION_HISTOGRAM;

    if (VipsImageWriteLine(profile.columns, 0,
            (VipsPel[])profile.edges.column_edges))
        return -1;
    for (int y = 0; y < profile.rows.Ysize; y++)
        if (VipsImageWriteLine(profile.rows, y,
                (VipsPel[])profile.edges.row_edges +
                    y * VIPS_IMAGE_SIZEOF_PEL(profile.rows)))
            return -1;

    return 0;
}

// vips_profile
public int Profile(VipsImage in_data, ref VipsImage columns, ref VipsImage rows, params object[] args)
{
    var va_list = new VAList(args);
    int result;

    result = VipsCallSplit("profile", va_list, in_data, ref columns, ref rows);
    return result;
}
```