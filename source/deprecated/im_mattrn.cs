Here is the C# equivalent of the provided C code:

```csharp
using System;

public class Vips {
    // im_mattrn: matrix transpose
    public static DoubleMask im_mattrn(DoubleMask inMatrix, string name) {
        int xc, yc;
        DoubleMask outputMatrix = null;
        double[] outArray, aArray, bArray;

        /* Allocate output matrix. */
        if ((outputMatrix = new DoubleMask(name, inMatrix.YSize, inMatrix.XSize)) == null)
            return null;
        outputMatrix.Scale = inMatrix.Scale;
        outputMatrix.Offset = inMatrix.Offset;

        /* Transpose. */
        outArray = outputMatrix.Coefficients;
        aArray = inMatrix.Coefficients;

        for (yc = 0; yc < outputMatrix.YSize; yc++) {
            bArray = aArray;

            for (xc = 0; xc < outputMatrix.XSize; xc++) {
                outArray[xc + yc * outputMatrix.XSize] = bArray[xc];
                bArray += inMatrix.XSize;
            }

            aArray++;
        }

        return outputMatrix;
    }
}

public class DoubleMask {
    public int XSize { get; set; }
    public int YSize { get; set; }
    public double Scale { get; set; }
    public double Offset { get; set; }
    public double[] Coefficients { get; set; }

    public DoubleMask(string name, int ysize, int xsize) {
        // Implementation of the constructor
    }
}
```