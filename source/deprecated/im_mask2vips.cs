```csharp
// im_mask2vips
namespace VIPS {

    public class ImageMaskToVips {
        /// <summary>
        /// Write a one-band, double image to @out based on mask @in.
        /// </summary>
        /// <param name="in">input mask</param>
        /// <param name="out">output image</param>
        /// <returns>0 on success, -1 on error</returns>
        public static int im_mask2vips(DOUBLEMASK inMask, IMAGE outImage) {
            int x, y;
            double[] buf = new double[outImage.Xsize];
            double* p = inMask.coeff;
            double* q = buf;

            // Check the mask.
            if (inMask == null || inMask.coeff == null) {
                throw new ArgumentException("bad input mask");
            }

            // Make the output image.
            im_initdesc(outImage, inMask.xsize, inMask.ysize, 1,
                IM_BBITS_DOUBLE, IM_BANDFMT_DOUBLE,
                IM_CODING_NONE,
                IM_TYPE_B_W,
                1.0, 1.0,
                0, 0);
            if (im_setupout(outImage)) {
                return -1;
            }

            // Make an output buffer.
            if (buf == null) {
                return -1;
            }

            // Write!
            for (y = 0; y < outImage.Ysize; y++) {
                q = buf;

                for (x = 0; x < outImage.Xsize; x++)
                    *q++ = *p++;

                if (im_writeline(y, outImage, buf))
                    return -1;
            }

            vips_image_set_double(outImage, "scale", inMask.scale);
            vips_image_set_double(outImage, "offset", inMask.offset);

            return 0;
        }
    }

    public class ImageIntMaskToVips {
        /// <summary>
        /// Write a one-band, double image to @out based on mask @in.
        /// </summary>
        /// <param name="in">input mask</param>
        /// <param name="out">output image</param>
        /// <returns>0 on success, -1 on error</returns>
        public static int im_imask2vips(INTMASK inMask, IMAGE outImage) {
            DOUBLEMASK d;
            int result;

            if (!(d = im_imask2dmask(inMask, inMask.filename)))
                return -1;
            result = VIPS.ImageMaskToVips.im_mask2vips(d, outImage);
            im_free_dmask(d);

            return result;
        }
    }
}
```