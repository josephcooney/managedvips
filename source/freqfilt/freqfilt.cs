Here is the C# code equivalent to the provided C code:

```csharp
using System;

namespace Vips
{
    public abstract class VipsFreqfilt : VipsOperation
    {
        // base class for all Fourier stuff
        //
        // properties:
        // 	- single output image

        protected VipsImage inImage;
        protected VipsImage outImage;

        public VipsFreqfilt()
        {
            // Transform an n-band image with a 1-band processing function.
            //
            // Memory strategy: we need memory buffers for the input and the output of
            // fftw. In some modes fftw generates only half the output and we construct
            // the rest.

            inImage = new VipsImage();
            outImage = new VipsImage();
        }

        public override int Build(VipsObject context)
        {
            // vips_freqfilt_build: 
            //   <object-name>

            outImage = new VipsImage();

            if (base.Build(context) != 0)
                return -1;

            return 0;
        }

        protected class VipsFreqfiltClass : VipsOperationClass
        {
            public override void ClassInit(VipsObjectClass klass)
            {
                base.ClassInit(klass);

                // vobject_class->nickname = "freqfilt";
                // vobject_class->description = _("frequency-domain filter operations");
                // vobject_class->build = vips_freqfilt_build;

                VIPS_ARG_IMAGE(klass, "in", -1,
                    _("Input"),
                    _("Input image"),
                    VIPS_ARGUMENT_REQUIRED_INPUT);

                VIPS_ARG_IMAGE(klass, "out", 1,
                    _("Output"),
                    _("Output image"),
                    VIPS_ARGUMENT_REQUIRED_OUTPUT);
            }
        }

        protected class VipsFreqfiltInit : VipsOperationInit
        {
            public override void Init()
            {
                // vips_freqfilt_init:
            }
        }

        public static int Vips__Fftproc(VipsObject context, VipsImage inImage, ref VipsImage outImage, VipsFftProcessFn fn)
        {
            if (inImage.Bands == 1)
                return fn(context, inImage, ref outImage);

            VipsImage[] bands = new VipsImage[inImage.Bands];
            VipsImage[] fft = new VipsImage[inImage.Bands];

            for (int b = 0; b < inImage.Bands; b++)
            {
                if (!vips_extract_band(inImage, ref bands[b], b))
                    return -1;

                if (fn(context, bands[b], ref fft[b]))
                    return -1;
            }

            if (!vips_bandjoin(fft, outImage, inImage.Bands))
                return -1;

            return 0;
        }
    }

    public class VipsFreqfiltOperationInit : VipsOperationInit
    {
        public override void Init()
        {
            // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
#ifdef HAVE_FFTW
            GType vips_fwfft_get_type();
            GType vips_invfft_get_type();
#endif /*HAVE_FFTW*/
            GType vips_freqmult_get_type();
            GType vips_spectrum_get_type();
            GType vips_phasecor_get_type();

#ifdef HAVE_FFTW
            vips_fwfft_get_type();
            vips_invfft_get_type();
#endif /*HAVE_FFTW*/
            vips_freqmult_get_type();
            vips_spectrum_get_type();
            vips_phasecor_get_type();
        }
    }
}
```

Note that this code assumes the existence of `VipsOperation`, `VipsImage`, and other classes/types, which are not defined in the provided C code. These would need to be implemented separately or imported from another namespace.

Also note that some parts of the original C code have been omitted or simplified for brevity, such as the comments and debug statements.