Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsByteswap : VipsConversion
{
    public VipsImage inImage { get; private set; }

    public override int Gen(VipsRegion outRegion, object seq, object a, object b, bool stop)
    {
        var ir = (VipsRegion)seq;
        var im = ir.im;
        var r = outRegion.valid;

        if (vips_region_prepare(ir, r))
            return -1;

        for (int y = 0; y < r.height; y++)
        {
            var p = VIPS_REGION_ADDR(ir, r.left, r.top + y);
            var q = VIPS_REGION_ADDR(outRegion, r.left, r.top + y);

            if (VIPS_ALIGNED(p, vips_format_sizeof(im.BandFmt)) && VIPS_ALIGNED(q, vips_format_sizeof(im.BandFmt)))
                swap_aligned(p, q, r.width, im);
            else
                vips_byteswap_swap_unaligned(p, q, r.width * im.Bands, vips_format_sizeof(im.BandFmt));
        }

        return 0;
    }

    private static void SwapAligned(VipsPel[] inArray, VipsPel[] outArray, int width, VipsImage im)
    {
        var p = (guint16[])inArray;
        var q = (guint16[])outArray;
        var sz = (VIPS_IMAGE_SIZEOF_PEL(im) * width) / 2;

        for (int x = 0; x < sz; x++)
            q[x] = GUINT16_SWAP_LE_BE(p[x]);
    }

    private static void Swap4(VipsPel[] inArray, VipsPel[] outArray, int width, VipsImage im)
    {
        var p = (guint32[])inArray;
        var q = (guint32[])outArray;
        var sz = (VIPS_IMAGE_SIZEOF_PEL(im) * width) / 4;

        for (int x = 0; x < sz; x++)
            q[x] = GUINT32_SWAP_LE_BE(p[x]);
    }

    private static void Swap8(VipsPel[] inArray, VipsPel[] outArray, int width, VipsImage im)
    {
        var p = (guint64[])inArray;
        var q = (guint64[])outArray;
        var sz = (VIPS_IMAGE_SIZEOF_PEL(im) * width) / 8;

        for (int x = 0; x < sz; x++)
            q[x] = GUINT64_SWAP_LE_BE(p[x]);
    }

    private static void SwapUnaligned(VipsPel[] inArray, VipsPel[] outArray, int n, int size)
    {
        for (int x = 0; x < n; x++)
        {
            for (int i = 0; i < size; i++)
                outArray[i] = inArray[size - i - 1];

            Array.Copy(inArray, size * (x + 1), inArray, 0, size);
            Array.Copy(outArray, size * (x + 1), outArray, 0, size);
        }
    }

    public override int Build(VipsObject obj)
    {
        var conversion = VIPS_CONVERSION(obj);
        var byteswap = (VipsByteswap)obj;

        if (base.Build(obj))
            return -1;

        if (byteswap.inImage.Coding != VIPS_CODING_NONE || !vips_byteswap_swap_fn[byteswap.inImage.BandFmt])
            return vips_image_write(byteswap.inImage, conversion.out);

        if (vips_image_pio_input(byteswap.inImage))
            return -1;

        if (vips_image_pipelinev(conversion.out, VIPS_DEMAND_STYLE_THINSTRIP, byteswap.inImage, null))
            return -1;

        if (vips_image_generate(conversion.out, vips_start_one, Gen, vips_stop_one, byteswap.inImage, byteswap))
            return -1;

        return 0;
    }

    public static int Byteswap(VipsImage inImage, out VipsImage[] outImage)
    {
        var result = vips_call_split("byteswap", null, inImage, outImage);
        return result;
    }

    public static int _ByteswapBool(VipsImage inImage, out VipsImage[] outImage, bool swap)
    {
        if (swap)
            return Byteswap(inImage, outImage);
        else
            return vips_copy(inImage, outImage, null);
    }
}
```

Note that I've assumed the existence of certain classes and methods from the VIPS library, as they were not provided in the original code. Additionally, some types (e.g., `VipsPel`, `guint16`, etc.) are assumed to be defined elsewhere in the VIPS library.

Also note that this is a direct translation of the C code into C#, and may require modifications to work correctly with the .NET framework and the VIPS library.