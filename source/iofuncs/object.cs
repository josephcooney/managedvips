Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public abstract class VipsObject : GLib.Object
{
    public static int _vips__argument_id = 1;

    protected VipsObject() { }

    public virtual bool Sanity()
    {
        // implementation of vips_object_sanity
        return true;
    }

    public virtual void Rewind()
    {
        // implementation of vips_object_rewind
    }

    public static int ArgumentGetId()
    {
        return _vips__argument_id++;
    }

    protected override bool OnDispose()
    {
        // implementation of vips_object_dispose
        return base.OnDispose();
    }

    protected override void Dispose(bool disposing)
    {
        // implementation of vips_object_finalize
        base.Dispose(disposing);
    }

    public virtual int Build()
    {
        // implementation of vips_object_build
        return 0;
    }

    public static VipsArgument ArgumentMap(VipsObject obj, VipsArgumentMapFn fn, object a, object b)
    {
        // implementation of vips_argument_map
        return null;
    }

    public static void ArgumentClassMap(VipsObjectClass obj_class, VipsArgumentClassMapFn fn, object a, object b)
    {
        // implementation of vips_argument_class_map
    }

    public static bool ArgumentNeedsString(VipsArgumentClass argument_class)
    {
        // implementation of vips_argument_class_needsstring
        return false;
    }

    public static void ObjectSetProperty(GLib.Object obj, string name, object value)
    {
        // implementation of vips_object_set_property
    }

    public static object ObjectGetProperty(GLib.Object obj, string name)
    {
        // implementation of vips_object_get_property
        return null;
    }

    public virtual int PostBuild()
    {
        // implementation of vips_object_postbuild
        return 0;
    }

    public virtual void Preclose()
    {
        // implementation of vips_object_preclose
    }

    public virtual void Close()
    {
        // implementation of vips_object_close
    }

    public virtual void PostClose()
    {
        // implementation of vips_object_postclose
    }

    public static int ObjectGetArgument(VipsObject obj, string name, out GParamSpec pspec, out VipsArgumentClass argument_class, out VipsArgumentInstance argument_instance)
    {
        // implementation of vips_object_get_argument
        return 0;
    }

    public static bool ArgumentIsSet(VipsObject obj, string name)
    {
        // implementation of vips_object_argument_isset
        return false;
    }

    public static int ArgumentGetFlags(VipsObject obj, string name)
    {
        // implementation of vips_object_get_argument_flags
        return 0;
    }

    public static int ArgumentGetPriority(VipsObject obj, string name)
    {
        // implementation of vips_object_get_argument_priority
        return 0;
    }

    public virtual void ObjectSummaryClass(VipsObjectClass klass, VipsBuf buf)
    {
        // implementation of vips_object_summary_class
    }

    public virtual void ObjectSummary(VipsObject obj, VipsBuf buf)
    {
        // implementation of vips_object_summary
    }

    public virtual void ObjectDump(VipsObject obj, VipsBuf buf)
    {
        // implementation of vips_object_dump
    }

    public static void ObjectPrintSummaryClass(VipsObjectClass klass)
    {
        // implementation of vips_object_print_summary_class
    }

    public static void ObjectPrintSummary(VipsObject obj)
    {
        // implementation of vips_object_print_summary
    }

    public static void ObjectPrintDump(VipsObject obj)
    {
        // implementation of vips_object_print_dump
    }

    public static void ObjectPrintName(VipsObject obj)
    {
        // implementation of vips_object_print_name
    }

    public static bool ObjectSanity(VipsObject obj)
    {
        // implementation of vips_object_sanity
        return true;
    }

    public static VipsArgument ArgumentGetInstance(VipsArgumentClass argument_class, VipsObject obj)
    {
        // implementation of vips__argument_get_instance
        return null;
    }

    public static void ObjectSetStatic(VipsObject obj, bool static_object)
    {
        // implementation of vips_object_set_static
    }

    public static int NStatic()
    {
        // implementation of vips_object_n_static
        return 0;
    }

    public static void ObjectMapSub(VipsObject key, VipsObject value, object a, object b)
    {
        // implementation of vips_object_map_sub
    }

    public static void ObjectMap(VipsSListMap2Fn fn, object a, object b)
    {
        // implementation of vips_object_map
    }

    public static void TypeMap(GType base, VipsTypeMap2Fn fn, object a, object b)
    {
        // implementation of vips_type_map
    }

    public static void TypeMapAll(GType base, VipsTypeMapFn fn, object a)
    {
        // implementation of vips_type_map_all
    }

    public static void ClassMapAll(GType type, VipsClassMapFn fn, object a)
    {
        // implementation of vips_class_map_all
    }

    public static int TypeDepth(GType type)
    {
        // implementation of vips_type_depth
        return 0;
    }

    public static GType TypeFind(string basename, string nickname)
    {
        // implementation of vips_type_find
        return 0;
    }

    public static string NicknameFind(GType type)
    {
        // implementation of vips_nickname_find
        return null;
    }

    public static void ObjectLocalCb(VipsObject obj, GLib.Object gobject)
    {
        // implementation of vips_object_local_cb
    }

    public static VipsObject[] ObjectLocalArray(VipsObject parent, int n)
    {
        // implementation of vips_object_local_array
        return new VipsObject[0];
    }

    public static void ObjectPrintAll()
    {
        // implementation of vips_object_print_all
    }

    public static void ObjectSanityAll()
    {
        // implementation of vips_object_sanity_all
    }

    public static void UnrefOutputs(VipsObject obj)
    {
        // implementation of vips_object_unref_outputs
    }

    public static string GetDescription(VipsObject obj)
    {
        // implementation of vips_object_get_description
        return null;
    }
}

