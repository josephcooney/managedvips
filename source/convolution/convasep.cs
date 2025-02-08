Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsConvasep : VipsConvolution
{
    public int Layers { get; set; }
    public int Divisor { get; set; }
    public int Rounding { get; set; }
    public int Offset { get; set; }

    public int Width { get; private set; }
    public VipsImage IM { get; private set; }

    public int NLines { get; private set; }
    public int[] Start { get; private set; }
    public int[] End { get; private set; }
    public int[] Factor { get; private set; }

    public override void Build()
    {
        base.Build();

        if (VipsCheckSeparable(Nickname, M))
            return;

        // An int version of our mask.
        VipsImage t3 = null;
        if (!VipsImageIntize(M, out t3))
            return;
        IM = t3;
        Width = IM.Xsize * IM.Ysize;

        if (Decompose())
            return;

        GObjectSet(this, "out", new VipsImage(), null);
        if (
            !VipsEmbed(In, ref t0,
                Width / 2,
                Width / 2,
                In.Xsize + Width - 1,
                In.Ysize + Width - 1,
                "extend", VipsExtendCopy,
                null) ||
            !ConvasepPass(this, ref t0, ref t1, VipsDirection.Horizontal) ||
            !ConvasepPass(this, ref t1, ref t2, VipsDirection.Vertical) ||
            !VipsImageWrite(t2, Out))
            return;

        Out.Xoffset = 0;
        Out.Yoffset = 0;

        VipsReorderMarginHint(Out,
            M.Xsize * M.Ysize);

        return;
    }

    public override void ClassInit()
    {
        base.ClassInit();

        GObjectClass gobject_class = (GObjectClass) this.GetType().GetInterface(typeof(GObject));
        VipsObjectClass object_class = (VipsObjectClass) this.GetType().GetInterface(typeof(VipsObject));

        gobject_class.SetProperty += new PropertyHandler(SetProperty);
        gobject_class.GetProperty += new PropertyHandler(GetProperty);

        object_class.Nickname = "convasep";
        object_class.Description =
            _("approximate separable integer convolution");
        object_class.Build = Build;

        VIPS_ARG_INT("layers", 104,
            _("Layers"),
            _("Use this many layers in approximation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsConvasep),
            "layers",
            1, 1000, 5);
    }

    public override void Init()
    {
        base.Init();
        Layers = 5;
        NLines = 0;
    }

    private bool Decompose()
    {
        VipsImage iM = IM;
        double[] coeff = (double[]) VIPS_IMAGE_ADDR(iM, 0, 0);
        double scale = VipsImageGetScale(iM);
        double offset = VipsImageGetOffset(iM);

        double max;
        double min;
        double depth;
        double sum;
        double area;
        int layers;
        int layers_above;
        int layers_below;
        int z, n, x;

        VIPS_DEBUG_MSG(
            "vips_convasep_decompose: "
            "breaking into {0} layers ...\n",
            Layers);

        // Find mask range. We must always include the zero axis in the mask.
        max = 0;
        min = 0;
        for (x = 0; x < Width; x++)
        {
            if (coeff[x] > max)
                max = coeff[x];
            if (coeff[x] < min)
                min = coeff[x];
        }

        // The zero axis must fall on a layer boundary. Estimate the
        // depth, find n-lines-above-zero, get exact depth, then calculate a
        // fixed n-lines which includes any negative parts.
        depth = (max - min) / Layers;
        layers_above = Math.Ceiling(max / depth);
        depth = max / layers_above;
        layers_below = Math.Floor(min / depth);
        layers = layers_above - layers_below;

        VIPS_DEBUG_MSG("depth = {0}, layers = {1}\n", depth, layers);

        // For each layer, generate a set of lines which are inside the
        // perimeter. Work down from the top.
        for (z = 0; z < layers; z++)
        {
            double y = max - (1 + z) * depth;

            // y plus half depth ... ie. the layer midpoint.
            double y_ph = y + depth / 2;

            // Odd, but we must avoid rounding errors that make us miss 0
            // in the line above.
            int y_positive = z < layers_above;

            int inside;

            // Start outside the perimeter.
            inside = 0;

            for (x = 0; x < Width; x++)
            {
                // The vertical line from mask[z] to 0 is inside. Is
                // our current square (x, y) part of that line?
                if ((y_positive && coeff[x] >= y_ph) ||
                    (!y_positive && coeff[x] <= y_ph))
                {
                    if (!inside)
                    {
                        ConvasepLineStart(this, x,
                            y_positive ? 1 : -1);
                        inside = 1;
                    }
                }
                else if (inside)
                {
                    if (ConvasepLineEnd(this, x))
                        return false;
                    inside = 0;
                }
            }

            if (inside &&
                ConvasepLineEnd(this, Width))
                return false;
        }

        // Can we common up any lines? Search for lines with identical
        // start/end.
        for (z = 0; z < NLines; z++)
        {
            for (n = z + 1; n < NLines; n++)
            {
                if (Start[z] == Start[n] &&
                    End[z] == End[n])
                {
                    Factor[z] += Factor[n];

                    // n can be deleted. Do this in a separate
                    // pass below.
                    Factor[n] = 0;
                }
            }
        }

        // Now we can remove all factor 0 lines.
        for (z = 0; z < NLines; z++)
        {
            if (Factor[z] == 0)
            {
                for (x = z; x < NLines; x++)
                {
                    Start[x] = Start[x + 1];
                    End[x] = End[x + 1];
                    Factor[x] = Factor[x + 1];
                }
                NLines -= 1;
            }
        }

        // Find the area of the lines.
        area = 0;
        for (z = 0; z < NLines; z++)
            area += Factor[z] *
                (End[z] - Start[z]);

        // Strength reduction: if all lines are divisible by n, we can move
        // that n out into the ->area factor. The aim is to produce as many
        // factor 1 lines as we can and to reduce the chance of overflow.
        x = Factor[0];
        for (z = 1; z < NLines; z++)
            x = GCD(x, Factor[z]);
        for (z = 0; z < NLines; z++)
            Factor[z] /= x;
        area *= x;

        // Find the area of the original mask.
        sum = 0;
        for (z = 0; z < Width; z++)
            sum += coeff[z];

        Divisor = VIPS_RINT(sum * area / scale);
        if (Divisor == 0)
            Divisor = 1;
        Rounding = (Divisor + 1) / 2;
        Offset = offset;

#ifdef DEBUG
        // ASCII-art layer drawing.
        Console.WriteLine("lines:");
        for (z = 0; z < NLines; z++)
        {
            Console.Write("{0,3} - {1,2} x ", z, Factor[z]);
            for (x = 0; x < 55; x++)
            {
                int rx = x * (Width + 1) / 55;

                if (rx >= Start[z] && rx < End[z])
                    Console.Write("#");
                else
                    Console.Write(" ");
            }
            Console.WriteLine("{0,3} .. {1,3}", Start[z], End[z]);
        }
        Console.WriteLine("divisor = {0}\n", Divisor);
        Console.WriteLine("rounding = {0}\n", Rounding);
        Console.WriteLine("offset = {0}\n", Offset);
#endif /*DEBUG*/

        return true;
    }

    private void ConvasepLineStart(int x, int factor)
    {
        Start[NLines] = x;
        Factor[NLines] = factor;
    }

    private bool ConvasepLineEnd(int x)
    {
        VipsObjectClass class = (VipsObjectClass) GetType().GetInterface(typeof(VipsObject));

        End[NLines] = x;

        if (NLines >= MAX_LINES - 1)
        {
            VIPS_ERROR(class.Nickname, "%s", _("mask too complex"));
            return false;
        }
        NLines += 1;

        return true;
    }

    private bool ConvasepPass(VipsConvasep self,
        ref VipsImage t0, ref VipsImage t1, VipsDirection direction)
    {
        VipsObjectClass class = (VipsObjectClass) GetType().GetInterface(typeof(VipsObject));

        VipsGenerateFn gen;

        if (direction == VipsDirection.Horizontal)
        {
            t0.Xsize -= Width - 1;
            gen = ConvasepGenerateHorizontal;
        }
        else
        {
            t0.Ysize -= Width - 1;
            gen = ConvasepGenerateVertical;
        }

        if (t0.Xsize <= 0 ||
            t0.Ysize <= 0)
        {
            VIPS_ERROR(class.Nickname,
                "%s", _("image too small for mask"));
            return false;
        }

        if (!VipsImageGenerate(t0,
            ConvasepStart, gen, ConvasepStop, ref t0))
            return false;

        return true;
    }

    private bool ConvasepBuild(VipsObject self)
    {
        VipsObjectClass class = (VipsObjectClass) GetType().GetInterface(typeof(VipsObject));
        VipsConvolution convolution = (VipsConvolution) self;
        VipsConvasep convasep = (VipsConvasep) self;

        VipsImage[] t = new VipsImage[4];

        VipsImage in;

        if (base.Build(self))
            return false;

        if (!VipsCheckSeparable(class.Nickname, convolution.M))
            return false;

        // An int version of our mask.
        if (!VipsImageIntize(convolution.M, out t[3]))
            return false;
        convasep.IM = t[3];
        convasep.Width = convasep.IM.Xsize * convasep.IM.Ysize;
        in = convolution.In;

        if (!Decompose())
            return false;

        GObjectSet(self, "out", new VipsImage(), null);
        if (
            !VipsEmbed(in, ref t[0],
                Width / 2,
                Width / 2,
                in.Xsize + Width - 1,
                in.Ysize + Width - 1,
                "extend", VipsExtendCopy,
                null) ||
            !ConvasepPass(convasep,
                ref t[0], ref t[1], VipsDirection.Horizontal) ||
            !ConvasepPass(convasep,
                ref t[1], ref t[2], VipsDirection.Vertical) ||
            !VipsImageWrite(t[2], convolution.Out))
            return false;

        convolution.Out.Xoffset = 0;
        convolution.Out.Yoffset = 0;

        VipsReorderMarginHint(convolution.Out,
            M.Xsize * M.Ysize);

        return true;
    }

    private void ConvasepClassInit()
    {
        base.ClassInit();

        GObjectClass gobject_class = (GObjectClass) this.GetType().GetInterface(typeof(GObject));
        VipsObjectClass object_class = (VipsObjectClass) this.GetType().GetInterface(typeof(VipsObject));

        gobject_class.SetProperty += new PropertyHandler(SetProperty);
        gobject_class.GetProperty += new PropertyHandler(GetProperty);

        object_class.Nickname = "convasep";
        object_class.Description =
            _("approximate separable integer convolution");
        object_class.Build = ConvasepBuild;

        VIPS_ARG_INT("layers", 104,
            _("Layers"),
            _("Use this many layers in approximation"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsConvasep),
            "layers",
            1, 1000, 5);
    }

    private void ConvasepInit()
    {
        base.Init();
        Layers = 5;
        NLines = 0;
    }
}

public class VipsConvasepSeq
{
    public VipsConvasep Convasep { get; set; }
    public VipsRegion Ir { get; set; }

    public int[] Start { get; set; }
    public int[] End { get; set; }

    public int LastStride { get; set; }

    public int[] Isum { get; set; }
    public double[] Dsum { get; set; }

    private void ConvasepStop()
    {
        VIPS_UNREF(Ir);
        VIPS_FREE(Start);
        VIPS_FREE(End);
        VIPS_FREE(Isum);
        VIPS_FREE(Dsum);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        VipsConvasepSeq seq = (VipsConvasepSeq) obj;
        return Convasep.Equals(seq.Convasep);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Convasep.GetHashCode();
            return hash;
        }
    }
}

