```csharp
// g_module_check_init
public static string GModuleCheckInit()
{
    // DEBUG
    Console.WriteLine("vips_magick: module init");

    // Load types for foreign file and buffer loading/saving
    typeof(VipsForeignLoadMagickFile).GetTypeInfo();
    typeof(VipsForeignLoadMagickBuffer).GetTypeInfo();
    typeof(VipsForeignLoadMagick7File).GetTypeInfo();
    typeof(VipsForeignLoadMagick7Buffer).GetTypeInfo();
    typeof(VipsForeignSaveMagickFile).GetTypeInfo();
    typeof(VipsForeignSaveMagickBmpFile).GetTypeInfo();
    typeof(VipsForeignSaveMagickBuffer).GetTypeInfo();
    typeof(VipsForeignSaveMagickBmpBuffer).GetTypeInfo();

#ifdef ENABLE_MAGICKLOAD
#ifdef HAVE_MAGICK6
    VipsForeignLoadMagickFile.GetTypeInfo();
    VipsForeignLoadMagickBuffer.GetTypeInfo();
#endif /*HAVE_MAGICK6*/

#ifdef HAVE_MAGICK7
    VipsForeignLoadMagick7File.GetTypeInfo();
    VipsForeignLoadMagick7Buffer.GetTypeInfo();
#endif /*HAVE_MAGICK7*/
#endif /*ENABLE_MAGICKLOAD*/

#ifdef ENABLE_MAGICKSAVE
    VipsForeignSaveMagickFile.GetTypeInfo();
    VipsForeignSaveMagickBmpFile.GetTypeInfo();
    VipsForeignSaveMagickBuffer.GetTypeInfo();
    VipsForeignSaveMagickBmpBuffer.GetTypeInfo();
#endif /*ENABLE_MAGICKSAVE*/

    return null;
}
```