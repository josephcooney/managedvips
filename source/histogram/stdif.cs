Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsStdif : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public double A { get; set; }
    public double M0 { get; set; }
    public double B { get; set; }
    public double S0 { get; set; }

    protected override void Generate(VipsRegion out_region, VipsImage in_image)
    {
        int bands = in_image.Bands;
        int npel = Width * Height;

        VipsRect r = out_region.Valid;
        VipsRegion ir = (VipsRegion)in_image;
        VipsImage stdif = this;

        VipsRect irect;
        int y;
        int lsk;
        int centre; /* Offset to move to centre of window */

        // What part of ir do we need?
        irect.Left = out_region.Valid.Left;
        irect.Top = out_region.Valid.Top;
        irect.Width = out_region.Valid.Width + Width;
        irect.Height = out_region.Valid.Height + Height;
        if (VipsRegion.Prepare(ir, ref irect))
            return;

        lsk = VipsRegion.LSkip(ir);
        centre = lsk * (Height / 2) + Width / 2;

        for (y = 0; y < r.Height; y++)
        {
            // Get input and output pointers for this line.
            VipsPel[] p = VipsRegion.Addr(ir, r.Left, r.Top + y);
            VipsPel[] q = VipsRegion.Addr(out_region, r.Left, r.Top + y);

            double f1 = A * M0;
            double f2 = 1.0 - A;
            double f3 = B * S0;

            VipsPel p1;
            int x, i, j, b;

            // We will get int overflow for windows larger than about 256
            // x 256, sadly.
            uint[] sum = new uint[MAX_BANDS];
            uint[] sum2 = new uint[MAX_BANDS];

            // Find sum, sum of squares for the start of this line.
            for (b = 0; b < bands; b++)
                sum[b] = 0;
            for (b = 0; b < bands; b++)
                sum2[b] = 0;

            p1 = p;
            for (j = 0; j < Height; j++)
            {
                i = 0;
                for (x = 0; x < Width; x++)
                {
                    for (b = 0; b < bands; b++)
                    {
                        int t = p1[i++];

                        sum[b] += t;
                        sum2[b] += t * t;
                    }
                }

                p1 += lsk;
            }

            // Loop for output pels.
            for (x = 0; x < r.Width; x++)
            {
                for (b = 0; b < bands; b++)
                {
                    // Find stats.
                    double mean = (double)sum[b] / npel;
                    double var = (double)sum2[b] / npel - (mean * mean);
                    double sig = Math.Sqrt(var);

                    // Transform.
                    double res = f1 + f2 * mean +
                        ((double)p[centre] - mean) *
                            (f3 / (S0 + B * sig));

                    // And write.
                    if (res < 0.0)
                        q[x] = 0;
                    else if (res >= 256.0)
                        q[x] = 255;
                    else
                        q[x] = res + 0.5;

                    // Adapt sums - remove the pels from the left
                    // hand column, add in pels for a new
                    // right-hand column.
                    p1 = p;
                    for (j = 0; j < Height; j++)
                    {
                        int t1 = p1[0];
                        int t2 = p1[bands * Width];

                        sum[b] -= t1;
                        sum2[b] -= t1 * t1;

                        sum[b] += t2;
                        sum2[b] += t2 * t2;

                        p1 += lsk;
                    }

                    p++;
                }
            }
        }
    }

    protected override void Build(VipsObject obj)
    {
        VipsObjectClass class = (VipsObjectClass)VipsObject.GetClass(obj);
        VipsStdif stdif = (VipsStdif)obj;

        VipsImage[] t = new VipsImage[3];

        VipsImage in_image;

        if (base.Build(obj))
            return;

        in_image = stdif.In;

        if (in_image.Decode(ref t[0]))
            return;
        in_image = t[0];

        if (VipsCheckFormat(class.Nickname, in_image, VipsFormat.UChar))
            return;

        if (stdif.Width > in_image.Xsize ||
            stdif.Height > in_image.Ysize)
        {
            VipsError(class.Nickname, "%s", _("window too large"));
            return;
        }
        if (in_image.Bands > MAX_BANDS)
        {
            VipsError(class.Nickname, "%s", _("too many bands"));
            return;
        }

        // Expand the input.
        if (VipsEmbed(in_image, ref t[1],
                stdif.Width / 2, stdif.Height / 2,
                in_image.Xsize + stdif.Width - 1, in_image.Ysize + stdif.Height - 1,
                "extend", VipsExtend.Copy,
                null))
            return;
        in_image = t[1];

        obj.SetProperty("out", new VipsImage());

        // Set demand hints. FATSTRIP is good for us, as THINSTRIP will cause
        // too many recalculations on overlaps.
        if (VipsImage.Pipelinev(stdif.Out,
                VipsDemandStyle.FatStrip, in_image, null))
            return;
        stdif.Out.Xsize -= stdif.Width - 1;
        stdif.Out.Ysize -= stdif.Height - 1;

        if (stdif.Out.Generate(stdif.Out, in_image))
            return;

        stdif.Out.Xoffset = 0;
        stdif.Out.Yoffset = 0;

        VipsReorderMarginHint(stdif.Out, stdif.Width * stdif.Height);
    }

    public static void ClassInit(VipsStdifClass class)
    {
        GObjectClass gobject_class = (GObjectClass)class;
        VipsObjectClass object_class = (VipsObjectClass)class;

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        object_class.Nickname = "stdif";
        object_class.Description = _("statistical difference");
        object_class.Build = new VipsOperation.Build(VipsStdif.Build);

        // Windows larger than 256x256 will overflow sum2, see above.
        VIPS_ARG_IMAGE(object_class, "in", 1,
            _("Input"),
            _("Input image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsStdif, In));

        VIPS_ARG_IMAGE(object_class, "out", 2,
            _("Output"),
            _("Output image"),
            VIPS_ARGUMENT_REQUIRED_OUTPUT,
            G_STRUCT_OFFSET(VipsStdif, Out));

        VIPS_ARG_INT(object_class, "width", 4,
            _("Width"),
            _("Window width in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsStdif, Width),
            1, 256, 11);

        VIPS_ARG_INT(object_class, "height", 5,
            _("Height"),
            _("Window height in pixels"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsStdif, Height),
            1, 256, 11);

        VIPS_ARG_DOUBLE(object_class, "a", 2,
            _("Mean weight"),
            _("Weight of new mean"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsStdif, A),
            0.0, 1.0, 0.5);

        VIPS_ARG_DOUBLE(object_class, "m0", 2,
            _("Mean"),
            _("New mean"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsStdif, M0),
            double.MinValue, double.MaxValue, 128);

        VIPS_ARG_DOUBLE(object_class, "b", 2,
            _("Deviation weight"),
            _("Weight of new deviation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsStdif, B),
            0.0, 2.0, 0.5);

        VIPS_ARG_DOUBLE(object_class, "s0", 2,
            _("Deviation"),
            _("New deviation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            G_STRUCT_OFFSET(VipsStdif, S0),
            double.MinValue, double.MaxValue, 50);
    }

    public static void Init(VipsStdif stdif)
    {
        stdif.Width = 11;
        stdif.Height = 11;
        stdif.A = 0.5;
        stdif.M0 = 128.0;
        stdif.B = 0.5;
        stdif.S0 = 50.0;
    }
}

public class VipsOperation
{
    public virtual void Generate(VipsRegion out_region, VipsImage in_image)
    {
    }

    public virtual void Build(VipsObject obj)
    {
    }

    public static void ClassInit(VipsOperationClass class)
    {
        GObjectClass gobject_class = (GObjectClass)class;
        VipsObjectClass object_class = (VipsObjectClass)class;

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        object_class.Nickname = "stdif";
        object_class.Description = _("statistical difference");
        object_class.Build = new VipsOperation.Build(VipsStdif.Build);
    }

    public static void Init(VipsOperation obj)
    {
    }
}

public class VipsImage
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }

    public virtual bool Decode(ref VipsImage image)
    {
        return false;
    }

    public virtual void Generate(VipsRegion out_region, VipsOperation operation)
    {
    }

    public static void Pipelinev(VipsImage output, VipsDemandStyle demand_style, VipsImage input, object[] args)
    {
    }
}

public class VipsRegion
{
    public VipsRect Valid { get; set; }

    public virtual bool Prepare(ref VipsRect rect)
    {
        return false;
    }

    public static void ReorderMarginHint(VipsImage image, int margin)
    {
    }

    public static uint LSkip(VipsRegion region)
    {
        return 0;
    }
}

public class VipsObject
{
    public virtual bool Build(VipsObject obj)
    {
        return false;
    }

    public static void SetProperty(object obj, string name, object value)
    {
    }

    public static object GetProperty(object obj, string name)
    {
        return null;
    }
}
```

Note that this code uses the `VipsOperation` class as a base for the `VipsStdif` class. The `Generate` and `Build` methods are implemented in the `VipsStdif` class, while the other methods are implemented in the base classes.

Also note that some of the methods (e.g., `Decode`, `Prepare`, `Pipelinev`) have been simplified or removed for brevity, as they were not directly related to the conversion process.