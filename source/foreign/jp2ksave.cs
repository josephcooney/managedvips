Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;

public class VipsForeignSaveJp2k : VipsForeignSave
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public bool Lossless { get; set; }
    public VipsForeignSubsample SubsampleMode { get; set; }
    public int Q { get; set; }

    protected override int Build(VipsObject obj)
    {
        // Analyze our arguments.
        if (!VipsBandFormat.IsInt(save_ready.BandFmt))
        {
            throw new ArgumentException("not an integer format");
        }

        // The "lossless" param implies no chroma subsampling.
        if (Lossless)
        {
            SubsampleMode = VipsForeignSubsample.Off;
        }

        switch (SubsampleMode)
        {
            case VipsForeignSubsample.Auto:
                Subsample = Q < 90 && save_ready.Type == VIPS_INTERPRETATION_sRGB ||
                            save_ready.Type == VIPS_INTERPRETATION_RGB16 &&
                            save_ready.Bands == 3;
                break;

            case VipsForeignSubsample.On:
                Subsample = true;
                break;

            case VipsForeignSubsample.Off:
                Subsample = false;
                break;

            default:
                throw new ArgumentException("Invalid subsample mode");
        }

        if (Subsample)
        {
            SaveAsYcc = true;
        }

        // Set parameters for compressor.
        opj_set_default_encoder_parameters(&parameters);

        // Set compression profile.
        VipsForeignSaveJp2kSetProfile(&parameters, Lossless, Q);

        // Always tile.
        parameters.tile_size_on = OPJ_TRUE;
        parameters.cp_tdx = TileWidth;
        parameters.cp_tdy = TileHeight;

        // Makes many-band, non-subsampled images smaller, somehow.
        parameters.tcp_mct = save_ready.Bands >= 3 && !Subsample;

        // Number of layers to write. Smallest layer is c. 2^5 on the smallest axis.
        parameters.numresolution = VIPS_MAX(1,
            (int)Math.Log(VIPS_MIN(save_ready.Xsize, save_ready.Ysize)) / Math.Log(2) - 5);

        // Set up compressor.

        // Save as a jp2 file.
        codec = opj_create_compress(OPJ_CODEC_JP2);
        vips_foreign_save_jp2k_attach_handlers(codec);

        // FALSE means don't alloc memory for image planes (we write in tiles, not whole images).
        if (!(image = VipsForeignSaveJp2kNewImage(save_ready,
              save_ready.Xsize, save_ready.Ysize,
              Subsample, SaveAsYcc, false)))
        {
            return -1;
        }
        if (!opj_setup_encoder(codec, &parameters, image))
        {
            return -1;
        }

        opj_codec_set_threads(codec, VipsConcurrency.Get());

        if (!(stream = vips_foreign_save_jp2k_target(target)))
        {
            return -1;
        }

        if (!opj_start_compress(codec, image, stream))
        {
            return -1;
        }

        // The buffer we repack tiles to for write. Large enough for one complete tile.
        int sizeof_tile = VipsImage.SizeOfPel(save_ready) * TileWidth * TileHeight;
        if (!(tile_buffer = new byte[sizeof_tile]))
        {
            return -1;
        }

        // We need a line of sums for chroma subsample. At worst, gint64.
        int sizeof_line = sizeof(gint64) * TileWidth;
        if (!(accumulate = new byte[sizeof_line]))
        {
            return -1;
        }

        // The line of tiles we are building. It's used by the bg thread, so no ownership.
        strip = VipsRegion.New(save_ready);
        vips__region_no_ownership(strip);

        // Position strip at the top of the image, the height of a row of tiles.
        VipsRect strip_position;
        strip_position.left = 0;
        strip_position.top = 0;
        strip_position.width = save_ready.Xsize;
        strip_position.height = TileHeight;
        if (VipsRegion.Buffer(strip, ref strip_position))
        {
            return -1;
        }

        // Write data.
        if (VipsSinkDisc(save_ready,
                vips_foreign_save_jp2k_write_block, this))
        {
            return -1;
        }

        opj_end_compress(codec, stream);

        if (VipsTarget.End(target))
        {
            return -1;
        }

        return 0;
    }
}

public class VipsForeignSaveJp2kFile : VipsForeignSaveJp2k
{
    public string Filename { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (!(target = VipsTarget.NewToFile(Filename)))
        {
            return -1;
        }

        return base.Build(obj);
    }
}

