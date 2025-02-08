Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class Copy : Conversion
{
    public Copy(Image inImage) 
        : base(inImage)
    {
    }

    public override int Build()
    {
        if (base.Build() != 0)
            return -1;

        if (in.IsPioInput())
            return -1;

        if (swap)
            Console.WriteLine("copy swap is deprecated, use byteswap instead");

        if (!Pipeline(out, DemandStyle.ThinStrip, in, null))
            return -1;

        // Take a copy of all the basic header fields. We use this for
        // sanity-checking the changes our caller has made.
        Image copyOfFields = out.Clone();

        // Use props to adjust header fields.
        foreach (string name in vips_copy_names)
        {
            GParamSpec pspec;
            ArgumentClass argument_class;
            ArgumentInstance argument_instance;

            if (!GetArgument(name, out pspec, out argument_class, out argument_instance))
                return -1;

            if (argument_instance.Assigned)
            {
                Type type = pspec.ValueType;
                object value = null;

                GetProperty(name, out value);

#ifdef VIPS_DEBUG
                Console.WriteLine("vips_copy_build: " + name + " = " + value);
#endif /* VIPS_DEBUG */

                SetProperty(name, value);
            }
        }

        // Disallow changes which alter sizeof(pel).
        uint pel_size_before = Image.SizeOfPel(copyOfFields);
        uint pel_size_after = Image.SizeOfPel(out);
        if (pel_size_after != pel_size_before)
        {
            throw new ArgumentException("must not change pel size");
        }

        if (!Generate(out, vips_start_one, vips_copy_gen, vips_stop_one, in))
            return -1;

        return 0;
    }
}

public class CopyClass : ConversionClass
{
    public static readonly string[] vips_copy_names = 
    {
        "interpretation",
        "xres",
        "yres",
        "xoffset",
        "yoffset",
        "bands",
        "format",
        "coding",
        "width",
        "height"
    };

    public CopyClass()
    {
    }

    protected override void Init(VipsCopy copy)
    {
        // Init our instance fields.
    }
}

public class VipsCopy
{
    public static int Gen(VipsRegion out_region, object seq, object a, object b, ref bool stop)
    {
        VipsRegion ir = (VipsRegion)seq;
        VipsRect r = out_region.Valid;

        if (vips_region_prepare(ir, r) || vips_region_region(out_region, ir, r, r.Left, r.Top))
            return -1;

        return 0;
    }

    public static int Build(VipsObject object)
    {
        VipsObjectClass class_ = VIPS_OBJECT_GET_CLASS(object);
        Conversion conversion = (Conversion)object;
        Copy copy = (Copy)object;

        if (base.Build(object) != 0)
            return -1;

        if (!in.IsPioInput())
            return -1;

        if (swap)
            Console.WriteLine("copy swap is deprecated, use byteswap instead");

        if (!Pipeline(out, DemandStyle.ThinStrip, in, null))
            return -1;

        // Take a copy of all the basic header fields. We use this for
        // sanity-checking the changes our caller has made.
        Image copyOfFields = out.Clone();

        // Use props to adjust header fields.
        foreach (string name in vips_copy_names)
        {
            GParamSpec pspec;
            ArgumentClass argument_class;
            ArgumentInstance argument_instance;

            if (!GetArgument(name, out pspec, out argument_class, out argument_instance))
                return -1;

            if (argument_instance.Assigned)
            {
                Type type = pspec.ValueType;
                object value = null;

                GetProperty(name, out value);

#ifdef VIPS_DEBUG
                Console.WriteLine("vips_copy_build: " + name + " = " + value);
#endif /* VIPS_DEBUG */

                SetProperty(name, value);
            }
        }

        // Disallow changes which alter sizeof(pel).
        uint pel_size_before = Image.SizeOfPel(copyOfFields);
        uint pel_size_after = Image.SizeOfPel(out);
        if (pel_size_after != pel_size_before)
        {
            throw new ArgumentException("must not change pel size");
        }

        if (!Generate(out, vips_start_one, vips_copy_gen, vips_stop_one, in))
            return -1;

        return 0;
    }
}

public class VipsCopyClass : ConversionClass
{
    public static readonly string[] vips_copy_names = 
    {
        "interpretation",
        "xres",
        "yres",
        "xoffset",
        "yoffset",
        "bands",
        "format",
        "coding",
        "width",
        "height"
    };

    public VipsCopyClass()
    {
    }

    protected override void Init(VipsCopy copy)
    {
        // Init our instance fields.
    }
}

public class Program
{
    public static int Copy(Image inImage, out Image outImage, params object[] args)
    {
        var copy = new Copy(inImage);
        return copy.Build();
    }

    public static int CopyFile(Image inImage, out Image outImage, params object[] args)
    {
        if (inImage.IsFile())
            return Copy(inImage, out outImage, args);

        var file = Vips.Image.NewTempFile("%s.v");
        if (!Vips.Image.Write(inImage, file))
            return -1;

        if (!Copy(file, out outImage, args))
            return -1;

        return 0;
    }
}
```
Note that I've assumed the existence of a `VipsDotNet` namespace containing classes and methods equivalent to those in the original C code. You may need to modify this code to match your specific use case.