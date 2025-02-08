Here is the C# code equivalent to the provided C code:

```csharp
using System;

namespace Vips
{
    public class GaussBlur : Operation
    {
        // vips_gaussblur_build()
        protected override int Build(VipsObject obj)
        {
            var gaussBlur = (GaussBlur)obj;
            var t = new Image[2];

            if (base.Build(obj) != 0)
                return -1;

            // vips_gaussmat() will make a 1x1 pixel mask for anything smaller than
            // this.
            if (gaussBlur.Sigma < 0.2)
            {
                if (Copy(gaussBlur.In, out t[1]))
                    return -1;
            }
            else
            {
                if (GaussMat(out t[0], gaussBlur.Sigma, gaussBlur.MinAmpl,
                            "separable", true, "precision", gaussBlur.Precision))
                    return -1;

#ifdef DEBUG
                Console.WriteLine("gaussblur: blurring with:");
                GaussMat.Print(t[0]);
#endif /*DEBUG*/

                g_info("gaussblur mask width {0}", t[0].Xsize);

                if (ConvSep(gaussBlur.In, out t[1], t[0],
                            "precision", gaussBlur.Precision))
                    return -1;
            }

            Object.SetProperty(obj, "out", new Image());

            if (Image.Write(t[1], gaussBlur.Out))
                return -1;

            return 0;
        }

        // vips_gaussblur_class_init()
        protected override void ClassInit(VipsObjectClass class_)
        {
            base.ClassInit(class_);

            ObjectClass.Nickname = "gaussblur";
            ObjectClass.Description = _("gaussian blur");
            ObjectClass.Build = Build;

            OperationClass.Flags = VIPS_OPERATION_SEQUENTIAL;

            // vips_gaussblur_build()
            VipsArg.Image("in", 1, _("Input"), _("Input image"),
                VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET(GaussBlur, In));

            VipsArg.Image("out", 2, _("Output"), _("Output image"),
                VIPS_ARGUMENT_REQUIRED_OUTPUT,
                G_STRUCT_OFFSET(GaussBlur, Out));

            VipsArg.Double("sigma", 3, _("Sigma"), _("Sigma of Gaussian"),
                VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET(GaussBlur, Sigma),
                0.0, 1000, 1.5);

            VipsArg.Double("min_ampl", 3, _("Minimum amplitude"), _("Minimum amplitude of Gaussian"),
                VIPS_ARGUMENT_OPTIONAL_INPUT,
                G_STRUCT_OFFSET(GaussBlur, MinAmpl),
                0.001, 1.0, 0.2);

            VipsArg.Enum("precision", 4, _("Precision"), _("Convolve with this precision"),
                VIPS_ARGUMENT_OPTIONAL_INPUT,
                G_STRUCT_OFFSET(GaussBlur, Precision),
                typeof(Precision), Precision.Integer);
        }

        // vips_gaussblur_init()
        protected override void Init(VipsObject obj)
        {
            base.Init(obj);

            Sigma = 1.5;
            MinAmpl = 0.2;
            Precision = Precision.Integer;
        }
    }

    public static class GaussBlurMethods
    {
        // vips_gaussblur()
        public static int GaussBlur(Image inImage, out Image[] outImages, double sigma, params object[] args)
        {
            var result = Vips.CallSplit("gaussblur", args, inImage, outImages, sigma);
            return result;
        }
    }

    // vips_gaussmat()
    public static class GaussMat
    {
        public static int Build(out Image image, double sigma, double minAmpl,
                                string separable, bool precision, string precisionName)
        {
            var t = new Image[1];

            if (GaussMat(image, out t[0], sigma, minAmpl))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(t[0]);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(out Image image, double sigma, double minAmpl)
        {
            var t = new Image[1];

            if (GaussMat(image, out t[0], sigma, minAmpl))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(t[0]);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(out Image image, double sigma)
        {
            var t = new Image[1];

            if (GaussMat(image, out t[0], sigma))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(t[0]);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(out Image image)
        {
            var t = new Image[1];

            if (GaussMat(image, out t[0]))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(t[0]);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(Image image, out Image image_, double sigma, double minAmpl,
                                string separable, bool precision, string precisionName)
        {
            if (GaussMat(image, out image_, sigma, minAmpl))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(image_);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", image_.Xsize);

            return 0;
        }

        public static int Build(Image image, out Image image_, double sigma, double minAmpl)
        {
            if (GaussMat(image, out image_, sigma, minAmpl))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(image_);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", image_.Xsize);

            return 0;
        }

        public static int Build(Image image, out Image image_, double sigma)
        {
            if (GaussMat(image, out image_, sigma))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(image_);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", image_.Xsize);

            return 0;
        }

        public static int Build(Image image, out Image image_)
        {
            if (GaussMat(image, out image_))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(image_);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", image_.Xsize);

            return 0;
        }

        public static int Build(Image image)
        {
            if (GaussMat(image))
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print(image);
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", image.Xsize);

            return 0;
        }

        public static int Build()
        {
            if (GaussMat())
                return -1;

#ifdef DEBUG
            Console.WriteLine("gaussmat: blurring with:");
            GaussMat.Print();
#endif /*DEBUG*/

            g_info("gaussmat mask width {0}", Xsize);

            return 0;
        }

        public static int GaussMat(Image image, out Image image_, double sigma, double minAmpl,
                                    string separable, bool precision, string precisionName)
        {
            // vips_gaussmat()
            var result = Vips.CallSplit("gaussmat", new object[] { sigma, minAmpl }, image, out image_);
            return result;
        }

        public static int GaussMat(Image image, out Image image_, double sigma, double minAmpl)
        {
            // vips_gaussmat()
            var result = Vips.CallSplit("gaussmat", new object[] { sigma, minAmpl }, image, out image_);
            return result;
        }

        public static int GaussMat(Image image, out Image image_, double sigma)
        {
            // vips_gaussmat()
            var result = Vips.CallSplit("gaussmat", new object[] { sigma }, image, out image_);
            return result;
        }

        public static int GaussMat(Image image, out Image image_)
        {
            // vips_gaussmat()
            var result = Vips.CallSplit("gaussmat", null, image, out image_);
            return result;
        }

        public static int GaussMat(Image image)
        {
            // vips_gaussmat()
            var result = Vips.CallSplit("gaussmat", null, image);
            return result;
        }
    }

    // vips_convsep()
    public static class ConvSep
    {
        public static int Build(Image inImage, out Image[] outImages, Image mask,
                                string precision, bool precisionName)
        {
            var t = new Image[1];

            if (ConvSep(inImage, out t[0], mask))
                return -1;

#ifdef DEBUG
            Console.WriteLine("convsep: blurring with:");
            ConvSep.Print(t[0]);
#endif /*DEBUG*/

            g_info("convsep mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(Image inImage, out Image[] outImages, Image mask)
        {
            var t = new Image[1];

            if (ConvSep(inImage, out t[0], mask))
                return -1;

#ifdef DEBUG
            Console.WriteLine("convsep: blurring with:");
            ConvSep.Print(t[0]);
#endif /*DEBUG*/

            g_info("convsep mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(Image inImage, out Image[] outImages)
        {
            var t = new Image[1];

            if (ConvSep(inImage, out t[0]))
                return -1;

#ifdef DEBUG
            Console.WriteLine("convsep: blurring with:");
            ConvSep.Print(t[0]);
#endif /*DEBUG*/

            g_info("convsep mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build(Image inImage)
        {
            var t = new Image[1];

            if (ConvSep(inImage, out t[0]))
                return -1;

#ifdef DEBUG
            Console.WriteLine("convsep: blurring with:");
            ConvSep.Print(t[0]);
#endif /*DEBUG*/

            g_info("convsep mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int Build()
        {
            var t = new Image[1];

            if (ConvSep(out t[0]))
                return -1;

#ifdef DEBUG
            Console.WriteLine("convsep: blurring with:");
            ConvSep.Print(t[0]);
#endif /*DEBUG*/

            g_info("convsep mask width {0}", t[0].Xsize);

            return 0;
        }

        public static int ConvSep(Image inImage, out Image image_, Image mask,
                                    string precision, bool precisionName)
        {
            // vips_convsep()
            var result = Vips.CallSplit("convsep", new object[] { precision }, inImage, out image_, mask);
            return result;
        }

        public static int ConvSep(Image inImage, out Image image_, Image mask)
        {
            // vips_convsep()
            var result = Vips.CallSplit("convsep", null, inImage, out image_, mask);
            return result;
        }

        public static int ConvSep(Image inImage, out Image image_)
        {
            // vips_convsep()
            var result = Vips.CallSplit("convsep", null, inImage, out image_);
            return result;
        }

        public static int ConvSep(Image inImage)
        {
            // vips_convsep()
            var result = Vips.CallSplit("convsep", null, inImage);
            return result;
        }
    }
}
```