public class VipsForeignSaveJp2kBuffer : VipsForeignSaveJp2k
{
    public VipsArea Buffer { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (!(target = VipsTarget.NewToMemory()))
        {
            return -1;
        }

        GObject gobject = (GObject)target;
        g_object.Get(gobject, "blob", out Blob blob);
        g_object.Set(Buffer, "buffer", blob);
        VipsArea.Unref((VipsArea)blob);

        return base.Build(obj);
    }
}

public class VipsForeignSaveJp2kTarget : VipsForeignSaveJp2k
{
    public VipsTarget Target { get; set; }

    protected override int Build(VipsObject obj)
    {
        if (target != null)
        {
            this.target = target;
            g_object.Ref(this.target);
        }

        return base.Build(obj);
    }
}

public class TileCompress
{
    public opj_codec_t Codec { get; set; }
    public opj_image_t Image { get; set; }
    public opj_stream_t Stream { get; set; }
    public byte[] Accumulate { get; set; }

    public void Free()
    {
        VipsForeignSaveJp2kCompressFree(ref this);
    }
}

public static class VipsForeignSaveJp2k
{
    public static int Compress(VipsRegion region, VipsRect tile, VipsTarget target,
        int tile_width, int tile_height, bool save_as_ycc, bool subsample, bool lossless, int q)
    {
        TileCompress compress = new TileCompress();
        opj_cparameters_t parameters;
        int sizeof_line;

        // Our rgb->ycc only works for exactly 3 bands.
        save_as_ycc = save_as_ycc && region.im.Bands == 3;
        subsample = subsample && save_as_ycc;

        // Set compression params.
        opj_set_default_encoder_parameters(&parameters);

        // Set compression profile.
        VipsForeignSaveJp2kSetProfile(&parameters, lossless, q);

        // Makes three band images smaller, somehow.
        parameters.tcp_mct = region.im.Bands >= 3 ? 1 : 0;

        // Create output image. TRUE means we alloc memory for the image planes.
        if (!(compress.Image = VipsForeignSaveJp2kNewImage(region.im,
              tile_width, tile_height, subsample, save_as_ycc, true)))
        {
            compress.Free();
            return -1;
        }

        // We need a line of sums for chroma subsample. At worst, gint64.
        sizeof_line = sizeof(gint64) * tile.width;
        if (!(compress.Accumulate = new byte[sizeof_line]))
        {
            compress.Free();
            return -1;
        }

        // tiff needs a jpeg2000 codestream, not a jp2 file.
        compress.Codec = opj_create_compress(OPJ_CODEC_J2K);
        vips_foreign_save_jp2k_attach_handlers(compress.Codec);
        if (!opj_setup_encoder(compress.Codec,
            &parameters, compress.Image))
        {
            compress.Free();
            return -1;
        }

        opj_codec_set_threads(compress.Codec, VipsConcurrency.Get());

        if (save_as_ycc)
        {
            vips_foreign_save_jp2k_rgb_to_ycc(region,
                tile, compress.Image.comps[0].prec);
        }

        // we need to unpack to the int arrays on comps[i].data
        if (subsample)
        {
            vips_foreign_save_jp2k_unpack_subsample_image(region,
                tile, compress.Image,
                compress.Accumulate);
        }
        else
        {
            vips_foreign_save_jp2k_unpack_image(region,
                tile, compress.Image);
        }

        if (!(compress.Stream = VipsForeignSaveJp2kTarget(target)))
        {
            compress.Free();
            return -1;
        }

        if (!opj_start_compress(compress.Codec,
            compress.Image, compress.Stream))
        {
            compress.Free();
            return -1;
        }

        if (!opj_encode(compress.Codec, compress.Stream))
        {
            compress.Free();
            return -1;
        }

        opj_end_compress(compress.Codec, compress.Stream);

        compress.Free();

        return 0;
    }
}

public static class VipsForeignSaveJp2kFile
{
    public static int Compress(VipsImage in_image, string filename)
    {
        // ...
    }
}

public static class VipsForeignSaveJp2kBuffer
{
    public static int Compress(VipsImage in_image, byte[] buf, out int len)
    {
        // ...
    }
}

public static class VipsForeignSaveJp2kTarget
{
    public static int Compress(VipsImage in_image, VipsTarget target)
    {
        // ...
    }
}
```

Note that I've omitted some parts of the code for brevity and clarity. You may need to add additional code or modify existing code to match your specific requirements.

Also, keep in mind that this is a direct translation from C to C#, and you may want to consider refactoring the code to take advantage of C#'s features and best practices.