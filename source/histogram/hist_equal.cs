Here is the converted C# code:

```csharp
using System;

namespace VIPS {
    public class HistEqual : Operation {
        public Image In { get; set; }
        public Image Out { get; set; }

        // -1 for all bands, or the band we scan.
        public int Which { get; set; }

        public override int Build() {
            var t = new Image[5];
            Out = new Image();

            if (base.Build())
                return -1;

            // norm can return a uchar output for a ushort input if the range is
            // small, so make sure we cast back to the input type again.
            if (HistFind(In, ref t[0], "band", Which) ||
                HistCum(t[0], ref t[1]) ||
                HistNorm(t[1], ref t[2]) ||
                Cast(t[2], ref t[3], In.BandFmt) ||
                MapLut(In, ref t[4], t[3]) ||
                ImageWrite(t[4], Out))
                return -1;

            return 0;
        }
    }

    public class HistEqualClass : OperationClass {
        public override void ClassInit() {
            base.ClassInit();

            // nickname
            Nickname = "hist_equal";
            Description = "histogram equalisation";

            // properties
            AddProperty("in", typeof(Image), 1, "Input image");
            AddProperty("out", typeof(Image), 2, "Output image");

            // arguments
            AddArgument("band", typeof(int), 110, "Equalise with this band");
        }
    }

    public class HistEqualInit : OperationInit {
        public override void Init() {
            Which = -1;
        }
    }

    public static class VipsHistEqual {
        public static int HistEqual(Image inImage, out Image outImage, params object[] args) {
            var result = CallSplit("hist_equal", inImage, outImage, args);
            return result;
        }
    }
}
```

Note that I've assumed the existence of `VIPS` namespace and classes like `Operation`, `Image`, etc. as they are not provided in the original C code. Also, some methods like `HistFind`, `HistCum`, `HistNorm`, `Cast`, `MapLut`, `ImageWrite`, `CallSplit` are assumed to be implemented elsewhere in the VIPS library.