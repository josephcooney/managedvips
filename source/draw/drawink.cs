```csharp
// vips_drawink_build
public int Build(VipsObject obj)
{
    VipsDrawinkClass class = (VipsDrawinkClass)VipsObject.GetClass(obj);
    VipsDraw draw = (VipsDraw)obj;
    VipsDrawink drawink = (VipsDrawink)obj;

#ifdef DEBUG
    Console.WriteLine("vips_drawink_build: " + obj.Name());
#endif /*DEBUG*/

    if (class.Build(obj) != 0)
        return -1;

    if (drawink.Ink != null &&
        !(drawink.PixelInk = VipsVectorToInk(class.Nickname,
            draw.Image,
            VIPS_ARRAY_ADDR(drawink.Ink, 0), null,
            VIPS_AREA(drawink.Ink).N)))
        return -1;

    return 0;
}

// vips_drawink_class_init
public class VipsDrawinkClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // gobject_class->set_property = vips_object_set_property;
        // gobject_class->get_property = vips_object_get_property;

        Nickname = "drawink";
        Description = _("draw with ink operations");
        Build = new VipsObject.Build(VipsDrawinkClass.Build);

        VIPS_ARG_BOXED("ink", 2,
            _("Ink"),
            _("Color for pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsDrawink, Ink),
            typeof(VipsArrayDouble));
    }
}

// vips_drawink_init
public class VipsDrawink : VipsObject
{
    public VipsDrawink()
    {
        Ink = new VipsArrayDouble(1);
    }

    // Fill a scanline between points x1 and x2 inclusive. x1 < x2.
    public int DrawScanline(int y, int x1, int x2)
    {
        VipsDraw draw = (VipsDraw)this;

        VipsPel* mp;
        int i;
        int len;

        g_assert(x1 <= x2);

        if (y < 0 ||
            y >= draw.Image.Ysize)
            return 0;
        if (x1 < 0 &&
            x2 < 0)
            return 0;
        if (x1 >= draw.Image.Xsize &&
            x2 >= draw.Image.Xsize)
            return 0;
        x1 = VIPS_CLIP(0, x1, draw.Image.Xsize - 1);
        x2 = VIPS_CLIP(0, x2, draw.Image.Xsize - 1);

        mp = (VipsPel*)VIPS_IMAGE_ADDR(draw.Image, x1, y);
        len = x2 - x1 + 1;

        for (i = 0; i < len; i++)
        {
            DrawPel(mp);
            mp += draw.PSize;
        }

        return 0;
    }
}
```