```csharp
// im_vips2dz

public static int ImVips2Dz(VipsImage inImage, string filename)
{
    // char *p, *q;
    // char name[FILENAME_MAX];
    // char mode[FILENAME_MAX];
    // char buf[FILENAME_MAX];

    VipsForeignDzLayout layout = VipsForeignDzLayout.DZ;
    string suffix = ".jpeg";
    int overlap = 0;
    int tile_size = 256;
    VipsForeignDzDepth depth = VipsForeignDzDepth.ONEPIXEL;
    bool centre = false;
    VipsAngle angle = VipsAngle.D0;

    // We can't use im_filename_split() --- it assumes that we have a
    // filename with an extension before the ':', and filename here is
    // actually a dirname.
    //
    // Just split on the first ':'.
    string name = filename;
    if (name.Contains(":"))
    {
        int colonIndex = name.IndexOf(":");
        name = name.Substring(0, colonIndex);
        string mode = name.Substring(colonIndex + 1);
        name = mode;
    }

    // ... rest of the method remains the same ...
}
```