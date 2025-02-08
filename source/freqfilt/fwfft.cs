Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

public class VipsFwfft : VipsFreqfilt
{
    private static Mutex _fftLock = new Mutex();

    public override int Build(VipsObject obj)
    {
        VipsFreqfilt freqfilt = (VipsFreqfilt)obj;
        VipsImage[] t = new VipsImage[4];
        VipsImage inImg;

        _fftInit();

        if (base.Build(obj))
            return -1;

        inImg = freqfilt.In;

        if (vipsCheckMono(base.Nickname, inImg) || vipsCheckUncoded(base.Nickname, inImg))
            return -1;

        // Convert input to a real double membuffer.
        t[1] = new VipsImage();
        if (!VipsCastDouble(inImg, out t[0], null) || !t[0].Write(t[1]))
            return -1;

        // Make the plan for the transform. Yes, they really do use nx for height and ny for width.
        double[] plannerScratch = new double[VipsImageNPels(inImg)];
        double[] halfComplex = new double[inImg.Ysize * (inImg.Xsize / 2 + 1) * 2];

        _fftLock.WaitOne();
        if (!fftwPlanDftR2c2d(inImg.Ysize, inImg.Xsize, plannerScratch, halfComplex))
        {
            _fftLock.ReleaseMutex();
            vipsError(base.Nickname, "%s", "unable to create transform plan");
            return -1;
        }
        _fftLock.ReleaseMutex();

        fftwExecuteDftR2c(fftwPlanDftR2c2d(inImg.Ysize, inImg.Xsize, plannerScratch, halfComplex));

        _fftLock.WaitOne();
        fftwDestroyPlan(fftwPlanDftR2c2d(inImg.Ysize, inImg.Xsize, plannerScratch, halfComplex));
        _fftLock.ReleaseMutex();

        // Write to out as another memory buffer.
        VipsImage outImg = new VipsImage();
        if (!VipsImagePipelinev(outImg, VipsDemandStyle.Any, inImg, null))
            return -1;
        outImg.BandFmt = VipsFormat.DpComplex;
        outImg.Type = VipsInterpretation.Fourier;

        double[] buf = new double[VipsImageNPels(outImg)];

        // Copy and normalise. The right half is the up/down and left/right flip of the left, but conjugated.
        int x, y;
        for (x = 0; x < inImg.Xsize / 2 + 1; x++)
        {
            buf[0] = halfComplex[x * 2] / VipsImageNPels(outImg);
            buf[1] = halfComplex[x * 2 + 1] / VipsImageNPels(outImg);
            halfComplex += 2;
            buf += 2;
        }

        for (x = inImg.Xsize / 2 + 1; x < outImg.Xsize; x++)
        {
            buf[0] = halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2] / VipsImageNPels(outImg);
            buf[1] = -halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2 + 1] / VipsImageNPels(outImg);
            halfComplex -= 2;
            buf += 2;
        }

        if (!VipsImageWriteLine(outImg, 0, buf))
            return -1;

        for (y = 1; y < outImg.Ysize; y++)
        {
            int offset = y * inImg.Xsize / 2 + inImg.Xsize / 2;
            for (x = 0; x < inImg.Xsize / 2 + 1; x++)
            {
                buf[0] = halfComplex[offset * 2] / VipsImageNPels(outImg);
                buf[1] = halfComplex[offset * 2 + 1] / VipsImageNPels(outImg);
                offset++;
                buf += 2;
            }

            for (x = inImg.Xsize / 2 + 1; x < outImg.Xsize; x++)
            {
                buf[0] = halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2] / VipsImageNPels(outImg);
                buf[1] = -halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2 + 1] / VipsImageNPels(outImg);
                offset--;
                buf += 2;
            }

            if (!VipsImageWriteLine(outImg, y, buf))
                return -1;
        }

        freqfilt.Out = outImg;

        return 0;
    }
}

public class VipsFwfftClass : VipsFreqfiltClass
{
    public override void ClassInit(VipsFwfftClass klass)
    {
        base.ClassInit(klass);

        klass.Nickname = "fwfft";
        klass.Description = "forward FFT";
        klass.Build = new VipsObject.VipsBuildDelegate(VipsFwfft.Build);
    }
}

public class VipsFwfftInit : VipsFreqfiltInit
{
    public override void Init(VipsFwfft obj)
    {
        base.Init(obj);
    }
}

public static class VipsFwfftMethods
{
    public static int VipsFwfft(VipsImage inImg, out VipsImage[] outImg, params object[] args)
    {
        return VipsCallSplit("fwfft", args, inImg, outImg);
    }

    private static void _fftInit()
    {
        if (_fftLock == null)
            _fftLock = new Mutex();
    }
}

