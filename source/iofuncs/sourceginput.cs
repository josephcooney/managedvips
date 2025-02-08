```csharp
// vips_source_g_input_stream_build
public class VipsSourceGInputStream : VipsSource
{
    public override int Build()
    {
        var source = (VipsSource)this;
        var sourceGinput = (VipsSourceGInputStream)source;
        var error = new GError();

        Debug.WriteLine("vips_source_g_input_stream_build: " + source);

        if (base.Build() != 0)
            return -1;

        if (sourceGinput.Stream is FileInputStream stream)
        {
            // It's unclear if this will ever produce useful output.
            var info = stream.QueryInfo(GFileAttributeStandardName, null, out error);
            if (info == null)
            {
                VipsGError(error);
                return -1;
            }

            source.Info = info;

#ifdef DEBUG
            {
                var attributes = stream.ListAttributes();
                for (int i = 0; attributes[i] != null; i++)
                {
                    var name = attributes[i];
                    var value = stream.GetAttributeAsString(name);

                    Debug.WriteLine("stream attribute: " + name + " = " + value);
                    value.Dispose();
                }
                attributes.Dispose();
            }
#endif // DEBUG

            if (source.Info.Name != null)
                this["filename"] = source.Info.Name;
        }

        if (sourceGinput.Stream is Seekable streamSeekable && streamSeekable.CanSeek)
            sourceGinput.Seekable = streamSeekable;

        return 0;
    }
}

// vips_source_g_input_stream_read
public class VipsSourceGInputStream : VipsSource
{
    public override int Read(byte[] buffer, int length)
    {
        var source = (VipsSource)this;
        var sourceGinput = (VipsSourceGInputStream)source;

        Debug.WriteLine("vips_source_g_input_stream_read: " + length);

        // Do we need to loop on this call? The docs are unclear.
        var bytesRead = sourceGinput.Stream.Read(buffer, 0, length);
        if (bytesRead < 0)
        {
            VIPS_DEBUG_MSG("    %s\n", error.Message);
            VipsGError(error);
            return -1;
        }

        Debug.WriteLine("    (returned " + bytesRead + " bytes)");

        return bytesRead;
    }
}

// lseek_to_seek_type
public static GSeekType LseekToSeekType(int whence)
{
    switch (whence)
    {
        default:
        case SEEK_CUR:
            return GSeekType.Current;
        case SEEK_SET:
            return GSeekType.Set;
        case SEEK_END:
            return GSeekType.End;
    }
}

// vips_source_g_input_stream_seek
public class VipsSourceGInputStream : VipsSource
{
    public override long Seek(long offset, int whence)
    {
        var source = (VipsSource)this;
        var sourceGinput = (VipsSourceGInputStream)source;
        var type = LseekToSeekType(whence);
        var error = new GError();

        Debug.WriteLine("vips_source_g_input_stream_seek: offset = " + offset + ", whence = " + whence);

        if (sourceGinput.Seekable)
        {
            if (!sourceGinput.Seekable.Seek(offset, type, out _, ref error))
            {
                VipsGError(error);
                return -1;
            }

            var newPosition = sourceGinput.Seekable.Tell();
        }
        else
            newPosition = -1;

        Debug.WriteLine("  (new position = " + newPosition + ")");

        return newPosition;
    }
}

// vips_source_g_input_stream_class_init
public class VipsSourceGInputStreamClass : VipsSourceClass
{
    public override void ClassInit()
    {
        base.ClassInit();

        this["stream"] = new GObjectPropertyInfo("Stream", typeof(GInputStream), "The stream to read from");
    }
}

// vips_source_g_input_stream_new
public class VipsSourceGInputStream : VipsSource
{
    public static VipsSourceGInputStream New(GInputStream stream)
    {
        var source = new VipsSourceGInputStream();
        source["stream"] = stream;

        if (source.Build() != 0)
            return null;

        return source;
    }
}
```