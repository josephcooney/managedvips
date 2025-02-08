```csharp
// im_raw2vips
public static int ImRaw2Vips(string filename, Image out, int width, int height, int bpp, int offset)
{
    VipsImage t;

    if (Vips.RawLoad(filename, out, width, height, bpp, "offset", offset))
        return -1;
    if (!Vips.ImageWrite(t, out))
    {
        g_object_unref(t);
        return -1;
    }
    g_object_unref(t);

    return 0;
}

// im_vips2raw
public static int ImVips2Raw(Image in, int fd)
{
    return Vips.RawSaveFd(in, fd, null);
}
```