```csharp
// vips_magickload (from vips_magickload.c)

public static int MagickLoad(string filename, ref VipsImage image, params object[] args)
{
    var ap = new object[args.Length];
    Array.Copy(args, 0, ap, 0, args.Length);
    return VipsCallSplit("magickload", ap, filename, ref image);
}

// vips_magickload_buffer (from vips_magickload.c)

public static int MagickLoadBuffer(byte[] buf, size_t len, ref VipsImage image, params object[] args)
{
    var blob = new VipsBlob(buf, len);
    var ap = new object[args.Length];
    Array.Copy(args, 0, ap, 0, args.Length);
    var result = VipsCallSplit("magickload_buffer", ap, blob, ref image);
    blob.Dispose();
    return result;
}
```