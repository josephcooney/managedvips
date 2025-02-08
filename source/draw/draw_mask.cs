Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class VipsDrawMask : VipsDrawink
{
    public VipsImage Mask { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    protected override int Build(VipsObject obj)
    {
        var draw = (VipsDraw)obj;
        var drawink = (VipsDrawink)obj;
        var mask = (VipsDrawMask)obj;

        if (base.Build(obj) != 0)
            return -1;

        if (Vips__draw_mask_direct(draw.Image, mask.Mask, drawink.PixelInk, mask.X, mask.Y))
            return -1;

        return 0;
    }

    public static int DrawMask(VipsImage image, VipsImage mask, double[] ink, int x, int y)
    {
        var area_ink = new VipsArea(new double[] { ink });
        var result = Vips.CallSplit("draw_mask", image, area_ink, mask, x, y);
        area_ink.Dispose();
        return result;
    }

    public static int DrawMask1(VipsImage image, double ink, VipsImage mask, int x, int y)
    {
        var array_ink = new double[] { ink };
        var area_ink = new VipsArea(array_ink);
        var result = Vips.CallSplit("draw_mask", image, area_ink, mask, x, y);
        area_ink.Dispose();
        return result;
    }

    public static int DrawMaskLabq(VipsImage image, VipsImage mask, double[] ink, int x, int y)
    {
        var lab_buffer = new float[image.Width * 3];
        for (int y2 = 0; y2 < image.Height; y2++)
        {
            var to = Vips.ImageAddr(image, x, y + y2);
            var m = Vips.ImageAddr(mask, x, y + y2);

            Vips.LabQ2LabVec(lab_buffer, to, image.Width);
            for (int i = 0; i < lab_buffer.Length; i++)
                lab_buffer[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
            Vips.Lab2LabQVec(to, lab_buffer, image.Width);
        }
        return 0;
    }

    public static int DrawMaskDirect(VipsImage image, VipsImage mask, double[] ink, int x, int y)
    {
        var image_rect = new VipsRect();
        var area_rect = new VipsRect();
        var image_clip = new VipsRect();
        var mask_clip = new VipsRect();

        if (Vips.CheckCodingNoneOrLabq("draw_mask_direct", image) ||
            Vips.ImageInplace(image) ||
            Vips.ImageWioInput(mask) ||
            Vips.CheckMono("draw_mask_direct", mask) ||
            Vips.CheckUncoded("draw_mask_direct", mask) ||
            Vips.CheckFormat("draw_mask_direct", mask, Vips.Format.UChar))
            return -1;

        area_rect.Left = x;
        area_rect.Top = y;
        area_rect.Width = mask.Xsize;
        area_rect.Height = mask.Ysize;
        image_rect.Left = 0;
        image_rect.Top = 0;
        image_rect.Width = image.Xsize;
        image_rect.Height = image.Ysize;
        Vips.RectIntersectRect(&area_rect, &image_rect, ref image_clip);

        mask_clip = image_clip;
        mask_clip.Left -= x;
        mask_clip.Top -= y;

        if (!Vips.RectIsEmpty(&image_clip))
            switch (image.Coding)
            {
                case Vips.Coding.Labq:
                    return DrawMaskLabq(image, mask, ink, x, y);

                case Vips.Coding.None:
                    for (int y2 = 0; y2 < image.Height; y2++)
                    {
                        var to = Vips.ImageAddr(image, x, y + y2);
                        var m = Vips.ImageAddr(mask, x, y + y2);

                        switch (image.BandFmt)
                        {
                            case Vips.Format.UChar:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.Char:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.UShort:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.Short:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.UInt:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.Int:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.Float:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.Double:
                                for (int i = 0; i < image.Width; i++)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.Complex:
                                for (int i = 0; i < image.Width; i += 2)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                to[i + 1] = ((double)ink[(i + 1) / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            case Vips.Format.DpComplex:
                                for (int i = 0; i < image.Width; i += 2)
                                    to[i] = ((double)ink[i / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                to[i + 1] = ((double)ink[(i + 1) / 3] * m[i % image.Width] + (255 - m[i % image.Width])) / 255;
                                break;

                            default:
                                throw new Exception("Invalid band format");
                        }
                    }
                    return 0;

                default:
                    throw new Exception("Invalid coding");
            }

        return 0;
    }
}
```

Note that this code uses the VipsDotNet library, which is a .NET wrapper for the VIPS image processing library. The `VipsDrawMask` class inherits from `VipsDrawink` and provides methods for drawing a mask on an image. The `DrawMaskDirect` method is equivalent to the C function `vips__draw_mask_direct`, but it uses the VipsDotNet API instead of direct calls to the VIPS library.