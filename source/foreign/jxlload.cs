Here is the converted code:

```csharp
using System;
using System.IO;

public class VipsForeignLoadJxl : VipsForeignLoad
{
    public VipsForeignLoadJxl()
    {
        n = 1;
        delay = new GArray(0, sizeof(int));
    }

    public override int Build(VipsObject obj)
    {
        var jxl = (VipsForeignLoadJxl)obj;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_jxl_build:");
#endif /*DEBUG*/

        jxl.runner = JxlThreadParallelRunnerCreate(null);
        jxl.decoder = JxlDecoderCreate(null);

        if (JxlDecoderSetParallelRunner(jxl.decoder, JxlThreadParallelRunner, jxl.runner))
        {
            vips_foreign_load_jxl_error(jxl, "JxlDecoderSetParallelRunner");
            return -1;
        }

        if (base.Build(obj) == -1)
            return -1;

        return 0;
    }

    public override bool IsA(VipsSource source)
    {
        var p = vips_source_sniff(source, 12);
        JxlSignature sig = JxlSignatureCheck(p, 12);

        if (sig != JXL_SIG_INVALID && sig != JXL_SIG_NOT_ENOUGH_BYTES)
            return true;

        return false;
    }

    public override VipsForeignFlags GetFlags(VipsForeignLoad load)
    {
        // FIXME .. could support random access for non-animated images.
        return VIPS_FOREIGN_SEQUENTIAL;
    }

    private void vips_foreign_load_jxl_error(VipsForeignLoadJxl jxl, string details)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(jxl);

        // TODO ... jxl has no way to get error messages at the moment.
        vips_error(class.Nickname, "error %s", details);
    }

    private int vips_foreign_load_jxl_build(VipsForeignLoadJxl jxl)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(jxl);

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_jxl_build:");
#endif /*DEBUG*/

        jxl.runner = JxlThreadParallelRunnerCreate(null);
        jxl.decoder = JxlDecoderCreate(null);

        if (JxlDecoderSetParallelRunner(jxl.decoder, JxlThreadParallelRunner, jxl.runner))
        {
            vips_foreign_load_jxl_error(jxl, "JxlDecoderSetParallelRunner");
            return -1;
        }

        if (base.Build((VipsObject)jxl) == -1)
            return -1;

        return 0;
    }

    private int vips_foreign_load_jxl_set_box_buffer(VipsForeignLoadJxl jxl)
    {
        if (!jxl.box_data || !jxl.box_size)
            return 0;

        var class = (VipsObjectClass)VipsObject.GetClass(jxl);

        uint8[] new_data;
        size_t new_size;
        size_t box_size = *jxl.box_size;

        new_size = box_size + INPUT_BUFFER_SIZE;
        new_data = jxl.box_data.Length > 0 ? (uint8[])jxl.box_data.Clone() : new uint8[new_size];

        if (!new_data)
        {
            vips_error(class.Nickname, "%s", "out of memory");
            return -1;
        }

        jxl.box_data = new_data;

        JxlDecoder.SetBoxBuffer(jxl.decoder, new_data + box_size, INPUT_BUFFER_SIZE);

        return 0;
    }

    private int vips_foreign_load_jxl_release_box_buffer(VipsForeignLoadJxl jxl)
    {
        if (!jxl.box_data || !jxl.box_size)
            return 0;

        size_t remaining = JxlDecoder.ReleaseBoxBuffer(jxl.decoder);
        *jxl.box_size += INPUT_BUFFER_SIZE - remaining;

        return 0;
    }

    private int vips_foreign_load_jxl_fill_input(VipsForeignLoadJxl jxl, size_t bytes_remaining)
    {
        gint64 bytes_read;

#ifdef DEBUG_VERBOSE
        Console.WriteLine("vips_foreign_load_jxl_fill_input: " + (INPUT_BUFFER_SIZE - bytes_remaining) + " bytes requested");
#endif /*DEBUG_VERBOSE*/

        Array.Copy(jxl.input_buffer, jxl.bytes_in_buffer - bytes_remaining, INPUT_BUFFER_SIZE - bytes_remaining);
        bytes_read = vips_source_read(jxl.source, jxl.input_buffer + bytes_remaining, INPUT_BUFFER_SIZE - bytes_remaining);

        // Read error.
        if (bytes_read < 0)
            return -1;

        jxl.bytes_in_buffer = bytes_read + bytes_remaining;

#ifdef DEBUG_VERBOSE
        Console.WriteLine("vips_foreign_load_jxl_fill_input: " + bytes_read + " bytes read");
#endif /*DEBUG_VERBOSE*/

        return (int)bytes_read;
    }

    private int vips_foreign_load_jxl_process(VipsForeignLoadJxl jxl)
    {
        JxlDecoderStatus status;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_jxl_process: starting ...");
#endif /*DEBUG*/

        while ((status = JxlDecoder.ProcessInput(jxl.decoder)) == JXL_DEC_NEED_MORE_INPUT)
        {
            size_t bytes_remaining;
            int bytes_read;

#ifdef DEBUG_VERBOSE
            Console.WriteLine("vips_foreign_load_jxl_process: reading ...");
#endif /*DEBUG_VERBOSE*/

            bytes_remaining = JxlDecoder.ReleaseInput(jxl.decoder);
            bytes_read = vips_foreign_load_jxl_fill_input(jxl, bytes_remaining);

            if (bytes_read < 0)
                return -1;

            if (jxl.bytes_in_buffer > 0)
                JxlDecoder.SetInput(jxl.decoder, jxl.input_buffer, jxl.bytes_in_buffer);

            if (!bytes_read)
                JxlDecoder.CloseInput(jxl.decoder);
        }

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_jxl_process: seen ");
#endif /*DEBUG*/

        return (int)status;
    }

    private int vips_foreign_load_jxl_read_frame(VipsForeignLoadJxl jxl, VipsImage frame, int frame_no)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(jxl);

        size_t buffer_size;
        JxlDecoderStatus status;

        if (jxl.frame_no >= frame_no)
            return 0;

        int skip = frame_no - jxl.frame_no - 1;
        if (skip > 0)
        {
#ifdef DEBUG_VERBOSE
            Console.WriteLine("vips_foreign_load_jxl_read_frame: skipping " + skip + " frames");
#endif /*DEBUG_VERBOSE*/

            JxlDecoder.SkipFrames(jxl.decoder, skip);
            jxl.frame_no += skip;
        }

        // Read to the end of the image.
        do
        {
            switch ((status = vips_foreign_load_jxl_process(jxl)))
            {
                case JXL_DEC_ERROR:
                    vips_foreign_load_jxl_error(jxl, "JxlDecoderProcessInput");
                    return -1;

                case JXL_DEC_FRAME:
                    jxl.frame_no++;
                    break;

                case JXL_DEC_NEED_IMAGE_OUT_BUFFER:
                    if (JxlDecoder.ImageOutBufferSize(jxl.decoder, out buffer_size))
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderImageOutBufferSize");
                        return -1;
                    }
                    if (buffer_size != VIPS_IMAGE_SIZEOF_IMAGE(frame))
                    {
                        vips_error(class.Nickname, "%s", "bad buffer size");
                        return -1;
                    }
                    if (JxlDecoder.SetImageOutBuffer(jxl.decoder, buffer_size, VIPS_IMAGE_ADDR(frame, 0, 0), VIPS_IMAGE_SIZEOF_IMAGE(frame)))
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderSetImageOutBuffer");
                        return -1;
                    }
                    break;

                case JXL_DEC_FULL_IMAGE:
                    // We decoded the required frame and can return
                    if (jxl.frame_no >= frame_no)
                        return 0;

                    break;

                default:
                    break;
            }
        } while (status != JXL_DEC_SUCCESS);

        // We didn't find the required frame
        vips_error(class.Nickname, "%s", "not enough frames");
        return -1;
    }

    private int vips_foreign_load_jxl_generate(VipsRegion out_region, void* seq, void* a, void* b, bool* stop)
    {
        var jxl = (VipsForeignLoadJxl)a;

        // jxl>frame_no numbers from 1.
        int frame = 1 + out_region.Valid.Top / jxl.info.xsize + jxl.page;
        int line = out_region.Valid.Top % jxl.info.ysize;

#ifdef DEBUG_VERBOSE
        Console.WriteLine("vips_foreign_load_jxl_generate: line " + out_region.Valid.Top);
#endif /*DEBUG_VERBOSE*/

        GAssert(out_region.Valid.Height == 1);

        if (vips_foreign_load_jxl_read_frame(jxl, jxl.frame, frame))
            return -1;

        Array.Copy(VIPS_IMAGE_ADDR(jxl.frame, 0, line), VIPS_REGION_ADDR(out_region, 0, out_region.Valid.Top), VIPS_IMAGE_SIZEOF_LINE(jxl.frame));

        return 0;
    }

    private int vips_foreign_load_jxl_fix_exif(VipsForeignLoadJxl jxl)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(jxl);

        if (!jxl.exif_data || vips_isprefix("Exif", (char)jxl.exif_data))
            return 0;

        if (jxl.exif_size < 4)
        {
            g_warning("%s: invalid data in EXIF box", class.Nickname);
            return -1;
        }

        // Offset is stored in big-endian
        size_t offset = GUINT32_FROM_BE(*((guint32)jxl.exif_data));
        if (offset > jxl.exif_size - 4)
        {
            g_warning("%s: invalid data in EXIF box", class.Nickname);
            return -1;
        }

        size_t new_size = jxl.exif_size - 4 - offset + 6;
        uint8[] new_data;
        if (!(new_data = VIPS_MALLOC(NULL, new_size)))
            return -1;

        Array.Copy("Exif\0\0", 0, new_data, 0, 6);
        Array.Copy(jxl.exif_data + 4 + offset, 0, new_data, 6, new_size - 6);

        VIPS_FREE(jxl.exif_data);
        jxl.exif_size = new_size;
        jxl.exif_data = new_data;

        return 0;
    }

    private int vips_foreign_load_jxl_set_header(VipsForeignLoadJxl jxl, VipsImage out)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(jxl);

        VipsBandFormat format;
        VipsInterpretation interpretation;

        if (jxl.info.xsize >= VIPS_MAX_COORD || jxl.info.ysize >= VIPS_MAX_COORD)
        {
            vips_error(class.Nickname, "%s", "image size out of bounds");
            return -1;
        }

        switch (jxl.format.data_type)
        {
            case JXL_TYPE_UINT8:
                format = VIPS_FORMAT_UCHAR;
                break;

            case JXL_TYPE_UINT16:
                format = VIPS_FORMAT_USHORT;
                break;

            case JXL_TYPE_FLOAT:
                format = VIPS_FORMAT_FLOAT;
                break;

            default:
                GAssertNotReached();
        }

        switch (jxl.info.num_color_channels)
        {
            case 1:
                switch (format)
                {
                    case VIPS_FORMAT_UCHAR:
                        interpretation = VIPS_INTERPRETATION_B_W;
                        break;

                    case VIPS_FORMAT_USHORT:
                        interpretation = VIPS_INTERPRETATION_GREY16;
                        break;

                    default:
                        interpretation = VIPS_INTERPRETATION_B_W;
                }
                break;

            case 3:
                switch (format)
                {
                    case VIPS_FORMAT_UCHAR:
                        interpretation = VIPS_INTERPRETATION_sRGB;
                        break;

                    case VIPS_FORMAT_USHORT:
                        interpretation = VIPS_INTERPRETATION_RGB16;
                        break;

                    case VIPS_FORMAT_FLOAT:
                        interpretation = VIPS_INTERPRETATION_scRGB;
                        break;

                    default:
                        interpretation = VIPS_INTERPRETATION_sRGB;
                }
                break;

            default:
                interpretation = VIPS_INTERPRETATION_MULTIBAND;
                break;
        }

        if (jxl.frame_count > 1)
        {
            if (jxl.n == -1)
                jxl.n = jxl.frame_count - jxl.page;

            if (jxl.page < 0 || jxl.n <= 0 || jxl.page + jxl.n > jxl.frame_count)
            {
                vips_error(class.Nickname, "%s", "bad page number");
                return -1;
            }

            vips_image_set_int(out, VIPS_META_N_PAGES, jxl.frame_count);

            if (jxl.n > 1)
                vips_image_set_int(out, VIPS_META_PAGE_HEIGHT, jxl.info.ysize);

            if (jxl.is_animated)
            {
                int[] delay = (int[])jxl.delay.Data;

                vips_image_set_array_int(out, "delay", delay, jxl.frame_count);

                // gif uses centiseconds for delays
                vips_image_set_int(out, "gif-delay", VIPS_RINT(delay[0] / 10.0));

                vips_image_set_int(out, "loop", jxl.info.animation.num_loops);
            }
        }
        else
        {
            jxl.n = 1;
            jxl.page = 0;
        }

        // Init jxl->frame only when we need to decode multiple frames.
        // Otherwise, we can decode the frame right to the output
        if (jxl.n > 1 && !jxl.frame)
        {
            jxl.frame = vips_image_new_memory();
            vips_image_init_fields(jxl.frame, jxl.info.xsize, jxl.info.ysize, jxl.format.num_channels, format, VIPS_CODING_NONE, interpretation, 1.0, 1.0);
            if (vips_image_pipelinev(jxl.frame, VIPS_DEMAND_STYLE_THINSTRIP, null) || vips_image_write_prepare(jxl.frame))
                return -1;
        }

        vips_image_init_fields(out, jxl.info.xsize, jxl.info.ysize * jxl.n, jxl.format.num_channels, format, VIPS_CODING_NONE, interpretation, 1.0, 1.0);

        // Even though this is a full image reader, we hint thinstrip since
        // we are quite happy serving that if anything downstream
        // would like it.
        if (vips_image_pipelinev(out, VIPS_DEMAND_STYLE_THINSTRIP, null))
            return -1;

        if (jxl.icc_data && jxl.icc_size > 0)
        {
            vips_image_set_blob(out, VIPS_META_ICC_NAME, (VipsCallbackFn)vips_area_free_cb, jxl.icc_data, jxl.icc_size);
            jxl.icc_data = null;
            jxl.icc_size = 0;
        }

        if (jxl.exif_data && jxl.exif_size > 0)
        {
            vips_image_set_blob(out, VIPS_META_EXIF_NAME, (VipsCallbackFn)vips_area_free_cb, jxl.exif_data, jxl.exif_size);
            jxl.exif_data = null;
            jxl.exif_size = 0;
        }

        if (jxl.xmp_data && jxl.xmp_size > 0)
        {
            vips_image_set_blob(out, VIPS_META_XMP_NAME, (VipsCallbackFn)vips_area_free_cb, jxl.xmp_data, jxl.xmp_size);
            jxl.xmp_data = null;
            jxl.xmp_size = 0;
        }

        vips_image_set_int(out, VIPS_META_ORIENTATION, jxl.info.orientation);

        vips_image_set_int(out, VIPS_META_BITS_PER_SAMPLE, jxl.info.bits_per_sample);

        return 0;
    }

    private int vips_foreign_load_jxl_header(VipsForeignLoad load)
    {
        var jxl = (VipsForeignLoadJxl)load;

        JxlDecoderStatus status;
        JXL_BOOL decompress_boxes = JXL_TRUE;
        JxlFrameHeader h;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_jxl_header:");
#endif /*DEBUG*/

        if (vips_source_rewind(jxl.source))
            return -1;

        JxlDecoder.Rewind(jxl.decoder);
        if (JxlDecoder.SubscribeEvents(jxl.decoder, JXL_DEC_COLOR_ENCODING | JXL_DEC_BASIC_INFO | JXL_DEC_BOX | JXL_DEC_FRAME))
        {
            vips_foreign_load_jxl_error(jxl, "JxlDecoderSubscribeEvents");
            return -1;
        }

        if (JxlDecoder.SetDecompressBoxes(jxl.decoder, JXL_TRUE) != JXL_DEC_SUCCESS)
            decompress_boxes = JXL_FALSE;

        if (vips_foreign_load_jxl_fill_input(jxl, 0) < 0)
            return -1;
        JxlDecoder.SetInput(jxl.decoder, jxl.input_buffer, jxl.bytes_in_buffer);

        jxl.frame_count = 0;

        // Read to the end of the header.
        do
        {
            switch ((status = vips_foreign_load_jxl_process(jxl)))
            {
                case JXL_DEC_ERROR:
                    vips_foreign_load_jxl_error(jxl, "JxlDecoderProcessInput");
                    return -1;

                case JXL_DEC_BOX:
                    // Flush previous box data if any
                    if (vips_foreign_load_jxl_release_box_buffer(jxl))
                        return -1;

                    JxlBoxType type;
                    if (JxlDecoder.GetBoxType(jxl.decoder, out type, decompress_boxes) != JXL_DEC_SUCCESS)
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderGetBoxType");
                        return -1;
                    }

#ifdef DEBUG
                    var type_s = new char[] { type[0], type[1], type[2], type[3], 0 };
                    Console.WriteLine("vips_foreign_load_jxl_header found box " + type_s);
#endif /*DEBUG*/

                    if (!memcmp(type, "Exif", 4))
                    {
                        jxl.box_size = ref jxl.exif_size;
                        jxl.box_data = ref jxl.exif_data;
                    }
                    else if (!memcmp(type, "xml ", 4))
                    {
                        jxl.box_size = ref jxl.xmp_size;
                        jxl.box_data = ref jxl.xmp_data;
                    }
                    else
                    {
                        jxl.box_size = null;
                        jxl.box_data = null;
                    }

                    if (vips_foreign_load_jxl_set_box_buffer(jxl))
                        return -1;

                    break;

                case JXL_DEC_BOX_NEED_MORE_OUTPUT:
                    if (vips_foreign_load_jxl_release_box_buffer(jxl) || vips_foreign_load_jxl_set_box_buffer(jxl))
                        return -1;

                    break;

                case JXL_DEC_BASIC_INFO:
                    if (JxlDecoder.GetBasicInfo(jxl.decoder, out jxl.info))
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderGetBasicInfo");
                        return -1;
                    }
#ifdef DEBUG
                    vips_foreign_load_jxl_print_info(ref jxl.info);
#endif /*DEBUG*/

                    // Pick a pixel format to decode to.
                    jxl.format.num_channels = jxl.info.num_color_channels + jxl.info.num_extra_channels;
                    if (jxl.info.exponent_bits_per_sample > 0 || jxl.info.alpha_exponent_bits > 0)
                        jxl.format.data_type = JXL_TYPE_FLOAT;
                    else if (jxl.info.bits_per_sample > 8)
                        jxl.format.data_type = JXL_TYPE_UINT16;
                    else
                        jxl.format.data_type = JXL_TYPE_UINT8;
                    jxl.format.endianness = JXL_NATIVE_ENDIAN;
                    jxl.format.align = 0;

#ifdef DEBUG
                    vips_foreign_load_jxl_print_format(ref jxl.format);
#endif /*DEBUG*/

                    break;

                case JXL_DEC_COLOR_ENCODING:
                    if (JxlDecoder.GetICCProfileSize(jxl.decoder, out buffer_size))
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderGetICCProfileSize");
                        return -1;
                    }

#ifdef DEBUG
                    Console.WriteLine("vips_foreign_load_jxl_header: " + buffer_size + " byte profile");
#endif /*DEBUG*/
                    if (!(jxl.icc_data = VIPS_MALLOC(NULL, buffer_size)))
                        return -1;

                    if (JxlDecoder.GetColorAsICCProfile(jxl.decoder, out buffer_size, jxl.icc_data))
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderGetColorAsICCProfile");
                        return -1;
                    }
                    break;

                case JXL_DEC_FRAME:
                    if (JxlDecoder.GetFrameHeader(jxl.decoder, ref h) != JXL_DEC_SUCCESS)
                    {
                        vips_foreign_load_jxl_error(jxl, "JxlDecoderGetFrameHeader");
                        return -1;
                    }

#ifdef DEBUG
                    vips_foreign_load_jxl_print_frame_header(ref h);
#endif /*DEBUG*/

                    if (jxl.info.have_animation)
                    {
                        // tick duration in seconds
                        double tick = jxl.info.animation.tps_denominator / jxl.info.animation.tps_numerator;
                        // this duration in ms
                        int ms = VIPS_RINT(1000.0 * h.duration * tick);
                        // h.duration of 0xffffffff is used for multipage JXL ... map
                        // this to -1 in delay
                        int duration = h.duration == 0xffffffff ? -1 : ms;

                        jxl.delay.AppendVal(duration);

                    }

                    jxl.frame_count++;

                    break;

                default:
                    break;
            }
        } while (status != JXL_DEC_SUCCESS);

        // Detect JXL multipage (rather than animated).
        int[] delay = (int[])jxl.delay.Data;
        for (int i = 0; i < jxl.delay.Length; i++)
            if (delay[i] != -1)
            {
                jxl.is_animated = true;
                break;
            }

        // Flush box data if any
        if (vips_foreign_load_jxl_release_box_buffer(jxl))
            return -1;

        if (vips_foreign_load_jxl_fix_exif(jxl))
            return -1;

        if (vips_foreign_load_jxl_set_header(jxl, load.Out))
            return -1;

        VIPS_SETSTR(load.Out.Filename, vips_connection_filename(VIPS_CONNECTION(jxl.source)));

        return 0;
    }

    private int vips_foreign_load_jxl_load(VipsForeignLoad load)
    {
        var jxl = (VipsForeignLoadJxl)load;
        var t = new VipsImage[VipsObject.LocalArraySize((VipsObject)load, 3)];

        VipsImage out;

#ifdef DEBUG
        Console.WriteLine("vips_foreign_load_jxl_load:");
#endif /*DEBUG*/

        t[0] = vips_image_new();
        if (vips_foreign_load_jxl_set_header(jxl, t[0]))
            return -1;

        // We have to rewind ... we can't be certain the header
        // decoder left the input in the correct place.
        if (vips_source_rewind(jxl.source))
            return -1;

        JxlDecoder.Rewind(jxl.decoder);
        if (JxlDecoder.SubscribeEvents(jxl.decoder, JXL_DEC_FRAME | JXL_DEC_FULL_IMAGE))
        {
            vips_foreign_load_jxl_error(jxl, "JxlDecoderSubscribeEvents");
            return -1;
        }

        if (vips_foreign_load_jxl_fill_input(jxl, 0) < 0)
            return -1;
        JxlDecoder.SetInput(jxl.decoder, jxl.input_buffer, jxl.bytes_in_buffer);

        if (jxl.n > 1)
        {
            if (vips_image_generate(t[0], null, vips_foreign_load_jxl_generate, null, jxl, null) || vips_sequential(t[0], ref t[1], null))
                return -1;

            out = t[1];
        }
        else
        {
            // We need only a single frame, we can read it right to the output
            if (vips_image_write_prepare(t[0]) || vips_foreign_load_jxl_read_frame(jxl, t[0], jxl.page + 1))
                return -1;

            out = t[0];
        }

        if (vips_image_write(out, load.Real))
            return -1;

        // Switch to pixel decode.
        if (vips_source_decode(jxl.source))
            return -1;

        return 0;
    }
}
```

Note that I've assumed the existence of certain types and methods from the VIPS library, as well as the `JxlDecoder` and `JxlThreadParallelRunner` classes. You may need to modify the code to match your specific use case.

Also note that this is a direct translation of the C code, without any optimizations or improvements for C#. If you're interested in optimizing the performance of this code, I'd be happy to help with that as well!