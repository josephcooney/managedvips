Here is the C# code equivalent to the provided C code:

```csharp
using System;
using Vips;

public class GaussMat : Create
{
    public double Sigma { get; set; }
    public double MinAmpl { get; set; }
    public bool Separable { get; set; }
    public Precision Precision { get; set; }

    public GaussMat()
    {
        Sigma = 1;
        MinAmpl = 0.1;
        Precision = Precision.Integer;
    }

    protected override int Build(VipsObject obj)
    {
        VipsObjectClass classObj = (VipsObjectClass)VipsObject.GetClass(obj);
        VipsCreate create = (VipsCreate)obj;
        GaussMat gaussmat = (GaussMat)obj;

        double sig2 = 2 * gaussmat.Sigma * gaussmat.Sigma;
        int max_x = Math.Min(8 * gaussmat.Sigma, MASK_SANITY);

        int x, y;
        int width, height;
        double sum;

        if (base.Build(obj) != 0)
            return -1;

        // The old, deprecated @integer property has been deliberately set to
        // FALSE and they've not used the new @precision property ... switch
        // to float to help them out.
        if (VipsObject.ArgumentIsSet(obj, "integer") &&
            !VipsObject.ArgumentIsSet(obj, "precision") &&
            !gaussmat.Integer)
            gaussmat.Precision = Precision.Float;

        // Find the size of the mask. Limit the mask size to 10k x 10k for
        // sanity. We allow x == 0, meaning a 1x1 mask.
        for (x = 0; x < max_x; x++)
        {
            double v = Math.Exp(-((double)(x * x)) / sig2);

            if (v < gaussmat.MinAmpl)
                break;
        }
        if (x >= MASK_SANITY)
        {
            Vips.Error(classObj.Nickname, "%s", _("mask too large"));
            return -1;
        }
        width = 2 * Math.Max(x - 1, 0) + 1;
        height = gaussmat.Separable ? 1 : width;

        Vips.ImageInitFields(create.Out,
            width, height, 1,
            Vips.Format.Double, Vips.Coding.None,
            Vips.Interpretation.Multiband,
            1.0, 1.0);
        if (Vips.ImagePipelinev(create.Out, Vips.DemandStyle.Any, null) ||
            Vips.ImageWritePrepare(create.Out))
            return -1;

        sum = 0;
        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                int xo = x - width / 2;
                int yo = y - height / 2;
                double distance = xo * xo + yo * yo;
                double v = Math.Exp(-distance / sig2);

                if (gaussmat.Precision != Precision.Integer)
                    v = Vips.Rint(20 * v);

                Vips.Matrix(create.Out, x, y) = v;
                sum += v;
            }
        }

        // Make sure we can't make sum == 0: it'd certainly cause /0 later.
        if (sum == 0)
            sum = 1;

        Vips.ImageSetDouble(create.Out, "scale", sum);
        Vips.ImageSetDouble(create.Out, "offset", 0.0);

        return 0;
    }

    public static void ClassInit(VipsGaussmatClass classObj)
    {
        GObjectClass gobjectClass = (GObjectClass)classObj;
        VipsObjectClass vobjectClass = (VipsObjectClass)classObj;

        gobjectClass.SetProperty = Vips.ObjectSetProperty;
        gobjectClass.GetProperty = Vips.ObjectGetProperty;

        vobjectClass.Nickname = "gaussmat";
        vobjectClass.Description = _("make a gaussian image");
        vobjectClass.Build = Build;

        Vips.ArgDouble(classObj, "sigma", 2,
            _("Sigma"),
            _("Sigma of Gaussian"),
            Vips.Argument.RequiredInput,
            typeof(GaussMat).GetField("sigma").Offset,
            0.000001, 10000.0, 1.0);

        Vips.ArgDouble(classObj, "min_ampl", 3,
            _("Minimum amplitude"),
            _("Minimum amplitude of Gaussian"),
            Vips.Argument.RequiredInput,
            typeof(GaussMat).GetField("min_ampl").Offset,
            0.000001, 10000.0, 0.1);

        Vips.ArgBool(classObj, "separable", 4,
            _("Separable"),
            _("Generate separable Gaussian"),
            Vips.Argument.OptionalInput,
            typeof(GaussMat).GetField("separable").Offset,
            false);

        Vips.ArgBool(classObj, "integer", 5,
            _("Integer"),
            _("Generate integer Gaussian"),
            Vips.Argument.OptionalInput | Vips.Argument.Deprecated,
            typeof(GaussMat).GetField("integer").Offset,
            false);

        Vips.ArgEnum(classObj, "precision", 6,
            _("Precision"),
            _("Generate with this precision"),
            Vips.Argument.OptionalInput,
            typeof(GaussMat).GetField("precision").Offset,
            typeof(Precision));
    }

    public static void Init(VipsGaussmat gaussmat)
    {
        gaussmat.Sigma = 1;
        gaussmat.MinAmpl = 0.1;
        gaussmat.Precision = Precision.Integer;
    }
}

public class VipsGaussmat : Create
{
    public double Sigma { get; set; }
    public double MinAmpl { get; set; }
    public bool Separable { get; set; }
    public Precision Precision { get; set; }

    public static int GaussMat(VipsImage[] out, double sigma, double min_ampl)
    {
        VipsObject obj = new VipsObject();
        VipsCreate create = new VipsCreate(obj);
        GaussMat gaussmat = new GaussMat();

        gaussmat.Sigma = sigma;
        gaussmat.MinAmpl = min_ampl;

        if (gaussmat.Build(create) != 0)
            return -1;

        return 0;
    }
}
```

Note that I've assumed the `Vips` namespace is already defined elsewhere in your codebase, and that you have the necessary VIPS library installed. Also, some types like `Precision`, `VipsObjectClass`, `GObjectClass`, etc., are not defined here as they are part of the VIPS library.