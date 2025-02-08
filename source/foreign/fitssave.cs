Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

// Converted from: vips_foreign_save_fits_build()
public class FitsSave : ForeignSave
{
    public override int Build(VipsObject obj)
    {
        var save = (ForeignSave)obj;
        var fits = (FitsSave)obj;
        var t = new VipsImage[2];

        if (base.Build(obj) != 0)
            return -1;

        // FITS is written bottom-to-top, so we must flip.
        // But all vips readers must work top-to-bottom (or vips_copy()'s seq
        // hint won't work) so we must cache the input image.
        t[0] = Vips.Image.NewMemory();
        if (Vips.Image.Write(save.Ready, t[0]) != 0 ||
            Vips.Flip(t[0], out t[1], Vips.Direction.Vertical, null) != 0 ||
            Vips.FitsWrite(t[1], fits.Filename) != 0)
            return -1;

        return 0;
    }
}

// Converted from: vips_foreign_save_fits_class_init()
public class FitsSaveClass : ForeignSaveClass
{
    public override void ClassInit(VipsForeignSaveClass klass)
    {
        base.ClassInit(klass);

        // cfitsio has not been fuzzed, so should not be used with
        // untrusted input unless you are very careful.
        Vips.OperationFlags |= Vips.OperationFlag.Untrusted;

        // Save a bit of typing.
        var bandfmt_fits = new[]
        {
            Vips.Format.UChar,
            Vips.Format.Char,
            Vips.Format.UShort,
            Vips.Format.Short,
            Vips.Format.UInt,
            Vips.Format.Int,
            Vips.Format.Float,
            Vips.Format.Complex,
            Vips.Format.Double,
            Vips.Format.DComplex
        };

        // cfitsio has not been fuzzed, so should not be used with
        // untrusted input unless you are very careful.
        klass.Saveable = Vips.Saveable.Any;
        klass.FormatTable = bandfmt_fits;

        // Save a bit of typing.
        var arg = new Vips.Argument("filename", 1,
            "Filename",
            "Filename to save to",
            Vips.ArgumentFlag.RequiredInput);

        // cfitsio has not been fuzzed, so should not be used with
        // untrusted input unless you are very careful.
        klass.AddArgument(arg);
    }
}

// Converted from: vips_foreign_save_fits_init()
public class FitsSave : ForeignSave
{
    public FitsSave() { }
}

// Converted from: vips_fitssave()
public static int VipsFitssave(VipsImage inImage, string filename)
{
    return Vips.CallSplit("fitssave", inImage, filename);
}
```