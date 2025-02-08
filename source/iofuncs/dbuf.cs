```csharp
// vips_dbuf_init
public void VipsDbufInit(ref VipsDbuf dbuf)
{
    dbuf.data = null;
    dbuf.allocatedSize = 0;
    dbuf.dataSize = 0;
    dbuf.writePoint = 0;
}

// vips_dbuf_minimum_size
public bool VipsDbufMinimumSize(ref VipsDbuf dbuf, long size)
{
    if (size > dbuf.allocatedSize)
    {
        const long newAllocatedSize = 3 * (16 + size) / 2;

        byte[] newData;
        if ((newData = (byte[])dbuf.data.Clone().Resize(newAllocatedSize)) == null)
        {
            throw new OutOfMemoryException();
        }

        dbuf.data = newData;
        dbuf.allocatedSize = newAllocatedSize;
    }
    return true;
}

// vips_dbuf_allocate
public bool VipsDbufAllocate(ref VipsDbuf dbuf, long size)
{
    return VipsDbufMinimumSize(ref dbuf, dbuf.writePoint + size);
}

// vips_dbuf_read
public long VipsDbufRead(VipsDbuf dbuf, byte[] data, long size)
{
    const long available = dbuf.dataSize - dbuf.writePoint;
    const long copied = Math.Min(size, available);

    Array.Copy(dbuf.data, dbuf.writePoint, data, 0, copied);
    dbuf.writePoint += copied;

    return copied;
}

// vips_dbuf_get_write
public byte[] VipsDbufGetWrite(VipsDbuf dbuf, out long size)
{
    byte[] write = new byte[dbuf.allocatedSize - dbuf.writePoint];
    Array.Clear(write, 0, write.Length);
    dbuf.writePoint = dbuf.allocatedSize;
    dbuf.dataSize = dbuf.allocatedSize;

    if (size != null) size = write.Length;

    return write;
}

// vips_dbuf_write
public bool VipsDbufWrite(VipsDbuf dbuf, byte[] data, long size)
{
    if (!VipsDbufAllocate(ref dbuf, size))
        return false;

    Array.Copy(data, 0, dbuf.data, dbuf.writePoint, size);
    dbuf.writePoint += size;
    dbuf.dataSize = Math.Max(dbuf.dataSize, dbuf.writePoint);

    return true;
}

// vips_dbuf_writef
public bool VipsDbufWritef(VipsDbuf dbuf, string fmt, params object[] args)
{
    var line = string.Format(fmt, args);
    if (VipsDbufWrite(ref dbuf, Encoding.UTF8.GetBytes(line), line.Length))
        return true;
    return false;
}

// vips_dbuf_write_amp
public bool VipsDbufWriteAmp(VipsDbuf dbuf, string str)
{
    foreach (var c in str)
    {
        if ((c < 32 && c != '\n' && c != '\t' && c != '\r') ||
            c == '<' || c == '>' || c == '&')
        {
            // You'd think we could output "&#x02%x;", but xml
            // 1.0 parsers barf on that. xml 1.1 allows this, but
            // there are almost no parsers.
            if (!VipsDbufWritef(ref dbuf, "&#x%04x;", (int)0x2400 + c))
                return false;
        }
        else if (c == '<')
        {
            if (!VipsDbufWrite(ref dbuf, new byte[] { '&' }, 1))
                return false;
        }
        else if (c == '>')
        {
            if (!VipsDbufWrite(ref dbuf, new byte[] { '&' }, 1))
                return false;
        }
        else if (c == '&')
        {
            if (!VipsDbufWrite(ref dbuf, new byte[] { '&' }, 1))
                return false;
        }
        else
        {
            if (!VipsDbufWrite(ref dbuf, new byte[] { c }, 1))
                return false;
        }
    }

    return true;
}

// vips_dbuf_reset
public void VipsDbufReset(ref VipsDbuf dbuf)
{
    dbuf.writePoint = 0;
    dbuf.dataSize = 0;
}

// vips_dbuf_destroy
public void VipsDbufDestroy(ref VipsDbuf dbuf)
{
    VipsDbufReset(ref dbuf);
    if (dbuf.data != null) dbuf.data = null;
    dbuf.allocatedSize = 0;
}

// vips_dbuf_seek
public bool VipsDbufSeek(VipsDbuf dbuf, long offset, int whence)
{
    long newWritePoint;

    switch (whence)
    {
        case SEEK_SET:
            newWritePoint = offset;
            break;
        case SEEK_END:
            newWritePoint = dbuf.dataSize + offset;
            break;
        case SEEK_CUR:
            newWritePoint = dbuf.writePoint + offset;
            break;
        default:
            g_assert(0);
            newWritePoint = dbuf.writePoint;
            break;
    }

    if (newWritePoint < 0)
    {
        throw new ArgumentException("negative seek");
    }

    // Possibly need to grow the buffer
    if (!VipsDbufMinimumSize(ref dbuf, newWritePoint))
        return false;

    dbuf.writePoint = newWritePoint;
    if (dbuf.dataSize < dbuf.writePoint)
    {
        Array.Clear(dbuf.data, (int)dbuf.dataSize, (int)(dbuf.writePoint - dbuf.dataSize));
        dbuf.dataSize = dbuf.writePoint;
    }

    return true;
}

// vips_dbuf_truncate
public void VipsDbufTruncate(VipsDbuf dbuf)
{
    dbuf.dataSize = dbuf.writePoint;
}

// vips_dbuf_tell
public long VipsDbufTell(VipsDbuf dbuf)
{
    return dbuf.writePoint;
}

// vips_dbuf_null_terminate
public bool VipsDbufNullTerminate(ref VipsDbuf dbuf)
{
    if (!VipsDbufMinimumSize(ref dbuf, dbuf.dataSize + 1))
        return false;

    dbuf.data[dbuf.dataSize] = 0;

    return true;
}

// vips_dbuf_steal
public byte[] VipsDbufSteal(VipsDbuf dbuf, out long size)
{
    var data = (byte[])dbuf.data.Clone();
    if (!VipsDbufNullTerminate(ref dbuf))
        throw new OutOfMemoryException();

    if (size != null) size = dbuf.dataSize;

    VipsDbufDestroy(ref dbuf);

    return data;
}

// vips_dbuf_string
public byte[] VipsDbufString(VipsDbuf dbuf, out long size)
{
    var data = (byte[])dbuf.data.Clone();
    if (!VipsDbufNullTerminate(ref dbuf))
        throw new OutOfMemoryException();

    if (size != null) size = dbuf.dataSize;

    return data;
}
```