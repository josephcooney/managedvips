```csharp
// im_vips2jpeg
public static int ImVips2Jpeg(VipsImage in, string filename)
{
    int qfac = 75;

    // profile has to default to null, meaning "no param". If we default
    // to "none" we will not attach the profile from the metadata.
    string profile = null;

    char[] name = new char[FILENAME_MAX];
    char[] mode = new char[FILENAME_MAX];
    char[] buf = new char[FILENAME_MAX];

    // Parse mode from filename.
    ImFilenameSplit(filename, name, mode);
    Array.Copy(mode, 0, buf, 0, mode.Length);
    int p = 0;
    string q = null;

    if ((q = ImGetNextOption(ref p)) != null)
    {
        if (string.Compare(q, "") != 0)
            qfac = int.Parse(buf);
    }
    if ((q = ImGetNextOption(ref p)) != null)
    {
        if (string.Compare(q, "") != 0)
            profile = q;
    }
    if ((q = ImGetNextOption(ref p)) != null)
    {
        throw new ArgumentException($"im_vips2jpeg: unknown extra options \"{q}\"");
    }

    return VipsJpegsave(in, name,
        "Q", qfac, "profile", profile, null);
}

// im_vips2bufjpeg
public static int ImVips2BufJpeg(VipsImage in, VipsImage out, int qfac, ref byte[] obuf, ref int olen)
{
    size_t len;

    if (VipsJpegsaveBuffer(in, ref obuf, ref len, "Q", qfac, null))
        return -1;
    if (out != null)
        ImAddCallback(out, "close",
            (im_callback_fn)VipsFree, obuf, null);

    if (olen.HasValue)
        olen = (int)len;

    return 0;
}

// im_vips2mimejpeg
public static int ImVips2MimeJpeg(VipsImage in, int qfac)
{
    return VipsJpegsaveMime(in, "Q", qfac, null);
}
```