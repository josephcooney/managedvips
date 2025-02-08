Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignLoadPdf : VipsForeignLoad
{
    // Set by subclasses.
    public VipsSource Source { get; set; }

    // Load this page.
    public int PageNo { get; set; }

    // Load this many pages.
    public int N { get; set; }

    // Render at this DPI.
    public double Dpi { get; set; }

    // Calculate this from DPI. At 72 DPI, we render 1:1.
    public double Scale { get; set; }

    // Background colour.
    public VipsArrayDouble Background { get; set; }

    // Decrypt with this.
    public string Password { get; set; }

    private FPDF_FILEACCESS FileAccess;
    private FPDF_DOCUMENT Doc;
    private FPDF_PAGE Page;
    private FPDF_FORMFILLINFO FormCallbacks;
    private FPDF_FORMHANDLE Form;
    private int CurrentPage;

    // Doc has this many pages.
    public int NPages { get; set; }

    // We need to read out the size of each page we will render, and lay
    // them out in the final image.
    public VipsRect Image { get; set; }
    public VipsRect[] Pages { get; set; }

    // The [double] background converted to image format.
    public byte[] Ink { get; set; }

    public VipsForeignLoadPdf()
    {
        Dpi = 72.0;
        Scale = 1.0;
        N = 1;
        CurrentPage = -1;
        Background = new VipsArrayDouble(new double[] { 255.0 });
    }
}

public class VipsForeignLoadPdfFile : VipsForeignLoadPdf
{
    // Filename for load.
    public string Filename { get; set; }

    public VipsForeignLoadPdfFile()
    {
    }

    protected override int Build(VipsObject obj)
    {
        if (Filename != null && Source == null)
            Source = new VipsSource(new VipsBlob(Filename));

        return base.Build(obj);
    }
}

public class VipsForeignLoadPdfBuffer : VipsForeignLoadPdf
{
    // Load from a buffer.
    public VipsArea Buf { get; set; }

    public VipsForeignLoadPdfBuffer()
    {
    }

    protected override int Build(VipsObject obj)
    {
        if (Buf != null && Source == null)
            Source = new VipsSource(new VipsBlob(Buf));

        return base.Build(obj);
    }
}

public class VipsForeignLoadPdfSource : VipsForeignLoadPdf
{
    // Load from a source.
    public VipsSource Source { get; set; }

    public VipsForeignLoadPdfSource()
    {
    }

    protected override int Build(VipsObject obj)
    {
        if (Source != null)
            this.Source = Source;

        return base.Build(obj);
    }
}

public class VipsForeignLoadPdfClass : VipsForeignLoadPdf
{
    // This is the m_GetBlock function for FPDF_FILEACCESS.
    private static GMutex PdfiumMutex = new GMutex();

    public override int Build(VipsObject obj)
    {
        if (Source == null && Filename != null)
            Source = new VipsSource(new VipsBlob(Filename));

        return base.Build(obj);
    }

    protected override void Dispose(GObject gobject)
    {
        VipsForeignLoadPdf pdf = (VipsForeignLoadPdf)gobject;

        Close(pdf);

        base.Dispose(gobject);
    }

    private static void Close(VipsForeignLoadPdf pdf)
    {
        lock (PdfiumMutex)
        {
            FPDF_ClosePage(pdf.Page);
            FPDFDOC_ExitFormFillEnvironment(pdf.Form);
            FPDF_CloseDocument(pdf.Doc);
            VIPS_UNREF(pdf.Source);

            pdf.CurrentPage = -1;
        }
    }

    private static void PdfiumError()
    {
        int err = FPDF_GetLastError();

        if (err >= 0 && err < VipsForeignLoadPdfErrors.Length)
            Vips.Error("pdfload", "%s", VipsForeignLoadPdfErrors[err]);
        else
            Vips.Error("pdfload", "%s", "unknown error");
    }

    private static void PdfiumInitCB()
    {
        FPDF_LIBRARY_CONFIG config = new FPDF_LIBRARY_CONFIG();
        config.version = 2;
        config.m_pUserFontPaths = null;
        config.m_pIsolate = null;
        config.m_v8EmbedderSlot = 0;

        FPDF_InitLibraryWithConfig(ref config);
    }

