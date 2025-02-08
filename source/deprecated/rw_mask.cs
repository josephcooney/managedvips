Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;

public class VipsMask
{
    public struct INTMASK
    {
        public int xsize, ysize;
        public double scale, offset;
        public int[] coeff;
        public string filename;
    }

    public struct DOUBLEMASK
    {
        public int xsize, ysize;
        public double scale, offset;
        public double[] coeff;
        public string filename;
    }

    // Free mask structure and any attached arrays.
    public static void im_free_imask(INTMASK inMask)
    {
        if (inMask.coeff != null) Array.Clear(inMask.coeff, 0, inMask.coeff.Length);
        if (inMask.filename != null) inMask.filename = null;
        inMask.coeff = null;
        inMask.filename = null;
    }

    public static void im_free_dmask(DOUBLEMASK inMask)
    {
        if (inMask.coeff != null) Array.Clear(inMask.coeff, 0, inMask.coeff.Length);
        if (inMask.filename != null) inMask.filename = null;
        inMask.coeff = null;
        inMask.filename = null;
    }

    // Create an empty imask.
    public static INTMASK im_create_imask(string filename, int xsize, int ysize)
    {
        INTMASK outMask;

        if (xsize <= 0 || ysize <= 0 || string.IsNullOrEmpty(filename))
        {
            Console.WriteLine("im_create_imask: bad arguments");
            return new INTMASK();
        }

        outMask.coeff = new int[xsize * ysize];
        outMask.filename = filename;
        outMask.scale = 1.0;
        outMask.offset = 0.0;
        outMask.xsize = xsize;
        outMask.ysize = ysize;

        return outMask;
    }

    // Create an imask and initialise it from the function parameter list.
    public static INTMASK im_create_imaskv(string filename, int xsize, int ysize, params int[] values)
    {
        var outMask = im_create_imask(filename, xsize, ysize);
        for (int i = 0; i < xsize * ysize; i++)
            outMask.coeff[i] = values[i];
        return outMask;
    }

    // Create an empty dmask.
    public static DOUBLEMASK im_create_dmask(string filename, int xsize, int ysize)
    {
        DOUBLEMASK outMask;

        if (xsize <= 0 || ysize <= 0 || string.IsNullOrEmpty(filename))
        {
            Console.WriteLine("im_create_dmask: bad arguments");
            return new DOUBLEMASK();
        }

        outMask.coeff = new double[xsize * ysize];
        outMask.filename = filename;
        outMask.scale = 1.0;
        outMask.offset = 0.0;
        outMask.xsize = xsize;
        outMask.ysize = ysize;

        return outMask;
    }

    // Create a dmask and initialise it from the function parameter list.
    public static DOUBLEMASK im_create_dmaskv(string filename, int xsize, int ysize, params double[] values)
    {
        var outMask = im_create_dmask(filename, xsize, ysize);
        for (int i = 0; i < xsize * ysize; i++)
            outMask.coeff[i] = values[i];
        return outMask;
    }

    // Read a line from a file.
    private static string get_line(FileStream fp)
    {
        var buffer = new char[32768];
        if (!fp.ReadLine(buffer, 0, buffer.Length))
        {
            Console.WriteLine("read_mask: unexpected EOF");
            return null;
        }
        return new string(buffer);
    }

    // Read the first line of a file.
    private static bool read_header(FileStream fp, out int xs, out int ys, out double scale, out double offset)
    {
        var buffer = get_line(fp);
        if (buffer == null) return false;

        var values = buffer.Split(new char[] { ' ', ',', '\t', ';', '"' }, StringSplitOptions.RemoveEmptyEntries);

        if (values.Length != 4 || !int.TryParse(values[0], out xs) ||
            !int.TryParse(values[1], out ys) || !double.TryParse(values[2], out scale) ||
            !double.TryParse(values[3], out offset))
        {
            Console.WriteLine("read_header: error reading matrix header");
            return false;
        }

        if (xs <= 0 || ys <= 0)
        {
            Console.WriteLine("read_header: bad arguments");
            return false;
        }
        if (scale == 0.0)
        {
            Console.WriteLine("read_header: scale should be non-zero");
            return false;
        }

        return true;
    }

