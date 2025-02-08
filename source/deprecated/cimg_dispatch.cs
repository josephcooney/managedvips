Here is the converted C# code:

```csharp
// Function dispatch tables for cimg wrappers.

using System;

public class ImCimg
{
    // This file is part of VIPS.
    //
    // VIPS is free software; you can redistribute it and/or modify
    // it under the terms of the GNU Lesser General Public License as published by
    // the Free Software Foundation; either version 2 of the License, or
    // (at your option) any later version.
    //
    // This program is distributed in the hope that it will be useful,
    // but WITHOUT ANY WARRANTY; without even the implied warranty of
    // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    // GNU Lesser General Public License for more details.
    //
    // You should have received a copy of the GNU Lesser General Public License
    // along with this program; if not, write to the Free Software
    // Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
    // 02110-1301  USA

    public static int GreycVec(IMAGE src, IMAGE dst)
    {
        // greyc_vec function from VIPS.
        //
        // This is a noise-removing filter.

        int iterations = (int)src.GetArgument(2);
        double amplitude = (double)src.GetArgument(3);
        double sharpness = (double)src.GetArgument(4);
        double anisotropy = (double)src.GetArgument(5);
        double alpha = (double)src.GetArgument(6);
        double sigma = (double)src.GetArgument(7);
        double dl = (double)src.GetArgument(8);
        double da = (double)src.GetArgument(9);
        double gaussPrec = (double)src.GetArgument(10);
        int interpolation = (int)src.GetArgument(11);
        int fastApprox = (int)src.GetArgument(12);

        if (!Vips.GreycMask(src, dst, null,
            iterations,
            amplitude, sharpness, anisotropy,
            alpha, sigma,
            dl, da, gaussPrec,
            interpolation, fastApprox))
        {
            return -1;
        }

        return 0;
    }

    public static im_arg_desc[] GreycArgTypes = new im_arg_desc[]
    {
        new im_arg_desc("src", typeof(IMAGE)),
        new im_arg_desc("dst", typeof(IMAGE)),
        new im_arg_desc("iterations", typeof(int)),
        new im_arg_desc("amplitude", typeof(double)),
        new im_arg_desc("sharpness", typeof(double)),
        new im_arg_desc("anisotropy", typeof(double)),
        new im_arg_desc("alpha", typeof(double)),
        new im_arg_desc("sigma", typeof(double)),
        new im_arg_desc("dl", typeof(double)),
        new im_arg_desc("da", typeof(double)),
        new im_arg_desc("gaussPrec", typeof(double)),
        new im_arg_desc("interpolation", typeof(int)),
        new im_arg_desc("fastApprox", typeof(int))
    };

    public static im_function GreycDesc = new im_function
    {
        Name = "im_greyc",
        Description = "noise-removing filter",
        Flags = (im_fn_flags)(IM_FN_TRANSFORM | IM_FN_PIO),
        DispatchFunction = GreycVec,
        ArgListSize = GreycArgTypes.Length,
        ArgList = GreycArgTypes
    };

    public static int GreycMaskVec(IMAGE src, IMAGE dst, IMAGE mask)
    {
        // greyc_mask_vec function from VIPS.
        //
        // This is a noise-removing filter with a mask.

        int iterations = (int)src.GetArgument(3);
        double amplitude = (double)src.GetArgument(4);
        double sharpness = (double)src.GetArgument(5);
        double anisotropy = (double)src.GetArgument(6);
        double alpha = (double)src.GetArgument(7);
        double sigma = (double)src.GetArgument(8);
        double dl = (double)src.GetArgument(9);
        double da = (double)src.GetArgument(10);
        double gaussPrec = (double)src.GetArgument(11);
        int interpolation = (int)src.GetArgument(12);
        int fastApprox = (int)src.GetArgument(13);

        if (!Vips.GreycMask(src, dst, mask,
            iterations,
            amplitude, sharpness, anisotropy,
            alpha, sigma,
            dl, da, gaussPrec,
            interpolation, fastApprox))
        {
            return -1;
        }

        return 0;
    }

    public static im_arg_desc[] GreycMaskArgTypes = new im_arg_desc[]
    {
        new im_arg_desc("src", typeof(IMAGE)),
        new im_arg_desc("dst", typeof(IMAGE)),
        new im_arg_desc("mask", typeof(IMAGE)),
        new im_arg_desc("iterations", typeof(int)),
        new im_arg_desc("amplitude", typeof(double)),
        new im_arg_desc("sharpness", typeof(double)),
        new im_arg_desc("anisotropy", typeof(double)),
        new im_arg_desc("alpha", typeof(double)),
        new im_arg_desc("sigma", typeof(double)),
        new im_arg_desc("dl", typeof(double)),
        new im_arg_desc("da", typeof(double)),
        new im_arg_desc("gaussPrec", typeof(double)),
        new im_arg_desc("interpolation", typeof(int)),
        new im_arg_desc("fastApprox", typeof(int))
    };

    public static im_function GreycMaskDesc = new im_function
    {
        Name = "im_greyc_mask",
        Description = "noise-removing filter, with a mask",
        Flags = (im_fn_flags)(IM_FN_TRANSFORM | IM_FN_PIO),
        DispatchFunction = GreycMaskVec,
        ArgListSize = GreycMaskArgTypes.Length,
        ArgList = GreycMaskArgTypes
    };

    public static im_function[] FunctionList = new im_function[]
    {
        GreycDesc,
        GreycMaskDesc
    };

    // Package of functions.
    public class ImPackage : im_package
    {
        public string Name { get; set; }
        public int ArgCount { get; set; }
        public im_function[] Functions { get; set; }

        public ImPackage(string name, int argCount, im_function[] functions)
        {
            Name = name;
            ArgCount = argCount;
            Functions = functions;
        }
    }

    public static ImPackage ImCimg = new ImPackage("cimg", FunctionList.Length, FunctionList);
}
```

Note: I've assumed that the `Vips` class and `im_package`, `im_function`, `im_arg_desc`, `IM_FN_TRANSFORM`, `IM_FN_PIO` types are defined elsewhere in your codebase. If not, you'll need to define them as well.

Also note that this is a direct translation of the C code, without any optimizations or improvements specific to C#.