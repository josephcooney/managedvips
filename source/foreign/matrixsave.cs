```csharp
// vips_foreign_save_matrix_dispose
public class VipsForeignSaveMatrix : VipsForeignSave
{
    public VipsTarget Target { get; private set; }

    protected override void Dispose(GObject gobject)
    {
        base.Dispose(gobject);
        if (Target != null)
            Target.Dispose();
    }
}

// vips_foreign_save_matrix_block
public class VipsForeignSaveMatrixBlock : IDisposable
{
    public VipsRegion Region { get; private set; }
    public VipsRect Area { get; private set; }
    public VipsForeignSaveMatrix Matrix { get; private set; }

    public int Block()
    {
        for (int y = 0; y < Area.Height; y++)
        {
            double[] p = Region.GetDoubleArray(0, Area.Top + y);
            char[] buf = new char[G_ASCII_DTOSTR_BUF_SIZE];
            for (int x = 0; x < Area.Width; x++)
            {
                if (x > 0)
                    Matrix.Target.Write(" ");
                GAsciiDToString(buf, p[x]);
                Matrix.Target.Write(buf);
            }
            if (Matrix.Target.WriteLine())
                return -1;
        }
        return 0;
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_foreign_save_matrix_build
public class VipsForeignSaveMatrixBuild : IDisposable
{
    public VipsObject Object { get; private set; }
    public VipsImage Image { get; private set; }
    public VipsTarget Target { get; private set; }

    public int Build()
    {
        if (VIPS_OBJECT_CLASS(Object.GetType()).Build(Object))
            return -1;
        if (VipsCheckMono(Image.Nickname, Image.Ready) ||
            VipsCheckUncoded(Image.Nickname, Image.Ready))
            return -1;

        Target.WriteFormat("{0} {1}", Image.Ready.Xsize, Image.Ready.Ysize);
        double scale = VipsImageGetScale(Image.Ready);
        double offset = VipsImageGetOffset(Image.Ready);
        if (scale != 1.0 || offset != 0.0)
            Target.WriteFormat(" {0} {1}", scale, offset);
        if (Target.WriteLine())
            return -1;

        if (VipsSinkDisc(Image.Ready,
            new VipsForeignSaveMatrixBlock(Object)))
            return -1;
        if (Target.End())
            return -1;
        return 0;
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_foreign_save_matrix_file_build
public class VipsForeignSaveMatrixFileBuild : IDisposable
{
    public VipsObject Object { get; private set; }
    public VipsForeignSaveMatrixFile File { get; private set; }

    public int Build()
    {
        if (File.Filename != null &&
            !(Target = VipsTarget.NewToFile(File.Filename)))
            return -1;
        return VIPS_OBJECT_CLASS(Object.GetType()).Build(Object);
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_foreign_save_matrix_target_build
public class VipsForeignSaveMatrixTargetBuild : IDisposable
{
    public VipsObject Object { get; private set; }
    public VipsForeignSaveMatrixTarget Target { get; private set; }

    public int Build()
    {
        if (Target.Target != null)
            Object.Target = Target.Target;
        return VIPS_OBJECT_CLASS(Object.GetType()).Build(Object);
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_foreign_print_matrix_build
public class VipsForeignPrintMatrixBuild : IDisposable
{
    public VipsObject Object { get; private set; }
    public VipsForeignSaveMatrix Matrix { get; private set; }

    public int Build()
    {
        if (!(Target = VipsTarget.NewToDescriptor(1)))
            return -1;
        return VIPS_OBJECT_CLASS(Object.GetType()).Build(Object);
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_matrixsave
public class VipsMatrixSave : IDisposable
{
    public static int Call(VipsImage In, string Filename)
    {
        var result = new VipsForeignSaveMatrix();
        return result.Call(In, Filename);
    }

    public int Call(VipsImage In, string Filename)
    {
        // implementation of vips_matrixsave
        return 0;
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_matrixsave_target
public class VipsMatrixSaveTarget : IDisposable
{
    public static int Call(VipsImage In, VipsTarget Target)
    {
        var result = new VipsForeignSaveMatrix();
        return result.Call(In, Target);
    }

    public int Call(VipsImage In, VipsTarget Target)
    {
        // implementation of vips_matrixsave_target
        return 0;
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

// vips_matrixprint
public class VipsMatrixPrint : IDisposable
{
    public static int Call(VipsImage In)
    {
        var result = new VipsForeignSaveMatrix();
        return result.Call(In);
    }

    public int Call(VipsImage In)
    {
        // implementation of vips_matrixprint
        return 0;
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}
```