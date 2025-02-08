```csharp
// imb_LabQ2Lab: CONVERT n pels from packed 32bit Lab to float values 
// in a buffer
// ARGS:   VipsPel *inp       pointer to first byte of Lab32 buffer
// float *outbuf   destination buffer
//	int n           number of pels to process
// (C) K.Martinez 2/5/93

public static void vips_LabQ2Lab_line(VipsColour colour, VipsPel[] out, ref VipsPel[] in, int width)
{
    signed char* restrict p = (signed char*)in[0];
    float[] restrict q = (float[])out;

    int l;
    int lsbs; /* for lsbs byte */
    int i;  /* counter      */

    // Read input with a signed pointer to get signed ab easily.
    for (i = 0; i < width; i++)
    {
        // Get extra bits.
        lsbs = ((unsigned char*)p)[3];

        // Build L.
        l = ((unsigned char*)p)[0];
        l = (l << 2) | (lsbs >> 6);
        q[0] = (float)l * (100.0 / 1023.0);

        // Build a.
        l = VIPS_LSHIFT_INT(p[1], 3) | ((lsbs >> 3) & 0x7);
        q[1] = (float)l * 0.125;

        // And b.
        l = VIPS_LSHIFT_INT(p[2], 3) | (lsbs & 0x7);
        q[2] = (float)l * 0.125;

        p += 4;
        q += 3;
    }
}

public static void vips__LabQ2Lab_vec(float[] out, VipsPel[] in, int width)
{
    vips_LabQ2Lab_line(null, out, ref in, width);
}

// imb_LabQ2Lab_class_init: Class initialisation
static void vips_LabQ2Lab_class_init(VipsLabQ2LabClass* class)
{
    VipsObjectClass* object_class = (VipsObjectClass*)class;
    VipsColourClass* colour_class = VIPS_COLOUR_CLASS(class);

    object_class->nickname = "LabQ2Lab";
    object_class->description = _("unpack a LabQ image to float Lab");

    colour_class->process_line = vips_LabQ2Lab_line;
}

// imb_LabQ2Lab_init: Object initialisation
static void vips_LabQ2Lab_init(VipsLabQ2Lab* labq2lab)
{
    VipsColour* colour = VIPS_COLOUR(labq2lab);
    VipsColourCode* code = VIPS_COLOUR_CODE(labq2lab);

    colour->coding = VIPS_CODING_NONE;
    colour->interpretation = VIPS_INTERPRETATION_LAB;
    colour->format = VIPS_FORMAT_FLOAT;
    colour->bands = 3;

    code->input_coding = VIPS_CODING_LABQ;
}

// vips_LabQ2Lab: (method)
// @in: input image
// @out: (out): output image
// @...: %NULL-terminated list of optional named arguments
public static int vips_LabQ2Lab(VipsImage in, ref VipsImage out, params object[] args)
{
    return vips_call_split("LabQ2Lab", args, ref in, ref out);
}
```