```csharp
// vips_measure.c

using System;
using VipsDotNet;

public class VipsMeasure : VipsOperation
{
    public VipsImage In { get; set; }
    public VipsImage Out { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int H { get; set; }
    public int V { get; set; }

    protected override int Build()
    {
        // vips_measure_build
        if (base.Build())
            return -1;

        var ready = In.Decode();
        Local(ready);

        var bands = ready.GetBands();

        SetProperty("out", new VipsImageMatrix(bands, H * V));

        if (!GetProperty<bool>("width"))
            Width = ready.Width;
        if (!GetProperty<bool>("height"))
            Height = ready.Height;

        var pw = (double)Width / H;
        var ph = (double)Height / V;

        var w = (pw + 1) / 2;
        var h = (ph + 1) / 2;

        for (var j = 0; j < V; j++)
        {
            for (var i = 0; i < H; i++)
            {
                var x = Left + i * pw + (pw + 2) / 4;
                var y = Top + j * ph + (ph + 2) / 4;

                double avg, dev;

                for (var b = 0; b < bands; b++)
                {
                    var t = new VipsImage[2];
                    Local(t);

                    if (ExtractArea(ready, t[0], x, y, w, h, null) ||
                        ExtractBand(t[0], t[1], b, null) ||
                        Avg(t[1], out: ref avg, null) ||
                        Deviate(t[1], out: ref dev, null))
                    {
                        return -1;
                    }

                    if (dev * 5 > Math.Abs(avg) && Math.Abs(avg) > 3)
                        Console.WriteLine($"Warning: patch {i} x {j}, band {b}: avg = {avg}, sdev = {dev}");

                    VipsMatrix.Set(Out, b, i + j * H, avg);
                }
            }
        }

        return 0;
    }

    protected override void ClassInit(VipsMeasureClass class_)
    {
        base.ClassInit(class_);

        var gobject_class = (GObjectClass)class_;
        var object_class = (VipsObjectClass)class_;

        gobject_class.SetProperty += VipsObjectSetProperty;
        gobject_class.GetProperty += VipsObjectGetProperty;

        object_class.Nickname = "measure";
        object_class.Description = _("measure a set of patches on a color chart");
        object_class.Build = Build;

        VIPS_ARG_IMAGE(class_, "in", 1, _("Input"), _("Image to measure"), VIPS_ARGUMENT_REQUIRED_INPUT, OffsetOf(In));
        VIPS_ARG_IMAGE(class_, "out", 2, _("Output"), _("Output array of statistics"), VIPS_ARGUMENT_REQUIRED_OUTPUT, OffsetOf(Out));
        VIPS_ARG_INT(class_, "h", 5, _("Across"), _("Number of patches across chart"), VIPS_ARGUMENT_REQUIRED_INPUT, OffsetOf(H), 1, VIPS_MAX_COORD, 1);
        VIPS_ARG_INT(class_, "v", 6, _("Down"), _("Number of patches down chart"), VIPS_ARGUMENT_REQUIRED_INPUT, OffsetOf(V), 1, VIPS_MAX_COORD, 1);
        VIPS_ARG_INT(class_, "left", 10, _("Left"), _("Left edge of extract area"), VIPS_ARGUMENT_OPTIONAL_INPUT, OffsetOf(Left), 0, VIPS_MAX_COORD, 0);
        VIPS_ARG_INT(class_, "top", 11, _("Top"), _("Top edge of extract area"), VIPS_ARGUMENT_OPTIONAL_INPUT, OffsetOf(Top), 0, VIPS_MAX_COORD, 0);
        VIPS_ARG_INT(class_, "width", 12, _("Width"), _("Width of extract area"), VIPS_ARGUMENT_OPTIONAL_INPUT, OffsetOf(Width), 1, VIPS_MAX_COORD, 1);
        VIPS_ARG_INT(class_, "height", 13, _("Height"), _("Height of extract area"), VIPS_ARGUMENT_OPTIONAL_INPUT, OffsetOf(Height), 1, VIPS_MAX_COORD, 1);
    }

    protected override void Init()
    {
        base.Init();
    }
}

public class Program
{
    public static int VipsMeasure(VipsImage in_, out VipsImage[] out_, int h, int v)
    {
        var measure = new VipsMeasure { In = in_ };
        measure.H = h;
        measure.V = v;

        return measure.Build();
    }
}
```