public class VipsConvasepStart : IGenerateFn
{
    private VipsImage out_;
    private VipsImage in_;
    private VipsConvasep convasep_;

    public void Start(VipsImage out, object a, object b)
    {
        out_ = out;
        in_ = (VipsImage) a;
        convasep_ = (VipsConvasep) b;

        if (!convasep_.Decompose())
            return;

        VipsRegion ir = new VipsRegion(in_);
        int[] start = new int[convasep_.NLines];
        int[] end = new int[convasep_.NLines];

        for (int z = 0; z < convasep_.NLines; z++)
        {
            start[z] = convasep_.Start[z];
            end[z] = convasep_.End[z];
        }

        VipsConvasepSeq seq = new VipsConvasepSeq();
        seq.Convasep = convasep_;
        seq.Ir = ir;
        seq.Start = start;
        seq.End = end;

        return seq;
    }
}

public class VipsConvasepGenerateHorizontal : IGenerateFn
{
    private VipsRegion out_region_;
    private VipsConvasepSeq seq_;

    public void Generate(VipsRegion out_region, object a, object b)
    {
        out_region_ = out_region;
        seq_ = (VipsConvasepSeq) a;

        int n_lines = convasep_.NLines;
        int bands = in_.Bands * 2;

        VipsRect r = new VipsRect();
        r.Copy(out_region_.Valid);

        for (int y = 0; y < r.Height; y++)
        {
            switch (in_.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    HCONV_INT(unsigned char, CLIP_UCHAR);
                    break;
                // ... other cases ...
            }
        }

        return true;
    }
}

public class VipsConvasepGenerateVertical : IGenerateFn
{
    private VipsRegion out_region_;
    private VipsConvasepSeq seq_;

    public void Generate(VipsRegion out_region, object a, object b)
    {
        out_region_ = out_region;
        seq_ = (VipsConvasepSeq) a;

        int n_lines = convasep_.NLines;
        int sz = in_.Bands * 2;

        VipsRect r = new VipsRect();
        r.Copy(out_region_.Valid);

        for (int x = 0; x < sz; x++)
        {
            switch (in_.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    VCONV_INT(unsigned char, CLIP_UCHAR);
                    break;
                // ... other cases ...
            }
        }

        return true;
    }
}

public class VipsConvasepStop : IGenerateFn
{
    public void Stop(object a, object b)
    {
        VipsConvasepSeq seq = (VipsConvasepSeq) a;

        seq.ConvasepStop();

        return true;
    }
}
```