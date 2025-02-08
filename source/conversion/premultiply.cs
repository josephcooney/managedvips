Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

public class Premultiply : Conversion
{
    public override int Gen(VipsRegion out_region, object vseq, object a, object b, ref bool stop)
    {
        var premultiply = (VipsPremultiply)b;
        var ir = (VipsRegion)vseq;
        var im = ir.im;
        var r = out_region.valid;
        var width = r.width;
        var bands = im.Bands;
        var max_alpha = premultiply.max_alpha;

        int x, y, i;

        if (vips_region_prepare(ir, ref r))
            return -1;

        for (y = 0; y < r.height; y++)
        {
            VipsPel[] in_array = new VipsPel[r.width * bands];
            VipsPel[] out_array = new VipsPel[r.width * bands];

            var in_ptr = VIPS_REGION_ADDR(ir, r.left, r.top + y);
            var out_ptr = VIPS_REGION_ADDR(out_region, r.left, r.top + y);

            switch (im.BandFmt)
            {
                case VipsBandFormat.UCHAR:
                    PremultiplyUchar(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.CHAR:
                    PremultiplyChar(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.USHORT:
                    PremultiplyUshort(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.SHORT:
                    PremultiplyShort(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.UINT:
                    PremultiplyUint(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.INT:
                    PremultiplyInt(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.FLOAT:
                    PremultiplyFloat(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                case VipsBandFormat.DOUBLE:
                    PremultiplyDouble(in_array, out_array, in_ptr, out_ptr, width, bands, max_alpha);
                    break;

                default:
                    g_assert_not_reached();
                    break;
            }
        }

        return 0;
    }

    private void PremultiplyUchar(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = (float)(in_array[i] * nalpha);

            out_array[bands - 1] = (float)clip_alpha;
        }
    }

    private void PremultiplyChar(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = (float)(in_array[i] * nalpha);

            out_array[bands - 1] = (float)clip_alpha;
        }
    }

    private void PremultiplyUshort(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = (float)(in_array[i] * nalpha);

            out_array[bands - 1] = (float)clip_alpha;
        }
    }

    private void PremultiplyShort(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = (float)(in_array[i] * nalpha);

            out_array[bands - 1] = (float)clip_alpha;
        }
    }

    private void PremultiplyUint(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = (float)(in_array[i] * nalpha);

            out_array[bands - 1] = (float)clip_alpha;
        }
    }

    private void PremultiplyInt(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = (float)(in_array[i] * nalpha);

            out_array[bands - 1] = (float)clip_alpha;
        }
    }

    private void PremultiplyFloat(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = in_array[i] * nalpha;

            out_array[bands - 1] = clip_alpha;
        }
    }

    private void PremultiplyDouble(VipsPel[] in_array, VipsPel[] out_array, VipsPel* in_ptr, VipsPel* out_ptr, int width, int bands, double max_alpha)
    {
        for (int x = 0; x < width; x++)
        {
            var alpha = in_ptr[bands - 1];
            var clip_alpha = Math.Min(Math.Max(0, alpha), max_alpha);
            var nalpha = clip_alpha / max_alpha;

            for (int i = 0; i < bands - 1; i++)
                out_array[i] = in_array[i] * nalpha;

            out_array[bands - 1] = clip_alpha;
        }
    }

    public override int Build(VipsObject obj)
    {
        var class = VIPS_OBJECT_GET_CLASS(obj);
        var conversion = (VipsConversion)obj;
        var premultiply = (VipsPremultiply)obj;
        var t = vips_object_local_array(obj, 1);

        var in_image = premultiply.in;

        if (vips_image_decode(in_image, ref t[0]))
            return -1;

        in_image = t[0];

        // Trivial case: fall back to copy().
        if (in_image.Bands == 1)
            return vips_image_write(in_image, conversion.out);

        if (vips_check_noncomplex(class.nickname, in_image))
            return -1;

        if (vips_image_pipelinev(conversion.out,
                VIPS_DEMAND_STYLE_THINSTRIP, in_image, null))
            return -1;

        // Is max-alpha unset? Default to the correct value for this
        // interpretation.
        if (!vips_object_argument_isset(obj, "max_alpha"))
            premultiply.max_alpha = vips_interpretation_max_alpha(in_image.Type);

        if (in_image.BandFmt == VipsBandFormat.DOUBLE)
            conversion.out.BandFmt = VipsBandFormat.DOUBLE;
        else
            conversion.out.BandFmt = VipsBandFormat.FLOAT;

        if (vips_image_generate(conversion.out,
                vips_start_one, Gen, vips_stop_one,
                in_image, premultiply))
            return -1;

        return 0;
    }

    public static int Premultiply(VipsImage in_image, ref VipsImage out_image, params object[] args)
    {
        var result = vips_call_split("premultiply", args, in_image, ref out_image);
        return result;
    }
}

public class VipsPremultiply : Conversion
{
    public double max_alpha { get; set; }

    public override void Init()
    {
        max_alpha = 255.0;
    }

    public static void ClassInit(TypeRegistry type_registry)
    {
        var class = new Type(type_registry, "VipsPremultiply", typeof(VipsPremultiply));
        class.SetProperty("nickname", "premultiply");
        class.SetProperty("description", "premultiply image alpha");

        var operation_class = (Type)class;
        operation_class.flags |= VIPS_OPERATION_SEQUENTIAL;

        var arg_image = new Type(type_registry, "VipsImage", typeof(VipsImage));
        arg_image.name = "in";
        arg_image.description = "Input";
        arg_image.arg_type = VIPS_ARGUMENT_REQUIRED_INPUT;
        arg_image.offset = G_STRUCT_OFFSET(typeof(VipsPremultiply), in_image);

        var arg_max_alpha = new Type(type_registry, "double", typeof(double));
        arg_max_alpha.name = "max_alpha";
        arg_max_alpha.description = "Maximum alpha";
        arg_max_alpha.arg_type = VIPS_ARGUMENT_OPTIONAL_INPUT;
        arg_max_alpha.offset = G_STRUCT_OFFSET(typeof(VipsPremultiply), max_alpha);
        arg_max_alpha.min_value = 0.0;
        arg_max_alpha.max_value = 100000000.0;
        arg_max_alpha.default_value = 255.0;

        operation_class.args.Add(arg_image);
        operation_class.args.Add(arg_max_alpha);

        type_registry.RegisterType(class);
    }
}
```

This code defines a `VipsPremultiply` class that inherits from the `Conversion` class and implements the necessary methods for premultiplying an image's alpha channel. The `Gen` method is used to generate the output image, and the `Build` method is used to build the conversion pipeline.

The `PremultiplyUchar`, `PremultiplyChar`, etc. methods are used to perform the actual premultiplication of the image data based on the input format.

The `ClassInit` method is used to register the `VipsPremultiply` class with the VIPS type registry and define its properties and arguments.

Note that this code assumes that you have already converted other VIPS methods in separate files, as mentioned in your question.