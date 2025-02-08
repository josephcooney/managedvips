Here is the converted C# code:

```csharp
// load analyze from a file
// 
// 5/12/11
//     - from openslideload.c

using System;
using VipsDotNet;

public class VipsForeignLoadAnalyze : VipsForeignLoad
{
    // Filename for load.
    public string filename { get; set; }

    public override int GetFlagsFilename(string filename)
    {
        return VIPS_FOREIGN_PARTIAL;
    }

    public override int GetFlags()
    {
        return VIPS_FOREIGN_PARTIAL;
    }

    public override int Header(VipsForeignLoad load)
    {
        var analyze = (VipsForeignLoadAnalyze)load;

        if (VipsDotNet.Vips.vips__analyze_read_header(analyze.filename, load.out))
            return -1;

        VipsDotNet.Vips.VIPS_SETSTR(load.out.filename, analyze.filename);

        return 0;
    }

    public override int Load(VipsForeignLoad load)
    {
        var analyze = (VipsForeignLoadAnalyze)load;

        if (VipsDotNet.Vips.vips__analyze_read(analyze.filename, load.real))
            return -1;

        return 0;
    }
}

// vips_foreign_analyze_suffs[] = { ".img", ".hdr", NULL };

public static class VipsForeignLoadAnalyzeClass
{
    public static void ClassInit()
    {
        // GObjectClass
        var gobject_class = typeof(VipsObject).GetInterface(typeof(GObject));
        var object_class = (VipsObject)gobject_class;
        var operation_class = (VipsOperation)object_class;
        var foreign_class = (VipsForeign)object_class;
        var load_class = (VipsForeignLoadClass)object_class;

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        object_class.Nickname = "analyzeload";
        object_class.Description = _("load an Analyze6 image");

        // This is fuzzed, but you're unlikely to want to use it on
        // untrusted files.
        operation_class.Flags |= VIPS_OPERATION_UNTRUSTED;

        foreign_class.Suffs = new string[] { ".img", ".hdr" };

        // is_a() is not that quick ... lower the priority.
        foreign_class.Priority = -50;

        load_class.IsA += VipsDotNet.Vips.vips__isanalyze;
        load_class.GetFlagsFilename = VipsForeignLoadAnalyze.GetFlagsFilename;
        load_class.GetFlags = VipsForeignLoadAnalyze.GetFlags;
        load_class.Header = VipsForeignLoadAnalyze.Header;
        load_class.Load = VipsForeignLoadAnalyze.Load;

        // VIPS_ARG_STRING
        var arg_string = typeof(VipsObject).GetField("arg_string", BindingFlags.Static | BindingFlags.Public);
        arg_string.SetValue(null, "filename", 1, _("Filename"), _("Filename to load from"), VIPS_ARGUMENT_REQUIRED_INPUT, G_STRUCT_OFFSET(typeof(VipsForeignLoadAnalyze), filename));
    }
}

// vips_analyzeload:
// @filename: file to load
// @out: (out): decompressed image
// @...: %NULL-terminated list of optional named arguments
//
// Load an Analyze 6.0 file. If @filename is "fred.img", this will look for
// an image header called "fred.hdr" and pixel data in "fred.img". You can
// also load "fred" or "fred.hdr".
//
// Images are
// loaded lazilly and byte-swapped, if necessary. The Analyze metadata is read
// and attached.
//
// See also: vips_image_new_from_file().
//
// Returns: 0 on success, -1 on error.

public class VipsForeignLoadAnalyzeMethods
{
    public static int VipsAnalyzeload(string filename, ref VipsImage out, params object[] args)
    {
        var result = VipsDotNet.Vips.vips_call_split("analyzeload", filename, ref out);
        return result;
    }
}
```