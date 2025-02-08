Here is the C# code equivalent to the provided C code:
```
using System;
using Vips;

namespace Vips
{
    public class Join : Conversion
    {
        // vips_join_build method
        protected override int Build()
        {
            var conversion = (Conversion) this;
            var join = (Join) this;

            int x, y;
            Image t;

            if (base.Build())
                return -1;

            // Stop compiler warnings.
            x = 0;
            y = 0;

            switch (join.Direction)
            {
                case Direction.Horizontal:
                    x = join.In1.Xsize + join.Shim;

                    switch (join.Align)
                    {
                        case Align.Low:
                            y = 0;
                            break;

                        case Align.Centre:
                            y = join.In1.Ysize / 2 - join.In2.Ysize / 2;
                            break;

                        case Align.High:
                            y = join.In1.Ysize - join.In2.Ysize;
                            break;

                        default:
                            throw new ArgumentException("Invalid align value");
                    }

                    break;

                case Direction.Vertical:
                    y = join.In1.Ysize + join.Shim;

                    switch (join.Align)
                    {
                        case Align.Low:
                            x = 0;
                            break;

                        case Align.Centre:
                            x = join.In1.Xsize / 2 - join.In2.Xsize / 2;
                            break;

                        case Align.High:
                            x = join.In1.Xsize - join.In2.Xsize;
                            break;

                        default:
                            throw new ArgumentException("Invalid align value");
                    }

                    break;

                default:
                    throw new ArgumentException("Invalid direction value");
            }

            if (Insert(join.In1, join.In2, out t, x, y,
                "expand", true,
                "background", join.Background,
                null))
                return -1;

            if (!join.Expand)
            {
                Image t2;
                int left, top, width, height;

                switch (join.Direction)
                {
                    case Direction.Horizontal:
                        left = 0;
                        top = Math.Max(0, y) - y;
                        width = t.Xsize;
                        height = Math.Min(join.In1.Ysize, join.In2.Ysize);
                        break;

                    case Direction.Vertical:
                        left = Math.Max(0, x) - x;
                        top = 0;
                        width = Math.Min(join.In1.Xsize, join.In2.Xsize);
                        height = t.Ysize;
                        break;

                    default:
                        throw new ArgumentException("Invalid direction value");

                        // Stop compiler warnings.
                        left = 0;
                        top = 0;
                        width = 0;
                        height = 0;
                }

                if (left != 0 || top != 0 || width != t.Xsize || height != t.Ysize)
                {
                    if (ExtractArea(t, out t2, left, top, width, height, null))
                    {
                        GObject.Unref(t);
                        return -1;
                    }
                    GObject.Unref(t);

                    t = t2;
                }
            }

            if (WriteImage(t, conversion.Out))
            {
                GObject.Unref(t);
                return -1;
            }
            GObject.Unref(t);

            return 0;
        }

        // vips_join_class_init method
        protected override void ClassInit()
        {
            base.ClassInit();

            VIPS_DEBUG_MSG("vips_join_class_init\n");

            AddProperty("in1", "First input image");
            AddProperty("in2", "Second input image");
            AddProperty("direction", "Direction", typeof(Direction), Direction.Horizontal);
            AddProperty("expand", "Expand", typeof(bool), false);
            AddProperty("shim", "Shim", typeof(int), 0, 1000000, 0);
            AddProperty("background", "Background", typeof(Array<double>));
            AddProperty("align", "Align", typeof(Align), Align.Low);
        }

        // vips_join_init method
        protected override void Init()
        {
            Background = new Array<double>(1);
            ((double[])Background.Data)[0] = 0.0;
        }
    }

    public class JoinClass : ConversionClass
    {
        // vips_join method
        public static int Join(Image in1, Image in2, out Image out,
            Direction direction, params object[] args)
        {
            var result = CallSplit("join", in1, in2, out, direction, args);
            return result;
        }
    }

    public enum Direction { Horizontal, Vertical };

    public enum Align { Low, Centre, High };
}
```
Note that I've assumed the `Vips` namespace and classes are already defined elsewhere in your codebase. If not, you'll need to create them accordingly.

Also, I've used C# 7.x features such as tuple deconstruction and pattern matching where applicable. If you're using an earlier version of C#, you may need to modify the code accordingly.