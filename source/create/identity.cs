Here is the C# code equivalent to the provided C code:

```csharp
// identity LUTs
//
// Copyright 1991, N. Dessipris.
//
// Author N. Dessipris
// Written on 11/03/1991
// Updated on:
// 18/6/93 JC
//	- im_outcheck() call added
//	- ANSIfied
// 24/8/94 JC
//	- im_identity_ushort() added
// 24/3/10
// 	- gtkdoc
// 3/7/13
// 	- redo as a class

using System;

public class VipsIdentity : VipsCreate
{
    public int Bands { get; set; }
    public bool UShort { get; set; }
    public int Size { get; set; }

    // vips_identity_gen
    private static void IdentityGen(VipsRegion outRegion, object seq, object a, object b, ref bool stop)
    {
        VipsIdentity identity = (VipsIdentity)a;
        VipsRect r = outRegion.Valid;
        int le = r.Left;
        int ri = VIPS_RECT_RIGHT(r);
        int x, i;

        if (identity.UShort)
        {
            // IDENTITY(unsigned short);
            for (x = le; x < ri; x++)
            {
                for (i = 0; i < identity.Bands; i++)
                    outRegion.Data[i] = (ushort)x;
                outRegion.Data += identity.Bands;
            }
        }
        else
        {
            // IDENTITY(unsigned char);
            for (x = le; x < ri; x++)
            {
                for (i = 0; i < identity.Bands; i++)
                    outRegion.Data[i] = (byte)x;
                outRegion.Data += identity.Bands;
            }
        }
    }

    // vips_identity_build
    protected override int Build()
    {
        VipsCreate create = VIPS_CREATE(this);
        VipsIdentity identity = this;

        if (base.Build())
            return -1;

        VipsImage image = create.Out;
        image.InitFields(identity.UShort ? identity.Size : 256, 1, identity.Bands,
            identity.UShort ? VipsFormat.Ushort : VipsFormat.Uchar,
            VipsCoding.None, VipsInterpretation.Histogram,
            1.0, 1.0);

        if (image.Pipelinev(VipsDemandStyle.Any, null) ||
            image.Generate(null, IdentityGen, null, identity, null))
            return -1;

        return 0;
    }

    // vips_identity_class_init
    protected override void ClassInit()
    {
        base.ClassInit();

        VIPS_ARG_INT("bands", 3,
            _("Bands"),
            _("Number of bands in LUT"),
            VIPSArgument.OptionalInput,
            typeof(VipsIdentity).GetField("bands").Offset,
            1, 100000, 1);

        VIPS_ARG_BOOL("ushort", 4,
            _("Ushort"),
            _("Create a 16-bit LUT"),
            VIPSArgument.OptionalInput,
            typeof(VipsIdentity).GetField("ushort").Offset,
            false);

        VIPS_ARG_INT("size", 5,
            _("Size"),
            _("Size of 16-bit LUT"),
            VIPSArgument.OptionalInput,
            typeof(VipsIdentity).GetField("size").Offset,
            1, 65536, 65536);
    }

    // vips_identity_init
    protected override void Init()
    {
        base.Init();
        Bands = 1;
        UShort = false;
        Size = 65536;
    }
}

public class VipsIdentityClass : VipsCreateClass
{
    public static VipsIdentityClass Type { get; private set; }

    static VipsIdentityClass()
    {
        Type = (VipsIdentityClass)GType.Register(typeof(VipsIdentity));
        GType.AddClassField(Type, "nickname", "identity");
        GType.AddClassField(Type, "description",
            _("make a 1D image where pixel values are indexes"));
        GType.AddClassField(Type, "build", new Delegate(Build));
    }
}

public class VipsIdentityFactory : VipsCreateFactory
{
    public static VipsIdentityFactory Factory { get; private set; }

    static VipsIdentityFactory()
    {
        Factory = (VipsIdentityFactory)GType.Register(typeof(VipsIdentityFactory));
        GType.AddClassField(Factory, "create", new Delegate(Create));
    }
}

public class VipsIdentity : VipsCreate
{
    public static int VipsIdentity(VipsImage[] outImages, params object[] args)
    {
        return VipsCallSplit("identity", outImages, args);
    }
}
```