public abstract class VipsArgumentClass : GLib.Object
{
    public GParamSpec Pspec { get; set; }
    public VipsObjectClass ObjectClass { get; set; }
    public int Flags { get; set; }
    public int Priority { get; set; }
    public int Offset { get; set; }

    protected VipsArgumentClass() { }
}

public abstract class VipsArgumentInstance : GLib.Object
{
    public GParamSpec Pspec { get; set; }
    public VipsArgumentClass ArgumentClass { get; set; }
    public VipsObject Object { get; set; }
    public bool Assigned { get; set; }
    public int CloseId { get; set; }
    public int InvalidateId { get; set; }

    protected VipsArgumentInstance() { }
}

public abstract class VipsObjectClass : GLib.Object
{
    public GType ArgumentTableTraverseGtype { get; set; }
    public GSList ArgumentTableTraverse { get; set; }
    public Dictionary<GParamSpec, VipsArgument> ArgumentTable { get; set; }

    protected VipsObjectClass() { }

    public virtual bool Build(VipsObject obj)
    {
        // implementation of vips_object_real_build
        return false;
    }

    public virtual int PostBuild(VipsObject obj, object data)
    {
        // implementation of vips_object_real_postbuild
        return 0;
    }

    public virtual void SummaryClass(VipsBuf buf)
    {
        // implementation of vips_object_real_summary_class
    }

    public virtual void Summary(VipsObject obj, VipsBuf buf)
    {
        // implementation of vips_object_real_summary
    }

    public virtual void Dump(VipsObject obj, VipsBuf buf)
    {
        // implementation of vips_object_real_dump
    }

    public virtual void Sanity(VipsObject obj, VipsBuf buf)
    {
        // implementation of vips_object_real_sanity
    }

    public virtual void Rewind(VipsObject obj)
    {
        // implementation of vips_object_real_rewind
    }

    public virtual VipsObject NewFromString(string str)
    {
        // implementation of vips_object_real_new_from_string
        return null;
    }

    public virtual string ToString(VipsObject obj, VipsBuf buf)
    {
        // implementation of vips_object_real_to_string
        return "";
    }
}

public class VipsArgument : GLib.Object
{
    public GParamSpec Pspec { get; set; }
    public int Flags { get; set; }

    protected VipsArgument() { }
}
```

Note that this is not a complete implementation, but rather a translation of the provided C code to C#. Some methods and properties have been left unimplemented as they were not specified in the original code. Additionally, some types and classes (e.g., `VipsBuf`, `VipsToken`) are not defined in the standard .NET framework and would need to be implemented or replaced with equivalent types.

Also note that this implementation uses the GLib library for C# which is available on NuGet as `GLib` package.