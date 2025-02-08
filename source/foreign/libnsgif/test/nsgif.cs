```csharp
// nsgif_options.c: Options for the NSGIF utility.

using System;
using System.Collections.Generic;

public class nsgif_options {
    public string file { get; set; }
    public string ppm { get; set; }
    public ulong loops { get; set; }
    public bool palette { get; set; }
    public bool version { get; set; }
    public bool info { get; set; }
    public bool help { get; set; }
}

// cli.c: Command line interface.

public class cli_table_entry {
    public char s { get; set; }
    public string l { get; set; }
    public cli_type t { get; set; }
    public bool no_pos { get; set; }
    public nsgif_options_v v { get; set; }
    public string d { get; set; }
}

public enum cli_type {
    CLI_BOOL,
    CLI_UINT,
    CLI_STRING
}

public struct nsgif_options_v {
    public bool b;
    public uint u;
    public string s;
}

public class cli_table {
    public cli_table_entry[] entries { get; set; }
    public int count { get; set; }
    public int min_positional { get; set; }
    public string d { get; set; }
}

// bitmap.c: Bitmap utilities.

public static void* bitmap_create(int width, int height) {
    if (width > 4096 || height > 4096) {
        return null;
    }

    return Marshal.AllocHGlobal(width * height * 4);
}

public static byte[] bitmap_get_buffer(void* bitmap) {
    return new byte[(int)bitmap.Length];
}

public static void bitmap_destroy(void* bitmap) {
    Marshal.FreeHGlobal(bitmap);
}

// file.c: File utilities.

public static byte[] load_file(string path, out int data_size) {
    using (FileStream fs = new FileStream(path, FileMode.Open)) {
        data_size = (int)fs.Length;
        return ReadFully(fs);
    }
}

private static byte[] ReadFully(FileStream stream)
{
    byte[] buffer = new byte[32768];
    using (MemoryStream ms = new MemoryStream())
    {
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }
}

// warning.c: Warning utilities.

public static void warning(string context, nsgif_error err) {
    Console.WriteLine($"{context}: {nsgif_strerror(err)}");
}

// print_gif_info.c: Print GIF info.

public static void print_gif_info(nsgif_info_t* info) {
    uint8_t[] bg = new uint8_t[3];
    Array.Copy((uint8_t[])info.background, 0, bg, 0, 3);

    Console.WriteLine("gif:");
    Console.WriteLine($"  width: {info.width}");
    Console.WriteLine($"  height: {info.height}");
    Console.WriteLine($"  max-loops: {info.loop_max}");
    Console.WriteLine($"  frame-count: {info.frame_count}");
    Console.WriteLine($"  global palette: {(info.global_palette ? "yes" : "no")}");
    Console.WriteLine("  background:");
    Console.WriteLine($"    red: 0x{bg[0]:X2}");
    Console.WriteLine($"    green: 0x{bg[1]:X2}");
    Console.WriteLine($"    blue: 0x{bg[2]:X2}");
    Console.WriteLine($"  frames:");
}

// print_gif_frame_info.c: Print GIF frame info.

public static void print_gif_frame_info(nsgif_frame_info_t* info, uint32_t i) {
    string disposal = nsgif_str_disposal(info.disposal);

    Console.WriteLine($"  - frame: {i}");
    Console.WriteLine($"    local palette: {(info.local_palette ? "yes" : "no")}");
    Console.WriteLine($"    disposal-method: {disposal}");
    Console.WriteLine($"    transparency: {(info.transparency ? "yes" : "no")}");
    Console.WriteLine($"    interlaced: {(info.interlaced ? "yes" : "no")}");
    Console.WriteLine($"    display: {(info.display ? "yes" : "no")}");
    Console.WriteLine($"    delay: {info.delay}");
    Console.WriteLine($"    rect:");
    Console.WriteLine($"      x: {info.rect.x0}");
    Console.WriteLine($"      y: {info.rect.y0}");
    Console.WriteLine($"      w: {info.rect.x1 - info.rect.x0}");
    Console.WriteLine($"      h: {info.rect.y1 - info.rect.y0}");
}

// save_palette.c: Save palette.

public static bool save_palette(string img_filename, string palette_filename, uint32_t[] palette, int used_entries) {
    using (StreamWriter sw = new StreamWriter(palette_filename)) {
        sw.WriteLine("P3");
        sw.WriteLine($"# {img_filename}: {palette_filename}");
        sw.WriteLine($"# Colour count: {used_entries}");
        sw.WriteLine($"{16 * 32} {16 * 32} 256");

        for (int y = 0; y < 16; y++) {
            for (int x = 0; x < 16; x++) {
                if (x % 16 == 0 || y % 16 == 0) {
                    sw.Write("0 0 0 ");
                } else {
                    int offset = (y / 16 * 16 + x / 16) * used_entries;
                    uint8_t[] entry = new uint8_t[3];
                    Array.Copy(palette, offset, entry, 0, 3);
                    sw.Write($"{entry[0]} {entry[1]} {entry[2]} ");
                }
            }

            sw.WriteLine();
        }
    }

    return true;
}

// save_global_palette.c: Save global palette.

public static bool save_global_palette(nsgif_t* gif) {
    uint32_t[] table = new uint32_t[NSGIF_MAX_COLOURS];
    int entries;

    nsgif_global_palette(gif, table, ref entries);

    return save_palette(nsgif_options.file, "global-palette.ppm", table, entries);
}

// save_local_palette.c: Save local palette.

public static bool save_local_palette(nsgif_t* gif, uint32_t frame) {
    uint32_t[] table = new uint32_t[NSGIF_MAX_COLOURS];
    string filename = $"local-palette-{frame}.ppm";
    int entries;

    nsgif_local_palette(gif, frame, table, ref entries);

    return save_palette(nsgif_options.file, filename, table, entries);
}

// decode.c: Decode GIF.

public static void decode(Stream ppm, string name, nsgif_t* gif, bool first) {
    nsgif_error err;
    uint32_t frame_prev = 0;
    nsgif_info_t* info;

    info = nsgif_get_info(gif);

    if (first && ppm != null) {
        using (StreamWriter sw = new StreamWriter(ppm)) {
            sw.WriteLine("P3");
            sw.WriteLine($"# {name}");
            sw.WriteLine($"# width                {info.width} ");
            sw.WriteLine($"# height               {info.height} ");
            sw.WriteLine($"# frame_count          {info.frame_count} ");
            sw.WriteLine($"# loop_max             {info.loop_max} ");
            sw.WriteLine($"{info.width} {info.height * info.frame_count}");
        }
    }

    if (first && nsgif_options.info) {
        print_gif_info(info);
    }
    if (first && nsgif_options.palette && info.global_palette) {
        save_global_palette(gif);
    }

    while (true) {
        nsgif_bitmap_t* bitmap;
        uint8_t[] image;
        uint32_t frame_new;
        uint32_t delay_cs;
        nsgif_rect_t area;

        err = nsgif_frame_prepare(gif, ref area,
                ref delay_cs, ref frame_new);
        if (err != NSGIF_OK) {
            warning("nsgif_frame_prepare", err);
            return;
        }

        if (frame_new < frame_prev) {
            /* Must be an animation that loops. We only care about
             * decoding each frame once in this utility. */
            return;
        }
        frame_prev = frame_new;

        if (first && nsgif_options.info) {
            nsgif_frame_info_t* f_info;

            f_info = nsgif_get_frame_info(gif, frame_new);
            if (f_info != null) {
                print_gif_frame_info(f_info, frame_new);
            }
        }
        if (first && nsgif_options.palette) {
            save_local_palette(gif, frame_new);
        }

        err = nsgif_frame_decode(gif, frame_new, ref bitmap);
        if (err != NSGIF_OK) {
            Console.WriteLine($"Frame {frame_new}: "
                    $"nsgif_decode_frame failed: {nsgif_strerror(err)}");
            /* Continue decoding the rest of the frames. */

        } else if (first && ppm != null) {
            using (StreamWriter sw = new StreamWriter(ppm)) {
                sw.WriteLine($"# frame {frame_new}:");
                image = new uint8_t[bitmap.Length];
                Array.Copy((uint8_t[])bitmap, 0, image, 0, bitmap.Length);
                for (int y = 0; y < info.height; y++) {
                    for (int x = 0; x < info.width; x++) {
                        int z = (y * info.width + x) * 4;
                        sw.Write($"{image[z]} {image[z + 1]} {image[z + 2]} ");
                    }
                    sw.WriteLine();
                }
            }
        }

        if (delay_cs == NSGIF_INFINITE) {
            /** This frame is the last. */
            return;
        }
    }
}

// main.c: Main function.

public static int Main(string[] args) {
    nsgif_options_v v = new nsgif_options_v();
    cli_table table = new cli_table();

    // Override default options with any command line args
    if (!cli_parse(ref table, args)) {
        cli_help(ref table, args[0]);
        return 1;
    }

    if (nsgif_options.help) {
        cli_help(ref table, args[0]);
        return 0;
    }

    if (nsgif_options.version) {
        Console.WriteLine($"{NSGIF_NAME} {NSGIF_VERSION}");
        return 0;
    }

    if (nsgif_options.ppm != null) {
        using (StreamWriter sw = new StreamWriter(nsgif_options.ppm)) {
            // ...
        }
    }

    // create our gif animation
    nsgif_error err;
    nsgif_t* gif;

    err = nsgif_create(ref v, NSGIF_BITMAP_FMT_R8G8B8A8, ref gif);
    if (err != NSGIF_OK) {
        warning("nsgif_create", err);
        return 1;
    }

    // load file into memory
    byte[] data = load_file(nsgif_options.file, out int size);

    // Scan the raw data
    err = nsgif_data_scan(gif, size, data);
    if (err != NSGIF_OK) {
        /* Not fatal; some GIFs are nasty. Can still try to decode
         * any frames that were decoded successfully. */
        warning("nsgif_data_scan", err);
    }

    nsgif_data_complete(gif);

    if (nsgif_options.loops == 0) {
        nsgif_options.loops = 1;
    }

    for (ulong i = 0; i < nsgif_options.loops; i++) {
        decode(null, nsgif_options.file, gif, i == 0);

        /* We want to ignore any loop limit in the GIF. */
        nsgif_reset(gif);
    }

    // clean up
    nsgif_destroy(gif);
    data = null;

    return 0;
}
```