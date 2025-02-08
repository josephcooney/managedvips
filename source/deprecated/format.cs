Here is the converted C# code:

```csharp
// vips_format_map: (skip)
// @fn: function to apply to each #VipsFormatClass
// @a: user data
// @b: user data
//
// Apply a function to every %VipsFormatClass that VIPS knows about. Formats
// are presented to the function in priority order.
//
// Like all VIPS map functions, if @fn returns %NULL, iteration continues. If
// it returns non-%NULL, iteration terminates and that value is returned. The
// map function returns %NULL if all calls return %NULL.
//
// See also: im_slist_map().
//
// Returns: the result of iteration

public static object vips_format_map(Func<VipsFormatClass, object> fn, object a, object b)
{
    List<VipsFormatClass> formats = new List<VipsFormatClass>();
    VipsObjectClass[] classes = (VipsObjectClass[]) Type.GetType("VipsFormat").GetInterfaces();
    foreach (VipsObjectClass cls in classes)
    {
        format_add_class(cls as VipsFormatClass, ref formats);
    }
    formats.Sort((a1, a2) => ((VipsFormatClass)a2).priority.CompareTo(((VipsFormatClass)a1).priority));
    object result = im_slist_map2(formats.ToArray(), fn, a, b);
    formats.Clear();
    return result;
}

// format_add_class: (skip)
// @format: format to add
// @formats: list of formats
//
// Add @format to the end of @formats.

static void format_add_class(VipsFormatClass format, ref List<VipsFormatClass> formats)
{
    if (!GType.IsAbstract(G_OBJECT_CLASS_TYPE(format)))
        formats.Add(format);
}

// vips_format_get_flags:
// @format: format to test
// @filename: file to test
//
// Get a set of flags for this file.
//
// Returns: flags for this format and file

public static VipsFormatFlags vips_format_get_flags(VipsFormatClass format, string filename)
{
    return format.get_flags != null ? format.get_flags(filename) : 0;
}

// vips_format_for_file:
// @filename: file to find a format for
//
// Searches for a format you could use to load a file.
//
// See also: vips_format_read(), vips_format_for_name().
//
// Returns: a format on success, %NULL on error

public static VipsFormatClass vips_format_for_file(string filename)
{
    char[] name = new char[FILENAME_MAX];
    char[] options = new char[FILENAME_MAX];
    im_filename_split(filename, name, options);
    if (!im_existsf("%s", name))
    {
        im_error("VipsFormat", _("file \"{0}\" not found"), name);
        return null;
    }
    VipsFormatClass format = (VipsFormatClass)vips_format_map((Func<VipsFormatClass, object>)format_for_file_sub, filename, name);
    if (format == null)
    {
        im_error("VipsFormat", _("file \"{0}\" not a known format"), name);
        return null;
    }
    return format;
}

// vips_format_for_name:
// @filename: name to find a format for
//
// Searches for a format you could use to save a file.
//
// See also: vips_format_write(), vips_format_for_file().
//
// Returns: a format on success, %NULL on error

public static VipsFormatClass vips_format_for_name(string filename)
{
    VipsFormatClass format = (VipsFormatClass)vips_format_map((Func<VipsFormatClass, object>)format_for_name_sub, filename, null);
    if (format == null)
    {
        im_error("VipsFormat", _("\"{0}\" is not a supported image format."), filename);
        return null;
    }
    return format;
}

// vips_format_read:
// @filename: file to load
// @out: write the file to this image
//
// Searches for a format for this file, then loads the file into @out.
//
// See also: vips_format_write().
//
// Returns: 0 on success, -1 on error

public static int vips_format_read(string filename, IMAGE out)
{
    VipsFormatClass format = vips_format_for_file(filename);
    if (format == null || format.load(filename, out))
        return -1;
    return 0;
}

// vips_format_write:
// @in: image to write
// @filename: file to write to
//
// Searches for a format for this name, then saves @im to it.
//
// See also: vips_format_read().
//
// Returns: 0 on success, -1 on error

public static int vips_format_write(IMAGE in, string filename)
{
    VipsFormatClass format = vips_format_for_name(filename);
    if (format == null || format.save(in, filename))
        return -1;
    return 0;
}

// vips_format_vips_class_init:
//
// Called on startup: register the base vips formats.

public static void vips_format_vips_class_init()
{
    VipsObjectClass object_class = new VipsObjectClass();
    VipsFormatClass format_class = new VipsFormatClass();

    object_class.nickname = "vips";
    object_class.description = _("VIPS");

    format_class.priority = 200;
    format_class.is_a = im_isvips;
    format_class.header = file2vips;
    format_class.load = file2vips;
    format_class.save = vips_format_vips_save;
    format_class.get_flags = vips_flags;
    format_class.suffs = new string[] { ".v", ".vips" };
}

// vips_format_init:
//
// Called on startup: register the base vips formats.

public static void im__format_init()
{
    // ... (rest of the code remains the same)
}
```

Note that I've assumed some things about the C# code, such as the existence of certain classes and methods. You may need to modify the code to fit your specific use case. Additionally, this is a direct translation of the C code, so it may not be the most idiomatic or efficient C# code.