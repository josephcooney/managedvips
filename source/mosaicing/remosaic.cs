```csharp
// remosaic_fn (from remosaic.c)

public class RemosaicFn : IJoinNodeFunction
{
    public VipsImage Invoke(JoinNode node)
    {
        SymbolTable st = node.SymbolTable;
        VipsImage im = node.Image;

        if (im == null)
        {
            throw new FileNotFoundException($"file \"{node.Name}\" not found");
        }

        // Remove substring remosaic->old_str from in->filename, replace with
        // remosaic->new_str.
        string filename = im.FileName;
        int oldStrIndex = filename.LastIndexOf(remosaic.OldStr);
        if (oldStrIndex != -1)
        {
            filename = filename.Substring(0, oldStrIndex) + remosaic.NewStr +
                       filename.Substring(oldStrIndex + remosaic.OldLen);
        }

#ifdef DEBUG
        Console.WriteLine($"vips_remosaic: filename \"{im.FileName}\" -> \"{filename}\"");
#endif /*DEBUG*/

        VipsImage out;
        if (!(out = VipsGlobalOpenImage(st, filename)))
        {
            return null;
        }

        if (out.Xsize != im.Xsize || out.Ysize != im.Ysize)
        {
            throw new ArgumentException($"substitute image \"{filename}\" is not the same size as \"{im.FileName}\"");
        }

        return out;
    }
}

// vips_remosaic_build (from remosaic.c)

public class VipsRemosaicBuild : VipsObjectBuild
{
    public override int Invoke(VipsObject obj)
    {
        VipsRemosaic remosaic = (VipsRemosaic)obj;

        SymbolTable st;

        g_object_set(remosaic, "out", new VipsImage(), null);

        if (base.Invoke(obj) != 0)
            return -1;

        if ((st = VipsBuildSymtab(remosaic.Out, SYM_TAB_SIZE)) == null ||
            VipsParseDesc(st, remosaic.In) != 0)
            return -1;

        remosaic.OldLen = remosaic.OldStr.Length;
        remosaic.NewLen = remosaic.NewStr.Length;
        if (VipsBuildMosaic(st, remosaic.Out,
                            new RemosaicFn(), remosaic) != 0)
            return -1;

        return 0;
    }
}

// vips_remosaic_class_init (from remosaic.c)

public class VipsRemosaicClass : VipsOperationClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        GObjectClass gobjectClass = (GObjectClass)base;
        VipsObjectClass objectClass = (VipsObjectClass)this;

        gobjectClass.SetProperty = VipsObjectSetProperty;
        gobjectClass.GetProperty = VipsObjectGetProperty;

        objectClass.Nickname = "remosaic";
        objectClass.Description = "rebuild an mosaiced image";
        objectClass.Build = new VipsRemosaicBuild();

        VIPS_ARG_IMAGE(this, "in", 1,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsRemosaic, In));

        VIPS_ARG_IMAGE(this, "out", 2,
            _("Output"),
            _("Output image"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsRemosaic, Out));

        VIPS_ARG_STRING(this, "old_str", 5,
            _("old_str"),
            _("Search for this string"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsRemosaic, OldStr),
            "");

        VIPS_ARG_STRING(this, "new_str", 6,
            _("new_str"),
            _("And swap for this string"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsRemosaic, NewStr),
            "");
    }
}

// vips_remosaic_init (from remosaic.c)

public class VipsRemosaic : VipsOperation
{
    public override void Init()
    {
        base.Init();
    }
}

// vips_remosaic (from remosaic.c)

public static int VipsRemosaic(VipsImage in, ref VipsImage out,
                                string oldStr, string newStr)
{
    va_list ap;
    int result;

    va_start(ap, newStr);
    result = VipsCallSplit("remosaic", ap, in, ref out, oldStr, newStr);
    va_end(ap);

    return result;
}
```