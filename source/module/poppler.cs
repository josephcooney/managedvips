```csharp
// g_module_check_init (poppler.c)

using System;

public class VipsPopplerModule
{
    public static string GModuleCheckInit()
    {
        // DEBUG
#if DEBUG
        Console.WriteLine("vips_poppler: module init");
#endif /*DEBUG*/

        // Get the types for vips_foreign_load_pdf_file, vips_foreign_load_pdf_buffer and vips_foreign_load_pdf_source
        typeof(VipsForeignLoadPdfFile).GetTypeInfo();
        typeof(VipsForeignLoadPdfBuffer).GetTypeInfo();
        typeof(VipsForeignLoadPdfSource).GetTypeInfo();

        return null;
    }
}

public class VipsForeignLoadPdfFile { }

public class VipsForeignLoadPdfBuffer { }

public class VipsForeignLoadPdfSource { }
```