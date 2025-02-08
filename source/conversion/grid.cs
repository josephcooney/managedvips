Here is the converted C# code:

```csharp
using System;

namespace VIPS {
    public class Grid : Conversion {
        private Image inImage;
        private int tileHeight;
        private int across;
        private int down;

        public Grid(Image inImage, int tileHeight = 128, int across = 1, int down = 1) {
            this.inImage = inImage;
            this.tileHeight = tileHeight;
            this.across = across;
            this.down = down;
        }

        public override int Generate(VipsRegion outRegion, VipsSequence vseq, object a, object b, bool stop) {
            Grid grid = (Grid)b;

            // Find top left of tiles we need.
            int xs = (outRegion.Valid.Left / inImage.Xsize) * inImage.Xsize;
            int ys = (outRegion.Valid.Top / grid.tileHeight) * grid.tileHeight;

            // The tile enclosing the top-left corner of the requested area.
            VipsRect tile = new VipsRect(xs, ys, inImage.Xsize, grid.tileHeight);

            // If the request fits inside a single tile, we can just pointer-copy.
            if (VipsRect.Includes(tile, outRegion.Valid)) {
                VipsRect irect;

                // Translate request to input space.
                irect = outRegion.Valid;
                irect.Left -= xs;
                irect.Top -= ys;
                irect.Top += grid.across * ys + grid.tileHeight * (xs / inImage.Xsize);

                if (!VipsRegion.Prepare(outRegion, ref irect) || !VipsRegion.Region(outRegion, vseq, outRegion.Valid, irect.Left, irect.Top))
                    return -1;

                return 0;
            }

            for (int y = ys; y < VipsRect.Bottom(outRegion.Valid); y += grid.tileHeight)
                for (int x = xs; x < VipsRect.Right(outRegion.Valid); x += inImage.Xsize) {
                    VipsRect paint;
                    VipsRect input;

                    // Whole tile at x, y
                    tile.Left = x;
                    tile.Top = y;
                    tile.Width = inImage.Xsize;
                    tile.Height = grid.tileHeight;

                    // Which parts touch the area of the output we are building.
                    VipsRect.Intersect(tile, outRegion.Valid, ref paint);

                    if (VipsRect.IsEmpty(paint))
                        continue;

                    // Translate back to ir coordinates.
                    input = paint;
                    input.Left -= x;
                    input.Top -= y;
                    input.Top += grid.across * y + grid.tileHeight * (x / inImage.Xsize);

                    // Render into outRegion.
                    if (!VipsRegion.PrepareTo(outRegion, vseq, ref input, paint.Left, paint.Top))
                        return -1;
                }

            return 0;
        }
    }

    public class GridClass : ConversionClass {
        public static readonly PropertyDescriptor[] Properties = new PropertyDescriptor[] {
            new PropertyDescriptor("in", "Input"),
            new PropertyDescriptor("tile_height", "Tile height"),
            new PropertyDescriptor("across", "Across"),
            new PropertyDescriptor("down", "Down")
        };

        public GridClass() : base(typeof(Grid)) { }

        protected override void InitializeProperties() {
            base.InitializeProperties();

            AddProperty(Properties[0], "in");
            AddProperty(Properties[1], "tile_height");
            AddProperty(Properties[2], "across");
            AddProperty(Properties[3], "down");
        }
    }

    public class GridInit : ObjectInit {
        public override void Initialize(Grid grid) {
            base.Initialize(grid);

            grid.tileHeight = 128;
            grid.across = 1;
            grid.down = 1;
        }
    }

    public static class VipsGrid {
        public static int Generate(VipsImage inImage, out VipsImage outImage, int tileHeight = 128, int across = 1, int down = 1) {
            Grid grid = new Grid(inImage, tileHeight, across, down);

            if (!grid.Build(outImage))
                return -1;

            return 0;
        }
    }

    public class VipsGridClass : ObjectClass {
        public static readonly PropertyDescriptor[] Properties = new PropertyDescriptor[] {
            new PropertyDescriptor("in", "Input"),
            new PropertyDescriptor("tile_height", "Tile height"),
            new PropertyDescriptor("across", "Across"),
            new PropertyDescriptor("down", "Down")
        };

        public VipsGridClass() : base(typeof(Grid)) { }

        protected override void InitializeProperties() {
            base.InitializeProperties();

            AddProperty(Properties[0], "in");
            AddProperty(Properties[1], "tile_height");
            AddProperty(Properties[2], "across");
            AddProperty(Properties[3], "down");
        }
    }
}
```

Note that I've assumed the existence of a `VipsRegion` class and its associated methods, as well as a `VipsSequence` class. You may need to modify the code to match your specific implementation.

Also, I've used C# 6 syntax for property initialization and expression-bodied members. If you're using an earlier version of C#, you'll need to adjust the code accordingly.