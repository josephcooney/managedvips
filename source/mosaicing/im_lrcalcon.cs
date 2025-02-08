Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Linq;

public class VipsImage {
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }
    public int BandFmt { get; set; }
    public byte[] data { get; set; }

    public bool im_incheck() {
        // implementation of im_incheck method
        return true;
    }
}

public class TiePoints {
    public int nopoints { get; set; }
    public int halfareasize { get; set; }
    public int halfcorsize { get; set; }
    public int[] x_reference { get; set; }
    public int[] y_reference { get; set; }
    public int[] contrast { get; set; }
}

public class PosCont {
    public int x { get; set; }
    public int y { get; set; }
    public int cont { get; set; }
}

public static class VipsExtensions {
    public static bool all_black(this VipsImage im, int xpos, int ypos, int winsize) {
        const int hwinsize = (winsize - 1) / 2;
        const int left = xpos - hwinsize;
        const int top = ypos - hwinsize;
        const int ls = im.Xsize;

        for (int y = 0; y < winsize; y++) {
            var line = im.data.Skip(top * ls + left).Take(ls).ToArray();
            if (!line.All(p => p == 0))
                return false;
        }
        return true;
    }

    public static int calculate_contrast(this VipsImage im, int xpos, int ypos, int winsize) {
        const int hwinsize = (winsize - 1) / 2;
        const int left = xpos - hwinsize;
        const int top = ypos - hwinsize;
        const int ls = im.Xsize;

        var line = im.data.Skip(top * ls + left).Take(ls).ToArray();
        int total = 0;
        for (int y = 0; y < winsize - 1; y++) {
            var p = line;
            for (int x = 0; x < winsize - 1; x++) {
                var lrd = Math.Abs((byte)p[0] - p[1]);
                var tbd = Math.Abs((byte)p[0] - im.data[(top + y) * ls + left + x + 1]);
                total += lrd + tbd;
                p++;
            }
        }
        return total;
    }

    public static int pos_compare(PosCont l, PosCont r) {
        return r.cont - l.cont;
    }

    public static void vips__find_best_contrast(VipsImage im,
        int xpos, int ypos, int xsize, int ysize,
        int[] xarray, int[] yarray, int[] cont,
        int nbest, int hcorsize) {
        const int windowsize = 2 * hcorsize + 1;
        const int nacross = (xsize - windowsize + hcorsize) / hcorsize;
        const int ndown = (ysize - windowsize + hcorsize) / hcorsize;

        if (nacross <= 0 || ndown <= 0) {
            throw new ArgumentException("overlap too small for your search size");
        }

        var pc = Enumerable.Range(0, nacross * ndown).Select(i => new PosCont()).ToArray();

        for (int i = 0, y = 0; y < ndown; y++)
            for (int x = 0; x < nacross; x++) {
                int left = xpos + x * hcorsize;
                int top = ypos + y * hcorsize;

                if (im.all_black(left, top, windowsize))
                    continue;

                pc[i].x = left;
                pc[i].y = top;
                pc[i].cont = im.calculate_contrast(left, top, windowsize);
                i++;
            }

        int elms = i;

        if (elms < nbest) {
            throw new ArgumentException($"found {elms} tie-points, need at least {nbest}");
        }

        Array.Sort(pc, (l, r) => pos_compare(l, r));

        for (int i = 0; i < nbest; i++) {
            xarray[i] = pc[i].x;
            yarray[i] = pc[i].y;
            cont[i] = pc[i].cont;
        }
    }

    public static int vips__lrcalcon(VipsImage ref, TiePoints points) {
        const int border = points.halfareasize;

        const int aheight = ref.Ysize / AREAS;

        const int len = points.nopoints / AREAS;

        for (int i = 0; ; i++) {
            var area = new VipsRect { height = aheight, width = ref.Xsize, left = 0, top = 0 };
            vips_rect_marginadjust(ref, -border);
            if (area.top >= ref.Ysize)
                break;

            if (vips__find_best_contrast(ref,
                    area.left, area.top, area.width, area.height,
                    points.x_reference + i * len,
                    points.y_reference + i * len,
                    points.contrast + i * len,
                    len,
                    points.halfcorsize))
                return -1;
        }
        return 0;
    }

    public static void vips_rect_marginadjust(VipsImage im, int margin) {
        // implementation of vips_rect_marginadjust method
    }
}

public class VipsRect {
    public int height { get; set; }
    public int width { get; set; }
    public int left { get; set; }
    public int top { get; set; }
}
```

Note that I've made some assumptions about the implementation of certain methods (e.g. `im_incheck`, `vips_rect_marginadjust`) as they were not provided in the original C code. You may need to modify these implementations to match your specific requirements.

Also, I've used LINQ for array operations and sorting, which is a more modern and concise way of writing C# code. If you prefer a more traditional approach, you can replace these with manual loops.