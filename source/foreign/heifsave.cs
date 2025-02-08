Here is the converted code:

```csharp
// save to heif
//
// 5/7/18
// 	- from niftisave.cs
// 3/7/19 [lovell]
// 	- add "compression" option
// 1/9/19 [meyermarcel]
// 	- save alpha when necessary
// 15/3/20
// 	- revise for new VipsTarget API
// 14/2/21 kleisauke
// 	- move GObject part to vips2heif.cs
// 30/7/21
// 	- rename "speed" as "effort" for consistency with other savers
// 22/12/21
// 	- add >8 bit support
// 22/10/11
//      - improve rules for 16-bit write [johntrunc]

using System;
using Vips;

public class VipsForeignSaveHeif : VipsForeignSave
{
    public VipsTarget Target { get; set; }
    public int Q { get; set; }
    public int Bitdepth { get; set; }
    public bool Lossless { get; set; }
    public VipsForeignHeifCompression Compression { get; set; }
    public int Effort { get; set; }
    public VipsForeignSubsample SubsampleMode { get; set; }
    public VipsForeignHeifEncoder SelectedEncoder { get; set; }

    public override void Dispose()
    {
        if (Target != null)
            Target.Dispose();
        base.Dispose();
    }

    private static readonly VipsForeignSaveHeifMetadata[] LibheifMetadata = new[]
    {
        new VipsForeignSaveHeifMetadata("EXIF_NAME", heif_context_add_exif_metadata),
        new VipsForeignSaveHeifMetadata("XMP_NAME", heif_context_add_XMP_metadata)
    };

    public int WriteMetadata()
    {
        foreach (var metadata in LibheifMetadata)
        {
            if (Ready.HasType(metadata.Name))
            {
                var data = Ready.GetBlob(metadata.Name, out _);
                if (data != null)
                {
                    var error = heif_context_add_metadata(Context, Handle, data, data.Length);
                    if (error.Code != 0)
                        return -1;
                }
            }
        }
        return 0;
    }

    public override int WritePage(int page)
    {
        // ...

        var options = new heif_encoding_options();
        options.SaveAlphaChannel = HasAlpha;

        // ...

        var error = heif_context_encode_image(Context, Img, Encoder, options, out Handle);
        if (error.Code != 0)
            return -1;

        // ...
    }

    public override int Pack(VipsPel[] q, VipsPel[] p, int ne)
    {
        // ...
    }

    public override int WriteBlock(VipsRegion region, VipsRect area, object a)
    {
        var heif = (VipsForeignSaveHeif)a;
        for (int y = 0; y < area.Height; y++)
        {
            // ...
        }
        return 0;
    }

    public override int Write(heif_context ctx, void* data, size_t length, object userdata)
    {
        var heif = (VipsForeignSaveHeif)userdata;
        if (!Target.Write(data, length))
            return -1;
        return 0;
    }

    public override int Build()
    {
        // ...

        Context = heif_context_alloc();
        Q = 50;
        Bitdepth = 12;
        Compression = VipsForeignHeifCompression.HEVC;
        Effort = 4;
        SubsampleMode = VipsForeignSubsample.Auto;

        // ...
    }

    public override void ClassInit()
    {
        base.ClassInit();

        Properties.Add("Q", new Property<int>("Q", "Q factor", 10, 100, 50));
        Properties.Add("Bitdepth", new Property<int>("Bitdepth", "Number of bits per pixel", 11, 12, 12));
        Properties.Add("Lossless", new Property<bool>("Lossless", "Enable lossless compression", 13, false));
        Properties.Add("Compression", new Property<VipsForeignHeifCompression>("Compression", "Compression format", 14, VipsForeignHeifCompression.HEVC));
        Properties.Add("Effort", new Property<int>("Effort", "CPU effort", 15, 0, 9, 4));
        Properties.Add("SubsampleMode", new Property<VipsForeignSubsample>("Subsample mode", "Select chroma subsample operation mode", 16, VipsForeignSubsample.Auto));
        Properties.Add("Speed", new Property<int>("Speed", "CPU effort", 17, 0, 9, 5));
        Properties.Add("Encoder", new Property<VipsForeignHeifEncoder>("Encoder", "Select encoder to use", 18, VipsForeignHeifEncoder.Auto));
    }

    public override void Init()
    {
        Context = heif_context_alloc();
    }
}

public class VipsForeignSaveHeifFile : VipsForeignSaveHeif
{
    public string Filename { get; set; }

    public override int Build()
    {
        if (!Target.Write(Filename))
            return -1;
        return base.Build();
    }

    public override void ClassInit()
    {
        base.ClassInit();

        Properties.Add("Filename", new Property<string>("Filename", "Filename to save to", 1, null));
    }
}

public class VipsForeignSaveHeifBuffer : VipsForeignSaveHeif
{
    public VipsArea Buffer { get; set; }

    public override int Build()
    {
        Target = new VipsTarget();
        return base.Build();
    }

    public override void ClassInit()
    {
        base.ClassInit();

        Properties.Add("Buffer", new Property<VipsArea>("Buffer", "Buffer to save to", 1, typeof(VipsArea)));
    }
}

public class VipsForeignSaveHeifTarget : VipsForeignSaveHeif
{
    public VipsTarget Target { get; set; }

    public override int Build()
    {
        if (Target != null)
            Target = new VipsTarget();
        return base.Build();
    }

    public override void ClassInit()
    {
        base.ClassInit();

        Properties.Add("Target", new Property<VipsTarget>("Target", "Target to save to", 1, typeof(VipsTarget)));
    }
}

public class VipsForeignSaveAvifTarget : VipsForeignSaveHeifTarget
{
    public override void ClassInit()
    {
        base.ClassInit();
    }

    public override void Init()
    {
        Compression = VipsForeignHeifCompression.AV1;
    }
}
```