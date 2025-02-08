Here is the C# code equivalent to the given C code:

```csharp
using System;

namespace Vips
{
    public class MaskIdealBand : Mask
    {
        private double _frequencyCutoffX;
        private double _frequencyCutoffY;
        private double _radius;

        public override double Point(double dx, double dy)
        {
            // vips_mask_ideal_band_point
            var idealBand = this as MaskIdealBand;
            var fcx = idealBand._frequencyCutoffX;
            var fcy = idealBand._frequencyCutoffY;
            var r2 = idealBand._radius * idealBand._radius;

            var d1 = (dx - fcx) * (dx - fcx) + (dy - fcy) * (dy - fcy);
            var d2 = (dx + fcx) * (dx + fcx) + (dy + fcy) * (dy + fcy);

            return (d1 < r2 || d2 < r2) ? 1.0 : 0.0;
        }
    }

    public class MaskIdealBandClass : MaskClass
    {
        // vips_mask_ideal_band_class_init
        public override void ClassInit()
        {
            base.ClassInit();

            this.Nickname = "mask_ideal_band";
            this.Description = "make an ideal band filter";

            this.Point = new Func<double, double, double>(this.Point);

            VIPS_ARG_DOUBLE(this, "frequency_cutoff_x", 6,
                "Frequency cutoff x",
                "Frequency cutoff x",
                ArgumentType.RequiredInput,
                typeof(MaskIdealBand).GetProperty("FrequencyCutoffX").GetOffset(),
                0.0, 1000000.0, 0.5);

            VIPS_ARG_DOUBLE(this, "frequency_cutoff_y", 7,
                "Frequency cutoff y",
                "Frequency cutoff y",
                ArgumentType.RequiredInput,
                typeof(MaskIdealBand).GetProperty("FrequencyCutoffY").GetOffset(),
                0.0, 1000000.0, 0.5);

            VIPS_ARG_DOUBLE(this, "radius", 8,
                "Radius",
                "Radius of circle",
                ArgumentType.RequiredInput,
                typeof(MaskIdealBand).GetProperty("Radius").GetOffset(),
                0.0, 1000000.0, 0.1);
        }
    }

    public class MaskIdealBandInit : VipsObjectInit
    {
        // vips_mask_ideal_band_init
        public override void Init()
        {
            base.Init();

            var idealBand = this as MaskIdealBand;
            idealBand._frequencyCutoffX = 0.5;
            idealBand._frequencyCutoffY = 0.5;
            idealBand._radius = 0.1;
        }
    }

    public class VipsMaskIdealBand : VipsObject
    {
        // vips_mask_ideal_band
        public static int Create(VipsImage[] out, int width, int height,
            double frequencyCutoffX, double frequencyCutoffY,
            double radius, params object[] args)
        {
            var result = VipsCallSplit("mask_ideal_band", args, out, width, height,
                frequencyCutoffX, frequencyCutoffY, radius);

            return result;
        }
    }
}
```

Note that I've assumed the existence of `VipsImage`, `Mask`, and other classes/types as they are not provided in the original code. Also, some methods like `vips_call_split` are not implemented here as their implementation is not provided in the original code.