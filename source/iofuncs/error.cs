Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Vips
{
    public class Error
    {
        // vips_error_freeze:
        public static void Freeze()
        {
            lock (Vips.GlobalLock)
            {
                if (Vips.ErrorFreezeCount >= 0)
                    Vips.ErrorFreezeCount++;
            }
        }

        // vips_error_thaw:
        public static void Thaw()
        {
            lock (Vips.GlobalLock)
            {
                Vips.ErrorFreezeCount--;
                g_assert(Vips.ErrorFreezeCount >= 0);
            }
        }

        // vips_error_buffer:
        public static string Buffer
        {
            get
            {
                lock (Vips.GlobalLock)
                {
                    return Vips.ErrorBuf.All();
                }
            }
        }

        // vips_error_buffer_copy:
        public static string CopyBuffer()
        {
            lock (Vips.GlobalLock)
            {
                var buffer = new List<string>(Vips.ErrorBuf.All().Split('\n'));
                Vips.ErrorBuf.Rewind();
                return string.Join("\n", buffer);
            }
        }

        // vips_verror:
        public static void VerError(string domain, string fmt, params object[] args)
        {
#ifdef DEBUG
            var txt = new List<string>();
            foreach (var arg in args)
                txt.Add(arg.ToString());
            VIPS_DEBUG_MSG("vips_verror: {0}", string.Join(", ", txt));
#endif

            lock (Vips.GlobalLock)
            {
                if (!Vips.ErrorFreezeCount.HasValue || Vips.ErrorFreezeCount.Value == 0)
                {
                    if (!string.IsNullOrEmpty(domain))
                        Vips.ErrorBuf.AppendFormat("{0}: ", domain);
                    Vips.ErrorBuf.AppendFormat(fmt, args);
                    Vips.ErrorBuf.AppendLine();
                }
            }

            if (Vips.Fatal)
                Vips.ErrorExit("vips__fatal");
        }

        // vips_error:
        public static void Error(string domain, string fmt, params object[] args)
        {
            var vaList = new List<object>(args);
            VerError(domain, fmt, vaList.ToArray());
        }

        // vips_verror_system:
        public static void VerErrorSystem(int err, string domain, string fmt, params object[] args)
        {
            VerError(domain, fmt, args);
            Vips.Error(_("system error"), "%s", g_strerror(err));
        }

        // vips_error_system:
        public static void ErrorSystem(int err, string domain, string fmt, params object[] args)
        {
            var vaList = new List<object>(args);
            VerErrorSystem(err, domain, fmt, vaList.ToArray());
        }

        // vips_error_g:
        public static void ErrorG(out GError error)
        {
            if (error == null)
                error = new GError(VIPS_DOMAIN, -1);

            lock (Vips.GlobalLock)
            {
                var buffer = Vips.ErrorBuffer();
                g_set_error(ref error, VIPS_DOMAIN, -1, "%s", buffer);
                Vips.ErrorClear();
            }
        }

        // vips_g_error:
        public static void GError(GError error)
        {
            if (error != null)
            {
                Vips.Error("glib", "%s\n", error.Message);
                g_error_free(error);
                error = null;
            }
        }

        // vips_error_clear:
        public static void ErrorClear()
        {
            lock (Vips.GlobalLock)
            {
                Vips.ErrorBuf.Rewind();
            }
        }

        // vips_error_exit:
        public static void ErrorExit(string fmt, params object[] args)
        {
            if (!string.IsNullOrEmpty(fmt))
            {
                var vaList = new List<object>(args);
                Console.Error.WriteLine("{0}: {1}", Vips.GetPrgname(), string.Format(CultureInfo.InvariantCulture, fmt, vaList.ToArray()));
            }

            Console.Error.WriteLine(Vips.ErrorBuffer());

            Vips.Shutdown();

            if (Vips.Fatal)
                Environment.Exit(1);
            else
                throw new Exception("vips_error_exit");
        }

        // vips_check_uncoded:
        public static int CheckUncoded(string domain, VipsImage image)
        {
            if (image.Coding != VIPS_CODING_NONE)
            {
                Vips.Error(domain, "%s", _("image must be uncoded"));
                return -1;
            }

            return 0;
        }

        // vips_check_coding_noneorlabq:
        public static int CheckCodingNoneOrLabq(string domain, VipsImage image)
        {
            if (image.Coding != VIPS_CODING_NONE && image.Coding != VIPS_CODING_LABQ)
            {
                Vips.Error(domain, "%s", _("image coding must be 'none' or 'labq'"));
                return -1;
            }

            return 0;
        }

        // vips_check_coding_known:
        public static int CheckCodingKnown(string domain, VipsImage image)
        {
            if (image.Coding != VIPS_CODING_NONE && image.Coding != VIPS_CODING_LABQ && image.Coding != VIPS_CODING_RAD)
            {
                Vips.Error(domain, "%s", _("unknown image coding"));
                return -1;
            }

            return 0;
        }

        // vips_check_coding:
        public static int CheckCoding(string domain, VipsImage image, VipsCoding coding)
        {
            if (image.Coding != coding)
            {
                Vips.Error(domain, _("coding '%s' only"), Vips.EnumNick(VIPS_TYPE_CODING, coding));
                return -1;
            }

            return 0;
        }

        // vips_check_mono:
        public static int CheckMono(string domain, VipsImage image)
        {
            if (image.Bands != 1)
            {
                Vips.Error(domain, "%s", _("image must one band"));
                return -1;
            }

            return 0;
        }

        // vips_check_bands:
        public static int CheckBands(string domain, VipsImage image, int bands)
        {
            if (image.Bands != bands)
            {
                Vips.Error(domain, _("image must have %d bands"), bands);
                return -1;
            }

            return 0;
        }

        // vips_check_bands_1or3:
        public static int CheckBands1Or3(string domain, VipsImage image)
        {
            if (image.Bands != 1 && image.Bands != 3)
            {
                Vips.Error(domain, "%s", _("image must have one or three bands"));
                return -1;
            }

            return 0;
        }

        // vips_check_bands_atleast:
        public static int CheckBandsAtLeast(string domain, VipsImage image, int bands)
        {
            if (image.Bands < bands)
            {
                Vips.Error(domain, _("image must have at least %d bands"), bands);
                return -1;
            }

            return 0;
        }

        // vips_check_bands_1orn:
        public static int CheckBands1Orn(string domain, VipsImage image1, VipsImage image2)
        {
            if (image1.Bands != image2.Bands && !(image1.Bands == 1 || image2.Bands == 1))
            {
                Vips.Error(domain, "%s", _("images must have the same number of bands, or one must be single-band"));
                return -1;
            }

            return 0;
        }

        // vips_check_bands_1orn_unary:
        public static int CheckBands1OrnUnary(string domain, VipsImage image, int n)
        {
            if (image.Bands != 1 && image.Bands != n)
            {
                Vips.Error(domain, _("image must have 1 or %d bands"), n);
                return -1;
            }

            return 0;
        }

        // vips_check_noncomplex:
        public static int CheckNonComplex(string domain, VipsImage image)
        {
            if (Vips.BandFormatIsComplex(image.BandFmt))
            {
                Vips.Error(domain, "%s", _("image must be non-complex"));
                return -1;
            }

            return 0;
        }

        // vips_check_complex:
        public static int CheckComplex(string domain, VipsImage image)
        {
            if (!Vips.BandFormatIsComplex(image.BandFmt))
            {
                Vips.Error(domain, "%s", _("image must be complex"));
                return -1;
            }

            return 0;
        }

        // vips_check_twocomponents:
        public static int CheckTwocomponents(string domain, VipsImage image)
        {
            if (!Vips.BandFormatIsComplex(image.BandFmt) && image.Bands != 2)
            {
                Vips.Error(domain, "%s", _("image must be two-band or complex"));
                return -1;
            }

            return 0;
        }

        // vips_check_format:
        public static int CheckFormat(string domain, VipsImage image, VipsBandFormat fmt)
        {
            if (image.BandFmt != fmt)
            {
                Vips.Error(domain, _("image must be %s"), Vips.EnumString(VIPS_TYPE_BAND_FORMAT, fmt));
                return -1;
            }

            return 0;
        }

        // vips_check_int:
        public static int CheckInt(string domain, VipsImage image)
        {
            if (!Vips.BandFormatIsInt(image.BandFmt))
            {
                Vips.Error(domain, "%s", _("image must be integer"));
                return -1;
            }

            return 0;
        }

        // vips_check_uint:
        public static int CheckUint(string domain, VipsImage image)
        {
            if (!Vips.BandFormatIsUInt(image.BandFmt))
            {
                Vips.Error(domain, "%s", _("image must be unsigned integer"));
                return -1;
            }

            return 0;
        }

        // vips_check_8or16:
        public static int Check8Or16(string domain, VipsImage image)
        {
            if (image.BandFmt != VIPS_FORMAT_UCHAR && image.BandFmt != VIPS_FORMAT_USHORT &&
                image.BandFmt != VIPS_FORMAT_CHAR && image.BandFmt != VIPS_FORMAT_SHORT)
            {
                Vips.Error(domain, "%s", _("image must be 8- or 16-bit integer, signed or unsigned"));
                return -1;
            }

            return 0;
        }

        // vips_check_u8or16:
        public static int CheckU8Or16(string domain, VipsImage image)
        {
            if (image.BandFmt != VIPS_FORMAT_UCHAR && image.BandFmt != VIPS_FORMAT_USHORT)
            {
                Vips.Error(domain, "%s", _("image must be 8- or 16-bit unsigned integer"));
                return -1;
            }

            return 0;
        }

        // vips_check_u8or16orf:
        public static int CheckU8Or16Orf(string domain, VipsImage image)
        {
            if (image.BandFmt != VIPS_FORMAT_UCHAR && image.BandFmt != VIPS_FORMAT_USHORT &&
                image.BandFmt != VIPS_FORMAT_FLOAT)
            {
                Vips.Error(domain, "%s", _("image must be 8- or 16-bit unsigned integer, or float"));
                return -1;
            }

            return 0;
        }

        // vips_check_uintorf:
        public static int CheckUintOrf(string domain, VipsImage image)
        {
            if (image.BandFmt != VIPS_FORMAT_UCHAR && image.BandFmt != VIPS_FORMAT_USHORT &&
                image.BandFmt != VIPS_FORMAT_UINT && image.BandFmt != VIPS_FORMAT_FLOAT)
            {
                Vips.Error(domain, "%s", _("image must be unsigned int or float"));
                return -1;
            }

            return 0;
        }

        // vips_check_size_same:
        public static int CheckSizeSame(string domain, VipsImage image1, VipsImage image2)
        {
            if (image1.Xsize != image2.Xsize || image1.Ysize != image2.Ysize)
            {
                Vips.Error(domain, "%s", _("images must match in size"));
                return -1;
            }

            return 0;
        }

        // vips_check_oddsquare:
        public static int CheckOddsquare(string domain, VipsImage image)
        {
            if (image.Xsize != image.Ysize || image.Xsize % 2 == 0)
            {
                Vips.Error(domain, "%s", _("images must be odd and square"));
                return -1;
            }

            return 0;
        }

        // vips_check_bands_same:
        public static int CheckBandsSame(string domain, VipsImage image1, VipsImage image2)
        {
            if (image1.Bands != image2.Bands)
            {
                Vips.Error(domain, "%s", _("images must have the same number of bands"));
                return -1;
            }

            return 0;
        }

        // vips_check_bandno:
        public static int CheckBandNo(string domain, VipsImage image, int bandno)
        {
            if (bandno < -1 || bandno > image.Bands - 1)
            {
                Vips.Error(domain, "bandno must be -1, or less than %d", image.Bands);
                return -1;
            }

            return 0;
        }

        // vips_check_format_same:
        public static int CheckFormatSame(string domain, VipsImage image1, VipsImage image2)
        {
            if (image1.BandFmt != image2.BandFmt)
            {
                Vips.Error(domain, "%s", _("images must have the same band format"));
                return -1;
            }

            return 0;
        }

        // vips_check_coding_same:
        public static int CheckCodingSame(string domain, VipsImage image1, VipsImage image2)
        {
            if (image1.Coding != image2.Coding)
            {
                Vips.Error(domain, "%s", _("images must have the same coding"));
                return -1;
            }

            return 0;
        }

        // vips_check_vector_length:
        public static int CheckVectorLength(string domain, int n, int len)
        {
            if (n != len)
            {
                Vips.Error(domain, _("vector must have %d elements"), len);
                return -1;
            }

            return 0;
        }

        // vips_check_vector:
        public static int CheckVector(string domain, int n, VipsImage image)
        {
            if (n == image.Bands || n == 1 || (image.Bands == 1 && n > 1))
                return 0;

            if (image.Bands == 1)
                Vips.Error(domain, "%s", _("vector must have 1 element"));
            else
                Vips.Error(domain, _("vector must have 1 or %d elements"), image.Bands);

            return -1;
        }

        // vips_check_hist:
        public static int CheckHist(string domain, VipsImage image)
        {
            if (image.Xsize != 1 && image.Ysize != 1)
            {
                Vips.Error(domain, "%s", _("histograms must have width or height 1"));
                return -1;
            }
            if (VIPS_IMAGE_N_PELS(image) > 65536)
            {
                Vips.Error(domain, "%s", _("histograms must have not have more than 65536 elements"));
                return -1;
            }

            return 0;
        }

        // vips_check_matrix:
        public static int CheckMatrix(string domain, VipsImage image, out VipsImage matrix)
        {
            matrix = null;

            if (image.Xsize > 100000 || image.Ysize > 100000)
            {
                Vips.Error(domain, "%s", _("matrix image too large"));
                return -1;
            }
            if (image.Bands != 1)
            {
                Vips.Error(domain, "%s", _("matrix image must have one band"));
                return -1;
            }

            var t = Vips.Cast(image, VIPS_FORMAT_DOUBLE, null);
            if (t == null)
                return -1;

            matrix = Vips.ImageCopyMemory(t);
            Vips.Unref(t);

            return 0;
        }

        // vips_check_separable:
        public static int CheckSeparable(string domain, VipsImage image)
        {
            if (image.Xsize != 1 && image.Ysize != 1)
            {
                Vips.Error(domain, "%s", _("separable matrix images must have width or height 1"));
                return -1;
            }

            return 0;
        }

        // vips_check_precision_intfloat:
        public static int CheckPrecisionIntFloat(string domain, VipsPrecision precision)
        {
            if (precision != VIPS_PRECISION_INTEGER && precision != VIPS_PRECISION_FLOAT)
            {
                Vips.Error(domain, "%s", _("precision must be int or float"));
                return -1;
            }

            return 0;
        }
    }
}
```

Note that this is a direct translation of the C code to C#, and some modifications may be necessary to make it work correctly in a .NET environment. Additionally, some types and functions (e.g., `VipsImage`, `VipsBuf`, `g_mutex_lock`) are not defined in the provided C code and would need to be implemented separately.

Also note that this is a very large piece of code, and it's recommended to break it down into smaller, more manageable pieces.