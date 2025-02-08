```csharp
// im_ivector
int* im_ivector(int nl, int nh)
{
    int[] v = new int[nh - nl + 1];
    return v;
}

// im_fvector
float[] im_fvector(int nl, int nh)
{
    float[] v = new float[nh - nl + 1];
    return v;
}

// im_dvector
double[] im_dvector(int nl, int nh)
{
    double[] v = new double[nh - nl + 1];
    return v;
}

// im_free_ivector
void im_free_ivector(int* v, int nl, int nh)
{
    Array.Clear(v, 0, (nh - nl + 1));
}

// im_free_fvector
void im_free_fvector(float[] v, int nl, int nh)
{
    Array.Clear(v, 0, (nh - nl + 1));
}

// im_free_dvector
void im_free_dvector(double[] v, int nl, int nh)
{
    Array.Clear(v, 0, (nh - nl + 1));
}

// im_imat_alloc
int[,] im_imat_alloc(int nrl, int nrh, int ncl, int nch)
{
    int[,] m = new int[nrh - nrl + 1, nch - ncl + 1];
    return m;
}

// im_free_imat
void im_free_imat(int[,] m, int nrl, int nrh, int ncl, int nch)
{
    for (int i = nrh; i >= nrl; i--)
        Array.Clear(m[i - nrl, 0], 0, nch - ncl + 1);
}

// im_fmat_alloc
float[,] im_fmat_alloc(int nrl, int nrh, int ncl, int nch)
{
    float[,] m = new float[nrh - nrl + 1, nch - ncl + 1];
    return m;
}

// im_free_fmat
void im_free_fmat(float[,] m, int nrl, int nrh, int ncl, int nch)
{
    for (int i = nrh; i >= nrl; i--)
        Array.Clear(m[i - nrl, 0], 0, nch - ncl + 1);
}

// im_dmat_alloc
double[,] im_dmat_alloc(int nrl, int nrh, int ncl, int nch)
{
    double[,] m = new double[nrh - nrl + 1, nch - ncl + 1];
    return m;
}

// im_free_dmat
void im_free_dmat(double[,] m, int nrl, int nrh, int ncl, int nch)
{
    for (int i = nrh; i >= nrl; i--)
        Array.Clear(m[i - nrl, 0], 0, nch - ncl + 1);
}
```