    // Read a matrix from a file.
    public static DOUBLEMASK im_read_dmask(string filename)
    {
        using (var fp = new FileStream(filename, FileMode.Open))
        {
            int xs, ys;
            double sc, off;

            if (!read_header(fp, out xs, out ys, out sc, out off)) return null;

            var outMask = im_create_dmask(filename, xs, ys);
            outMask.scale = sc;
            outMask.offset = off;

            for (int i = 0; i < xs * ys; i++)
                outMask.coeff[i] = double.Parse(get_line(fp));

            return outMask;
        }
    }

    // Read an integer matrix from a file.
    public static INTMASK im_read_imask(string filename)
    {
        var dmask = im_read_dmask(filename);
        if (dmask == null) return new INTMASK();

        for (int i = 0; i < dmask.xsize * dmask.ysize; i++)
            if (Math.Ceiling(dmask.coeff[i]) != dmask.coeff[i])
            {
                Console.WriteLine("im_read_imask: coefficient at position (" + i % dmask.xsize + ", " + i / dmask.xsize + ") is not int");
                im_free_dmask(dmask);
                return new INTMASK();
            }

        var outMask = im_create_imask(filename, dmask.xsize, dmask.ysize);
        outMask.scale = dmask.scale;
        outMask.offset = dmask.offset;

        for (int i = 0; i < dmask.xsize * dmask.ysize; i++)
            outMask.coeff[i] = (int)dmask.coeff[i];

        im_free_dmask(dmask);

        return outMask;
    }

    // Scale the dmask to make an imask with a maximum value of 20.
    public static INTMASK im_scale_dmask(DOUBLEMASK inMask, string filename)
    {
        var size = inMask.xsize * inMask.ysize;

        if (im_check_dmask("im_scale_dmask", inMask) == false) return new INTMASK();

        var outMask = im_create_imask(filename, inMask.xsize, inMask.ysize);

        double maxval = inMask.coeff[0];
        for (int i = 1; i < size; i++)
            if (inMask.coeff[i] > maxval)
                maxval = inMask.coeff[i];

        for (int i = 0; i < size; i++)
            outMask.coeff[i] = (int)(inMask.coeff[i] * 20.0 / maxval);
        outMask.offset = inMask.offset;

        int isum = 0;
        double dsum = 0.0;
        for (int i = 0; i < size; i++)
        {
            isum += outMask.coeff[i];
            dsum += inMask.coeff[i];
        }

        if (dsum == inMask.scale)
            outMask.scale = isum;
        else if (dsum == 0.0)
            outMask.scale = 1.0;
        else
            outMask.scale = (int)(inMask.scale * isum / dsum);

        return outMask;
    }

    // Make an imask from the dmask, rounding to nearest.
    public static INTMASK im_dmask2imask(DOUBLEMASK inMask, string filename)
    {
        var size = inMask.xsize * inMask.ysize;

        if (im_check_dmask("im_dmask2imask", inMask) == false) return new INTMASK();

        var outMask = im_create_imask(filename, inMask.xsize, inMask.ysize);

        for (int i = 0; i < size; i++)
            outMask.coeff[i] = (int)Math.Round(inMask.coeff[i]);
        outMask.offset = Math.Round(inMask.offset);
        outMask.scale = Math.Round(inMask.scale);

        return outMask;
    }

    // Make a dmask from the imask.
    public static DOUBLEMASK im_imask2dmask(INTMASK inMask, string filename)
    {
        var size = inMask.xsize * inMask.ysize;

        if (im_check_imask("im_imask2dmask", inMask) == false) return new DOUBLEMASK();

        var outMask = im_create_dmask(filename, inMask.xsize, inMask.ysize);

        for (int i = 0; i < size; i++)
            outMask.coeff[i] = inMask.coeff[i];
        outMask.offset = inMask.offset;
        outMask.scale = inMask.scale;

        return outMask;
    }

