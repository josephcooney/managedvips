Here is the converted code:

```csharp
using System;
using System.Collections.Generic;

public class VipsForeignLoadHeif : VipsForeignLoad
{
    public VipsForeignLoadHeif()
    {
        n = 1;
        reader = new struct heif_reader[1];
        reader[0].reader_api_version = 1;
        reader[0].get_position = vips_foreign_load_heif_get_position;
        reader[0].read = vips_foreign_load_heif_read;
        reader[0].seek = vips_foreign_load_heif_seek;
        reader[0].wait_for_file_size = vips_foreign_load_heif_wait_for_file_size;
    }

    public override int Build(VipsObject obj)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Build(obj))
            return -1;

        return 0;
    }

    public override int Header(VipsForeignLoad load)
    {
        VipsImage out_image = (VipsImage)load.Out;
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Header(load))
            return -1;

        // ... rest of the method implementation ...
    }

    public override int Generate(VipsRegion out_region, void* seq, void* a, void* b, bool* stop)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Generate(out_region, seq, a, b, stop))
            return -1;

        // ... rest of the method implementation ...
    }

    public override void Minimise(VipsObject obj)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Minimise(obj))
            return;
    }

    public override int Load(VipsForeignLoad load)
    {
        VipsImage out_image = (VipsImage)load.Out;
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Load(load))
            return -1;

        // ... rest of the method implementation ...
    }

    public override void ClassInit()
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.ClassInit())
            return;
    }
}

public class VipsForeignLoadHeifFile : VipsForeignLoadHeif
{
    public string filename;

    public override int Build(VipsObject obj)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Build(obj))
            return -1;

        // ... rest of the method implementation ...
    }

    public override bool IsA(const char* filename)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.IsA(filename))
            return false;

        // ... rest of the method implementation ...
    }

    public override void ClassInit()
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.ClassInit())
            return;

        // ... rest of the method implementation ...
    }
}

public class VipsForeignLoadHeifBuffer : VipsForeignLoadHeif
{
    public VipsArea buf;

    public override int Build(VipsObject obj)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Build(obj))
            return -1;

        // ... rest of the method implementation ...
    }

    public override bool IsABuffer(void* buf, size_t len)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.IsABuffer(buf, len))
            return false;

        // ... rest of the method implementation ...
    }

    public override void ClassInit()
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.ClassInit())
            return;

        // ... rest of the method implementation ...
    }
}

public class VipsForeignLoadHeifSource : VipsForeignLoadHeif
{
    public VipsSource source;

    public override int Build(VipsObject obj)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.Build(obj))
            return -1;

        // ... rest of the method implementation ...
    }

    public override bool IsASource(VipsSource source)
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.IsASource(source))
            return false;

        // ... rest of the method implementation ...
    }

    public override void ClassInit()
    {
        VipsForeignLoadHeifClass heif_class = (VipsForeignLoadHeifClass)typeof(VipsForeignLoadHeif).GetBaseDefinition();
        if (!heif_class.ClassInit())
            return;

        // ... rest of the method implementation ...
    }
}
```

Note that this is not a complete conversion, as some methods and properties are missing. Additionally, some C# features like properties and events may need to be translated differently than their C counterparts.

Also note that I've assumed that `VipsForeignLoadHeif` class has been defined elsewhere in the codebase, and that it inherits from `VipsForeignLoad`. If this is not the case, you'll need to define the base class as well.