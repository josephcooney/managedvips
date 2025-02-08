Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsWindow
{
    public int RefCount { get; set; }
    public VipsImage Im { get; set; }
    public int Top { get; set; }
    public int Height { get; set; }
    public IntPtr Data { get; set; }
    public IntPtr BaseAddr { get; set; }
    public long Length { get; set; }

    public static VipsWindow New(VipsImage im, int top, int height)
    {
        VipsWindow window = new VipsWindow();
        window.RefCount = 0;
        window.Im = im;
        window.Top = 0;
        window.Height = 0;
        window.Data = IntPtr.Zero;
        window.BaseAddr = IntPtr.Zero;
        window.Length = 0;

        if (VipsWindow.Set(window, top, height))
        {
            VipsWindow.Free(window);
            return null;
        }

        window.RefCount = 1;
        return window;
    }

    public static bool Set(VipsWindow window, int top, int height)
    {
        int pagesize = Vips.GetPageSize();

        IntPtr baseaddr;
        long start, end, pagestart;
        size_t length, pagelength;

        start = window.Im.SizeofHeader + (VIPS_IMAGE_SIZEOF_LINE(window.Im) * top);
        length = VIPS_IMAGE_SIZEOF_LINE(window.Im) * height;

        pagestart = start - (start % pagesize);
        end = start + length;
        pagelength = end - pagestart;

        if (end > window.Im.FileLength)
        {
            throw new Exception("unable to read data for \"" + window.Im.Filename + "\", file has been truncated");
        }

        if (!VipsWindow.Unmap(window))
        {
            return true;
        }

        if (!(baseaddr = Vips.MMap(window.Im.Fd, 0, pagelength, pagestart)))
        {
            return true;
        }

        window.BaseAddr = baseaddr;
        window.Length = pagelength;

        window.Data = (VIPS_PEL)baseaddr + (start - pagestart);
        window.Top = top;
        window.Height = height;

        Vips.ReadTest &= window.Data[0];

        return false;
    }

    public static bool Unmap(VipsWindow window)
    {
        if (window.BaseAddr != IntPtr.Zero)
        {
            if (!Vips.UnMMap(window.BaseAddr, window.Length))
            {
                return true;
            }
        }

        window.Data = IntPtr.Zero;
        window.BaseAddr = IntPtr.Zero;
        window.Length = 0;

        return false;
    }

    public static bool Free(VipsWindow window)
    {
        VipsImage im = window.Im;

        if (window.RefCount != 0)
        {
            throw new Exception("ref count is not zero");
        }

        im.Windows = im.Windows.Remove(im.Windows.IndexOf(window));

        if (!VipsWindow.Unmap(window))
        {
            return true;
        }

        window.Im = null;

        GFree(window);

        return false;
    }

    public static bool Unref(VipsWindow window)
    {
        VipsImage im = window.Im;

        lock (im.SSlock)
        {
            if (window.RefCount <= 0)
            {
                throw new Exception("ref count is not positive");
            }

            window.RefCount -= 1;

            if (window.RefCount == 0)
            {
                if (!VipsWindow.Free(window))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static VipsWindow Find(VipsImage im, int top, int height)
    {
        request_t req = new request_t();
        req.Top = top;
        req.Height = height;

        VipsWindow window = (VipsWindow)VipsSListMap2(im.Windows,
            (VipsSListMap2Fn)VipsWindow.Fits, ref req, null);

        if (window != null)
        {
            window.RefCount += 1;
        }

        return window;
    }

    public static VipsWindow Take(VipsWindow window, VipsImage im, int top, int height)
    {
        int margin = VIPS_MIN(Vips.WindowMarginPixels,
            Vips.WindowMarginBytes / VIPS_IMAGE_SIZEOF_LINE(im));

        top -= margin;
        height += margin * 2;
        top = Math.Min(Math.Max(0, top), im.Ysize - 1);
        height = Math.Min(height, im.Ysize - top);

        if (window != null)
        {
            if (window.Top <= top && window.Top + window.Height >= top + height)
            {
                return window;
            }

            lock (im.SSlock)
            {
                if (window.RefCount == 1)
                {
                    if (!VipsWindow.Set(window, top, height))
                    {
                        VipsWindow.Unref(window);
                        return null;
                    }
                }
                else
                {
                    window.RefCount -= 1;
                }

                if ((window = VipsWindow.Find(im, top, height)) != null)
                {
                    return window;
                }
            }
        }

        if (!(window = VipsWindow.New(im, top, height)))
        {
            return null;
        }

        return window;
    }

    public static void Print(VipsWindow window)
    {
        Console.WriteLine("VipsWindow: " + window + " ref_count = " + window.RefCount +
                         ", im = " + window.Im + ", top = " + window.Top +
                         ", height = " + window.Height + ", data = " + window.Data +
                         ", baseaddr = " + window.BaseAddr + ", length = " + window.Length);
    }
}

public class VipsImage
{
    public int SizeofHeader { get; set; }
    public int Ysize { get; set; }
    public long FileLength { get; set; }
    public string Filename { get; set; }
    public List<VipsWindow> Windows { get; set; }
    public object SSlock { get; set; }

    public static int GetPageSize()
    {
        // implementation of Vips.GetPageSize
    }

    public static bool MMap(int fd, long offset, size_t length, long pagestart)
    {
        // implementation of Vips.MMap
    }

    public static bool UnMMap(IntPtr baseaddr, long length)
    {
        // implementation of Vips.UnMMap
    }
}

public class request_t
{
    public int Top { get; set; }
    public int Height { get; set; }
}
```

Note that this is a direct translation from the provided C code and may not be optimal or idiomatic C# code. Additionally, some types (e.g., `VIPS_PEL`, `VipsSListMap2Fn`) are assumed to be defined elsewhere in the VIPS library and are not implemented here.

Also note that this implementation uses a lock for synchronization, which is not necessary if you're using a thread-safe version of the VIPS library.