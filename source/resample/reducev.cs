Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

public class VipsReducev : VipsResample
{
    public double VShrink { get; set; }
    public double Gap { get; set; }
    public VipsKernel Kernel { get; set; }

    private int nPoint;
    private short[] matrixs;
    private double[] matrixf;
#ifdef HAVE_ORC
    private int[] matrixo;

    private OrcProgram[] passes;
#endif

    public VipsReducev()
    {
        Gap = 0.0;
        Kernel = new VipsKernel(VIPS_KERNEL_LANCZOS3);
    }

    protected override int Build(VipsObject obj)
    {
        var objectClass = (VipsObjectClass)VipsObject.GetClass(obj);
        var resample = (VipsResample)obj;
        var reducev = this;

        if (base.Build(obj))
            return -1;

        var inImage = resample.In;

        if (reducev.VShrink < 1.0)
        {
            throw new ArgumentException("Reduce factor should be >= 1.0");
        }

        int height = (int)Math.Round((double)inImage.Ysize / reducev.VShrink);

        double extraPixels = height * reducev.VShrink - inImage.Ysize;

        if (reducev.Gap > 0.0 && reducev.Kernel != VIPS_KERNEL_NEAREST)
        {
            if (reducev.Gap < 1.0)
            {
                throw new ArgumentException("Reduce gap should be >= 1.0");
            }

            int intVShrink = Math.Max(1, (int)Math.Floor((double)inImage.Ysize / height / reducev.Gap));

            if (intVShrink > 1)
            {
                var t = VipsImage.New();
                if (!VipsShrinkv(inImage, ref t, intVShrink, "ceil", true))
                    return -1;
                inImage = t;

                reducev.VShrink /= intVShrink;
                extraPixels /= intVShrink;
            }
        }

        if (reducev.VShrink == 1.0)
            return VipsImage.Write(inImage, resample.Out);

        nPoint = VipsReduce.GetPoints(reducev.Kernel, reducev.VShrink);
        Console.WriteLine("Reducev: " + nPoint + " point mask");

        if (nPoint > MAX_POINT)
        {
            throw new ArgumentException("Reduce factor too large");
        }

        reducev.VOffset = (1 + extraPixels) / 2.0 - 1;

        for (int y = 0; y < VIPS_TRANSFORM_SCALE + 1; y++)
        {
            matrixf[y] = new double[nPoint];
            matrixs[y] = new short[nPoint];

            if (!VipsReduce.MakeMask(matrixf[y], reducev.Kernel, nPoint, reducev.VShrink, (float)y / VIPS_TRANSFORM_SCALE))
                return -1;

            for (int i = 0; i < nPoint; i++)
                matrixs[y][i] = (short)(matrixf[y][i] * VIPS_INTERPOLATE_SCALE);
        }

#ifdef HAVE_ORC
        passes = new OrcProgram[MAX_PASS];

        if (!VipsReduce.Compile(reducev))
            return -1;

        for (int y = 0; y < VIPS_TRANSFORM_SCALE + 1; y++)
        {
            matrixo[y] = new int[nPoint];
            if (!VipsReduceVectorToFixedPoint(matrixf[y], matrixo[y], nPoint, 64))
                return -1;
        }
#endif

        var t = VipsImage.New();
        if (VipsImage.Decode(inImage, ref t))
            return -1;

        inImage = t;

        if (VipsEmbed(inImage, ref t, 0, Math.Max(0, nPoint / 2) - 1, inImage.Xsize, inImage.Ysize + nPoint, "extend", VIPS_EXTEND_COPY))
            return -1;

        inImage = t;

#ifdef HAVE_HWY
        if (inImage.BandFmt == VIPS_FORMAT_UCHAR && VipsVector.IsEnabled())
        {
            Console.WriteLine("Reducev: using vector path");
            var generate = VipsReducevUcharVectorGen;
        }
        else
#elif defined(HAVE_ORC)
        if (inImage.BandFmt == VIPS_FORMAT_UCHAR && VipsVector.IsEnabled() && !VipsReduce.Compile(reducev))
        {
            Console.WriteLine("Reducev: using vector path");

            for (int y = 0; y < VIPS_TRANSFORM_SCALE + 1; y++)
            {
                matrixo[y] = new int[nPoint];
                if (!VipsReduceVectorToFixedPoint(matrixf[y], matrixo[y], nPoint, 64))
                    return -1;
            }
        }
        else
#endif
        {
            Console.WriteLine("Reducev: using C path");
            var generate = VipsReducevGen;
        }

        t = VipsImage.New();
        if (VipsImage.Pipelinev(t, VIPS_DEMAND_STYLE_FATSTRIP, inImage))
            return -1;

        t.Ysize = height;
        if (t.Ysize <= 0)
        {
            throw new ArgumentException("Image has shrunk to nothing");
        }

        Console.WriteLine("Reducev: reducing " + inImage.Xsize + " x " + inImage.Ysize + " image to " + t.Xsize + " x " + t.Ysize);

        if (VipsImage.Generate(t, VipsReducevStart, generate, VipsReducevStop, inImage))
            return -1;

        inImage = t;

        VipsReorderMarginHint(inImage, nPoint);

#ifdef HAVE_HWY
        var sequential = new VipsSequential();
        if (!sequential.IsSequental(inImage))
        {
            Console.WriteLine("Reducev: not using sequential line cache");
        }
        else
        {
            Console.WriteLine("Reducev: using sequential line cache");

            t = VipsImage.New();
            if (sequential.Sequential(inImage, ref t, "tile_height", 10))
                return -1;
            inImage = t;
        }
#endif

        if (!VipsImage.Write(inImage, resample.Out))
            return -1;

        return 0;
    }

