```csharp
// Converted from: vips_freqmult_build()

public class VipsFreqMult : VipsFreqFilt
{
    public VipsImage Mask { get; set; }

    protected override int Build(VipsObject obj)
    {
        var freqfilt = (VipsFreqFilt)obj;
        var freqmult = (VipsFreqMult)obj;
        var t = new VipsImage[5];

        var inImg = freqfilt.In;

        if (VipsBandFormat.IsComplex(inImg.BandFmt))
        {
            if (!VipsMultiply(inImg, freqmult.Mask, out t[0], null) ||
                !VipsInvFft(t[0], out t[1], "real", true, null))
                return -1;

            inImg = t[1];
        }
        else
        {
            // Optimisation: output of vips_invfft() is double, we
            // will usually cast to char, so rather than keeping a
            // large double buffer and partial to char from that,
            // cast to a memory buffer and copy to out from that.
            //
            // FIXME does this actually work now we're a class? test
            // perhaps we need a temporary object

            t[4] = VipsImage.NewMemory();

            if (!VipsFwFft(inImg, out t[0], null) ||
                !VipsMultiply(t[0], freqmult.Mask, out t[1], null) ||
                !VipsInvFft(t[1], out t[2], "real", true, null) ||
                !VipsCast(t[2], out t[3], inImg.BandFmt, null) ||
                !VipsImage.Write(t[3], t[4]))
                return -1;

            inImg = t[4];
        }

        if (!VipsImage.Write(inImg, freqfilt.Out))
            return -1;

        return 0;
    }
}

// Converted from: vips_freqmult_class_init()

public class VipsFreqMultClass : VipsFreqFiltClass
{
    public static void ClassInit(VipsFreqMultClass klass)
    {
        var gobjectClass = (GObjectClass)klass;
        var vobjectClass = (VipsObjectClass)klass;

        gobjectClass.SetProperty = VipsObject.SetProperty;
        gobjectClass.GetProperty = VipsObject.GetProperty;

        vobjectClass.Nickname = "freqmult";
        vobjectClass.Description = _("frequency-domain filtering");
        vobjectClass.Build = new Func<VipsObject, int>(VipsFreqMult.Build);

        VIPS_ARG_IMAGE(klass, "mask", 0,
            _("Mask"),
            _("Input mask image"),
            VIPS_ARGUMENT_REQUIRED_INPUT,
            G_STRUCT_OFFSET(VipsFreqMult, Mask));
    }
}

// Converted from: vips_freqmult_init()

public class VipsFreqMult : VipsFreqFilt
{
    public VipsFreqMult()
    {
    }
}

// Converted from: vips_freqmult()

public static int VipsFreqMult(VipsImage inImg, VipsImage mask, out VipsImage outImg)
{
    var result = VipsCallSplit("freqmult", inImg, mask, out outImg);
    return result;
}
```