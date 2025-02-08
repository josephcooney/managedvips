Here is the C# code equivalent to the provided C code:

```csharp
// Define built-in VIPS types.
//
// J. Cupitt, 8/4/93.
//
// Modified:
// 21/5/07
//	- any length vector (Tom)
// 23/8/10
// 	- add IM_TYPE_RW flag for im__rw_image

using System;
using System.Collections.Generic;

public class VipsTypes
{
    // String containing each of the characters which can be used within a
    // single command line argument to separate the elements of a vector.
    public const string VecSeps = " ";

    // Input display type.
    public static readonly VipsTypeDesc ImInputDisplay = new VipsTypeDesc(
        VipsType.Display,
        0,
        VipsType.Arg,
        null,
        null);

    // Output display type.
    public static readonly VipsTypeDesc ImOutputDisplay = new VipsTypeDesc(
        VipsType.Display,
        sizeof(void*),
        VipsType.Output,
        null,
        null);

    // Init function for input images.
    private static int InputImageInit(object obj, string str)
    {
        IMAGE[] im = (IMAGE[])obj;

        return !im[0] = VipsDeprecatedOpenRead(str, false);
    }

    // Input image type.
    public static readonly VipsTypeDesc ImInputImage = new VipsTypeDesc(
        VipsType.Image,
        0,
        VipsType.Arg,
        (initObjFn)InputImageInit,
        (destObjFn)VipsClose);

    // Init function for output images.
    private static int OutputImageInit(object obj, string str)
    {
        IMAGE[] im = (IMAGE[])obj;

        return !im[0] = VipsOpenWrite(str);
    }

    // Output image type.
    public static readonly VipsTypeDesc ImOutputImage = new VipsTypeDesc(
        VipsType.Image,
        0,
        VipsType.Output | VipsType.Arg,
        (initObjFn)OutputImageInit,
        (destObjFn)VipsClose);

    // Init function for RW images.
    private static int RwImageInit(object obj, string str)
    {
        IMAGE[] im = (IMAGE[])obj;

        return !im[0] = ImOpen(str, "rw");
    }

    // RW image type.
    public static readonly VipsTypeDesc ImRwImage = new VipsTypeDesc(
        VipsType.Image,
        0,
        VipsType.Arg | VipsType.Rw,
        (initObjFn)RwImageInit,
        (destObjFn)VipsClose);

    // Init function for imagevec input.
    private static int InputImageVecInit(object obj, string str)
    {
        ImImageVecObject iv = (ImImageVecObject)obj;
        char[] strv = str.Split(VipsTypes.VecSeps.ToCharArray());
        int nargs = strv.Length;

        if (!(iv.vec = new IMAGE[nargs]))
            return -1;
        iv.n = nargs;

        for (int i = 0; i < nargs; i++)
            iv.vec[i] = null;

        for (int i = 0; i < nargs; i++)
            if (!(iv.vec[i] = ImOpen(strv[i], "rd")))
                return -1;

        return 0;
    }

    // Input image vector type.
    public static readonly VipsTypeDesc ImInputImageVec = new VipsTypeDesc(
        VipsType.ImageVec,
        sizeof(ImImageVecObject),
        VipsType.Arg,
        (initObjFn)InputImageVecInit,
        (destObjFn)ImageVecDest);

    // Init function for masks.
    private static int MaskInit(object obj, string str)
    {
        ImMaskObject mo = (ImMaskObject)obj;

        if (!(mo.name = VipsStrdup(null, str)))
            return -1;
        mo.mask = null;

        return 0;
    }

    // Init function for input dmasks.
    private static int DmaskInit(object obj, string str)
    {
        ImMaskObject mo = (ImMaskObject)obj;

        if (MaskInit(obj, str))
            return -1;
        if (!(mo.mask = ImReadDmask(str)))
            return -1;

        return 0;
    }

    // Init function for input imasks.
    private static int ImaskInit(object obj, string str)
    {
        ImMaskObject mo = (ImMaskObject)obj;

        if (MaskInit(obj, str))
            return -1;
        if (!(mo.mask = ImReadImask(str)))
            return -1;

        return 0;
    }

    // Destroy function for DOUBLEMASK.
    private static int DmaskDest(object obj)
    {
        ImMaskObject mo = (ImMaskObject)obj;

        VipsFree(mo.name);
        VipsFreeF(ImFreeDmask, mo.mask);

        return 0;
    }

    // Destroy function for INTMASK.
    private static int ImaskDest(object obj)
    {
        ImMaskObject mo = (ImMaskObject)obj;

        VipsFree(mo.name);
        VipsFreeF(ImFreeImask, mo.mask);

        return 0;
    }

    // Output dmask type.
    public static readonly VipsTypeDesc ImOutputDmask = new VipsTypeDesc(
        VipsType.Dmask,
        sizeof(ImMaskObject),
        VipsType.Output | VipsType.Arg,
        (initObjFn)MaskInit,
        (destObjFn)SaveDmaskDest);

    // Input dmask type.
    public static readonly VipsTypeDesc ImInputDmask = new VipsTypeDesc(
        VipsType.Dmask,
        sizeof(ImMaskObject),
        VipsType.Arg,
        (initObjFn)DmaskInit,
        (destObjFn)DmaskDest);

    // Output imask type.
    public static readonly VipsTypeDesc ImOutputImask = new VipsTypeDesc(
        VipsType.Imask,
        sizeof(ImMaskObject),
        VipsType.Output | VipsType.Arg,
        (initObjFn)MaskInit,
        (destObjFn)SaveImaskDest);

    // Input imask type.
    public static readonly VipsTypeDesc ImInputImask = new VipsTypeDesc(
        VipsType.Imask,
        sizeof(ImMaskObject),
        VipsType.Arg,
        (initObjFn)ImaskInit,
        (destObjFn)ImaskDest);

    // Output dmask to screen type.
    public static readonly VipsTypeDesc ImOutputDmaskScreen = new VipsTypeDesc(
        VipsType.Dmask,
        sizeof(ImMaskObject),
        VipsType.Output,
        (initObjFn)MaskInit,
        (destObjFn)DmaskDest);

    // Init function for double input.
    private static int InputDoubleInit(object obj, string str)
    {
        double[] d = (double[])obj;

        return !d[0] = GAsciiStrtod(str, null);
    }

    // Input double type.
    public static readonly VipsTypeDesc ImInputDouble = new VipsTypeDesc(
        VipsType.Double,
        sizeof(double),
        VipsType.Arg,
        (initObjFn)InputDoubleInit,
        null);

    // Destroy function for im_doublevec_object.
    private static int DoubleVecDest(object obj)
    {
        ImDoubleVecObject dv = (ImDoubleVecObject)obj;

        if (dv.vec != null)
            GFree(dv.vec);
        dv.vec = null;
        dv.n = 0;

        return 0;
    }

    // Init function for doublevec input.
    private static int InputDoubleVecInit(object obj, string str)
    {
        ImDoubleVecObject dv = (ImDoubleVecObject)obj;
        char[] strv = str.Split(VipsTypes.VecSeps.ToCharArray());
        int nargs = strv.Length;

        if (!(dv.vec = new double[nargs]))
            return -1;
        dv.n = nargs;

        for (int i = 0; i < nargs; i++)
            dv.vec[i] = GAsciiStrtod(strv[i], null);

        return 0;
    }

    // Input double vector type.
    public static readonly VipsTypeDesc ImInputDoubleVec = new VipsTypeDesc(
        VipsType.DoubleVec,
        sizeof(ImDoubleVecObject),
        VipsType.Arg,
        (initObjFn)InputDoubleVecInit,
        (destObjFn)DoubleVecDest);

    // Print function for doublevec output.
    public static int ImDvprint(object obj)
    {
        ImDoubleVecObject dv = (ImDoubleVecObject)obj;
        int i;

        for (i = 0; i < dv.n; i++)
            printf("%G ", dv.vec[i]);
        printf("\n");

        return 0;
    }

    // Output double vector type.
    public static readonly VipsTypeDesc ImOutputDoubleVec = new VipsTypeDesc(
        VipsType.DoubleVec,
        sizeof(ImDoubleVecObject),
        VipsType.Output,
        null,
        (destObjFn)DoubleVecDest);

    // Destroy function for im_intvec_object.
    private static int IntVecDest(object obj)
    {
        ImIntVecObject iv = (ImIntVecObject)obj;

        if (iv.vec != null)
            GFree(iv.vec);
        iv.vec = null;
        iv.n = 0;

        return 0;
    }

    // Init function for intvec input.
    private static int InputIntVecInit(object obj, string str)
    {
        ImIntVecObject iv = (ImIntVecObject)obj;
        char[] strv = str.Split(VipsTypes.VecSeps.ToCharArray());
        int nargs = strv.Length;

        if (!(iv.vec = new int[nargs]))
            return -1;
        iv.n = nargs;

        for (int i = 0; i < nargs; i++)
        {
            long val = Strtol(strv[i], null, 10);

            if (errno)
                VipsErrorSystem(errno, "input_intvec_init", _("bad integer \"{0}\""), strv[i]);
            if (val > int.MaxValue || val < int.MinValue)
                VipsError("input_intvec_init", "{0} overflows integer type", val);
            iv.vec[i] = (int)val;
        }

        return 0;
    }

    // Input int vector type.
    public static readonly VipsTypeDesc ImInputIntVec = new VipsTypeDesc(
        VipsType.IntVec,
        sizeof(ImIntVecObject),
        VipsType.Arg,
        (initObjFn)InputIntVecInit,
        (destObjFn)IntVecDest);

    // Print function for intvec output.
    public static int ImIprint(object obj)
    {
        ImIntVecObject iv = (ImIntVecObject)obj;
        int i;

        for (i = 0; i < iv.n; i++)
            printf("%d ", iv.vec[i]);
        printf("\n");

        return 0;
    }

    // Output int vector type.
    public static readonly VipsTypeDesc ImOutputIntVec = new VipsTypeDesc(
        VipsType.IntVec,
        sizeof(ImIntVecObject),
        VipsType.Output,
        null,
        (destObjFn)IntVecDest);

    // Init function for int input.
    private static int InputIntInit(object obj, string str)
    {
        int[] i = (int[])obj;

        return Sscanf(str, "%d", ref i[0]) != 1
            ? VipsError("input_int", "{0}", _("bad format"))
            : 0;
    }

    // Input int type.
    public static readonly VipsTypeDesc ImInputInt = new VipsTypeDesc(
        VipsType.Int,
        sizeof(int),
        VipsType.Arg,
        (initObjFn)InputIntInit,
        null);

    // Init function for string input.
    private static int InputStringInit(object obj, string str)
    {
        if (!obj = VipsStrdup(null, str))
            return -1;

        return 0;
    }

    // Input string type.
    public static readonly VipsTypeDesc ImInputString = new VipsTypeDesc(
        VipsType.String,
        0,
        VipsType.Arg,
        (initObjFn)InputStringInit,
        (destObjFn)VipsFree);

    // Output string type.
    public static readonly VipsTypeDesc ImOutputString = new VipsTypeDesc(
        VipsType.String,
        0,
        VipsType.Output,
        null,
        (destObjFn)VipsFree);

    // Output double type.
    public static readonly VipsTypeDesc ImOutputDouble = new VipsTypeDesc(
        VipsType.Double,
        sizeof(double),
        VipsType.Output,
        null,
        null);

    // Output complex type.
    public static readonly VipsTypeDesc ImOutputComplex = new VipsTypeDesc(
        VipsType.Complex,
        2 * sizeof(double),
        VipsType.Output,
        null,
        null);

    // Output int type.
    public static readonly VipsTypeDesc ImOutputInt = new VipsTypeDesc(
        VipsType.Int,
        sizeof(int),
        VipsType.Output,
        null,
        null);

    // Print function for int output.
    public static int ImIprint(object obj)
    {
        int[] i = (int[])obj;

        printf("%d\n", i[0]);

        return 0;
    }

    // Print function for string output.
    public static int ImSprint(object obj)
    {
        char[] s = (char[])obj;

        printf("%s\n", s);

        return 0;
    }

    // Print function for double output.
    public static int ImDprint(object obj)
    {
        double[] d = (double[])obj;

        printf("%G\n", d[0]);

        return 0;
    }

    // Print function for complex output.
    public static int ImCprint(object obj)
    {
        double[] d = (double[])obj;

        printf("%G %G\n", d[0], d[1]);

        return 0;
    }

    // Statistics to stdout.
    public static int ImDmsprint(object obj)
    {
        DOUBLEMASK mask = ((ImMaskObject)obj).mask;
        double[] row;
        int i, j;

        printf("band    minimum     maximum         sum       "
               "sum^2        mean   deviation\n");
        for (j = 0; j < mask.ysize; j++)
        {
            row = new double[mask.xsize];
            Array.Copy(mask.coeff, j * mask.xsize, row, 0, mask.xsize);
            if (j == 0)
                printf("all");
            else
                printf("%2d ", j);

            for (i = 0; i < 6; i++)
                printf("%12g", row[i]);
            printf("\n");
        }

        return 0;
    }

    // Init function for input gvalue.
    private static int InputGValueInit(object obj, string str)
    {
        GValue value = (GValue)obj;

        g_value_init(value, GType.String);
        g_value_set_string(value, str);

        return 0;
    }

    // Destroy function for gvalue.
    private static int GValueFree(object obj)
    {
        GValue value = (GValue)obj;

        if (g_is_value(value))
            g_value_unset(value);

        return 0;
    }

    // Input GValue type.
    public static readonly VipsTypeDesc ImInputGValue = new VipsTypeDesc(
        VipsType.GValue,
        sizeof(GValue),
        VipsType.Arg,
        (initObjFn)InputGValueInit,
        (destObjFn)GValueFree);

    // Print function for gvalue output.
    public static int ImGprint(object obj)
    {
        GValue value = (GValue)obj;
        char[] str_value;

        str_value = GStrdupValueContents(value);
        printf("%s\n", str_value);
        GFree(str_value);

        return 0;
    }

    // Init function for output gvalue.
    private static int OutputGValueInit(object obj)
    {
        GValue value = (GValue)obj;

        memset(value, 0, sizeof(GValue));

        return 0;
    }

    public static readonly VipsTypeDesc ImOutputGValue = new VipsTypeDesc(
        VipsType.GValue,
        sizeof(GValue),
        VipsType.Output,
        (initObjFn)OutputGValueInit,
        (destObjFn)GValueFree);

    // Init function for input interpolate.
    public static int InputInterpolateInit(object obj, string str)
    {
        GType type = g_type_from_name("VipsInterpolate");
        VipsObjectClass class_ = VIPS_OBJECT_CLASS(g_type_class_ref(type));
        VipsObject object;

        if (!(object = vips_object_new_from_string(class_, str)))
            return -1;
        if (vips_object_build(object))
        {
            g_object_unref(object);
            return -1;
        }
        obj = object;

        return 0;
    }

    // Destroy function for input interpolate.
    private static int InputInterpolateDest(object obj)
    {
        g_object_unref((GObject)obj);

        return 0;
    }

    public static readonly VipsTypeDesc ImInputInterpolate = new VipsTypeDesc(
        VipsType.Interpolate,
        0,
        VipsType.Arg,
        (initObjFn)InputInterpolateInit,
        (destObjFn)InputInterpolateDest);
}

public class VipsType
{
    public const int Display = 1;
    public const int Image = 2;
    public const int ImageVec = 3;
    public const int Dmask = 4;
    public const int Imask = 5;
    public const int Double = 6;
    public const int DoubleVec = 7;
    public const int Int = 8;
    public const int IntVec = 9;
    public const int String = 10;
    public const int Output = 16;
    public const int Arg = 32;
    public const int Rw = 64;
}

public class VipsTypeDesc
{
    public int Type { get; set; }
    public int Storage { get; set; }
    public int Flags { get; set; }
    public initObjFn Init { get; set; }
    public destObjFn Dest { get; set; }

    public VipsTypeDesc(int type, int storage, int flags, initObjFn init, destObjFn dest)
    {
        Type = type;
        Storage = storage;
        Flags = flags;
        Init = init;
        Dest = dest;
    }
}

public delegate int initObjFn(object obj, string str);
public delegate int destObjFn(object obj);

// Other VIPS methods in other files have been converted separately.
```

Note that the `VipsTypes` class contains all the type definitions and functions from the original C code. The `VipsType` enum defines the different types of objects that can be used with VIPS, such as images, masks, doubles, etc.

The `VipsTypeDesc` class represents a single type definition, which includes the type, storage requirements, flags, and initialization and destruction functions for the object.

The other methods in the `VipsTypes` class are similar to their C counterparts, but have been converted to use C# syntax and semantics.