    protected override void ClassInit(VipsObjectClass objectClass)
    {
        var gobjectClass = (GObjectClass)objectClass;
        var vobjectClass = (VipsObjectClass)objectClass;
        var operationClass = (VipsOperationClass)objectClass;

        VIPS_DEBUG_MSG("VipsReducev.ClassInit\n");

#ifdef HAVE_ORC
        gobjectClass.Finalize = VipsReducevFinalize;
#endif

        gobjectClass.SetProperty = VipsObject.SetProperty;
        gobjectClass.GetProperty = VipsObject.GetProperty;

        vobjectClass.Nickname = "reducev";
        vobjectClass.Description = "Shrink an image vertically";
        vobjectClass.Build = Build;

        operationClass.Flags = VIPS_OPERATION_SEQUENTIAL;

        VipsArgDouble(reducev, "VShrink", 3, "Vertical shrink factor");
        VipsArgEnum(reducev, "Kernel", 4, "Resampling kernel");
        VipsArgDouble(reducev, "Gap", 5, "Reducing gap");

        // Old name.
        VipsArgDouble(reducev, "YShrink", 3, "Vertical shrink factor");
    }

    protected override void Init(VipsObject obj)
    {
        Gap = 0.0;
        Kernel = new VipsKernel(VIPS_KERNEL_LANCZOS3);
    }
}

public class VipsReducevSequence : VipsRegion
{
    public VipsReducev Reducev { get; set; }

#ifdef HAVE_ORC
    private short[] t1;
    private short[] t2;
#endif

    public VipsReducevSequence(VipsImage inImage, VipsReducev reducev)
        : base(inImage)
    {
        Reducev = reducev;

        if (!VipsRegion.New(this))
            return;

#ifdef HAVE_ORC
        t1 = new short[VIPS_IMAGE_N_ELEMENTS(inImage)];
        t2 = new short[VIPS_IMAGE_N_ELEMENTS(inImage)];

        if (reducev.NPass > 0)
        {
            if (!t1 || !t2)
                return;
        }
#endif

        Ir = VipsRegion.New(inImage);
    }

    public override void Dispose()
    {
        base.Dispose();

#ifdef HAVE_ORC
        VIPS_FREE(t1);
        VIPS_FREE(t2);
#endif

        VIPS_UNREF(Ir);
    }
}

public class VipsReducevGen : VipsGenerateFn
{
    public int Gen(VipsRegion outRegion, object vseq, object a, object b, ref bool stop)
    {
        var inImage = (VipsImage)a;
        var reducev = (VipsReducev)b;
        var seq = (VipsReducevSequence)vseq;

        var ir = seq.Ir;
        var r = outRegion.Valid;

        int ne = r.Width * inImage.Bands;

        VipsRect s;

#ifdef DEBUG
        Console.WriteLine("VipsReducevGen: generating " + r.Width + " x " + r.Height + " at " + r.Left + " x " + r.Top);
#endif

        s.Left = r.Left;
        s.Top = r.Top * reducev.VShrink - reducev.VOffset;
        s.Width = r.Width;
        s.Height = r.Height * reducev.VShrink + reducev.NPoint;

        if (!VipsRegion.Prepare(ir, ref s))
            return -1;

        VIPS_GATE_START("VipsReducevGen: work");

        double Y = (r.Top + 0.5) * reducev.VShrink - 0.5 - reducev.VOffset;

        for (int y = 0; y < r.Height; y++)
        {
            var q = VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);
            int py = (int)Y;
            var p = VIPS_REGION_ADDR(ir, r.Left, py);

            switch (inImage.BandFmt)
            {
                case VIPS_FORMAT_UCHAR:
                    ReducevUnsignedIntTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);
                    break;

                case VIPS_FORMAT_CHAR:
                    ReducevSignedIntTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);
                    break;

