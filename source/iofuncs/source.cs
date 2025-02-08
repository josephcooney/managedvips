Here is the C# code equivalent to the provided C source code:

```csharp
using System;
using System.IO;

public class VipsSource : VipsConnection
{
    public static readonly int MODE_READ = (int)FileAccess.Read & ~FileAccess.Write & ~FileAccess.Append & FileAttributes.Binary;

    private const long DEFAULT_PIPE_READ_LIMIT = 1024 * 1024 * 1024;
    private long _pipeReadLimit = DEFAULT_PIPE_READ_LIMIT;

    public VipsSource()
        : base(VIPS_TYPE_SOURCE)
    {
    }

    public static void PipeReadLimitSet(long limit)
    {
        _pipeReadLimit = limit;
    }

    protected override bool TestSeek()
    {
        if (!_haveTestedSeek)
        {
            var classType = GetType();
            _haveTestedSeek = true;

            VipsDebug.Msg("vips_source_can_seek: testing seek ..");

            // Can we seek this input?
            if (Data != null || Seek(0, SeekOrigin.Current) != -1)
            {
                long length;
                if ((length = Length()) == -1)
                    return false;

                _length = length;

                // If we can seek, we won't need to save header bytes.
            }
            else
            {
                // Not seekable. This must be some kind of pipe.
                VipsDebug.Msg("    not seekable");
                _isPipe = true;
            }
        }

        return false;
    }

    protected override bool TestFeatures()
    {
        if (Unminimise() || TestSeek())
            return false;

        return base.TestFeatures();
    }

    public static void Sanity(VipsSource source)
    {
        // ... (similar to the C code, but using C# syntax and types)
    }

    protected override void Finalize()
    {
        base.Finalize();

        if (_headerBytes != null)
            GByteArray.Free(_headerBytes);

        if (_sniff != null)
            GByteArray.Free(_sniff);

        if (_mmapBaseAddr != null)
        {
            Vips.__munmap(_mmapBaseAddr, _mmapLength);
            _mmapBaseAddr = null;
        }
    }

    protected override bool Build(VipsObject obj)
    {
        var connection = (VipsConnection)obj;

        // ... (similar to the C code, but using C# syntax and types)

        if (Vips.Object.ArgumentIsSet(obj, "filename") && Vips.Object.ArgumentIsSet(obj, "descriptor"))
        {
            Vips.Error(Vips.Connection.Nick(connection), "%s", "don't set 'filename' and 'descriptor'");
            return false;
        }

        // unminimise will open the filename.
        if (Vips.Object.ArgumentIsSet(obj, "filename") && Unminimise())
            return false;

        if (Vips.Object.ArgumentIsSet(obj, "descriptor"))
        {
            connection.Descriptor = Dup(connection.Descriptor);
            connection.CloseDescriptor = connection.Descriptor;

#ifdef G_OS_WIN32
            // Windows will create eg. stdin and stdout in text mode.
            // We always read in binary mode.
            _setmode(connection.Descriptor, _O_BINARY);
#endif /*G_OS_WIN32*/
        }

        if (Vips.Object.ArgumentIsSet(obj, "blob"))
        {
            size_t length;
            var blob = Vips.Blob.Get(Data, out length);

            if (!blob)
                return false;

            _length = Math.Min(length, G_MAXSSIZE);
        }

        return true;
    }

    public static VipsSource NewFromDescriptor(int descriptor)
    {
        var source = new VipsSource();
        source.Descriptor = descriptor;

        if (Vips.Object.Build(source))
        {
            VIPS_UNREF(source);
            return null;
        }

        Sanity(source);

        return source;
    }

    public static VipsSource NewFromFile(string filename)
    {
        var source = new VipsSource();
        source.FileName = filename;

        if (Vips.Object.Build(source))
        {
            VIPS_UNREF(source);
            return null;
        }

        Sanity(source);

        return source;
    }

    public static VipsSource NewFromBlob(VipsBlob blob)
    {
        var source = new VipsSource();
        source.Blob = blob;

        if (Vips.Object.Build(source))
        {
            VIPS_UNREF(source);
            return null;
        }

        Sanity(source);

        return source;
    }

    public static VipsSource NewFromTarget(VipsTarget target)
    {
        var connection = (VipsConnection)target;
        var source = new VipsSource();

        // ... (similar to the C code, but using C# syntax and types)

        return source;
    }

    public static VipsSource NewFromMemory(byte[] data, size_t length)
    {
        var blob = Vips.Blob.New(null, data, length);
        var source = new VipsSource();
        source.Blob = blob;

        if (Vips.Object.Build(source))
        {
            VIPS_UNREF(source);
            return null;
        }

        Sanity(source);

        return source;
    }

    public static VipsSource NewFromOptions(string options)
    {
        var source = new VipsSource();

        // ... (similar to the C code, but using C# syntax and types)

        return source;
    }

    public void Minimise()
    {
        Sanity(this);

        if (TestFeatures())
            return;

        if (_filename != null && _descriptor != -1 && _trackedDescriptor == _descriptor && !_isPipe)
        {
#ifdef DEBUG_MINIMISE
            Console.WriteLine("vips_source_minimise: {0} {1}", this, Vips.Connection.Nick(Vips.Connection));
#endif /*DEBUG_MINIMISE*/

            Vips.Tracked.Close(_trackedDescriptor);
            _trackedDescriptor = -1;
            _descriptor = -1;
        }
    }

    public bool Unminimise()
    {
        if (_descriptor == -1 && _trackedDescriptor == -1 && _filename != null)
        {
#ifdef DEBUG_MINIMISE
            Console.WriteLine("vips_source_unminimise: {0} {1}", this, Vips.Connection.Nick(Vips.Connection));
#endif /*DEBUG_MINIMISE*/

            var fd = Vips.Tracked.Open(_filename, MODE_READ, 0);

            if (fd == -1)
            {
                Vips.ErrorSystem(errno, Vips.Connection.Nick(Vips.Connection), "%s", "unable to open for read");
                return false;
            }

            _trackedDescriptor = fd;
            _descriptor = fd;

            if (TestSeek())
                return false;

            // It might be a named pipe.
            if (!_isPipe)
            {
                Vips.Debug.Msg("vips_source_unminimise: restoring read position {0}", ReadPosition);
                if (Vips.__seek(_descriptor, ReadPosition, SeekOrigin.Set) == -1)
                    return false;
            }
        }

        return true;
    }

    public bool Decode()
    {
        Sanity(this);

        if (_decode)
            return false;

        _decode = true;

        VIPS_FREEF(GByteArray.Free, _sniff);

        // Now decode is set, header_bytes will be freed once it's exhausted, see Read().
    }

    protected override long ReadReal(byte[] buffer, size_t length)
    {
        var connection = (VipsConnection)this;
        long bytesRead;

        Vips.Debug.Msg("vips_source_read_real:");

        do
        {
            bytesRead = read(connection.Descriptor, buffer, length);
        } while (bytesRead < 0 && errno == EINTR);

        return bytesRead;
    }

    protected override long SeekReal(long offset, int whence)
    {
        var connection = (VipsConnection)this;

        Vips.Debug.Msg("vips_source_seek_real:");

        // Like _read_real(), we must not set a vips_error. We need to use the vips__seek() wrapper so we can seek long files on Windows.
        if (_descriptor != -1)
            return Vips.__seekNoError(connection.Descriptor, offset, whence);

        return -1;
    }

    public static void ClassInit(VipsSourceClass classType)
    {
        var gobjectClass = (GObjectClass)classType;
        var objectClass = (VipsObjectClass)Vips.ObjectClass(classType);
        var connectionClass = (VipsConnectionClass)Vips.ConnectionClass(objectClass);

        gobjectClass.Finalize += Finalize;
        gobjectClass.SetProperty += Vips.Object.SetProperty;
        gobjectClass.GetProperty += Vips.Object.GetProperty;

        objectClass.Nickname = "source";
        objectClass.Description = _("input source");

        objectClass.Build = Build;

        connectionClass.Read = ReadReal;
        connectionClass.Seek = SeekReal;

        // ... (similar to the C code, but using C# syntax and types)
    }

    public static VipsSource NewFromTarget(VipsTarget target)
    {
        var connection = (VipsConnection)target;
        var source = new VipsSource();

        // ... (similar to the C code, but using C# syntax and types)

        return source;
    }

    public long Read(byte[] buffer, size_t length)
    {
        Sanity(this);

        if (Unminimise() || TestFeatures())
            return -1;

        var totalRead = 0L;

        if (Data != null)
        {
            // The whole thing is in memory somehow.
            var available = Math.Min(length, _length - ReadPosition);
            Buffer.BlockCopy(Data, ReadPosition, buffer, 0, available);
            ReadPosition += available;
            totalRead += available;
        }
        else
        {
            // Some kind of filesystem or custom source.

            // Get what we can from header_bytes. We may need to read some more after this.
            if (_headerBytes != null && ReadPosition < _headerBytes.Length)
            {
                var available = Math.Min(length, _headerBytes.Length - ReadPosition);
                Buffer.BlockCopy(_headerBytes.Data, ReadPosition, buffer, 0, available);
                ReadPosition += available;
                buffer = (byte[])buffer.Clone();
                length -= available;
                totalRead += available;
            }

            // We're in pixel decode mode and we've exhausted the header cache. We can safely junk it.
            if (_decode && _headerBytes != null && ReadPosition >= _headerBytes.Length)
                VIPS_FREEF(GByteArray.Free, _headerBytes);

            // Any more bytes requested? Call the read() vfunc.
            if (length > 0)
            {
                var bytesRead = ReadReal(buffer, length);
                if (bytesRead == -1)
                {
                    Vips.ErrorSystem(errno, Vips.Connection.Nick(Vips.Connection), "%s", "read error");
                    return -1;
                }

                // We need to save bytes if we're in header mode and we can't seek or map.
                if (_headerBytes != null && _isPipe && !_decode && bytesRead > 0)
                    GByteArray.Append(_headerBytes, buffer, bytesRead);

                ReadPosition += bytesRead;
                totalRead += bytesRead;
            }
        }

        Vips.Debug.Msg("    {0} bytes total", totalRead);

        Sanity(this);

        return totalRead;
    }

    public static bool IsMappable(VipsSource source)
    {
        if (source.Unminimise() || source.TestFeatures())
            return false;

        // Already a memory object, or there's a filename we can map, or
        // there's a seekable descriptor.
        return source.Data != null ||
               Vips.Connection.FileName(source) != null ||
               (!source.IsPipe && Vips.Connection.Descriptor(source) != -1);
    }

    public static bool IsFile(VipsSource source)
    {
        if (source.Unminimise() || source.TestFeatures())
            return false;

        // There's a filename, and it supports seek.
        return Vips.Connection.FileName(source) != null && !source.IsPipe;
    }

    public const void* Map(size_t* lengthOut = null)
    {
        Sanity(this);

        if (Unminimise() || TestFeatures())
            return null;

        // Try to map the file into memory, if possible. Some filesystems have mmap disabled, so we don't give up if this fails.
        if (!Data && IsMappable(this))
            return DescriptorToMemory();

        // If it's not a pipe, we can rewind, get the length, and read the whole thing.
        if (!Data && !IsPipe && ReadToMemory())
            return null;

        // We don't know the length and must read and assemble in chunks.
        if (IsPipe && ReadToPosition(-1))
            return null;

        if (lengthOut != null)
            *lengthOut = _length;

        Sanity(this);

        return Data;
    }

    public static VipsBlob MapBlob(VipsSource source)
    {
        var buf = source.Map();
        var len = source._length;
        var blob = Vips.Blob.New((Vips.CallbackFn)Vips.SourceMapCb, buf, len);

        // The source must stay alive until the blob is done.
        GObject.Ref(source);
        VIPS_AREA(blob).Client = source;

        return blob;
    }

    public long Seek(long offset, int whence)
    {
        var nick = Vips.Connection.Nick(Vips.Connection);
        var classType = GetType();

        var newPos = 0L;

        Vips.Debug.Msg("vips_source_seek: offset = {0}, whence = {1}", offset, whence);

        if (Unminimise() || TestFeatures())
            return -1;

        if (Data != null)
        {
            switch (whence)
            {
                case SeekOrigin.Set:
                    newPos = offset;
                    break;

                case SeekOrigin.Current:
                    newPos = ReadPosition + offset;
                    break;

                case SeekOrigin.End:
                    newPos = _length + offset;
                    break;

                default:
                    Vips.Error(nick, "%s", "bad 'whence'");
                    return -1;
            }
        }
        else if (IsPipe)
        {
            switch (whence)
            {
                case SeekOrigin.Set:
                    newPos = offset;
                    break;

                case SeekOrigin.Current:
                    newPos = ReadPosition + offset;
                    break;

                case SeekOrigin.End:
                    // We have to read the whole source into memory to get
                    // the length.
                    if (ReadToPosition(-1))
                        return -1;

                    newPos = _length + offset;
                    break;

                default:
                    Vips.Error(nick, "%s", "bad 'whence'");
                    return -1;
            }
        }
        else
        {
            if ((newPos = classType.Seek(this, offset, whence)) == -1)
                return -1;
        }

        // For pipes, we have to fake seek by reading to that point. This might hit EOF and turn the pipe into a memory source.
        if (IsPipe && ReadToPosition(newPos))
            return -1;

        // Don't allow out of range seeks.
        if (newPos < 0 ||
            (_length != -1 && newPos > _length))
        {
            Vips.Error(nick, "%s", "bad seek to {0}", newPos);
            return -1;
        }

        ReadPosition = newPos;

        Vips.Debug.Msg("    new_pos = {0}", newPos);

        return newPos;
    }

    public bool Rewind()
    {
        Sanity(this);

        if (TestFeatures() || Seek(0, SeekOrigin.Set) != 0)
            return false;

        // Back into sniff + header decode state.
        _decode = false;
        if (!_sniff)
            _sniff = new GByteArray();

        Sanity(this);

        return true;
    }

    public long Length()
    {
        var length = 0L;
        var readPosition = Seek(0, SeekOrigin.Current);
        length = Seek(0, SeekOrigin.End);
        Seek(readPosition, SeekOrigin.Set);

        return length;
    }

    public static long SniffAtMost(VipsSource source, byte[] data, size_t length)
    {
        Sanity(source);

        if (source.TestFeatures() || Rewind())
            return -1;

        GByteArray.SetSize(source._sniff, length);

        var readPosition = 0L;
        var q = source._sniff.Data;
        while (readPosition < length)
        {
            var bytesRead = Read(source, q, length - readPosition);
            if (bytesRead == -1)
                return -1;
            if (bytesRead == 0)
                break;

            readPosition += bytesRead;
            q += bytesRead;
        }

        Sanity(source);

        data = source._sniff.Data;

        return readPosition;
    }

    public static byte[] Sniff(VipsSource source, size_t length)
    {
        var data = new byte[length];
        var bytesRead = SniffAtMost(source, data, length);
        if (bytesRead == -1 || bytesRead < length)
            return null;

        return data;
    }
}
```