```csharp
// im_rotate_imask90
public static INTMASK* RotateImask90(INTMASK* in, string filename)
{
    // Pass a mask through a vips operation, eg. im_rot90().
    return Vapplyimask(in, filename, ImRot45);
}

// im_rotate_dmask90
public static DOUBLEMASK* RotateDmask90(DOUBLEMASK* in, string filename)
{
    // Pass a mask through a vips operation, eg. im_rot90().
    return Vapplydmask(in, filename, ImRot45);
}

// vapplyimask
private static INTMASK* Vapplyimask(INTMASK* in, string name, Func<IMAGE, IMAGE, int> fn)
{
    // The type of the vips operations we support.
    var x = new IMAGE();
    if (!x.Open(name, "p"))
        return null;
    
    var t = new IMAGE[2];
    var d = new DOUBLEMASK[2];
    var out = new INTMASK();

    if (!(d[0] = ImLocalDmask(x, ImImask2Dmask(in, name))) ||
        !x.OpenLocalArray(t, 2, name, "p") ||
        !ImMask2Vips(d[0], t[0]) ||
        fn(t[0], t[1]) ||
        !(d[1] = ImLocalDmask(x, ImVips2Mask(t[1], name))) ||
        !(out = ImDmask2Imask(d[1], name)))
    {
        x.Close();
        return null;
    }
    
    out.Scale = in.Scale;
    out.Offset = in.Offset;

    x.Close();

    return out;
}

// vapplydmask
private static DOUBLEMASK* Vapplydmask(DOUBLEMASK* in, string name, Func<IMAGE, IMAGE, int> fn)
{
    // The type of the vips operations we support.
    var x = new IMAGE();
    if (!x.Open(name, "p"))
        return null;
    
    var t = new IMAGE[2];
    var out = new DOUBLEMASK();

    if (x.OpenLocalArray(t, 2, name, "p") ||
        !ImMask2Vips(in, t[0]) ||
        fn(t[0], t[1]) ||
        !(out = ImVips2Mask(t[1], name)))
    {
        x.Close();
        return null;
    }
    
    out.Scale = in.Scale;
    out.Offset = in.Offset;

    x.Close();

    return out;
}

// im_rot45
private static int ImRot45(IMAGE in, IMAGE out)
{
    // The type of the vips operations we support.
    var t = new VipsImage();
    if (VipsRot45(in, ref t, null))
        return -1;
    
    if (!VipsImageWrite(t, out))
    {
        GObject.Unref(t);
        return -1;
    }
    
    GObject.Unref(t);

    return 0;
}

// im_rotate_imask45
public static INTMASK* RotateImask45(INTMASK* in, string filename)
{
    // Pass a mask through a vips operation, eg. im_rot45().
    return Vapplyimask(in, filename, ImRot45);
}

// im_rotate_dmask45
public static DOUBLEMASK* RotateDmask45(DOUBLEMASK* in, string filename)
{
    // Pass a mask through a vips operation, eg. im_rot45().
    return Vapplydmask(in, filename, ImRot45);
}
```