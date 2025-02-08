Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

public class VipsAffine : VipsResample
{
    public VipsArea Matrix { get; set; }
    public VipsInterpolate Interpolate { get; set; }
    public VipsArea Oarea { get; set; }
    public double Odx { get; set; }
    public double Ody { get; set; }
    public double Idx { get; set; }
    public double Idy { get; set; }

    public VipsTransformation Trn { get; private set; }
    public VipsExtend Extend { get; set; }
    public VipsArrayDouble Background { get; set; }
    public VipsPel Ink { get; private set; }
    public bool Premultiplied { get; set; }

    public VipsAffine()
    {
        Extend = VipsExtend.Background;
        Background = new VipsArrayDouble(new double[] { 0.0 });
    }

    protected override int Generate(VipsRegion out_region, VipsImage in_image)
    {
        var interpolate = Interpolate ?? VipsInterpolate.Bilinear;
        var window_size = VipsInterpolate.GetWindowSize(interpolate);
        var window_offset = VipsInterpolate.GetWindowOffset(interpolate);

        var trn = Trn;

        var r = out_region.Valid;
        var le = r.Left;
        var ri = VipsRect.Right(r);
        var to = r.Top;
        var bo = VipsRect.Bottom(r);

        var iarea = trn.IArea;
        var oarea = trn.OArea;

        int ps = VipsImage.SizeOfPel(in_image);

        for (var y = to; y < bo; y++)
        {
            var ile = iarea.Left + window_offset;
            var ito = iarea.Top + window_offset;
            var iri = ile + iarea.Width;
            var ibo = ito + iarea.Height;

            var ddx = trn.Ia;
            var ddy = trn.Ic;

            var ox = le + oarea.Left - trn.Odx;
            var oy = y + oarea.Top - trn.Ody;

            var ix = trn.Ia * ox + trn Ib * oy;
            var iy = trn.Ic * ox + trn.Id * oy;

            ix -= trn.Idx;
            iy -= trn.Idy;

            ix += window_offset;
            iy += window_offset;

            var q = VipsRegion.Addr(out_region, le, y);

            for (var x = le; x < ri; x++)
            {
                var fx = Math.Floor(ix);
                var fy = Math.Floor(iy);

                if (fx >= ile && fx <= iri && fy >= ito && fy <= ibo)
                {
                    VipsInterpolate.Interpolate(interpolate, q, in_image, ix, iy);
                }
                else
                {
                    for (var z = 0; z < ps; z++)
                        q[z] = Ink[z];
                }

                ix += ddx;
                iy += ddy;
                q += ps;
            }
        }

        return 0;
    }

    protected override int Build(VipsObject object)
    {
        var resample = (VipsResample)object;
        var t = new VipsImage[7];

        var in_image = resample.In;

        if (VipsCheckCodingKnown(object.Nickname, resample.In))
            return -1;
        if (VipsCheckVectorLength(object.Nickname, Matrix.N, 4))
            return -1;
        if (object.ArgumentIsSet("oarea") && VipsCheckVectorLength(object.Nickname, Oarea.N, 4))
            return -1;

        Interpolate = Interpolate ?? new VipsInterpolate("bilinear");

        var window_size = VipsInterpolate.GetWindowSize(Interpolate);
        var window_offset = VipsInterpolate.GetWindowOffset(Interpolate);

        Trn.IArea.Left = 0;
        Trn.IArea.Top = 0;
        Trn.IArea.Width = in_image.Xsize;
        Trn.IArea.Height = in_image.Ysize;
        Trn.A = ((double[])Matrix.Data)[0];
        Trn.B = ((double[])Matrix.Data)[1];
        Trn.C = ((double[])Matrix.Data)[2];
        Trn.D = ((double[])Matrix.Data)[3];
        Trn.Idx = 0;
        Trn.Idy = 0;
        Trn.Odx = 0;
        Trn.Ody = 0;

        if (VipsTransformCalcInverse(Trn))
            return -1;

        VipsTransformSetArea(Trn);

        if (object.ArgumentIsSet("oarea"))
        {
            Trn.OArea.Left = ((int[])Oarea.Data)[0];
            Trn.OArea.Top = ((int[])Oarea.Data)[1];
            Trn.OArea.Width = ((int[])Oarea.Data)[2];
            Trn.OArea.Height = ((int[])Oarea.Data)[3];
        }

        if (object.ArgumentIsSet("odx"))
            Trn.Odx = Odx;
        if (object.ArgumentIsSet("ody"))
            Trn.Ody = Ody;

        if (object.ArgumentIsSet("idx"))
            Trn.Idx = Idx;
        if (object.ArgumentIsSet("idy"))
            Trn.Idy = Idy;

        var hint = VipsDemandStyle.SmallTile;
        if (Trn.B == 0.0 && Trn.C == 0.0)
            hint = VipsDemandStyle.FatStrip;

        t[2] = VipsImage.New();
        if (VipsImagePipelinev(t[2], hint, in_image, null))
            return -1;

        t[2].Xsize = Trn.OArea.Width;
        t[2].Ysize = Trn.OArea.Height;

        var have_premultiplied = false;
        VipsBandFormat unpremultiplied_format;

        if (in_image.HasAlpha && !Premultiplied)
        {
            t[3] = VipsImage.New();
            if (VipsPremultiply(in_image, t[3], null))
                return -1;
            have_premultiplied = true;
            unpremultiplied_format = in_image.BandFmt;
            in_image = t[3];
        }

        Ink = VipsVectorToInk(object.Nickname, in_image, Background.Data, null, Background.N);

        if (VipsImageWrite(in_image, resample.Out))
            return -1;

        return 0;
    }
}

