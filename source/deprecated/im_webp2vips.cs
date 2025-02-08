```csharp
// This file is part of VIPS.
// VIPS is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

using System;
using System.IO;

public class VipsFormatWebp : VipsFormat
{
    // webp2vips
    public int Webp2Vips(string name, Image out, bool headerOnly)
    {
        char[] filename = new char[FILENAME_MAX];
        char[] mode = new char[FILENAME_MAX];

        im_filename_split(name, filename, mode);

#if HAVE_LIBWEBP
        VipsSource source;
        int result;

        if ((source = vips_source_new_from_file(filename)) == null)
            return -1;
        if (headerOnly)
            result = vips__webp_read_header_source(source, out, 0, 1, 1);
        else
            result = vips__webp_read_source(source, out, 0, 1, 1);
        VIPS_UNREF(source);

        if (result != 0)
            return result;
#else
        throw new Exception("im_webp2vips: " + _("no webp support in your libvips"));
#endif /*HAVE_LIBWEBP*/

        return 0;
    }

    // im_webp2vips
    public int ImWebp2Vips(string name, Image out)
    {
        return Webp2Vips(name, out, false);
    }

#if HAVE_LIBWEBP

    // vips__iswebp
    public bool VipsIswebp(string filename)
    {
        VipsSource source;
        bool result;

        if ((source = vips_source_new_from_file(filename)) == null)
            return false;
        result = vips__iswebp_source(source);
        VIPS_UNREF(source);

        return result;
    }

    // im_webp2vips_header
    public int ImWebp2VipsHeader(string name, Image out)
    {
        return Webp2Vips(name, out, true);
    }

    // webp_suffs
    private static string[] webpSuffs = { ".webp", null };

    // vips_format_webp_class_init
    public void VipsFormatWebpClassInit()
    {
        VipsObjectClass objectClass = (VipsObjectClass)typeof(VipsFormatWebp).GetBaseType();
        VipsFormatClass formatClass = (VipsFormatClass)objectClass;

        objectClass.Nickname = "webp";
        objectClass.Description = _("webp");

        formatClass.IsA = VipsIswebp;
        formatClass.Header = ImWebp2VipsHeader;
        formatClass.Load = ImWebp2Vips;
        formatClass.Save = ImVips2Webp;
        formatClass.Suffs = webpSuffs;
    }

    // vips_format_webp_init
    public void VipsFormatWebpInit()
    {
    }

    public static Type RegisterType(Type type)
    {
        return typeof(VipsFormatWebp).Register();
    }
#endif /*HAVE_LIBWEBP*/
}
```