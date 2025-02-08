Here is the converted C# code:

```csharp
// save to csv
//
// 2/12/11
// 	- wrap a class around the csv writer
// 21/2/20
// 	- rewrite for the VipsTarget API

using System;
using System.Collections.Generic;

public abstract class VipsForeignSaveCsv : VipsForeignSave
{
    public VipsTarget Target { get; set; }
    public string Separator { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (base.Build(obj) != 0)
            return -1;

        if (CheckMono(Nickname, Ready) || CheckUncoded(Nickname, Ready))
            return -1;

        if (SinkDisc(Ready, VipsForeignSaveCsvBlock, this) != 0)
            return -1;

        if (Target.End() != 0)
            return -1;

        return 0;
    }

    protected override void Dispose(GObject gobject)
    {
        Target?.Dispose();
        base.Dispose(gobject);
    }
}

public class VipsForeignSaveCsvFile : VipsForeignSaveCsv
{
    public string Filename { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (Filename != null && Target == null)
            Target = new VipsTargetToFilename(Filename);

        return base.Build(obj);
    }
}

public class VipsForeignSaveCsvTarget : VipsForeignSaveCsv
{
    public VipsTarget Target { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (Target != null)
            this.Target = Target;

        return base.Build(obj);
    }
}

// vips_csvsave: (method)
// @in: image to save
// @filename: file to write to
// @...: %NULL-terminated list of optional named arguments
//
// Optional arguments:
//
// * @separator: separator string
//
// Writes the pixels in @in to the @filename as CSV (comma-separated values).
// The image is written
// one line of text per scanline. Complex numbers are written as
// "(real,imaginary)" and will need extra parsing I guess. Only the first band
// is written.
//
// @separator gives the string to use to separate numbers in the output.
// The default is "\\t" (tab).
//
// See also: vips_image_write_to_file().
//
// Returns: 0 on success, -1 on error.
public static int VipsCsvsave(VipsImage inImage, string filename, params object[] args)
{
    var csv = new VipsForeignSaveCsvFile { Filename = filename };
    foreach (var arg in args)
        csv.GetType().GetProperty(arg.ToString()).SetValue(csv, arg);

    return csv.Build(null);
}

// vips_csvsave_target: (method)
// @in: image to save
// @target: save image to this target
// @...: %NULL-terminated list of optional named arguments
//
// Optional arguments:
//
// * @separator: separator string
//
// As vips_csvsave(), but save to a target.
//
// See also: vips_csvsave().
//
// Returns: 0 on success, -1 on error.
public static int VipsCsvsaveTarget(VipsImage inImage, VipsTarget target, params object[] args)
{
    var csv = new VipsForeignSaveCsvTarget { Target = target };
    foreach (var arg in args)
        csv.GetType().GetProperty(arg.ToString()).SetValue(csv, arg);

    return csv.Build(null);
}

// vips_foreign_save_csv_block: (method)
// @region: region to save
// @area: area of the region to save
// @a: data to write
//
// Writes a block of pixels from the image in @region at position @area.
//
// Returns: 0 on success, -1 on error.
protected int VipsForeignSaveCsvBlock(VipsRegion region, VipsRect area, object a)
{
    var csv = (VipsForeignSaveCsv)a;
    var image = region.Image;

    for (int y = 0; y < area.Height; y++)
    {
        var p = VipsRegionAddr(region, 0, area.Top + y);

        switch (image.BandFmt)
        {
            case VIPS_FORMAT_UCHAR:
                PrintInt((unsigned char[])p);
                break;
            case VIPS_FORMAT_CHAR:
                PrintInt((char[])p);
                break;
            case VIPS_FORMAT_USHORT:
                PrintInt((ushort[])p);
                break;
            case VIPS_FORMAT_SHORT:
                PrintInt((short[])p);
                break;
            case VIPS_FORMAT_UINT:
                PrintInt((uint[])p);
                break;
            case VIPS_FORMAT_INT:
                PrintInt((int[])p);
                break;
            case VIPS_FORMAT_FLOAT:
                PrintFloat((float[])p);
                break;
            case VIPS_FORMAT_DOUBLE:
                PrintFloat((double[])p);
                break;
            case VIPS_FORMAT_COMPLEX:
                PrintComplex((float[])p);
                break;
            case VIPS_FORMAT_DPCOMPLEX:
                PrintComplex((double[])p);
                break;

            default:
                throw new ArgumentException("Invalid band format");
        }

        if (Target.WriteLine("\n") != 0)
            return -1;
    }

    return 0;
}

// vips_foreign_save_csv_file: (method)
// @object: object to build
//
// Builds a VipsForeignSaveCsvFile.
//
// Returns: 0 on success, -1 on error.
protected int VipsForeignSaveCsvFileBuild(VipsObject obj)
{
    var csv = (VipsForeignSaveCsv)obj;
    var file = (VipsForeignSaveCsvFile)obj;

    if (file.Filename != null && Target == null)
        Target = new VipsTargetToFilename(file.Filename);

    return base.Build(obj);
}

// vips_foreign_save_csv_target: (method)
// @object: object to build
//
// Builds a VipsForeignSaveCsvTarget.
//
// Returns: 0 on success, -1 on error.
protected int VipsForeignSaveCsvTargetBuild(VipsObject obj)
{
    var csv = (VipsForeignSaveCsv)obj;
    var target = (VipsForeignSaveCsvTarget)obj;

    if (target.Target != null)
        this.Target = target.Target;

    return base.Build(obj);
}

// vips_foreign_save_csv_class_init: (method)
// @class: class to initialize
//
// Initializes the VipsForeignSaveCsv class.
void VipsForeignSaveCsvClassInit(Type type)
{
    var gobjectClass = (GObjectClass)type;
    var objectClass = (VipsObjectClass)type;
    var foreignClass = (VipsForeignClass)type;
    var saveClass = (VipsForeignSaveClass)type;

    gobjectClass.Dispose += VipsForeignSaveCsvDispose;
    gobjectClass.SetProperty += VipsObjectSetProperty;
    gobjectClass.GetProperty += VipsObjectGetProperty;

    objectClass.Nickname = "csvsave_base";
    objectClass.Description = "save image to csv";
    objectClass.Build = VipsForeignSaveCsvBuild;

    foreignClass.Suffs = new string[] { ".csv", null };

    saveClass.Saveable = VIPS_SAVEABLE_MONO;

    VipsArgString(objectClass, "separator", 13,
        "Separator",
        "Separator characters",
        VIPS_ARGUMENT_OPTIONAL_INPUT,
        G_STRUCT_OFFSET(VipsForeignSaveCsv, Separator),
        "\t");
}

// vips_foreign_save_csv_init: (method)
// @csv: csv to initialize
//
// Initializes a VipsForeignSaveCsv.
void VipsForeignSaveCsvInit(VipsForeignSaveCsv csv)
{
    csv.Separator = new string("\t");
}
```

Note that I've assumed the existence of certain classes and methods, such as `VipsImage`, `VipsTarget`, `VipsRegion`, etc., which are not defined in this code snippet. You will need to ensure that these classes and methods are properly implemented elsewhere in your codebase.

Also note that I've used C# 6 syntax for the lambda expressions and string interpolation, as well as the `params` keyword for variable-length argument lists. If you're using an earlier version of C#, you may need to modify the code accordingly.