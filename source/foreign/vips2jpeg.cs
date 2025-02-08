Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsJpeg
{
    // vips__jpeg_message_table[]

    public static readonly string[] VipsJpegMessageTable = new string[]
    {
        "premature end of JPEG image",
        "unable to write to target",
        null
    };

    // vips__new_output_message()

    public static void NewOutputMessage(JpegCompress cinfo)
    {
        char buffer = new char[JMSG_LENGTH_MAX];

        cinfo.err.format_message(cinfo, buffer);
        VipsError("VipsJpeg", $"%s", buffer);

#ifdef DEBUG
        Console.WriteLine($"vips__new_output_message: \"{buffer}\"");
#endif /*DEBUG*/
    }

    // vips__new_error_exit()

    public static void NewErrorExit(JpegCompress cinfo)
    {
        ErrorManager eman = (ErrorManager)cinfo.err;

#ifdef DEBUG
        Console.WriteLine("vips__new_error_exit:");
#endif /*DEBUG*/

        // Close the fp if necessary.
        if (eman.fp != null)
        {
            ((System.IO.Stream)eman.fp).Close();
            eman.fp = null;
        }

        // Send the error message to VIPS. This method is overridden above.
        cinfo.err.output_message(cinfo);

        // Jump back.
        longjmp(eman.jmp, 1);
    }

    // Write

    public class Write
    {
        public JpegCompress cinfo;
        public ErrorManager eman;
        public JSAMPROW[] row_pointer;
        public bool invert;

        public Write()
        {
            cinfo = new JpegCompress();
            eman = new ErrorManager();
            cinfo.err = eman.pub;
            cinfo.err.addon_message_table = VipsJpegMessageTable;
            cinfo.err.first_addon_message = 1000;
            cinfo.err.last_addon_message = 1001;
            cinfo.dest = null;
            eman.fp = null;
            invert = false;
        }

        public void Destroy()
        {
            jpeg_destroy_compress(cinfo);
            VipsFree(row_pointer);

            GFree(this);
        }
    }

    // write_new()

    public static Write NewWrite()
    {
        Write write;

        if ((write = new Write()) == null)
            return null;

        write.row_pointer = null;
        write.cinfo.dest = null;
        write.eman.fp = null;
        write.invert = false;

        return write;
    }

    // write_blob()

    public static int WriteBlob(Write write, VipsImage image, string field, int app)
    {
        byte[] data;
        size_t data_length;

        if (!VipsImage.GetTypeOf(image, field))
            return 0;

        if (VipsImage.GetBlob(image, field, out data, out data_length) != 0)
            return -1;

        // Single jpeg markers can only hold 64kb, large objects must
        // be split into multiple markers.

        if (data_length > MAX_BYTES_IN_MARKER)
            GWarning($"field \"{field}\" is too large for a single JPEG marker, ignoring");

        else
        {
#ifdef DEBUG
            Console.WriteLine($"write_blob: attaching {data_length} bytes of {field}");
#endif /*DEBUG*/

            jpeg_write_marker(write.cinfo, app, data, (int)data_length);
        }

        return 0;
    }

    // write_xmp()

    public static int WriteXmp(Write write, VipsImage in_image)
    {
        byte[] data;
        size_t data_length;
        string p;

        if (!VipsImage.GetTypeOf(in_image, VIPS_META_XMP_NAME))
            return 0;

        if (VipsImage.GetBlob(in_image, VIPS_META_XMP_NAME, out data, out data_length) != 0)
            return -1;

        // To write >64kb XMP it you need to parse the whole XMP object,
        // pull out the most important fields, code just them into the main
        // XMP block, then write any remaining XMP objects into a set of
        // extended XMP markers.

        if (data_length > 60000)
            GWarning($"VipsJpeg: large XMP not saved");

        else
        {
            // We need to add the magic XML URL to the start, then a null
            // character, then the data.
            p = new string(data);
            p = $"http://ns.adobe.com/xap/1.0/{p}";

            jpeg_write_marker(write.cinfo, JPEG_APP0 + 1, System.Text.Encoding.UTF8.GetBytes(p), (int)data_length + p.Length);

            GFree(p);
        }

        return 0;
    }

    // write_exif()

    public static int WriteExif(Write write, VipsImage image)
    {
        if (WriteBlob(write, image, VIPS_META_EXIF_NAME, JPEG_APP0 + 1) != 0)
            return -1;

        return 0;
    }

    // ICC writer from lcms, slight tweaks.

    public static void WriteProfileData(JpegCompress cinfo, byte[] icc_data_ptr, uint icc_data_len)
    {
        uint num_markers; /* total number of markers we'll write */
        int cur_marker = 1; /* per spec, counting starts at 1 */
        uint length; /* number of bytes to write in this marker */

        g_assert(icc_data_len > 0);

        // Calculate the number of markers we'll need, rounding up of course
        num_markers = (icc_data_len + MAX_DATA_BYTES_IN_MARKER - 1) / MAX_DATA_BYTES_IN_MARKER;

        while (icc_data_len > 0)
        {
            // length of profile to put in this marker
            length = icc_data_len;
            if (length > MAX_DATA_BYTES_IN_MARKER)
                length = MAX_DATA_BYTES_IN_MARKER;
            icc_data_len -= length;

            // Write the JPEG marker header (APP2 code and marker length)
            jpeg_write_m_header(cinfo, ICC_MARKER, (uint)(length + ICC_OVERHEAD_LEN));

            // Write the marker identifying string "ICC_PROFILE" (null-terminated).
            // We code it in this less-than-transparent way so that the code works
            // even if the local character set is not ASCII.
            jpeg_write_m_byte(cinfo, 0x49);
            jpeg_write_m_byte(cinfo, 0x43);
            jpeg_write_m_byte(cinfo, 0x43);
            jpeg_write_m_byte(cinfo, 0x5F);
            jpeg_write_m_byte(cinfo, 0x50);
            jpeg_write_m_byte(cinfo, 0x52);
            jpeg_write_m_byte(cinfo, 0x4F);
            jpeg_write_m_byte(cinfo, 0x46);
            jpeg_write_m_byte(cinfo, 0x49);
            jpeg_write_m_byte(cinfo, 0x4C);
            jpeg_write_m_byte(cinfo, 0x45);
            jpeg_write_m_byte(cinfo, 0x0);

            // Add the sequencing info
            jpeg_write_m_byte(cinfo, cur_marker);
            jpeg_write_m_byte(cinfo, (int)num_markers);

            // Add the profile data
            while (length-- > 0)
            {
                jpeg_write_m_byte(cinfo, icc_data_ptr[0]);
                icc_data_ptr++;
            }
            cur_marker++;
        }
    }

    // write_profile_file()

    public static int WriteProfileFile(Write write, string profile)
    {
        VipsBlob blob;

        if (VipsProfileLoad(profile, out blob) != 0)
            return -1;

        if (blob != null)
        {
            byte[] data;
            size_t length;

            if (VipsBlob.Get(blob, out data, out length) != 0)
                return -1;

            WriteProfileData(write.cinfo, data, (uint)length);

#ifdef DEBUG
            Console.WriteLine($"write_profile_file: attached profile \"{profile}\"");
#endif /*DEBUG*/

            VipsArea.Unref((VipsArea)blob);
        }

        return 0;
    }

    // write_profile_meta()

    public static int WriteProfileMeta(Write write, VipsImage in_image)
    {
        byte[] data;
        size_t length;

        if (VipsImage.GetBlob(in_image, VIPS_META_ICC_NAME, out data, out length) != 0)
            return -1;

        WriteProfileData(write.cinfo, data, (uint)length);

#ifdef DEBUG
        Console.WriteLine($"write_profile_meta: attached {length} byte profile from header");
#endif /*DEBUG*/

        return 0;
    }

    // write_jpeg_block()

    public static int WriteJpegBlock(VipsRegion region, VipsRect rect, object a)
    {
        Write write = (Write)a;

        for (int y = 0; y < rect.height; y++)
            write.row_pointer[y] = (JSAMPROW)VipsRegion.Addr(region, rect.left, rect.top + y);

        // Catch any longjmp()s from jpeg_write_scanlines() here.
        if (setjmp(write.eman.jmp) != 0)
            return -1;

        if (write.invert)
        {
            int n_elements = region.im.Bands * rect.width;

            for (int y = 0; y < rect.height; y++)
            {
                byte[] line = write.row_pointer[y];

                for (int x = 0; x < n_elements; x++)
                    line[x] = (byte)(255 - line[x]);
            }
        }

        jpeg_write_scanlines(write.cinfo, write.row_pointer, rect.height);

        return 0;
    }

    // set_cinfo()

    public static void SetCInfo(JpegCompress cinfo, VipsImage in_image, int width, int height, int qfac,
        bool optimize_coding, bool progressive, bool trellis_quant, bool overshoot_deringing,
        bool optimize_scans, int quant_table, VipsForeignSubsample subsample_mode, int restart_interval)
    {
        J_COLOR_SPACE space;

        // Set compression parameters.
        cinfo.image_width = width;
        cinfo.image_height = height;
        cinfo.input_components = in_image.Bands;

        if (in_image.Bands == 4 && in_image.Type == VIPS_INTERPRETATION_CMYK)
            space = JCS_CMYK;
        else if (in_image.Bands == 3)
            space = JCS_RGB;
        else if (in_image.Bands == 1)
            space = JCS_GRAYSCALE;
        else
            // Use luminance compression for all channels.
            space = JCS_UNKNOWN;

        cinfo.in_color_space = space;

#ifdef HAVE_JPEG_EXT_PARAMS
        // Reset compression profile to libjpeg defaults
        if (jpeg_c_int_param_supported(cinfo, JINT_COMPRESS_PROFILE))
            jpeg_c_set_int_param(cinfo, JINT_COMPRESS_PROFILE, JCP_FASTEST);
#endif

        // Reset to default.
        jpeg_set_defaults(cinfo);

        // Compute optimal Huffman coding tables.
        cinfo.optimize_coding = optimize_coding;

        // Use a restart interval.
        if (restart_interval > 0)
            cinfo.restart_interval = restart_interval;

#ifdef HAVE_JPEG_EXT_PARAMS
        // Apply trellis quantisation to each 8x8 block. Implies "optimize_coding".
        if (trellis_quant)
        {
            if (jpeg_c_bool_param_supported(cinfo, JBOOLEAN_TRELLIS_QUANT))
            {
                jpeg_c_set_bool_param(cinfo, JBOOLEAN_TRELLIS_QUANT, true);
                cinfo.optimize_coding = true;
            }
            else
                GWarning("%s", "trellis_quant unsupported");
        }

        // Apply overshooting to samples with extreme values e.g. 0 & 255 for 8-bit.
        if (overshoot_deringing)
        {
            if (jpeg_c_bool_param_supported(cinfo, JBOOLEAN_OVERSHOOT_DERINGING))
                jpeg_c_set_bool_param(cinfo, JBOOLEAN_OVERSHOOT_DERINGING, true);
            else
                GWarning("%s", "overshoot_deringing unsupported");
        }

        // Split the spectrum of DCT coefficients into separate scans.
        // Requires progressive output. Must be set before jpeg_simple_progression.
        if (optimize_scans)
        {
            if (progressive)
            {
                if (jpeg_c_bool_param_supported(cinfo, JBOOLEAN_OPTIMIZE_SCANS))
                    jpeg_c_set_bool_param(cinfo, JBOOLEAN_OPTIMIZE_SCANS, true);
                else
                    GWarning("%s", "ignoring optimize_scans");
            }
            else
                GWarning("%s", "ignoring optimize_scans for baseline");
        }

        // Use predefined quantization table.
        if (quant_table > 0)
        {
            if (jpeg_c_int_param_supported(cinfo, JINT_BASE_QUANT_TBL_IDX))
                jpeg_c_set_int_param(cinfo, JINT_BASE_QUANT_TBL_IDX, quant_table);
            else
                GWarning("%s", "setting quant_table unsupported");
        }
#else
        // Using jpeglib.h without extension parameters, warn of ignored options.
        if (trellis_quant)
            GWarning("%s", "ignoring trellis_quant");
        if (overshoot_deringing)
            GWarning("%s", "ignoring overshoot_deringing");
        if (optimize_scans)
            GWarning("%s", "ignoring optimize_scans");
        if (quant_table > 0)
            GWarning("%s", "ignoring quant_table");
#endif

        // Set compression quality. Must be called after setting params above.
        jpeg_set_quality(cinfo, qfac, true);

        // Enable progressive write.
        if (progressive)
            jpeg_simple_progression(cinfo);

        // We must set chroma subsampling explicitly since some libjpegs do not
        // enable this by default.
        if (in_image.Bands == 3 && (subsample_mode == VIPS_FOREIGN_SUBSAMPLE_ON ||
            (subsample_mode == VIPS_FOREIGN_SUBSAMPLE_AUTO && qfac < 90)))
            cinfo.comp_info[0].h_samp_factor = cinfo.comp_info[0].v_samp_factor = 2;
        else
            cinfo.comp_info[0].h_samp_factor = cinfo.comp_info[0].v_samp_factor = 1;

        // Rest should have sampling factors 1,1.
        for (int i = 1; i < in_image.Bands; i++)
            cinfo.comp_info[i].h_samp_factor = cinfo.comp_info[i].v_samp_factor = 1;

        // Only write the JFIF headers if we have no EXIF.
        // Some readers get confused if you set both.
        cinfo.write_JFIF_header = false;
#ifndef HAVE_EXIF
        VipsJfifResolutionFromImage(cinfo, in_image);
        cinfo.write_JFIF_header = true;
#endif /*HAVE_EXIF*/
    }

    // write_metadata()

    public static int WriteMetadata(Write write, VipsImage in_image, string profile)
    {
        if (WriteExif(write, in_image) != 0 || WriteXmp(write, in_image) != 0 ||
            WriteBlob(write, in_image, VIPS_META_IPTC_NAME, JPEG_APP0 + 13) != 0)
            return -1;

        // A profile supplied as an argument overrides an embedded profile.
        if (profile != null)
        {
            if (WriteProfileFile(write, profile) != 0)
                return -1;
        }
        else if (VipsImage.GetTypeOf(in_image, VIPS_META_ICC_NAME))
        {
            if (WriteProfileMeta(write, in_image) != 0)
                return -1;
        }

        return 0;
    }

    // write_vips()

    public static int WriteVips(Write write, VipsImage in_image, int Q, string profile,
        bool optimize_coding, bool progressive, bool trellis_quant, bool overshoot_deringing,
        bool optimize_scans, int quant_table, VipsForeignSubsample subsample_mode, int restart_interval)
    {
        // Should have been converted for save.
        g_assert(in_image.BandFmt == VIPS_FORMAT_UCHAR);
        g_assert(in_image.Coding == VIPS_CODING_NONE);
        g_assert(in_image.Bands == 1 || in_image.Bands == 3 || in_image.Bands == 4);

        // Check input image.
        if (VipsImage.PioInput(in_image) != 0)
            return -1;

        SetCInfo(write.cinfo, in_image, in_image.Xsize, in_image.Ysize,
            Q, optimize_coding, progressive, trellis_quant, overshoot_deringing,
            optimize_scans, quant_table, subsample_mode, restart_interval);

        if (in_image.Bands == 4 && in_image.Type == VIPS_INTERPRETATION_CMYK)
            // IJG always sets an Adobe marker, so we should invert CMYK.
            write.invert = true;

        // Build VIPS output stuff now we know the image we'll be writing.
        if ((write.row_pointer = new JSAMPROW[in_image.Ysize]) == null)
            return -1;

        // Write app0 and build compress tables.
        jpeg_start_compress(write.cinfo, true);

        // All the other APP chunks come next.
        if (WriteMetadata(write, in_image, profile) != 0)
            return -1;

        // Write data. Note that the write function grabs the longjmp()!
        if (VipsSinkDisc(in_image, WriteJpegBlock, write) != 0)
            return -1;

        // We have to reinstate the setjmp() before we jpeg_finish_compress().
        if (setjmp(write.eman.jmp) != 0)
            return -1;

        // This should only be called on a successful write.
        jpeg_finish_compress(write.cinfo);

        return 0;
    }

    // vips__jpeg_target_dest()

    public static void JpegTargetDest(JpegCompress cinfo, VipsTarget target)
    {
        Dest dest = new Dest();

        if (cinfo.dest == null) /* first time for this JPEG object? */
            cinfo.dest =
                (struct jpeg_destination_mgr*)((*cinfo.mem).alloc_small(
                    (j_common_ptr)cinfo,
                    JPOOL_PERMANENT,
                    sizeof(Dest)));

        dest.pub.init_destination = init_destination;
        dest.pub.empty_output_buffer = empty_output_buffer;
        dest.pub.term_destination = term_destination;
        dest.target = target;

        cinfo.dest = dest;
    }

    // vips__jpeg_write_target()

    public static int JpegWriteTarget(VipsImage in_image, VipsTarget target,
        int Q, string profile, bool optimize_coding, bool progressive,
        bool trellis_quant, bool overshoot_deringing, bool optimize_scans,
        int quant_table, VipsForeignSubsample subsample_mode, int restart_interval)
    {
        Write write;

        if ((write = NewWrite()) == null)
            return -1;

        // Make jpeg compression object.
        if (setjmp(write.eman.jmp) != 0)
        {
            // Here for longjmp() during write_vips().
            write.Destroy();

            return -1;
        }
        jpeg_create_compress(write.cinfo);

        // Attach output.
        JpegTargetDest(write.cinfo, target);

        // Convert! Write errors come back here as an error return.
        if (WriteVips(write, in_image,
            Q, profile, optimize_coding, progressive,
            trellis_quant, overshoot_deringing, optimize_scans,
            quant_table, subsample_mode, restart_interval) != 0)
        {
            write.Destroy();
            return -1;
        }
        write.Destroy();

        if (VipsTarget.End(target) != 0)
            return -1;

        return 0;
    }

    // vips__jpeg_region_write_target()

    public static int JpegRegionWriteTarget(VipsRegion region, VipsRect rect,
        VipsTarget target, int Q, string profile, bool optimize_coding, bool progressive,
        VipsForeignKeep keep, bool trellis_quant, bool overshoot_deringing,
        bool optimize_scans, int quant_table, VipsForeignSubsample subsample_mode,
        int restart_interval)
    {
        Write write;

        if ((write = NewWrite()) == null)
            return -1;

        // Make jpeg compression object.
        if (setjmp(write.eman.jmp) != 0)
        {
            // Here for longjmp() during write_vips().
            write.Destroy();

            return -1;
        }
        jpeg_create_compress(write.cinfo);

        // Attach output.
        JpegTargetDest(write.cinfo, target);

        // Convert! Write errors come back here as an error return.
        if (WriteVipsRegion(write, region, rect,
            Q, profile, optimize_coding, progressive, keep,
            trellis_quant, overshoot_deringing, optimize_scans,
            quant_table, subsample_mode, restart_interval) != 0)
        {
            write.Destroy();
            return -1;
        }
        write.Destroy();

        if (VipsTarget.End(target) != 0)
            return -1;

        return 0;
    }

    // vips__jpeg_suffs[]

    public static readonly string[] VipsJpegSuffs = new string[]
    {
        ".jpg",
        ".jpeg",
        ".jpe",
        ".jfif",
        null
    };

    // write_vips_region()

    public static int WriteVipsRegion(Write write, VipsRegion region, VipsRect rect,
        int Q, string profile, bool optimize_coding, bool progressive,
        VipsForeignKeep keep, bool trellis_quant, bool overshoot_deringing,
        bool optimize_scans, int quant_table, VipsForeignSubsample subsample_mode,
        int restart_interval)
    {
        // the image we'll be writing
        VipsImage in_image = region.im;
        VipsImage x;

        SetCInfo(write.cinfo, in_image, rect.width, rect.height,
            Q, optimize_coding, progressive, trellis_quant, overshoot_deringing,
            optimize_scans, quant_table, subsample_mode, restart_interval);

        // Should have been converted for save.
        g_assert(in_image.BandFmt == VIPS_FORMAT_UCHAR);
        g_assert(in_image.Coding == VIPS_CODING_NONE);
        g_assert(in_image.Bands == 1 || in_image.Bands == 3 || in_image.Bands == 4);

        if (in_image.Bands == 4 && in_image.Type == VIPS_INTERPRETATION_CMYK)
            // FIXME ... need to invert on the fly as we send pixels to libjpeg
            write.invert = true;

        // Build VIPS output stuff now we know the image we'll be writing.
        if ((write.row_pointer = new JSAMPROW[rect.height]) == null)
            return -1;

        // Write app0 and build compress tables.
        jpeg_start_compress(write.cinfo, true);

        // Updating metadata, need to copy the image.
        if (VipsCopy(in_image, out x, null) != 0)
            return -1;

        // All the other APP chunks come next.
        if (VipsForeignUpdateMetadata(x, keep) != 0 || WriteMetadata(write, x, profile) != 0)
        {
            GObject.Unref(x);
            return -1;
        }

        GObject.Unref(x);

        // Write data. Note that the write function grabs the longjmp()!
        if (WriteJpegBlock(region, rect, write) != 0)
            return -1;

        // We have to reinstate the setjmp() before we jpeg_finish_compress().
        if (setjmp(write.eman.jmp) != 0)
            return -1;

        // This should only be called on a successful write.
        jpeg_finish_compress(write.cinfo);

        return 0;
    }
}
```