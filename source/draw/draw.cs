```csharp
// vips_draw_build()

public class VipsDraw : VipsOperation
{
    public override int Build()
    {
        // Check if the parent class build() method returns an error
        if (base.Build() != 0)
            return -1;

        // Check if the image type is known and if it's in-place
        if (!VipsObject.CheckCodingKnown(Nickname, Image) || VipsImage.IsInplace(Image))
            return -1;

        // Initialize line size and pixel size
        Lsize = VipsImage.SizeOfLine(Image);
        Psize = VipsImage.SizeOfPel(Image);

        // Set noclip to false
        NoClip = false;

        return 0;
    }
}

// vips_draw_class_init()

public class VipsDrawClass : VipsOperationClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Set the nickname and description of the object
        Nickname = "draw";
        Description = _("draw operations");

        // Initialize the build method
        Build = new Func<VipsObject, int>(VipsDraw.Build);

        // Define the image property
        VipsArgImage(imageProperty, "image", 1,
            _("Image"),
            _("Image to draw on"),
            VipsArgument.RequiredInput | VipsArgument.Modify);
    }
}

// vips_draw_init()

public class VipsDraw : VipsOperation
{
    public VipsDraw()
    {
        // Initialize the object
    }
}

// vips_draw_operation_init()

public static void DrawOperationInit()
{
    // Register the draw operations
    typeof(VipsDrawRect).GetType();
    typeof(VipsDrawImage).GetType();
    typeof(VipsDrawMask).GetType();
    typeof(VipsDrawLine).GetType();
    typeof(VipsDrawCircle).GetType();
    typeof(VipsDrawFlood).GetType();
    typeof(VipsDrawSmudge).GetType();
}
```