Here is the C# equivalent of the provided C code:

```csharp
// im_matcat
/// <summary>
/// Matrix catenations. Returns a new matrix which is the two source matrices joined together top-bottom.
/// They must be the same width.
/// </summary>
/// <param name="top">input matrix</param>
/// <param name="bottom">input matrix</param>
/// <param name="filename">filename for output</param>
/// <returns>the joined mask on success, or null on error.</returns>
public static DoubleMask im_matcat(DoubleMask top, DoubleMask bottom, string filename)
{
    int newxsize = top.XSize;
    int newysize = top.YSize + bottom.YSize;

    // matrices must be same width
    if (top.XSize != bottom.XSize)
    {
        throw new ArgumentException("matrices must be same width");
    }

    // Allocate output matrix.
    DoubleMask mat = im_create_dmask(filename, newxsize, newysize);

    if (mat == null) return null;

    // copy first matrix then add second on the end
    Array.Copy(top.Coeff, 0, mat.Coeff, 0, top.XSize * top.YSize * sizeof(double));
    double[] out = mat.Coeff;
    Array.Copy(bottom.Coeff, 0, out, top.XSize * top.YSize, bottom.XSize * bottom.YSize * sizeof(double));

    return mat;
}
```