    // Normalise the dmask.
    public static void im_norm_dmask(DOUBLEMASK mask)
    {
        var size = mask.xsize * mask.ysize;
        double scale = (mask.scale == 0) ? 0 : (1.0 / mask.scale);

        if (im_check_dmask("im_norm_dmask", mask) == false || (scale == 1 && mask.offset == 0))
            return;

        for (int i = 0; i < size; i++)
            mask.coeff[i] = mask.coeff[i] * scale + mask.offset;
        mask.scale = 1.0;
        mask.offset = 0.0;
    }

    // Duplicate an imask.
    public static INTMASK im_dup_imask(INTMASK inMask, string filename)
    {
        if (im_check_imask("im_dup_imask", inMask) == false) return new INTMASK();

        var outMask = im_create_imask(filename, inMask.xsize, inMask.ysize);
        outMask.offset = inMask.offset;
        outMask.scale = inMask.scale;

        for (int i = 0; i < inMask.xsize * inMask.ysize; i++)
            outMask.coeff[i] = inMask.coeff[i];

        return outMask;
    }

    // Duplicate a dmask.
    public static DOUBLEMASK im_dup_dmask(DOUBLEMASK inMask, string filename)
    {
        if (im_check_dmask("im_dup_dmask", inMask) == false) return new DOUBLEMASK();

        var outMask = im_create_dmask(filename, inMask.xsize, inMask.ysize);
        outMask.offset = inMask.offset;
        outMask.scale = inMask.scale;

        for (int i = 0; i < inMask.xsize * inMask.ysize; i++)
            outMask.coeff[i] = inMask.coeff[i];

        return outMask;
    }

    // Write to file.
    private static void write_line(FileStream fp, string fmt, params object[] values)
    {
        using (var writer = new StreamWriter(fp))
        {
            writer.Write(fmt, values);
        }
    }

    private static void write_double(FileStream fp, double d)
    {
        var buffer = g_ascii_dtostr(G_ASCII_DTOSTR_BUF_SIZE, d);
        using (var writer = new StreamWriter(fp))
        {
            writer.Write(buffer);
        }
    }

    // Write an imask to a file.
    public static int im_write_imask_name(INTMASK inMask, string filename)
    {
        using (var fp = new FileStream(filename, FileMode.Create))
        {
            write_line(fp, "{0} {1}", inMask.xsize, inMask.ysize);
            if (inMask.scale != 1 || inMask.offset != 0)
                write_line(fp, " {0} {1}", inMask.scale, inMask.offset);
            write_line(fp, "\n");

            for (int i = 0; i < inMask.xsize * inMask.ysize; i++)
                write_line(fp, "{0} ", inMask.coeff[i]);

            return 0;
        }
    }

    // Write an imask to a file.
    public static int im_write_imask(INTMASK inMask)
    {
        if (string.IsNullOrEmpty(inMask.filename))
        {
            Console.WriteLine("im_write_imask: filename not set");
            return -1;
        }

        return im_write_imask_name(inMask, inMask.filename);
    }

    // Write a dmask to a file.
    public static int im_write_dmask_name(DOUBLEMASK inMask, string filename)
    {
        using (var fp = new FileStream(filename, FileMode.Create))
        {
            write_line(fp, "{0} {1}", inMask.xsize, inMask.ysize);
            if (inMask.scale != 1 || inMask.offset != 0)
                write_double(fp, inMask.scale);
            write_line(fp, " ");
            write_double(fp, inMask.offset);
            write_line(fp, "\n");

            for (int i = 0; i < inMask.xsize * inMask.ysize; i++)
                write_double(fp, inMask.coeff[i]);
            write_line(fp, "\n");

            return 0;
        }
    }

