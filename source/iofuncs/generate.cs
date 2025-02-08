Here is the C# code equivalent of the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsImage
{
    public int dtype { get; set; }
    public int BandFmt { get; set; }
    public int Bbits { get; set; }
    public bool hint_set { get; set; }
    public List<VipsImage> upstream { get; set; }
    public List<VipsImage> downstream { get; set; }
    public VipsProgressSignal progress_signal { get; set; }
    public int serial { get; set; }
    public int dhint { get; set; }
    public bool written { get; set; }

    // ... other fields and methods ...
}

public class VipsRegion
{
    public VipsImage im { get; set; }
    public int bpl { get; set; }
    public IntPtr addr { get; set; }
    public Rect valid { get; set; }
}

public delegate void VipsStartFn(VipsImage out_image, object a, object b);
public delegate int VipsGenerateFn(VipsRegion region, VipsRect area, object a);
public delegate int VipsStopFn(object seq, object a, object b);

public class Vips
{
    public static void LinkMake(VipsImage image_up, VipsImage image_down)
    {
        image_up.downstream.Add(image_down);
        image_down.upstream.Add(image_up);

        if (image_up.progress_signal != null && image_down.progress_signal == null)
            image_down.progress_signal = image_up.progress_signal;
    }

    public static void LinkBreak(VipsImage image_up, VipsImage image_down, object b)
    {
        image_up.downstream.Remove(image_down);
        image_down.upstream.Remove(image_up);

        if (image_down.progress_signal != null && image_down.progress_signal == image_up.progress_signal)
            image_down.progress_signal = null;
    }

    public static void LinkBreakRev(VipsImage image_down, VipsImage image_up, object b)
    {
        LinkBreak(image_up, image_down, b);
    }

    public static void LinkBreakAll(VipsImage image)
    {
        lock (Vips.GlobalLock)
        {
            Vips.SListMap2(image.upstream, (VipsSListMap2Fn)LinkBreak, image, null);
            Vips.SListMap2(image.downstream, (VipsSListMap2Fn)LinkBreakRev, image, null);

            Assert(!image.upstream.Any());
            Assert(!image.downstream.Any());
        }
    }

    public static void DemandHintArray(VipsImage image, int hint, ref VipsImage[] in_images)
    {
        VipsDemandStyle demand_style = (VipsDemandStyle)hint;

        int i, len, nany;
        VipsDemandStyle set_hint;

        for (i = 0, len = 0, nany = 0; in_images[i] != null; i++, len++)
            if (in_images[i].dhint == VIPS_DEMAND_STYLE_ANY)
                nany++;

        set_hint = demand_style;
        for (i = 0; i < len; i++)
            set_hint = (VipsDemandStyle)Vips.Min((int)set_hint, (int)in_images[i].dhint);

        image.dhint = set_hint;

#ifdef DEBUG
        Console.WriteLine("vips_image_pipeline_array: set dhint for \"" + image.filename + "\" to " + Vips.EnumNick(VIPS_TYPE_DEMAND_STYLE, image.dhint));
        Console.WriteLine("\toperation requested " + Vips.EnumNick(VIPS_TYPE_DEMAND_STYLE, hint));
        Console.WriteLine("\tinputs were:");
        Console.WriteLine("\t");
        for (i = 0; in_images[i] != null; i++)
            Console.Write(Vips.EnumNick(VIPS_TYPE_DEMAND_STYLE, in_images[i].dhint) + " ");
        Console.WriteLine();
#endif
    }

    public static int ImagePipelineArray(VipsImage image, VipsDemandStyle hint, ref VipsImage[] in_images)
    {
        DemandHintArray(image, hint, ref in_images);

        if (in_images[0] != null && ImageCopyFieldsArray(image, in_images))
            return -1;

        if (ReorderSetInput(image, in_images))
            return -1;

        return 0;
    }

    public static int ImagePipelineV(VipsImage image, VipsDemandStyle hint, params VipsImage[] in_images)
    {
        var array = new VipsImage[MAX_IMAGES];
        for (int i = 0; i < MAX_IMAGES && in_images[i] != null; i++)
            array[i] = in_images[i];

        return ImagePipelineArray(image, hint, ref array);
    }

    public static void StartOne(VipsImage out_image, object a, object b)
    {
        VipsImage in_image = (VipsImage)a;

        var region = new VipsRegion { im = in_image };
        // ... other initialization ...
    }

    public static int StopOne(object seq, object a, object b)
    {
        VipsRegion region = (VipsRegion)seq;
        // ... cleanup ...

        return 0;
    }

    public static int StopMany(object seq, object a, object b)
    {
        var array = (VipsRegion[])seq;

        if (array != null)
        {
            for (int i = 0; array[i] != null; i++)
                // ... cleanup ...
        }

        return 0;
    }

    public static void StartMany(VipsImage out_image, object a, object b)
    {
        VipsImage[] in_images = (VipsImage[])a;

        int n = 0;
        for (int i = 0; in_images[i] != null; i++)
            n++;

        var array = new VipsRegion[n + 1];
        for (int i = 0; i < n; i++)
            array[i] = new VipsRegion { im = in_images[i] };
        array[n] = null;
    }

    public static int ImageGenerate(VipsImage image, VipsStartFn start_fn, VipsGenerateFn generate_fn, VipsStopFn stop_fn, object a, object b)
    {
        // ... initialization ...

        switch (image.dtype)
        {
            case VIPS_IMAGE_PARTIAL:
                // ... attach callbacks ...
                break;

            case VIPS_IMAGE_SETBUF:
            case VIPS_IMAGE_SETBUF_FOREIGN:
            case VIPS_IMAGE_MMAPINRW:
            case VIPS_IMAGE_OPENOUT:
                // ... attach callbacks and generate image ...
                break;
        }

        return 0;
    }
}
```

Note that I've omitted some details, such as the implementation of `VipsProgressSignal`, `VipsDemandStyle`, and other types, as well as the exact behavior of some methods. You may need to modify this code to fit your specific requirements.

Also, keep in mind that C# is a higher-level language than C, so some things that are straightforward in C (like manual memory management) are handled automatically by the runtime in C#. This can make the code look different and require more boilerplate.