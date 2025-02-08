Here is the converted C# code:

```csharp
// save to rad
//
// 2/12/11
// 	- wrap a class around the rad writer
// 23/5/16
//  	- split into file and buffer save classes

using System;
using VipsDotNet;

namespace VipsForeignSaveRad
{
    public abstract class VipsForeignSaveRad : VipsForeignSave
    {
        // Save a bit of typing.
        private const string UC = "UCHAR";
        private const string C = "CHAR";
        private const string US = "USHORT";
        private const string S = "SHORT";
        private const string UI = "UINT";
        private const string I = "INT";
        private const string F = "FLOAT";
        private const string X = "COMPLEX";
        private const string D = "DOUBLE";
        private const string DX = "DPCOMPLEX";

        public static readonly VipsBandFormat[] vips_foreign_save_rad_format_table =
        {
            // Band format:  UC C  US S  UI I  F  X  D  DX
            // Promotion:
            F, F, F, F, F, F, F, F, F, F
        };

        public VipsForeignSaveRad()
        {
        }

        protected override void ClassInit(VipsObjectClass class_)
        {
            base.ClassInit(class_);
            var object_class = (VipsObjectClass)class_;
            var foreign_class = (VipsForeignClass)class_;
            var save_class = (VipsForeignSaveClass)class_;

            object_class.Nickname = "radsave_base";
            object_class.Description = "save Radiance";

            foreign_class.Suffs = Vips__RadSuffs;

            save_class.Saveable = VIPS_SAVEABLE_RGB;
            save_class.FormatTable = vips_foreign_save_rad_format_table;
            save_class.Coding[VIPS_CODING_NONE] = false;
            save_class.Coding[VIPS_CODING_RAD] = true;
        }

        public override void Init()
        {
            base.Init();
        }
    }

    public class VipsForeignSaveRadFile : VipsForeignSaveRad
    {
        // Save a bit of typing.
        private string filename;

        public VipsForeignSaveRadFile()
        {
        }

        protected override int Build(VipsObject obj)
        {
            var save = (VipsForeignSave)obj;
            var file = (VipsForeignSaveRadFile)obj;

            var target = new VipsTarget(file.filename);

            if (base.Build(obj) != 0)
                return -1;

            if (!vips__rad_save(save.Ready, target))
                return -1;

            return 0;
        }

        public static readonly Property<string> FilenameProperty =
            new Property<string>("filename", typeof(VipsForeignSaveRadFile), "Filename",
                "Filename to save to", VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET<VipsForeignSaveRadFile>(VipsForeignSaveRadFile, filename),
                null);

        protected override void ClassInit(VipsObjectClass class_)
        {
            base.ClassInit(class_);
            var gobject_class = (GObjectClass)class_;
            var object_class = (VipsObjectClass)class_;

            gobject_class.SetProperty += vips_object_set_property;
            gobject_class.GetProperty += vips_object_get_property;

            object_class.Nickname = "radsave";
            object_class.Description = "save image to Radiance file";
            object_class.Build = Build;

            VIPS_ARG_STRING(class_, "filename", 1,
                "Filename",
                "Filename to save to",
                VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET<VipsForeignSaveRadFile>(VipsForeignSaveRadFile, filename),
                null);
        }

        public override void Init()
        {
            base.Init();
        }
    }

    public class VipsForeignSaveRadTarget : VipsForeignSaveRad
    {
        // Save a bit of typing.
        private VipsTarget target;

        public VipsForeignSaveRadTarget()
        {
        }

        protected override int Build(VipsObject obj)
        {
            var save = (VipsForeignSave)obj;
            var target_ = (VipsForeignSaveRadTarget)obj;

            if (base.Build(obj) != 0)
                return -1;

            if (!vips__rad_save(save.Ready, target_.target))
                return -1;

            return 0;
        }

        public static readonly Property<VipsTarget> TargetProperty =
            new Property<VipsTarget>("target", typeof(VipsForeignSaveRadTarget), "Target",
                "Target to save to", VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET<VipsForeignSaveRadTarget>(VipsForeignSaveRadTarget, target),
                VipsType.Target);

        protected override void ClassInit(VipsObjectClass class_)
        {
            base.ClassInit(class_);
            var gobject_class = (GObjectClass)class_;
            var object_class = (VipsObjectClass)class_;

            gobject_class.SetProperty += vips_object_set_property;
            gobject_class.GetProperty += vips_object_get_property;

            object_class.Nickname = "radsave_target";
            object_class.Description = "save image to Radiance target";
            object_class.Build = Build;

            VIPS_ARG_OBJECT(class_, "target", 1,
                "Target",
                "Target to save to",
                VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET<VipsForeignSaveRadTarget>(VipsForeignSaveRadTarget, target),
                VipsType.Target);
        }

        public override void Init()
        {
            base.Init();
        }
    }

    public class VipsForeignSaveRadBuffer : VipsForeignSaveRad
    {
        // Save a bit of typing.
        private VipsArea buf;

        public VipsForeignSaveRadBuffer()
        {
        }

        protected override int Build(VipsObject obj)
        {
            var save = (VipsForeignSave)obj;

            var target = new VipsTarget();

            if (!vips__rad_save(save.Ready, target))
                return -1;

            var blob = target.Blob;
            save.Buffer = blob;
            vips_area_unref(blob);

            return 0;
        }

        public static readonly Property<VipsArea> BufferProperty =
            new Property<VipsArea>("buffer", typeof(VipsForeignSaveRadBuffer), "Buffer",
                "Buffer to save to", VIPS_ARGUMENT_REQUIRED_OUTPUT,
                G_STRUCT_OFFSET<VipsForeignSaveRadBuffer>(VipsForeignSaveRadBuffer, buf),
                VipsType.Blob);

        protected override void ClassInit(VipsObjectClass class_)
        {
            base.ClassInit(class_);
            var gobject_class = (GObjectClass)class_;
            var object_class = (VipsObjectClass)class_;

            gobject_class.SetProperty += vips_object_set_property;
            gobject_class.GetProperty += vips_object_get_property;

            object_class.Nickname = "radsave_buffer";
            object_class.Description = "save image to Radiance buffer";
            object_class.Build = Build;

            VIPS_ARG_BOXED(class_, "buffer", 1,
                "Buffer",
                "Buffer to save to",
                VIPS_ARGUMENT_REQUIRED_OUTPUT,
                G_STRUCT_OFFSET<VipsForeignSaveRadBuffer>(VipsForeignSaveRadBuffer, buf),
                VipsType.Blob);
        }

        public override void Init()
        {
            base.Init();
        }
    }
}

public class Program
{
    [DllImport("vipsdotnet")]
    private static extern int vips_radsave(VipsImage in_, string filename);

    [DllImport("vipsdotnet")]
    private static extern int vips_radsave_buffer(VipsImage in_, out IntPtr buf, out size_t len);

    [DllImport("vipsdotnet")]
    private static extern int vips_radsave_target(VipsImage in_, VipsTarget target);
}
```

Note that I've used the `VipsDotNet` namespace to access the VIPS API. You'll need to install the `VipsDotNet` NuGet package and import it in your project for this code to work.

Also, I've assumed that you have already converted other VIPS methods in separate files, as mentioned in the problem statement. If not, you'll need to convert them separately before using this code.