public class VipsFwfftRfWfft1 : VipsObject
{
    public override int Execute(VipsObject obj, VipsImage inImg, out VipsImage[] outImg)
    {
        // Real to complex forward transform.
        VipsFwfft fwfft = (VipsFwfft)obj;
        VipsImage[] t = new VipsImage[4];

        double[] halfComplex = new double[inImg.Ysize * (inImg.Xsize / 2 + 1) * 2];
        double[] plannerScratch = new double[VipsImageNPels(inImg)];

        _fftInit();

        if (!VipsCastDouble(inImg, out t[0], null) || !t[0].Write(t[1]))
            return -1;

        _fftLock.WaitOne();
        if (!fftwPlanDftR2c2d(inImg.Ysize, inImg.Xsize, plannerScratch, halfComplex))
        {
            _fftLock.ReleaseMutex();
            vipsError("fwfft", "%s", "unable to create transform plan");
            return -1;
        }
        _fftLock.ReleaseMutex();

        fftwExecuteDftR2c(fftwPlanDftR2c2d(inImg.Ysize, inImg.Xsize, plannerScratch, halfComplex));

        _fftLock.WaitOne();
        fftwDestroyPlan(fftwPlanDftR2c2d(inImg.Ysize, inImg.Xsize, plannerScratch, halfComplex));
        _fftLock.ReleaseMutex();

        // Write to out as another memory buffer.
        VipsImage outImg = new VipsImage();
        if (!VipsImagePipelinev(outImg, VipsDemandStyle.Any, inImg, null))
            return -1;
        outImg.BandFmt = VipsFormat.DpComplex;
        outImg.Type = VipsInterpretation.Fourier;

        double[] buf = new double[VipsImageNPels(outImg)];

        // Copy and normalise. The right half is the up/down and left/right flip of the left, but conjugated.
        int x, y;
        for (x = 0; x < inImg.Xsize / 2 + 1; x++)
        {
            buf[0] = halfComplex[x * 2] / VipsImageNPels(outImg);
            buf[1] = halfComplex[x * 2 + 1] / VipsImageNPels(outImg);
            halfComplex += 2;
            buf += 2;
        }

        for (x = inImg.Xsize / 2 + 1; x < outImg.Xsize; x++)
        {
            buf[0] = halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2] / VipsImageNPels(outImg);
            buf[1] = -halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2 + 1] / VipsImageNPels(outImg);
            halfComplex -= 2;
            buf += 2;
        }

        if (!VipsImageWriteLine(outImg, 0, buf))
            return -1;

        for (y = 1; y < outImg.Ysize; y++)
        {
            int offset = y * inImg.Xsize / 2 + inImg.Xsize / 2;
            for (x = 0; x < inImg.Xsize / 2 + 1; x++)
            {
                buf[0] = halfComplex[offset * 2] / VipsImageNPels(outImg);
                buf[1] = halfComplex[offset * 2 + 1] / VipsImageNPels(outImg);
                offset++;
                buf += 2;
            }

            for (x = inImg.Xsize / 2 + 1; x < outImg.Xsize; x++)
            {
                buf[0] = halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2] / VipsImageNPels(outImg);
                buf[1] = -halfComplex[(inImg.Ysize * (inImg.Xsize - x) + inImg.Xsize / 2) * 2 + 1] / VipsImageNPels(outImg);
                offset--;
                buf += 2;
            }

            if (!VipsImageWriteLine(outImg, y, buf))
                return -1;
        }

        outImg = t[1];

        return 0;
    }
}

public class VipsFwfftCfWfft1 : VipsObject
{
    public override int Execute(VipsObject obj, VipsImage inImg, out VipsImage[] outImg)
    {
        // Complex to complex forward transform.
        VipsFwfft fwfft = (VipsFwfft)obj;
        VipsImage[] t = new VipsImage[4];

        double[] plannerScratch = new double[VipsImageNPels(inImg) * 2];
        double[] buf = new double[VipsImageNPels(outImg)];

        _fftInit();

        if (!VipsCastDpComplex(inImg, out t[0], null) || !t[0].Write(t[1]))
            return -1;

        _fftLock.WaitOne();
        if (!fftwPlanDft2d(inImg.Ysize, inImg.Xsize, plannerScratch))
        {
            _fftLock.ReleaseMutex();
            vipsError("fwfft", "%s", "unable to create transform plan");
            return -1;
        }
        _fftLock.ReleaseMutex();

        fftwExecuteDft(fftwPlanDft2d(inImg.Ysize, inImg.Xsize, plannerScratch));

        _fftLock.WaitOne();
        fftwDestroyPlan(fftwPlanDft2d(inImg.Ysize, inImg.Xsize, plannerScratch));
        _fftLock.ReleaseMutex();

        // Write to out as another memory buffer.
        VipsImage outImg = new VipsImage();
        if (!VipsImagePipelinev(outImg, VipsDemandStyle.Any, inImg, null))
            return -1;
        outImg.BandFmt = VipsFormat.DpComplex;
        outImg.Type = VipsInterpretation.Fourier;

        // Copy to out, normalise.
        int x, y;
        for (y = 0; y < outImg.Ysize; y++)
        {
            double[] q = buf;
            double* p = (double*)t[1].Data;

            for (x = 0; x < outImg.Xsize; x++)
            {
                q[0] = p[0] / VipsImageNPels(outImg);
                q[1] = p[1] / VipsImageNPels(outImg);
                p += 2;
                q += 2;
            }

            if (!VipsImageWriteLine(outImg, y, buf))
                return -1;
        }

        outImg = t[1];

        return 0;
    }
}
```