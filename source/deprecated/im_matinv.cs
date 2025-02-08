Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsMath
{
    public static double[][] ImLuDecomp(double[,] mat)
    {
        // Author: Tom Vajzovic
        // Copyright: 2006, Tom Vajzovic
        // Written on: 2006-09-08

        int N = mat.GetLength(0);
        if (N != mat.GetLength(1))
            throw new ArgumentException("Non-square matrix");

        double[] rowScale = new double[N];
        double[,] lu = new double[N, N + 1];

        for (int i = 0; i < N; ++i)
            for (int j = 0; j < N; ++j)
                lu[i, j] = mat[i, j];

        for (int i = 0; i < N; ++i)
        {
            rowScale[i] = 0.0;
            for (int j = 0; j < N; ++j)
            {
                double absVal = Math.Abs(lu[i, j]);
                if (absVal > rowScale[i])
                    rowScale[i] = absVal;
            }
            if (!rowScale[i])
                throw new ArgumentException("Singular matrix");
            rowScale[i] = 1.0 / rowScale[i];
        }

        for (int j = 0; j < N; ++j)
        {
            double max = -1.0;
            int iOfMax;

            // not needed, but stops a compiler warning
            iOfMax = 0;

            // loop over ROWS in upper-half, except diagonal
            for (int i = 0; i < j; ++i)
                for (int k = 0; k < i; ++k)
                    lu[i, j] -= lu[i, k] * lu[k, j];

            // loop over ROWS in diagonal and lower-half
            for (int i = j; i < N; ++i)
            {
                double absVal;

                for (int k = 0; k < j; ++k)
                    lu[i, j] -= lu[i, k] * lu[k, j];

                // find largest element in each COLUMN scaled so that
                // largest in each ROW is 1.0
                absVal = rowScale[i] * Math.Abs(lu[i, j]);

                if (absVal > max)
                {
                    max = absVal;
                    iOfMax = i;
                }
            }

            if (Math.Abs(lu[iOfMax, j]) < 2.0 * double.Epsilon)
            {
                // divisor is near zero
                throw new ArgumentException("Singular or near-singular matrix");
            }

            if (iOfMax != j)
            {
                // swap ROWS
                for (int k = 0; k < N; ++k)
                {
                    double temp = lu[j, k];
                    lu[j, k] = lu[iOfMax, k];
                    lu[iOfMax, k] = temp;
                }
                rowScale[iOfMax] = rowScale[j];
            }

            // record permutation
            for (int k = 0; k < N; ++k)
                lu[N, j + k * N] = iOfMax;

            // divide by best (largest scaled) pivot found
            for (int i = j + 1; i < N; ++i)
                lu[i, j] /= lu[j, j];
        }

        return lu;
    }

    public static void ImLuSolve(double[,] lu, double[] vec)
    {
        int N = lu.GetLength(0);
        if (N != lu.GetLength(1) + 1)
            throw new ArgumentException("Not an LU decomposed matrix");

        for (int i = 0; i < N; ++i)
        {
            int iPerm = (int)lu[N, i];
            if (iPerm != i)
            {
                double temp = vec[i];
                vec[i] = vec[iPerm];
                vec[iPerm] = temp;
            }
            for (int j = 0; j < i; ++j)
                vec[i] -= lu[i, j] * vec[j];
        }

        for (int i = N - 1; i >= 0; --i)
        {
            for (int j = i + 1; j < N; ++j)
                vec[i] -= lu[i, j] * vec[j];

            vec[i] /= lu[i, i];
        }
    }

    public static double[,] ImMatInv(double[,] mat, string filename)
    {
        int N = mat.GetLength(0);
        if (N != mat.GetLength(1))
            throw new ArgumentException("Non-square matrix");

        double[,] inv = new double[N, N];

        if (N < 4)
        {
            return MatInvDirect(inv, mat);
        }
        else
        {
            double[,] lu = ImLuDecomp(mat);

            if (!MatInvLULu(ref inv, ref lu))
                throw new ArgumentException("Error inverting matrix");

            return inv;
        }
    }

    public static int ImMatInvInplace(double[,] mat)
    {
        int N = mat.GetLength(0);
        if (N != mat.GetLength(1))
            throw new ArgumentException("Non-square matrix");

        if (N < 4)
        {
            double[,] dup = DupDmask(mat);

            return MatInvDirect(dup, mat);
        }
        else
        {
            double[,] lu = ImLuDecomp(mat);

            if (!MatInvLULu(ref mat, ref lu))
                return -1;

            return 0;
        }
    }

    public static int ImInvmat(double[][] matrix, int size)
    {
        double[,] mat = new double[size, size];

        for (int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                mat[i, j] = matrix[i][j];

        return ImMatInvInplace(mat);
    }

    private static bool MatInvLULu(ref double[,] inv, ref double[,] lu)
    {
        int N = lu.GetLength(0);

        double[] vec = new double[N];
        for (int j = 0; j < N; ++j)
        {
            for (int i = 0; i < N; ++i)
                vec[i] = 0.0;

            vec[j] = 1.0;
            ImLuSolve(lu, vec);

            for (int i = 0; i < N; ++i)
                inv[i, j] = vec[i];
        }

        return true;
    }

    private static double[,] MatInvDirect(double[,] inv, double[,] mat)
    {
        int N = mat.GetLength(0);
        switch (N)
        {
            case 1:
                if (Math.Abs(mat[0, 0]) < 2.0 * double.Epsilon)
                    throw new ArgumentException("Singular or near-singular matrix");

                inv[0, 0] = 1.0 / mat[0, 0];
                return inv;
            case 2:
                double det = mat[0, 0] * mat[1, 1] - mat[0, 1] * mat[1, 0];

                if (Math.Abs(det) < 2.0 * double.Epsilon)
                    throw new ArgumentException("Singular or near-singular matrix");

                inv[0, 0] = mat[1, 1] / det;
                inv[0, 1] = -mat[0, 1] / det;
                inv[1, 0] = -mat[1, 0] / det;
                inv[1, 1] = mat[0, 0] / det;

                return inv;
            case 3:
                double det2 = mat[0, 0] * (mat[1, 1] * mat[2, 2] - mat[1, 2] * mat[2, 1]) -
                    mat[0, 1] * (mat[1, 0] * mat[2, 2] - mat[1, 2] * mat[2, 0]) +
                    mat[0, 2] * (mat[1, 0] * mat[2, 1] - mat[1, 1] * mat[2, 0]);

                if (Math.Abs(det2) < 2.0 * double.Epsilon)
                    throw new ArgumentException("Singular or near-singular matrix");

                inv[0, 0] = (mat[1, 1] * mat[2, 2] - mat[1, 2] * mat[2, 1]) / det2;
                inv[0, 1] = (mat[0, 2] * mat[2, 1] - mat[0, 1] * mat[2, 2]) / det2;
                inv[0, 2] = (mat[0, 1] * mat[1, 2] - mat[0, 2] * mat[1, 1]) / det2;

                inv[1, 0] = (mat[1, 2] * mat[2, 0] - mat[1, 0] * mat[2, 2]) / det2;
                inv[1, 1] = (mat[0, 0] * mat[2, 2] - mat[0, 2] * mat[2, 0]) / det2;
                inv[1, 2] = (mat[0, 2] * mat[1, 0] - mat[0, 0] * mat[1, 2]) / det2;

                inv[2, 0] = (mat[1, 0] * mat[2, 1] - mat[1, 1] * mat[2, 0]) / det2;
                inv[2, 1] = (mat[0, 1] * mat[2, 0] - mat[0, 0] * mat[2, 1]) / det2;
                inv[2, 2] = (mat[0, 0] * mat[1, 1] - mat[0, 1] * mat[1, 0]) / det2;

                return inv;
            default:
                throw new ArgumentException("Matrix size not supported");
        }
    }

    private static double[,] DupDmask(double[,] mat)
    {
        int N = mat.GetLength(0);
        double[,] dup = new double[N, N];

        for (int i = 0; i < N; ++i)
            for (int j = 0; j < N; ++j)
                dup[i, j] = mat[i, j];

        return dup;
    }
}
```
Note that I've used the `double[,]` type to represent matrices in C#. This is equivalent to the `DOUBLEMASK` type in the original C code. Also, I've removed the `IM_ARRAY` macro and replaced it with a simple array allocation using the `new` keyword.

Additionally, I've renamed some of the functions and variables to follow standard C# naming conventions.