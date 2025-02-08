Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsImage
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public string filename { get; set; }
    public int Coding { get; set; }
}

public class TiePoints
{
    public string reference { get; set; }
    public string secondary { get; set; }
    public int nopoints { get; set; }
    public double deltax { get; set; }
    public double deltay { get; set; }
    public int halfcorsize { get; set; }
    public int halfareasize { get; set; }

    public int[] x_reference { get; set; }
    public int[] y_reference { get; set; }
    public int[] x_secondary { get; set; }
    public int[] y_secondary { get; set; }
    public double[] contrast { get; set; }
    public double[] correlation { get; set; }
    public double[] dx { get; set; }
    public double[] dy { get; set; }
    public double[] deviation { get; set; }

    public TiePoints()
    {
        x_reference = new int[VIPS_MAXPOINTS];
        y_reference = new int[VIPS_MAXPOINTS];
        x_secondary = new int[VIPS_MAXPOINTS];
        y_secondary = new int[VIPS_MAXPOINTS];
        contrast = new double[VIPS_MAXPOINTS];
        correlation = new double[VIPS_MAXPOINTS];
        dx = new double[VIPS_MAXPOINTS];
        dy = new double[VIPS_MAXPOINTS];
        deviation = new double[VIPS_MAXPOINTS];
    }
}

public class Vips
{
    public static int vips__find_lroverlap(VipsImage ref_in, VipsImage sec_in, VipsImage out,
        int bandno_in,
        int xref, int yref, int xsec, int ysec,
        int halfcorrelation, int halfarea,
        out int dx0, out int dy0,
        out double scale1, out double angle1, out double dx1, out double dy1)
    {
        VipsImage[] t = new VipsImage[6];

        VipsRect left = new VipsRect();
        VipsRect right = new VipsRect();
        VipsRect overlap = new VipsRect();

        TiePoints points = new TiePoints();
        TiePoints newpoints = new TiePoints();

        int i;
        int dx, dy;

        // Test cor and area.
        if (halfcorrelation < 0 || halfarea < 0 ||
            halfarea < halfcorrelation)
        {
            throw new Exception("bad area parameters");
        }

        // Set positions of left and right.
        left.left = 0;
        left.top = 0;
        left.width = ref_in.Xsize;
        left.height = ref_in.Ysize;
        right.left = xref - xsec;
        right.top = yref - ysec;
        right.width = sec_in.Xsize;
        right.height = sec_in.Ysize;

        // Find overlap.
        VipsRect.Intersect(ref_in, sec_in, out overlap);
        if (overlap.width < 2 * halfarea + 1 ||
            overlap.height < 2 * halfarea + 1)
        {
            throw new Exception("overlap too small for search");
        }

        // Extract overlaps as 8-bit, 1 band.
        if (!Vips.ExtractArea(ref_in, t[0], overlap.left, overlap.top,
                overlap.width, overlap.height) ||
            !Vips.ExtractArea(sec_in, t[1], overlap.left - right.left,
                overlap.top - right.top, overlap.width, overlap.height))
        {
            return -1;
        }
        if (ref_in.Coding == VIPS_CODING_LABQ)
        {
            if (!LabQ2sRGB(t[0], out t[2]) ||
                !LabQ2sRGB(t[1], out t[3]) ||
                !ExtractBand(t[2], out t[4], 1) ||
                !ExtractBand(t[3], out t[5], 1))
            {
                return -1;
            }
        }
        else if (ref_in.Coding == VIPS_CODING_NONE)
        {
            if (!ExtractBand(t[0], out t[2], bandno_in) ||
                !ExtractBand(t[1], out t[3], bandno_in) ||
                !Scale(t[2], out t[4]) ||
                !Scale(t[3], out t[5]))
            {
                return -1;
            }
        }
        else
        {
            throw new Exception("unknown Coding type");
        }

        // Initialise and fill TiePoints
        points.reference = ref_in.filename;
        points.secondary = sec_in.filename;
        points.nopoints = VIPS_MAXPOINTS;
        points.deltax = 0;
        points.deltay = 0;
        points.halfcorsize = halfcorrelation;
        points.halfareasize = halfarea;

        // Initialise the structure
        for (i = 0; i < VIPS_MAXPOINTS; i++)
        {
            points.x_reference[i] = 0;
            points.y_reference[i] = 0;
            points.x_secondary[i] = 0;
            points.y_secondary[i] = 0;
            points.contrast[i] = 0;
            points.correlation[i] = 0.0;
            points.dx[i] = 0.0;
            points.dy[i] = 0.0;
            points.deviation[i] = 0.0;
        }

        // Search ref for possible tie-points. Sets: p_points->contrast,
        // p_points->x,y_reference.
        if (!vips__lrcalcon(t[4], points))
        {
            return -1;
        }

        // For each candidate point, correlate against corresponding part of
        // sec. Sets x,y_secondary and fills correlation and dx, dy.
        if (!vips__chkpair(t[4], t[5], points))
        {
            return -1;
        }

        // First call to vips_clinear().
        if (!vips__initialize(points))
        {
            return -1;
        }

        // Improve the selection of tiepoints until all abs(deviations) are
        // < 1.0 by deleting all wrong points.
        if (!vips__improve(points, newpoints))
        {
            return -1;
        }

        // Average remaining offsets.
        if (!vips__avgdxdy(newpoints, out dx, out dy))
        {
            return -1;
        }

        // Offset with overlap position.
        dx0 = -right.left + dx;
        dy0 = -right.top + dy;

        // Write 1st order parameters too.
        scale1 = newpoints.l_scale;
        angle1 = newpoints.l_angle;
        dx1 = newpoints.l_deltax;
        dy1 = newpoints.l_deltay;

        return 0;
    }

