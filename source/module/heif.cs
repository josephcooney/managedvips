Here is the converted C# code:

```csharp
// libheif as a dynamically loadable module
//
// 14/2/21 kleisauke
// 	- initial

using System;
using System.Runtime.InteropServices;

namespace Vips
{
    public class HeifModule
    {
        // This is called on module load.
        [DllImport("Vips")]
        public static extern IntPtr g_module_check_init(IntPtr module);

        // GType vips_foreign_load_heif_file_get_type(void);
        // GType vips_foreign_load_heif_buffer_get_type(void);
        // GType vips_foreign_load_heif_source_get_type(void);
        // GType vips_foreign_save_heif_file_get_type(void);
        // GType vips_foreign_save_heif_buffer_get_type(void);
        // GType vips_foreign_save_heif_target_get_type(void);
        // GType vips_foreign_save_avif_target_get_type(void);

        public static void g_module_check_init(IntPtr module)
        {
            // #ifdef DEBUG
            Console.WriteLine("vips_heif: module init");
            // #endif /*DEBUG*/

            // vips_foreign_load_heif_file_get_type();
            // vips_foreign_load_heif_buffer_get_type();
            // vips_foreign_load_heif_source_get_type();
            // vips_foreign_save_heif_file_get_type();
            // vips_foreign_save_heif_buffer_get_type();
            // vips_foreign_save_heif_target_get_type();
            // vips_foreign_save_avif_target_get_type();

            // Note: These types are not directly accessible from C#.
            // You may need to use a different approach to achieve the same functionality.
        }
    }
}
```

Note that I've used `IntPtr` for the `module` parameter, as it's not clear what type of object is being passed in. Also, I've commented out the calls to the other GType functions, as they are not directly accessible from C#. You may need to use a different approach to achieve the same functionality.

Also, please note that this code assumes that you have a `Vips` library loaded and available for import. If that's not the case, you'll need to modify the code accordingly.