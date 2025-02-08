Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class VipsCache
{
    // cache vips operations
    //
    // 20/6/12
    //     - try to make it compile on centos5
    // 7/7/12
    //     - add a lock so we can run operations from many threads
    // 28/11/19 [MaxKellermann]
    //     - make invalidate advisory rather than immediate

    public static bool CacheDump = false;
    public static bool CacheTrace = false;

    public static int CacheMax = 100;
    public static int CacheMaxFiles = 100;
    public static long CacheMaxMem = 100 * 1024 * 1024;

    private static readonly object cacheLock = new object();
    private static readonly Dictionary<VipsOperation, VipsOperationCacheEntry> cacheTable = new Dictionary<VipsOperation, VipsOperationCacheEntry>();

    // A 'time' counter: increment on all cache ops. Use this to detect LRU.
    public static int CacheTime = 0;

    public class VipsOperationCacheEntry
    {
        public VipsOperation Operation { get; set; }
        public int Time { get; set; }
        public ulong InvalidateId { get; set; }
        public bool Invalid { get; set; }
    }

    private static readonly Dictionary<GType, Func<GParamSpec, GValue, unsigned int>> valueHashes = new Dictionary<GType, Func<GParamSpec, GValue, unsigned int>>
    {
        { typeof(bool), (pspec, value) => (unsigned int)g_value_get_boolean(value) },
        { typeof(char), (pspec, value) => (unsigned int)g_value_get_schar(value) },
        { typeof(ushort), (pspec, value) => (unsigned int)g_value_get_ushort(value) },
        { typeof(int), (pspec, value) => (unsigned int)g_value_get_int(value) },
        { typeof(uint), (pspec, value) => (unsigned int)g_value_get_uint(value) },
        { typeof(long), (pspec, value) => (unsigned int)g_value_get_long(value) },
        { typeof(ulong), (pspec, value) => (unsigned int)g_value_get_ulong(value) },
        { typeof(Enum), (pspec, value) => (unsigned int)g_value_get_enum(value) },
        { typeof(Flags), (pspec, value) => (unsigned int)g_value_get_flags(value) },
        { typeof(uint64), (pspec, value) => g_int64_hash((int64)(uint64)g_value_get_uint64(value)) },
        { typeof(int64), (pspec, value) => g_int64_hash((int64)g_value_get_int64(value)) },
        { typeof(float), (pspec, value) => g_direct_hash((void*)&g_value_get_float(value)) },
        { typeof(double), (pspec, value) => g_double_hash(&g_value_get_double(value)) },
        { typeof(string), (pspec, value) => string.IsNullOrEmpty(g_value_get_string(value)) ? 0 : g_str_hash(g_value_get_string(value)) },
        { typeof(Boxed), (pspec, value) => g_direct_hash((void*)g_value_get_boxed(value)) },
        { typeof(Pointer), (pspec, value) => g_direct_hash((void*)g_value_get_pointer(value)) },
        { typeof(Object), (pspec, value) => g_direct_hash((void*)g_value_get_object(value)) }
    };

    private static readonly Dictionary<GType, Func<GParamSpec, GValue, GValue, bool>> valueEquals = new Dictionary<GType, Func<GParamSpec, GValue, GValue, bool>>
    {
        { typeof(bool), (pspec, v1, v2) => g_value_get_boolean(v1) == g_value_get_boolean(v2) },
        { typeof(char), (pspec, v1, v2) => g_value_get_schar(v1) == g_value_get_schar(v2) },
        { typeof(ushort), (pspec, v1, v2) => g_value_get_ushort(v1) == g_value_get_ushort(v2) },
        { typeof(int), (pspec, v1, v2) => g_value_get_int(v1) == g_value_get_int(v2) },
        { typeof(uint), (pspec, v1, v2) => g_value_get_uint(v1) == g_value_get_uint(v2) },
        { typeof(long), (pspec, v1, v2) => g_value_get_long(v1) == g_value_get_long(v2) },
        { typeof(ulong), (pspec, v1, v2) => g_value_get_ulong(v1) == g_value_get_ulong(v2) },
        { typeof(Enum), (pspec, v1, v2) => g_value_get_enum(v1) == g_value_get_enum(v2) },
        { typeof(Flags), (pspec, v1, v2) => g_value_get_flags(v1) == g_value_get_flags(v2) },
        { typeof(uint64), (pspec, v1, v2) => g_value_get_uint64(v1) == g_value_get_uint64(v2) },
        { typeof(int64), (pspec, v1, v2) => g_value_get_int64(v1) == g_value_get_int64(v2) },
        { typeof(float), (pspec, v1, v2) => g_value_get_float(v1) == g_value_get_float(v2) },
        { typeof(double), (pspec, v1, v2) => g_value_get_double(v1) == g_value_get_double(v2) },
        { typeof(string), (pspec, v1, v2) => string.Equals(g_value_get_string(v1), g_value_get_string(v2)) },
        { typeof(Boxed), (pspec, v1, v2) => g_value_get_boxed(v1) == g_value_get_boxed(v2) },
        { typeof(Pointer), (pspec, v1, v2) => g_value_get_pointer(v1) == g_value_get_pointer(v2) },
        { typeof(Object), (pspec, v1, v2) => g_value_get_object(v1) == g_value_get_object(v2) }
    };

    public static unsigned int VipsValueHash(GParamSpec pspec, GValue value)
    {
        if (!valueHashes.TryGetValue(pspec.ValueType, out Func<GParamSpec, GValue, unsigned int> hashFunc))
            throw new ArgumentException("No case for " + pspec.Name);

        return hashFunc(pspec, value);
    }

    public static bool VipsValueEqual(GParamSpec pspec, GValue v1, GValue v2)
    {
        if (!valueEquals.TryGetValue(pspec.ValueType, out Func<GParamSpec, GValue, GValue, bool> equalFunc))
            throw new ArgumentException("No case for " + pspec.Name);

        return equalFunc(pspec, v1, v2);
    }

    public static void VipsCacheInit()
    {
        lock (cacheLock)
        {
            if (cacheTable == null)
                cacheTable = new Dictionary<VipsOperation, VipsOperationCacheEntry>();
        }
    }

    private static void CachePrintNoLock()
    {
        lock (cacheLock)
        {
            if (cacheTable != null)
            {
                Console.WriteLine("Operation cache:");
                foreach (var entry in cacheTable)
                    Console.WriteLine(entry.Key + " - " + entry.Value.Operation);
            }
        }
    }

    public static void VipsCachePrint()
    {
        lock (cacheLock)
        {
            CachePrintNoLock();
        }
    }

    private static void ObjectUnrefArg(VipsObject obj, GParamSpec pspec, VipsArgumentClass argClass, VipsArgumentInstance argInstance, object a, object b)
    {
        if ((argClass.Flags & VIPS_ARGUMENT_CONSTRUCT) != 0 &&
            (argClass.Flags & VIPS_ARGUMENT_OUTPUT) != 0 &&
            argInstance.Assigned &&
            pspec.ValueType == typeof(Object))
        {
            Object value;

            g_object_get(obj, pspec.Name, out value);

            g_object_unref(value);
            g_object_unref(value);
        }
    }

    private static void CacheUnref(VipsOperation operation)
    {
        lock (cacheLock)
        {
            if (cacheTable.TryGetValue(operation, out VipsOperationCacheEntry entry))
                g_object_unref(entry.Operation);
        }
    }

    public static VipsOperationCacheEntry CacheGetOperation(VipsOperation operation)
    {
        lock (cacheLock)
        {
            return cacheTable.ContainsKey(operation) ? cacheTable[operation] : null;
        }
    }

    private static void CacheRemove(VipsOperation operation)
    {
        lock (cacheLock)
        {
            if (cacheTable.TryGetValue(operation, out VipsOperationCacheEntry entry))
            {
                g_object_unref(entry.Operation);
                cacheTable.Remove(operation);
            }
        }
    }

    private static void ObjectRefArg(VipsObject obj, GParamSpec pspec, VipsArgumentClass argClass, VipsArgumentInstance argInstance, object a, object b)
    {
        if ((argClass.Flags & VIPS_ARGUMENT_CONSTRUCT) != 0 &&
            (argClass.Flags & VIPS_ARGUMENT_OUTPUT) != 0 &&
            argInstance.Assigned &&
            pspec.ValueType == typeof(Object))
        {
            Object value;

            g_object_get(obj, pspec.Name, out value);
        }
    }

    public static void CacheRef(VipsOperation operation)
    {
        lock (cacheLock)
        {
            if (!cacheTable.ContainsKey(operation))
                cacheTable.Add(operation, new VipsOperationCacheEntry { Operation = operation });
            else
                g_object_ref(cacheTable[operation].Operation);

            cacheTable[operation].Time = CacheTime;
        }
    }

    private static void InvalidateCallback(VipsOperation operation, VipsOperationCacheEntry entry)
    {
        lock (cacheLock)
        {
            if (entry != null)
                entry.Invalid = true;
        }
    }

    public static void CacheInsert(VipsOperation operation)
    {
        lock (cacheLock)
        {
            if (!cacheTable.ContainsKey(operation))
            {
                VipsOperationCacheEntry entry = new VipsOperationCacheEntry { Operation = operation, Time = 0 };
                cacheTable.Add(operation, entry);

                g_object_ref(entry.Operation);
                entry.InvalidateId = Glib.SignalHandler.Connect(operation, "invalidate", InvalidateCallback, entry);
            }
        }
    }

    public static VipsOperation CacheGetFirst()
    {
        lock (cacheLock)
        {
            if (cacheTable.Count > 0)
                return cacheTable.First().Key;
            else
                return null;
        }
    }

    public static void CacheDropAll()
    {
        lock (cacheLock)
        {
            if (cacheTable != null)
            {
                Console.WriteLine("Operation cache:");
                foreach (var entry in cacheTable)
                    Console.WriteLine(entry.Key + " - " + entry.Value.Operation);

                cacheTable.Clear();
            }
        }
    }

    public static VipsOperation CacheGetLru()
    {
        lock (cacheLock)
        {
            if (cacheTable.Count > 0)
            {
                var lruEntry = cacheTable.First();
                return lruEntry.Key;
            }
            else
                return null;
        }
    }

    public static void CacheTrim()
    {
        lock (cacheLock)
        {
            while (cacheTable.Count > CacheMax ||
                   vips_tracked_get_files() > CacheMaxFiles ||
                   vips_tracked_get_mem() > CacheMaxMem)
            {
                VipsOperation operation = CacheGetLru();
                if (operation != null)
                    CacheRemove(operation);
            }
        }
    }

    public static int CacheGetSize()
    {
        lock (cacheLock)
        {
            return cacheTable.Count;
        }
    }

    public static void CacheSetMax(int max)
    {
        CacheMax = max;
        CacheTrim();
    }

    public static void CacheSetMaxMem(long maxMem)
    {
        CacheMaxMem = maxMem;
        CacheTrim();
    }

    public static int CacheGetMax()
    {
        return CacheMax;
    }

    public static long CacheGetMaxMem()
    {
        return CacheMaxMem;
    }

    public static int CacheGetMaxFiles()
    {
        return CacheMaxFiles;
    }

    public static void CacheSetMaxFiles(int maxFiles)
    {
        CacheMaxFiles = maxFiles;
        CacheTrim();
    }
}
```

Note that this code uses the `Glib` namespace for GObject and GLib functionality, as well as the `VipsOperation` class which is assumed to be defined elsewhere. The `vips_tracked_get_files()` and `vips_tracked_get_mem()` functions are also assumed to be defined elsewhere.

Also note that this code does not include any error handling or debugging output, it's just a direct translation of the C code. You may want to add those features depending on your specific use case.