Here is the converted C# code:

```csharp
// base class for all resample operations
//
// properties:
// 	- one in, one out
// 	- not point-to-point
// 	- size can change in any way
// 	- bands, type, format etc. all fixed

using System;

namespace Vips
{
    public abstract class Resample : Operation
    {
        // SECTION: resample
        // @short_description: resample images in various ways
        // @stability: Stable
        // @include: vips/vips.h

        // These operations build on each other in a set of layers.
        //
        // First, vips_affine() applies an affine transform to an image. This is any
        // sort of 2D transform which preserves straight lines; so any combination of
        // stretch, sheer, rotate and translate. You supply an interpolator for it to
        // use to generate pixels, see vips_interpolate_new(). It will not produce
        // good results for very large shrinks: you'll see aliasing.
        //
        // vips_reduce() is like vips_affine(), but it can only shrink images, it can't
        // enlarge, rotate, or skew. It's very fast and uses an adaptive kernel for
        // interpolation.
        //
        // vips_shrink() is a fast block shrinker. It can quickly reduce images by
        // large integer factors. It will give poor results for small size reductions:
        // again, you'll see aliasing.
        //
        // Next, vips_resize() specialises in the common task of image reduce and
        // enlarge. It strings together combinations of vips_shrink(), vips_reduce(),
        // vips_affine() and others to implement a general, high-quality image
        // resizer.
        //
        // Finally, vips_thumbnail() combines load and resize in one operation, and adds
        // colour management and correct handling of alpha transparency. Because load
        // and resize happen together, it can exploit tricks like JPEG and TIFF
        // shrink-on-load, giving a (potentially) huge speedup. vips_thumbnail_image()
        // is only there for emergencies, don't use it unless you really have to.
        //
        // As a separate thing, `vips_mapim() can apply arbitrary 2D image transforms
        // to an image.

        public enum Size { Both, Up, Down, Force }

        protected Resample()
        {
        }
    }

    public abstract class VipsResample : Resample
    {
        private Image inImage;
        private Image outImage;

        public VipsResample(Image inImage)
        {
            this.inImage = inImage;
        }

        // Called from iofuncs to init all operations in this dir. Use a plugin system
        // instead?
        public static void OperationInit()
        {
            ThumbnailFile.GetType();
            ThumbnailBuffer.GetType();
            ThumbnailImage.GetType();
            ThumbnailSource.GetType();
            Mapim.GetType();
            Shrink.GetType();
            Shrinkh.GetType();
            Shrinkv.GetType();
            Reduce.GetType();
            Reduceth.GetType();
            Reducev.GetType();
            Quadratic.GetType();
            Affine.GetType();
            Similarity.GetType();
            Rotate.GetType();
            Resize.GetType();
        }

        protected override int Build()
        {
            // base class for all resample operations
            //
            // properties:
            // 	- one in, one out
            // 	- not point-to-point
            // 	- size can change in any way
            // 	- bands, type, format etc. all fixed

            outImage = new Image();
            if (base.Build())
                return -1;

            return 0;
        }

        protected override void ClassInit()
        {
            base.ClassInit();

            ArgImage argIn = new ArgImage("in", 1,
                _("Input"),
                _("Input image argument"),
                ArgumentRequired.Input,
                G_STRUCT_OFFSET(VipsResample, inImage));

            ArgImage argOut = new ArgImage("out", 2,
                _("Output"),
                _("Output image"),
                ArgumentRequired.Output,
                G_STRUCT_OFFSET(VipsResample, outImage));
        }

        protected override void Init()
        {
            base.Init();
        }
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g. `VipsOperation`, `ArgImage`, etc.) which are not defined in this code snippet. You will need to define these classes and methods separately.

Also, please note that C# does not have direct equivalents for some of the C constructs used in the original code (e.g. `G_DEFINE_ABSTRACT_TYPE`), so I've had to use equivalent C# constructs or leave out certain details.