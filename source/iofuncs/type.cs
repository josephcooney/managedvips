Here is the converted C# code:

```csharp
using System;
using System.Collections.Generic;

public class VipsThing : IDisposable
{
    public int i;

    public VipsThing(int i)
    {
        this.i = i;
    }

    public static VipsThing Copy(VipsThing thing)
    {
        return new VipsThing(thing.i);
    }

    public void Dispose()
    {
        // No equivalent to g_free in C#
        // This is a placeholder, you should implement the correct disposal logic
    }
}

public class VipsArea : IDisposable
{
    private int count;
    private System.Threading.Mutex lock;
    private int length;
    private object data;
    private VipsCallbackFn free_fn;
    private GType type;
    private size_t sizeof_type;

    public VipsArea(VipsCallbackFn free_fn, object data)
    {
        this.count = 1;
        this.lock = new System.Threading.Mutex();
        this.length = 0;
        this.data = data;
        this.free_fn = free_fn;
        this.type = 0;
        this.sizeof_type = 0;

        if (Vips.__leak)
        {
            Vips.area_all.Add(this);
        }
    }

    public static VipsArea Copy(VipsArea area)
    {
        return new VipsArea(area.free_fn, area.data);
    }

    public void Dispose()
    {
        lock.EnterWriteLock();
        try
        {
            if (free_fn != null && data != null)
            {
                free_fn(data, this);
                free_fn = null;
            }
            data = null;

            count--;

            if (count == 0)
            {
                Vips.area_free(this);

                lock.Dispose();

                GCHandle handle = GCHandle.Alloc(this);
                handle.Free();
                handle = default(GCHandle);

                if (Vips.__leak)
                {
                    lock.EnterWriteLock();
                    try
                    {
                        Vips.area_all.Remove(this);
                    }
                    finally
                    {
                        lock.ExitWriteLock();
                    }
                }

#ifdef DEBUG
                lock.EnterWriteLock();
                try
                {
                    Console.WriteLine("vips_area_unref: free .. total = " + Vips.area_all.Count());
                }
                finally
                {
                    lock.ExitWriteLock();
                }
#endif // DEBUG
            }
            else
            {
                lock.ExitWriteLock();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error disposing VipsArea: " + ex.Message);
        }
    }

    public static void Unref(VipsArea area)
    {
        area.Dispose();
    }

    public static VipsArea New(VipsCallbackFn free_fn, object data)
    {
        return new VipsArea(free_fn, data);
    }

    public static VipsArea NewArray(GType type, size_t sizeof_type, int n)
    {
        object[] array = new object[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = null;
        }
        return new VipsArea((VipsCallbackFn)Vips.area_free_cb, array);
    }

    public static void FreeArrayObject(object[] array)
    {
        foreach (object obj in array)
        {
            if (obj != null)
            {
                GCHandle handle = GCHandle.Alloc(obj);
                handle.Free();
                handle = default(GCHandle);
            }
        }
    }

    public static VipsArea NewArrayObject(int n)
    {
        object[] array = new object[n + 1];
        for (int i = 0; i < n + 1; i++)
        {
            array[i] = null;
        }
        return new VipsArea((VipsCallbackFn)Vips.area_free_array_object, array);
    }

    public static void Get(VipsArea area, out size_t length, out int n, out GType type, out size_t sizeof_type)
    {
        if (area != null)
        {
            length = area.length;
            n = area.n;
            type = area.type;
            sizeof_type = area.sizeof_type;
        }
    }

    public static object GetData(VipsArea area, out size_t length, out int n, out GType type, out size_t sizeof_type)
    {
        if (area != null)
        {
            Get(area, out length, out n, out type, out sizeof_type);
            return area.data;
        }
        else
        {
            return null;
        }
    }

    public static void TransformAreaGString(GValue src_value, GValue dest_value)
    {
        VipsArea area = (VipsArea)src_value.Value;
        char[] buf = new char[256];
        string str = "VIPS_TYPE_AREA, count = " + area.count + ", data = " + area.data.ToString();
        dest_value.Value = str;
    }

    public class SaveString : IDisposable
    {
        private string value;

        public SaveString(string value)
        {
            this.value = value;
        }

        public static SaveString New(int i)
        {
            return new SaveString(i.ToString());
        }

        public static SaveString New(double d)
        {
            return new SaveString(d.ToString());
        }

        public static SaveString New(float f)
        {
            return new SaveString(f.ToString());
        }

        public void Dispose()
        {
            // No equivalent to g_free in C#
            // This is a placeholder, you should implement the correct disposal logic
        }
    }

    public class RefString : IDisposable
    {
        private VipsArea area;

        public RefString(VipsArea area)
        {
            this.area = area;
        }

        public static RefString New(string str)
        {
            return new RefString(Vips.ref_string_new(str));
        }

        public string Get(out size_t length)
        {
            object data = Vips.area_get_data(area, out length, null, null, null);
            return (string)data;
        }

        public void Dispose()
        {
            // No equivalent to g_free in C#
            // This is a placeholder, you should implement the correct disposal logic
        }
    }

    public class Blob : IDisposable
    {
        private VipsArea area;

        public Blob(VipsArea area)
        {
            this.area = area;
        }

        public static Blob New(VipsCallbackFn free_fn, object data, size_t length)
        {
            return new Blob(Vips.blob_new(free_fn, data, length));
        }

        public static Blob Copy(object data, size_t length)
        {
            return new Blob(Vips.blob_copy(data, length));
        }

        public void Dispose()
        {
            // No equivalent to g_free in C#
            // This is a placeholder, you should implement the correct disposal logic
        }
    }

    public class ArrayInt : IDisposable
    {
        private VipsArea area;

        public ArrayInt(VipsArea area)
        {
            this.area = area;
        }

        public static ArrayInt New(int[] array, int n)
        {
            return new ArrayInt(Vips.array_int_new(array, n));
        }

        public static ArrayInt Newv(params int[] args)
        {
            return new ArrayInt(Vips.array_int_newv(args.Length, args));
        }

        public int[] Get(out int n)
        {
            object data = Vips.area_get_data(area, null, out n, null, null);
            return (int[])data;
        }

        public void Dispose()
        {
            // No equivalent to g_free in C#
            // This is a placeholder, you should implement the correct disposal logic
        }
    }

    public class ArrayDouble : IDisposable
    {
        private VipsArea area;

        public ArrayDouble(VipsArea area)
        {
            this.area = area;
        }

        public static ArrayDouble New(double[] array, int n)
        {
            return new ArrayDouble(Vips.array_double_new(array, n));
        }

        public static ArrayDouble Newv(params double[] args)
        {
            return new ArrayDouble(Vips.array_double_newv(args.Length, args));
        }

        public double[] Get(out int n)
        {
            object data = Vips.area_get_data(area, null, out n, null, null);
            return (double[])data;
        }

        public void Dispose()
        {
            // No equivalent to g_free in C#
            // This is a placeholder, you should implement the correct disposal logic
        }
    }

    public class ArrayImage : IDisposable
    {
        private VipsArea area;

        public ArrayImage(VipsArea area)
        {
            this.area = area;
        }

        public static ArrayImage New(VipsImage[] array, int n)
        {
            return new ArrayImage(Vips.array_image_new(array, n));
        }

        public static ArrayImage Newv(params VipsImage[] args)
        {
            return new ArrayImage(Vips.array_image_newv(args.Length, args));
        }

        public VipsImage[] Get(out int n)
        {
            object data = Vips.area_get_data(area, null, out n, null, null);
            return (VipsImage[])data;
        }

        public void Dispose()
        {
            // No equivalent to g_free in C#
            // This is a placeholder, you should implement the correct disposal logic
        }
    }

    public class Value : IDisposable
    {
        private GValue value;

        public Value(GValue value)
        {
            this.value = value;
        }

        public static void SetArea(GValue value, VipsCallbackFn free_fn, object data)
        {
            Vips.area_set(value, free_fn, data);
        }

        public static object GetArea(GValue value, out size_t length)
        {
            return Vips.area_get_data((VipsArea)value.Value, out length, null, null, null);
        }

        public static string GetSaveString(GValue value)
        {
            return (string)value.Value;
        }

        public static void SetSaveString(GValue value, string str)
        {
            value.Value = str;
        }

        public static void SetSaveStringf(GValue value, string fmt, params object[] args)
        {
            value.Value = string.Format(fmt, args);
        }

        public static string GetRefString(GValue value, out size_t length)
        {
            return (string)Vips.area_get_data((VipsArea)value.Value, out length, null, null, null);
        }

        public static void SetRefString(GValue value, string str)
        {
            Vips.ref_string_set(value, str);
        }

        public static object GetBlob(GValue value, out size_t length)
        {
            return Vips.area_get_data((VipsArea)value.Value, out length, null, null, null);
        }

        public static void SetBlob(GValue value, VipsCallbackFn free_fn, object data, size_t length)
        {
            Vips.blob_set(value, free_fn, data, length);
        }

        public static void SetBlobFree(GValue value, object data, size_t length)
        {
            Vips.blob_set_free(value, data, length);
        }

        public static int[] GetArrayInt(GValue value, out int n)
        {
            return (int[])Vips.area_get_data((VipsArea)value.Value, null, out n, null, null);
        }

        public static void SetArrayInt(GValue value, int[] array, int n)
        {
            Vips.array_int_set(value, array, n);
        }

        public static double[] GetArrayDouble(GValue value, out int n)
        {
            return (double[])Vips.area_get_data((VipsArea)value.Value, null, out n, null, null);
        }

        public static void SetArrayDouble(GValue value, double[] array, int n)
        {
            Vips.array_double_set(value, array, n);
        }

        public static VipsImage[] GetArrayImage(GValue value, out int n)
        {
            return (VipsImage[])Vips.area_get_data((VipsArea)value.Value, null, out n, null, null);
        }

        public static void SetArrayImage(GValue value, int n)
        {
            Vips.array_image_set(value, n);
        }
    }

    public class Meta
    {
        private static Type thingType;
        private static Type saveStringType;
        private static Type areaType;
        private static Type refStringType;
        private static Type blobType;
        private static Type arrayIntType;
        private static Type arrayDoubleType;
        private static Type arrayImageType;

        public static void InitTypes()
        {
            thingType = typeof(VipsThing);
            saveStringType = typeof(SaveString);
            areaType = typeof(VipsArea);
            refStringType = typeof(RefString);
            blobType = typeof(Blob);
            arrayIntType = typeof(ArrayInt);
            arrayDoubleType = typeof(ArrayDouble);
            arrayImageType = typeof(ArrayImage);

            // Register transform functions to convert between an array of integers and doubles
            GValue.RegisterTransformFunc(arrayIntType, arrayDoubleType, new TransformArrayIntToArrayDouble());
            GValue.RegisterTransformFunc(arrayDoubleType, arrayIntType, new TransformArrayDoubleToArrayInt());
        }
    }

    private class TransformArrayIntToArrayDouble : ITransformFunc
    {
        public void Transform(GValue src_value, GValue dest_value)
        {
            int[] array = (int[])src_value.Value;
            double[] newArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
            dest_value.Value = newArray;
        }
    }

    private class TransformArrayDoubleToArrayInt : ITransformFunc
    {
        public void Transform(GValue src_value, GValue dest_value)
        {
            double[] array = (double[])src_value.Value;
            int[] newArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = (int)array[i];
            }
            dest_value.Value = newArray;
        }
    }

    public class TransformFunc : ITransformFunc
    {
        private Func<GValue, GValue> func;

        public TransformFunc(Func<GValue, GValue> func)
        {
            this.func = func;
        }

        public void Transform(GValue src_value, GValue dest_value)
        {
            dest_value.Value = func(src_value);
        }
    }

    public interface ITransformFunc
    {
        void Transform(GValue src_value, GValue dest_value);
    }
}
```

Note that this is not a direct conversion of the C code to C#, but rather an implementation of the VIPS library in C# based on the provided specification. Some parts may have been simplified or modified to fit the C# syntax and idioms.

Also note that some types, such as `VipsImage` and `GType`, are not defined in this code snippet, as they were not provided in the original specification. You will need to define these types yourself or use existing implementations.

This implementation should provide a good starting point for creating a C# version of the VIPS library. However, it may require further modifications and testing to ensure that it works correctly and efficiently.