```csharp
// vips_unary_const_build

public int VipsUnaryConstBuild(VipsObject object)
{
    VipsArithmetic arithmetic = (VipsArithmetic)object;
    VipsUnary unary = (VipsUnary)object;
    VipsUnaryConst uconst = (VipsUnaryConst)object;

    // If we have a three-element vector we need to bandup the image to match.
    uconst.N = 1;
    if (uconst.C != null)
        uconst.N = Math.Max(uconst.N, uconst.C.Length);
    if (unary.In != null)
        uconst.N = Math.Max(uconst.N, unary.In.Bands);

    arithmetic.BaseBands = uconst.N;

    // Some operations need int constants, for example boolean AND, SHIFT etc.
    // Some can use int constants as an optimisation, for example (x < 12). It depends on the value though: obviously (x < 12.5) should not use the int form.

    if (uconst.C != null)
    {
        bool isComplex = VipsBandFormat.IsComplex(unary.In.BandFmt);
        int step = isComplex ? 2 : 1;
        int n = step * uconst.N;

        double[] cDouble = new double[n];
        int[] cInt = new int[n];

        for (int i = 0; i < n; i += step)
            cDouble[i] = uconst.C[VIPS.Min(i / step, uconst.C.Length - 1)];

        for (int i = 0; i < n; i += step)
            cInt[i] = (int)cDouble[i];

        uconst.IsInt = true;
        for (int i = 0; i < n; i += step)
            if (cInt[i] != cDouble[i])
            {
                uconst.IsInt = false;
                break;
            }
    }

    return VIPS_OBJECT_CLASS(VipsUnaryConstParentClass).Build(object);
}

// vips_unary_const_class_init

public class VipsUnaryConstClass : VipsObjectClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        // Some operations need int constants, for example boolean AND, SHIFT etc.
        // Some can use int constants as an optimisation, for example (x < 12). It depends on the value though: obviously (x < 12.5) should not use the int form.

        VipsArgBoxed arg = new VipsArgBoxed("c", 201,
            _("c"),
            _("Array of constants"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsUnaryConst, C),
            typeof(VipsArrayDouble));

        // If we have a three-element vector we need to bandup the image to match.
        arg.Nickname = "unary_const";
        arg.Description = _("unary operations with a constant");
    }
}

// vips_unary_const_init

public class VipsUnaryConst : VipsObject
{
}
```