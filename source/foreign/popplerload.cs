Here is the converted code:

```csharp
// vips_foreign_load_pdf_build
public class VipsForeignLoadPdf : VipsForeignLoad
{
    public VipsForeignLoadPdf(VipsSource source, GInputStream stream)
    {
        this.source = source;
        this.stream = stream;
    }

    protected override int Build()
    {
        if (vips_source_rewind(source))
            return -1;

        total_scale = scale * dpi / 72.0;

        doc = poppler_document_new_from_stream(stream, vips_source_length(source), password, null);
        if (doc == null)
        {
            vips_g_error(null);
            return -1;
        }

        return base.Build();
    }
}

// vips_foreign_load_pdf_get_flags_filename
public class VipsForeignLoadPdf : VipsForeignLoad
{
    public override VipsForeignFlags GetFlagsFilename(string filename)
    {
        return VIPS_FOREIGN_PARTIAL;
    }
}

// vips_foreign_load_pdf_get_flags
public class VipsForeignLoadPdf : VipsForeignLoad
{
    public override VipsForeignFlags GetFlags()
    {
        return VIPS_FOREIGN_PARTIAL;
    }
}

// vips_foreign_load_pdf_get_page
public class VipsForeignLoadPdf : VipsForeignLoadPdfBase
{
    protected override int GetPage(int page_no)
    {
        if (current_page != page_no || page == null)
        {
            Unref(page);
            current_page = -1;

            page = poppler_document_get_page(doc, page_no);
            current_page = page_no;
        }

        return 0;
    }
}

// vips_foreign_load_pdf_set_image
public class VipsForeignLoadPdf : VipsForeignLoadPdfBase
{
    protected override int SetImage(VipsImage out)
    {
        // ...

        if (vips_image_pipelinev(out, VIPS_DEMAND_STYLE_SMALLTILE, null) != 0)
            return -1;

        return 0;
    }
}

// vips_foreign_load_pdf_header
public class VipsForeignLoadPdf : VipsForeignLoadPdfBase
{
    protected override int Header()
    {
        // ...

        pages = new VipsRect[pdf->n];

        top = 0;
        image.left = 0;
        image.top = 0;
        image.width = 0;
        image.height = 0;

        for (int i = 0; i < pdf->n; i++)
        {
            if (vips_foreign_load_pdf_get_page(pdf, pdf->page_no + i) != 0)
                return -1;

            poppler_page_get_size(pdf.page, out width, out height);
            pages[i].left = 0;
            pages[i].top = top;
            pages[i].width = VIPS_RINT(width * total_scale);
            pages[i].height = VIPS_RINT(height * total_scale);

            if (pages[i].width > image.width)
                image.width = pages[i].width;

            image.height += pages[i].height;

            top += pages[i].height;
        }

        // ...

        return 0;
    }
}

// vips_foreign_load_pdf_generate
public class VipsForeignLoadPdf : VipsForeignLoadPdfBase
{
    protected override int Generate(VipsRegion out_region, void* seq, void* a, void* b, bool* stop)
    {
        // ...
    }
}

// vips_foreign_load_pdf_load
public class VipsForeignLoadPdf : VipsForeignLoadPdfBase
{
    protected override int Load()
    {
        // ...

        return 0;
    }
}

// vips_foreign_load_pdf_class_init
public class VipsForeignLoadPdfClass : VipsObjectClass
{
    public VipsForeignLoadPdfClass(Type type)
        : base(type)
    {
        // ...
    }
}

// vips_foreign_load_pdf_file_dispose
public class VipsForeignLoadPdfFile : VipsForeignLoadPdfBase
{
    protected override void Dispose()
    {
        VIPS_FREE(uri);
        base.Dispose();
    }
}

// vips_foreign_load_pdf_file_header
public class VipsForeignLoadPdfFile : VipsForeignLoadPdfBase
{
    protected override int Header()
    {
        // ...

        return 0;
    }
}

// vips_foreign_load_pdf_file_build
public class VipsForeignLoadPdfFile : VipsForeignLoadPdfBase
{
    protected override int Build()
    {
        // ...

        return 0;
    }
}

// vips_foreign_load_pdf_buffer_dispose
public class VipsForeignLoadPdfBuffer : VipsForeignLoadPdfBase
{
    protected override void Dispose()
    {
        base.Dispose();
    }
}

// vips_foreign_load_pdf_buffer_build
public class VipsForeignLoadPdfBuffer : VipsForeignLoadPdfBase
{
    protected override int Build()
    {
        // ...

        return 0;
    }
}

// vips_foreign_load_pdf_source_dispose
public class VipsForeignLoadPdfSource : VipsForeignLoadPdfBase
{
    protected override void Dispose()
    {
        base.Dispose();
    }
}

// vips_foreign_load_pdf_source_build
public class VipsForeignLoadPdfSource : VipsForeignLoadPdfBase
{
    protected override int Build()
    {
        // ...

        return 0;
    }
}
```