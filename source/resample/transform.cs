Here is the converted C# code:

```csharp
// vips__transform_calc_inverse
public static int TransformCalcInverse(VipsTransformation trn)
{
    double det = trn.A * trn.D - trn.B * trn.C;

    if (Math.Abs(det) < 2.0 * Double.Epsilon)
    {
        // divisor is near zero
        throw new Exception("singular or near-singular matrix");
    }

    double tmp = 1.0 / det;

    trn.IA = tmp * trn.D;
    trn.IB = -tmp * trn.B;
    trn.IC = -tmp * trn.C;
    trn.ID = tmp * trn.A;

    return 0;
}

// vips__transform_init
public static void TransformInit(VipsTransformation trn)
{
    trn.OArea.Left = 0;
    trn.OArea.Top = 0;
    trn.OArea.Width = -1;
    trn.OArea.Height = -1;
    trn.IArea.Left = 0;
    trn.IArea.Top = 0;
    trn.IArea.Width = -1;
    trn.IArea.Height = -1;
    trn.A = 1.0; // Identity transform
    trn.B = 0.0;
    trn.C = 0.0;
    trn.D = 1.0;
    trn.Idx = 0.0;
    trn.Idy = 0.0;
    trn.Odx = 0.0;
    trn.Ody = 0.0;

    TransformCalcInverse(trn);
}

// vips__transform_isidentity
public static bool TransformIsIdentity(VipsTransformation trn)
{
    return (trn.A == 1.0 && trn.B == 0.0 &&
            trn.C == 0.0 && trn.D == 1.0 &&
            trn.Idx == 0.0 && trn.Idy == 0.0 &&
            trn.Odx == 0.0 && trn.Ody == 0.0);
}

// vips__transform_add
public static int TransformAdd(VipsTransformation in1, VipsTransformation in2, VipsTransformation out)
{
    out.A = in1.A * in2.A + in1.C * in2.B;
    out.B = in1.B * in2.A + in1.D * in2.B;
    out.C = in1.A * in2.C + in1.C * in2.D;
    out.D = in1.B * in2.C + in1.D * in2.D;

    // fixme: do idx/idy as well

    out.Odx = in1.Odx * in2.A + in1.Ody * in2.B + in2.Odx;
    out.Ody = in1.Odx * in2.C + in1.Ody * in2.D + in2.Ody;

    if (TransformCalcInverse(out))
        return -1;

    return 0;
}

// vips__transform_print
public static void TransformPrint(VipsTransformation trn)
{
    Console.WriteLine("vips__transform_print:");
    Console.WriteLine($" iarea: left={trn.IArea.Left}, top={trn.IArea.Top}, width={trn.IArea.Width}, height={trn.IArea.Height}");
    Console.WriteLine($" oarea: left={trn.OArea.Left}, top={trn.OArea.Top}, width={trn.OArea.Width}, height={trn.OArea.Height}");
    Console.WriteLine($" mat: a={trn.A}, b={trn.B}, c={trn.C}, d={trn.D}");
    Console.WriteLine($" off: odx={trn.Odx}, ody={trn.Ody}, idx={trn.Idx}, idy={trn.Idy}");
}

// vips__transform_forward_point
public static void TransformForwardPoint(VipsTransformation trn, double x, double y, out double ox, out double oy)
{
    x += trn.Idx;
    y += trn.Idy;

    ox = trn.A * x + trn.B * y + trn.Odx;
    oy = trn.C * x + trn.D * y + trn.Ody;
}

// vips__transform_invert_point
public static void TransformInvertPoint(VipsTransformation trn, double x, double y, out double ox, out double oy)
{
    x -= trn.Odx;
    y -= trn.Ody;

    ox = trn.IA * x + trn.IB * y - trn.Idx;
    oy = trn.IC * x + trn.ID * y - trn.Idy;
}

// transform_rect
private static void TransformRect(VipsTransformation trn, Func<VipsTransformation, double, double, out double, out double> transform, VipsRect inRect, out VipsRect outRect)
{
    double x1, y1; // Map corners
    double x2, y2;
    double x3, y3;
    double x4, y4;
    double left, right, top, bottom;

    // Map input VipsRect.
    transform(trn, inRect.Left, inRect.Top, out x1, out y1);
    transform(trn, inRect.Left, VipsRect.Bottom(inRect), out x3, out y3);
    transform(trn, VipsRect.Right(inRect), inRect.Top, out x2, out y2);
    transform(trn, VipsRect.Right(inRect), VipsRect.Bottom(inRect), out x4, out y4);

    // Find bounding box for these four corners. Round-to-nearest to try
    // to stop rounding errors growing images.
    left = Math.Min(x1, Math.Min(x2, Math.Min(x3, x4)));
    right = Math.Max(x1, Math.Max(x2, Math.Max(x3, x4)));
    top = Math.Min(y1, Math.Min(y2, Math.Min(y3, y4)));
    bottom = Math.Max(y1, Math.Max(y2, Math.Max(y3, y4)));

    outRect.Left = (int)Math.Round(left);
    outRect.Top = (int)Math.Round(top);
    outRect.Width = (int)Math.Round(right - left);
    outRect.Height = (int)Math.Round(bottom - top);
}

// vips__transform_forward_rect
public static void TransformForwardRect(VipsTransformation trn, VipsRect inRect, out VipsRect outRect)
{
    TransformRect(trn, (trn) => TransformForwardPoint(trn, 0, 0), inRect, out outRect);
}

// vips__transform_invert_rect
public static void TransformInvertRect(VipsTransformation trn, VipsRect inRect, out VipsRect outRect)
{
    TransformRect(trn, (trn) => TransformInvertPoint(trn, 0, 0), inRect, out outRect);
}

// vips__transform_set_area
public static void TransformSetArea(VipsTransformation trn)
{
    TransformForwardRect(trn, ref trn.IArea, out trn.OArea);
}
```

Note that I've used the `Math` class for mathematical operations and the `Console` class for printing. I've also replaced the `VIPS_MIN`, `VIPS_MAX`, and `VIPS_ROUND_INT` macros with their corresponding C# equivalents. Additionally, I've assumed that the `VipsRect` struct has been defined elsewhere in your codebase. If not, you'll need to define it as well.

Also note that I've used the `out` keyword for method parameters where a value needs to be returned from the method. This is a common pattern in C# when working with methods that return multiple values.