```csharp
// vips_mosaicing_operation_init

public static void VipsMosaicingOperationInit()
{
    // extern GType vips_merge_get_type(void);
    // extern GType vips_mosaic_get_type(void);
    // extern GType vips_mosaic1_get_type(void);
    // extern GType vips_match_get_type(void);
    // extern GType vips_globalbalance_get_type(void);
    // extern GType vips_matrixinvert_get_type(void);

    VipsMerge.GetGType();
    VipsMosaic.GetGType();
    VipsMosaic1.GetGType();
    VipsMatrixInvert.GetGType();
    VipsMatch.GetGType();
    VipsGlobalbalance.GetGType();
}
```