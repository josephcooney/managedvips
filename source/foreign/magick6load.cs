Here is the converted C# code:

```csharp
// load with libMagick
//
// 5/12/11
// 	- from openslideload.c
// 17/1/12
// 	- remove header-only loads
// 11/6/13
// 	- add @all_frames option, off by default
// 14/2/16
// 	- add @page option, 0 by default
// 25/11/16
// 	- add @n, deprecate @all_frames (just sets n = -1)
// 8/9/17
// 	- don't cache magickload
// 21/4/21 kleisauke
// 	- include GObject part from magickload.c

using System;

namespace Vips {
    public class ForeignLoadMagick : ForeignLoad {
        // Deprecated. Just sets n = -1.
        public bool AllFrames { get; set; }

        public string Density { get; set; } // Load at this resolution
        public int Page { get; set; }       // Load this page (frame)
        public int N { get; set; }          // Load this many pages

        public ForeignLoadMagick() {
            N = 1;
        }
    }

    public class ForeignLoadMagickFile : ForeignLoadMagick {
        public string Filename { get; set; }

        public ForeignLoadMagickFile() { }
    }

    public class ForeignLoadMagickBuffer : ForeignLoadMagick {
        public Blob Buffer { get; set; }

        public ForeignLoadMagickBuffer() { }
    }

    // This file is part of VIPS.
    //
    // VIPS is free software; you can redistribute it and/or modify
    // it under the terms of the GNU Lesser General Public License as published by
    // the Free Software Foundation; either version 2 of the License, or
    // (at your option) any later version.

    // This program is distributed in the hope that it will be useful,
    // but WITHOUT ANY WARRANTY; without even the implied warranty of
    // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    // GNU Lesser General Public License for more details.

    // You should have received a copy of the GNU Lesser General Public License
    // along with this program; if not, write to the Free Software
    // Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
    // 02110-1301  USA

    public class ForeignFlags {
        public const int PARTIAL = 0;
    }

    public abstract class ForeignLoad : Object {
        public virtual ForeignFlags GetFlagsFilename(string filename) {
            return ForeignFlags.PARTIAL;
        }

        public virtual ForeignFlags GetFlags() {
            return ForeignFlags.PARTIAL;
        }
    }

    public class VipsForeignLoadMagickClass : Type {
        public static readonly Type Type = typeof(VipsForeignLoadMagick);

        public override Type GetParentType() {
            return typeof(ForeignLoad);
        }
    }

    public class VipsForeignLoadMagickFileClass : Type {
        public static readonly Type Type = typeof(VipsForeignLoadMagickFile);

        public override Type GetParentType() {
            return typeof(VipsForeignLoadMagick);
        }
    }

    public class VipsForeignLoadMagickBufferClass : Type {
        public static readonly Type Type = typeof(VipsForeignLoadMagickBuffer);

        public override Type GetParentType() {
            return typeof(VipsForeignLoadMagick);
        }
    }

    public class ForeignLoadMagickFile : ForeignLoadMagick {
        // Unfortunately, libMagick does not support header-only reads very well. See
        //
        // http://www.imagemagick.org/discourse-server/viewtopic.php?f=1&t=20017
        //
        // Test especially with BMP, GIF, TGA. So we are forced to read the entire
        // image in the @header() method.
        public override int Header(ForeignLoad load) {
            VipsForeignLoadMagick magick = (VipsForeignLoadMagick)load;
            VipsForeignLoadMagickFile magick_file = (VipsForeignLoadMagickFile)load;

            if (magick.AllFrames)
                magick.N = -1;

            if (vips__magick_read(magick_file.Filename,
                    load.Out, magick.Density,
                    magick.Page, magick.N))
                return -1;

            VIPS_SETSTR(load.Out.Filename, magick_file.Filename);

            return 0;
        }
    }

    public class ForeignLoadMagickBuffer : ForeignLoadMagick {
        // Unfortunately, libMagick does not support header-only reads very well. See
        //
        // http://www.imagemagick.org/discourse-server/viewtopic.php?f=1&t=20017
        //
        // Test especially with BMP, GIF, TGA. So we are forced to read the entire
        // image in the @header() method.
        public override int Header(ForeignLoad load) {
            VipsForeignLoadMagick magick = (VipsForeignLoadMagick)load;
            VipsForeignLoadMagickBuffer magick_buffer = (VipsForeignLoadMagickBuffer)load;

            if (magick.AllFrames)
                magick.N = -1;

            if (vips__magick_read_buffer(
                    magick_buffer.Buffer.Data, magick_buffer.Buffer.Length,
                    load.Out, magick.Density, magick.Page,
                    magick.N))
                return -1;

            return 0;
        }
    }

    public class VipsForeignLoadMagickBufferIsABuffer : ForeignLoad {
        // Unfortunately, libMagick does not support header-only reads very well. See
        //
        // http://www.imagemagick.org/discourse-server/viewtopic.php?f=1&t=20017
        //
        // Test especially with BMP, GIF, TGA. So we are forced to read the entire
        // image in the @header() method.
        public override bool IsABuffer(byte[] buf, int len) {
            return len > 10 && magick_ismagick(buf, len);
        }
    }

    public class VipsForeignLoadMagickFileIsAMagick : ForeignLoad {
        public override bool IsAMagick(string filename) {
            // Fetch up to the first 100 bytes. Hopefully that'll be enough.
            byte[] buf = new byte[100];
            int len;

            return (len = vips__get_bytes(filename, buf, 100)) > 10 &&
                magick_ismagick(buf, len);
        }
    }

    public class VipsForeignLoadMagickFileClassInit : Type {
        public static readonly Type Type = typeof(VipsForeignLoadMagickFile);

        public override void ClassInit(TypeClass type_class) {
            // Don't cache magickload: it can gobble up memory and disc.
            operation_flags |= VIPS_OPERATION_NOCACHE;

            // *magick is fuzzed, but it's such a huge thing it's safer to
            // disable it.
            operation_flags |= VIPS_OPERATION_UNTRUSTED;

            // We need to be well to the back of the queue since vips's
            // dedicated loaders are usually preferable.
            priority = -100;
        }
    }

    public class VipsForeignLoadMagickBufferClassInit : Type {
        public static readonly Type Type = typeof(VipsForeignLoadMagickBuffer);

        public override void ClassInit(TypeClass type_class) {
            // Don't cache magickload: it can gobble up memory and disc.
            operation_flags |= VIPS_OPERATION_NOCACHE;

            // *magick is fuzzed, but it's such a huge thing it's safer to
            // disable it.
            operation_flags |= VIPS_OPERATION_UNTRUSTED;

            // We need to be well to the back of the queue since vips's
            // dedicated loaders are usually preferable.
            priority = -100;
        }
    }

    public class VipsForeignLoadMagickFileProperty : Property {
        public static readonly Property Density = new Property(
                "density", typeof(string), 21,
                _("Density"),
                _("Canvas resolution for rendering vector formats like SVG"),
                VIPS_ARGUMENT_OPTIONAL_INPUT,
                G_STRUCT_OFFSET(VipsForeignLoadMagick, density),
                null);

        public static readonly Property Page = new Property(
                "page", typeof(int), 22,
                _("Page"),
                _("First page to load"),
                VIPS_ARGUMENT_OPTIONAL_INPUT,
                G_STRUCT_OFFSET(VipsForeignLoadMagick, page),
                0, 100000, 0);

        public static readonly Property N = new Property(
                "n", typeof(int), 23,
                _("n"),
                _("Number of pages to load, -1 for all"),
                VIPS_ARGUMENT_OPTIONAL_INPUT,
                G_STRUCT_OFFSET(VipsForeignLoadMagick, n),
                -1, 100000, 1);

        public static readonly Property AllFrames = new Property(
                "all_frames", typeof(bool), 20,
                _("All frames"),
                _("Read all frames from an image"),
                VIPS_ARGUMENT_OPTIONAL_INPUT | VIPS_ARGUMENT_DEPRECATED,
                G_STRUCT_OFFSET(VipsForeignLoadMagick, all_frames),
                false);
    }

    public class VipsForeignLoadMagickBufferProperty : Property {
        public static readonly Property Buffer = new Property(
                "buffer", typeof(Blob), 1,
                _("Buffer"),
                _("Buffer to load from"),
                VIPS_ARGUMENT_REQUIRED_INPUT,
                G_STRUCT_OFFSET(VipsForeignLoadMagickBuffer, buffer),
                null);
    }
}
```

Note that this is a direct conversion of the C code and may not be perfect. Some parts of the code might need adjustments to fit the C# syntax and conventions. Additionally, some types like `VIPS_TYPE_BLOB` or `G_STRUCT_OFFSET` are not directly available in C#. You would need to create your own equivalent types or use existing ones if they exist.

Also note that this conversion is quite complex due to the amount of code involved. It's recommended to review and test each part of the converted code carefully before using it in a production environment.