    private static void GetBlock(VipsForeignLoadPdf pdf, unsigned long position, byte[] pBuf, unsigned long size)
    {
        // PDFium guarantees these.
        g_assert(size > 0);
        g_assert(position >= 0);
        g_assert(position + size <= pdf.FileAccess.m_FileLen);

        if (Vips.Source.Seek(pdf.Source, position, SeekSet) < 0)
            return;

        while (size > 0)
        {
            gint64 bytes_read = Vips.Source.Read(pdf.Source, pBuf, size);

            if (bytes_read < 0)
                return;
            pBuf += bytes_read;
            size -= bytes_read;
        }
    }

    private static int GetPage(VipsForeignLoadPdf pdf, int page_no)
    {
        if (pdf.CurrentPage != page_no)
        {
            lock (PdfiumMutex)
            {
                FPDF_ClosePage(pdf.Page);
                pdf.CurrentPage = -1;

#ifdef DEBUG
                Console.WriteLine("vips_foreign_load_pdf_get_page: " + page_no);
#endif /*DEBUG*/

                if (!(pdf.Page = FPDF_LoadPage(pdf.Doc, page_no)))
                {
                    PdfiumError();
                    Vips.Error(VipsForeignLoadPdfClass.Nickname,
                        _("unable to load page %d"), page_no);
                    return -1;
                }
                pdf.CurrentPage = page_no;
            }
        }

        return 0;
    }

    private static void SetImage(VipsForeignLoadPdf pdf, VipsImage out)
    {
        int i;
        double res;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_pdf_set_image: " + pdf);
#endif /*DEBUG*/

        // We render to a tilecache, so it has to be SMALLTILE.
        if (Vips.Image.Pipeline(out, Vips.DemandStyle.SmallTile, null) < 0)
            return;

        // Extract and attach metadata. Set the old name too for compat.
        Vips.Image.SetInt(out, "pdf-n_pages", pdf.NPages);
        Vips.Image.SetInt(out, VIPS_META_N_PAGES, pdf.NPages);

        lock (PdfiumMutex)
        {
            for (i = 0; i < n_metadata; i++)
            {
                VipsForeignLoadPdfMetadata metadata =
                    vips_foreign_load_pdf_metadata[i];

                char text = new char[1024];
                int len;

                len = FPDF_GetMetaText(pdf.Doc, metadata.tag, text, 1024);
                if (len > 0)
                {
                    string str;

                    // Silently ignore coding errors.
                    if ((str = g_utf16_to_utf8((gunichar2)text, len,
                            null, null, null)) != null)
                    {
                        Vips.Image.SetString(out,
                            metadata.field, str);
                        G.Free(str);
                    }
                }
            }
        }

        // We need pixels/mm for vips.
        res = pdf.Dpi / 25.4;

        Vips.Image.InitFields(out,
            pdf.Image.Width, pdf.Image.Height,
            4, VIPS_FORMAT_UCHAR,
            VIPS_CODING_NONE, VIPS_INTERPRETATION_sRGB, res, res);
    }

