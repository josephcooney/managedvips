Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;

// VIPS package handling.
//
// J. Cupitt, 8/4/93.
//
// 18/2/04 JC
//	- now uses g_module_*() instead of dlopen()
// 9/8/04
//	- uses glib dir scanning stuff instead of dirent.h
// 20/5/08
// 	- note_dependencies() does IMAGEVEC as well as IMAGE
// 5/8/08
// 	- silent success in loading plugins if the dir isn't there

public class VipsPackageHandling
{
    // Standard VIPS packages.
    public static im_package Arithmetic { get; } = new im_package("arithmetic");
    public static im_package Cimg { get; } = new im_package("cimg");
    public static im_package Colour { get; } = new im_package("colour");
    public static im_package Conversion { get; } = new im_package("conversion");
    public static im_package Convolution { get; } = new im_package("convolution");
    public static im_package Deprecated { get; } = new im_package("deprecated");
    public static im_package Format { get; } = new im_package("format");
    public static im_package FreqFilt { get; } = new im_package("freq_filt");
    public static im_package HistogramsLut { get; } = new im_package("histograms_lut");
    public static im_package Inplace { get; } = new im_package("inplace");
    public static im_package Mask { get; } = new im_package("mask");
    public static im_package Morphology { get; } = new im_package("morphology");
    public static im_package Mosaicing { get; } = new im_package("mosaicing");
    public static im_package Other { get; } = new im_package("other");
    public static im_package Resample { get; } = new im_package("resample");
    public static im_package Video { get; } = new im_package("video");

    // im_guess_prefix() args.
    private static readonly im_arg_desc[] GuessPrefixArgs =
    {
        new im_arg_desc("argv0", typeof(string)),
        new im_arg_desc("env_name", typeof(string)),
        new im_arg_desc("PREFIX", typeof(string))
    };

    public static int GuessPrefixVec(im_object argv)
    {
        string prefix = VipsGuessPrefix(argv[0], argv[1]);

        if (prefix == null)
        {
            argv[2] = null;
            return -1;
        }

        argv[2] = im_strdup(null, prefix);

        return 0;
    }

    // Description of im_guess_prefix.
    public static readonly im_function GuessPrefixDesc =
    {
        Name = "im_guess_prefix",
        Description = "guess install area",
        Flags = 0,
        DispatchFunction = GuessPrefixVec,
        Argc = GuessPrefixArgs.Length,
        Args = GuessPrefixArgs
    };

    // im_guess_libdir() args.
    private static readonly im_arg_desc[] GuessLibdirArgs =
    {
        new im_arg_desc("argv0", typeof(string)),
        new im_arg_desc("env_name", typeof(string)),
        new im_arg_desc("LIBDIR", typeof(string))
    };

    public static int GuessLibdirVec(im_object argv)
    {
        string libdir = VipsGuessLibdir(argv[0], argv[1]);

        if (libdir == null)
        {
            argv[2] = null;
            return -1;
        }

        argv[2] = im_strdup(null, libdir);

        return 0;
    }

    // Description of im_guess_libdir.
    public static readonly im_function GuessLibdirDesc =
    {
        Name = "im_guess_libdir",
        Description = "guess library area",
        Flags = 0,
        DispatchFunction = GuessLibdirVec,
        Argc = GuessLibdirArgs.Length,
        Args = GuessLibdirArgs
    };

    // ...

    public static void Main()
    {
        // im_run_command(name, argc, argv)
        int argc = Environment.GetCommandLineArgs().Length - 1;
        string[] argv = Environment.GetCommandLineArgs();
        if (im_run_command(argv[0], argc, argv) != 0)
            return;

        Console.WriteLine("VIPS package handling.");
    }
}

public class im_package
{
    public string Name { get; set; }

    public im_package(string name)
    {
        Name = name;
    }
}

public class im_function
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Flags { get; set; }
    public Action<im_object> DispatchFunction { get; set; }
    public int Argc { get; set; }
    public im_arg_desc[] Args { get; set; }
}

public class im_arg_desc
{
    public string Name { get; set; }
    public Type Type { get; set; }

    public im_arg_desc(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}

public class im_object
{
    public object[] Args { get; set; }

    public im_object()
    {
        Args = new object[0];
    }
}

public static class Vips
{
    public static string GuessPrefix(string argv0, string env_name)
    {
        // implementation of vips_guess_prefix
    }

    public static string GuessLibdir(string argv0, string env_name)
    {
        // implementation of vips_guess_libdir
    }

    public static int Version(int flag)
    {
        // implementation of vips_version
    }
}

public class im_type_desc
{
    public string Type { get; set; }
    public Action<object, string> Init { get; set; }
    public bool Flags { get; set; }
    public Action<object> Print { get; set; }
    public Action<object> Dest { get; set; }
}

public class im_imagevec_object
{
    public VipsImage[] Vec { get; set; }

    public im_imagevec_object(VipsImage[] vec)
    {
        Vec = vec;
    }
}
```

Note that I've omitted the implementation of some functions and classes, as they are not provided in the original C code. You will need to implement them according to your requirements.

Also, please note that this is a simplified version of the original C code, and you may need to modify it to fit your specific needs.

Please let me know if you have any questions or need further assistance!