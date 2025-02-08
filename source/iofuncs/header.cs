Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsImage
{
    public int Xsize { get; set; }
    public int Ysize { get; set; }
    public int Bands { get; set; }
    public VipsBandFormat BandFmt { get; set; }
    public VipsCoding Coding { get; set; }
    public VipsInterpretation Type { get; set; }
    public double Xres { get; set; }
    public double Yres { get; set; }
    public int Xoffset { get; set; }
    public int Yoffset { get; set; }
    public string filename { get; set; }
    public string mode { get; set; }

    public List<VipsMeta> meta_traverse = new List<VipsMeta>();
    public Dictionary<string, VipsMeta> meta = new Dictionary<string, VipsMeta>();

    public void InitFields(int xsize, int ysize, int bands, VipsBandFormat format, VipsCoding coding, VipsInterpretation interpretation, double xres, double yres)
    {
        Xsize = xsize;
        Ysize = ysize;
        Bands = bands;
        BandFmt = format;
        Coding = coding;
        Type = interpretation;
        Xres = Math.Max(0, xres);
        Yres = Math.Max(0, yres);
    }

    public static void MetaInit(VipsImage image)
    {
        if (!image.meta)
            image.meta = new Dictionary<string, VipsMeta>();
    }
}

public class VipsMeta
{
    public string name { get; set; }
    public GValue value { get; set; }
    public VipsImage im { get; set; }

    public VipsMeta(VipsImage image, string name, GValue value)
    {
        this.name = name;
        this.value = value;
        this.im = image;

        image.meta.Add(name, this);
        image.meta_traverse.Add(this);
    }
}

public class VipsBandFormat
{
    // Enum values for band format
}

public class VipsCoding
{
    // Enum values for coding
}

public class VipsInterpretation
{
    // Enum values for interpretation
}

public static class VipsImageExtensions
{
    public static int GetWidth(this VipsImage image)
    {
        return image.Xsize;
    }

    public static int GetHeight(this VipsImage image)
    {
        return image.Ysize;
    }

    public static int GetBands(this VipsImage image)
    {
        return image.Bands;
    }

    public static VipsBandFormat GetFormat(this VipsImage image)
    {
        return image.BandFmt;
    }

    public static double GetXres(this VipsImage image)
    {
        return image.Xres;
    }

    public static double GetYres(this VipsImage image)
    {
        return image.Yres;
    }

    public static int GetXoffset(this VipsImage image)
    {
        return image.Xoffset;
    }

    public static int GetYoffset(this VipsImage image)
    {
        return image.Yoffset;
    }

    public static string GetFilename(this VipsImage image)
    {
        return image.filename;
    }

    public static string GetMode(this VipsImage image)
    {
        return image.mode;
    }

    public static double GetScale(this VipsImage image)
    {
        GValue value = new GValue();
        if (image.GetType().GetProperty("scale") != null && image.GetType().GetProperty("scale").GetValue(image) is GValue)
            ((GValue)image.GetType().GetProperty("scale").GetValue(image)).CopyTo(value);
        return value.GetDouble();
    }

    public static double GetOffset(this VipsImage image)
    {
        GValue value = new GValue();
        if (image.GetType().GetProperty("offset") != null && image.GetType().GetProperty("offset").GetValue(image) is GValue)
            ((GValue)image.GetType().GetProperty("offset").GetValue(image)).CopyTo(value);
        return value.GetDouble();
    }

