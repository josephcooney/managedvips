```csharp
// lu_decomp: @mat: matrix to decompose
public static VipsImage* LuDecomp(VipsImage mat)
{
    int i, j, k;
    double[] row_scale = new double[mat.Xsize];
    VipsImage lu;

    if (row_scale == null) {
        return null;
    }

    if ((lu = VipsImage.NewMatrix(mat.Xsize, mat.Xsize + 1)) == null)
    {
        g_free(row_scale);
        return null;
    }

    // copy all coefficients and then perform decomposition in-place
    Array.Copy(VIPS_MATRIX(lu, 0, 0), VIPS_MATRIX(mat, 0, 0),
        mat.Xsize * mat.Xsize * sizeof(double));

    for (i = 0; i < mat.Xsize; ++i)
    {
        row_scale[i] = 0.0;

        for (j = 0; j < mat.Xsize; ++j)
        {
            double abs_val = Math.Abs(ME(lu, i, j));

            // find largest in each ROW
            if (abs_val > row_scale[i])
                row_scale[i] = abs_val;
        }

        if (!row_scale[i]) {
            vips_error("matrixinvert", "singular matrix");
            g_object_unref(lu);
            g_free(row_scale);
            return null;
        }

        // fill array with scaling factors for each ROW
        row_scale[i] = 1.0 / row_scale[i];
    }

    for (j = 0; j < mat.Xsize; ++j) { /* loop over COLs */
        double max = -1.0;
        int i_of_max;

        // not needed, but stops a compiler warning
        i_of_max = 0;

        // loop over ROWS in upper-half, except diagonal
        for (i = 0; i < j; ++i)
            for (k = 0; k < i; ++k)
                ME(lu, i, j) -= ME(lu, i, k) * ME(lu, k, j);

        // loop over ROWS in diagonal and lower-half
        for (i = j; i < mat.Xsize; ++i) {
            double abs_val;

            for (k = 0; k < j; ++k)
                ME(lu, i, j) -= ME(lu, i, k) * ME(lu, k, j);

            // find largest element in each COLUMN scaled so that
            // largest in each ROW is 1.0
            abs_val = row_scale[i] * Math.Abs(ME(lu, i, j));

            if (abs_val > max) {
                max = abs_val;
                i_of_max = i;
            }
        }

        if (Math.Abs(ME(lu, i_of_max, j)) < TOO_SMALL) {
            // divisor is near zero
            vips_error("matrixinvert", "singular or near-singular matrix");
            g_object_unref(lu);
            g_free(row_scale);
            return null;
        }

        if (i_of_max != j) {
            // swap ROWS
            for (k = 0; k < mat.Xsize; ++k) {
                double temp = ME(lu, j, k);
                ME(lu, j, k) = ME(lu, i_of_max, k);
                ME(lu, i_of_max, k) = temp;
            }

            row_scale[i_of_max] = row_scale[j];
            // no need to copy this scale back up - we won't use it
        }

        // record permutation
        ME(lu, j, mat.Xsize) = i_of_max;

        // divide by best (largest scaled) pivot found
        for (i = j + 1; i < mat.Xsize; ++i)
            ME(lu, i, j) /= ME(lu, j, j);
    }
    g_free(row_scale);

    return lu;
}

// lu_solve: @lu: matrix to solve @vec: name for output matrix
public static int LuSolve(VipsImage lu, double[] vec)
{
    int i, j;

    if (lu.Xsize + 1 != lu.Ysize) {
        vips_error("matrixinvert", "not an LU decomposed matrix");
        return -1;
    }

    for (i = 0; i < lu.Xsize; ++i) {
        int i_perm = ME(lu, i, lu.Xsize);

        if (i_perm != i) {
            double temp = vec[i];
            vec[i] = vec[i_perm];
            vec[i_perm] = temp;
        }
        for (j = 0; j < i; ++j)
            vec[i] -= ME(lu, i, j) * vec[j];
    }

    for (i = lu.Xsize - 1; i >= 0; --i) {

        for (j = i + 1; j < lu.Xsize; ++j)
            vec[i] -= ME(lu, i, j) * vec[j];

        vec[i] /= ME(lu, i, i);
    }

    return 0;
}

// vips_matrixinvert_solve: @matrix
public static int VipsMatrixInvertSolve(VipsMatrixinvert matrix)
{
    VipsImage out = matrix.Out;

    int i, j;
    double[] vec;

    if ((matrix.Lu = LuDecomp(matrix.Mat)) == null)
        return -1;

    if ((vec = new double[matrix.Lu.Xsize]) == null)
        return -1;

    for (j = 0; j < matrix.Lu.Xsize; ++j) {
        for (i = 0; i < matrix.Lu.Xsize; ++i)
            vec[i] = 0.0;

        vec[j] = 1.0;

        if (LuSolve(matrix.Lu, vec))
            return -1;

        for (i = 0; i < matrix.Lu.Xsize; ++i)
            ME(out, i, j) = vec[i];
    }

    return 0;
}

// vips_matrixinvert_direct: @matrix
public static int VipsMatrixInvertDirect(VipsMatrixinvert matrix)
{
    VipsObjectClass class = VIPS_OBJECT_GET_CLASS(matrix);
    VipsImage in = matrix.Mat;
    VipsImage out = matrix.Out;

    switch (matrix.Mat.Xsize) {
        case 1: {
            double det = ME(in, 0, 0);

            if (Math.Abs(det) < TOO_SMALL) {
                // divisor is near zero
                vips_error(class.Nickname,
                    "%s", _("singular or near-singular matrix"));
                return -1;
            }

            ME(out, 0, 0) = 1.0 / det;
        } break;

        case 2: {
            double det = ME(in, 0, 0) * ME(in, 1, 1) -
                ME(in, 0, 1) * ME(in, 1, 0);

            double tmp;

            if (Math.Abs(det) < TOO_SMALL) {
                // divisor is near zero
                vips_error(class.Nickname,
                    "%s", _("singular or near-singular matrix"));
                return -1;
            }

            tmp = 1.0 / det;
            ME(out, 0, 0) = tmp * ME(in, 1, 1);
            ME(out, 0, 1) = -tmp * ME(in, 0, 1);
            ME(out, 1, 0) = -tmp * ME(in, 1, 0);
            ME(out, 1, 1) = tmp * ME(in, 0, 0);
        } break;

        case 3: {
            double det;
            double tmp;

            det = ME(in, 0, 0) *
                (ME(in, 1, 1) * ME(in, 2, 2) - ME(in, 1, 2) * ME(in, 2, 1));
            det -= ME(in, 0, 1) *
                (ME(in, 1, 0) * ME(in, 2, 2) - ME(in, 1, 2) * ME(in, 2, 0));
            det += ME(in, 0, 2) *
                (ME(in, 1, 0) * ME(in, 2, 1) - ME(in, 1, 1) * ME(in, 2, 0));

            if (Math.Abs(det) < TOO_SMALL) {
                // divisor is near zero
                vips_error(class.Nickname,
                    "%s", _("singular or near-singular matrix"));
                return -1;
            }

            tmp = 1.0 / det;

            ME(out, 0, 0) = tmp *
                (ME(in, 1, 1) * ME(in, 2, 2) - ME(in, 1, 2) * ME(in, 2, 1));
            ME(out, 1, 0) = tmp *
                (ME(in, 1, 2) * ME(in, 2, 0) - ME(in, 1, 0) * ME(in, 2, 2));
            ME(out, 2, 0) = tmp *
                (ME(in, 1, 0) * ME(in, 2, 1) - ME(in, 1, 1) * ME(in, 2, 0));

            ME(out, 0, 1) = tmp *
                (ME(in, 0, 2) * ME(in, 2, 1) - ME(in, 0, 1) * ME(in, 2, 2));
            ME(out, 1, 1) = tmp *
                (ME(in, 0, 0) * ME(in, 2, 2) - ME(in, 0, 2) * ME(in, 2, 0));
            ME(out, 2, 1) = tmp *
                (ME(in, 0, 1) * ME(in, 2, 0) - ME(in, 0, 0) * ME(in, 2, 1));

            ME(out, 0, 2) = tmp *
                (ME(in, 0, 1) * ME(in, 1, 2) - ME(in, 0, 2) * ME(in, 1, 1));
            ME(out, 1, 2) = tmp *
                (ME(in, 0, 2) * ME(in, 1, 0) - ME(in, 0, 0) * ME(in, 1, 2));
            ME(out, 2, 2) = tmp *
                (ME(in, 0, 0) * ME(in, 1, 1) - ME(in, 0, 1) * ME(in, 1, 0));
        } break;

        // TODO(kleisauke):
        // We sometimes use 4x4 matrices, could we also make a
        // direct version for those? For e.g.:
        // https://stackoverflow.com/a/1148405/10952119
        default:
            g_assert(0);
            return -1;
    }

    return 0;
}

// vips_matrixinvert_build: @object
public static int VipsMatrixInvertBuild(VipsObject object)
{
    VipsObjectClass class = VIPS_OBJECT_GET_CLASS(object);
    VipsMatrixinvert matrix = (VipsMatrixinvert)object;

    if (VIPS_OBJECT_CLASS(vips_matrixinvert_parent_class).Build(object))
        return -1;

    if (!VipsCheckMatrix(class.Nickname, matrix.In, ref matrix.Mat))
        return -1;

    if (matrix.Mat.Xsize != matrix.Mat.Ysize) {
        vips_error(class.Nickname, "%s", _("non-square matrix"));
        return -1;
    }

    g_object_set(matrix,
        "out", VipsImage.NewMatrix(matrix.Mat.Xsize, matrix.Mat.Ysize),
        null);

    // Direct path for < 4x4 matrices
    if (matrix.Mat.Xsize >= 4) {
        if (VipsMatrixInvertSolve(matrix))
            return -1;
    }
    else {
        if (VipsMatrixInvertDirect(matrix))
            return -1;
    }

    return 0;
}

// vips_matrixinvert_class_init: @class
public static void VipsMatrixInvertClassInit(VipsMatrixinvertClass class)
{
    GObjectClass gobject_class = G_OBJECT_CLASS(class);
    VipsObjectClass vobject_class = VIPS_OBJECT_GET_CLASS(class);

    gobject_class.Dispose += VipsMatrixInvertDispose;
    gobject_class.SetProperty += VipsObjectSetProperty;
    gobject_class.GetProperty += VipsObjectGetProperty;

    vobject_class.Nickname = "matrixinvert";
    vobject_class.Description = _("invert an matrix");
    vobject_class.Build = VipsMatrixInvertBuild;

    VIPS_ARG_IMAGE(class, "in", 0,
        _("Input"),
        _("An square matrix"),
        VIPS_ARGUMENT_REQUIRED_INPUT,
        G_STRUCT_OFFSET(VipsMatrixinvert, In));

    VIPS_ARG_IMAGE(class, "out", 1,
        _("Output"),
        _("Output matrix"),
        VIPS_ARGUMENT_REQUIRED_OUTPUT,
        G_STRUCT_OFFSET(VipsMatrixinvert, Out));
}

// vips_matrixinvert_init: @matrix
public static void VipsMatrixInvertInit(VipsMatrixinvert matrix)
{
}

// vips_matrixinvert: (method) @m: matrix to invert @out: (out): output matrix @...: %NULL-terminated list of optional named arguments
public static int VipsMatrixInvert(VipsImage m, ref VipsImage out, ...)
{
    va_list ap;
    int result;

    va_start(ap, out);
    result = VipsCallSplit("matrixinvert", ap, m, ref out);
    va_end(ap);

    return result;
}
```