Here is the C# code equivalent to the provided C code:

```csharp
using System;

public class VipsFloat2Rad : VipsColourCode
{
    public override void Line(VipsColour colour, VipsPel[] outArray, VipsPel[][] inArray, int width)
    {
        COLOR[] inp = (COLOR[])inArray[0];
        COLR[] outBuf = (COLR[])outArray;

        while (width-- > 0)
        {
            SetColr(outBuf[0], inp[0][RED], inp[0][GRN], inp[0][BLU]);
            inp++;
            outBuf++;
        }
    }

    public override void ClassInit(VipsFloat2RadClass classType)
    {
        VipsObjectClass objectClass = (VipsObjectClass)classType;
        VipsColourClass colourClass = VIPS_COLOUR_CLASS(classType);

        objectClass.Nickname = "float2rad";
        objectClass.Description = _("transform float RGB to Radiance coding");

        colourClass.ProcessLine = Line;
    }

    public override void Init(VipsFloat2Rad float2Rad)
    {
        VipsColour colour = VIPS_COLOUR(float2Rad);
        VipsColourCode code = VIPS_COLOUR_CODE(float2Rad);

        colour.Coding = VIPS_CODING_RAD;
        colour.Interpretation = VIPS_INTERPRETATION_scRGB;
        colour.Format = VIPS_FORMAT_UCHAR;
        colour.InputBands = 3;
        colour.Bands = 4;

        code.InputCoding = VIPS_CODING_NONE;
        code.InputFormat = VIPS_FORMAT_FLOAT;
    }
}

public class VipsFloat2RadClass : VipsColourCodeClass
{
    public override void Init(VipsObjectClass objectClass)
    {
        base.Init(objectClass);
        ClassInit(this);
    }

    public override void ClassInit()
    {
        VipsObjectClass objectClass = (VipsObjectClass)this;
        VipsColourClass colourClass = VIPS_COLOUR_CLASS(this);

        objectClass.Nickname = "float2rad";
        objectClass.Description = _("transform float RGB to Radiance coding");

        colourClass.ProcessLine = new VipsColourClass.ProcessLineDelegate(Line);
    }
}

public class VipsFloat2RadInit : VipsObjectInit
{
    public override void Init(VipsObject obj)
    {
        VipsFloat2Rad float2Rad = (VipsFloat2Rad)obj;
        VipsColour colour = VIPS_COLOUR(float2Rad);
        VipsColourCode code = VIPS_COLOUR_CODE(float2Rad);

        colour.Coding = VIPS_CODING_RAD;
        colour.Interpretation = VIPS_INTERPRETATION_scRGB;
        colour.Format = VIPS_FORMAT_UCHAR;
        colour.InputBands = 3;
        colour.Bands = 4;

        code.InputCoding = VIPS_CODING_NONE;
        code.InputFormat = VIPS_FORMAT_FLOAT;
    }
}

public class VipsFloat2Rad : VipsObject
{
    public override int Call(VipsImage inImage, VipsImage[] outImages)
    {
        return vips_call_split("float2rad", inImage, outImages);
    }

    public static void Main(string[] args)
    {
        // Usage example:
        // VipsImage image = new VipsImage();
        // VipsFloat2Rad float2Rad = new VipsFloat2Rad();
        // int result = float2Rad.Call(image, null);
    }
}
```

Note that this code assumes the existence of other classes and methods in the `Vips` namespace, which are not provided here. The `SetColr` method is also assumed to be implemented elsewhere.

Also note that C# does not support direct conversion of C code due to differences in syntax and semantics between the two languages. This implementation is a manual translation of the original C code into equivalent C# code.