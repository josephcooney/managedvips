Here is the converted C# code:

```csharp
using System;

namespace Vips
{
    public class ConversionFunctions
    {
        // system_vec
        public static int SystemVec(IMAGE inImage, string command, out string[] output)
        {
            return im_system(inImage, command, output);
        }

        // system_args
        private static readonly ArgumentDescriptor[] SystemArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("im", "Input image"),
            new ArgumentDescriptor("command", "Command to run on the image"),
            new ArgumentDescriptor("output", "Output string")
        };

        // system_desc
        public static readonly FunctionDescriptor SystemDesc = new FunctionDescriptor(
            "system",
            "Run a command on an image",
            0,
            SystemVec,
            SystemArgs);

        // system_image_vec
        public static int SystemImageVec(IMAGE inImage, IMAGE outImage, string inFormat, string outFormat, string command, out string[] log)
        {
            return im_system_image(inImage, inFormat, outFormat, command, log);
        }

        // system_image_args
        private static readonly ArgumentDescriptor[] SystemImageArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("in_format", "Input format"),
            new ArgumentDescriptor("out_format", "Output format"),
            new ArgumentDescriptor("command", "Command to run on the image"),
            new ArgumentDescriptor("log", "Log string")
        };

        // system_image_desc
        public static readonly FunctionDescriptor SystemImageDesc = new FunctionDescriptor(
            "system_image",
            "Run a command on an image, with image output",
            0,
            SystemImageVec,
            SystemImageArgs);

        // subsample_vec
        public static int SubsampleVec(IMAGE inImage, IMAGE outImage, int xshrink, int yshrink)
        {
            return im_subsample(inImage, outImage, xshrink, yshrink);
        }

        // subsample_args
        private static readonly ArgumentDescriptor[] SubsampleArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("xshrink", "X shrink factor"),
            new ArgumentDescriptor("yshrink", "Y shrink factor")
        };

        // subsample_desc
        public static readonly FunctionDescriptor SubsampleDesc = new FunctionDescriptor(
            "subsample",
            "Subsample an image by integer factors",
            ImFn.Pio,
            SubsampleVec,
            SubsampleArgs);

        // gaussnoise_args
        private static readonly ArgumentDescriptor[] GaussNoiseArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("xsize", "X size of the noise image"),
            new ArgumentDescriptor("ysize", "Y size of the noise image"),
            new ArgumentDescriptor("mean", "Mean of the noise"),
            new ArgumentDescriptor("sigma", "Sigma of the noise")
        };

        // gaussnoise_vec
        public static int GaussNoiseVec(IMAGE outImage, int xsize, int ysize, double mean, double sigma)
        {
            return im_gaussnoise(outImage, xsize, ysize, mean, sigma);
        }

        // gaussnoise_desc
        public static readonly FunctionDescriptor GaussNoiseDesc = new FunctionDescriptor(
            "gaussnoise",
            "Generate an image of gaussian noise with specified statistics",
            ImFn.Pio | ImFn.Nocache,
            GaussNoiseVec,
            GaussNoiseArgs);

        // extract_args
        private static readonly ArgumentDescriptor[] ExtractArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("left", "Left position of the area to extract"),
            new ArgumentDescriptor("top", "Top position of the area to extract"),
            new ArgumentDescriptor("width", "Width of the area to extract"),
            new ArgumentDescriptor("height", "Height of the area to extract"),
            new ArgumentDescriptor("band", "Band number to extract")
        };

        // extract_vec
        public static int ExtractVec(IMAGE inputImage, IMAGE outputImage, int left, int top, int width, int height, int band)
        {
            return im_extract_areabands(inputImage, outputImage, left, top, width, height, band, 1);
        }

        // extract_desc
        public static readonly FunctionDescriptor ExtractDesc = new FunctionDescriptor(
            "extract",
            "Extract an area or band from an image",
            ImFn.Transform | ImFn.Pio,
            ExtractVec,
            ExtractArgs);

        // extract_area_args
        private static readonly ArgumentDescriptor[] ExtractAreaArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("left", "Left position of the area to extract"),
            new ArgumentDescriptor("top", "Top position of the area to extract"),
            new ArgumentDescriptor("width", "Width of the area to extract"),
            new ArgumentDescriptor("height", "Height of the area to extract")
        };

        // extract_area_vec
        public static int ExtractAreaVec(IMAGE inputImage, IMAGE outputImage, int x, int y, int w, int h)
        {
            return im_extract_area(inputImage, outputImage, x, y, w, h);
        }

        // extract_area_desc
        public static readonly FunctionDescriptor ExtractAreaDesc = new FunctionDescriptor(
            "extract_area",
            "Extract an area from an image",
            ImFn.Transform | ImFn.Pio,
            ExtractAreaVec,
            ExtractAreaArgs);

        // extract_bands_args
        private static readonly ArgumentDescriptor[] ExtractBandsArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("band", "Band number to extract"),
            new ArgumentDescriptor("nbands", "Number of bands to extract")
        };

        // extract_bands_vec
        public static int ExtractBandsVec(IMAGE inputImage, IMAGE outputImage, int chsel, int nbands)
        {
            return im_extract_bands(inputImage, outputImage, chsel, nbands);
        }

        // extract_bands_desc
        public static readonly FunctionDescriptor ExtractBandsDesc = new FunctionDescriptor(
            "extract_bands",
            "Extract several bands from an image",
            ImFn.Pio,
            ExtractBandsVec,
            ExtractBandsArgs);

        // extract_band_args
        private static readonly ArgumentDescriptor[] ExtractBandArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("band", "Band number to extract")
        };

        // extract_band_vec
        public static int ExtractBandVec(IMAGE inputImage, IMAGE outputImage, int chsel)
        {
            return im_extract_band(inputImage, outputImage, chsel);
        }

        // extract_band_desc
        public static readonly FunctionDescriptor ExtractBandDesc = new FunctionDescriptor(
            "extract_band",
            "Extract a band from an image",
            ImFn.Pio,
            ExtractBandVec,
            ExtractBandArgs);

        // extract_areabands_args
        private static readonly ArgumentDescriptor[] ExtractAreabandsArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("left", "Left position of the area to extract"),
            new ArgumentDescriptor("top", "Top position of the area to extract"),
            new ArgumentDescriptor("width", "Width of the area to extract"),
            new ArgumentDescriptor("height", "Height of the area to extract"),
            new ArgumentDescriptor("band", "Band number to extract"),
            new ArgumentDescriptor("nbands", "Number of bands to extract")
        };

        // extract_areabands_vec
        public static int ExtractAreabandsVec(IMAGE inputImage, IMAGE outputImage, int left, int top, int width, int height, int band, int nbands)
        {
            return im_extract_areabands(inputImage, outputImage, left, top, width, height, band, nbands);
        }

        // extract_areabands_desc
        public static readonly FunctionDescriptor ExtractAreabandsDesc = new FunctionDescriptor(
            "extract_areabands",
            "Extract an area and bands from an image",
            ImFn.Transform | ImFn.Pio,
            ExtractAreabandsVec,
            ExtractAreabandsArgs);

        // bandjoin_args
        private static readonly ArgumentDescriptor[] BandJoinArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in1", "First input image"),
            new ArgumentDescriptor("in2", "Second input image"),
            new ArgumentDescriptor("out", "Output image")
        };

        // bandjoin_vec
        public static int BandJoinVec(IMAGE in1, IMAGE in2, IMAGE out)
        {
            return im_bandjoin(in1, in2, out);
        }

        // bandjoin_desc
        public static readonly FunctionDescriptor BandJoinDesc = new FunctionDescriptor(
            "bandjoin",
            "Bandwise join of two images",
            ImFn.Pio,
            BandJoinVec,
            BandJoinArgs);

        // gbandjoin_args
        private static readonly ArgumentDescriptor[] GbandJoinArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image vector"),
            new ArgumentDescriptor("out", "Output image")
        };

        // gbandjoin_vec
        public static int GbandJoinVec(im_imagevec_object inImageVector, IMAGE out)
        {
            return im_gbandjoin(inImageVector.vec, out, inImageVector.n);
        }

        // gbandjoin_desc
        public static readonly FunctionDescriptor GbandJoinDesc = new FunctionDescriptor(
            "gbandjoin",
            "Bandwise join of many images",
            ImFn.Pio,
            (arg) => GbandJoinVec((im_imagevec_object)arg, (IMAGE)arg[1]),
            GbandJoinArgs);

        // text_args
        private static readonly ArgumentDescriptor[] TextArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("text", "Text to render"),
            new ArgumentDescriptor("font", "Font to use"),
            new ArgumentDescriptor("width", "Width of the text"),
            new ArgumentDescriptor("alignment", "Alignment of the text"),
            new ArgumentDescriptor("dpi", "DPI of the output image")
        };

        // text_vec
        public static int TextVec(IMAGE outImage, string text, string font, int width, int alignment, int dpi)
        {
            return im_text(outImage, text, font, width, alignment, dpi);
        }

        // text_desc
        public static readonly FunctionDescriptor TextDesc = new FunctionDescriptor(
            "text",
            "Generate a text image",
            ImFn.Pio,
            (arg) => TextVec((IMAGE)arg[0], (string)arg[1], (string)arg[2], (int)arg[3], (int)arg[4], (int)arg[5]),
            TextArgs);

        // black_args
        private static readonly ArgumentDescriptor[] BlackArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("x_size", "X size of the output image"),
            new ArgumentDescriptor("y_size", "Y size of the output image"),
            new ArgumentDescriptor("bands", "Number of bands in the output image")
        };

        // black_vec
        public static int BlackVec(IMAGE outImage, int xs, int ys, int bands)
        {
            return im_black(outImage, xs, ys, bands);
        }

        // black_desc
        public static readonly FunctionDescriptor BlackDesc = new FunctionDescriptor(
            "black",
            "Generate a black image",
            ImFn.Pio,
            (arg) => BlackVec((IMAGE)arg[0], (int)arg[1], (int)arg[2], (int)arg[3]),
            BlackArgs);

        // clip2fmt_args
        private static readonly ArgumentDescriptor[] Clip2FmtArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("ofmt", "Output format")
        };

        // clip2fmt_vec
        public static int Clip2FmtVec(IMAGE inImage, IMAGE outImage, int ofmt)
        {
            return im_clip2fmt(inImage, outImage, ofmt);
        }

        // clip2fmt_desc
        public static readonly FunctionDescriptor Clip2FmtDesc = new FunctionDescriptor(
            "clip2fmt",
            "Convert an image format to a specified output format",
            ImFn.Pio | ImFn.Ptop,
            (arg) => Clip2FmtVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2]),
            Clip2FmtArgs);

        // c2rect_vec
        public static int C2RectVec(IMAGE inImage, IMAGE outImage)
        {
            return im_c2rect(inImage, outImage);
        }

        // c2rect_desc
        public static readonly FunctionDescriptor C2RectDesc = new FunctionDescriptor(
            "c2rect",
            "Convert phase and amplitude to real and imaginary",
            ImFn.Ptop | ImFn.Pio,
            (arg) => C2RectVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // c2amph_vec
        public static int C2AmphVec(IMAGE inImage, IMAGE outImage)
        {
            return im_c2amph(inImage, outImage);
        }

        // c2amph_desc
        public static readonly FunctionDescriptor C2AmphDesc = new FunctionDescriptor(
            "c2amph",
            "Convert real and imaginary to phase and amplitude",
            ImFn.Ptop | ImFn.Pio,
            (arg) => C2AmphVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // ri2c_vec
        public static int Ri2CVec(IMAGE inImage, IMAGE outImage)
        {
            return im_ri2c(inImage, outImage);
        }

        // ri2c_desc
        public static readonly FunctionDescriptor Ri2CDesc = new FunctionDescriptor(
            "ri2c",
            "Join two non-complex images to form a complex image",
            ImFn.Ptop | ImFn.Pio,
            (arg) => Ri2CVec((IMAGE)arg[0], (IMAGE)arg[1]),
            two_in_one_out);

        // c2imag_vec
        public static int C2ImagVec(IMAGE inImage, IMAGE outImage)
        {
            return im_c2imag(inImage, outImage);
        }

        // c2imag_desc
        public static readonly FunctionDescriptor C2ImagDesc = new FunctionDescriptor(
            "c2imag",
            "Extract the imaginary part of a complex image",
            ImFn.Ptop | ImFn.Pio,
            (arg) => C2ImagVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // c2real_vec
        public static int C2RealVec(IMAGE inImage, IMAGE outImage)
        {
            return im_c2real(inImage, outImage);
        }

        // c2real_desc
        public static readonly FunctionDescriptor C2RealDesc = new FunctionDescriptor(
            "c2real",
            "Extract the real part of a complex image",
            ImFn.Ptop | ImFn.Pio,
            (arg) => C2RealVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // copy_set_args
        private static readonly ArgumentDescriptor[] CopySetArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("Type", "Copy type"),
            new ArgumentDescriptor("Xres", "X resolution"),
            new ArgumentDescriptor("Yres", "Y resolution"),
            new ArgumentDescriptor("Xoffset", "X offset"),
            new ArgumentDescriptor("Yoffset", "Y offset")
        };

        // copy_set_vec
        public static int CopySetVec(IMAGE inImage, IMAGE outImage, int Type, double Xres, double Yres, int Xoffset, int Yoffset)
        {
            return im_copy_set(inImage, outImage, Type, Xres, Yres, Xoffset, Yoffset);
        }

        // copy_set_desc
        public static readonly FunctionDescriptor CopySetDesc = new FunctionDescriptor(
            "copy_set",
            "Copy an image and set its informational fields",
            ImFn.Pio,
            (arg) => CopySetVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2], (double)arg[3], (double)arg[4], (int)arg[5], (int)arg[6]),
            CopySetArgs);

        // copy_set_meta_args
        private static readonly ArgumentDescriptor[] CopySetMetaArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("field", "Field to set"),
            new ArgumentDescriptor("value", "Value to set")
        };

        // copy_set_meta_vec
        public static int CopySetMetaVec(IMAGE inImage, IMAGE outImage, string field, GValue value)
        {
            return im_copy_set_meta(inImage, outImage, field, value);
        }

        // copy_set_meta_desc
        public static readonly FunctionDescriptor CopySetMetaDesc = new FunctionDescriptor(
            "copy_set_meta",
            "Copy an image and set a meta field",
            ImFn.Pio,
            (arg) => CopySetMetaVec((IMAGE)arg[0], (IMAGE)arg[1], (string)arg[2], (GValue)arg[3]),
            CopySetMetaArgs);

        // copy_morph_args
        private static readonly ArgumentDescriptor[] CopyMorphArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("Bands", "Number of bands in the output image"),
            new ArgumentDescriptor("BandFmt", "Format of the bands in the output image"),
            new ArgumentDescriptor("Coding", "Coding scheme for the output image")
        };

        // copy_morph_vec
        public static int CopyMorphVec(IMAGE inImage, IMAGE outImage, int Bands, int BandFmt, int Coding)
        {
            return im_copy_morph(inImage, outImage, Bands, BandFmt, Coding);
        }

        // copy_morph_desc
        public static readonly FunctionDescriptor CopyMorphDesc = new FunctionDescriptor(
            "copy_morph",
            "Copy an image and set its pixel layout",
            ImFn.Pio,
            (arg) => CopyMorphVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2], (int)arg[3], (int)arg[4]),
            CopyMorphArgs);

        // copy_args
        private static readonly ArgumentDescriptor[] CopyArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // copy_vec
        public static int CopyVec(IMAGE inImage, IMAGE outImage)
        {
            return im_copy(inImage, outImage);
        }

        // copy_desc
        public static readonly FunctionDescriptor CopyDesc = new FunctionDescriptor(
            "copy",
            "Copy an image",
            ImFn.Pio | ImFn.Nocache,
            (arg) => CopyVec((IMAGE)arg[0], (IMAGE)arg[1]),
            CopyArgs);

        // copy_file_args
        private static readonly ArgumentDescriptor[] CopyFileArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output file")
        };

        // copy_file_vec
        public static int CopyFileVec(IMAGE inImage, string out)
        {
            return im_copy_file(inImage, out);
        }

        // copy_file_desc
        public static readonly FunctionDescriptor CopyFileDesc = new FunctionDescriptor(
            "copy_file",
            "Copy an image to a file and return the file",
            ImFn.Pio,
            (arg) => CopyFileVec((IMAGE)arg[0], (string)arg[1]),
            CopyFileArgs);

        // copy_swap_args
        private static readonly ArgumentDescriptor[] CopySwapArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // copy_swap_vec
        public static int CopySwapVec(IMAGE inImage, IMAGE outImage)
        {
            return im_copy_swap(inImage, outImage);
        }

        // copy_swap_desc
        public static readonly FunctionDescriptor CopySwapDesc = new FunctionDescriptor(
            "copy_swap",
            "Copy an image and swap its byte order",
            ImFn.Pio,
            (arg) => CopySwapVec((IMAGE)arg[0], (IMAGE)arg[1]),
            CopySwapArgs);

        // fliphor_args
        private static readonly ArgumentDescriptor[] FliphorArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // fliphor_vec
        public static int FliphorVec(IMAGE inImage, IMAGE outImage)
        {
            return im_fliphor(inImage, outImage);
        }

        // fliphor_desc
        public static readonly FunctionDescriptor FliphorDesc = new FunctionDescriptor(
            "fliphor",
            "Flip an image left-right",
            ImFn.Pio,
            (arg) => FliphorVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // flipver_args
        private static readonly ArgumentDescriptor[] FlipverArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // flipver_vec
        public static int FlipverVec(IMAGE inImage, IMAGE outImage)
        {
            return im_flipver(inImage, outImage);
        }

        // flipver_desc
        public static readonly FunctionDescriptor FlipverDesc = new FunctionDescriptor(
            "flipver",
            "Flip an image top-bottom",
            ImFn.Pio,
            (arg) => FlipverVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // falsecolour_args
        private static readonly ArgumentDescriptor[] FalseColourArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // falsecolour_vec
        public static int FalseColourVec(IMAGE inImage, IMAGE outImage)
        {
            return im_falsecolour(inImage, outImage);
        }

        // falsecolour_desc
        public static readonly FunctionDescriptor FalseColourDesc = new FunctionDescriptor(
            "falsecolour",
            "Turn luminance changes into chrominance changes",
            ImFn.Ptop | ImFn.Pio,
            (arg) => FalseColourVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // insert_args
        private static readonly ArgumentDescriptor[] InsertArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image"),
            new ArgumentDescriptor("sub", "Sub-image to insert"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("x", "X position of the sub-image"),
            new ArgumentDescriptor("y", "Y position of the sub-image")
        };

        // insert_vec
        public static int InsertVec(IMAGE inImage, IMAGE subImage, IMAGE outImage, int x, int y)
        {
            return im_insert(inImage, subImage, outImage, x, y);
        }

        // insert_desc
        public static readonly FunctionDescriptor InsertDesc = new FunctionDescriptor(
            "insert",
            "Insert a sub-image into an image at a specified position",
            ImFn.Pio | ImFn.Transform,
            (arg) => InsertVec((IMAGE)arg[0], (IMAGE)arg[1], (IMAGE)arg[2], (int)arg[3], (int)arg[4]),
            InsertArgs);

        // insertset_args
        private static readonly ArgumentDescriptor[] InsertSetArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("main", "Main image"),
            new ArgumentDescriptor("sub", "Sub-image to insert"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("x", "X positions of the sub-image"),
            new ArgumentDescriptor("y", "Y positions of the sub-image")
        };

        // insertset_vec
        public static int InsertSetVec(IMAGE mainImage, IMAGE subImage, IMAGE outImage, int n, int[] x, int[] y)
        {
            return im_insertset(mainImage, subImage, outImage, n, x, y);
        }

        // insertset_desc
        public static readonly FunctionDescriptor InsertSetDesc = new FunctionDescriptor(
            "insertset",
            "Insert a sub-image into an image at every specified position",
            0,
            (arg) => InsertSetVec((IMAGE)arg[0], (IMAGE)arg[1], (IMAGE)arg[2], (int)arg[3], (int[])arg[4], (int[])arg[5]),
            InsertSetArgs);

        // insert_noexpand_args
        private static readonly ArgumentDescriptor[] InsertNoExpandArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image"),
            new ArgumentDescriptor("sub", "Sub-image to insert"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("x", "X position of the sub-image"),
            new ArgumentDescriptor("y", "Y position of the sub-image")
        };

        // insert_noexpand_vec
        public static int InsertNoExpandVec(IMAGE inImage, IMAGE subImage, IMAGE outImage, int x, int y)
        {
            return im_insert_noexpand(inImage, subImage, outImage, x, y);
        }

        // insert_noexpand_desc
        public static readonly FunctionDescriptor InsertNoExpandDesc = new FunctionDescriptor(
            "insert_noexpand",
            "Insert a sub-image into an image at a specified position without expanding the output image",
            ImFn.Pio | ImFn.Transform,
            (arg) => InsertNoExpandVec((IMAGE)arg[0], (IMAGE)arg[1], (IMAGE)arg[2], (int)arg[3], (int)arg[4]),
            InsertNoExpandArgs);

        // rot180_args
        private static readonly ArgumentDescriptor[] Rot180Args = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // rot180_vec
        public static int Rot180Vec(IMAGE inImage, IMAGE outImage)
        {
            return im_rot180(inImage, outImage);
        }

        // rot180_desc
        public static readonly FunctionDescriptor Rot180Desc = new FunctionDescriptor(
            "rot180",
            "Rotate an image 180 degrees",
            ImFn.Pio | ImFn.Transform,
            (arg) => Rot180Vec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // rot90_args
        private static readonly ArgumentDescriptor[] Rot90Args = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // rot90_vec
        public static int Rot90Vec(IMAGE inImage, IMAGE outImage)
        {
            return im_rot90(inImage, outImage);
        }

        // rot90_desc
        public static readonly FunctionDescriptor Rot90Desc = new FunctionDescriptor(
            "rot90",
            "Rotate an image 90 degrees clockwise",
            ImFn.Pio | ImFn.Transform,
            (arg) => Rot90Vec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // rot270_args
        private static readonly ArgumentDescriptor[] Rot270Args = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // rot270_vec
        public static int Rot270Vec(IMAGE inImage, IMAGE outImage)
        {
            return im_rot270(inImage, outImage);
        }

        // rot270_desc
        public static readonly FunctionDescriptor Rot270Desc = new FunctionDescriptor(
            "rot270",
            "Rotate an image 270 degrees clockwise",
            ImFn.Pio | ImFn.Transform,
            (arg) => Rot270Vec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // lrjoin_args
        private static readonly ArgumentDescriptor[] LrJoinArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in1", "First input image"),
            new ArgumentDescriptor("in2", "Second input image"),
            new ArgumentDescriptor("out", "Output image")
        };

        // lrjoin_vec
        public static int LrJoinVec(IMAGE in1, IMAGE in2, IMAGE out)
        {
            return im_lrjoin(in1, in2, out);
        }

        // lrjoin_desc
        public static readonly FunctionDescriptor LrJoinDesc = new FunctionDescriptor(
            "lrjoin",
            "Join two images left-right",
            ImFn.Pio | ImFn.Transform,
            (arg) => LrJoinVec((IMAGE)arg[0], (IMAGE)arg[1], (IMAGE)arg[2]),
            two_in_one_out);

        // tbjoin_args
        private static readonly ArgumentDescriptor[] TbJoinArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in1", "First input image"),
            new ArgumentDescriptor("in2", "Second input image"),
            new ArgumentDescriptor("out", "Output image")
        };

        // tbjoin_vec
        public static int TbJoinVec(IMAGE in1, IMAGE in2, IMAGE out)
        {
            return im_tbjoin(in1, in2, out);
        }

        // tbjoin_desc
        public static readonly FunctionDescriptor TbJoinDesc = new FunctionDescriptor(
            "tbjoin",
            "Join two images top-bottom",
            ImFn.Pio | ImFn.Transform,
            (arg) => TbJoinVec((IMAGE)arg[0], (IMAGE)arg[1], (IMAGE)arg[2]),
            two_in_one_out);

        // scale_args
        private static readonly ArgumentDescriptor[] ScaleArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // scale_vec
        public static int ScaleVec(IMAGE inImage, IMAGE outImage)
        {
            return im_scale(inImage, outImage);
        }

        // scale_desc
        public static readonly FunctionDescriptor ScaleDesc = new FunctionDescriptor(
            "scale",
            "Scale an image linearly to fit the range 0-255",
            ImFn.Pio,
            (arg) => ScaleVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // scaleps_args
        private static readonly ArgumentDescriptor[] ScalePsArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // scaleps_vec
        public static int ScalePsVec(IMAGE inImage, IMAGE outImage)
        {
            return im_scaleps(inImage, outImage);
        }

        // scaleps_desc
        public static readonly FunctionDescriptor ScalePsDesc = new FunctionDescriptor(
            "scaleps",
            "Logarithmic scale an image to fit the range 0-255",
            0,
            (arg) => ScalePsVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // grid_args
        private static readonly ArgumentDescriptor[] GridArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("tile_height", "Tile height"),
            new ArgumentDescriptor("across", "Number of tiles across"),
            new ArgumentDescriptor("down", "Number of tiles down")
        };

        // grid_vec
        public static int GridVec(IMAGE inImage, IMAGE outImage, int tileHeight, int across, int down)
        {
            return im_grid(inImage, outImage, tileHeight, across, down);
        }

        // grid_desc
        public static readonly FunctionDescriptor GridDesc = new FunctionDescriptor(
            "grid",
            "Chop a tall thin image into a grid of images",
            ImFn.Transform | ImFn.Pio,
            (arg) => GridVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2], (int)arg[3], (int)arg[4]),
            GridArgs);

        // replicate_args
        private static readonly ArgumentDescriptor[] ReplicateArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("across", "Number of times to replicate horizontally"),
            new ArgumentDescriptor("down", "Number of times to replicate vertically")
        };

        // replicate_vec
        public static int ReplicateVec(IMAGE inImage, IMAGE outImage, int across, int down)
        {
            return im_replicate(inImage, outImage, across, down);
        }

        // replicate_desc
        public static readonly FunctionDescriptor ReplicateDesc = new FunctionDescriptor(
            "replicate",
            "Replicate an image horizontally and vertically",
            ImFn.Transform | ImFn.Pio,
            (arg) => ReplicateVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2], (int)arg[3]),
            ReplicateArgs);

        // zoom_args
        private static readonly ArgumentDescriptor[] ZoomArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("xfac", "X scaling factor"),
            new ArgumentDescriptor("yfac", "Y scaling factor")
        };

        // zoom_vec
        public static int ZoomVec(IMAGE inImage, IMAGE outImage, int xFac, int yFac)
        {
            return im_zoom(inImage, outImage, xFac, yFac);
        }

        // zoom_desc
        public static readonly FunctionDescriptor ZoomDesc = new FunctionDescriptor(
            "zoom",
            "Simple zoom of an image by integer factors",
            ImFn.Transform | ImFn.Pio,
            (arg) => ZoomVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2], (int)arg[3]),
            ZoomArgs);

        // msb_args
        private static readonly ArgumentDescriptor[] MsbArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image")
        };

        // msb_vec
        public static int MsbVec(IMAGE inImage, IMAGE outImage)
        {
            return im_msb(inImage, outImage);
        }

        // msb_desc
        public static readonly FunctionDescriptor MsdDesc = new FunctionDescriptor(
            "msb",
            "Convert to uchar by discarding bits",
            ImFn.Pio | ImFn.Ptop,
            (arg) => MsbVec((IMAGE)arg[0], (IMAGE)arg[1]),
            one_in_one_out);

        // msb_band_args
        private static readonly ArgumentDescriptor[] MsbBandArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("in", "Input image"),
            new ArgumentDescriptor("out", "Output image"),
            new ArgumentDescriptor("band", "Band number to convert")
        };

        // msb_band_vec
        public static int MsbBandVec(IMAGE inImage, IMAGE outImage, int band)
        {
            return im_msb_band(inImage, outImage, band);
        }

        // msb_band_desc
        public static readonly FunctionDescriptor MsdBandDesc = new FunctionDescriptor(
            "msb_band",
            "Convert to single band uchar by discarding bits",
            ImFn.Pio | ImFn.Ptop,
            (arg) => MsbBandVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2]),
            MsbBandArgs);

        // wrap_args
        private static readonly ArgumentDescriptor[] WrapArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("x", "X offset to apply"),
            new ArgumentDescriptor("y", "Y offset to apply")
        };

        // wrap_vec
        public static int WrapVec(IMAGE inImage, IMAGE outImage)
        {
            return im_wrap(inImage, outImage);
        }

        // wrap_desc
        public static readonly FunctionDescriptor WrapDesc = new FunctionDescriptor(
            "wrap",
            "Shift the origin of an image and wrap at the sides",
            ImFn.Pio | ImFn.Transform,
            (arg) => WrapVec((IMAGE)arg[0], (IMAGE)arg[1]),
            WrapArgs);

        // embed_args
        private static readonly ArgumentDescriptor[] EmbedArgs = new ArgumentDescriptor[]
        {
            new ArgumentDescriptor("input", "Input image"),
            new ArgumentDescriptor("output", "Output image"),
            new ArgumentDescriptor("type", "Type of embedding to apply"),
            new ArgumentDescriptor("x", "X position of the embedded image"),
            new ArgumentDescriptor("y", "Y position of the embedded image"),
            new ArgumentDescriptor("width", "Width of the embedded image"),
            new ArgumentDescriptor("height", "Height of the embedded image")
        };

        // embed_vec
        public static int EmbedVec(IMAGE inImage, IMAGE outImage, int type, int x, int y, int width, int height)
        {
            return im_embed(inImage, outImage, type, x, y, width, height);
        }

        // embed_desc
        public static readonly FunctionDescriptor EmbedDesc = new FunctionDescriptor(
            "embed",
            "Embed an image within a set of borders",
            ImFn.Pio | ImFn.Transform,
            (arg) => EmbedVec((IMAGE)arg[0], (IMAGE)arg[1], (int)arg[2], (int)arg[3], (int)arg[4], (int)arg[5], (int)arg[6]),
            EmbedArgs);

        // conv_list
        private static readonly FunctionDescriptor[] ConvList = new FunctionDescriptor[]
        {
            GaussNoiseDesc,
            BandJoinDesc,
            BlackDesc,
            C2AmphDesc,
            C2ImagDesc,
            C2RealDesc,
            C2RectDesc,
            Clip2FmtDesc,
            CopyDesc,
            CopyFileDesc,
            CopyMorphDesc,
            CopySwapDesc,
            CopySetDesc,
            CopySetMetaDesc,
            ExtractAreaDesc,
            ExtractAreabandsDesc,
            ExtractBandDesc,
            ExtractBandsDesc,
            ExtractDesc,
            FalseColourDesc,
            FliphorDesc,
            FlipverDesc,
            GbandJoinDesc,
            GridDesc,
            InsertDesc,
            InsertSetDesc,
            InsertNoExpandDesc,
            EmbedDesc,
            LrJoinDesc,
            MsdDesc,
            MsdBandDesc,
            ReplicateDesc,
            Ri2CDesc,
            Rot180Desc,
            Rot270Desc,
            Rot90Desc,
            ScaleDesc,
            ScalePsDesc,
            SubsampleDesc,
            SystemDesc,
            SystemImageDesc,
            TbJoinDesc,
            TextDesc,
            WrapDesc,
            ZoomDesc
        };

        // im__conversion
        public static readonly Package ImConversion = new Package(
            "conversion",
            ConvList.Length,
            ConvList);
    }
}
```