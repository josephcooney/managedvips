Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class LogMat : Create
{
    public double Sigma { get; set; }
    public double MinAmpl { get; set; }
    public bool Separable { get; set; }
    public Precision Precision { get; set; }

    public override int Build()
    {
        // The old, deprecated @integer property has been deliberately set to
        // FALSE and they've not used the new @precision property ... switch
        // to float to help them out.
        if (HasArgument("integer") && !HasArgument("precision"))
            Precision = Precision.Float;

        // Find the size of the mask. We want to eval the mask out to the
        // flat zero part, ie. beyond the minimum and to the point where it
        // comes back up towards zero.
        double last = 0;
        for (int x = 0; x < 5000; x++)
        {
            const double distance = x * x;
            double val;

            // Handbook of Pattern Recognition and image processing
            // by Young and Fu AP 1986 pp 220-221
            // temp =  (1.0 / (2.0 * IM_PI * sig4)) *
            //	(2.0 - (distance / sig2)) *
            //	exp((-1.0) * distance / (2.0 * sig2))

            val = 0.5 *
                (2.0 - (distance / (Sigma * Sigma))) *
                Math.Exp(-distance / (2.0 * (Sigma * Sigma)));

            // Stop when change in value (ie. difference from the last
            // point) is positive (ie. we are going up) and absolute value
            // is less than the min.
            if (val - last >= 0 && Math.Abs(val) < MinAmpl)
                break;

            last = val;
        }
        if (x == 5000)
        {
            throw new ArgumentException("mask too large");
        }

        int width = x * 2 + 1;
        int height = Separable ? 1 : width;

        Image image = Create.Out;
        image.Init(width, height, 1, Format.Double, Coding.None, Interpretation.Multiband);
        if (image.Pipeline() || image.WritePrepare())
            return -1;

        double sum = 0.0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int xo = x - width / 2;
                int yo = y - height / 2;
                double distance = xo * xo + yo * yo;
                double v = 0.5 *
                    (2.0 - (distance / (Sigma * Sigma))) *
                    Math.Exp(-distance / (2.0 * (Sigma * Sigma)));

                if (Precision == Precision.Integer)
                    v = Rint(20 * v);

                image[x, y] = v;
                sum += v;
            }
        }

        image.SetDouble("scale", sum);
        image.SetDouble("offset", 0.0);

        return 0;
    }

    public static int LogMat(VipsImage outImage, double sigma, double minAmpl, params object[] args)
    {
        Create create = new Create();
        create.Out = outImage;

        // Set default values
        create.Sigma = sigma;
        create.MinAmpl = minAmpl;
        create.Precision = Precision.Integer;

        // Parse arguments
        foreach (object arg in args)
        {
            if (arg is string && arg.ToString() == "separable")
                create.Separable = true;
            else if (arg is string && arg.ToString() == "precision" && args.Length > 1)
                create.Precision = (Precision)args[1];
            // Add other argument parsing logic here
        }

        return create.Build();
    }
}
```

Note that I've assumed the `VipsDotNet` namespace and classes are available, as they are not provided in the original C code. You may need to modify the code to match your specific VIPS implementation.

Also, I've left out some argument parsing logic for brevity, you should add it according to your needs.

This code defines a `LogMat` class that inherits from `Create`. The `Build()` method is equivalent to the C `vips_logmat_build()` function. The `LogMat()` method is equivalent to the C `vips_logmat()` function and creates an instance of the `LogMat` class with default values, parses arguments, and calls the `Build()` method.

Please note that this code is not tested and may require modifications to work correctly in your specific environment.