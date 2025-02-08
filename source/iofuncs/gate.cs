Here is the converted C# code:

```csharp
// gate.cs --- thread profiling

using System;
using System.Collections.Generic;
using System.IO;

public class VipsThreadGateBlock
{
    public VipsThreadGateBlock Prev { get; set; }
    public long[] Time = new long[VIPS_GATE_SIZE];
    public int I { get; set; }
}

// What we track for each gate-name.
public class VipsThreadGate
{
    public string Name { get; set; }
    public VipsThreadGateBlock Start { get; set; }
    public VipsThreadGateBlock Stop { get; set; }
}

// One of these in per-thread private storage.
public class VipsThreadProfile
{
    // <private>

    public string Name { get; set; }
    public System.Threading.Thread Thread { get; set; }
    public Dictionary<string, VipsThreadGate> Gates { get; set; }
    public VipsThreadGate Memory { get; set; }
}

public static class Vips
{
    // vips_profile_set:
    // @profile: %TRUE to enable profile recording
    //
    // If set, vips will record profiling information, and dump it on program
    // exit. These profiles can be analysed with the `vipsprofile` program.
    public static void ProfileSet(bool profile)
    {
        Vips.ThreadProfile = profile;
    }

    private static bool ThreadProfile { get; set; }
}

public class Gate
{
    // vips_thread_gate_block_save:
    //
    // Save a thread gate block to the given file pointer.
    public static void ThreadGateBlockSave(VipsThreadGateBlock block, TextWriter fp)
    {
        for (int i = block.I - 1; i >= 0; i--)
            fp.Write(block.Time[i]);
        fp.WriteLine();
        if (block.Prev != null)
            ThreadGateBlockSave(block.Prev, fp);
    }

    // vips_thread_profile_save_gate:
    //
    // Save a thread gate to the given file pointer.
    public static void ThreadProfileSaveGate(VipsThreadGate gate, TextWriter fp)
    {
        if (gate.Start.I > 0 || gate.Start.Prev != null)
        {
            fp.WriteLine("gate: " + gate.Name);
            fp.WriteLine("start:");
            ThreadGateBlockSave(gate.Start, fp);
            fp.WriteLine("stop:");
            ThreadGateBlockSave(gate.Stop, fp);
        }
    }

    // vips_thread_profile_save_cb:
    //
    // Callback function for saving a thread profile.
    public static void ThreadProfileSaveCb(string key, VipsThreadGate value, TextWriter data)
    {
        VipsThreadGate gate = value;
        TextWriter fp = data;

        ThreadProfileSaveGate(gate, fp);
    }

    // vips_thread_profile_save:
    //
    // Save the thread profile to the given file pointer.
    public static void ThreadProfileSave(VipsThreadProfile profile)
    {
        lock (Vips.GlobalLock)
        {
            VIPS.DebugMsg("vips_thread_profile_save: " + profile.Name);

            if (!Vips.ThreadFP.HasValue)
            {
                Vips.ThreadFP = File.OpenWrite("vips-profile.txt");
                if (Vips.ThreadFP == null)
                {
                    lock (Vips.GlobalLock);
                    VIPS.Warning("unable to create profile log");
                    return;
                }

                Console.WriteLine("recording profile in vips-profile.txt");
            }

            using (TextWriter fp = new StreamWriter(Vips.ThreadFP.Value))
            {
                fp.WriteLine("thread: " + profile.Name + " (" + profile.Thread.ManagedThreadId + ")");
                foreach (var gate in profile.Gates)
                    ThreadProfileSaveCb(gate.Key, gate.Value, fp);
                ThreadProfileSaveGate(profile.Memory, fp);
            }
        }
    }

    // vips_thread_gate_block_free:
    //
    // Free a thread gate block.
    public static void ThreadGateBlockFree(VipsThreadGateBlock block)
    {
        if (block.Prev != null)
            ThreadGateBlockFree(block.Prev);
        VIPS.Free(block);
    }

    // vips_thread_gate_free:
    //
    // Free a thread gate.
    public static void ThreadGateFree(VipsThreadGate gate)
    {
        ThreadGateBlockFree(gate.Start);
        ThreadGateBlockFree(gate.Stop);
        VIPS.Free(gate);
    }

    // vips_thread_profile_free:
    //
    // Free a thread profile.
    public static void ThreadProfileFree(VipsThreadProfile profile)
    {
        VIPS.DebugMsg("vips_thread_profile_free: " + profile.Name);

        if (profile.Gates != null)
            foreach (var gate in profile.Gates.Values)
                ThreadGateFree(gate);
        if (profile.Memory != null)
            ThreadGateFree(profile.Memory);
        VIPS.Free(profile);
    }

    public static void ThreadProfileStop()
    {
        if (Vips.ThreadProfile)
            VIPS.Free(Vips.ThreadFP.Value);
    }

    // thread_profile_destroy_notify:
    //
    // Notify function for destroying a thread profile.
    private static void ThreadProfileDestroyNotify(object data)
    {
        VipsThreadProfile profile = (VipsThreadProfile)data;

        if (profile != null)
            ThreadProfileFree(profile);
    }

    public class GlobalLock
    {
        public static object Lock { get; set; }
    }

    // vips_thread_gate_new:
    //
    // Create a new thread gate.
    public static VipsThreadGate ThreadGateNew(string name)
    {
        VipsThreadGate gate = new VipsThreadGate();
        gate.Name = name;
        gate.Start = new VipsThreadGateBlock();
        gate.Stop = new VipsThreadGateBlock();

        return gate;
    }

    // vips__thread_profile_attach:
    //
    // Attach a thread profile to the given thread.
    public static void ThreadProfileAttach(string threadName)
    {
        VIPS.DebugMsg("vips__thread_profile_attach: " + threadName);

        VipsThreadProfile profile = new VipsThreadProfile();
        profile.Name = threadName;
        profile.Thread = System.Threading.Thread.CurrentThread;
        profile.Gates = new Dictionary<string, VipsThreadGate>();
        profile.Memory = ThreadGateNew("memory");
    }

    // vips_thread_profile_get:
    //
    // Get the current thread profile.
    public static VipsThreadProfile ThreadProfileGet()
    {
        return (VipsThreadProfile)GPrivate.Get(Vips.ThreadProfileKey);
    }

    private static object ThreadProfileKey { get; set; }
}

public class GPrivate
{
    public static T Get<T>(object key)
    {
        // implementation of g_private_get
    }

    public static void Set(object key, object value)
    {
        // implementation of g_private_set
    }
}
```

Note that I've assumed the existence of a `VIPS` class with methods like `DebugMsg`, `Warning`, and `Free`. You'll need to implement these classes and methods according to your requirements.

Also, I've used the `System.Threading` namespace for thread-related functionality. If you're using .NET Core or .NET 5+, you may need to use the `Microsoft.Extensions.Caching.Memory` package instead.

Please note that this is a direct conversion of the C code to C#, without any optimizations or improvements specific to C#. You may want to review and refactor the code to make it more idiomatic and efficient.