    private static int Header(VipsForeignLoad load)
    {
        VipsObjectClass class = Vips.Object.GetClass(load);

        lock (PdfiumMutex)
        {
            pdf.NPages = FPDF_GetPageCount(pdf.Doc);
        }

        // @n == -1 means until the end of the doc.
        if (pdf.N == -1)
            pdf.N = pdf.NPages - pdf.PageNo;

        if (pdf.PageNo + pdf.N > pdf.NPages ||
            pdf.PageNo < 0 ||
            pdf.N <= 0)
        {
            Vips.Error(class.Nickname, "%s", _("pages out of range"));
            return -1;
        }

        // Lay out the pages in our output image.
        if (!(pdf.Pages = new VipsRect[pdf.N]))
            return -1;

        int top = 0;
        pdf.Image.Left = 0;
        pdf.Image.Top = 0;
        pdf.Image.Width = 0;
        pdf.Image.Height = 0;
        for (i = 0; i < pdf.N; i++)
        {
            if (GetPage(pdf, pdf.PageNo + i) < 0)
                return -1;
            pdf.Pages[i].Left = 0;
            pdf.Pages[i].Top = top;
            // We do round to nearest, in the same way that vips_resize()
            // does round to nearest. Without this, things like
            // shrink-on-load will break.
            pdf.Pages[i].Width = (int)(FPDF_GetPageWidth(pdf.Page) * pdf.Scale);
            pdf.Pages[i].Height = (int)(FPDF_GetPageHeight(pdf.Page) * pdf.Scale);

            // PDFium allows page width or height to be less than 1 (!!).
            if (pdf.Pages[i].Width < 1 ||
                pdf.Pages[i].Height < 1 ||
                pdf.Pages[i].Width > VIPS_MAX_COORD ||
                pdf.Pages[i].Height > VIPS_MAX_COORD)
            {
                Vips.Error(class.Nickname,
                    "%s", _("page size out of range"));
                return -1;
            }

            if (pdf.Pages[i].Width > pdf.Image.Width)
                pdf.Image.Width = pdf.Pages[i].Width;
            pdf.Image.Height += pdf.Pages[i].Height;

            top += pdf.Pages[i].Height;
        }

        // If all pages are the same height, we can tag this as a toilet roll
        // image.
        for (i = 1; i < pdf.N; i++)
            if (pdf.Pages[i].Height != pdf.Pages[0].Height)
                break;

        // Only set page-height if we have more than one page, or this could
        // accidentally turn into an animated image later.
        if (pdf.N > 1)
            Vips.Image.SetInt(load.Out,
                VIPS_META_PAGE_HEIGHT, pdf.Pages[0].Height);

        SetImage(pdf, load.Out);

        // Convert the background to the image format.
        if (!(pdf.Ink = Vips.VectorToInk(class.Nickname,
              load.Out,
              new double[] { 255.0 }, null,
              new int[] { 1 })))
            return -1;
        Vips.BGRA2RGBA((uint[])pdf.Ink, 1);

        return 0;
    }

    private static void Minimise(VipsObject obj, VipsForeignLoadPdf pdf)
    {
        Vips.Source.Minimise(pdf.Source);
    }

    private static int Generate(VipsRegion out_region,
        void* seq, void* a, void* b, bool* stop)
    {
        VipsForeignLoadPdf pdf = (VipsForeignLoadPdf)a;
        VipsRect r = out_region.Valid;

        // Search through the pages we are drawing for the first containing
        // this rect. This could be quicker, perhaps a binary search, but who
        // cares.
        int i;
        for (i = 0; i < pdf.N; i++)
            if (Vips.Rect.Bottom(pdf.Pages[i]) > r.Top)
                break;

        // Reset out region. Otherwise there might be parts of previous pages
        // left.
        Vips.Region.Black(out_region);

        int top = r.Top;
        while (top < Vips.Rect.Bottom(r))
        {
            VipsRect rect;
            FPDF_BITMAP bitmap;

            Vips.Rect.IntersectRect(ref r, ref pdf.Pages[i], ref rect);

            if (GetPage(pdf, pdf.PageNo + i) < 0)
                return -1;

            lock (PdfiumMutex)
            {
                // 4 means RGBA.
                bitmap = FPDFBitmap_CreateEx(rect.Width, rect.Height, 4,
                    Vips.Region.Addr(out_region, rect.Left, rect.Top),
                    Vips.Region.LSkip(out_region));

                // Only paint the background if there's no transparency.
                if (!FPDFPage_HasTransparency(pdf.Page))
                {
                    uint ink = (uint)pdf.Ink[0];

                    FPDFBitmap_FillRect(bitmap,
                        0, 0, rect.Width, rect.Height, ink);
                }

                // pdfium writes bgra by default, we need rgba
                FPDF_RenderPageBitmap(bitmap, pdf.Page,
                    pdf.Pages[i].Left - rect.Left,
                    pdf.Pages[i].Top - rect.Top,
                    pdf.Pages[i].Width, pdf.Pages[i].Height,
                    0, FPDF_ANNOT | FPDF_REVERSE_BYTE_ORDER);

                FPDF_FFLDraw(pdf.Form, bitmap, pdf.Page,
                    pdf.Pages[i].Left - rect.Left,
                    pdf.Pages[i].Top - rect.Top,
                    pdf.Pages[i].Width, pdf.Pages[i].Height,
                    0, FPDF_ANNOT | FPDF_REVERSE_BYTE_ORDER);

                FPDFBitmap_Destroy(bitmap);
            }

            top += rect.Height;
            i++;
        }

        return 0;
    }

