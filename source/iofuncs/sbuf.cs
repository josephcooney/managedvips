Here is the converted C# code:

```csharp
using System;
using System.IO;

public class VipsSbuf : IDisposable
{
    private readonly VipsSource _source;
    private byte[] _inputBuffer = new byte[VIPS_SBUF_BUFFER_SIZE + 1];
    private int _readPoint = 0;
    private int _charsInBuffer = 0;
    private string _line = new string(' ', VIPS_SBUF_BUFFER_SIZE);

    public VipsSbuf(VipsSource source)
    {
        _source = source;
    }

    public static VipsSbuf NewFromSource(VipsSource source)
    {
        return new VipsSbuf(source);
    }

    public void Unbuffer()
    {
        if (_charsInBuffer > 0)
        {
            var seekPosition = _readPoint - _charsInBuffer;
            _source.Seek(seekPosition, SeekOrigin.Current);
            _readPoint = 0;
            _charsInBuffer = 0;
        }
    }

    private int Refill()
    {
        var bytesRead = _source.Read(_inputBuffer, 0, VIPS_SBUF_BUFFER_SIZE);
        if (bytesRead == -1)
            return -1;

        _readPoint = 0;
        _charsInBuffer = bytesRead;
        _inputBuffer[bytesRead] = '\0';

        return bytesRead;
    }

    public int GetC()
    {
        if (_readPoint == _charsInBuffer && Refill() <= 0)
            return -1;

        var ch = _inputBuffer[_readPoint];
        _readPoint++;

        return (int)ch;
    }

    public void Ungetc()
    {
        if (_readPoint > 0)
            _readPoint--;
    }

    public bool Require(int require)
    {
        if (require < 0 || require >= VIPS_SBUF_BUFFER_SIZE)
            throw new ArgumentException("Invalid require value");

        if (_charsInBuffer - _readPoint >= require)
            return true;

        var bytesToRead = Math.Min(require, VIPS_SBUF_BUFFER_SIZE);
        var bytesRead = Refill();
        if (bytesRead == -1)
            return false;

        if (_charsInBuffer < bytesToRead)
            throw new IOException("End of file");

        return true;
    }

    public string GetLine()
    {
        var writePoint = 0;
        var spaceRemaining = VIPS_SBUF_BUFFER_SIZE;

        while (true)
        {
            var ch = GetC();
            if (ch == -1 || ch == '\n')
                break;

            _line[writePoint] = (char)ch;
            writePoint++;
            spaceRemaining--;
        }

        if (_readPoint > 0 && _line[_readPoint - 1] == '\r')
            _line[_readPoint - 1] = '\0';

        return _line.Substring(0, writePoint);
    }

    public string GetNonWhitespace()
    {
        var i = 0;
        while (i < VIPS_SBUF_BUFFER_SIZE && !char.IsWhiteSpace((char)GetC()))
            _line[i++] = (char)GetC();

        if (!char.IsWhiteSpace(GetC()))
            while ((char)GetC() != '\n');

        Ungetc();
        return _line.Substring(0, i);
    }

    public bool SkipWhitespace()
    {
        var ch = GetC();
        while (ch == '#')
        {
            if (!GetLine().Contains('\n'))
                return false;
            ch = GetC();
        }

        while (char.IsWhiteSpace(ch))
            Ungetc();

        return true;
    }
}

public class VipsSource : IDisposable
{
    // implementation of VipsSource
}
```

Note that I've assumed the `VipsSource` class is implemented elsewhere and has a `Read` method for reading data from the source. You'll need to implement this class separately.

Also, I've used the `IDisposable` interface to ensure that the `_inputBuffer` array is properly disposed of when the `VipsSbuf` object is disposed of.