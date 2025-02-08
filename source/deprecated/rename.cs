Here is the C# code equivalent to the provided C code:

```csharp
// im_remainderconst_vec.c --- wrappers for various renamed functions

using System;
using VipsDotNet;

public class Rename
{
    // im_remainderconst_vec
    public static int ImRemainderConstVec(IMAGE inImage, IMAGE outImage, int n, double[] c)
    {
        return ImRemainderVec(inImage, outImage, n, c);
    }

    // im_and_vec
    public static int ImAndVec(IMAGE inImage, IMAGE outImage, int n, double[] c)
    {
        return ImAndimageVec(inImage, outImage, n, c);
    }

    // im_or_vec
    public static int ImOrVec(IMAGE inImage, IMAGE outImage, int n, double[] c)
    {
        return ImOriimageVec(inImage, outImage, n, c);
    }

    // im_eor_vec
    public static int ImEorVec(IMAGE inImage, IMAGE outImage, int n, double[] c)
    {
        return ImEoriimageVec(inImage, outImage, n, c);
    }

    // im_andconst
    public static int ImAndConst(IMAGE inImage, IMAGE outImage, double c)
    {
        return ImAndimageConst(inImage, outImage, c);
    }

    // im_orconst
    public static int ImOrConst(IMAGE inImage, IMAGE outImage, double c)
    {
        return ImOriimageConst(inImage, outImage, c);
    }

    // im_eorconst
    public static int ImEorConst(IMAGE inImage, IMAGE outImage, double c)
    {
        return ImEoriimageConst(inImage, outImage, c);
    }

    // im_errormsg
    public static void ImErrorMsg(string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);
        ImVerror("untranslated", fmt, ap);
    }

    // im_verrormsg
    public static void ImVerrormsg(string fmt, VipsVaList ap)
    {
        ImVerror("untranslated", fmt, ap);
    }

    // im_errormsg_system
    public static void ImErrorMsgSystem(int err, string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);
        ImVerrorSystem(err, "untranslated", fmt, ap);
    }

    // im_diagnostics
    public static void ImDiagnostics(string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);
        ImVDiag("untranslated", fmt, ap);
    }

    // im_warning
    public static void ImWarning(string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);
        ImVWarn("untranslated", fmt, ap);
    }

    // vips_g_thread_join
    public static IntPtr VipsGThreadJoin(GThread thread)
    {
        return GThread.Join(thread);
    }

    // im_affine
    public static int ImAffine(IMAGE inImage, IMAGE outImage,
        double a, double b, double c, double d, double dx, double dy,
        int ox, int oy, int ow, int oh)
    {
        return ImAffinei(inImage, outImage,
            VipsInterpolateBilinearStatic(),
            a, b, c, d, dx, dy,
            ox, oy, ow, oh);
    }

    // im_similarity_area
    public static int ImSimilarityArea(IMAGE inImage, IMAGE outImage,
        double a, double b, double dx, double dy,
        int ox, int oy, int ow, int oh)
    {
        return ImAffinei(inImage, outImage,
            VipsInterpolateBilinearStatic(),
            a, -b, b, a, dx, dy,
            ox, oy, ow, oh);
    }

    // im_similarity
    public static int ImSimilarity(IMAGE inImage, IMAGE outImage,
        double a, double b, double dx, double dy)
    {
        return ImAffineiAll(inImage, outImage,
            VipsInterpolateBilinearStatic(),
            a, -b, b, a, dx, dy);
    }

    // im_measure
    public static DOUBLEMASK[] ImMeasure(IMAGE im, IMAGE_BOX box, int h, int v,
        int[] sel, int nsel, string name)
    {
        return ImMeasureArea(im,
            box.XStart,
            box.YStart,
            box.XSize,
            box.YSize,
            h, v, sel, nsel, name);
    }

    // im_extract
    public static int ImExtract(IMAGE inImage, IMAGE outImage, IMAGE_BOX box)
    {
        if (box.ChSel == -1)
            return ImExtractAreabands(inImage, outImage,
                box.XStart, box.YStart, box.XSize, box.YSize,
                0, inImage.Bands);
        else
            return ImExtractAreabands(inImage, outImage,
                box.XStart, box.YStart, box.XSize, box.YSize,
                box.ChSel, 1);
    }

    // im_render_fade
    public static int ImRenderFade(IMAGE inImage, IMAGE outImage, IMAGE mask,
        int width, int height, int max,
        int fps, int steps,
        int priority,
        NotifyFn notify, object client)
    {
        return ImRenderPriority(inImage, outImage, mask,
            width, height, max,
            priority,
            notify, client);
    }

    // im_render
    public static int ImRender(IMAGE inImage, IMAGE outImage, IMAGE mask,
        int width, int height, int max,
        NotifyFn notify, object client)
    {
        return ImRenderPriority(
            inImage, outImage, mask,
            width, height, max,
            0, notify, client);
    }

    // im_makerw
    public static int ImMakerw(IMAGE im)
    {
        return ImRwcheck(im);
    }

    // im_icc_export
    public static int ImIccExport(IMAGE inImage, IMAGE outImage,
        string outputProfileFilename, int intent)
    {
        return ImIccExportDepth(inImage, outImage,
            8, outputProfileFilename, (VipsIntent)intent);
    }

    // im_segment
    public static int ImSegment(IMAGE test, IMAGE mask, int[] segments)
    {
        return ImLabelRegions(test, mask, segments);
    }

    // im_convf
    public static int ImConvf(IMAGE inImage, IMAGE outImage, DOUBLEMASK mask)
    {
        return ImConvF(inImage, outImage, mask);
    }

    // im_convf_raw
    public static int ImConvfRaw(IMAGE inImage, IMAGE outImage, DOUBLEMASK mask)
    {
        return ImConvFRaw(inImage, outImage, mask);
    }

    // im_convsepf
    public static int ImConvsepF(IMAGE inImage, IMAGE outImage, DOUBLEMASK mask)
    {
        return ImConvSepF(inImage, outImage, mask);
    }

    // im_convsepf_raw
    public static int ImConvsepFRaw(IMAGE inImage, IMAGE outImage, DOUBLEMASK mask)
    {
        return ImConvSepFRaw(inImage, outImage, mask);
    }

    // im_isint
    public static bool ImIsInt(IMAGE im)
    {
        return VipsBandFmt.IsInt(im.BandFmt);
    }

    // im_isuint
    public static bool ImIsUint(IMAGE im)
    {
        return VipsBandFmt.IsUint(im.BandFmt);
    }

    // im_isfloat
    public static bool ImIsFloat(IMAGE im)
    {
        return VipsBandFmt.IsFloat(im.BandFmt);
    }

    // im_iscomplex
    public static bool ImIsComplex(IMAGE im)
    {
        return VipsBandFormat.IsComplex(im.BandFmt);
    }

    // im_isscalar
    public static bool ImIsScalar(IMAGE im)
    {
        return !ImIsComplex(im);
    }

    // im_c2ps
    public static int ImC2Ps(IMAGE inImage, IMAGE outImage)
    {
        return ImAbs(inImage, outImage);
    }

    // im_clip
    public static int ImClip(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.UChar);
    }

    // im_clip2c
    public static int ImClip2C(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.Char);
    }

    // im_clip2us
    public static int ImClip2Us(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.UShort);
    }

    // im_clip2s
    public static int ImClip2S(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.Short);
    }

    // im_clip2ui
    public static int ImClip2Ui(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.UInt);
    }

    // im_clip2i
    public static int ImClip2I(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.Int);
    }

    // im_clip2f
    public static int ImClip2F(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.Float);
    }

    // im_clip2d
    public static int ImClip2D(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.Double);
    }

    // im_clip2cm
    public static int ImClip2Cm(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.Complex);
    }

    // im_clip2dcm
    public static int ImClip2Dcm(IMAGE inImage, IMAGE outImage)
    {
        return ImClip2Fmt(inImage, outImage, VipsBandFmt.DComplex);
    }

    // im_copy_from
    public static int ImCopyFrom(IMAGE inImage, IMAGE outImage, ImageArchitecture architecture)
    {
        switch (architecture)
        {
            case ImageArchitecture.Native:
                return ImCopy(inImage, outImage);

            case ImageArchitecture.ByteSwapped:
                return ImCopySwap(inImage, outImage);

            case ImageArchitecture.LsbFirst:
                return ImAmiMSBfirst() ? ImCopySwap(inImage, outImage) : ImCopy(inImage, outImage);

            case ImageArchitecture.MsbFirst:
                return ImAmiMSBfirst() ? ImCopy(inImage, outImage) : ImCopySwap(inImage, outImage);

            default:
                ImError("im_copy_from", _("bad architecture: %d"), architecture);
                return -1;
        }
    }

    // im_isnative
    public static bool ImIsNative(ImageArchitecture arch)
    {
        switch (arch)
        {
            case ImageArchitecture.Native:
                return true;

            case ImageArchitecture.ByteSwapped:
                return false;

            case ImageArchitecture.LsbFirst:
                return !ImAmiMSBfirst();

            case ImageArchitecture.MsbFirst:
                return ImAmiMSBfirst();

            default:
                g_assert(0);
        }

        // Keep -Wall happy.
        return -1;
    }

    // im_iterate
    public static int ImIterate(IMAGE im,
        StartFn start, GenerateFn generate, StopFn stop,
        object b, object c)
    {
        return VipsSink(im, start, (VipsGenerateFn)generate, stop, b, c);
    }

    // im_render_priority
    public static int ImRenderPriority(IMAGE inImage, IMAGE outImage, IMAGE mask,
        int width, int height, int max,
        int priority,
        NotifyFn notify, object client)
    {
        return VipsSinkScreen(inImage, outImage, mask,
            width, height, max, priority, notify, client);
    }

    // vips_rawsave_fd
    public static int VipsRawSaveFd(VipsImage inImage, int fd, params object[] args)
    {
        var ap = new VipsVaList(args);
        return VipsCallSplit("rawsave_target", ap, inImage, VipsTarget.NewToDescriptor(fd));
    }

    // im_circle
    public static int ImCircle(IMAGE im, int cx, int cy, int radius, int intensity)
    {
        PEL ink = new PEL { Value = (byte)intensity };

        if (ImRwcheck(im) ||
            ImCheckUncoded("im_circle", im) ||
            ImCheckMono("im_circle", im) ||
            ImCheckFormat("im_circle", im, VipsBandFmt.UChar))
            return -1;

        return ImDrawCircle(im, cx, cy, radius, false, ink);
    }

    // im_flood_copy
    public static int ImFloodCopy(IMAGE inImage, IMAGE outImage, int x, int y, PEL[] ink)
    {
        IMAGE t = ImOpenLocal(outImage, "im_flood_blob_copy", "t");

        if (t == null ||
            ImCopy(inImage, t) ||
            ImFlood(t, x, y, ink, null) ||
            ImCopy(t, outImage))
            return -1;

        return 0;
    }

    // im_flood_blob_copy
    public static int ImFloodBlobCopy(IMAGE inImage, IMAGE outImage, int x, int y, PEL[] ink)
    {
        IMAGE t = ImOpenLocal(outImage, "im_flood_blob_copy", "t");

        if (t == null ||
            ImCopy(inImage, t) ||
            ImFloodBlob(t, x, y, ink, null) ||
            ImCopy(t, outImage))
            return -1;

        return 0;
    }

    // im_flood_other_copy
    public static int ImFloodOtherCopy(IMAGE test, IMAGE mark, IMAGE outImage,
        int x, int y, int serial)
    {
        IMAGE t = ImOpenLocal(outImage, "im_flood_other_copy", "t");

        if (t == null ||
            ImCopy(mark, t) ||
            ImFloodOther(test, t, x, y, serial, null) ||
            ImCopy(t, outImage))
            return -1;

        return 0;
    }

    // im_paintrect
    public static int ImPaintRect(IMAGE im, Rect r, PEL[] ink)
    {
        return ImDrawRect(im,
            r.Left, r.Top, r.Width, r.Height, 1, ink);
    }

    // im_insertplace
    public static int ImInsertPlace(IMAGE main, IMAGE sub, int x, int y)
    {
        return ImDrawImage(main, sub, x, y);
    }

    // im_fastline
    public static int ImFastLine(IMAGE im, int x1, int y1, int x2, int y2, PEL[] pel)
    {
        return ImDrawLine(im, x1, y1, x2, y2, pel);
    }

    // im_fastlineuser
    public static int ImFastLineUser(IMAGE im,
        int x1, int y1, int x2, int y2,
        VipsPlotFn fn, object client1, object client2, object client3)
    {
        return ImDrawLineUser(im, x1, y1, x2, y2,
            fn, client1, client2, client3);
    }

    // im_plotmask
    public static int ImPlotMask(IMAGE im, int ix, int iy, PEL[] ink, PEL[] mask, Rect r)
    {
        IMAGE maskIm = ImImage(mask,
            r.Width, r.Height, 1, VipsBandFmt.UChar);

        if (maskIm == null ||
            ImDrawMask(im, maskIm, ix + r.Left, iy + r.Top, ink))
        {
            ImClose(maskIm);
            return -1;
        }

        ImClose(maskIm);

        return 0;
    }

    // im_readpoint
    public static int ImReadPoint(IMAGE im, int x, int y, PEL[] pel)
    {
        return ImReadPoint(im, x, y, pel);
    }

    // im_plotpoint
    public static int ImPlotPoint(IMAGE im, int x, int y, PEL[] pel)
    {
        return ImDrawPoint(im, x, y, pel);
    }

    // im_smear
    public static int ImSmear(IMAGE im, int ix, int iy, Rect r)
    {
        int x, y, a, b, c;
        int ba = im.Bands;
        int el = ba * im.Xsize;
        Rect area, image, clipped;
        double[] total = new double[256];

        if (ImRwcheck(im))
            return -1;

        // Don't do the margins.
        area = r;
        area.Left += ix;
        area.Top += iy;
        image.Left = 0;
        image.Top = 0;
        image.Width = im.Xsize;
        image.Height = im.Ysize;
        ImRectMarginAdjust(&image, -1);
        image.Left--;
        ImRectIntersectRect(&area, &image, &clipped);

        // Any left?
        if (ImRectIsEmpty(&clipped))
            return 0;

/* What we do for each type.
 */
#define SMEAR(TYPE) \
    for (y = clipped.Top; y < clipped.Top + clipped.Height; y++) \
        for (x = clipped.Left; \
                x < clipped.Left + clipped.Width; x++) { \
            TYPE *to = (TYPE *)im.data + x * ba + y * el; \
            TYPE *from = to - el; \
            TYPE *f; \

        // Loop through the remaining pixels.
        switch (im.BandFmt)
        {
            case VipsBandFmt.UChar:
                SMEAR(unsigned char);
                break;

            case VipsBandFmt.Char:
                SMEAR(char);
                break;

            case VipsBandFmt.UShort:
                SMEAR(unsigned short);
                break;

            case VipsBandFmt.Short:
                SMEAR(short);
                break;

            case VipsBandFmt.UInt:
                SMEAR(unsigned int);
                break;

            case VipsBandFmt.Int:
                SMEAR(int);
                break;

            case VipsBandFmt.Float:
                SMEAR(float);
                break;

            case VipsBandFmt.Double:
                SMEAR(double);
                break;

            // Do complex types too. Just treat as float and double, but with
            // twice the number of bands.
            case VipsBandFmt.Complex:
                // Twice number of bands: double size and bands.
                ba *= 2;
                el *= 2;

                SMEAR(float);

                break;

            case VipsBandFmt.DComplex:
                // Twice number of bands: double size and bands.
                ba *= 2;
                el *= 2;

                SMEAR(double);

                break;

            default:
                ImError("im_smear", "%s", _("unknown band format"));
                return -1;
        }

        return 0;
    }

    // im_smudge
    public static int ImSmudge(VipsImage image, int ix, int iy, Rect r)
    {
        return ImDrawSmudge(image,
            r.Left + ix, r.Top + iy, r.Width, r.Height);
    }

    // im_flood
    public static int ImFlood(IMAGE im, int x, int y, PEL[] ink, Rect dout)
    {
        return ImDrawFlood(im, x, y, ink, dout);
    }

    // im_flood_blob
    public static int ImFloodBlob(IMAGE im, int x, int y, PEL[] ink, Rect dout)
    {
        return ImDrawFloodBlob(im, x, y, ink, dout);
    }

    // im_flood_other
    public static int ImFloodOther(IMAGE test, IMAGE mark,
        int x, int y, int serial, Rect dout)
    {
        return ImDrawFloodOther(mark, test, x, y, serial, dout);
    }

    // vips_check_coding_rad
    public static int VipsCheckCodingRad(string domain, VipsImage im)
    {
        return VipsCheckCoding(domain, im, VipsCoding.Rad);
    }

    // vips_check_coding_labq
    public static int VipsCheckCodingLabq(string domain, VipsImage im)
    {
        return VipsCheckCoding(domain, im, VipsCoding.LabQ);
    }

    // vips_check_bands_3ormore
    public static int VipsCheckBands3OrMore(string domain, VipsImage im)
    {
        return VipsCheckBandsAtLeast(domain, im, 3);
    }

    // vips_info_set
    public static void VipsInfoSet(bool info)
    {
        Vips__info = info;

        if (info)
        {
            const string old = g_getenv("G_MESSAGES_DEBUG");
            if (!old)
                old = "";
            var newEnv = g_strdup_printf("%s VIPS", old);
            g_setenv("G_MESSAGES_DEBUG", newEnv, true);
            g_free(newEnv);
        }
    }

    // vips_vinfo
    public static void VipsVInfo(string domain, string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);

        if (Vips__info)
        {
            g_mutex_lock(Vips__global_lock);
            Console.Error.WriteLine($"({_("info")}) {_("untranslated")}: {domain}");
            Console.Error.WriteLine(fmt, ap);
            Console.Error.WriteLine();
            g_mutex_unlock(Vips__global_lock);
        }
    }

    // vips_info
    public static void VipsInfo(string domain, string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);

        VipsVinfo(domain, fmt, ap);
    }

    // vips_vwarn
    public static void VipsVWarn(string domain, string fmt, params object[] args)
    {
        if (!g_getenv("IM_WARNING") &&
            !g_getenv("VIPS_WARNING"))
        {
            g_mutex_lock(Vips__global_lock);
            Console.Error.WriteLine($"({_("vips warning")}) {_("untranslated")}: {domain}");
            Console.Error.WriteLine(fmt, ap);
            Console.Error.WriteLine();
            g_mutex_unlock(Vips__global_lock);
        }

        if (Vips__fatal)
            VipsErrorExit("vips__fatal");
    }

    // vips_warn
    public static void VipsWarn(string domain, string fmt, params object[] args)
    {
        var ap = new VipsVaList(args);

        VipsVwarn(domain, fmt, ap);
    }

    // vips_autorot_get_angle
    public static VipsAngle VipsAutorotGetAngle(VipsImage im)
    {
        return VipsAngle.D0;
    }

    // vips_free
    public static int VipsFree(IntPtr buf)
    {
        g_free(buf);

        return 0;
    }

    // vips_thread_isvips
    public static bool VipsThreadIsVips()
    {
        return VipsThreadIsVips();
    }

    // vips_cache_operation_add
    public static void VipsCacheOperationAdd(VipsOperation operation)
    {
    }

    // vips_cache_operation_lookup
    public static VipsOperation? VipsCacheOperationLookup(VipsOperation operation)
    {
        return null;
    }

    // vips_target_finish
    public static void VipsTargetFinish(VipsTarget target)
    {
        VipsTargetEnd(target);
    }

    // vips_strncpy
    public static string VipsStrncpy(string dest, string src, int n)
    {
        g_strlcpy(dest, src, n);

        return dest;
    }

    // vips_strrstr
    public static string VipsStrrstr(string haystack, string needle)
    {
        return g_strrstr(haystack, needle);
    }

    // vips_ispostfix
    public static bool VipsIsPostfix(string a, string b)
    {
        return g_str_has_suffix(a, b);
    }

    // vips_vsnprintf
    public static int VipsVsnprintf(string str, size_t size, string format, params object[] args)
    {
        var ap = new VipsVaList(args);

        return g_vsnprintf(str, size, format, ap);
    }

    // vips_snprintf
    public static int VipsSnprintf(string str, size_t size, string format, params object[] args)
    {
        var ap = new VipsVaList(args);

        return g_vsnprintf(str, size, format, ap);
    }
}
```

Note that I've assumed the existence of certain classes and methods in the `VipsDotNet` namespace, which are not shown here. You will need to implement these yourself or use an existing implementation.

Also note that this is a direct translation of the C code, without any optimizations or improvements for the .NET platform. You may want to consider refactoring the code to take advantage of .NET features and best practices.