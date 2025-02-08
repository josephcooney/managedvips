```csharp
// vips_foreign_load_matrix_dispose
public class VipsForeignLoadMatrix : VipsForeignLoad
{
    public override void Dispose(GObject gobject)
    {
        base.Dispose(gobject);
        var matrix = (VipsForeignLoadMatrix)gobject;
        VIPS.Unref(matrix.Source);
        VIPS.Unref(matrix.Sbuf);
        VIPS.Free(matrix.Linebuf);
    }
}

// vips_foreign_load_matrix_build
public class VipsForeignLoadMatrix : VipsForeignLoad
{
    public override int Build(VipsObject object)
    {
        var matrix = (VipsForeignLoadMatrix)object;
        if (!(matrix.Sbuf = VipsSbuf.NewFromSource(matrix.Source)))
            return -1;

        if (base.Build(object))
            return -1;

        return 0;
    }
}

// vips_foreign_load_matrix_get_flags
public class VipsForeignLoadMatrix : VipsForeignLoad
{
    public override VipsForeignFlags GetFlags()
    {
        return 0;
    }
}

// parse_matrix_header
private static int ParseMatrixHeader(string line, out int width, out int height, out double scale, out double offset)
{
    var header = new double[4];
    var p = line;
    var q = VipsBreakToken(p, " \t");
    var i = 0;

    while (q != null && i < 4)
    {
        if (VipsStrtod(p, ref header[i]))
        {
            Vips.Error("matload", _("bad number \"{0}\""), p);
            return -1;
        }

        p = q;
        q = VipsBreakToken(p, " \t");
        i++;
    }

    if (i < 4)
        header[3] = 0.0;
    if (i < 3)
        header[2] = 1.0;
    if (i < 2)
    {
        Vips.Error("matload", "%s", _("no width / height"));
        return -1;
    }

    if (VIPS.Floor(header[0]) != header[0] || VIPS.Floor(header[1]) != header[1])
    {
        Vips.Error("mask2vips", "%s", _("width / height not int"));
        return -1;
    }

    // Width / height can be 65536 for a 16-bit LUT, for example.
    width = (int)header[0];
    height = (int)header[1];

    if (width <= 0 || width > 100000 || height <= 0 || height > 100000)
    {
        Vips.Error("mask2vips", "%s", _("width / height out of range"));
        return -1;
    }

    if (header[2] == 0.0)
    {
        Vips.Error("mask2vips", "%s", _("zero scale"));
        return -1;
    }

    scale = header[2];
    offset = header[3];

    return 0;
}

// vips_foreign_load_matrix_header
public class VipsForeignLoadMatrix : VipsForeignLoad
{
    public override int Header(VipsImage out)
    {
        var matrix = (VipsForeignLoadMatrix)this;
        var line = VipsSbuf.GetLineCopy(matrix.Sbuf);
        var width = 0;
        var height = 0;
        var scale = 0.0;
        var offset = 0.0;

        // Rewind.
        VipsSbuf.Unbuffer(matrix.Sbuf);

        if (Vips.Source.Rewind(matrix.Source))
            return -1;

        if (ParseMatrixHeader(line, out width, out height, out scale, out offset) != 0)
        {
            g_free(line);
            return -1;
        }

        g_free(line);

        if (Vips.Image.Pipelinev(out, VIPS.DemandStyle.ThinStrip, null))
            return -1;

        Vips.Image.InitFields(out, width, height, 1,
                              VIPS.Format.Double,
                              VIPS.Coding.None,
                              VIPS.Interpretation.B_W,
                              1.0, 1.0);

        Vips.Image.SetDouble(out, "scale", scale);
        Vips.Image.SetDouble(out, "offset", offset);

        VIPS.SetStr(out.Filename, Vips.ConnectionFilename(VIPS.Connection(matrix.Source)));

        if ((matrix.Linebuf = VIPS.Array(null, width, double)) == null)
            return -1;

        return 0;
    }
}

// vips_foreign_load_matrix_load
public class VipsForeignLoadMatrix : VipsForeignLoad
{
    public override int Load(VipsImage out)
    {
        var matrix = (VipsForeignLoadMatrix)this;
        var x = 0;
        var y = 0;

        if (Vips.Image.Pipelinev(out.Real, VIPS.DemandStyle.ThinStrip, null))
            return -1;

        Vips.Image.InitFields(out.Real,
                              out.Out.Xsize, out.Out.Ysize, 1,
                              VIPS.Format.Double,
                              VIPS.Coding.None,
                              VIPS.Interpretation.B_W,
                              1.0, 1.0);

        for (y = 0; y < out.Real.Ysize; y++)
        {
            var line = VipsSbuf.GetLineCopy(matrix.Sbuf);
            var p = line;
            var q = VipsBreakToken(p, " \t");

            while ((q != null) && (x < out.Out.Xsize))
            {
                if (VipsStrtod(p, ref matrix.Linebuf[x]))
                {
                    Vips.Error(base.Class.Nickname,
                               _("bad number \"{0}\""), p);
                    g_free(line);
                    return -1;
                }

                p = q;
                q = VipsBreakToken(p, " \t");
                x++;
            }

            g_free(line);

            if (x != out.Out.Xsize)
            {
                Vips.Error(base.Class.Nickname,
                           _("line {0} too short"), y);
                return -1;
            }

            if (Vips.Image.WriteLine(out.Real, y, (Vips.Pel)matrix.Linebuf))
                return -1;

            x = 0;
        }

        return 0;
    }
}

// vips_foreign_load_matrix_class_init
public class VipsForeignLoadMatrix : VipsForeignLoad
{
    public static void ClassInit(VipsForeignLoadClass class_)
    {
        base.ClassInit(class_);
        var gobject_class = GObjectClass(class_);
        var object_class = (Vips.ObjectClass)class_;
        var load_class = (Vips.ForeignLoadClass)class_;

        gobject_class.Dispose += vips_foreign_load_matrix_dispose;

        object_class.Nickname = "matrixload_base";
        object_class.Description = _("load matrix");
        object_class.Build = vips_foreign_load_matrix_build;

        load_class.GetFlags = vips_foreign_load_matrix_get_flags;
        load_class.Header = vips_foreign_load_matrix_header;
        load_class.Load = vips_foreign_load_matrix_load;
    }
}

// vips_foreign_load_matrix_file_get_flags_filename
public class VipsForeignLoadMatrixFile : VipsForeignLoadMatrix
{
    public override VipsForeignFlags GetFlagsFilename(string filename)
    {
        return 0;
    }
}

// vips_foreign_load_matrix_file_build
public class VipsForeignLoadMatrixFile : VipsForeignLoadMatrix
{
    public override int Build(VipsObject object)
    {
        var file = (Vips.ForeignLoadMatrixFile)object;

        if (file.Filename != null && !(this.Source = Vips.Source.NewFromFile(file.Filename)))
            return -1;

        if (base.Build(object))
            return -1;

        return 0;
    }
}

// vips_foreign_load_matrix_file_is_a
public class VipsForeignLoadMatrixFile : VipsForeignLoadMatrix
{
    public override bool IsA(string filename)
    {
        var line = new char[80];
        var bytes = Vips.GetBytes(filename, line, 79);
        if (bytes <= 0)
            return false;
        line[bytes] = '\0';

        Vips.ErrorFreeze();
        var result = ParseMatrixHeader((string)line,
                                        out _, out _, out _, out _);
        Vips.ErrorThaw();

        return result == 0;
    }
}

// vips_foreign_load_matrix_file_class_init
public class VipsForeignLoadMatrixFile : VipsForeignLoadMatrix
{
    public static void ClassInit(VipsForeignLoadClass class_)
    {
        base.ClassInit(class_);
        var gobject_class = GObjectClass(class_);
        var object_class = (Vips.ObjectClass)class_;
        var foreign_class = (Vips.ForeignClass)class_;
        var load_class = (Vips.ForeignLoadClass)class_;

        gobject_class.SetProperty += Vips.Object.SetProperty;
        gobject_class.GetProperty += Vips.Object.GetProperty;

        object_class.Nickname = "matrixload";
        object_class.Build = vips_foreign_load_matrix_file_build;

        foreign_class.Suffs = new string[] { ".mat", null };

        load_class.IsA = vips_foreign_load_matrix_file_is_a;
        load_class.GetFlagsFilename = vips_foreign_load_matrix_file_get_flags_filename;

        VIPS.ArgString(class_, "filename", 1,
                       _("Filename"),
                       _("Filename to load from"),
                       VIPS.Argument.RequiredInput,
                       G.StructOffset(typeof(Vips.ForeignLoadMatrixFile), "filename"),
                       null);
    }
}

// vips_foreign_load_matrix_source_build
public class VipsForeignLoadMatrixSource : VipsForeignLoadMatrix
{
    public override int Build(VipsObject object)
    {
        var source = (Vips.ForeignLoadMatrixSource)object;

        if (source.Source != null)
            this.Source = source.Source;
        else
            g_object_ref(this.Source);

        if (base.Build(object))
            return -1;

        return 0;
    }
}

// vips_foreign_load_matrix_source_is_a_source
public class VipsForeignLoadMatrixSource : VipsForeignLoadMatrix
{
    public override bool IsASource(Vips.Source source)
    {
        var data = new unsigned char[80];
        var bytes_read = Vips.Source.SniffAtMost(source, ref data, 79);
        if (bytes_read <= 0)
            return false;
        var line = new string(data);

        Vips.ErrorFreeze();
        var result = ParseMatrixHeader(line,
                                        out _, out _, out _, out _);
        Vips.ErrorThaw();

        return result == 0;
    }
}

// vips_foreign_load_matrix_source_class_init
public class VipsForeignLoadMatrixSource : VipsForeignLoadMatrix
{
    public static void ClassInit(VipsOperationClass class_)
    {
        base.ClassInit(class_);
        var gobject_class = GObjectClass(class_);
        var object_class = (Vips.ObjectClass)class_;
        var operation_class = (Vips.OperationClass)class_;
        var load_class = (Vips.ForeignLoadClass)class_;

        gobject_class.SetProperty += Vips.Object.SetProperty;
        gobject_class.GetProperty += Vips.Object.GetProperty;

        object_class.Nickname = "matrixload_source";
        object_class.Build = vips_foreign_load_matrix_source_build;

        operation_class.Flags |= VIPS.Operation.NoCache;

        load_class.IsASource = vips_foreign_load_matrix_source_is_a_source;

        VIPS.ArgObject(class_, "source", 1,
                       _("Source"),
                       _("Source to load from"),
                       VIPS.Argument.RequiredInput,
                       G.StructOffset(typeof(Vips.ForeignLoadMatrixSource), "source"),
                       typeof(Vips.Source));
    }
}

// vips_matrixload
public static int MatrixLoad(string filename, out Vips.Image out)
{
    var result = Vips.CallSplit("matrixload", filename, out);
    return result;
}

// vips_matrixload_source
public static int MatrixLoadSource(Vips.Source source, out Vips.Image out)
{
    var result = Vips.CallSplit("matrixload_source", source, out);
    return result;
}
```