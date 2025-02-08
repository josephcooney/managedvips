Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.IO;

public class VipsTarget : IDisposable
{
    public const int BUFFER_SIZE = 1024 * 1024 * 1024; // 1 GB

    private bool _ended;
    private string _filename;
    private int _descriptor;
    private FileStream _fileStream;
    private MemoryStream _memoryStream;
    private byte[] _outputBuffer;
    private int _writePoint;
    private GString _memoryBuffer;

    public VipsTarget()
    {
        _outputBuffer = new byte[BUFFER_SIZE];
        _writePoint = 0;
    }

    ~VipsTarget()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_ended) return;

        if (_fileStream != null)
        {
            _fileStream.Close();
            _fileStream = null;
        }
        else if (_memoryBuffer != null)
        {
            var data = _memoryBuffer.ToString();
            _memoryBuffer.Dispose();
            _memoryBuffer = null;
            vips_blob_set(_blob, (VipsCallbackFn)vips_area_free_cb, data, data.Length);
        }

        _ended = true;
    }

    public int WriteToDescriptor(int descriptor)
    {
        if (_fileStream != null) return -1;

        _descriptor = descriptor;
        _fileStream = new FileStream(descriptor, FileMode.OpenOrCreate, FileAccess.Write);

        return 0;
    }

    public int WriteToFile(string filename)
    {
        if (_fileStream != null) return -1;

        _filename = filename;
        _fileStream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);

        return 0;
    }

    public int WriteToMemory()
    {
        if (_memoryBuffer != null) return -1;

        _memoryBuffer = new GString();
        _memoryStream = new MemoryStream();

        return 0;
    }

    public void Flush()
    {
        if (_fileStream != null)
        {
            _fileStream.Flush();
        }
        else if (_memoryBuffer != null && _writePoint > 0)
        {
            var data = new byte[_writePoint];
            Array.Copy(_outputBuffer, 0, data, 0, _writePoint);
            _memoryStream.Write(data, 0, data.Length);
            _writePoint = 0;
        }
    }

    public int Write(byte[] buffer, int length)
    {
        if (length > BUFFER_SIZE - _writePoint && Flush() < 0) return -1;

        if (length > BUFFER_SIZE - _writePoint)
        {
            // Still too large? Do an unbuffered write.
            var data = new byte[length];
            Array.Copy(buffer, 0, data, 0, length);
            var bytesWritten = WriteUnbuffered(data, 0, length);

            if (bytesWritten < 0) return -1;
        }
        else
        {
            Array.Copy(buffer, 0, _outputBuffer, _writePoint, length);
            _writePoint += length;
        }

        return 0;
    }

    public int WriteUnbuffered(byte[] buffer, int offset, int length)
    {
        var bytesWritten = 0;

        while (length > 0)
        {
            // write() uses int not size_t on windows, so we need to chunk
            // ... max 1gb, why not
            int chunkSize = VIPS_MIN(1024 * 1024 * 1024, length);
            var data = new byte[chunkSize];
            Array.Copy(buffer, offset, data, 0, chunkSize);

            bytesWritten += Write(data, 0, chunkSize);

            if (bytesWritten <= 0)
            {
                vips_error_system(errno,
                    vips_connection_nick(VIPS_CONNECTION(this)),
                    "%s", _("write error"));
                return -1;
            }

            length -= chunkSize;
            offset += chunkSize;
        }

        return bytesWritten;
    }

    public int Read(byte[] buffer, int length)
    {
        if (Flush() < 0) return -1;

        if (_memoryBuffer != null && _writePoint > 0)
        {
            var data = new byte[length];
            Array.Copy(_outputBuffer, 0, data, 0, length);
            _writePoint -= length;
            return length;
        }
        else
        {
            var bytesRead = _fileStream.Read(buffer, 0, length);

            if (bytesRead < 0)
            {
                vips_error_system(errno,
                    vips_connection_nick(VIPS_CONNECTION(this)),
                    "%s", _("read error"));
                return -1;
            }

            return bytesRead;
        }
    }

    public int Seek(long position, int whence)
    {
        if (Flush() < 0) return -1;

        if (_memoryBuffer != null)
        {
            switch (whence)
            {
                case SEEK_SET:
                    _writePoint = (int)position;
                    break;

                case SEEK_CUR:
                    _writePoint += (int)position;
                    break;

                case SEEK_END:
                    _writePoint = (int)(_memoryBuffer.Length + position);
                    break;

                default:
                    vips_error(vips_connection_nick(VIPS_CONNECTION(this)),
                        "%s", _("bad 'whence'"));
                    return -1;
            }
        }
        else
        {
            var newPosition = _fileStream.Position + position;

            if (newPosition > _fileStream.Length)
            {
                _fileStream.SetLength(newPosition);
            }

            _fileStream.Position = newPosition;
        }

        return 0;
    }

    public int End()
    {
        Dispose(true);

        return 0;
    }

    public byte[] Steal(out long length)
    {
        if (Flush() < 0) return null;

        if (_memoryBuffer != null)
        {
            var data = _memoryBuffer.ToString();
            _memoryBuffer.Dispose();
            _memoryBuffer = null;

            // We must have a valid byte array, or end will fail.
            _memoryBuffer = new GString();

            length = data.Length;
            return System.Text.Encoding.UTF8.GetBytes(data);
        }
        else
        {
            var data = new byte[_fileStream.Length];
            _fileStream.Position = 0;

            var bytesRead = _fileStream.Read(data, 0, (int)_fileStream.Length);

            if (bytesRead < 0)
            {
                vips_error_system(errno,
                    vips_connection_nick(VIPS_CONNECTION(this)),
                    "%s", _("read error"));
                return null;
            }

            length = bytesRead;

            return data;
        }
    }

    public string StealText()
    {
        var data = Steal(out _);

        if (data == null) return null;

        return System.Text.Encoding.UTF8.GetString(data);
    }

    public int Putc(int ch)
    {
        if (_writePoint >= BUFFER_SIZE && Flush() < 0) return -1;

        _outputBuffer[_writePoint++] = (byte)ch;

        return 0;
    }

    public int Writes(string str)
    {
        var data = System.Text.Encoding.UTF8.GetBytes(str);

        return Write(data, 0, data.Length);
    }

    public int Writef(string fmt, params object[] args)
    {
        var line = string.Format(fmt, args);

        return Writes(line);
    }

    public int WriteAmp(string str)
    {
        const char* p;

        for (p = str; *p; p++)
        {
            if (*p < 32 &&
                *p != '\n' &&
                *p != '\t' &&
                *p != '\r')
            {
                // You'd think we could output "&#x02%x;", but xml
                // 1.0 parsers barf on that. xml 1.1 allows this, but
                // there are almost no parsers.

                // U+2400 onwards are unicode glyphs for the ASCII
                // control characters, so we can use them -- thanks
                // electroly.
                if (Writef("&#x%04x;", 0x2400 + *p) < 0)
                    return -1;
            }
            else if (*p == '<')
            {
                if (Writes("&lt;") < 0)
                    return -1;
            }
            else if (*p == '>')
            {
                if (Writes("&gt;") < 0)
                    return -1;
            }
            else if (*p == '&')
            {
                if (Writes("&amp;") < 0)
                    return -1;
            }
            else
            {
                if (Putc(*p) < 0)
                    return -1;
            }
        }

        return 0;
    }
}
```

Note that I've assumed the existence of a `GString` class, which is not part of the standard .NET library. You may need to replace this with a suitable alternative, such as a `StringBuilder`. Additionally, I've used the `System.IO` namespace for file operations and the `System.Text.Encoding.UTF8.GetBytes()` method to convert strings to byte arrays.

Also note that some methods have been modified to match the C# syntax and conventions. For example, the `WriteUnbuffered` method now takes three parameters (offset and length) instead of two (length), and the `Steal` method returns a byte array instead of a string.

Please let me know if you need any further modifications or clarifications!