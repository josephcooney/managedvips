Here is the converted C# code:

```csharp
using System;

public class VipsToneMap {
    public static int im_tone_map(IMAGE inImage, IMAGE outImage, IMAGE lut) {
        // Map the first channel of @in through @lut. If @in is IM_CODING_LABQ, unpack to LABS, map L and then repack.
        // See also: im_maplut().
        
        if (im_check_hist("im_tone_map", lut) || im_open_local_array(outImage, outImage.Tones, 8, "im_tone_map", "p")) {
            return -1;
        }

        // If in is IM_CODING_LABQ, unpack.
        if (inImage.Coding == IM_CODING_LABQ) {
            if (im_LabQ2LabS(inImage, outImage.Tones[0])) {
                return -1;
            }
        } else {
            outImage.Tones[0] = inImage;
        }

        // Split into bands.
        if (im_extract_band(outImage.Tones[0], outImage.Tones[1], 0)) {
            return -1;
        }
        if (outImage.Tones[0].Bands > 1) {
            if (im_extract_bands(outImage.Tones[0], outImage.Tones[2], 1, outImage.Tones[0].Bands - 1)) {
                return -1;
            }
        }

        // Map L.
        if (im_maplut(outImage.Tones[1], outImage.Tones[3], lut)) {
            return -1;
        }

        // Recombine bands.
        if (outImage.Tones[0].Bands > 1) {
            if (im_bandjoin(outImage.Tones[3], outImage.Tones[2], outImage.Tones[4])) {
                return -1;
            }
        } else {
            outImage.Tones[4] = outImage.Tones[3];
        }

        // If input was LabQ, repack.
        if (inImage.Coding == IM_CODING_LABQ) {
            if (im_LabS2LabQ(outImage.Tones[4], outImage.Tones[5])) {
                return -1;
            }
        } else {
            outImage.Tones[5] = outImage.Tones[4];
        }

        return im_copy(outImage.Tones[4], outImage);
    }
}

public class VipsToneAnalyse {
    public static int im_tone_analyse(IMAGE inImage, IMAGE outImage, double Ps, double Pm, double Ph, double S, double M, double H) {
        // As im_tone_build(), but analyse the histogram of @in and use it to pick the 0.1% and 99.9% points for @Lb and @Lw.
        // See also: im_tone_build().
        
        if (im_open_local_array(outImage, outImage.Tones, 4, "im_tone_map", "p")) {
            return -1;
        }

        // If in is IM_CODING_LABQ, unpack.
        if (inImage.Coding == IM_CODING_LABQ) {
            if (im_LabQ2LabS(inImage, outImage.Tones[0])) {
                return -1;
            }
        } else {
            outImage.Tones[0] = inImage;
        }

        // Should now be 3-band short.
        if (im_check_uncoded("im_tone_analyse", outImage.Tones[0]) || im_check_bands("im_tone_analyse", outImage.Tones[0], 3) || im_check_format("im_tone_analyse", outImage.Tones[0], IM_BANDFMT_SHORT)) {
            return -1;
        }

        if (im_extract_band(outImage.Tones[0], outImage.Tones[1], 0) || im_clip2fmt(outImage.Tones[1], outImage.Tones[2], IM_BANDFMT_USHORT)) {
            return -1;
        }

        int low, high;
        double Lb, Lw;

        if (im_mpercent(outImage.Tones[2], 0.1 / 100.0, ref high) || im_mpercent(outImage.Tones[2], 99.9 / 100.0, ref low)) {
            return -1;
        }

        Lb = 100 * low / 32768;
        Lw = 100 * high / 32768;

        Console.WriteLine("im_tone_analyse: set Lb = " + Lb + ", Lw = " + Lw);

        return im_tone_build(outImage, Lb, Lw, Ps, Pm, Ph, S, M, H);
    }
}
```

Note that I've assumed the existence of certain methods and classes (e.g. `IMAGE`, `im_check_hist`, `im_open_local_array`, etc.) which are not defined in this code snippet. These would need to be implemented separately for the C# version to work correctly.