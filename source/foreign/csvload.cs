Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignLoadCsv : VipsObject
{
    public VipsForeignLoadCsv()
    {
        // vips_foreign_load_csv_init
        lines = -1;
        whitespace = " ";
        separator = ";,\t";
    }

    public override void Dispose()
    {
        // vips_foreign_load_csv_dispose
        if (source != null)
            VIPS_UNREF(source);
        if (sbuf != null)
            VIPS_UNREF(sbuf);
        if (linebuf != null)
            VIPS_FREE(linebuf);
    }

    public override int Build()
    {
        // vips_foreign_load_csv_build
        sbuf = new VipsSbuf();
        if (!VIPS_SBUF_NEW_FROM_SOURCE(sbuf, source))
            return -1;

        whitemap = new char[256];
        sepmap = new char[256];

        for (int i = 0; i < 256; i++)
        {
            whitemap[i] = 0;
            sepmap[i] = 0;
        }

        string whitespaceStr = whitespace;
        for (int i = 0; i < whitespaceStr.Length; i++)
            whitemap[whitespaceStr[i]] = 1;

        string separatorStr = separator;
        for (int i = 0; i < separatorStr.Length; i++)
            sepmap[separatorStr[i]] = 1;

        // \n must not be in the maps or we'll get very confused.
        sepmap['\n'] = 0;
        whitemap['\n'] = 0;

        if (base.Build())
            return -1;

        return 0;
    }

    public override VipsForeignFlags GetFlags()
    {
        // vips_foreign_load_csv_get_flags
        return 0;
    }

    private int SkipWhite()
    {
        // vips_foreign_load_csv_skip_white
        int ch = VIPS_SBUF_GETC(sbuf);
        while (ch != EOF && ch != '\n' && whitemap[ch])
            ch = VIPS_SBUF_GETC(sbuf);

        if (ch == -1)
            return EOF;

        VIPS_SBUF_UNGETC(sbuf);

        return ch;
    }

    private int SkipQuoted()
    {
        // vips_foreign_load_csv_skip_quoted
        int ch = VIPS_SBUF_GETC(sbuf);
        while (ch != EOF && ch != '\n')
        {
            if (ch == '\\')
                ch = VIPS_SBUF_GETC(sbuf);

            if (ch == '"')
                break;
        }

        if (ch == '\n')
            VIPS_SBUF_UNGETC(sbuf);

        return ch;
    }

    private string FetchItem()
    {
        // vips_foreign_load_csv_fetch_item
        int writePoint = 0;
        int spaceRemaining = MAX_ITEM_SIZE - 1;
        int ch;

        while ((ch = VIPS_SBUF_GETC(sbuf)) != -1 && ch != '\n' &&
               !whitemap[ch] && !sepmap[ch] && spaceRemaining > 0)
        {
            item[writePoint] = (char)ch;
            writePoint++;
            spaceRemaining--;
        }

        item[writePoint] = '\0';

        if (ch == -1 && writePoint == 0)
            return null;

        while (ch != -1 && ch != '\n' && !whitemap[ch] && !sepmap[ch])
            ch = VIPS_SBUF_GETC(sbuf);

        if (ch == '\n' || whitemap[ch] || sepmap[ch])
            VIPS_SBUF_UNGETC(sbuf);

        return item;
    }

    private int ReadDouble(double[] linebuf)
    {
        // vips_foreign_load_csv_read_double
        double value = 0;

        int ch = SkipWhite();
        if (ch == EOF || ch == '\n')
            return ch;

        if (ch == '"')
        {
            ch = VIPS_SBUF_GETC(sbuf);
            ch = SkipQuoted();
        }
        else if (!sepmap[ch])
        {
            string item = FetchItem();
            if (item == null)
                return EOF;

            if (VipsStrtod(item, ref value))
            {
                // Only a warning, since (for example) exported
                // spreadsheets will often have text or date fields.
                VIPS_WARNING("bad number, line %d, column %d", lineno, colno);
            }
        }

        ch = SkipWhite();
        if (ch == EOF || ch == '\n')
            return ch;

        if (sepmap[ch])
            VIPS_SBUF_GETC(sbuf);

        return ch;
    }

    public override int Header()
    {
        // vips_foreign_load_csv_header
        VipsObjectClass class = VIPS_OBJECT_GET_CLASS(this);
        VipsForeignLoadCsv csv = this as VipsForeignLoadCsv;

        int i;
        double value;
        int ch;
        int width;
        int height;

        // Rewind.
        VIPS_SBUF_UNBUFFER(sbuf);
        if (VIPS_SOURCE_REWIND(source))
            return -1;

        // Skip the first few lines.
        for (i = 0; i < skip; i++)
        {
            if (!VIPS_SBUF_GET_LINE(sbuf))
            {
                VIPS_ERROR(class.Nickname, "%s", _("unexpected end of file"));
                return -1;
            }
        }

        // Parse the first line to get the number of columns.
        csv.lineno = skip + 1;
        csv.colno = 0;
        do
        {
            csv.colno++;
            ch = ReadDouble(linebuf);
        } while (ch != '\n' && ch != EOF);

        width = csv.colno;

        if ((linebuf = VIPS_ARRAY(NULL, width, double)) == null)
            return -1;

        // If @lines is -1, we must scan the whole file to get the height.
        if (csv.lines == -1)
            for (height = 0; VIPS_SBUF_GET_LINE(sbuf); height++)
                ;
        else
            height = csv.lines;

        VipsImageInitFields(out, width, height, 1,
                            VipsFormat.Double,
                            VipsCoding.None,
                            VipsInterpretation.BW,
                            1.0, 1.0);
        if (VipsImagePipelinev(out, VipsDemandStyle.ThinStrip, null))
            return -1;

        VIPS_SETSTR(out.Filename, VIPS_CONNECTION_FILENAME(VIPS_CONNECTION(source)));

        return 0;
    }

    public override int Load()
    {
        // vips_foreign_load_csv_load
        VipsObjectClass class = VIPS_OBJECT_GET_CLASS(this);
        VipsForeignLoadCsv csv = this as VipsForeignLoadCsv;

        int i;
        int x, y;
        int ch;

        // Rewind.
        VIPS_SBUF_UNBUFFER(sbuf);
        if (VIPS_SOURCE_REWIND(source))
            return -1;

        // Skip the first few lines.
        for (i = 0; i < skip; i++)
        {
            if (!VIPS_SBUF_GET_LINE(sbuf))
            {
                VIPS_ERROR(class.Nickname, "%s", _("unexpected end of file"));
                return -1;
            }
        }

        VipsImageInitFields(real, out.Xsize, out.Ysize, 1,
                            VipsFormat.Double,
                            VipsCoding.None,
                            VipsInterpretation.BW,
                            1.0, 1.0);
        if (VipsImagePipelinev(real, VipsDemandStyle.ThinStrip, null))
            return -1;

        csv.lineno = skip + 1;
        for (y = 0; y < real.Ysize; y++)
        {
            csv.colno = 0;

            // Not needed, but stops a used-before-set compiler warning.
            ch = EOF;

            // Some lines may be shorter.
            memset(linebuf, 0, real.Xsize * sizeof(double));

            for (x = 0; x < real.Xsize; x++)
            {
                double value;

                csv.colno++;
                ch = ReadDouble(linebuf);
                if (ch == EOF && fail_on >= VipsFailOn.Truncated)
                {
                    VIPS_ERROR(class.Nickname, "%s", _("unexpected end of file"));
                    return -1;
                }
                if (ch == '\n' && x != real.Xsize - 1)
                {
                    VIPS_ERROR(class.Nickname,
                        _("line %d has only %d columns"), csv.lineno, csv.colno);
                    // Unequal length lines, but no EOF.
                    if (fail_on >= VipsFailOn.Error)
                        return -1;
                }

                linebuf[x] = value;
            }

            // Step over the line separator.
            if (ch == '\n')
            {
                VIPS_SBUF_GETC(sbuf);
                csv.lineno++;
            }

            if (VipsImageWriteLine(real, y, (VipsPel[])linebuf))
                return -1;
        }

        return 0;
    }
}

