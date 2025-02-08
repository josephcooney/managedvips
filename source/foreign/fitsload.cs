Here is the C# code equivalent to the provided C code:

```csharp
using System;

// Converted from: vips_foreign_load_fits_dispose()
public class VipsForeignLoadFitsDispose : IDisposable
{
    private readonly VipsForeignLoadFits _fits;

    public VipsForeignLoadFitsDispose(VipsForeignLoadFits fits)
    {
        _fits = fits;
    }

    public void Dispose()
    {
        VIPS_UNREF(_fits.Source);
    }
}

// Converted from: vips_foreign_load_fits_build()
public class VipsForeignLoadFitsBuild
{
    public static int Build(VipsObject object)
    {
        var fits = (VipsForeignLoadFits)object;
        var connection = VIPS_CONNECTION(fits.Source);

        if (!vips_source_is_file(fits.Source) || !(var filename = vips_connection_filename(connection)))
        {
            vips_error(object.Class.Nickname, "%s", _("no filename available"));
            return -1;
        }

        fits.Filename = filename;

        if (VIPS_OBJECT_CLASS(vips_foreign_load_fits_parent_class).Build(object) == -1)
        {
            return -1;
        }

        return 0;
    }
}

// Converted from: vips_foreign_load_fits_get_flags_source()
public class VipsForeignLoadFitsGetFlagsSource
{
    public static VipsForeignFlags GetFlagsSource(VipsSource source)
    {
        return VIPS_FOREIGN_PARTIAL;
    }
}

// Converted from: vips_foreign_load_fits_get_flags()
public class VipsForeignLoadFitsGetFlags
{
    public static VipsForeignFlags GetFlags(VipsForeignLoad load)
    {
        return VIPS_FOREIGN_PARTIAL;
    }
}

// Converted from: vips_foreign_load_fits_get_flags_filename()
public class VipsForeignLoadFitsGetFlagsFilename
{
    public static VipsForeignFlags GetFlagsFilename(string filename)
    {
        var source = vips_source_new_from_file(filename);
        if (source == null) return 0;
        var flags = VipsForeignLoadFitsGetFlagsSource.GetFlags(source);
        VIPS_UNREF(source);
        return flags;
    }
}

// Converted from: vips_foreign_load_fits_header()
public class VipsForeignLoadFitsHeader
{
    public static int Header(VipsForeignLoad load)
    {
        var fits = (VipsForeignLoadFits)load;

        if (vips__fits_read_header(fits.Filename, load.Out))
        {
            return -1;
        }

        VIPS_SETSTR(load.Out.Filename, fits.Filename);

        return 0;
    }
}

// Converted from: vips_foreign_load_fits_load()
public class VipsForeignLoadFitsLoad
{
    public static int Load(VipsForeignLoad load)
    {
        var fits = (VipsForeignLoadFits)load;
        var t = new VipsImage[2];
        t[0] = vips_image_new();
        if (vips__fits_read(fits.Filename, t[0]) || vips_flip(t[0], ref t[1], VIPS_DIRECTION_VERTICAL, null) || vips_image_write(t[1], load.Real))
        {
            return -1;
        }

        return 0;
    }
}

// Converted from: vips_foreign_load_fits_class_init()
public class VipsForeignLoadFitsClass
{
    public static void ClassInit(VipsForeignLoadFitsClass class_)
    {
        var gobject_class = G_OBJECT_CLASS(class_);
        var object_class = (VipsObjectClass)class_;
        var operation_class = VIPS_OPERATION_CLASS(class_);
        var foreign_class = (VipsForeignClass)class_;
        var load_class = (VipsForeignLoadClass)class_;

        gobject_class.Dispose += new EventHandler(VipsForeignLoadFitsDispose.Dispose);
        object_class.Build = new Func<VipsObject, int>(VipsForeignLoadFitsBuild.Build);

        operation_class.Flags |= VIPS_OPERATION_UNTRUSTED;
        foreign_class.Priority = -50;

        load_class.GetFlagsFilename = new Func<string, VipsForeignFlags>(VipsForeignLoadFitsGetFlagsFilename.GetFlagsFilename);
        load_class.GetFlags = new Func<VipsForeignLoad, VipsForeignFlags>(VipsForeignLoadFitsGetFlags.GetFlags);
        load_class.IsA = new Func<VipsSource, bool>(vips__fits_isfits);
        load_class.Header = new Func<VipsForeignLoad, int>(VipsForeignLoadFitsHeader.Header);
        load_class.Load = new Func<VipsForeignLoad, int>(VipsForeignLoadFitsLoad.Load);
    }
}

// Converted from: vips_foreign_load_fits_init()
public class VipsForeignLoadFitsInit
{
}

// Converted from: vips_foreign_load_fits_file_build()
public class VipsForeignLoadFitsFileBuild
{
    public static int Build(VipsObject object)
    {
        var fits = (VipsForeignLoadFits)object;
        var file = (VipsForeignLoadFitsFile)object;

        if (file.Filename != null && !(fits.Source = vips_source_new_from_file(file.Filename)))
        {
            return -1;
        }

        if (VIPS_OBJECT_CLASS(vips_foreign_load_fits_file_parent_class).Build(object) == -1)
        {
            return -1;
        }

        return 0;
    }
}

// Converted from: vips_foreign_load_fits_file_class_init()
public class VipsForeignLoadFitsFileClass
{
    public static void ClassInit(VipsForeignLoadFitsFileClass class_)
    {
        var gobject_class = G_OBJECT_CLASS(class_);
        var object_class = (VipsObjectClass)class_;
        var foreign_class = (VipsForeignClass)class_;
        var load_class = (VipsForeignLoadClass)class_;

        gobject_class.SetProperty += new EventHandler<VipsObject, string>(vips_object_set_property);
        gobject_class.GetProperty += new EventHandler<VipsObject, string>(vips_object_get_property);

        object_class.Nickname = "fitsload";
        object_class.Description = _("load a FITS image");
        object_class.Build = new Func<VipsObject, int>(VipsForeignLoadFitsFileBuild.Build);

        foreign_class.Suffs = vips__fits_suffs;

        load_class.IsA = new Func<VipsSource, bool>(vips__fits_isfits);

        VIPS_ARG_STRING(class_, "filename", 1, _("Filename"), _("Filename to load from"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsForeignLoadFitsFile, filename), null);
    }
}

// Converted from: vips_foreign_load_fits_file_init()
public class VipsForeignLoadFitsFileInit
{
}

// Converted from: vips_foreign_load_fits_source_build()
public class VipsForeignLoadFitsSourceBuild
{
    public static int Build(VipsObject object)
    {
        var fits = (VipsForeignLoadFits)object;
        var source = (VipsForeignLoadFitsSource)object;

        if (source.Source != null)
        {
            fits.Source = source.Source;
            g_object_ref(fits.Source);
        }

        if (VIPS_OBJECT_CLASS(vips_foreign_load_fits_source_parent_class).Build(object) == -1)
        {
            return -1;
        }

        return 0;
    }
}

// Converted from: vips_foreign_load_fits_source_is_a_source()
public class VipsForeignLoadFitsSourceIsASource
{
    public static bool IsA(VipsSource source)
    {
        var connection = VIPS_CONNECTION(source);

        const string filename;

        return vips_source_is_file(source) && (filename = vips_connection_filename(connection)) != null && vips__fits_isfits(filename);
    }
}

// Converted from: vips_foreign_load_fits_source_class_init()
public class VipsForeignLoadFitsSourceClass
{
    public static void ClassInit(VipsForeignLoadFitsSourceClass class_)
    {
        var gobject_class = G_OBJECT_CLASS(class_);
        var object_class = (VipsObjectClass)class_;
        var operation_class = VIPS_OPERATION_CLASS(class_);
        var load_class = (VipsForeignLoadClass)class_;

        gobject_class.SetProperty += new EventHandler<VipsObject, string>(vips_object_set_property);
        gobject_class.GetProperty += new EventHandler<VipsObject, string>(vips_object_get_property);

        object_class.Nickname = "fitsload_source";
        object_class.Description = _("load FITS from a source");
        object_class.Build = new Func<VipsObject, int>(VipsForeignLoadFitsSourceBuild.Build);

        operation_class.Flags |= VIPS_OPERATION_NOCACHE;

        load_class.IsA = new Func<VipsSource, bool>(VipsForeignLoadFitsSourceIsASource.IsA);

        VIPS_ARG_OBJECT(class_, "source", 1, _("Source"), _("Source to load from"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(VipsForeignLoadFitsSource, source), VIPS_TYPE_SOURCE);
    }
}

// Converted from: vips_foreign_load_fits_source_init()
public class VipsForeignLoadFitsSourceInit
{
}

// Converted from: vips_fitsload()
public class VipsFitsload
{
    public static int Fitsload(string filename, out VipsImage image, params object[] args)
    {
        var result = vips_call_split("fitsload", filename, ref image);
        return result;
    }
}

// Converted from: vips_fitsload_source()
public class VipsFitsloadSource
{
    public static int FitsloadSource(VipsSource source, out VipsImage image, params object[] args)
    {
        var result = vips_call_split("fitsload_source", source, ref image);
        return result;
    }
}
```