    public static int vips__lrmosaic(VipsImage ref, VipsImage sec, VipsImage out,
        int bandno,
        int xref, int yref, int xsec, int ysec,
        int hwindowsize, int hsearchsize,
        int mwidth)
    {
        int dx0, dy0;
        double scale1, angle1, dx1, dy1;

        VipsImage dummy = new VipsImage();
        if (vips__find_lroverlap(ref, sec, dummy,
                bandno,
                xref, yref, xsec, ysec,
                hwindowsize, hsearchsize,
                out dx0, out dy0,
                out scale1, out angle1, out dx1, out dy1))
        {
            return -1;
        }

        // Merge left right.
        VipsImage x = new VipsImage();
        if (!Merge(ref, sec, out x, VIPS_DIRECTION_HORIZONTAL, dx0, dy0,
                "mblend", mwidth,
                null))
        {
            g_object_unref(x);
            return -1;
        }
        if (!Vips.ImageWrite(x, out))
        {
            g_object_unref(x);
            return -1;
        }

        g_object_unref(x);

        return 0;
    }
}

public class VipsRect
{
    public int left { get; set; }
    public int top { get; set; }
    public int width { get; set; }
    public int height { get; set; }

    public static void Intersect(VipsImage a, VipsImage b, out VipsRect result)
    {
        // implementation of intersect
    }
}

public class VipsImageExtensions
{
    public static bool ExtractArea(this VipsImage image, out VipsImage area,
        int left, int top, int width, int height)
    {
        // implementation of extract_area
    }

    public static bool LabQ2sRGB(this VipsImage image, out VipsImage result)
    {
        // implementation of labq2srgb
    }

    public static bool ExtractBand(this VipsImage image, out VipsImage band,
        int bandno)
    {
        // implementation of extract_band
    }

    public static bool Scale(this VipsImage image, out VipsImage result)
    {
        // implementation of scale
    }
}

public class VipsMergeExtensions
{
    public static bool Merge(this VipsImage a, VipsImage b, out VipsImage result,
        int direction, int dx0, int dy0,
        string method, int mwidth,
        string[] options)
    {
        // implementation of merge
    }

    public static bool ImageWrite(this VipsImage image, object output)
    {
        // implementation of image_write
    }
}
```
Note that some methods are not implemented as they require additional information about the VIPS library and its functionality. You will need to implement these methods according to your requirements.

Also note that this is a direct translation from C to C# and may not be the most idiomatic or efficient way to write the code in C#.