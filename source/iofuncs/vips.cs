Here is the converted C# code:

```csharp
using System;
using System.IO;
using System.Text;

namespace Vips
{
    public class VipsImage
    {
        // ... (rest of the class remains the same)
    }

    public static class VipsExtensions
    {
        public static int OpenImageRead(this VipsImage image, string filename)
        {
            int fd = VipsTrackedOpen(filename, Mode.ReadWrite, 0);
            if (fd == -1)
                fd = VipsTrackedOpen(filename, Mode.ReadOnly, 0);

            if (fd == -1)
            {
                VipsErrorSystem(errno, "VipsImage", _("unable to open \"{0}\""), filename);
                return -1;
            }

            return fd;
        }

        public static int OpenImageWrite(this VipsImage image, string filename, bool temp)
        {
            int flags = Mode.Write;

#ifndef O_TMPFILE
            if (temp)
                Console.WriteLine("vips__open_image_write: O_TMPFILE not available");
#endif

#ifdef O_TMPFILE
            if (temp)
            {
                char[] dirname = Path.GetDirectoryName(filename);
                fd = VipsTrackedOpen(dirname, O_TMPFILE | O_RDWR, 0644);
                GFree(dirname);

                if (fd < 0)
                    Console.WriteLine("vips__open_image_write: O_TMPFILE failed!");
            }
#endif

#ifdef _O_TEMPORARY
            if (temp)
            {
                flags |= _O_TEMPORARY;
                Console.WriteLine("vips__open_image_write: setting _O_TEMPORARY");
            }
#endif

            if (fd < 0)
            {
                fd = VipsTrackedOpen(filename, flags, 0644);
            }

            if (fd < 0)
            {
                Console.WriteLine("vips__open_image_write: failed!");
                VipsErrorSystem(errno, "VipsImage", _("unable to write to \"{0}\""), filename);
                return -1;
            }

            return fd;
        }
    }

    public static class VipsHelper
    {
        private const int ChunkSize = 1024;

        public static char[] ReadChunk(int fd, long offset, int length)
        {
            byte[] buf = new byte[length + 1];

            if (VipsSeek(fd, offset, Seek.Set) == -1)
                return null;
            if (!Read(fd, buf, length))
            {
                VipsError("VipsImage", "%s", _("unable to read history"));
                return null;
            }
            buf[length] = '\0';

            return Encoding.UTF8.GetString(buf);
        }

        public static bool HasExtensionBlock(VipsImage image)
        {
            long psize = ImagePixelLength(image);
            g_assert(image.FileLength > 0);

            return image.FileLength - psize > 0;
        }

        public static void* ReadExtensionBlock(VipsImage image, out int size)
        {
            long psize = ImagePixelLength(image);
            g_assert(image.FileLength > 0);
            if (image.FileLength - psize > 100 * 1024 * 1024)
            {
                VipsError("VipsImage", "%s", _("more than 100 megabytes of XML? sufferin' succotash!"));
                return null;
            }
            if (image.FileLength - psize == 0)
                return null;

            byte[] buf = ReadChunk(image.Fd, psize, image.FileLength - psize);
            size = (int)(image.FileLength - psize);

            return Encoding.UTF8.GetString(buf);
        }

        public static int WriteExtensionBlock(VipsImage image, byte[] buf, int size)
        {
            long length;
            long psize;

            psize = ImagePixelLength(image);
            if ((length = VipsFileLength(image.Fd)) == -1)
                return -1;
            if (length < psize)
            {
                VipsError("VipsImage", "%s", _("file has been truncated"));
                return -1;
            }

            if (VipsFTruncate(image.Fd, psize) || VipsSeek(image.Fd, psize, Seek.Set) == -1)
                return -1;
            if (!Write(image.Fd, buf, size))
                return -1;

            Console.WriteLine("vips__write_extension_block: written {0} bytes of XML to {1}", size, image.Filename);

            return 0;
        }

        public static string BuildXml(VipsImage image)
        {
            VipsTarget target = new VipsTarget();
            StringBuilder sb = new StringBuilder();

            target.Write(" <?xml version=\"1.0\"?>\n");
            target.Write(" <root xmlns=\"" + NamespaceUri + "/vips/" + VipsMajorVersion + "." + VipsMinorVersion + "." + VipsMicroVersion + "\">\n");

            string history = image.GetHistory();
            if (g_utf8_validate(history, -1, null))
            {
                target.Write("  <field type=\"" + g_type_name(VIPS_TYPE_REF_STRING) + "\" name=\"Hist\">");
                target.WriteAmp(history);
                target.Write("</field>\n");
            }

            target.Write("  </header>\n");
            target.Write("  <meta>\n");

            if (image.Map((VipsSListMap2Fn)BuildXmlMeta, target))
            {
                VIPS_UNREF(target);
                return null;
            }

            target.Write("  </meta>\n");
            target.Write(" </root>\n");

            string result = Encoding.UTF8.GetString(target.Text);

            VIPS_UNREF(target);

            return result;
        }

        public static void BuildXmlMeta(VipsImage image, string field, GValue value, object a)
        {
            VipsTarget target = (VipsTarget)a;

            // ... (rest of the method remains the same)
        }
    }
}
```

Note that I've assumed some things about the C# code, such as the existence of certain classes and methods. You may need to modify the code to fit your specific use case.

Also, please note that this is a direct translation of the C code, without any optimizations or improvements for C#. The resulting C# code may not be as efficient or idiomatic as it could be.