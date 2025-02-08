```csharp
// add operation
//
// Copyright: 1990, N. Dessipris.
//
// Author: Nicos Dessipris
// Written on: 02/05/1990
// Modified on:
// 29/4/93 J.Cupitt
//	- now works for partial images
// 1/7/93 JC
// 	- adapted for partial v2
// 9/5/95 JC
//	- simplified: now just handles 10 cases (instead of 50), using
//	  im_clip2*() to help
//	- now uses im_wrapmany() rather than im_generate()
// 31/5/96 JC
//	- SWAP() removed, *p++ removed
// 27/9/04
//	- im__cast_and_call() now matches bands as well
//	- ... so 1 band + 4 band image -> 4 band image
// 8/12/06
// 	- add liboil support
// 18/8/08
// 	- revise upcasting system
// 	- im__cast_and_call() no longer sets bbits for you
// 	- add gtkdoc comments
// 	- remove separate complex case, just double size
// 11/9/09
// 	- im__cast_and_call() becomes im__arith_binary()
// 	- more of operation scaffold moved inside
// 25/7/10
// 	- remove oil support again ... we'll try Orc instead
// 29/10/10
// 	- move to VipsVector for Orc support
// 28/2/11
// 	- argh vector int/uint was broken
// 4/4/11
// 	- rewrite as a class
// 2/12/13
// 	- remove vector code, gcc autovec with -O3 is now as fast

using System;
using System.Collections.Generic;

public class VipsAdd : VipsArithmetic
{
    public override int ProcessLine(VipsImage image, VipsPel[] outArray, VipsPel[][] inArrays)
    {
        // Add all input types. Keep types here in sync with vips_add_format_table below.
        switch (image.Format)
        {
            case VIPS_FORMAT_UCHAR:
                Loop<unsigned char, unsigned short>(inArrays[0], inArrays[1], outArray);
                break;
            case VIPS_FORMAT_CHAR:
                Loop<signed char, signed short>(inArrays[0], inArrays[1], outArray);
                break;
            case VIPS_FORMAT_USHORT:
                Loop<unsigned short, unsigned int>(inArrays[0], inArrays[1], outArray);
                break;
            case VIPS_FORMAT_SHORT:
                Loop<signed short, signed int>(inArrays[0], inArrays[1], outArray);
                break;
            case VIPS_FORMAT_UINT:
                Loop<unsigned int, unsigned int>(inArrays[0], inArrays[1], outArray);
                break;
            case VIPS_FORMAT_INT:
                Loop<signed int, signed int>(inArrays[0], inArrays[1], outArray);
                break;

            case VIPS_FORMAT_FLOAT:
            case VIPS_FORMAT_COMPLEX:
                Loop<float, float>(inArrays[0], inArrays[1], outArray);
                break;

            case VIPS_FORMAT_DOUBLE:
            case VIPS_FORMAT_DPCOMPLEX:
                Loop<double, double>(inArrays[0], inArrays[1], outArray);
                break;

            default:
                throw new Exception("Invalid image format");
        }

        return 0;
    }
}

// Save a bit of typing.
public enum Format
{
    UC = VIPS_FORMAT_UCHAR,
    C = VIPS_FORMAT_CHAR,
    US = VIPS_FORMAT_USHORT,
    S = VIPS_FORMAT_SHORT,
    UI = VIPS_FORMAT_UINT,
    I = VIPS_FORMAT_INT,
    F = VIPS_FORMAT_FLOAT,
    X = VIPS_FORMAT_COMPLEX,
    D = VIPS_FORMAT_DOUBLE,
    DX = VIPS_FORMAT_DPCOMPLEX
}

// Type promotion for addition. Sign and value preserving. Make sure these match the case statement in add_buffer above.
public static readonly Format[] vips_add_format_table = new Format[]
{
    // Band format:  UC  C  US  S  UI  I  F  X  D  DX
    // Promotion:
    Format.US, Format.S, Format.UI, Format.I, Format.UI, Format.I, Format.F, Format.X, Format.D, Format.DX
};

public class VipsAddClass : VipsArithmeticClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        this.Nickname = "add";
        this.Description = "add two images";

        this.ProcessLine = new Func<VipsImage, VipsPel[], VipsPel[][]>(VipsAdd.ProcessLine);

        vips_arithmetic_set_format_table(this, vips_add_format_table);
    }
}

public class VipsAddInit : VipsArithmeticInit
{
    public override void Init(VipsAdd add)
    {
        base.Init(add);
    }
}

// Save a bit of typing.
public static void Loop<T1, T2>(T1[] left, T1[] right, T2[] q)
{
    for (int x = 0; x < left.Length; x++)
    {
        q[x] = (T2)(left[x] + right[x]);
    }
}

// vips_add:
// @left: input image
// @right: input image
// @out: (out): output image
// @...: %NULL-terminated list of optional named arguments
//
// This operation calculates @in1 + @in2 and writes the result to @out.
//
// If the images differ in size, the smaller image is enlarged to match the larger by adding zero pixels along the bottom and right.
//
// If the number of bands differs, one of the images must have one band. In this case, an n-band image is formed from the one-band image by joining n copies of the one-band image together, and then the two n-band images are operated upon.
//
// The two input images are cast up to the smallest common format (see table Smallest common format in <link linkend="libvips-arithmetic">arithmetic</link>), then the following table is used to determine the output type:
//
// <table>
//   <title>VipsAdd type promotion</title>
//   <tgroup cols='2' align='left' colsep='1' rowsep='1'>
//     <thead>
//       <row>
//         <entry>input type</entry>
//         <entry>output type</entry>
//       </row>
//     </thead>
//     <tbody>
//       <row>
//         <entry>uchar</entry>
//         <entry>ushort</entry>
//       </row>
//       <row>
//         <entry>char</entry>
//         <entry>short</entry>
//       </row>
//       <row>
//         <entry>ushort</entry>
//         <entry>uint</entry>
//       </row>
//       <row>
//         <entry>short</entry>
//         <entry>int</entry>
//       </row>
//       <row>
//         <entry>uint</entry>
//         <entry>uint</entry>
//       </row>
//       <row>
//         <entry>int</entry>
//         <entry>int</entry>
//       </row>
//       <row>
//         <entry>float</entry>
//         <entry>float</entry>
//       </row>
//       <row>
//         <entry>double</entry>
//         <entry>double</entry>
//       </row>
//       <row>
//         <entry>complex</entry>
//         <entry>complex</entry>
//       </row>
//       <row>
//         <entry>double complex</entry>
//         <entry>double complex</entry>
//       </row>
//     </tbody>
//   </tgroup>
// </table>
//
// In other words, the output type is just large enough to hold the whole range of possible values.
//
// See also: vips_subtract(), vips_linear().
//
// Returns: 0 on success, -1 on error
public static int VipsAdd(VipsImage left, VipsImage right, ref VipsImage out)
{
    // Call the ProcessLine method to perform the addition operation.
    return new VipsAdd().ProcessLine(left, out.PelArray, new VipsPel[][] { left.PelArray, right.PelArray });
}
```