    // Write a dmask to a file.
    public static int im_write_dmask(DOUBLEMASK inMask)
    {
        if (string.IsNullOrEmpty(inMask.filename))
        {
            Console.WriteLine("im_write_dmask: filename not set");
            return -1;
        }

        return im_write_dmask_name(inMask, inMask.filename);
    }

    // Copy an imask into a matrix.
    public static void im_copy_imask_matrix(INTMASK mask, int[,] matrix)
    {
        for (int y = 0; y < mask.ysize; y++)
            for (int x = 0; x < mask.xsize; x++)
                matrix[x, y] = mask.coeff[y * mask.xsize + x];
    }

    // Copy a matrix into an imask.
    public static void im_copy_matrix_imask(int[,] matrix, INTMASK mask)
    {
        for (int y = 0; y < mask.ysize; y++)
            for (int x = 0; x < mask.xsize; x++)
                mask.coeff[y * mask.xsize + x] = matrix[x, y];
    }

    // Copy a dmask into a matrix.
    public static void im_copy_dmask_matrix(DOUBLEMASK mask, double[,] matrix)
    {
        for (int y = 0; y < mask.ysize; y++)
            for (int x = 0; x < mask.xsize; x++)
                matrix[x, y] = mask.coeff[y * mask.xsize + x];
    }

    // Copy a matrix to a dmask.
    public static void im_copy_matrix_dmask(double[,] matrix, DOUBLEMASK mask)
    {
        for (int y = 0; y < mask.ysize; y++)
            for (int x = 0; x < mask.xsize; x++)
                mask.coeff[y * mask.xsize + x] = matrix[x, y];
    }

    // Print an imask to stdout.
    public static void im_print_imask(INTMASK inMask)
    {
        Console.WriteLine("{0}: {1} {2} {3} {4}", inMask.filename, inMask.xsize, inMask.ysize, inMask.scale, inMask.offset);

        for (int k = 0; k < inMask.xsize * inMask.ysize; k++)
            Console.Write(inMask.coeff[k] + "\t");

        Console.WriteLine();
    }

    // Print a dmask to stdout.
    public static void im_print_dmask(DOUBLEMASK inMask)
    {
        Console.WriteLine("{0}: {1} {2} {3} {4}", inMask.filename, inMask.xsize, inMask.ysize, inMask.scale, inMask.offset);

        for (int k = 0; k < inMask.xsize * inMask.ysize; k++)
            Console.Write(inMask.coeff[k] + "\t");

        Console.WriteLine();
    }

    // Make the mask local to an image.
    public static DOUBLEMASK im_local_dmask(VipsImage outImage, DOUBLEMASK mask)
    {
        if (im_check_dmask("im_local_dmask", mask) == false) return new DOUBLEMASK();

        if (outImage.AddCloseCallback((image) => { im_free_dmask(mask); }))
            im_free_dmask(mask);

        return mask;
    }

    // Make the mask local to an image.
    public static INTMASK im_local_imask(VipsImage outImage, INTMASK mask)
    {
        if (im_check_imask("im_local_imask", mask) == false) return new INTMASK();

        if (outImage.AddCloseCallback((image) => { im_free_imask(mask); }))
            im_free_imask(mask);

        return mask;
    }

    // Check the dmask.
    private static bool im_check_dmask(string funcName, DOUBLEMASK inMask)
    {
        for (int i = 0; i < inMask.xsize * inMask.ysize; i++)
            if (!double.IsFinite(inMask.coeff[i]))
            {
                Console.WriteLine("{0}: coefficient at position ({1}, {2}) is not finite", funcName, i % inMask.xsize, i / inMask.xsize);
                return false;
            }

        return true;
    }

    // Check the imask.
    private static bool im_check_imask(string funcName, INTMASK inMask)
    {
        for (int i = 0; i < inMask.xsize * inMask.ysize; i++)
            if (!double.IsFinite(inMask.coeff[i]))
            {
                Console.WriteLine("{0}: coefficient at position ({1}, {2}) is not finite", funcName, i % inMask.xsize, i / inMask.xsize);
                return false;
            }

        return true;
    }
}
```