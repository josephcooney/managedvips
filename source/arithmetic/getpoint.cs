```csharp
// vips_getpoint_build
public class VipsGetpoint : VipsOperation
{
    public VipsImage In { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool UnpackComplex { get; set; }

    public VipsArrayDouble OutArray { get; private set; }

    protected override int Build()
    {
        // Unpack to double. Complex unpacks to 2 * bands.
        var iscomplex = UnpackComplex && VipsBandFormat.IsComplex(In.BandFmt);
        var targetBands = iscomplex ? In.Bands * 2 : In.Bands;
        var targetFormat = iscomplex ? VipsFormat.DpComplex : VipsFormat.Double;

        // Crop, decode and unpack to double.
        if (VipsCrop(In, out var croppedImage, X, Y, 1, 1) ||
            VipsImageDecode(croppedImage, out var decodedImage) ||
            VipsCast(decodedImage, out var castedImage, targetFormat))
            return -1;

        // To a mem buffer, then copy to out.
        if (!(var memoryBuffer = VipsImageNewMemory()) ||
            VipsImageWrite(castedImage, memoryBuffer))
            return -1;

        var vector = new double[targetBands];
        Array.Copy(memoryBuffer.Data, vector, targetBands);
        OutArray = new VipsArrayDouble(vector);

        GObject.SetProperty(this, "OutArray", OutArray);
        VipsArea.Unref(VipsArea.OutArray);

        return 0;
    }
}

// vips_getpoint_class_init
public class VipsGetpointClass : VipsOperationClass
{
    public static readonly string Nickname = "getpoint";
    public static readonly string Description = _("read a point from an image");

    protected override void ClassInit()
    {
        base.ClassInit();

        // Properties
        VipsArgImage("in", 1, _("Input"), _("Input image"));
        VipsArgBoxed("out_array", 2, _("Output array"), _("Array of output values"));
        VipsArgInt("x", 5, _("x"), _("Point to read"));
        VipsArgInt("y", 6, _("y"), _("Point to read"));
        VipsArgBool("unpack_complex", 7, _("unpack_complex"), _("Complex pixels should be unpacked"));
    }
}

// vips_getpoint
public static int VipsGetpoint(VipsImage In, double[] Vector, ref int N, int X, int Y)
{
    var getpoint = new VipsGetpoint();
    getpoint.In = In;
    getpoint.X = X;
    getpoint.Y = Y;

    if (getpoint.Build() != 0)
        return -1;

    Vector = VIPS_ARRAY(NULL, getpoint.OutArray.N, double);
    Array.Copy(getpoint.OutArray.Data, Vector, getpoint.OutArray.N);
    N = getpoint.OutArray.N;
    VipsArea.Unref(VipsArea.OutArray);

    return 0;
}
```