                case VIPS_FORMAT_USHORT:
                    ReducevUnsignedIntTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);
                    break;

                case VIPS_FORMAT_SHORT:
                    ReducevSignedIntTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);
                    break;

                case VIPS_FORMAT_UINT:
                    ReducevUnsignedIntTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);
                    break;

                case VIPS_FORMAT_INT:
                    ReducevSignedIntTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);
                    break;

                case VIPS_FORMAT_FLOAT:
                case VIPS_FORMAT_COMPLEX:
                    ReducevFloatTab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixf[(int)Y]);
                    break;

                case VIPS_FORMAT_DPCOMPLEX:
                case VIPS_FORMAT_DOUBLE:
                    ReducevNotab(reducev, q, p, ne, VIPS_REGION_LSKIP(ir), Y - py);
                    break;

                default:
                    throw new ArgumentException();
            }

            Y += reducev.VShrink;
        }

        VIPS_GATE_STOP("VipsReducevGen: work");

        VIPS_COUNT_PIXELS(outRegion, "VipsReducevGen");

        return 0;
    }
}

public class VipsReducevUcharVectorGen : VipsGenerateFn
{
    public int Gen(VipsRegion outRegion, object vseq, object a, object b, ref bool stop)
    {
        var inImage = (VipsImage)a;
        var reducev = (VipsReducev)b;
        var seq = (VipsReducevSequence)vseq;

        var ir = seq.Ir;
        var r = outRegion.Valid;

        int ne = r.Width * inImage.Bands;

        VipsRect s;

#ifdef DEBUG
        Console.WriteLine("VipsReducevUcharVectorGen: generating " + r.Width + " x " + r.Height + " at " + r.Left + " x " + r.Top);
#endif

        s.Left = r.Left;
        s.Top = r.Top * reducev.VShrink - reducev.VOffset;
        s.Width = r.Width;
        s.Height = r.Height * reducev.VShrink + reducev.NPoint;

        if (!VipsRegion.Prepare(ir, ref s))
            return -1;

        VIPS_GATE_START("VipsReducevUcharVectorGen: work");

        double Y = (r.Top + 0.5) * reducev.VShrink - 0.5 - reducev.VOffset;

        for (int y = 0; y < r.Height; y++)
        {
            var q = VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);
            int py = (int)Y;
            var p = VIPS_REGION_ADDR(ir, r.Left, py);

            VipsReducevUcharHwy(q, p, reducev.NPoint, ne, VIPS_REGION_LSKIP(ir), reducev.Matrixs[(int)Y]);

            Y += reducev.VShrink;
        }

        VIPS_GATE_STOP("VipsReducevUcharVectorGen: work");

        VIPS_COUNT_PIXELS(outRegion, "VipsReducevUcharVectorGen");

        return 0;
    }
}

public class VipsReducevVectorGen : VipsGenerateFn
{
    public int Gen(VipsRegion outRegion, object vseq, object a, object b, ref bool stop)
    {
        var inImage = (VipsImage)a;
        var reducev = (VipsReducev)b;
        var seq = (VipsReducevSequence)vseq;

        var ir = seq.Ir;
        var r = outRegion.Valid;

        int ne = r.Width * inImage.Bands;

        VipsRect s;

#ifdef DEBUG
        Console.WriteLine("VipsReducevVectorGen: generating " + r.Width + " x " + r.Height + " at " + r.Left + " x " + r.Top);
#endif

        s.Left = r.Left;
        s.Top = r.Top * reducev.VShrink - reducev.VOffset;
        s.Width = r.Width;
        s.Height = r.Height * reducev.VShrink + reducev.NPoint;

        if (!VipsRegion.Prepare(ir, ref s))
            return -1;

        VIPS_GATE_START("VipsReducevVectorGen: work");

        double Y = (r.Top + 0.5) * reducev.VShrink - 0.5 - reducev.VOffset;

        for (int y = 0; y < r.Height; y++)
        {
            var q = VIPS_REGION_ADDR(outRegion, r.Left, r.Top + y);
            int py = (int)Y;
            var p = VIPS_REGION_ADDR(ir, r.Left, py);

            var executor = new OrcExecutor[MAX_PASS];

            for (int i = 0; i < reducev.NPass; i++)
            {
                var pass = &reducev.Passes[i];

                for (int j = 0; j < pass.NScanline; j++)
                    executor[i].SetArray(pass.R + 1 + j, VIPS_REGION_ADDR(ir, r.Left, py + j + pass.First));

                executor[i].SetArray(pass.R, seq.T1);
                executor[i].SetArray(pass.D2, seq.T2);

                for (int j = 0; j < pass.NParam; j++)
                    executor[i].SetParam(ORC_VAR_P1 + j, reducev.Matrixo[(int)Y][j + pass.First]);

                executor[i].SetArray(pass.D1, q);
                executor[i].Run();

                VIPS_SWAP(short[], seq.T1, seq.T2);
            }

            Y += reducev.VShrink;
        }

        VIPS_GATE_STOP("VipsReducevVectorGen: work");

        VIPS_COUNT_PIXELS(outRegion, "VipsReducevVectorGen");

        return 0;
    }
}

