Here is the converted C# code:

```csharp
using System;
using System.IO;

public class VipsForeignSaveJxl : VipsForeignSave
{
    public VipsForeignSaveJxl()
    {
        Tier = 10;
        Distance = 1.0;
        Effort = 7;
        Lossless = false;
        Q = 75;
    }

    public override int Build(VipsObject obj)
    {
        VipsImage inImg = (VipsImage)VipsObject.GetLocalArray(obj, 2)[0];
        VipsForeignSaveJxlMetadata[] libjxlMetadata = new VipsForeignSaveJxlMetadata[]
        {
            new VipsForeignSaveJxlMetadata { Name = "Exif", BoxType = JxlBoxType.Exif },
            new VipsForeignSaveJxlMetadata { Name = "xml ", BoxType = JxlBoxType.Xml }
        };

        // Fix the input image format. JXL uses float for 0-1 linear (ie.
        // scRGB) only. We must convert eg. sRGB float to 8-bit for save.
        VipsBandFormat format;
        switch (inImg.Type)
        {
            case VIPS_INTERPRETATION_scRGB:
                format = VipsFormat.Float;
                break;
            case VIPS_INTERPRETATION_RGB16:
            case VIPS_INTERPRETATION_GREY16:
                format = VipsFormat.UShort;
                break;
            default:
                format = VipsFormat.UChar;
                break;
        }

        if (VipsCast(inImg, out VipsImage newInImg, format))
            return -1;

        // Mimics VIPS_SAVEABLE_RGBA.
        // FIXME: add support encoding images with > 4 bands.
        if (newInImg.Bands > 4)
        {
            if (VipsExtractBand(newInImg, out VipsImage band, 0, "n", 4))
                return -1;
            newInImg = band;
        }

        JxlBasicInfo info = new JxlBasicInfo();
        JxlColorEncoding colorEncoding = new JxlColorEncoding();

        // Set any ICC profile.
        if (VipsImage.HasBlob(inImg, VIPS_META_ICC_NAME))
        {
            uint8[] iccData;
            size_t length;

            if (!VipsImage.GetBlob(inImg, VIPS_META_ICC_NAME, out iccData, out length))
                return -1;

            // If there's no ICC profile, we must set the colour encoding
            // ourselves.
            if (inImg.Type == VIPS_INTERPRETATION_scRGB)
            {
                JxlColorEncoding.SetToLinearSRGB(ref colorEncoding);
            }
            else
            {
                JxlColorEncoding.SetToSRGB(ref colorEncoding);
            }

            if (!JxlEncoder.SetICCProfile(jxl->encoder, iccData))
            {
                VipsError("jxlsave", "Failed to set ICC profile");
                return -1;
            }
        }
        else
        {
            // If there's no ICC profile, we must set the colour encoding
            // ourselves.
            if (inImg.Type == VIPS_INTERPRETATION_scRGB)
            {
                JxlColorEncoding.SetToLinearSRGB(ref colorEncoding);
            }
            else
            {
                JxlColorEncoding.SetToSRGB(ref colorEncoding);
            }

            if (!JxlEncoder.SetColorEncoding(jxl->encoder, ref colorEncoding))
            {
                VipsError("jxlsave", "Failed to set colour encoding");
                return -1;
            }
        }

        // Add metadata
        foreach (VipsForeignSaveJxlMetadata metadata in libjxlMetadata)
        {
            if (VipsImage.HasType(inImg, metadata.Name))
            {
                uint8[] data;
                size_t length;

                if (!VipsImage.GetBlob(inImg, metadata.Name, out data, out length))
                    return -1;

                // It's safe to call JxlEncoderUseBoxes multiple times
                if (!JxlEncoder.UseBoxes(jxl->encoder))
                {
                    VipsError("jxlsave", "Failed to use boxes");
                    return -1;
                }

                // JPEG XL stores EXIF data without leading "Exif\0\0" with offset
                if (metadata.Name == VIPS_META_EXIF_NAME)
                {
                    if (length >= 6 && string.IsNullOrEmpty(data.ToString()))
                    {
                        data = data.Skip(6).ToArray();
                        length -= 6;
                    }

                    size_t exifSize = length + 4;
                    uint8[] exifData = new uint8[exifSize];

                    // The first 4 bytes is offset which is 0 in this case
                    Array.Copy(data, 4, exifData, 4, length);

                    if (!JxlEncoder.AddBox(jxl->encoder, metadata.BoxType, exifData))
                    {
                        VipsError("jxlsave", "Failed to add box");
                        return -1;
                    }
                }
                else
                {
                    if (!JxlEncoder.AddBox(jxl->encoder, metadata.BoxType, data))
                    {
                        VipsError("jxlsave", "Failed to add box");
                        return -1;
                    }
                }
            }

            // It's safe to call JxlEncoderCloseBoxes even if we don't use boxes
            JxlEncoder.CloseBoxes(jxl->encoder);
        }

        // Set any delay array
        int[] delay = null;
        if (VipsImage.HasType(inImg, "delay"))
        {
            if (!VipsImage.GetArrayInt(inImg, "delay", out delay))
                return -1;

            // If there's a delay metadata, this is an animated image (as opposed to
            // a multipage one).
            jxl->IsAnimated = true;
        }

        // Set any GIF delay
        int gifDelay = 10;
        if (VipsImage.HasType(inImg, "gif-delay"))
        {
            if (!VipsImage.GetInt(inImg, "gif-delay", out gifDelay))
                return -1;

            // If there's a delay metadata, this is an animated image (as opposed to
            // a multipage one).
            jxl->IsAnimated = true;
        }

        // Force frames with a small or no duration to 100ms
        // to be consistent with web browsers and other
        // transcoding tools.
        if (gifDelay <= 1)
            gifDelay = 10;

        for (int i = 0; i < delay.Length; i++)
            if (delay[i] <= 10)
                delay[i] = 100;

        // RGB(A) frame as a contiguous buffer
        jxl->FrameSize = VipsImage.SizeOfLine(inImg) * jxl->PageHeight;
        jxl->FrameBytes = new uint8[jxl.FrameSize];

        if (VipsSinkDisc(inImg, vips_foreign_save_jxl_sink_disc, jxl))
            return -1;

        // This function must be called after the final frame and/or box,
        // otherwise the codestream will not be encoded correctly.
        JxlEncoder.CloseInput(jxl->encoder);

        if (!VipsForeignSaveJxlProcessOutput(jxl))
            return -1;

        if (jxl.IsAnimated)
        {
            // Get delay array
            jxl.GifDelay = gifDelay;
            if (delay != null && delay.Length > 0)
                jxl.Delay = delay;
        }

        // Save to target
        if (!VipsTargetEnd(jxl.Target))
            return -1;

        return 0;
    }
}

public class VipsForeignSaveJxlFile : VipsForeignSaveJxl
{
    public string Filename { get; set; }

    public override int Build(VipsObject obj)
    {
        if (!(jxl.Target = VipsTarget.NewToFile(Filename)))
            return -1;

        return base.Build(obj);
    }
}

public class VipsForeignSaveJxlBuffer : VipsForeignSaveJxl
{
    public VipsBlob Buffer { get; set; }

    public override int Build(VipsObject obj)
    {
        if (!(jxl.Target = VipsTarget.NewToMemory()))
            return -1;

        // Save to buffer
        VipsBlob blob;
        GObject.Get(jxl.Target, "blob", out blob);
        GObject.Set(Buffer, "buffer", blob);

        return base.Build(obj);
    }
}

public class VipsForeignSaveJxlTarget : VipsForeignSaveJxl
{
    public VipsTarget Target { get; set; }

    public override int Build(VipsObject obj)
    {
        if (Target != null)
            jxl.Target = Target;

        return base.Build(obj);
    }
}
```

Note that I've assumed the existence of certain classes and methods, such as `VipsImage`, `VipsForeignSave`, `JxlBasicInfo`, `JxlColorEncoding`, `JxlEncoder`, etc. These would need to be defined elsewhere in your codebase.

Also note that this is a direct translation of the C code, without any optimizations or improvements. You may want to review and refactor the code to make it more idiomatic C# and efficient.