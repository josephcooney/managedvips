```csharp
using System;
using System.Collections.Generic;

public class VipsConvi : VipsConvolution
{
    public int n_point { get; set; }
    public int nnz { get; set; }
    public int[] coeff { get; set; }
    public int[] coeff_pos { get; set; }

#if defined(HAVE_HWY) || defined(HAVE_ORC)
    public short[] mant { get; set; }
    public int exp { get; set; }
#endif /*HAVE_HWY || HAVE_ORC*/

#ifdef HAVE_ORC
    public int sexp { get; set; }
    public int n_pass { get; set; }
    public Pass[] pass { get; set; }
    public int r { get; set; }
    public int d1 { get; set; }
    public OrcProgram program { get; set; }
#endif /*HAVE_ORC*/
}

public class VipsConviSequence
{
    public VipsConvi convi { get; set; }
    public VipsRegion ir { get; set; }
    public int[] offsets { get; set; }
    public int last_bpl { get; set; }

#ifdef HAVE_ORC
    public short[] t1 { get; set; }
    public short[] t2 { get; set; }
#endif /*HAVE_ORC*/
}

public class Pass
{
    public int first { get; set; }
    public int last { get; set; }
    public int r { get; set; }
    public int d1 { get; set; }
    public int n_const { get; set; }
    public int n_scanline { get; set; }
    public int[] line { get; set; }
    public OrcProgram program { get; set; }
}

public class VipsConviClass : VipsConvolutionClass
{
    public static void Initialize(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsConvolution);
    }
}

public class VipsConviSequenceClass : VipsObjectClass
{
    public static void Initialize(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsObject);
    }
}

public class PassClass : VipsObjectClass
{
    public static void Initialize(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsObject);
    }
}

public class OrcProgram
{
    public int AddDestination(int size, string name)
    {
        // ...
    }

    public int AddSource(int size, string name)
    {
        // ...
    }

    public int AddConstant(string name, object value, int size)
    {
        // ...
    }

    public int AppendDsStr(string op, string a, string b)
    {
        // ...
    }

    public int AppendStr(string op, string a, string b, string c)
    {
        // ...
    }
}

public class OrcExecutor
{
    public void SetProgram(OrcProgram program)
    {
        // ...
    }

    public void SetN(int n)
    {
        // ...
    }

    public void Run()
    {
        // ...
    }
}

public class VipsConvi : VipsConvolution
{
    public int n_point { get; set; }
    public int nnz { get; set; }
    public int[] coeff { get; set; }
    public int[] coeff_pos { get; set; }

#if defined(HAVE_HWY) || defined(HAVE_ORC)
    public short[] mant { get; set; }
    public int exp { get; set; }
#endif /*HAVE_HWY || HAVE_ORC*/

#ifdef HAVE_ORC
    public int sexp { get; set; }
    public int n_pass { get; set; }
    public Pass[] pass { get; set; }
    public int r { get; set; }
    public int d1 { get; set; }
    public OrcProgram program { get; set; }
#endif /*HAVE_ORC*/

    public VipsConviSequence Start(VipsImage in, VipsConvi convi)
    {
        // ...
    }

#if defined(HAVE_HWY)
    public int UcharVectorGen(VipsRegion out_region, VipsConviSequence seq, VipsImage in, VipsConvi convi)
    {
        // ...
    }
#elif defined(HAVE_ORC)
    public int GenVector(VipsRegion out_region, VipsConviSequence seq, VipsImage in, VipsConvi convi)
    {
        // ...
    }
#endif /*HAVE_HWY*/

    public int Gen(VipsRegion out_region, VipsConviSequence seq, VipsImage in, VipsConvi convi)
    {
        // ...
    }

#if defined(HAVE_HWY) || defined(HAVE_ORC)
    public int Intize(VipsImage M)
    {
        // ...
    }
#endif /*HAVE_HWY || HAVE_ORC*/

    public override bool Build()
    {
        // ...
    }
}

public class VipsConviClass : VipsConvolutionClass
{
    public static void Initialize(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsConvolution);
    }
}

public class VipsConviSequenceClass : VipsObjectClass
{
    public static void Initialize(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsObject);
    }
}

public class PassClass : VipsObjectClass
{
    public static void Initialize(Type type)
    {
        // ...
    }

    public override Type GetParentType()
    {
        return typeof(VipsObject);
    }
}
```