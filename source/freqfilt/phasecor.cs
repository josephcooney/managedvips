```csharp
// vips_phasecor_build
public int Build(VipsObject object)
{
    VipsFreqfilt freqfilt = (VipsFreqfilt)object;
    VipsPhasecor phasecor = (VipsPhasecor)object;
    VipsImage[] t = new VipsImage[5];

    // Call parent class build method
    if (VIPS_OBJECT_CLASS(vips_phasecor_parent_class).build(object) != 0)
        return -1;

    VipsImage in1 = freqfilt.In;
    VipsImage in2 = phasecor.in2;

    // Convert input images to complex format if necessary
    if (in1.BandFmt != VIPS_FORMAT_COMPLEX)
    {
        if (VipsFwfft(in1, out t[0], null) != 0)
            return -1;
        in1 = t[0];
    }

    if (in2.BandFmt != VIPS_FORMAT_COMPLEX)
    {
        if (VipsFwfft(in2, out t[1], null) != 0)
            return -1;
        in2 = t[1];
    }

    // Calculate phase correlation
    if (VipsCrossPhase(in1, in2, out t[2], null) != 0 ||
        VipsInvfft(t[2], out t[3], "real", true, null) != 0 ||
        VipsImageWrite(t[3], freqfilt.Out) != 0)
        return -1;

    return 0;
}

// vips_phasecor_class_init
public class VipsPhasecorClass : VipsFreqfiltClass
{
    public VipsPhasecorClass()
    {
        // Set up GObject and VObject properties
        this.Nickname = "phasecor";
        this.Description = _("calculate phase correlation");
        this.Build = new BuildDelegate(Build);
        this.Properties.Add(new VIPS_ARG_IMAGE("in2", 0, _("in2"), _("Second input image"), VIPS_ARGUMENT_REQUIRED_INPUT));
    }
}

// vips_phasecor_init
public VipsPhasecor Init()
{
    return new VipsPhasecor();
}

// vips_phasecor
public static int Phasecor(VipsImage in1, VipsImage in2, out VipsImage[] out)
{
    // Call split function to handle optional arguments
    va_list ap;
    int result = VipsCallSplit("phasecor", ref ap, in1, in2, out);
    va_end(ap);

    return result;
}
```