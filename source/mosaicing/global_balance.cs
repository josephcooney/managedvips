Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsGlobalBalance : VipsOperation
{
    public override int Build(VipsObject obj)
    {
        VipsGlobalBalance globalbalance = (VipsGlobalBalance)obj;

        SymbolTable st;
        TransformFn trn;

        obj.SetProperty("out", new VipsImage());

        if (base.Build(obj))
            return -1;

        st = new SymbolTable(100);
        if (!AnalyzeMosaic(st, globalbalance.In))
            return -1;

        if (!FindFactors(st, globalbalance.Gamma))
            return -1;

        trn = globalbalance.IntOutput ? Transform : Transformf;
        if (!BuildMosaic(st, globalbalance.Out, trn, globalbalance.Gamma))
            return -1;

        return 0;
    }

    public override void ClassInit(VipsOperationClass class_)
    {
        base.ClassInit(class_);

        VipsObjectClass object_class = (VipsObjectClass)class_;
        GObjectClass gobject_class = (GObjectClass)object_class;

        gobject_class.SetProperty = base.GetProperty;
        gobject_class.GetProperty = base.GetProperty;

        object_class.Nickname = "globalbalance";
        object_class.Description = "Global balance an image mosaic";

        VipsArgImage in_arg = new VipsArgImage("in", 1, "Input", "Input image", VipsArgument.RequiredInput);
        VipsArgImage out_arg = new VipsArgImage("out", 2, "Output", "Output image", VipsArgument.RequiredOutput);

        VipsArgDouble gamma_arg = new VipsArgDouble("gamma", 5, "Gamma", "Image gamma", VipsArgument.OptionalInput, 0.00001, 10, 1.6);
        VipsArgBool int_output_arg = new VipsArgBool("int_output", 7, "Int output", "Integer output", VipsArgument.OptionalInput, false);

        object_class.Args.Add(in_arg);
        object_class.Args.Add(out_arg);
        object_class.Args.Add(gamma_arg);
        object_class.Args.Add(int_output_arg);
    }

    public override void Init(VipsGlobalBalance globalbalance)
    {
        base.Init(globalbalance);
        globalbalance.Gamma = 1.6;
    }
}

public class SymbolTable
{
    public int Sz { get; set; }
    public GSList[] Table { get; set; }
    public VipsImage Im { get; set; }
    public int Novl { get; set; }
    public int Nim { get; set; }
    public int Njoin { get; set; }
    public JoinNode Root { get; set; }
    public JoinNode Leaf { get; set; }
    public double[] Fac { get; set; }

    public SymbolTable(int sz)
    {
        Sz = sz;
        Table = new GSList[sz];
        Im = null;
        Novl = 0;
        Nim = 0;
        Njoin = 0;
        Root = null;
        Leaf = null;
        Fac = new double[100];
    }
}

public class JoinNode
{
    public int Type { get; set; }
    public string Name { get; set; }
    public bool Dirty { get; set; }
    public int Mwidth { get; set; }
    public VipsImage Im { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double Dx { get; set; }
    public double Dy { get; set; }
    public JoinNode Arg1 { get; set; }
    public JoinNode Arg2 { get; set; }
    public VipsTransformation Thistrn { get; set; }
    public GSList Overlaps { get; set; }

    public JoinNode()
    {
        Type = 0;
        Name = "";
        Dirty = false;
        Mwidth = -1;
        Im = null;
        A = 0.0;
        B = 0.0;
        Dx = 0.0;
        Dy = 0.0;
        Arg1 = null;
        Arg2 = null;
        Thistrn = new VipsTransformation();
        Overlaps = null;
    }
}

public class TransformFn
{
    public virtual VipsImage MakeMosaic(JoinNode node, double[] gamma)
    {
        // implementation of transform function
    }

    public virtual VipsImage MakeMosaicf(JoinNode node, double[] gamma)
    {
        // implementation of transform function for float output
    }
}

public class VipsGlobalBalanceClass : VipsOperationClass
{
    public override void ClassInit(VipsOperationClass class_)
    {
        base.ClassInit(class_);
        // ...
    }

    public override void Init(VipsGlobalBalance globalbalance)
    {
        base.Init(globalbalance);
        // ...
    }
}

public class VipsImage
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }
    public VipsFormat BandFmt { get; set; }

    public VipsImage()
    {
        Xsize = 0;
        Ysize = 0;
        Bands = 0;
        BandFmt = VipsFormat.UCHAR;
    }
}

public class VipsTransformation
{
    public double A { get; set; }
    public double B { get; set; }
    public double C { get; set; }
    public double D { get; set; }
    public int Odx { get; set; }
    public int Ody { get; set; }

    public VipsTransformation()
    {
        A = 1.0;
        B = 0.0;
        C = 0.0;
        D = 1.0;
        Odx = 0;
        Ody = 0;
    }
}

public class VipsFormat
{
    public const int UCHAR = 1;
    public const int USHORT = 2;
    // ...
}

public static class VipsOperation
{
    public virtual int Build(VipsObject obj)
    {
        // implementation of build method
    }

    public virtual void ClassInit(VipsOperationClass class_)
    {
        // implementation of class init method
    }

    public virtual void Init(VipsOperation operation)
    {
        // implementation of init method
    }
}

public static class VipsArgImage
{
    public string Name { get; set; }
    public int Index { get; set; }
    public string Description { get; set; }
    public string Nickname { get; set; }
    public VipsArgument ArgumentType { get; set; }

    public VipsArgImage(string name, int index, string description, string nickname, VipsArgument argumentType)
    {
        Name = name;
        Index = index;
        Description = description;
        Nickname = nickname;
        ArgumentType = argumentType;
    }
}

public static class VipsArgDouble
{
    public string Name { get; set; }
    public int Index { get; set; }
    public string Description { get; set; }
    public string Nickname { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double DefaultValue { get; set; }

    public VipsArgDouble(string name, int index, string description, string nickname, VipsArgument argumentType, double minValue, double maxValue, double defaultValue)
    {
        Name = name;
        Index = index;
        Description = description;
        Nickname = nickname;
        ArgumentType = argumentType;
        MinValue = minValue;
        MaxValue = maxValue;
        DefaultValue = defaultValue;
    }
}

public static class VipsArgBool
{
    public string Name { get; set; }
    public int Index { get; set; }
    public string Description { get; set; }
    public string Nickname { get; set; }

    public VipsArgBool(string name, int index, string description, string nickname)
    {
        Name = name;
        Index = index;
        Description = description;
        Nickname = nickname;
    }
}

public static class VipsArgument
{
    public enum Type
    {
        RequiredInput,
        OptionalInput,
        // ...
    }

    public Type ArgumentType { get; set; }

    public VipsArgument(Type argumentType)
    {
        ArgumentType = argumentType;
    }
}
```

Note that this is a simplified version of the original C code and some parts may be missing or not fully implemented. Also, this code uses C# syntax and does not include all the details from the original C code.

Also note that `VipsImage`, `VipsTransformation` and other classes are assumed to be already defined elsewhere in the project.

Please let me know if you need any further assistance!