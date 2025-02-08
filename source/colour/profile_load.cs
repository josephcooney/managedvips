```csharp
// vips_profile_fallback_get
static IntPtr vips_profile_fallback_get(string name, out int length)
{
    for (int i = 0; ; i++)
    {
        VipsProfileFallback fallback = vips__profile_fallback_table[i];
        if (fallback != null && g_ascii_strcasecmp(fallback.name, name) == 0)
        {
            byte[] data;
            GConverter converter;
            GConverterResult res;
            int bytes_read;
            int bytes_written;

            data = new byte[fallback.length];
            converter = G_CONVERTER(g_zlib_decompressor_new(G_ZLIB_COMPRESSOR_FORMAT_ZLIB));

            res = g_converter_convert(converter, fallback.data, fallback.length, data, fallback.length, G_CONVERTER_INPUT_AT_END, out bytes_read, out bytes_written, null);
            g_object_unref(converter);

            if (res == G_CONVERTER_FINISHED)
            {
                length = fallback.length;
                return Marshal.AllocHGlobal(data);
            }
            else
            {
                g_free(data);
                g_warning("fallback profile decompression failed");
            }
        }
    }

    return IntPtr.Zero;
}

// vips_profile_load_build
static int vips_profile_load_build(VipsObject obj)
{
    VipsProfileLoad load = (VipsProfileLoad)obj;

    if (VIPS_OBJECT_CLASS(vips_profile_load_parent_class).build(obj) != 0)
        return -1;

    if (g_ascii_strcasecmp(load.name, "none") == 0)
        load.profile = null;
    else
    {
        IntPtr data = vips_profile_fallback_get(load.name, out int length);
        if (data != IntPtr.Zero)
            load.profile = VipsBlob.New((VipsCallbackFn)vips_area_free_cb, data, length);
        else
        {
            byte[] data2 = vips__file_read_name(load.name, vips__icc_dir(), out int length2);
            if (data2 == null)
            {
                vips_error(VIPS_OBJECT_GET_CLASS(obj).nickname, _("unable to load profile \"{0}\""), load.name);
                return -1;
            }
            else
                load.profile = VipsBlob.New((VipsCallbackFn)vips_area_free_cb, data2, length2);
        }
    }

    g_object_set(obj, "profile", load.profile, null);

    if (load.profile != null)
    {
        vips_area_unref((VipsArea)load.profile);
        load.profile = null;
    }

    return 0;
}

// vips_profile_load_class_init
static void vips_profile_load_class_init(VipsProfileLoadClass class)
{
    GObjectClass gobject_class = (GObjectClass)class;
    VipsObjectClass object_class = (VipsObjectClass)class;

    gobject_class.set_property = vips_object_set_property;
    gobject_class.get_property = vips_object_get_property;

    object_class.nickname = "profile_load";
    object_class.description = _("load named ICC profile");
    object_class.build = vips_profile_load_build;

    VIPS_ARG_STRING(class, "name", 1,
        _("Name"),
        _("Profile name"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsProfileLoad, name),
        null);

    VIPS_ARG_BOXED(class, "profile", 2,
        _("Profile"),
        _("Loaded profile"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsProfileLoad, profile),
        VIPS_TYPE_BLOB);
}

// vips_profile_load_init
static void vips_profile_load_init(VipsProfileLoad load)
{
}

// vips_profile_load
public static int vips_profile_load(string name, out VipsBlob profile, params object[] args)
{
    va_list ap;
    int result;

    va_start(ap, args);
    result = vips_call_split("profile_load", ap, name, ref profile);
    va_end(ap);

    return result;
}

// vips__profile_set
public static int vips__profile_set(VipsImage image, string name)
{
    VipsBlob profile;

    if (vips_profile_load(name, out profile, null) != 0)
        return -1;

    if (profile != null)
    {
        GValue value = new GValue();
        g_value_init(value, VIPS_TYPE_BLOB);
        g_value_set_boxed(value, profile);
        vips_image_set(image, VIPS_META_ICC_NAME, ref value);
        g_value_unset(value);
    }
    else
        vips_image_remove(image, VIPS_META_ICC_NAME);

    if (profile != null)
    {
        vips_area_unref((VipsArea)profile);
        profile = null;
    }

    return 0;
}
```