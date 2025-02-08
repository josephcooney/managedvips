Here is the converted C# code:

```csharp
// vips_foreign_save_raw_dispose
public class VipsForeignSaveRaw : VipsForeignSave
{
    public override void Dispose()
    {
        base.Dispose();
        if (Target != null)
            Target.Dispose();
    }
}

// vips_foreign_save_raw_block
private int VipsForeignSaveRawBlock(VipsRegion region, VipsRect area, object a)
{
    var raw = (VipsForeignSaveRaw)a;
    for (int y = 0; y < area.Height; y++)
        if (!Target.Write(VIPS_REGION_ADDR(region, area.Left, area.Top + y), VIPS_IMAGE_SIZEOF_PEL(region.Im) * area.Width))
            return -1;

    return 0;
}

// vips_foreign_save_raw_build
private int VipsForeignSaveRawBuild()
{
    if (base.Build())
        return -1;

    if (ImagePioInput() || SinkDisc(VIPS_REGION_ADDR(Im), VipsForeignSaveRawBlock, this))
        return -1;

    if (!Target.End())
        return -1;

    return 0;
}

// vips_foreign_save_raw_class_init
public class VipsForeignSaveRawClass : VipsObjectClass
{
    public override void Dispose()
    {
        base.Dispose();
        if (Target != null)
            Target.Dispose();
    }

    public override int Build()
    {
        if (base.Build())
            return -1;

        if (ImagePioInput() || SinkDisc(VIPS_REGION_ADDR(Im), VipsForeignSaveRawBlock, this))
            return -1;

        if (!Target.End())
            return -1;

        return 0;
    }
}

// vips_foreign_save_raw_file_build
public class VipsForeignSaveRawFile : VipsForeignSaveRaw
{
    public override int Build()
    {
        if (base.Build())
            return -1;

        if (Filename != null && !(Target = new VipsTarget(Filename)))
            return -1;

        return base.Build();
    }
}

// vips_foreign_save_raw_target_build
public class VipsForeignSaveRawTarget : VipsForeignSaveRaw
{
    public override int Build()
    {
        if (base.Build())
            return -1;

        if (Target != null)
        {
            this.Target = Target;
            GCHandle.Alloc(Target);
        }

        return base.Build();
    }
}

// vips_foreign_save_raw_buffer_build
public class VipsForeignSaveRawBuffer : VipsForeignSaveRaw
{
    public override int Build()
    {
        if (base.Build())
            return -1;

        var blob = new VipsBlob();
        Target = new VipsTarget(blob);
        GCHandle.Alloc(Target);

        return base.Build();
    }
}

// vips_rawsave
public static int RawSave(VipsImage inImage, string filename, params object[] args)
{
    return VipsCallSplit("rawsave", args, inImage, filename);
}

// vips_rawsave_buffer
public static int RawSaveBuffer(VipsImage inImage, out byte[] buf, out size_t len, params object[] args)
{
    var area = new VipsArea();
    var result = VipsCallSplit("rawsave_buffer", args, inImage, ref area);

    if (!result && area != null)
    {
        if (buf != null)
            buf = area.Data;
        if (len != 0)
            len = area.Length;

        VipsArea.Unref(area);
    }

    return result;
}

// vips_rawsave_target
public static int RawSaveTarget(VipsImage inImage, VipsTarget target, params object[] args)
{
    return VipsCallSplit("rawsave_target", args, inImage, target);
}
```