```csharp
// @(#) Extract a tile from a pyramid as a jpeg
// @(#)
// @(#) int
// @(#) im_bernd(const char *tiffname,
// @(#) 	int x, int y, int w, int h)
// @(#)
// @(#)
// @(#) Returns 0 on success and -1 on error
// @(#)
// *
// * 7/5/99 JC
// *	- from im_tiff2vips and im_vips2jpeg, plus some stuff from Steve
// * 11/7/01 JC
// *	- page number now in filename
// * 12/5/09
// *	- fix signed/unsigned warning

using System;
using VipsDotNet;

public class Program
{
    public static int im_bernd(string tiffname, int x, int y, int w, int h)
    {
        // @(#) Returns 0 on success and -1 on error
        Image inImage = new Image("im_bernd:1", AccessMode.ReadWrite);

        if (inImage == null || !Vips.Tiff2Vips(tiffname, inImage) ||
            extract(inImage, x, y, w, h))
        {
            inImage.Dispose();
            return -1;
        }
        inImage.Dispose();

        return 0;
    }

    public static int extract(Image inImage, int x, int y, int w, int h)
    {
        // @(#) Returns 0 on success and -1 on error
        Image t1 = new Image("im_bernd:2", AccessMode.ReadWrite);

        if (t1 == null || !Vips.ExtractArea(inImage, t1, x, y, w, h) ||
            Vips.Vips2BufJpeg(t1, inImage, 75, out byte[] buf, out int len))
        {
            t1.Dispose();
            return -1;
        }

        try
        {
            using (var stream = new System.IO.MemoryStream())
            {
                stream.Write(buf, 0, len);
                stream.Position = 0;

                var writer = new System.IO.BinaryWriter(stream);
                writer.Write(buf, 0, len);

                Console.Out.Write(buf, 0, len);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("im_bernd: " + ex.Message);
            t1.Dispose();
            return -1;
        }

        t1.Dispose();

        return 0;
    }
}
```