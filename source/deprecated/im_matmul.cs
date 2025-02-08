```csharp
// im_matmul (from vips/im_mat.c)

public static DOUBLEMASK* ImMatMul(DOUBLEMASK* in1, DOUBLEMASK* in2, string name)
{
    int xc, yc, col;
    double sum;
    DOUBLEMASK mat = new DOUBLEMASK();
    double[] out = new double[in2.XSize * in1.YSize];
    double[] a = new double[in1.XSize];
    double[] b = new double[in2.XSize];

    // Check matrix sizes.
    if (in1.XSize != in2.YSize)
    {
        ImError("im_matmul", "%s", "bad sizes");
        return null;
    }

    // Allocate output matrix.
    mat.Name = name;
    mat.Coeff = out;

    // Multiply.
    for (yc = 0; yc < in1.YSize; yc++)
    {
        double[] s2 = new double[in2.XSize];

        for (col = 0; col < in2.XSize; col++)
        {
            // Get ready to sweep a row.
            sum = 0.0;
            a = in1.Coeff;

            for (sum = 0.0, xc = 0; xc < in1.XSize; xc++)
            {
                sum += a[xc] * s2[col];
                a++;
            }

            out[yc * in2.XSize + col] = sum;
        }
    }

    return mat;
}

// im_error (from vips/im_error.c)

public static void ImError(string func, string format, params object[] args)
{
    // implementation of im_error function
}
```