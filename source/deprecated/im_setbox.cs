```csharp
// im_setbox (from vips/im_box.c)

public static void ImSetBox(IMAGE_BOX pbox, int xst, int yst, int xsiz, int ysiz, int ch_select)
{
    // Set the coordinates of a box to an IMAGE_BOX.
    pbox.XStart = xst;
    pbox.YStart = yst;
    pbox.XSize = xsiz;
    pbox.YSize = ysiz;
    pbox.ChSelect = ch_select;
}
```