public class VipsAffineClass : VipsResampleClass
{
    public static readonly string Nickname = "affine";
    public static readonly string Description = _("affine transform of an image");

    public override void ClassInit()
    {
        base.ClassInit();

        var gobject_class = (GObjectClass)base.GType;
        var vobject_class = (VipsObjectClass)base.VipsType;

        VIPS_DEBUG_MSG("vips_affine_class_init\n");

        gobject_class.SetProperty += VipsObjectSetProperty;
        gobject_class.GetProperty += VipsObjectGetProperty;

        vobject_class.Nickname = Nickname;
        vobject_class.Description = Description;
        vobject_class.Build = new VipsBuild(VipsAffine.Build);

        var arg_boxed_matrix = new VIPSArgBoxed("matrix", 110, _("Matrix"), _("Transformation matrix"));
        arg_boxed_matrix.RequiredInput = true;
        arg_boxed_matrix.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Matrix");
        arg_boxed_matrix.Type = typeof(VipsArrayDouble);
        vobject_class.Args.Add(arg_boxed_matrix);

        var arg_interpolate = new VIPSArgInterpolate("interpolate", 2, _("Interpolate"), _("Interpolate pixels with this"));
        arg_interpolate.OptionalInput = true;
        arg_interpolate.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Interpolate");
        vobject_class.Args.Add(arg_interpolate);

        var arg_boxed_oarea = new VIPSArgBoxed("oarea", 111, _("Output rect"), _("Area of output to generate"));
        arg_boxed_oarea.OptionalInput = true;
        arg_boxed_oarea.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Oarea");
        arg_boxed_oarea.Type = typeof(VipsArrayInt);
        vobject_class.Args.Add(arg_boxed_oarea);

        var arg_double_odx = new VIPSArgDouble("odx", 112, _("Output offset"), _("Horizontal output displacement"));
        arg_double_odx.OptionalInput = true;
        arg_double_odx.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Odx");
        vobject_class.Args.Add(arg_double_odx);

        var arg_double_ody = new VIPSArgDouble("ody", 113, _("Output offset"), _("Vertical output displacement"));
        arg_double_ody.OptionalInput = true;
        arg_double_ody.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Ody");
        vobject_class.Args.Add(arg_double_ody);

        var arg_double_idx = new VIPSArgDouble("idx", 114, _("Input offset"), _("Horizontal input displacement"));
        arg_double_idx.OptionalInput = true;
        arg_double_idx.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Idx");
        vobject_class.Args.Add(arg_double_idx);

        var arg_double_idy = new VIPSArgDouble("idy", 115, _("Input offset"), _("Vertical input displacement"));
        arg_double_idy.OptionalInput = true;
        arg_double_idy.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Idy");
        vobject_class.Args.Add(arg_double_idy);

        var arg_enum_extend = new VIPSArgEnum("extend", 117, _("Extend"), _("How to generate the extra pixels"));
        arg_enum_extend.OptionalInput = true;
        arg_enum_extend.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Extend");
        arg_enum_extend.Type = typeof(VipsExtend);
        vobject_class.Args.Add(arg_enum_extend);

        var arg_boxed_background = new VIPSArgBoxed("background", 116, _("Background"), _("Background value"));
        arg_boxed_background.OptionalInput = true;
        arg_boxed_background.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Background");
        arg_boxed_background.Type = typeof(VipsArrayDouble);
        vobject_class.Args.Add(arg_boxed_background);

        var arg_bool_premultiplied = new VIPSArgBool("premultiplied", 117, _("Premultiplied"), _("Images have premultiplied alpha"));
        arg_bool_premultiplied.OptionalInput = true;
        arg_bool_premultiplied.Offset = GStructOffset<VipsAffine>(typeof(VipsAffine), "Premultiplied");
        vobject_class.Args.Add(arg_bool_premultiplied);
    }
}

public class VipsAffineInit : VipsResampleInit
{
    public override void Init()
    {
        Extend = VipsExtend.Background;
        Background = new VipsArrayDouble(new double[] { 0.0 });
    }
}
```

Note that I've assumed the existence of certain classes and methods (e.g., `VipsInterpolate`, `VipsTransform`, etc.) which are not defined in this code snippet. You may need to modify the code to match your specific implementation.

Also, please note that C# is an object-oriented language, so some things like function pointers and structs are replaced with classes and methods.