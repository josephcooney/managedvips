Here is the converted C# code:

```csharp
// Function dispatch tables for freq_filt.
//
// J. Cupitt, 23/2/95
// 22/4/97 JC
//  - oops, im_freqflt() was wrong

using System;

namespace VIPS {
    public class ImObject {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ImFunction {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Flags { get; set; }
        public Func<ImObject[], int> DispatchFunction { get; set; }
        public int ArgListSize { get; set; }
        public ImArgDesc[] ArgList { get; set; }
    }

    public class ImArgDesc {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public static class VipsFunctions {
        // One image in, one out.
        private static readonly ImFunction create_fmask_desc = new ImFunction {
            Name = "im_create_fmask",
            Description = "create frequency domain filter mask",
            Flags = 0,
            DispatchFunction = CreateFmaskVec,
            ArgListSize = 9,
            ArgList = new[] {
                new ImArgDesc { Name = "out", Type = typeof(ImObject) },
                new ImArgDesc { Name = "width", Type = typeof(int) },
                new ImArgDesc { Name = "height", Type = typeof(int) },
                new ImArgDesc { Name = "type", Type = typeof(int) },
                new ImArgDesc { Name = "p1", Type = typeof(double) },
                new ImArgDesc { Name = "p2", Type = typeof(double) },
                new ImArgDesc { Name = "p3", Type = typeof(double) },
                new ImArgDesc { Name = "p4", Type = typeof(double) },
                new ImArgDesc { Name = "p5", Type = typeof(double) }
            }
        };

        // Call im_create_fmask via arg vector.
        private static int CreateFmaskVec(ImObject[] argv) {
            int width = (int)argv[1].Id;
            int height = (int)argv[2].Id;
            int type = (int)argv[3].Id;
            double p1 = (double)argv[4].Id;
            double p2 = (double)argv[5].Id;
            double p3 = (double)argv[6].Id;
            double p4 = (double)argv[7].Id;
            double p5 = (double)argv[8].Id;

            return Vips.CreateFmask(argv[0], width, height,
                type, p1, p2, p3, p4, p5);
        }

        // Description of im_create_fmask.
        public static ImFunction CreateFmaskDesc => create_fmask_desc;

        // Args to im_flt_image_freq().
        private static readonly ImArgDesc[] flt_image_freq_args = new[] {
            new ImArgDesc { Name = "in", Type = typeof(ImObject) },
            new ImArgDesc { Name = "out", Type = typeof(ImObject) },
            new ImArgDesc { Name = "type", Type = typeof(int) },
            new ImArgDesc { Name = "p1", Type = typeof(double) },
            new ImArgDesc { Name = "p2", Type = typeof(double) },
            new ImArgDesc { Name = "p3", Type = typeof(double) },
            new ImArgDesc { Name = "p4", Type = typeof(double) },
            new ImArgDesc { Name = "p5", Type = typeof(double) }
        };

        // Call im_flt_image_freq via arg vector.
        private static int FltImageFreqVec(ImObject[] argv) {
            int type = (int)argv[2].Id;
            double p1 = (double)argv[3].Id;
            double p2 = (double)argv[4].Id;
            double p3 = (double)argv[5].Id;
            double p4 = (double)argv[6].Id;
            double p5 = (double)argv[7].Id;

            return Vips.FltImageFreq(argv[0], argv[1],
                type, p1, p2, p3, p4, p5);
        }

        // Description of im_flt_image_freq.
        public static ImFunction FltImageFreqDesc => new ImFunction {
            Name = "im_flt_image_freq",
            Description = "frequency domain filter image",
            Flags = 0,
            DispatchFunction = FltImageFreqVec,
            ArgListSize = flt_image_freq_args.Length,
            ArgList = flt_image_freq_args
        };

        // Args to im_fractsurf().
        private static readonly ImArgDesc[] fractsurf_args = new[] {
            new ImArgDesc { Name = "out", Type = typeof(ImObject) },
            new ImArgDesc { Name = "size", Type = typeof(int) },
            new ImArgDesc { Name = "dimension", Type = typeof(double) }
        };

        // Call im_fractsurf via arg vector.
        private static int FractsurfVec(ImObject[] argv) {
            int size = (int)argv[1].Id;
            double dim = (double)argv[2].Id;

            return Vips.Fractsurf(argv[0], size, dim);
        }

        // Description of im_fractsurf.
        public static ImFunction FractsurfDesc => new ImFunction {
            Name = "im_fractsurf",
            Description = "generate a fractal surface of given dimension",
            Flags = 1,
            DispatchFunction = FractsurfVec,
            ArgListSize = fractsurf_args.Length,
            ArgList = fractsurf_args
        };

        // Args to im_freqflt().
        private static readonly ImArgDesc[] freqflt_args = new[] {
            new ImArgDesc { Name = "in", Type = typeof(ImObject) },
            new ImArgDesc { Name = "mask", Type = typeof(ImObject) },
            new ImArgDesc { Name = "out", Type = typeof(ImObject) }
        };

        // Call im_freqflt via arg vector.
        private static int FreqFltVec(ImObject[] argv) {
            return Vips.FreqFlt(argv[0], argv[1], argv[2]);
        }

        // Description of im_freqflt.
        public static ImFunction FreqFltDesc => new ImFunction {
            Name = "im_freqflt",
            Description = "frequency-domain filter of in with mask",
            Flags = 1,
            DispatchFunction = FreqFltVec,
            ArgListSize = freqflt_args.Length,
            ArgList = freqflt_args
        };

        // Call im_disp_ps via arg vector.
        private static int DispPsVec(ImObject[] argv) {
            return Vips.DispPs(argv[0], argv[1]);
        }

        // Description of im_disp_ps.
        public static ImFunction DispPsDesc => new ImFunction {
            Name = "im_disp_ps",
            Description = "make displayable power spectrum",
            Flags = 1,
            DispatchFunction = DispPsVec,
            ArgListSize = 2,
            ArgList = new[] {
                new ImArgDesc { Name = "in", Type = typeof(ImObject) },
                new ImArgDesc { Name = "out", Type = typeof(ImObject) }
            }
        };

        // Call im_rotquad via arg vector.
        private static int RotQuadVec(ImObject[] argv) {
            return Vips.RotQuad(argv[0], argv[1]);
        }

        // Description of im_rotquad.
        public static ImFunction RotQuadDesc => new ImFunction {
            Name = "im_rotquad",
            Description = "rotate image quadrants to move origin to centre",
            Flags = 1,
            DispatchFunction = RotQuadVec,
            ArgListSize = 2,
            ArgList = new[] {
                new ImArgDesc { Name = "in", Type = typeof(ImObject) },
                new ImArgDesc { Name = "out", Type = typeof(ImObject) }
            }
        };

        // Call im_fwfft via arg vector.
        private static int FwFftVec(ImObject[] argv) {
            return Vips.FwFft(argv[0], argv[1]);
        }

        // Description of im_fwfft.
        public static ImFunction FwFftDesc => new ImFunction {
            Name = "im_fwfft",
            Description = "forward fast-fourier transform",
            Flags = 1,
            DispatchFunction = FwFftVec,
            ArgListSize = 2,
            ArgList = new[] {
                new ImArgDesc { Name = "in", Type = typeof(ImObject) },
                new ImArgDesc { Name = "out", Type = typeof(ImObject) }
            }
        };

        // Call im_invfft via arg vector.
        private static int InvFftVec(ImObject[] argv) {
            return Vips.InvFft(argv[0], argv[1]);
        }

        // Description of im_invfft.
        public static ImFunction InvFftDesc => new ImFunction {
            Name = "im_invfft",
            Description = "inverse fast-fourier transform",
            Flags = 1,
            DispatchFunction = InvFftVec,
            ArgListSize = 2,
            ArgList = new[] {
                new ImArgDesc { Name = "in", Type = typeof(ImObject) },
                new ImArgDesc { Name = "out", Type = typeof(ImObject) }
            }
        };

        // Call im_invfftr via arg vector.
        private static int InvFftrVec(ImObject[] argv) {
            return Vips.InvFftr(argv[0], argv[1]);
        }

        // Description of im_invfftr.
        public static ImFunction InvFftrDesc => new ImFunction {
            Name = "im_invfftr",
            Description = "real part of inverse fast-fourier transform",
            Flags = 1,
            DispatchFunction = InvFftrVec,
            ArgListSize = 2,
            ArgList = new[] {
                new ImArgDesc { Name = "in", Type = typeof(ImObject) },
                new ImArgDesc { Name = "out", Type = typeof(ImObject) }
            }
        };

        // Call im_phasecor_fft via arg vector.
        private static int PhaseCorFftVec(ImObject[] argv) {
            return Vips.PhaseCorFft(argv[0], argv[1], argv[2]);
        }

        // Description of im_phasecor_fft.
        public static ImFunction PhaseCorFftDesc => new ImFunction {
            Name = "im_phasecor_fft",
            Description = "non-normalised correlation of gradient of in2 within in1",
            Flags = 1,
            DispatchFunction = PhaseCorFftVec,
            ArgListSize = 3,
            ArgList = new[] {
                new ImArgDesc { Name = "in1", Type = typeof(ImObject) },
                new ImArgDesc { Name = "in2", Type = typeof(ImObject) },
                new ImArgDesc { Name = "out", Type = typeof(ImObject) }
            }
        };

        // Package up all these functions.
        public static readonly ImFunction[] freq_list = new[] {
            CreateFmaskDesc,
            DispPsDesc,
            FltImageFreqDesc,
            FractsurfDesc,
            FreqFltDesc,
            FwFftDesc,
            RotQuadDesc,
            InvFftDesc,
            PhaseCorFftDesc,
            InvFftrDesc
        };

        // Package of functions.
        public static readonly ImPackage im__freq_filt = new ImPackage {
            Name = "freq_filt",
            FunctionCount = freq_list.Length,
            Functions = freq_list
        };
    }
}
```

Note that I've assumed the `Vips` class has methods for each of the VIPS functions, and that these methods take the required arguments. You may need to modify this code to match your actual implementation.

Also note that I've used a simple `Func<ImObject[], int>` delegate for the dispatch function, which assumes that the dispatch function takes an array of `ImObject` instances as input and returns an integer result. If your dispatch functions have different signatures, you'll need to modify this code accordingly.