    public static int GetPageHeight(this VipsImage image)
    {
        if (image.meta.ContainsKey(VIPS_META_PAGE_HEIGHT))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(VIPS_META_PAGE_HEIGHT).GetValue(image)).CopyTo(value);
            return (int)value.GetInt();
        }
        else
        {
            return image.Ysize;
        }
    }

    public static int GetNPages(this VipsImage image)
    {
        if (image.meta.ContainsKey(VIPS_META_N_PAGES))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(VIPS_META_N_PAGES).GetValue(image)).CopyTo(value);
            return (int)value.GetInt();
        }
        else
        {
            return 1;
        }
    }

    public static int GetConcurrency(this VipsImage image)
    {
        if (image.meta.ContainsKey(VIPS_META_CONCURRENCY))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(VIPS_META_CONCURRENCY).GetValue(image)).CopyTo(value);
            return (int)value.GetInt();
        }
        else
        {
            return 1;
        }
    }

    public static int GetNSubifds(this VipsImage image)
    {
        if (image.meta.ContainsKey(VIPS_META_N_SUBIFDS))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(VIPS_META_N_SUBIFDS).GetValue(image)).CopyTo(value);
            return (int)value.GetInt();
        }
        else
        {
            return 0;
        }
    }

    public static int GetOrientation(this VipsImage image)
    {
        if (image.meta.ContainsKey(VIPS_META_ORIENTATION))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(VIPS_META_ORIENTATION).GetValue(image)).CopyTo(value);
            return (int)value.GetInt();
        }
        else
        {
            return 1;
        }
    }

    public static bool GetOrientationSwap(this VipsImage image)
    {
        int orientation = GetOrientation(image);
        return orientation >= 5 && orientation <= 8;
    }

    public static void* GetData(this VipsImage image)
    {
        // Implementation of vips_image_get_data()
    }

    public static void Set(VipsImage image, string name, GValue value)
    {
        MetaInit(image);
        image.meta[name] = new VipsMeta(image, name, value);
    }

    public static int Get(this VipsImage image, string name, out GValue value)
    {
        if (image.meta.ContainsKey(name))
        {
            value = image.meta[name].value;
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public static bool Remove(VipsImage image, string name)
    {
        if (image.meta.ContainsKey(name))
        {
            image.meta.Remove(name);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void Map(this VipsImage image, VipsImageMapFn fn, object a)
    {
        // Implementation of vips_image_map()
    }

    public static string[] GetFields(this VipsImage image)
    {
        List<string> fields = new List<string>();
        foreach (var meta in image.meta.Values)
            fields.Add(meta.name);
        return fields.ToArray();
    }

    public static void SetArea(VipsImage image, string name, VipsCallbackFn free_fn, object data)
    {
        GValue value = new GValue();
        vips_value_set_area(value, free_fn, data);
        Set(image, name, value);
    }

    public static int GetBlob(this VipsImage image, string name, out byte[] data, out int length)
    {
        // Implementation of vips_image_get_blob()
    }

    public static void SetInt(VipsImage image, string name, int i)
    {
        GValue value = new GValue();
        value.SetInt(i);
        Set(image, name, value);
    }

    public static int GetInt(this VipsImage image, string name, out int i)
    {
        if (image.meta.ContainsKey(name))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(name).GetValue(image)).CopyTo(value);
            i = value.GetInt();
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public static void SetDouble(VipsImage image, string name, double d)
    {
        GValue value = new GValue();
        value.SetDouble(d);
        Set(image, name, value);
    }

    public static int GetDouble(this VipsImage image, string name, out double d)
    {
        if (image.meta.ContainsKey(name))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(name).GetValue(image)).CopyTo(value);
            d = value.GetDouble();
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public static int GetString(this VipsImage image, string name, out string str)
    {
        if (image.meta.ContainsKey(name))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(name).GetValue(image)).CopyTo(value);
            str = vips_value_get_save_string(value);
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public static void SetString(VipsImage image, string name, string str)
    {
        GValue value = new GValue();
        vips_value_set_ref_string(value, str);
        Set(image, name, value);
    }

    public static int GetAsString(this VipsImage image, string name, out string str)
    {
        if (image.meta.ContainsKey(name))
        {
            GValue value = new GValue();
            ((GValue)image.GetType().GetProperty(name).GetValue(image)).CopyTo(value);
            str = vips_value_get_save_string(value);
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public static void HistoryPrintf(VipsImage image, string fmt, params object[] args)
    {
        // Implementation of vips_image_history_printf()
    }

    public static int HistoryArgs(VipsImage image, string name, int argc, string[] argv)
    {
        // Implementation of vips_image_history_args()
    }

    public static string GetHistory(this VipsImage image)
    {
        return image.Hist;
    }
}

public delegate void VipsImageMapFn(VipsImage image, string field, GValue value, object a);

public delegate void VipsCallbackFn(object data);
```

Note that this is not an exhaustive conversion and some methods are left unimplemented. You will need to complete the implementation based on your specific requirements.

Also, note that C# does not have direct equivalents for some of the C types and functions used in the original code (e.g., `GValue`, `VipsMeta`, etc.). I've assumed that these types and functions are part of a custom library or framework. If this is not the case, you will need to modify the code accordingly.

Finally, keep in mind that this conversion is just an approximation, and some details may have been lost during the translation process.