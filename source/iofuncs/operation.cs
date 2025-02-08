Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public abstract class VipsOperation : VipsObject
{
    public enum OperationFlags
    {
        None = 0,
        Sequential = 1 << 0,
        NoCache = 1 << 1,
        Deprecated = 1 << 2,
        Untrusted = 1 << 3,
        Blocked = 1 << 4,
        Revalidate = 1 << 5
    }

    public static readonly Signal InvalidateSignal;

    protected VipsOperation()
    {
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    protected override void Finalize()
    {
        base.Finalize();
    }

    public virtual int Build()
    {
        if ((Flags & OperationFlags.Blocked) != 0)
        {
            throw new InvalidOperationException("Operation is blocked");
        }

        return VipsObject.Build(this);
    }

    public static VipsOperation New(string name)
    {
        Type type = VipsType.Find("VipsOperation", name);

        if (type == null)
        {
            throw new ArgumentException($"Class \"{name}\" not found");
        }

        VipsObject obj = Activator.CreateInstance(type, null) as VipsObject;

        return obj as VipsOperation;
    }

    public static int CallRequiredOptional(VipsOperation operation, params object[] required, params string[] optional)
    {
        if (required.Length == 0 && optional.Length == 0)
        {
            throw new ArgumentException("No arguments provided");
        }

        var requiredArgs = new List<object>();
        var optionalArgs = new Dictionary<string, object>();

        for (int i = 0; i < required.Length; i++)
        {
            requiredArgs.Add(required[i]);
        }

        for (int i = 0; i < optional.Length; i += 2)
        {
            if (optional[i + 1] == null)
            {
                throw new ArgumentException($"Missing value for option \"{optional[i]}\"");
            }
            optionalArgs.Add(optional[i], optional[i + 1]);
        }

        return VipsOperation.CallRequiredOptional(operation, requiredArgs.ToArray(), optionalArgs);
    }

    public static int CallByName(string operationName, params object[] required, params string[] optional)
    {
        VipsOperation operation = New(operationName);

        if (operation == null)
        {
            throw new ArgumentException($"Class \"{operationName}\" not found");
        }

        return VipsOperation.CallRequiredOptional(operation, required, optional);
    }

    public static int Call(string operationName, params object[] args)
    {
        var requiredArgs = new List<object>();
        var optionalArgs = new Dictionary<string, object>();

        for (int i = 0; i < args.Length; i++)
        {
            if ((i & 1) == 0)
            {
                requiredArgs.Add(args[i]);
            }
            else
            {
                optionalArgs.Add((string)args[i - 1], args[i]);
            }
        }

        return VipsOperation.CallByName(operationName, requiredArgs.ToArray(), optionalArgs.Keys.ToArray());
    }

    public static int CallSplit(string operationName, params string[] optional)
    {
        var requiredArgs = new List<object>();
        var optionalArgs = new Dictionary<string, object>();

        for (int i = 0; i < optional.Length; i += 2)
        {
            if (optional[i + 1] == null)
            {
                throw new ArgumentException($"Missing value for option \"{optional[i]}\"");
            }
            optionalArgs.Add(optional[i], optional[i + 1]);
        }

        return VipsOperation.CallByName(operationName, requiredArgs.ToArray(), optionalArgs);
    }

    public static int CallSplitOptionString(string operationName, string optionString, params object[] args)
    {
        var requiredArgs = new List<object>();
        var optionalArgs = new Dictionary<string, object>();

        for (int i = 0; i < args.Length; i++)
        {
            if ((i & 1) == 0)
            {
                requiredArgs.Add(args[i]);
            }
            else
            {
                optionalArgs.Add((string)args[i - 1], args[i]);
            }
        }

        return VipsOperation.CallByName(operationName, requiredArgs.ToArray(), optionalArgs.Keys.ToArray());
    }

    public static int CallOptions(string operationName, params string[] options)
    {
        var requiredArgs = new List<object>();
        var optionalArgs = new Dictionary<string, object>();

        for (int i = 0; i < options.Length; i += 2)
        {
            if (options[i + 1] == null)
            {
                throw new ArgumentException($"Missing value for option \"{options[i]}\"");
            }
            optionalArgs.Add(options[i], options[i + 1]);
        }

        return VipsOperation.CallByName(operationName, requiredArgs.ToArray(), optionalArgs);
    }

    public static int CallArgv(VipsOperation operation, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("No arguments provided");
        }

        var requiredArgs = new List<object>();
        var optionalArgs = new Dictionary<string, object>();

        for (int i = 0; i < args.Length; i++)
        {
            if ((i & 1) == 0)
            {
                requiredArgs.Add(args[i]);
            }
            else
            {
                optionalArgs.Add(args[i - 1], args[i]);
            }
        }

        return VipsOperation.CallRequiredOptional(operation, requiredArgs.ToArray(), optionalArgs);
    }

    public static void BlockSet(string name, bool state)
    {
        Type type = VipsType.Find(name);

        if (type != null && IsSubclassOf(type, typeof(VipsOperation)))
        {
            var operationClass = Activator.CreateInstance(type) as VipsOperation;
            operationClass.Flags |= state ? OperationFlags.Blocked : 0;
        }
    }

    public static void Invalidate(VipsOperation operation)
    {
        g_signal_emit(operation, InvalidateSignal, 0);
    }

    protected override string Usage()
    {
        return base.Usage();
    }

    protected override void Dump(VipsBuf buf)
    {
        base.Dump(buf);
    }

    protected override void Summary(VipsBuf buf)
    {
        base.Summary(buf);
    }
}
```

Note that this is a simplified version of the original code, and some parts may not be fully implemented. Additionally, some types and methods have been omitted or replaced with their C# equivalents.

Also note that this code uses the `VipsObject` class as a base class for `VipsOperation`, which is not shown in the provided code snippet. You will need to implement this class separately.

Please let me know if you need further assistance!