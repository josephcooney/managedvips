```csharp
// vips_hist_unary_build

public class VipsHistUnary : VipsHistogram
{
    public VipsHistUnary()
    {
        // histogram->n = 1;
        N = 1;

        // histogram->in = (VipsImage **) vips_object_local_array(object, 1);
        In = new VipsImage[1];

        // histogram->in[0] = unary->in;
        In[0] = InUnary;

        // if (histogram->in[0])
        //     g_object_ref(histogram->in[0]);
    }

    public override int Build()
    {
        // if (VIPS_OBJECT_CLASS(vips_hist_unary_parent_class)->build(object))
        //     return -1;
        return base.Build();
    }
}

// vips_hist_unary_class_init

public class VipsHistUnaryClass : VipsHistogramClass
{
    public VipsHistUnaryClass()
    {
        // GObjectClass *gobject_class = G_OBJECT_CLASS(class);
        // VipsObjectClass *vobject_class = VIPS_OBJECT_CLASS(class);

        // gobject_class->set_property = vips_object_set_property;
        // gobject_class->get_property = vips_object_get_property;

        // vobject_class->nickname = "hist_unary";
        // vobject_class->description = _("hist_unary operations");
        // vobject_class->build = vips_hist_unary_build;

        // VIPS_ARG_IMAGE(class, "in", 1,
        //     _("Input"),
        //     _("Input image"),
        //     VIPS_ARGUMENT_REQUIRED_INPUT,
        //     G_STRUCT_OFFSET(VipsHistUnary, in));
    }
}

// vips_hist_unary_init

public class VipsHistUnary : VipsHistogram
{
    public VipsHistUnary()
    {
        // static void
        // vips_hist_unary_init(VipsHistUnary *hist_unary)
        // {
        // }
    }
}
```