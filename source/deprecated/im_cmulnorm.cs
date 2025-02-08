```csharp
// im_cmulnorm.c

using System;

public class ImageMultiplyNormalise
{
    // im_cmulnorm()
    public static int ImCmulnorm(IMAGE in1, IMAGE in2, IMAGE out)
    {
        // Check if the output image can be opened for writing
        IMAGE t1 = Vips.Image.OpenLocal(out, "im_cmulnorm:1", "p");

        // Multiply two complex images and store result in t1
        if (Vips.Image.Multiply(in1, in2, t1) != 0)
            return -1;

        // Apply im_sign() to the real and imaginary parts of each pixel
        if (Vips.Image.Sign(t1, out) != 0)
            return -1;

        return 0;
    }
}
```