Here is the C# code equivalent to the provided C code:

```csharp
using System;
using VipsDotNet;

// One image in, one out.
static im_arg_desc[] one_in_one_out = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image }
};

// Two images in, one out.
static im_arg_desc[] two_in_one_out = {
    new im_arg_desc { name = "in1", type = im_type.image },
    new im_arg_desc { name = "in2", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image }
};

// Image in, number out.
static im_arg_desc[] image_in_num_out = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "value", type = im_type.double_ }
};

// Args for im_recomb.
static im_arg_desc[] recomb_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image },
    new im_arg_desc { name = "matrix", type = im_type.dmask }
};

// Call im_recomb via arg vector.
static int recomb_vec(im_object[] argv)
{
    VipsInterpolate interpolate = (VipsInterpolate)argv[1];
    double x = argv[2].GetDouble();
    double y = argv[3].GetDouble();
    int band = argv[4].GetInt();

    return im_recomb(argv[0], argv[1], interpolate, x, y, band);
}

// Description of im_recomb.
static im_function recomb_desc = {
    name = "im_recomb",
    description = "linear recombination with mask",
    flags = im_fn_pio,
    dispatch = recomb_vec,
    arg_list_size = 3,
    arg_list = recomb_args
};

// Call im_abs via arg vector.
static int abs_vec(im_object[] argv)
{
    return im_abs(argv[0], argv[1]);
}

// Description of im_abs.
static im_function abs_desc = {
    name = "im_abs",
    description = N_("absolute value"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = abs_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_add via arg vector.
static int add_vec(im_object[] argv)
{
    return im_add(argv[0], argv[1], argv[2]);
}

// Description of im_add.
static im_function add_desc = {
    name = "im_add",
    description = N_("add two images"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = add_vec,
    arg_list_size = 3,
    arg_list = two_in_one_out
};

// Call im_avg via arg vector.
static int avg_vec(im_object[] argv)
{
    double f;

    if (im_avg(argv[0], ref f))
        return -1;

    argv[1].SetDouble(f);
    return 0;
}

// Description of im_avg.
static im_function avg_desc = {
    name = "im_avg",
    description = N_("average value of image"),
    flags = im_fn_pio,
    dispatch = avg_vec,
    arg_list_size = 2,
    arg_list = image_in_num_out
};

// Args for im_point.
static im_arg_desc[] point_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "interpolate", type = im_type.interp },
    new im_arg_desc { name = "x", type = im_type.double_ },
    new im_arg_desc { name = "y", type = im_type.double_ },
    new im_arg_desc { name = "band", type = im_type.int_ },
    new im_arg_desc { name = "out", type = im_type.double_ }
};

// Call im_point via arg vector.
static int point_vec(im_object[] argv)
{
    VipsInterpolate interpolate = (VipsInterpolate)argv[1];
    double x = argv[2].GetDouble();
    double y = argv[3].GetDouble();
    int band = argv[4].GetInt();

    return im_point(argv[0], interpolate, x, y, band);
}

// Description of im_point.
static im_function point_desc = {
    name = "im_point",
    description = N_("interpolate value at single point"),
    flags = im_fn_pio,
    dispatch = point_vec,
    arg_list_size = 6,
    arg_list = point_args
};

// Args for im_point_bilinear.
static im_arg_desc[] point_bilinear_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "x", type = im_type.double_ },
    new im_arg_desc { name = "y", type = im_type.double_ },
    new im_arg_desc { name = "band", type = im_type.int_ },
    new im_arg_desc { name = "val", type = im_type.double_ }
};

// Call im_point_bilinear via arg vector.
static int point_bilinear_vec(im_object[] argv)
{
    return im_point_bilinear(argv[0], argv[1].GetDouble(), argv[2].GetDouble(), argv[3].GetInt());
}

// Description of im_point_bilinear.
static im_function point_bilinear_desc = {
    name = "im_point_bilinear",
    description = N_("interpolate value at single point, linearly"),
    flags = im_fn_pio,
    dispatch = point_bilinear_vec,
    arg_list_size = 5,
    arg_list = point_bilinear_args
};

// Call im_deviate via arg vector.
static int deviate_vec(im_object[] argv)
{
    double f;

    if (im_deviate(argv[0], ref f))
        return -1;

    argv[1].SetDouble(f);
    return 0;
}

// Description of im_deviate.
static im_function deviate_desc = {
    name = "im_deviate",
    description = N_("standard deviation of image"),
    flags = im_fn_pio,
    dispatch = deviate_vec,
    arg_list_size = 2,
    arg_list = image_in_num_out
};

// Call im_exp10tra via arg vector.
static int exp10tra_vec(im_object[] argv)
{
    return im_exp10tra(argv[0], argv[1]);
}

// Description of im_exp10tra.
static im_function exp10tra_desc = {
    name = "im_exp10tra",
    description = N_("10^pel of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = exp10tra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_exptra via arg vector.
static int exptra_vec(im_object[] argv)
{
    return im_exptra(argv[0], argv[1]);
}

// Description of im_exptra.
static im_function exptra_desc = {
    name = "im_exptra",
    description = N_("e^pel of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = exptra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Args for im_powtra().
static im_arg_desc[] powtra_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image },
    new im_arg_desc { name = "x", type = im_type.double_ }
};

// Call im_expntra via arg vector.
static int expntra_vec(im_object[] argv)
{
    double a = argv[2].GetDouble();

    return im_expntra(argv[0], argv[1], a);
}

// Description of im_expntra.
static im_function expntra_desc = {
    name = "im_expntra",
    description = N_("x^pel of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = expntra_vec,
    arg_list_size = 3,
    arg_list = powtra_args
};

// Args for im_expntra_vec().
static im_arg_desc[] expntra_vec_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image },
    new im_arg_desc { name = "v", type = im_type.dvec }
};

// Call im_expntra_vec() via arg vector.
static int expntra_vec_vec(im_object[] argv)
{
    im_doublevec_object v = (im_doublevec_object)argv[2];

    return im_expntra_vec(argv[0], argv[1], v.n, v.vec);
}

// Description of im_expntra_vec.
static im_function expntra_vec_desc = {
    name = "im_expntra_vec",
    description = N_("[x,y,z]^pel of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = expntra_vec_vec,
    arg_list_size = 3,
    arg_list = expntra_vec_args
};

// Call im_divide via arg vector.
static int divide_vec(im_object[] argv)
{
    return im_divide(argv[0], argv[1], argv[2]);
}

// Description of im_divide.
static im_function divide_desc = {
    name = "im_divide",
    description = N_("divide two images"),
    flags = im_fn_pio,
    dispatch = divide_vec,
    arg_list_size = 3,
    arg_list = two_in_one_out
};

// Call im_invert via arg vector.
static int invert_vec(im_object[] argv)
{
    return im_invert(argv[0], argv[1]);
}

// Description of im_invert.
static im_function invert_desc = {
    name = "im_invert",
    description = N_("photographic negative"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = invert_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Args for im_lintra().
static im_arg_desc[] lintra_args = {
    new im_arg_desc { name = "a", type = im_type.double_ },
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "b", type = im_type.double_ },
    new im_arg_desc { name = "out", type = im_type.image }
};

// Call im_lintra() via arg vector.
static int lintra_vec(im_object[] argv)
{
    double a = argv[0].GetDouble();
    double b = argv[2].GetDouble();

    return im_lintra(a, argv[1], b, argv[3]);
}

// Description of im_lintra().
static im_function lintra_desc = {
    name = "im_lintra",
    description = N_("calculate a*in + b = outfile"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = lintra_vec,
    arg_list_size = 4,
    arg_list = lintra_args
};

// Args for im_lintra_vec().
static im_arg_desc[] lintra_vec_args = {
    new im_arg_desc { name = "a", type = im_type.dvec },
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "b", type = im_type.dvec },
    new im_arg_desc { name = "out", type = im_type.image }
};

// Call im_lintra_vec() via arg vector.
static int lintra_vec_vec(im_object[] argv)
{
    im_doublevec_object a = (im_doublevec_object)argv[0];
    im_doublevec_object b = (im_doublevec_object)argv[2];

    if (a.n != b.n)
        throw new Exception("vectors not equal length");

    return im_lintra_vec(a.n, a.vec, argv[1], b.vec, argv[3]);
}

// Description of im_lintra_vec().
static im_function lintra_vec_desc = {
    name = "im_lintra_vec",
    description = N_("calculate a*in + b -> out, a and b vectors"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = lintra_vec_vec,
    arg_list_size = 4,
    arg_list = lintra_vec_args
};

// Call im_log10tra via arg vector.
static int log10tra_vec(im_object[] argv)
{
    return im_log10tra(argv[0], argv[1]);
}

// Description of im_log10tra.
static im_function log10tra_desc = {
    name = "im_log10tra",
    description = N_("log10 of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = log10tra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_logtra via arg vector.
static int logtra_vec(im_object[] argv)
{
    return im_logtra(argv[0], argv[1]);
}

// Description of im_logtra.
static im_function logtra_desc = {
    name = "im_logtra",
    description = N_("ln of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = logtra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_tantra via arg vector.
static int tantra_vec(im_object[] argv)
{
    return im_tantra(argv[0], argv[1]);
}

// Description of im_tantra.
static im_function tantra_desc = {
    name = "im_tantra",
    description = N_("tan of image (angles in degrees)"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = tantra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_atantra via arg vector.
static int atantra_vec(im_object[] argv)
{
    return im_atantra(argv[0], argv[1]);
}

// Description of im_atantra.
static im_function atantra_desc = {
    name = "im_atantra",
    description = N_("atan of image (result in degrees)"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = atantra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_costra via arg vector.
static int costra_vec(im_object[] argv)
{
    return im_costra(argv[0], argv[1]);
}

// Description of im_costra.
static im_function costra_desc = {
    name = "im_costra",
    description = N_("cos of image (angles in degrees)"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = costra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_acostra via arg vector.
static int acostra_vec(im_object[] argv)
{
    return im_acostra(argv[0], argv[1]);
}

// Description of im_acostra.
static im_function acostra_desc = {
    name = "im_acostra",
    description = N_("acos of image (result in degrees)"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = acostra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_ceil via arg vector.
static int ceil_vec(im_object[] argv)
{
    return im_ceil(argv[0], argv[1]);
}

// Description of im_ceil.
static im_function ceil_desc = {
    name = "im_ceil",
    description = N_("round to smallest integer value not less than"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = ceil_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_floor via arg vector.
static int floor_vec(im_object[] argv)
{
    return im_floor(argv[0], argv[1]);
}

// Description of im_floor.
static im_function floor_desc = {
    name = "im_floor",
    description = N_("round to largest integer value not greater than"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = floor_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_rint via arg vector.
static int rint_vec(im_object[] argv)
{
    return im_rint(argv[0], argv[1]);
}

// Description of im_rint.
static im_function rint_desc = {
    name = "im_rint",
    description = N_("round to nearest integer value"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = rint_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_sintra via arg vector.
static int sintra_vec(im_object[] argv)
{
    return im_sintra(argv[0], argv[1]);
}

// Description of im_sintra.
static im_function sintra_desc = {
    name = "im_sintra",
    description = N_("sin of image (angles in degrees)"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = sintra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_bandmean via arg vector.
static int bandmean_vec(im_object[] argv)
{
    return im_bandmean(argv[0], argv[1]);
}

// Description of im_bandmean.
static im_function bandmean_desc = {
    name = "im_bandmean",
    description = N_("average image bands"),
    flags = im_fn_pio,
    dispatch = bandmean_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_sign via arg vector.
static int sign_vec(im_object[] argv)
{
    return im_sign(argv[0], argv[1]);
}

// Description of im_sign.
static im_function sign_desc = {
    name = "im_sign",
    description = N_("unit vector in direction of value"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = sign_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_asintra via arg vector.
static int asintra_vec(im_object[] argv)
{
    return im_asintra(argv[0], argv[1]);
}

// Description of im_asintra.
static im_function asintra_desc = {
    name = "im_asintra",
    description = N_("asin of image (result in degrees)"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = asintra_vec,
    arg_list_size = 2,
    arg_list = one_in_one_out
};

// Call im_max via arg vector.
static int max_vec(im_object[] argv)
{
    double f;

    if (im_max(argv[0], ref f))
        return -1;

    argv[1].SetDouble(f);
    return 0;
}

// Description of im_max.
static im_function max_desc = {
    name = "im_max",
    description = N_("maximum value of image"),
    flags = im_fn_pio,
    dispatch = max_vec,
    arg_list_size = 2,
    arg_list = image_in_num_out
};

// Args for maxpos (and minpos).
static im_arg_desc[] maxpos_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "position", type = im_type.complex }
};

// Call im_maxpos via arg vector.
static int maxpos_vec(im_object[] argv)
{
    double f;
    int x, y;

    if (im_maxpos(argv[0], ref x, ref y, ref f))
        return -1;

    ((double[])argv[1])[0] = x;
    ((double[])argv[1])[1] = y;
    return 0;
}

// Description of im_maxpos.
static im_function maxpos_desc = {
    name = "im_maxpos",
    description = N_("position of maximum value of image"),
    flags = 0,
    dispatch = maxpos_vec,
    arg_list_size = 2,
    arg_list = maxpos_args
};

// Args to im_maxpos_avg.
static im_arg_desc[] maxpos_avg_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "x", type = im_type.double_ },
    new im_arg_desc { name = "y", type = im_type.double_ },
    new im_arg_desc { name = "out", type = im_type.double_ }
};

// Call im_maxpos_avg via arg vector.
static int maxpos_avg_vec(im_object[] argv)
{
    return im_maxpos_avg(argv[0], argv[1], argv[2], argv[3]);
}

// Description of im_maxpos_avg.
static im_function maxpos_avg_desc = {
    name = "im_maxpos_avg",
    description = N_("position of maximum value of image, averaging in case of draw"),
    flags = im_fn_pio,
    dispatch = maxpos_avg_vec,
    arg_list_size = 4,
    arg_list = maxpos_avg_args
};

// Args to im_min/maxpos_vec.
static im_arg_desc[] maxpos_vec_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "n", type = im_type.int_ },
    new im_arg_desc { name = "xes", type = im_type.ivec },
    new im_arg_desc { name = "yes", type = im_type.ivec },
    new im_arg_desc { name = "maxima", type = im_type.dvec }
};

// Call im_maxpos_vec via arg vector.
static int maxpos_vec_vec(im_object[] argv)
{
    int n = argv[1].GetInt();
    im_intvec_object xes = (im_intvec_object)argv[2];
    im_intvec_object yes = (im_intvec_object)argv[3];
    im_doublevec_object maxima = (im_doublevec_object)argv[4];

    xes.vec = new int[n];
    xes.n = n;
    yes.vec = new int[n];
    yes.n = n;
    maxima.vec = new double[n];
    maxima.n = n;

    if (!xes.vec || !yes.vec || !maxima.vec ||
        im_maxpos_vec(argv[0], xes.vec, yes.vec, maxima.vec, n))
        return -1;

    return 0;
}

// Description of im_maxpos_vec.
static im_function maxpos_vec_desc = {
    name = "im_maxpos_vec",
    description = N_("position and value of n maxima of image"),
    flags = im_fn_pio,
    dispatch = maxpos_vec_vec,
    arg_list_size = 5,
    arg_list = maxpos_vec_args
};

// Call im_minpos_vec via arg vector.
static int minpos_vec_vec(im_object[] argv)
{
    int n = argv[1].GetInt();
    im_intvec_object xes = (im_intvec_object)argv[2];
    im_intvec_object yes = (im_intvec_object)argv[3];
    im_doublevec_object minima = (im_doublevec_object)argv[4];

    xes.vec = new int[n];
    xes.n = n;
    yes.vec = new int[n];
    yes.n = n;
    minima.vec = new double[n];
    minima.n = n;

    if (!xes.vec || !yes.vec || !minima.vec ||
        im_minpos_vec(argv[0], xes.vec, yes.vec, minima.vec, n))
        return -1;

    return 0;
}

// Description of im_minpos_vec.
static im_function minpos_vec_desc = {
    name = "im_minpos_vec",
    description = N_("position and value of n minima of image"),
    flags = im_fn_pio,
    dispatch = minpos_vec_vec,
    arg_list_size = 5,
    arg_list = maxpos_vec_args
};

// Args for measure.
static im_arg_desc[] measure_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "mask", type = im_type.dmask },
    new im_arg_desc { name = "x", type = im_type.int_ },
    new im_arg_desc { name = "y", type = im_type.int_ },
    new im_arg_desc { name = "w", type = im_type.int_ },
    new im_arg_desc { name = "h", type = im_type.int_ },
    new im_arg_desc { name = "h_patches", type = im_type.int_ },
    new im_arg_desc { name = "v_patches", type = im_type.int_ }
};

// Call im_measure via arg vector.
static int measure_vec(im_object[] argv)
{
    im_mask_object mo = (im_mask_object)argv[1];

    int x = argv[2].GetInt();
    int y = argv[3].GetInt();
    int w = argv[4].GetInt();
    int h = argv[5].GetInt();

    int u = argv[6].GetInt();
    int v = argv[7].GetInt();

    if (!(mo.mask =
            im_measure_area(argv[0],
                x, y, w, h, u, v, null, 0, mo.name)))
        return -1;

    return 0;
}

// Description of im_measure.
static im_function measure_desc = {
    name = "im_measure",
    description = N_("measure averages of a grid of patches"),
    flags = im_fn_pio,
    dispatch = measure_vec,
    arg_list_size = 8,
    arg_list = measure_args
};

// Call im_min via arg vector.
static int min_vec(im_object[] argv)
{
    double f;

    if (im_min(argv[0], ref f))
        return -1;

    argv[1].SetDouble(f);
    return 0;
}

// Description of im_min.
static im_function min_desc = {
    name = "im_min",
    description = N_("minimum value of image"),
    flags = im_fn_pio,
    dispatch = min_vec,
    arg_list_size = 2,
    arg_list = image_in_num_out
};

// Call im_minpos via arg vector.
static int minpos_vec(im_object[] argv)
{
    double f;
    int x, y;

    if (im_minpos(argv[0], ref x, ref y, ref f))
        return -1;

    ((double[])argv[1])[0] = x;
    ((double[])argv[1])[1] = y;
    return 0;
}

// Description of im_minpos.
static im_function minpos_desc = {
    name = "im_minpos",
    description = N_("position of minimum value of image"),
    flags = 0,
    dispatch = minpos_vec,
    arg_list_size = 2,
    arg_list = maxpos_args
};

// Call im_remainder via arg vector.
static int remainder_vec(im_object[] argv)
{
    return im_remainder(argv[0], argv[1], argv[2]);
}

// Description of im_remainder.
static im_function remainder_desc = {
    name = "im_remainder",
    description = N_("remainder after integer division"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = remainder_vec,
    arg_list_size = 3,
    arg_list = two_in_one_out
};

// Call im_remainderconst via arg vector.
static int remainderconst_vec(im_object[] argv)
{
    double c = argv[2].GetDouble();

    return im_remainderconst(argv[0], argv[1], c);
}

// Args for im_remainderconst().
static im_arg_desc[] remainderconst_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image },
    new im_arg_desc { name = "x", type = im_type.double_ }
};

// Description of im_remainderconst.
static im_function remainderconst_desc = {
    name = "im_remainderconst",
    description = N_("remainder after integer division by a constant"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = remainderconst_vec,
    arg_list_size = 3,
    arg_list = remainderconst_args
};

// Call im_remainder_vec via arg vector.
static int remainder_vec_vec(im_object[] argv)
{
    im_doublevec_object dv = (im_doublevec_object)argv[2];

    return im_remainder_vec(argv[0], argv[1], dv.n, dv.vec);
}

// Args for im_remainder_vec().
static im_arg_desc[] remainder_vec_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "out", type = im_type.image },
    new im_arg_desc { name = "x", type = im_type.dvec }
};

// Description of im_remainder_vec.
static im_function remainder_vec_desc = {
    name = "im_remainder_vec",
    description = N_("remainder after integer division by a vector of constants"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = remainder_vec_vec,
    arg_list_size = 3,
    arg_list = remainder_vec_args
};

// Call im_multiply via arg vector.
static int multiply_vec(im_object[] argv)
{
    return im_multiply(argv[0], argv[1], argv[2]);
}

// Description of im_multiply.
static im_function multiply_desc = {
    name = "im_multiply",
    description = N_("multiply two images"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = multiply_vec,
    arg_list_size = 3,
    arg_list = two_in_one_out
};

// Call im_powtra() via arg vector.
static int powtra_vec(im_object[] argv)
{
    double a = argv[2].GetDouble();

    return im_powtra(argv[0], argv[1], a);
}

// Description of im_powtra().
static im_function powtra_desc = {
    name = "im_powtra",
    description = N_("pel^x of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = powtra_vec,
    arg_list_size = 3,
    arg_list = powtra_args
};

// Call im_powtra_vec() via arg vector.
static int powtra_vec_vec(im_object[] argv)
{
    im_doublevec_object rv = (im_doublevec_object)argv[2];

    return im_powtra_vec(argv[0], argv[1], rv.n, rv.vec);
}

// Description of im_powtra_vec().
static im_function powtra_vec_desc = {
    name = "im_powtra_vec",
    description = N_("pel^[x,y,z] of image"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = powtra_vec_vec,
    arg_list_size = 3,
    arg_list = expntra_vec_args
};

// Args for im_stats.
static im_arg_desc[] stats_args = {
    new im_arg_desc { name = "in", type = im_type.image },
    new im_arg_desc { name = "statistics", type = im_type.dmask }
};

// Call im_stats() via arg vector.
static int stats_vec(im_object[] argv)
{
    im_mask_object mo = (im_mask_object)argv[1];

    if (!(mo.mask = im_stats(argv[0])))
        return -1;

    return 0;
}

// Description of im_stats().
static im_function stats_desc = {
    name = "im_stats",
    description = N_("many image statistics in one pass"),
    flags = im_fn_pio,
    dispatch = stats_vec,
    arg_list_size = 2,
    arg_list = stats_args
};

// Call im_subtract via arg vector.
static int subtract_vec(im_object[] argv)
{
    return im_subtract(argv[0], argv[1], argv[2]);
}

// Description of im_subtract.
static im_function subtract_desc = {
    name = "im_subtract",
    description = N_("subtract two images"),
    flags = im_fn_pio,
    dispatch = subtract_vec,
    arg_list_size = 3,
    arg_list = two_in_one_out
};

// Args for im_linreg.
static im_arg_desc[] linreg_args = {
    new im_arg_desc { name = "ins", type = im_type.imagevec },
    new im_arg_desc { name = "out", type = im_type.image },
    new im_arg_desc { name = "xs", type = im_type.dvec }
};

// Call im_linreg() via arg vector.
static int linreg_vec(im_object[] argv)
{
    im_imagevec_object ins_vec = (im_imagevec_object)argv[0];
    im_doublevec_object xs_vec = (im_doublevec_object)argv[2];

    IMAGE out = (IMAGE)argv[1];
    IMAGE **ins = new IMAGE[xs_vec.n + 1];

    for (int i = 0; i < ins_vec.n; ++i)
        ins[i] = ins_vec.vec[i];

    ins[ins_vec.n] = null;

    if (xs_vec.n != ins_vec.n) {
        throw new Exception("image vector and x vector differ in length");
    }

    return im_linreg(ins, out, xs_vec.vec);
}

// Description of im_linreg().
static im_function linreg_desc = {
    name = "im_linreg",
    description = N_("pixelwise linear regression"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = linreg_vec,
    arg_list_size = 3,
    arg_list = linreg_args
};

// Call im_cross_phase via arg vector.
static int cross_phase_vec(im_object[] argv)
{
    return im_cross_phase(argv[0], argv[1], argv[2]);
}

// Description of im_cross_phase.
static im_function cross_phase_desc = {
    name = "im_cross_phase",
    description = N_("phase of cross power spectrum of two complex images"),
    flags = im_fn_pio | im_fn_ptop,
    dispatch = cross_phase_vec,
    arg_list_size = 3,
    arg_list = two_in_one_out
};

// Package up all these functions.
static im_function[] arith_list = {
    abs_desc,
    acostra_desc,
    add_desc,
    asintra_desc,
    atantra_desc,
    avg_desc,
    point_desc,
    point_bilinear_desc,
    bandmean_desc,
    ceil_desc,
    costra_desc,
    cross_phase_desc,
    deviate_desc,
    divide_desc,
    exp10tra_desc,
    expntra_desc,
    expntra_vec_desc,
    exptra_desc,
    floor_desc,
    invert_desc,
    lintra_desc,
    linreg_desc,
    lintra_vec_desc,
    log10tra_desc,
    logtra_desc,
    max_desc,
    maxpos_desc,
    maxpos_avg_desc,
    maxpos_vec_desc,
    measure_desc,
    min_desc,
    minpos_desc,
    minpos_vec_desc,
    multiply_desc,
    powtra_desc,
    powtra_vec_desc,
    recomb_desc,
    remainder_desc,
    remainderconst_desc,
    remainder_vec_desc,
    rint_desc,
    sign_desc,
    sintra_desc,
    stats_desc,
    subtract_desc,
    tantra_desc
};

// Package of functions.
im_package im__arithmetic = {
    name = "arithmetic",
    size = arith_list.Length,
    list = arith_list
};
```