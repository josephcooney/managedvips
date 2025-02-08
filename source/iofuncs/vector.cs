Here is the converted C# code:

```csharp
// vips__vector_init
public static void VipsVectorInit()
{
    #if HAVE_ORC
        Orc.Init();
    #endif

    // Check whether any features are being disabled by the environment.
    string env = Environment.GetEnvironmentVariable("VIPS_VECTOR");
    if (env != null)
        VipsVectorDisableTargets(g_ascii_strtoll(env, null, 0));

    // Look for the deprecated IM_NOVECTOR environment variable as well.
    if (Environment.GetEnvironmentVariable("VIPS_NOVECTOR") != null
#if ENABLE_DEPRECATED
        || Environment.GetEnvironmentVariable("IM_NOVECTOR") != null
#endif
        )
        VipsVectorEnabled = false;
}

// vips_vector_isenabled
public static bool VipsVectorIsEnabled()
{
    #if HAVE_HWY
        return VipsVectorEnabled && VipsVectorGetSupportedTargets() != 0;
    #elif defined(HAVE_ORC)
        return VipsVectorEnabled;
    #else
        return false;
    #endif
}

// vips_vector_set_enabled
public static void VipsVectorSetEnabled(bool enabled)
{
    VipsVectorEnabled = enabled;
}

// vips_vector_get_builtin_targets
public static long VipsVectorGetBuiltInTargets()
{
    #if HAVE_HWY
        return HWY_TARGETS;
    #else
        return 0;
    #endif
}

// vips_vector_get_supported_targets
public static long VipsVectorGetSupportedTargets()
{
    #if HAVE_HWY
        return hwy::SupportedTargets() & ~(HWY_EMU128 | HWY_SCALAR);
    #elif defined(HAVE_ORC)
        return OrcTargetGetDefaultFlags(OrcTargetGetDefault());
    #else
        return 0;
    #endif
}

// vips_vector_target_name
public static string VipsVectorTargetName(long target)
{
    #if HAVE_HWY
        return hwy::TargetName(target);
    #elif defined(HAVE_ORC)
        return OrcTargetGetFlagName(OrcTargetGetDefault(), (int)Math.Log(target, 2));
    #else
        return null;
    #endif
}

// vips_vector_disable_targets
public static void VipsVectorDisableTargets(long disabledTargets)
{
    #if HAVE_HWY
        hwy::SetSupportedTargetsForTest(VipsVectorGetSupportedTargets() & ~disabledTargets);
    #endif
}
```

Note that I've assumed the `g_ascii_strtoll` function is equivalent to `long.Parse` in C#. If this is not the case, please let me know and I'll adjust the code accordingly.