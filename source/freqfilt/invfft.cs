Here is the C# code equivalent to the provided C code:
```csharp
using System;
using System.Threading;

public class VipsInvfft : VipsFreqfilt
{
    public bool Real { get; set; }

    protected override int Build(VipsObject obj)
    {
        var freqfilt = (VipsFreqfilt)obj;
        var invfft = (VipsInvfft)obj;
        var t = new VipsImage[4];

        VipsImage inImg;

        vips__fft_init();

        if (base.Build(obj))
            return -1;

        inImg = freqfilt.In;

        if (vips_image_decode(inImg, ref t[0]))
            return -1;
        inImg = t[0];

        if (invfft.Real)
        {
            if (vips__fftproc(invfft, inImg, ref t[1], RInvfft1))
                return -1;
        }
        else
        {
            if (vips__fftproc(invfft, inImg, ref t[1], CInvfft1))
                return -1;
        }

        if (vips_image_write(t[1], freqfilt.Out))
            return -1;

        return 0;
    }

    protected override void ClassInit(VipsInvfftClass class_)
    {
        base.ClassInit(class_);

        var gobject_class = G_OBJECT_CLASS(class_);
        var vobject_class = VIPS_OBJECT_CLASS(class_);

        gobject_class.SetProperty += VipsObject.SetProperty;
        gobject_class.GetProperty += VipsObject.GetProperty;

        vobject_class.Nickname = "invfft";
        vobject_class.Description = _("inverse FFT");
        vobject_class.Build = Build;

        VIPS_ARG_BOOL(class_, "real", 4,
            _("Real"),
            _("Output only the real part of the transform"),
            VIPS_ARGUMENT_OPTIONAL_INPUT,
            typeof(VipsInvfft).GetProperty("Real").GetGetMethod(),
            false);
    }

    protected override void Init()
    {
        base.Init();
    }
}

public class CInvfft1
{
    public int Cinvfft1(VipsObject obj, VipsImage inImg, ref VipsImage outImg)
    {
        var t = new VipsImage[4];
        var invfft = (VipsInvfft)obj;
        var class_ = VIPS_OBJECT_GET_CLASS(invfft);

        if (vips_check_mono(class_.Nickname, inImg) ||
            vips_check_uncoded(class_.Nickname, inImg))
            return -1;

        outImg = vips_image_new_memory();
        if (vips_cast_dpcomplex(inImg, ref t[0], null) ||
            vips_image_write(t[0], outImg))
            return -1;

        var planner_scratch = new double[VIPS_IMAGE_N_PELS(inImg) * 2];
        lock (vips__fft_lock)
        {
            var plan = fftw_plan_dft_2d(inImg.Ysize, inImg.Xsize,
                (fftw_complex)planner_scratch,
                (fftw_complex)planner_scratch,
                FFTW_BACKWARD,
                0);
            if (plan == null)
            {
                vips_error(class_.Nickname, "%s", _("unable to create transform plan"));
                return -1;
            }
        }

        fftw_execute_dft(plan,
            (fftw_complex)outImg.Data, (fftw_complex)outImg.Data);

        lock (vips__fft_lock)
        {
            fftw_destroy_plan(plan);
        }

        outImg.Type = VIPS_INTERPRETATION_B_W;

        return 0;
    }
}

public class RInvfft1
{
    public int RInvfft1(VipsObject obj, VipsImage inImg, ref VipsImage outImg)
    {
        var t = new VipsImage[4];
        var invfft = (VipsInvfft)obj;
        var class_ = VIPS_OBJECT_GET_CLASS(invfft);
        const int half_width = inImg.Xsize / 2 + 1;

        double[] half_complex;
        double[] planner_scratch;
        fftw_plan plan;
        int x, y;
        double[] q, p;

        t[1] = vips_image_new_memory();
        if (vips_cast_dpcomplex(inImg, ref t[0], null) ||
            vips_image_write(t[0], t[1]))
            return -1;

        half_complex = new double[t[1].Ysize * half_width * 2];
        q = half_complex;
        for (y = 0; y < t[1].Ysize; y++)
        {
            p = ((double[])t[1].Data) + (uint)y * t[1].Xsize * 2;

            for (x = 0; x < half_width; x++)
            {
                q[0] = p[0];
                q[1] = p[1];
                p += 2;
                q += 2;
            }
        }

        outImg = vips_image_new_memory();
        if (vips_image_pipelinev(outImg, VIPS_DEMAND_STYLE_ANY, t[1], null))
            return -1;
        outImg.BandFmt = VIPS_FORMAT_DOUBLE;
        outImg.Type = VIPS_INTERPRETATION_B_W;
        if (vips_image_write_prepare(outImg))
            return -1;

        planner_scratch = new double[t[1].Ysize * half_width * 2];
        lock (vips__fft_lock)
        {
            plan = fftw_plan_dft_c2r_2d(t[1].Ysize, t[1].Xsize,
                (fftw_complex)planner_scratch, (double[])outImg.Data,
                0);
            if (plan == null)
            {
                vips_error(class_.Nickname, "%s", _("unable to create transform plan"));
                return -1;
            }
        }

        fftw_execute_dft_c2r(plan,
            (fftw_complex)half_complex, (double[])outImg.Data);

        lock (vips__fft_lock)
        {
            fftw_destroy_plan(plan);
        }

        return 0;
    }
}

public static class VipsInvfftMethods
{
    public static int VipsInvfft(VipsImage inImg, ref VipsImage outImg, params object[] args)
    {
        var result = vips_call_split("invfft", args, inImg, ref outImg);
        return result;
    }
}
```
Note that I've assumed the existence of certain classes and methods (e.g. `VipsObject`, `VipsImage`, `vips_check_mono`, etc.) which are not defined in this code snippet. You will need to ensure that these classes and methods are properly implemented elsewhere in your codebase.

Also, I've used the `lock` statement to implement the mutex locking mechanism, as it is not directly available in C#. The `lock` statement acquires a lock on the specified object, allowing only one thread to execute the code within the lock at any given time.