```csharp
// im_vips2csv

using System;
using VipsDotNet;

public class ImageConverter
{
    public static int ImVips2Csv(Image inImage, string filename)
    {
        // char *separator = "\t";
        string separator = "\t";

        // char name[FILENAME_MAX];
        // char mode[FILENAME_MAX];
        // char *p, *q, *r;

        // Parse mode string.
        var parts = filename.Split(new[] { ' ' }, 3);
        if (parts.Length < 2)
            return -1; // invalid filename

        var name = parts[0];
        var mode = parts[1];

        // p = &mode[0];
        // while ((q = im_getnextoption(&p))) {
        //     if (im_isprefix("sep", q) && (r = im_getsuboption(q)))
        //         separator = r;
        // }

        if (VipsCsvSave(inImage, name, "separator", separator))
            return -1;

        return 0;
    }
}
```