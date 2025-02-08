```csharp
// vips_hist_entropy_build
public int VipsHistEntropyBuild(VipsObject obj)
{
    VipsObjectClass klass = (VipsObjectClass)VipsObject.GetClass(obj);
    VipsHistEntropy entropy = (VipsHistEntropy)obj;
    VipsImage[] t = new VipsImage[4];

    double avg;
    double sum;

    if (klass.ParentBuild(obj) != 0)
        return -1;

    if (!VipsCheckHist(klass.Nickname, entropy.In))
        return -1;

    // Compute:
    //   norm_hist = hist / sum(hist)
    //   entropy = -sum(norm_hist * log2(norm_hist))
    if (VipsAvg(entropy.In, out avg, null) != 0)
        return -1;
    sum = avg * VipsImage.GetNPels(entropy.In) * entropy.In.Bands;
    if ((VipsLinear1(entropy.In, t[0], 1.0 / sum, 0, null) != 0) ||
        (VipsLog(t[0], t[1], null) != 0) ||
        (VipsLinear1(t[1], t[2], 1.0 / Math.Log(2), 0, null) != 0) ||
        (VipsMultiply(t[0], t[2], t[3], null) != 0) ||
        (VipsAvg(t[3], out avg, null) != 0))
        return -1;

    VipsObject.SetProperty(entropy,
        "out", -avg * VipsImage.GetNPels(entropy.In) * entropy.In.Bands,
        null);

    return 0;
}

// vips_hist_entropy_class_init
public void VipsHistEntropyClassInit(VipsHistEntropyClass klass)
{
    GObjectClass gobject_class = (GObjectClass)klass;
    VipsObjectClass object_class = (VipsObjectClass)klass;

    gobject_class.SetProperty = VipsObject.GetProperty;
    gobject_class.GetProperty = VipsObject.GetProperty;

    object_class.Nickname = "hist_entropy";
    object_class.Description = _("estimate image entropy");
    object_class.Build = VipsHistEntropyBuild;

    VipsArgImage(klass, "in", 1,
        _("Input"),
        _("Input histogram image"),
        VipsArgument.RequiredInput,
        typeof(VipsHistEntropy).GetField("In"));

    VipsArgDouble(klass, "out", 2,
        _("Output"),
        _("Output value"),
        VipsArgument.RequiredOutput,
        typeof(VipsHistEntropy).GetField("Out"),
        double.MinValue, double.MaxValue, 0.0);
}

// vips_hist_entropy_init
public void VipsHistEntropyInit(VipsHistEntropy entropy)
{
}

// vips_hist_entropy
public int VipsHistEntropy(VipsImage in, ref double out, params object[] args)
{
    var result = VipsCallSplit("hist_entropy", in, out);
    return result;
}
```