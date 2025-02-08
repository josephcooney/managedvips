Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class ArrayJoin : Conversion
{
    public static int Gen(VipsRegion region, object seq, object a, object b, ref bool stop)
    {
        // Find the left/top/width/height of the cells this region touches.
        var inArray = (VipsImage[])a;
        var join = (ArrayJoin)b;
        var conversion = VipsConversion.Get(join);
        var r = region.Valid;

        int cellWidth = join.HSpacing + join.Shim;
        int cellHeight = join.VSpacing + join.Shim;
        int left = r.Left / cellWidth;
        int top = r.Top / cellHeight;
        int width = (VIPS_ROUND_UP(VIPS_RECT_RIGHT(r), cellWidth) - VIPS_ROUND_DOWN(r.Left, cellWidth)) / cellWidth;
        int height = (VIPS_ROUND_UP(VIPS_RECT_BOTTOM(r), cellHeight) - VIPS_ROUND_DOWN(r.Top, cellHeight)) / cellHeight;

        int i;
        var reg;

        // Size of image array.
        inArray.Length;

        // Does this rect fit completely within one of our inputs? We can just forward the request.
        if (width == 1 && height == 1)
        {
            VipsRect need;

            i = Math.Min(inArray.Length - 1, left + top * join.Across);

            // The part of in[i] we need.
            need = region.Valid;
            need.Left -= join.Rects[i].Left;
            need.Top -= join.Rects[i].Top;

            // And render into out_region. We can't just forward a pointer since we are about to unref reg.
            reg = VipsRegion.New(inArray[i]);
            if (VipsRegion.PrepareTo(reg, region, ref need, r.Left, r.Top))
            {
                Object.Unref(reg);
                return -1;
            }
            Object.Unref(reg);
        }
        else
        {
            // Output requires more than one input. Paste all touching inputs into the output.
            int x, y;

            for (y = 0; y < height; y++)
                for (x = 0; x < width; x++)
                {
                    i = Math.Min(inArray.Length - 1, x + left + (y + top) * join.Across);

                    reg = VipsRegion.New(inArray[i]);

                    if (Vips__InsertPasteRegion(region, reg, ref join.Rects[i]))
                    {
                        Object.Unref(reg);
                        return -1;
                    }

                    Object.Unref(reg);
                }
        }

        // In sequential mode, we can minimise an input once our generate point is well past the end of it.
        if (VipsImage.IsSequential(conversion.Out))
            for (i = 0; i < inArray.Length; i++)
            {
                int bottomEdge = VIPS_RECT_BOTTOM(ref join.Rects[i]);

                if (!join.Minimised[i] && r.Top > bottomEdge + 1024)
                {
                    join.Minimised[i] = true;
                    VipsImage.MinimiseAll(inArray[i]);
                }
            }

        return 0;
    }

    public static int Build(VipsObject obj)
    {
        var class = (VipsObjectClass)VipsObject.GetClass(obj);
        var conversion = (VipsConversion)VipsConversion.Get(obj);
        var join = (ArrayJoin)obj;

        VipsImage[] inArray;
        int n;

        if (VipsObject.ClassBuild(obj))
            return -1;

        inArray = VipsArrayImage.Get(join.In, out n);

        // Array length zero means error.
        if (n == 0)
            return -1;

        for (int i = 0; i < n; i++)
            if (VipsImage.PioInput(inArray[i]) || VipsCheckCodingKnown(class.Nickname, inArray[i]))
                return -1;

        // Move all input images to a common format and number of bands.
        var format = new VipsImage[inArray.Length];
        if (!Vips__FormatAlikeVec(inArray, format))
            return -1;
        inArray = format;

        // We have to include the number of bands in @background in our calculation.
        var band = new VipsImage[inArray.Length];
        if (!Vips__BandAlikeVec(class.Nickname, inArray, band, n, join.Background))
            return -1;
        inArray = band;

        // Now sizealike: search for the largest image.
        int hSpacing = inArray[0].Xsize;
        int vSpacing = inArray[0].Ysize;
        for (int i = 1; i < n; i++)
        {
            if (inArray[i].Xsize > hSpacing)
                hSpacing = inArray[i].Xsize;
            if (inArray[i].Ysize > vSpacing)
                vSpacing = inArray[i].Ysize;
        }

        if (!VipsObject.ArgumentIsSet(obj, "hspacing"))
            join.HSpacing = hSpacing;
        if (!VipsObject.ArgumentIsSet(obj, "vspacing"))
            join.VSpacing = vSpacing;

        hSpacing = join.HSpacing;
        vSpacing = join.VSpacing;

        if (!VipsObject.ArgumentIsSet(obj, "across"))
            join.Across = n;

        // How many images down the grid?
        join.Down = (int)Math.Ceiling((double)n / join.Across);

        // The output size.
        int outputWidth = hSpacing * join.Across + join.Shim * (join.Across - 1);
        int outputHeight = vSpacing * join.Down + join.Shim * (join.Down - 1);

        // Make a rect for the position of each input.
        var rects = new VipsRect[n];
        for (int i = 0; i < n; i++)
        {
            int x = i % join.Across;
            int y = i / join.Across;

            rects[i].Left = x * (hSpacing + join.Shim);
            rects[i].Top = y * (vSpacing + join.Shim);
            rects[i].Width = hSpacing;
            rects[i].Height = vSpacing;

            // In the centre of the array, we make width / height larger by shim.
            if (x != join.Across - 1)
                rects[i].Width += join.Shim;
            if (y != join.Down - 1)
                rects[i].Height += join.Shim;

            // The right edge of the final image is stretched to the right to fill the whole row.
            if (i == n - 1)
                rects[i].Width = outputWidth - rects[i].Left;
        }

        // A thing to track which inputs we've signalled minimise on.
        var minimised = new bool[n];
        for (int i = 0; i < n; i++)
            minimised[i] = false;

        // Each image must be cropped and aligned within an @hspacing by @vspacing box.
        var size = new VipsImage[inArray.Length];
        for (int i = 0; i < n; i++)
        {
            int left, top;
            int width, height;

            // Compiler warnings.
            left = 0;
            top = 0;

            switch (join.HAlign)
            {
                case VIPS_ALIGN_LOW:
                    left = 0;
                    break;

                case VIPS_ALIGN_CENTRE:
                    left = (hSpacing - inArray[i].Xsize) / 2;
                    break;

                case VIPS_ALIGN_HIGH:
                    left = hSpacing - inArray[i].Xsize;
                    break;

                default:
                    throw new Exception("Invalid horizontal alignment");
            }

            switch (join.VAlign)
            {
                case VIPS_ALIGN_LOW:
                    top = 0;
                    break;

                case VIPS_ALIGN_CENTRE:
                    top = (vSpacing - inArray[i].Ysize) / 2;
                    break;

                case VIPS_ALIGN_HIGH:
                    top = vSpacing - inArray[i].Ysize;
                    break;

                default:
                    throw new Exception("Invalid vertical alignment");
            }

            width = rects[i].Width;
            height = rects[i].Height;

            if (!VipsEmbed(inArray[i], ref size[i], left, top, width, height, "extend", VIPS_EXTEND_BACKGROUND, "background", join.Background))
                return -1;
        }

        if (VipsImagePipelineArray(conversion.Out, VIPS_DEMAND_STYLE_THINSTRIP, size))
            return -1;

        conversion.Out.Xsize = outputWidth;
        conversion.Out.Ysize = outputHeight;

        // Don't use start_many -- the set of input images can be huge (many 10s of 1000s) and we don't want to have 20,000 regions active.
        if (!VipsImage.Generate(conversion.Out, null, Gen))
            return -1;

        return 0;
    }

    public static void ClassInit(VipsArrayjoinClass class_)
    {
        var gobjectClass = (GObjectClass)class_;
        var vobjectClass = (VipsObjectClass)VipsObject.GetClass(class_);
        var operationClass = (VipsOperationClass)VipsOperation.GetClass(class_);

        VIPS_DEBUG_MSG("vips_arrayjoin_class_init\n");

        gobjectClass.SetProperty += VipsObject.SetProperty;
        gobjectClass.GetProperty += VipsObject.GetProperty;

        vobjectClass.Nickname = "arrayjoin";
        vobjectClass.Description = "Join an array of images.";
        vobjectClass.Build = Build;

        operationClass.Flags = VIPS_OPERATION_SEQUENTIAL;

        VipsArg.Boxed("in", -1, "Input", "Array of input images", VIPS_ARGUMENT_REQUIRED_INPUT);
        VipsArg.Int("across", 4, "Across", "Number of images per row", VIPS_ARGUMENT_OPTIONAL_INPUT);
        VipsArg.Int("shim", 5, "Shim", "Space between images, in pixels", VIPS_ARGUMENT_OPTIONAL_INPUT);
        VipsArg.Boxed("background", 6, "Background", "Colour for new pixels", VIPS_ARGUMENT_OPTIONAL_INPUT);
        VipsArg.Enum("halign", 7, "Horizontal align", "Align on the left, centre or right", VIPS_ARGUMENT_OPTIONAL_INPUT);
        VipsArg.Enum("valign", 8, "Vertical align", "Align on the top, centre or bottom", VIPS_ARGUMENT_OPTIONAL_INPUT);
        VipsArg.Int("hspacing", 9, "Horizontal spacing", "Horizontal spacing between images", VIPS_ARGUMENT_OPTIONAL_INPUT);
        VipsArg.Int("vspacing", 10, "Vertical spacing", "Vertical spacing between images", VIPS_ARGUMENT_OPTIONAL_INPUT);
    }

    public static void Init(ArrayJoin join)
    {
        // Init our instance fields.
        join.Background = VipsArea.NewArray(GType.Double, sizeof(double), 1);
        ((double[])join.Background.Data)[0] = 0.0;
    }
}

public class ArrayJoinClass : ConversionClass
{
    public static void ClassInit(VipsArrayjoinClass class_)
    {
        // ...
    }

    public static void Init(ArrayJoin join)
    {
        // ...
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available and contains all necessary classes and methods. You may need to adjust this code based on your actual VIPS library version and usage.

Also, please note that some parts of the original C code have been omitted or simplified for brevity, such as error handling and logging. You should review the original code carefully to ensure that all necessary functionality is preserved in the converted C# code.