Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class DrawImage : VipsDraw
{
    public DrawImage(VipsImage image) : base(image)
    {
        Mode = CombineMode.Set;
    }

    public override int Build()
    {
        // Check if sub-image has the same coding as main image
        if (CheckCodingKnown(Nickname, Image) ||
            CheckCodingSame(Nickname, Image, Sub) ||
            CheckBands1OrUnary(Nickname, Sub, Image.Bands))
            return -1;

        // SET mode works for any matching coding, but every other mode needs uncoded images
        if (Mode != CombineMode.Set && CheckUncoded(Nickname, Image))
            return -1;

        // Cast sub-image to match main image in bands and format
        if (Sub.Coding == VipsCoding.None)
        {
            if (!BandUp(Nickname, Sub, out var temp, Image.Bands) ||
                !Cast(temp, out var casted, Image.BandFmt))
                return -1;

            Sub = casted;
        }

        // Make rects for main and sub images
        var imageRect = new VipsRect(0, 0, Image.Xsize, Image.Ysize);
        var subRect = new VipsRect(X, Y, Sub.Xsize, Sub.Ysize);
        var clipRect = new VipsRect();

        if (!VipsRect.IntersectRect(imageRect, subRect, out clipRect))
            return -1;

        // Paint sub-image onto main image
        if (clipRect.Width > 0 && clipRect.Height > 0)
        {
            var p = VipsImage.Addr(Sub, clipRect.Left - X, clipRect.Top - Y);
            var q = VipsImage.Addr(Image, clipRect.Left, clipRect.Top);

            switch (Mode)
            {
                case CombineMode.Set:
                    VipsMemcpy(q, p, clipRect.Width * VipsImage.SizeOfPel(Sub));
                    break;

                case CombineMode.Add:
                    DrawImageModeAdd(clipRect.Width);
                    break;

                default:
                    throw new ArgumentException("Invalid mode");
            }
        }

        return 0;
    }

    private void DrawImageModeAdd(int n)
    {
        // Complex just doubles the size
        var sz = n * Sub.Bands *
            (VipsBandFormat.IsComplex(Sub.BandFmt) ? 2 : 1);

        switch (Sub.BandFmt)
        {
            case VipsBandFormat.UChar:
                LoopUChar(sz);
                break;

            case VipsBandFormat.Char:
                LoopSChar(sz);
                break;

            case VipsBandFormat.UShort:
                LoopUShort(sz);
                break;

            case VipsBandFormat.SShort:
                LoopSShort(sz);
                break;

            case VipsBandFormat.UInt:
                LoopUInt(sz);
                break;

            case VipsBandFormat.Int:
                LoopSInt(sz);
                break;

            case VipsBandFormat.Float:
            case VipsBandFormat.Complex:
                LoopF(sz);
                break;

            case VipsBandFormat.Double:
            case VipsBandFormat.DComplex:
                LoopD(sz);
                break;

            default:
                throw new ArgumentException("Invalid band format");
        }
    }

    private void LoopUChar(int sz)
    {
        var pt = (ushort*)VipsMemalign(sizeof(ushort), sz);
        var qt = (ushort*)VipsMemalign(sizeof(ushort), sz);

        for (var x = 0; x < sz; x++)
        {
            var v = (pt[x] + qt[x]);

            qt[x] = VipsClip(0, v, UChar.MaxValue);
        }

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopSChar(int sz)
    {
        var pt = (sbyte*)VipsMemalign(sizeof(sbyte), sz);
        var qt = (sbyte*)VipsMemalign(sizeof(sbyte), sz);

        for (var x = 0; x < sz; x++)
        {
            var v = (pt[x] + qt[x]);

            qt[x] = VipsClip(SChar.MinValue, v, SChar.MaxValue);
        }

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopUShort(int sz)
    {
        var pt = (ushort*)VipsMemalign(sizeof(ushort), sz);
        var qt = (ushort*)VipsMemalign(sizeof(ushort), sz);

        for (var x = 0; x < sz; x++)
        {
            var v = (pt[x] + qt[x]);

            qt[x] = VipsClip(0, v, UShort.MaxValue);
        }

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopSShort(int sz)
    {
        var pt = (sbyte*)VipsMemalign(sizeof(sbyte), sz);
        var qt = (sbyte*)VipsMemalign(sizeof(sbyte), sz);

        for (var x = 0; x < sz; x++)
        {
            var v = (pt[x] + qt[x]);

            qt[x] = VipsClip(SChar.MinValue, v, SChar.MaxValue);
        }

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopUInt(int sz)
    {
        var pt = (uint*)VipsMemalign(sizeof(uint), sz);
        var qt = (uint*)VipsMemalign(sizeof(uint), sz);

        for (var x = 0; x < sz; x++)
        {
            var v = (pt[x] + qt[x]);

            qt[x] = VipsClip(0, v, UInt.MaxValue);
        }

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopSInt(int sz)
    {
        var pt = (sbyte*)VipsMemalign(sizeof(sbyte), sz);
        var qt = (sbyte*)VipsMemalign(sizeof(sbyte), sz);

        for (var x = 0; x < sz; x++)
        {
            var v = (pt[x] + qt[x]);

            qt[x] = VipsClip(SChar.MinValue, v, SChar.MaxValue);
        }

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopF(int sz)
    {
        var pt = (float*)VipsMemalign(sizeof(float), sz);
        var qt = (float*)VipsMemalign(sizeof(float), sz);

        for (var x = 0; x < sz; x++)
            qt[x] += pt[x];

        VipsFree(pt);
        VipsFree(qt);
    }

    private void LoopD(int sz)
    {
        var pt = (double*)VipsMemalign(sizeof(double), sz);
        var qt = (double*)VipsMemalign(sizeof(double), sz);

        for (var x = 0; x < sz; x++)
            qt[x] += pt[x];

        VipsFree(pt);
        VipsFree(qt);
    }

    public int X { get; set; }
    public int Y { get; set; }
    public CombineMode Mode { get; set; }
}

public class DrawImageClass : VipsDrawClass
{
    public static void ClassInit()
    {
        var gobjectClass = (GObjectClass)typeof(DrawImage).GetTypeInfo().Assembly.GetType("Vips.GObject");
        var vobjectClass = (VipsObjectClass)typeof(VipsObject).GetTypeInfo().Assembly.GetType("Vips.VipsObject");

        gobjectClass.SetProperty += VipsObject.SetProperty;
        gobjectClass.GetProperty += VipsObject.GetProperty;

        vobjectClass.Nickname = "draw_image";
        vobjectClass.Description = "paint an image into another image";

        var args = new[]
        {
            new VipsArg("sub", 5, "Sub-image", "Sub-image to insert into main image"),
            new VipsArg("x", 6, "x", "Draw image here"),
            new VipsArg("y", 7, "y", "Draw image here"),
            new VipsArg("mode", 8, "Mode", "Combining mode")
        };

        foreach (var arg in args)
        {
            vobjectClass.AddArgument(arg);
        }
    }

    public DrawImageClass()
    {
        Init();
    }

    private void Init()
    {
        // Initialize properties
        X = 0;
        Y = 0;
        Mode = CombineMode.Set;
    }
}

public enum CombineMode
{
    Set,
    Add
}
```

Note that I've assumed the `Vips` namespace is already defined elsewhere in your codebase, and that you have the necessary dependencies installed. Also, this code uses the `VipsDotNet` library to interact with the VIPS image processing library.

Please note that this is a direct translation of the C code to C#, without any optimizations or improvements specific to the .NET platform. You may want to review and refine the code to better suit your needs.