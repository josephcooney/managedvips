```csharp
// vips_dilate_uchar_hwy
public static void DilateUCharHwy(VipsRegion out_region, VipsRegion ir, VipsRect r, int sz, int nn128, int[] offsets, byte[] coeff)
{
    int bo = r.Bottom;

    const int N = 8; // Lanes(du8);
    const uint zero = 0;
    const uint one = 255;

    for (int y = r.Top; y < bo; ++y)
    {
        VipsPel[] p = ir.GetRow(y);
        VipsPel[] q = out_region.GetRow(y);

        int x = 0;
        for (; x + N <= sz; x += N)
        {
            uint sum = zero;

            for (int i = 0; i < nn128; ++i)
            {
                uint mmk = coeff[i];

                // Load with an offset.
                uint pix = p[offsets[i]];

                if (mmk != one)
                    pix &= ~one;
                sum |= pix;
            }

            q[x] = sum;
            Array.Copy(p, x, q, x, N);
        }

        for (; x < sz; ++x)
        {
            uint sum = zero;

            for (int i = 0; i < nn128; ++i)
            {
                // Load with an offset.
                uint pix = p[offsets[i]];

                if (!coeff[i])
                    pix &= ~one;
                sum |= pix;
            }

            q[x] = sum & one;
        }
    }
}

// vips_erode_uchar_hwy
public static void ErodeUCharHwy(VipsRegion out_region, VipsRegion ir, VipsRect r, int sz, int nn128, int[] offsets, byte[] coeff)
{
    int bo = r.Bottom;

    const int N = 8; // Lanes(du8);
    const uint one = 255;

    for (int y = r.Top; y < bo; ++y)
    {
        VipsPel[] p = ir.GetRow(y);
        VipsPel[] q = out_region.GetRow(y);

        int x = 0;
        for (; x + N <= sz; x += N)
        {
            uint sum = one;

            for (int i = 0; i < nn128; ++i)
            {
                uint mmk = coeff[i];

                // Load with an offset.
                uint pix = p[offsets[i]];

                if (mmk != one)
                    pix &= ~one;
                sum &= pix;
            }

            q[x] = sum;
            Array.Copy(p, x, q, x, N);
        }

        for (; x < sz; ++x)
        {
            uint sum = one;

            for (int i = 0; i < nn128; ++i)
            {
                // Load with an offset.
                uint pix = p[offsets[i]];

                if (!coeff[i])
                    pix &= ~one;
                sum &= pix;
            }

            q[x] = sum & one;
        }
    }
}
```