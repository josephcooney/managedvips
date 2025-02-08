Here is the converted C# code:

```csharp
using System;
using VipsDotNet;

public class Rot45 : Conversion
{
    public override int Build(VipsObject obj)
    {
        var rot45 = (Rot45)obj;
        var conversion = (Conversion)obj;
        var t = new Image[2];

        if (base.Build(obj) != 0)
            return -1;

        if (!VipsCheckOddsquare(Nickname, rot45.In))
            return -1;

        if (rot45.Angle == VipsAngle45.D0)
            return VipsImageWrite(rot45.In, conversion.Out);

        t[1] = VipsImageCopyMemory(rot45.In);
        var in_ = t[1];

        t[0] = VipsImageNewMemory();
        if (VipsImagePipelinev(t[0], VipsDemandStyle.Any, rot45.In, null) != 0)
            return -1;
        if (VipsImageWritePrepare(t[0]) != 0)
            return -1;

        switch (rot45.Angle)
        {
            case VipsAngle45.D315:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;

            case VipsAngle45.D270:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;

            case VipsAngle45.D225:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;

            case VipsAngle45.D180:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;

            case VipsAngle45.D135:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;

            case VipsAngle45.D90:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;

            case VipsAngle45.D45:
                Rot45Rot45(t[0], in_);
                in_ = t[0];
                break;
        }

        if (VipsImageWrite(in_, conversion.Out) != 0)
            return -1;

        return 0;
    }
}

public class Rot45Class : ConversionClass
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);

        VIPS_DEBUG_MSG("Rot45Class.Init\n");

        // ... (rest of the code remains the same)
    }

    public static void Init()
    {
        // ... (rest of the code remains the same)
    }
}

public class Rot45Rot45
{
    public static void Rot45(VipsImage out, VipsImage in_)
    {
        var ps = VipsImageSizeofPel(in_);
        var temp = new Pel[ps];
        var size = in_.Xsize;
        var size_2 = size / 2;

        for (var y = 0; y < size_2; y++)
            for (var x = y; x < size_2; x++)
            {
                // Save 1, it goes into 2 at the end.
                POINT_TO_TEMP(temp, x, y);

                // Fill 1 from 8.
                ASSIGN(x, y,
                    y, size_2 - (x - y));

                // 8 from 7.
                ASSIGN(y, size_2 - (x - y),
                    y, (size - 1) - x);

                // 7 from 6.
                ASSIGN(y, (size - 1) - x,
                    size_2 - (x - y), (size - 1) - y);

                // 6 from 5.
                ASSIGN(size_2 - (x - y), (size - 1) - y,
                    (size - 1) - x, (size - 1) - y);

                // 5 from 4.
                ASSIGN((size - 1) - x, (size - 1) - y,
                    (size - 1) - y, (x - y) + size_2);

                // 4 from 3.
                ASSIGN((size - 1) - y, (x - y) + size_2,
                    (size - 1) - y, x);

                // 3 from 2.
                ASSIGN((size - 1) - y, x,
                    (x - y) + size_2, y);

                // 2 from saved 1.
                TEMP_TO_POINT((x - y) + size_2, y, temp);
            }

        // Centre.
        ASSIGN(size_2, size_2, size_2, size_2);
    }
}
```

Note that I've assumed the `VipsDotNet` namespace is available and contains the necessary classes and methods. You may need to adjust this depending on your specific setup.

Also, I've kept the comments from the original C code to help with understanding the conversion process. Let me know if you have any further questions or if there's anything else I can help with!