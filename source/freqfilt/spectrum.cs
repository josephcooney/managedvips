```csharp
// vips_spectrum_build
public int Build(VipsObject obj)
{
    VipsFreqfilt freqfilt = (VipsFreqfilt)VipsObject.GetFrequencyFilter(obj);
    VipsImage[] t = new VipsImage[5];

    VipsImage in;

    if (VipsObjectClass.Build((VipsObject)obj))
        return -1;

    in = freqfilt.In;

    if (in.Format != VipsFormat.Complex)
    {
        if (!VipsFwfft(in, t, null))
            return -1;
        in = t[0];
    }

    if (!VipsAbs(in, t, null) ||
        !VipsScale(t[0], t[1], "log", true, null) ||
        !VipsWrap(t[1], t[2], null))
        return -1;

    if (!VipsImageWrite(t[2], freqfilt.Out))
        return -1;

    return 0;
}

// vips_spectrum_class_init
public class VipsSpectrumClass : VipsObjectClass
{
    public override string Nickname { get; set; } = "spectrum";
    public override string Description { get; set; } = _("make displayable power spectrum");
    public override int Build(VipsObject obj) => vips_spectrum_build((VipsObject)obj);
}

// vips_spectrum_init
public VipsSpectrum()
{
}

// vips_spectrum
public static int Spectrum(VipsImage in, out VipsImage[] out)
{
    va_list ap;
    int result;

    va_start(ap, out);
    result = VipsCallSplit("spectrum", ap, in, out);
    va_end(ap);

    return result;
}
```