Here is the C# code equivalent to the provided C code:

```csharp
// vips_reducev_uchar_hwy

public static void VipsReduceVUCharHwy(VipsPel[] pout, VipsPel[] pin,
    int n, int ne, int lskip, short[] k)
{
#if HWY_TARGET != HWY_SCALAR
    const int N = 16;
#else
    const int N = 1; // scalar case
#endif

    for (int x = 0; x + N <= ne; x += N)
    {
        uint8_t[] p = pin.Skip(x).Take(N).ToArray();
        uint8_t[] q = pout.Skip(x).Take(N).ToArray();

#if HWY_ARCH_X86 || HWY_ARCH_WASM || HWY_TARGET == HWY_EMU128
        // Initialize the sum with the addition on x86 and Wasm,
        // avoids an extra add instruction. Should be safe given
        // that only one accumulator is used.
        long[] sum0 = new long[N];
        long[] sum2 = new long[N];
        long[] sum4 = new long[N];
        long[] sum6 = new long[N];

#else
        long[] sum0 = new long[N];
        long[] sum2 = new long[N];
        long[] sum4 = new long[N];
        long[] sum6 = new long[N];
#endif

        long[] sum1 = new long[N]; // unused on x86 and Wasm
        long[] sum3 = new long[N]; // unused on x86 and Wasm
        long[] sum5 = new long[N]; // unused on x86 and Wasm
        long[] sum7 = new long[N]; // unused on x86 and Wasm

        int i = 0;
        for (; i + 2 <= n; i += 2)
        {
            // Load two coefficients at once.
            short mmk = k[i];

            uint8_t top = p[0];
            uint8_t bottom = p[1];

            uint16 source = (uint16)((top << 8) | bottom);

            long pix = InterleaveWholeLower(source, 0);
            sum0[x] = ReorderWidenMulAccumulate(pix, mmk, sum0[x], sum1[x]);

            pix = InterleaveWholeUpper(source, 0);
            sum2[x] = ReorderWidenMulAccumulate(pix, mmk, sum2[x], sum3[x]);

            source = (uint16)((top << 8) | bottom);
            pix = InterleaveLower(source, 0);

            sum4[x] = ReorderWidenMulAccumulate(pix, mmk, sum4[x], sum5[x]);

            pix = InterleaveUpper(source, 0);
            sum6[x] = ReorderWidenMulAccumulate(pix, mmk, sum6[x], sum7[x]);
        }
        for (; i < n; ++i)
        {
            short mmk = k[i];

            uint8_t top = p[0];
            uint16 source = (uint16)((top << 8) | 0);

            long pix = InterleaveLower(source, 0);
            sum0[x] = ReorderWidenMulAccumulate(pix, mmk, sum0[x], sum1[x]);

            pix = InterleaveUpper(source, 0);
            sum2[x] = ReorderWidenMulAccumulate(pix, mmk, sum2[x], sum3[x]);

            source = (uint16)((top << 8) | 0);
            pix = InterleaveLower(source, 0);

            sum4[x] = ReorderWidenMulAccumulate(pix, mmk, sum4[x], sum5[x]);

            pix = InterleaveUpper(source, 0);
            sum6[x] = ReorderWidenMulAccumulate(pix, mmk, sum6[x], sum7[x]);
        }

        sum0 = RearrangeToOddPlusEven(sum0, sum1);
        sum2 = RearrangeToOddPlusEven(sum2, sum3);
        sum4 = RearrangeToOddPlusEven(sum4, sum5);
        sum6 = RearrangeToOddPlusEven(sum6, sum7);

#if !(HWY_ARCH_X86 || HWY_ARCH_WASM || HWY_TARGET == HWY_EMU128)
        for (int j = 0; j < N; ++j)
        {
            sum0[j] += VIPS_INTERPOLATE_SCALE >> 1;
            sum2[j] += VIPS_INTERPOLATE_SCALE >> 1;
            sum4[j] += VIPS_INTERPOLATE_SCALE >> 1;
            sum6[j] += VIPS_INTERPOLATE_SCALE >> 1;
        }
#endif

        // The final 32->8 conversion.
        for (int j = 0; j < N; ++j)
        {
            sum0[j] >>= VIPS_INTERPOLATE_SHIFT;
            sum2[j] >>= VIPS_INTERPOLATE_SHIFT;
            sum4[j] >>= VIPS_INTERPOLATE_SHIFT;
            sum6[j] >>= VIPS_INTERPOLATE_SHIFT;
        }

#if HWY_ARCH_RVV || (HWY_ARCH_ARM_A64 && HWY_TARGET <= HWY_SVE)
        // RVV/SVE defines demotion as writing to the upper or lower half
        // of each lane, rather than compacting them within a vector.
        long[] demoted0 = new long[N];
        long[] demoted1 = new long[N];
        long[] demoted2 = new long[N];
        long[] demoted3 = new long[N];

        for (int j = 0; j < N / 4; ++j)
        {
            demoted0[j * 4] = sum0[j];
            demoted1[j * 4] = sum2[j];
            demoted2[j * 4] = sum4[j];
            demoted3[j * 4] = sum6[j];
        }

#else
        long[] demoted0 = new long[N / 2];
        long[] demoted1 = new long[N / 2];
        long[] demoted2 = new long[N];

        for (int j = 0; j < N / 2; ++j)
        {
            demoted0[j] = ReorderDemote2To(sum0[j], sum2[j]);
            demoted1[j] = ReorderDemote2To(sum4[j], sum6[j]);
        }

        long[] demoted = new long[N];
        for (int j = 0; j < N / 2; ++j)
        {
            demoted[j * 2] = demoted0[j];
            demoted[j * 2 + 1] = demoted1[j];
        }
#endif

        for (int j = 0; j < N; ++j)
        {
            q[x + j] = (uint8_t)(demoted[j]);
        }
    }

    // `ne` was not a multiple of the vector length `N`;
    // proceed one by one.
    for (; x < ne; ++x)
    {
        uint8_t[] p = pin.Skip(x).Take(1).ToArray();
        uint8_t[] q = pout.Skip(x).Take(1).ToArray();

#if HWY_ARCH_X86 || HWY_ARCH_WASM || HWY_TARGET == HWY_EMU128
        // Initialize the sum with the addition on x86 and Wasm,
        // avoids an extra add instruction. Should be safe given
        // that only one accumulator is used.
        long[] sum0 = new long[1];
#else
        long[] sum0 = new long[1];
#endif

        long[] sum1 = new long[1]; // unused on x86 and Wasm

        int i = 0;
        for (; i + 2 <= n; i += 2)
        {
            // Load two coefficients at once.
            short mmk = k[i];

            uint8_t top = p[0];
            uint16 source = (uint16)((top << 8) | 0);

            long pix = InterleaveLower(source, 0);
            sum0[x] = ReorderWidenMulAccumulate(pix, mmk, sum0[x], sum1[x]);
        }
        for (; i < n; ++i)
        {
            short mmk = k[i];

            uint8_t top = p[0];
            uint16 source = (uint16)((top << 8) | 0);

            long pix = InterleaveLower(source, 0);
            sum0[x] = ReorderWidenMulAccumulate(pix, mmk, sum0[x], sum1[x]);
        }

        sum0 = RearrangeToOddPlusEven(sum0, sum1);

#if !(HWY_ARCH_X86 || HWY_ARCH_WASM || HWY_TARGET == HWY_EMU128)
        sum0[x] += VIPS_INTERPOLATE_SCALE >> 1;
#endif

        // The final 32->8 conversion.
        sum0[x] >>= VIPS_INTERPOLATE_SHIFT;

        long demoted = DemoteTo(sum0[x]);
        q[x] = (uint8_t)demoted;
    }
}
```

Note that I've assumed the `VipsPel` type is a byte array, and the `k` array contains short values. Also, I've used the `InterleaveWholeLower`, `InterleaveWholeUpper`, `ReorderWidenMulAccumulate`, `RearrangeToOddPlusEven`, `DemoteTo`, and `ReorderDemote2To` functions as they are not provided in the original code snippet. You may need to implement these functions or replace them with equivalent ones based on your specific requirements.