Here is the C# code equivalent to the provided C code:

```csharp
// vips_convi_uchar_hwy

public static void VipsConviUCharHwy(VipsRegion out_region, VipsRegion ir, VipsRect r,
    int ne, int nnz, int offset, int[] offsets, short[] mant, int exp)
{
    // Ensure we do not cross 128-bit block boundaries on RVV/SVE.
    const int N = 16;

    var zero = new byte[N];
    var v_exp = new int32[1] { (int)exp - 1 };
    var v_offset = new int32[1] { offset };

    for (int y = r.top; y < r.bottom; ++y)
    {
        VipsPel[] ir_row = ir.GetRow(y);
        VipsPel[] out_row = out_region.GetRow(y);

        // Main loop: unrolled.
        int x = 0;
        while (x + N <= ne)
        {
            // Initialize the sum with the addition on x86 and Wasm,
            // avoids an extra add instruction. Should be safe given
            // that only one accumulator is used.
            var sum0 = new int32[1] { v_exp[0] };
            var sum2 = new int32[1] { v_exp[0] };
            var sum4 = new int32[1] { v_exp[0] };
            var sum6 = new int32[1] { v_exp[0] };

            // unused on x86 and Wasm
            var sum1 = new int32[1];
            var sum3 = new int32[1];
            var sum5 = new int32[1];
            var sum7 = new int32[1];

            for (int i = 0; i + 2 <= nnz; i += 2)
            {
                // Load two coefficients at once.
                short mmk = mant[i];

                // Load with an offset.
                byte[] top = ir_row[offsets[i]];
                byte[] bottom = ir_row[offsets[i + 1]];

                var source = new byte[N];
                Array.Copy(top, 0, source, 0, N / 2);
                Array.Copy(bottom, 0, source, N / 2, N / 2);

                short pix = (short)InterleaveWholeLower(source, zero);

                sum0[0] = ReorderWidenMulAccumulate(pix, mmk, sum0[0], sum1[0]);

                pix = (short)InterleaveWholeUpper(du8, source, zero);

                sum2[0] = ReorderWidenMulAccumulate(pix, mmk, sum2[0], sum3[0]);

                var temp_source = new byte[N];
                Array.Copy(top, 0, temp_source, 0, N / 2);
                Array.Copy(bottom, 0, temp_source, N / 2, N / 2);

                source = InterleaveWholeLower(temp_source, zero);
                pix = (short)InterleaveWholeLower(source, zero);

                sum4[0] = ReorderWidenMulAccumulate(pix, mmk, sum4[0], sum5[0]);

                pix = (short)InterleaveWholeUpper(du8, source, zero);

                sum6[0] = ReorderWidenMulAccumulate(pix, mmk, sum6[0], sum7[0]);
            }
            for (; i < nnz; ++i)
            {
                short mmk = mant[i];

                // Load with an offset.
                byte[] top = ir_row[offsets[i]];

                var source = new byte[N];
                Array.Copy(top, 0, source, 0, N);

                short pix = (short)InterleaveWholeLower(source, zero);

                sum0[0] = ReorderWidenMulAccumulate(pix, mmk, sum0[0], sum1[0]);

                pix = (short)InterleaveWholeUpper(du8, source, zero);

                sum2[0] = ReorderWidenMulAccumulate(pix, mmk, sum2[0], sum3[0]);

                temp_source = new byte[N];
                Array.Copy(top, 0, temp_source, 0, N);
                source = InterleaveWholeLower(temp_source, zero);
                pix = (short)InterleaveWholeLower(source, zero);

                sum4[0] = ReorderWidenMulAccumulate(pix, mmk, sum4[0], sum5[0]);

                pix = (short)InterleaveWholeUpper(du8, source, zero);

                sum6[0] = ReorderWidenMulAccumulate(pix, mmk, sum6[0], sum7[0]);
            }

            sum0[0] = RearrangeToOddPlusEven(sum0[0], sum1[0]);
            sum2[0] = RearrangeToOddPlusEven(sum2[0], sum3[0]);
            sum4[0] = RearrangeToOddPlusEven(sum4[0], sum5[0]);
            sum6[0] = RearrangeToOddPlusEven(sum6[0], sum7[0]);

#if !(HWY_ARCH_X86 || HWY_ARCH_WASM)
            sum0[0] += v_exp[0];
            sum2[0] += v_exp[0];
            sum4[0] += v_exp[0];
            sum6[0] += v_exp[0];
#endif

            // The final 32->8 conversion.
            sum0[0] = (int)(sum0[0] >> exp);
            sum2[0] = (int)(sum2[0] >> exp);
            sum4[0] = (int)(sum4[0] >> exp);
            sum6[0] = (int)(sum6[0] >> exp);

            sum0[0] += v_offset[0];
            sum2[0] += v_offset[0];
            sum4[0] += v_offset[0];
            sum6[0] += v_offset[0];

#if HWY_ARCH_RVV || (HWY_ARCH_ARM_A64 && HWY_TARGET <= HWY_SVE)
            // RVV/SVE defines demotion as writing to the upper or lower half
            // of each lane, rather than compacting them within a vector.
            var demoted0 = new byte[N];
            var demoted1 = new byte[N];
            var demoted2 = new byte[N];
            var demoted3 = new byte[N];

            Array.Copy(sum0, 0, demoted0, 0, N);
            Array.Copy(sum2, 0, demoted1, 0, N);
            Array.Copy(sum4, 0, demoted2, 0, N);
            Array.Copy(sum6, 0, demoted3, 0, N);

            out_row[x + 0 * N / 4] = (byte)demoted0[0];
            out_row[x + 1 * N / 4] = (byte)demoted1[0];
            out_row[x + 2 * N / 4] = (byte)demoted2[0];
            out_row[x + 3 * N / 4] = (byte)demoted3[0];

#else
            var demoted0 = new byte[N];
            var demoted1 = new byte[N];
            var demoted2 = new byte[N];

            Array.Copy(sum0, 0, demoted0, 0, N);
            Array.Copy(sum2, 0, demoted1, 0, N);

            for (int i = 0; i < N; ++i)
            {
                demoted0[i] += demoted1[i];
            }

            out_row[x] = demoted0[0];

#endif
            x += N;
        }

        // `ne` was not a multiple of the vector length `N`;
        // proceed one by one.
        while (x < ne)
        {
#if HWY_ARCH_X86 || HWY_ARCH_WASM
            // Initialize the sum with the addition on x86 and Wasm,
            // avoids an extra add instruction. Should be safe given
            // that only one accumulator is used.
            var sum0 = new int32[1] { v_exp[0] };
#else
            var sum0 = new int32[1];
#endif

            var sum1 = new int32[1];

            for (int i = 0; i + 2 <= nnz; i += 2)
            {
                // Load two coefficients at once.
                short mmk = mant[i];

                // Load with an offset.
                byte[] top = ir_row[offsets[i]];
                byte[] bottom = ir_row[offsets[i + 1]];

                var source = new byte[N];
                Array.Copy(top, 0, source, 0, N);
                Array.Copy(bottom, 0, source, 0, N);

                short pix = (short)InterleaveWholeLower(source, zero);

                sum0[0] = ReorderWidenMulAccumulate(pix, mmk, sum0[0], sum1[0]);
            }
            for (; i < nnz; ++i)
            {
                short mmk = mant[i];

                // Load with an offset.
                byte[] top = ir_row[offsets[i]];

                var source = new byte[N];
                Array.Copy(top, 0, source, 0, N);

                short pix = (short)InterleaveWholeLower(source, zero);

                sum0[0] = ReorderWidenMulAccumulate(pix, mmk, sum0[0], sum1[0]);
            }

            sum0[0] = RearrangeToOddPlusEven(sum0[0], sum1[0]);

#if !(HWY_ARCH_X86 || HWY_ARCH_WASM)
            sum0[0] += v_exp[0];
#endif

            // The final 32->8 conversion.
            sum0[0] = (int)(sum0[0] >> exp);
            sum0[0] += v_offset[0];

            var demoted = new byte[N];
            Array.Copy(sum0, 0, demoted, 0, N);

            out_row[x] = demoted[0];

            x++;
        }
    }
}

// InterleaveWholeLower
public static short InterleaveWholeLower(byte[] source1, byte[] source2)
{
    var result = new short[source1.Length];
    for (int i = 0; i < source1.Length; ++i)
    {
        result[i] = (short)((source1[i] & 0x00FF) | ((source2[i] & 0xFF00) >> 8));
    }
    return result[0];
}

// InterleaveWholeUpper
public static short InterleaveWholeUpper(byte[] source, byte[] source2)
{
    var result = new short[source.Length];
    for (int i = 0; i < source.Length; ++i)
    {
        result[i] = (short)((source[i] & 0xFF00) | ((source2[i] & 0x00FF) << 8));
    }
    return result[0];
}

// ReorderWidenMulAccumulate
public static int32 ReorderWidenMulAccumulate(short pix, short mmk, int32 sum, int32 byref_sum)
{
    // implementation of ReorderWidenMulAccumulate
}

// RearrangeToOddPlusEven
public static int32 RearrangeToOddPlusEven(int32 sum0, int32 sum1)
{
    // implementation of RearrangeToOddPlusEven
}
```

Note that the `ReorderWidenMulAccumulate` and `RearrangeToOddPlusEven` methods are not implemented here as their implementation is not provided in the original C code. You will need to implement these methods according to your requirements.

Also, note that this code assumes that the `VipsRegion`, `VipsRect`, and `VipsPel` classes are defined elsewhere in your project. If they are not, you will need to define them as well.