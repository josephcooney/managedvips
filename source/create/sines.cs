```csharp
// vips_sines_point (from vips_sines_point in the C code)

public float VipsSinesPoint(VipsImage point, int x, int y)
{
    VipsSines sines = (VipsSines)point;

    return (float)Math.Cos(sines.c * (x * sines.costheta - y * sines.sintheta));
}

// vips_sines_build (from vips_sines_build in the C code)

public int VipsSinesBuild(VipsImage image)
{
    VipsSines sines = (VipsSines)image;

    double theta;
    double factor;

    if (((VipsObject)sines).build())
        return -1;

    theta = sines.hfreq == 0.0
        ? Math.PI / 2.0
        : Math.Atan(sines.vfreq / sines.hfreq);
    factor = Math.Sqrt(sines.hfreq * sines.hfreq +
        sines.vfreq * sines.vfreq);
    sines.costheta = Math.Cos(theta);
    sines.sintheta = Math.Sin(theta);
    sines.c = factor * Math.PI * 2.0 / ((VipsPoint)sines).width;

    return 0;
}

// vips_sines_class_init (from vips_sines_class_init in the C code)

public class VipsSinesClass : VipsObjectClass
{
    public VipsSinesClass()
    {
        // Set up properties and arguments
        this.Nickname = "sines";
        this.Description = "make a 2D sine wave";

        this.Build = new Func<VipsImage, int>(VipsSinesBuild);

        this.Point = new Func<VipsImage, int, float>(VipsSinesPoint);
    }
}

// vips_sines_init (from vips_sines_init in the C code)

public VipsSines()
{
    hfreq = 0.5;
    vfreq = 0.5;
}

// vips_sines (from vips_sines in the C code)

public int VipsSines(VipsImage[] out, int width, int height, params object[] args)
{
    // Call the split function
    return VipsCallSplit("sines", out, width, height, args);
}
```