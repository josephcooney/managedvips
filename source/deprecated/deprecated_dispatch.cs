Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

// Function dispatch tables for deprecated operations.

// This file is part of VIPS.
//
// VIPS is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
// 02110-1301  USA

public class ImDeprecatedFunctions
{
    // One image in, one out.
    public static readonly string[] oneInOneOut = new string[]
    {
        "in",
        "out"
    };

    public static int QuadraticVec(ImObject[] argv)
    {
        return ImQuadratic(argv[0], argv[1], argv[2]);
    }

    public static readonly ImFunction quadraticDesc = new ImFunction
    {
        Name = "im_quadratic",
        Description = "transform via quadratic",
        Flags = ImFn.Pio,
        DispatchFunction = QuadraticVec,
        ArgListSize = 3,
        Args = new ImArgDesc[]
        {
            new ImArgDesc("in", ImInput.Image),
            new ImArgDesc("out", ImOutput.Image),
            new ImArgDesc("coeff", ImInput.Image)
        }
    };

    // Two images in, one out.
    public static readonly string[] twoInOneOut = new string[]
    {
        "in1",
        "in2",
        "out"
    };

    public static int ClipVec(ImObject[] argv)
    {
        return ImClip(argv[0], argv[1]);
    }

    public static readonly ImFunction clipDesc = new ImFunction
    {
        Name = "im_clip",
        Description = "convert to unsigned 8-bit integer",
        Flags = ImFn.Ptop | ImFn.Pio,
        DispatchFunction = ClipVec,
        ArgListSize = 2,
        Args = oneInOneOut
    };

    // ...

    public static readonly ImFunction[] deprecatedList = new ImFunction[]
    {
        argb2rgbaDesc,
        floodCopyDesc,
        floodBlobCopyDesc,
        floodOtherCopyDesc,
        clipDesc,
        c2psDesc,
        resizeLinearDesc,
        cmulnormDesc,
        fav4Desc,
        gaddDesc,
        iccExportDesc,
        litecorDesc,
        affineDesc,
        clip2cDesc,
        clip2cmDesc,
        clip2dDesc,
        clip2dcmDesc,
        clip2fDesc,
        clip2iDesc,
        convsubDesc,
        convfDesc,
        convsepfDesc,
        clip2sDesc,
        clip2uiDesc,
        insertplacesetDesc,
        clip2usDesc,
        printDesc,
        sliceDesc,
        berndDesc,
        segmentDesc,
        lineDesc,
        threshDesc,
        convfRawDesc,
        convRawDesc,
        contrastSurfaceRawDesc,
        convsepfRawDesc,
        convsepRawDesc,
        fastcorRawDesc,
        gradcorRawDesc,
        spcorRawDesc,
        lhisteqRawDesc,
        stdifRawDesc,
        rankRawDesc,
        dilateRawDesc,
        erodeRawDesc,
        similarityAreaDesc,
        similarityDesc,
        remainderconstVecDesc,
        mask2vipsDesc,
        vips2maskDesc,
        insertplaceDesc,
        circleDesc,
        andimageDesc,
        andimageConstDesc,
        andimageVecDesc,
        orimageDesc,
        orimageConstDesc,
        orimageVecDesc,
        eorimageDesc,
        eorimageConstDesc,
        eorimageVecDesc,
        shiftleftVecDesc,
        shiftleftDesc,
        shiftrightVecDesc,
        shiftrightDesc,
        blendDesc,
        equalDesc,
        equalVecDesc,
        equalConstDesc,
        ifthenelseDesc,
        lessDesc,
        lessVecDesc,
        lessConstDesc,
        lesseqDesc,
        lesseqVecDesc,
        lesseqConstDesc,
        moreDesc,
        moreVecDesc,
        moreConstDesc,
        moreeqDesc,
        moreeqVecDesc,
        moreeqConstDesc,
        notequalDesc,
        notequalVecDesc,
        notequalConstDesc,
        quadraticDesc
    };

    public static readonly ImPackage imDeprecated = new ImPackage
    {
        Name = "deprecated",
        FunctionListSize = deprecatedList.Length,
        Functions = deprecatedList
    };
}

public class ImFunction
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ImFn Flags { get; set; }
    public int DispatchFunction { get; set; }
    public int ArgListSize { get; set; }
    public ImArgDesc[] Args { get; set; }
}

public class ImPackage
{
    public string Name { get; set; }
    public int FunctionListSize { get; set; }
    public ImFunction[] Functions { get; set; }
}

public enum ImFn
{
    Pio,
    Ptop,
    Transform,
    Pio | Ptop
}

public class ImArgDesc
{
    public string Name { get; set; }
    public ImInput InputType { get; set; }
}

public enum ImInput
{
    Image,
    Double,
    Int,
    String,
    DoubleVec,
    IntVec
}

public class ImObject
{
    // ...
}

public class ImDoublevecObject : ImObject
{
    public int n;
    public double[] vec;
}

public class ImIntvecObject : ImObject
{
    public int n;
    public int[] vec;
}
```