    private static int Load(VipsForeignLoad load)
    {
        VipsImage[] t = new VipsImage[2];

        // Read to this image, then cache to out, see below.
        t[0] = new VipsImage();

        // Close input immediately at end of read.
        t[0].Connect("minimise",
            (s, a) => Minimise(a, load as VipsForeignLoadPdf));

        SetImage(load.Out, t[0]);

        if (Vips.Image.Generate(t[0],
                null, Generate, null, load as VipsForeignLoadPdf,
                null) < 0 ||
            Vips.Tilecache(t[0], ref t[1],
                "tile_width", TILE_SIZE,
                "tile_height", TILE_SIZE,
                "max_tiles", 2 * (1 + t[0].Xsize / TILE_SIZE),
                null) < 0 ||
            Vips.Image.Write(t[1], load.Real))
            return -1;

        return 0;
    }

    public static void OnceInit(void* client)
    {
        // We must make the mutex on class init (not _build) since we
        // can lock even if build is not called.
        PdfiumMutex = new GMutex();
    }
}

public class VipsForeignLoadPdfFileClass : VipsForeignLoadPdfClass
{
    public override int Build(VipsObject obj)
    {
        if (Filename != null && Source == null)
            Source = new VipsSource(new VipsBlob(Filename));

        return base.Build(obj);
    }

    protected override void Dispose(GObject gobject)
    {
        VipsForeignLoadPdfFile file = (VipsForeignLoadPdfFile)gobject;

        Close(file);

        base.Dispose(gobject);
    }

    private static void Close(VipsForeignLoadPdfFile file)
    {
        lock (PdfiumMutex)
        {
            FPDF_ClosePage(file.Page);
            FPDFDOC_ExitFormFillEnvironment(file.Form);
            FPDF_CloseDocument(file.Doc);
            VIPS_UNREF(file.Source);

            file.CurrentPage = -1;
        }
    }

    public static void ClassInit(VipsForeignLoadPdfClass class_)
    {
        // We must make the mutex on class init (not _build) since we
        // can lock even if build is not called.
        PdfiumMutex = new GMutex();
    }
}

public class VipsForeignLoadPdfBufferClass : VipsForeignLoadPdfClass
{
    public override int Build(VipsObject obj)
    {
        if (Buf != null && Source == null)
            Source = new VipsSource(new VipsBlob(Buf));

        return base.Build(obj);
    }

    protected override void Dispose(GObject gobject)
    {
        VipsForeignLoadPdfBuffer buffer = (VipsForeignLoadPdfBuffer)gobject;

        Close(buffer);

        base.Dispose(gobject);
    }

    private static void Close(VipsForeignLoadPdfBuffer buffer)
    {
        lock (PdfiumMutex)
        {
            FPDF_ClosePage(buffer.Page);
            FPDFDOC_ExitFormFillEnvironment(buffer.Form);
            FPDF_CloseDocument(buffer.Doc);
            VIPS_UNREF(buffer.Source);

            buffer.CurrentPage = -1;
        }
    }

    public static void ClassInit(VipsForeignLoadPdfClass class_)
    {
        // We must make the mutex on class init (not _build) since we
        // can lock even if build is not called.
        PdfiumMutex = new GMutex();
    }
}

public class VipsForeignLoadPdfSourceClass : VipsForeignLoadPdfClass
{
    public override int Build(VipsObject obj)
    {
        if (Source != null)
            this.Source = Source;

        return base.Build(obj);
    }

    protected override void Dispose(GObject gobject)
    {
        VipsForeignLoadPdfSource source = (VipsForeignLoadPdfSource)gobject;

        Close(source);

        base.Dispose(gobject);
    }

    private static void Close(VipsForeignLoadPdfSource source)
    {
        lock (PdfiumMutex)
        {
            FPDF_ClosePage(source.Page);
            FPDFDOC_ExitFormFillEnvironment(source.Form);
            FPDF_CloseDocument(source.Doc);
            VIPS_UNREF(source.Source);

            source.CurrentPage = -1;
        }
    }

    public static void ClassInit(VipsForeignLoadPdfClass class_)
    {
        // We must make the mutex on class init (not _build) since we
        // can lock even if build is not called.
        PdfiumMutex = new GMutex();
    }
}
```

Note that I've assumed some types and methods are available in your environment, such as `Vips`, `GObject`, `GMutex`, etc. Also, I've used C# syntax and naming conventions to make the code more readable. If you need any further assistance or clarification, please let me know!