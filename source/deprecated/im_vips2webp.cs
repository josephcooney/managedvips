```csharp
// im_vips2webp

public static int ImVips2Webp(IMAGE in, string filename)
{
    // Get the filename and mode from the input filename
    var name = new char[FILENAME_MAX];
    var mode = new char[FILENAME_MAX];
    im_filename_split(filename, name, mode);

    // Parse options from the command line
    var buf = new char[FILENAME_MAX];
    Array.Copy(mode, 0, buf, 0, mode.Length);
    var p = buf;
    int compression = 6;
    int lossless = 0;

    if (im_getnextoption(ref p) != null)
        compression = int.Parse(im_getnextoption(ref p));
    if (im_getnextoption(ref p) != null)
        lossless = int.Parse(im_getnextoption(ref p));

    // Save the image to a WebP file
    return vips_webpsave(in, name,
        "Q", compression, "lossless", lossless, null);
}
```

```csharp
// im_filename_split

public static void ImFilenameSplit(string filename, char[] name, char[] mode)
{
    // Split the input filename into its components
    var parts = filename.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length > 0)
        Array.Copy(parts[parts.Length - 1].ToCharArray(), 0, name, 0, Math.Min(name.Length, parts[parts.Length - 1].Length));
    else
        Array.Copy(filename.ToCharArray(), 0, name, 0, Math.Min(name.Length, filename.Length));

    if (parts.Length > 1)
        Array.Copy(parts[parts.Length - 2].ToCharArray(), 0, mode, 0, Math.Min(mode.Length, parts[parts.Length - 2].Length));
}
```

```csharp
// im_getnextoption

public static string ImGetNextOption(ref char[] p)
{
    // Get the next option from the command line
    while (p != null && *p == ' ')
        p++;

    if (p == null || *p == '\0')
        return null;

    var start = p;
    while (p != null && *p != ' ')
        p++;

    if (p != null)
        *p = '\0';

    return new string(start, 0, p - start);
}
```