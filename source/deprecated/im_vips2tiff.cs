```csharp
// im_vips2tiff

using System;

public class VipsForeignTiffCompression
{
    public const int NONE = 0;
    public const int PACKBITS = 1;
    public const int CCITTFAX4 = 2;
    public const int LZW = 3;
    public const int DEFLATE = 6;
    public const int JPEG = 7;
}

public class VipsForeignTiffPredictor
{
    public const int NONE = 0;
}

public class VipsForeignTiffResunit
{
    public const int CM = 1;
    public const int INCH = 2;
}

public class Image
{
    public double Xres { get; set; }
    public double Yres { get; set; }
}

public class Program
{
    public static int im_vips2tiff(Image inImage, string filename)
    {
        // char *p, *q, *r;
        string name = "";
        string mode = "";
        string buf = "";

        VipsForeignTiffCompression compression = VipsForeignTiffCompression.NONE;
        int Q = 75;
        VipsForeignTiffPredictor predictor = VipsForeignTiffPredictor.NONE;
        string profile = null;
        bool tile = false;
        int tileWidth = 128;
        int tileHeight = 128;
        bool pyramid = false;
        bool squash = false;
        VipsForeignTiffResunit resunit = VipsForeignTiffResunit.CM;
        double xres = inImage.Xres * 10.0;
        double yres = inImage.Yres * 10.0;
        bool bigtiff = false;

        im_filename_split(filename, out name, out mode);
        buf = mode;
        string p = buf;

        while (true)
        {
            string q = im_getnextoption(ref p);
            if (q == null) break;

            if (im_isprefix("none", q))
                compression = VipsForeignTiffCompression.NONE;
            else if (im_isprefix("packbits", q))
                compression = VipsForeignTiffCompression.PACKBITS;
            else if (im_isprefix("ccittfax4", q))
                compression = VipsForeignTiffCompression.CCITTFAX4;
            else if (im_isprefix("lzw", q))
            {
                compression = VipsForeignTiffCompression.LZW;

                string r = im_getsuboption(q);
                int i;
                if (!int.TryParse(r, out i) || sscanf(r, "%d", out i) != 1)
                {
                    im_error("im_vips2tiff", "%s", _("bad predictor parameter"));
                    return -1;
                }
                predictor = (VipsForeignTiffPredictor)i;
            }
            else if (im_isprefix("deflate", q))
            {
                compression = VipsForeignTiffCompression.DEFLATE;

                string r = im_getsuboption(q);
                int i;
                if (!int.TryParse(r, out i) || sscanf(r, "%d", out i) != 1)
                {
                    im_error("im_vips2tiff", "%s", _("bad predictor parameter"));
                    return -1;
                }
                predictor = (VipsForeignTiffPredictor)i;
            }
            else if (im_isprefix("jpeg", q))
            {
                compression = VipsForeignTiffCompression.JPEG;

                string r = im_getsuboption(q);
                if (!int.TryParse(r, out Q) || sscanf(r, "%d", out Q) != 1)
                {
                    im_error("im_vips2tiff", "%s", _("bad JPEG quality parameter"));
                    return -1;
                }
            }
            else
            {
                im_error("im_vips2tiff", "%s", _("unknown compression mode \"" + q + "\"\nshould be one of \"none\", \"packbits\", \"ccittfax4\", \"lzw\", \"deflate\" or \"jpeg\""));
                return -1;
            }
        }

        while (true)
        {
            string q = im_getnextoption(ref p);
            if (q == null) break;

            if (im_isprefix("tile", q))
            {
                tile = true;

                string r = im_getsuboption(q);
                if (!int.TryParse(r, out tileWidth) || !int.TryParse(r.Substring(3), out tileHeight) || sscanf(r, "%dx%d", out tileWidth, out tileHeight) != 2)
                {
                    im_error("im_vips2tiff", "%s", _("bad tile sizes"));
                    return -1;
                }
            }
            else if (im_isprefix("strip", q))
                tile = false;
            else
            {
                im_error("im_vips2tiff", "%s", _("unknown layout mode \"" + q + "\"\nshould be one of \"tile\" or \"strip\""));
                return -1;
            }
        }

        while (true)
        {
            string q = im_getnextoption(ref p);
            if (q == null) break;

            if (im_isprefix("pyramid", q))
                pyramid = true;
            else if (im_isprefix("flat", q))
                pyramid = false;
            else
            {
                im_error("im_vips2tiff", "%s", _("unknown multi-res mode \"" + q + "\"\nshould be one of \"flat\" or \"pyramid\""));
                return -1;
            }
        }

        while (true)
        {
            string q = im_getnextoption(ref p);
            if (q == null) break;

            if (im_isprefix("onebit", q))
                squash = true;
            else if (im_isprefix("manybit", q))
                squash = false;
            else
            {
                im_error("im_vips2tiff", "%s", _("unknown format \"" + q + "\"\nshould be one of \"onebit\" or \"manybit\""));
                return -1;
            }
        }

        while (true)
        {
            string q = im_getnextoption(ref p);
            if (q == null) break;

            if (im_isprefix("res_cm", q))
                resunit = VipsForeignTiffResunit.CM;
            else if (im_isprefix("res_inch", q))
                resunit = VipsForeignTiffResunit.INCH;
            else
            {
                im_error("im_vips2tiff", "%s", _("unknown resolution unit \"" + q + "\"\nshould be one of \"res_cm\" or \"res_inch\""));
                return -1;
            }

            string r = im_getsuboption(q);
            if (r != null)
            {
                if (!double.TryParse(r, out xres) || !double.TryParse(r.Substring(3), out yres) || sscanf(r, "%lfx%lf", out xres, out yres) != 2)
                {
                    if (!double.TryParse(r, out xres))
                    {
                        im_error("im_vips2tiff", "%s", _("bad resolution values"));
                        return -1;
                    }

                    yres = xres;
                }

                // vips resolutions are always in pixels/mm. If the
                // user specifies ",res_inch:72x72" then they are
                // using pixels/inch instead and we must convert.
                if (resunit == VipsForeignTiffResunit.INCH)
                {
                    xres /= 2.54;
                    yres /= 2.54;
                }
            }
        }

        while (true)
        {
            string q = im_getnextoption(ref p);
            if (q == null) break;

            profile = im_strdup(null, q);

            if (string.Equals(q, "8", StringComparison.Ordinal))
                bigtiff = true;

            if (im_isprefix("unknown", q))
            {
                im_error("im_vips2tiff", "%s", _("unknown extra options \"" + q + "\""));
                return -1;
            }
        }

        if (vips_tiffsave(inImage, name,
            "compression", compression,
            "Q", Q,
            "predictor", predictor,
            "profile", profile,
            "tile", tile,
            "tile_width", tileWidth,
            "tile_height", tileHeight,
            "pyramid", pyramid,
            "squash", squash,
            "resunit", resunit,
            "xres", xres,
            "yres", yres,
            "bigtiff", bigtiff,
            null))
        {
            return -1;
        }

        return 0;
    }
}
```