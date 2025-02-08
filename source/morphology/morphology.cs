Here is the converted C# code:

```csharp
// base class for all morphological operations
//
// properties:
// 	- one input image

using System;

namespace Vips
{
    public abstract class Morphology : Operation
    {
        // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
        public static void MorphologyOperationInit()
        {
            Type vipsMorphType = typeof(Morph);
            Type vipsRankType = typeof(Rank);
            Type vipsCountlinesType = typeof(Countlines);
            Type vipsLabelregionsType = typeof(Labelregions);
            Type vipsFillNearestType = typeof(FillNearest);

            // Note: This is equivalent to the C code, but in C# we don't need to call GetMethod() or Invoke()
        }

        protected Morphology()
        {
        }
    }

    public class Morph : Morphology
    {
        // SECTION: morphology
        // @short_description: morphological operators, rank filters and related image analysis
        // @see_also: <link linkend="libvips-arithmetic">arithmetic</link>
        // @stability: Stable
        // @include: vips/vips.h

        // The morphological functions search images for particular patterns of pixels,
        // specified with the mask argument, either adding or removing pixels when they find a match.
        // They are useful for cleaning up images --- for example, you might threshold an image,
        // and then use one of the morphological functions to remove all single isolated pixels from the result.

        public Morph()
            : base()
        {
        }

        // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
        protected override void Init()
        {
            base.Init();
        }
    }

    public class Rank : Morphology
    {
        // SECTION: morphology
        // @short_description: morphological operators, rank filters and related image analysis
        // @see_also: <link linkend="libvips-arithmetic">arithmetic</link>
        // @stability: Stable
        // @include: vips/vips.h

        // The morphological functions search images for particular patterns of pixels,
        // specified with the mask argument, either adding or removing pixels when they find a match.
        // They are useful for cleaning up images --- for example, you might threshold an image,
        // and then use one of the morphological functions to remove all single isolated pixels from the result.

        public Rank()
            : base()
        {
        }

        // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
        protected override void Init()
        {
            base.Init();
        }
    }

    public class Countlines : Morphology
    {
        // SECTION: morphology
        // @short_description: morphological operators, rank filters and related image analysis
        // @see_also: <link linkend="libvips-arithmetic">arithmetic</link>
        // @stability: Stable
        // @include: vips/vips.h

        // The morphological functions search images for particular patterns of pixels,
        // specified with the mask argument, either adding or removing pixels when they find a match.
        // They are useful for cleaning up images --- for example, you might threshold an image,
        // and then use one of the morphological functions to remove all single isolated pixels from the result.

        public Countlines()
            : base()
        {
        }

        // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
        protected override void Init()
        {
            base.Init();
        }
    }

    public class Labelregions : Morphology
    {
        // SECTION: morphology
        // @short_description: morphological operators, rank filters and related image analysis
        // @see_also: <link linkend="libvips-arithmetic">arithmetic</link>
        // @stability: Stable
        // @include: vips/vips.h

        // The morphological functions search images for particular patterns of pixels,
        // specified with the mask argument, either adding or removing pixels when they find a match.
        // They are useful for cleaning up images --- for example, you might threshold an image,
        // and then use one of the morphological functions to remove all single isolated pixels from the result.

        public Labelregions()
            : base()
        {
        }

        // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
        protected override void Init()
        {
            base.Init();
        }
    }

    public class FillNearest : Morphology
    {
        // SECTION: morphology
        // @short_description: morphological operators, rank filters and related image analysis
        // @see_also: <link linkend="libvips-arithmetic">arithmetic</link>
        // @stability: Stable
        // @include: vips/vips.h

        // The morphological functions search images for particular patterns of pixels,
        // specified with the mask argument, either adding or removing pixels when they find a match.
        // They are useful for cleaning up images --- for example, you might threshold an image,
        // and then use one of the morphological functions to remove all single isolated pixels from the result.

        public FillNearest()
            : base()
        {
        }

        // Called from iofuncs to init all operations in this dir. Use a plugin system instead?
        protected override void Init()
        {
            base.Init();
        }
    }
}
```