Here is the converted C# code:

```csharp
using System;

// vips_target_custom_write_real
public gint64 VipsTargetCustomWriteReal(VipsTarget target, IntPtr data, size_t length)
{
    gint64 bytes_written = 0;
    g_signal_emit(target, vips_target_custom_signals[SIG_WRITE], 0, data, (gint64)length, ref bytes_written);
    return bytes_written;
}

// vips_target_custom_read_real
public gint64 VipsTargetCustomReadReal(VipsTarget target, IntPtr buffer, size_t length)
{
    gint64 bytes_read = 0;
    g_signal_emit(target, vips_target_custom_signals[SIG_READ], 0, buffer, (gint64)length, ref bytes_read);
    return bytes_read;
}

// vips_target_custom_seek_real
public gint64 VipsTargetCustomSeekReal(VipsTarget target, gint64 offset, int whence)
{
    GValue args[3] = { G_VALUE_INIT, G_VALUE_INIT, G_VALUE_INIT };
    GValue result = G_VALUE_INIT;
    gint64 new_position;

    g_value_init(&args[0], G_TYPE_OBJECT);
    g_value_set_object(&args[0], target);
    g_value_init(&args[1], G_TYPE_INT64);
    g_value_set_int64(&args[1], offset);
    g_value_init(&args[2], G_TYPE_INT);
    g_value_set_int(&args[2], whence);

    g_value_init(&result, G_TYPE_INT64);
    g_value_set_int64(&result, -1);

    g_signal_emitv((const GValue*)&args, vips_target_custom_signals[SIG_SEEK], 0, &result);
    new_position = g_value_get_int64(&result);

    g_value_unset(&args[0]);
    g_value_unset(&args[1]);
    g_value_unset(&args[2]);
    g_value_unset(&result);

    return new_position;
}

// vips_target_custom_end_real
public int VipsTargetCustomEndReal(VipsTarget target)
{
    int result = 0;
    g_signal_emit(target, vips_target_custom_signals[SIG_END], 0, ref result);
    return result;
}

// vips_target_custom_finish_real
public void VipsTargetCustomFinishReal(VipsTarget target)
{
    g_signal_emit(target, vips_target_custom_signals[SIG_FINISH], 0);
}

// vips_target_custom_write_signal_real
public gint64 VipsTargetCustomWriteSignalReal(VipsTargetCustom target_custom, IntPtr data, gint64 length)
{
    return 0;
}

// vips_target_custom_read_signal_real
public gint64 VipsTargetCustomReadSignalReal(VipsTargetCustom target_custom, IntPtr data, gint64 length)
{
    return 0;
}

// vips_target_custom_seek_signal_real
public gint64 VipsTargetCustomSeekSignalReal(VipsTargetCustom target_custom, gint64 offset, int whence)
{
    return -1;
}

// vips_target_custom_end_signal_real
public int VipsTargetCustomEndSignalReal(VipsTargetCustom target_custom)
{
    return 0;
}

// vips_target_custom_finish_signal_real
public void VipsTargetCustomFinishSignalReal(VipsTargetCustom target_custom)
{
}

// vips_target_custom_class_init
protected override void VipsTargetCustomClassInit()
{
    base.VipsObjectClassInit();
    base.VipsTargetClassInit();

    // Define signals
    vips_target_custom_signals[SIG_WRITE] = g_signal_new("write", G_TYPE_FROM_CLASS(this), G_SIGNAL_ACTION, 0, null, null, vips_INT64__POINTER_INT64, G_TYPE_INT64, 2);
    vips_target_custom_signals[SIG_READ] = g_signal_new("read", G_TYPE_FROM_CLASS(this), G_SIGNAL_ACTION, 0, null, null, vips_INT64__POINTER_INT64, G_TYPE_INT64, 2);
    vips_target_custom_signals[SIG_SEEK] = g_signal_new("seek", G_TYPE_FROM_CLASS(this), G_SIGNAL_ACTION, 0, null, null, vips_INT64__INT64_INT, G_TYPE_INT64, 2);
    vips_target_custom_signals[SIG_END] = g_signal_new("end", G_TYPE_FROM_CLASS(this), G_SIGNAL_ACTION, 0, null, null, vips_INT__VOID, G_TYPE_INT, 0);
    vips_target_custom_signals[SIG_FINISH] = g_signal_new("finish", G_TYPE_FROM_CLASS(this), G_SIGNAL_ACTION, 0, null, null, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);
}

// vips_target_custom_init
protected override void VipsTargetCustomInit()
{
    base.VipsObjectInit();
    base.VipsTargetInit();
}

// vips_target_custom_new
public static VipsTargetCustom* VipsTargetCustomNew()
{
    VipsTargetCustom* target_custom = new VipsTargetCustom();

    if (vips_object_build(VIPS_OBJECT(target_custom)))
    {
        return null;
    }

    return target_custom;
}
```