public class VipsForeignLoadCsvFile : VipsObject
{
    public VipsForeignLoadCsvFile()
    {
        // vips_foreign_load_csv_file_init
    }

    public override int Build()
    {
        // vips_foreign_load_csv_file_build
        if (filename != null)
            source = new VipsSource(filename);

        return base.Build();
    }
}

public class VipsForeignLoadCsvSource : VipsObject
{
    public VipsForeignLoadCsvSource()
    {
        // vips_foreign_load_csv_source_init
    }

    public override int Build()
    {
        // vips_foreign_load_csv_source_build
        if (source != null)
            this.source = source;

        return base.Build();
    }
}

public class VipsForeignLoadCsvFileClass : VipsObjectClass
{
    public VipsForeignLoadCsvFileClass()
    {
        // vips_foreign_load_csv_file_class_init
    }

    public override void ClassInit(VipsObjectClass class)
    {
        base.ClassInit(class);

        class.Nickname = "csvload";
        class.Description = _("load csv");

        class.Build = new VipsBuildDelegate(vips_foreign_load_csv_file_build);
        class.GetFlagsFilename = new VipsGetFlagsDelegate(vips_foreign_load_csv_file_get_flags_filename);

        VIPS_ARG_STRING(class, "filename", 1,
            _("Filename"),
            _("Filename to load from"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadCsvFile, filename),
            null);
    }
}

public class VipsForeignLoadCsvSourceClass : VipsObjectClass
{
    public VipsForeignLoadCsvSourceClass()
    {
        // vips_foreign_load_csv_source_class_init
    }

    public override void ClassInit(VipsObjectClass class)
    {
        base.ClassInit(class);

        class.Nickname = "csvload_source";
        class.Build = new VipsBuildDelegate(vips_foreign_load_csv_source_build);
        class.IsASource = new VipsIsASourceDelegate(vips_foreign_load_csv_source_is_a_source);

        VIPS_ARG_OBJECT(class, "source", 1,
            _("Source"),
            _("Source to load from"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsForeignLoadCsvSource, source),
            VipsType.Source);
    }
}

public class VipsForeignLoadCsv : VipsObject
{
    // vips_foreign_load_csv

    public static int Csvload(const string filename, out VipsImage image)
    {
        return VipsCallSplit("csvload", null, filename, ref image);
    }

    public static int CsvloadSource(VipsSource source, out VipsImage image)
    {
        return VipsCallSplit("csvload_source", null, source, ref image);
    }
}
```

Note that this is a direct translation of the C code to C#, and some modifications may be necessary to make it work correctly in a .NET environment. Additionally, some types and methods (e.g., `VIPS_SBUF_GETC`, `VIPS_SBUF_UNGETC`) are not directly equivalent in C# and would need to be replaced with their C# equivalents or implemented manually.

Also note that this code assumes the existence of certain classes and methods (e.g., `VipsObject`, `VipsImage`, `VipsSource`, `VipsFormat`, etc.) which are not defined here. These classes and methods should be defined elsewhere in your project for this code to work correctly.