public class VipsReducevUcharHwy
{
    public void UcharHwy(VipsPel[] q, VipsPel[] p, int nPoint, int ne, int lskip, short[] cy)
    {
        for (int z = 0; z < ne; z++)
        {
            var sum = ReduceSum(p[z], lskip / sizeof(int), cy, nPoint);
            sum = UnsignedFixedRound(sum);
            q[z] = VIPS_CLIP(0, sum, UCHAR_MAX);
        }
    }
}

public class VipsReducevSignedIntTab
{
    public void SignedIntTab(VipsPel[] q, VipsPel[] p, int ne, int lskip, short[] cy)
    {
        for (int z = 0; z < ne; z++)
        {
            var sum = ReduceSum(p[z], lskip / sizeof(int), cy, nPoint);
            sum = SignedFixedRound(sum);
            q[z] = VIPS_CLIP(SCHAR_MIN, sum, SCHAR_MAX);
        }
    }
}

public class VipsReducevUnsignedIntTab
{
    public void UnsignedIntTab(VipsPel[] q, VipsPel[] p, int ne, int lskip, short[] cy)
    {
        for (int z = 0; z < ne; z++)
        {
            var sum = ReduceSum(p[z], lskip / sizeof(int), cy, nPoint);
            sum = UnsignedFixedRound(sum);
            q[z] = VIPS_CLIP(0, sum, UCHAR_MAX);
        }
    }
}

public class VipsReducevFloatTab
{
    public void FloatTab(VipsPel[] q, VipsPel[] p, int ne, int lskip, double[] cy)
    {
        for (int z = 0; z < ne; z++)
            q[z] = ReduceSum(p[z], lskip / sizeof(int), cy, nPoint);
    }
}

public class VipsReducevNotab
{
    public void Notab(VipsPel[] q, VipsPel[] p, int ne, int lskip, double y)
    {
        var cy = new double[nPoint];

        VipsReduce.MakeMask(cy, reducev.Kernel, nPoint, reducev.VShrink, (float)y);

        for (int z = 0; z < ne; z++)
            q[z] = ReduceSum(p[z], lskip / sizeof(int), cy, nPoint);
    }
}

public class VipsReduceVectorToFixedPoint
{
    public bool VectorToFixedPoint(double[] inArray, int[] outArray, int n, int scale)
    {
        double fsum;
        int i;
        int target;
        int sum;
        double high;
        double low;
        double guess;

        fsum = 0.0;
        for (i = 0; i < n; i++)
            fsum += inArray[i];
        target = VIPS_RINT(fsum * scale);

        high = scale + (n + 1) / 2;
        low = scale - (n + 1) / 2;

        do
        {
            guess = (high + low) / 2.0;

            for (i = 0; i < n; i++)
                outArray[i] = VIPS_RINT(inArray[i] * guess);

            sum = 0;
            for (i = 0; i < n; i++)
                sum += outArray[i];

            if (sum == target)
                break;
            if (sum < target)
                low = guess;
            if (sum > target)
                high = guess;

        } while (high - low > 0.01);

        if (sum != target)
        {
            int eachError = (target - sum) / n;
            int extraError = (target - sum) % n;

            int direction = extraError > 0 ? 1 : -1;
            int nElements = Math.Abs(extraError);

            for (i = 0; i < n; i++)
                outArray[i] += eachError;

            for (i = 0; i < nElements; i++)
                outArray[i] += direction;
        }

        return true;
    }
}
```