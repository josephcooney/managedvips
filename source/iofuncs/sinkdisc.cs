Here is the converted C# code:

```csharp
// wbuffer_write.c

using System;
using System.Threading;

public class WriteBuffer : IDisposable
{
    public VipsRegion Region { get; set; }
    public VipsRect Area { get; set; }
    public Semaphore Go { get; set; }
    public Semaphore NWrite { get; set; }
    public Semaphore Done { get; set; }
    public Semaphore Finish { get; set; }
    public int WriteErrno { get; set; }
    public bool Running { get; set; }
    public bool Kill { get; set; }

    public WriteBuffer(VipsRegion region, VipsRect area)
    {
        Region = region;
        Area = area;
        Go = new Semaphore(0, "go");
        NWrite = new Semaphore(0, "nwrite");
        Done = new Semaphore(0, "done");
        Finish = new Semaphore(0, "finish");
        WriteErrno = 0;
        Running = false;
        Kill = false;

        // Is there a thread running this region? Kill it!
        if (Running)
        {
            Kill = true;
            Go.Release();
            Finish.WaitOne();

            VIPS_DEBUG_MSG("wbuffer_free:\n");

            Running = false;
        }
    }

    public void Dispose()
    {
        Region.Dispose();
        Go.Dispose();
        NWrite.Dispose();
        Done.Dispose();
        Finish.Dispose();
    }
}

public class Write
{
    public SinkBase SinkBase { get; set; }
    public VipsRegionWrite WriteFn { get; set; }
    public object A { get; set; }
    public WriteBuffer Buf { get; set; }
    public WriteBuffer BufBack { get; set; }

    public Write(SinkBase sinkBase, VipsRegionWrite writeFn, object a)
    {
        SinkBase = sinkBase;
        WriteFn = writeFn;
        A = a;
        Buf = new WriteBuffer(null, null);
        BufBack = new WriteBuffer(null, null);
    }
}

public class WriteThreadState : VipsThreadState
{
    public WriteBuffer Buf { get; set; }

    public WriteThreadState(VipsImage im, object a) : base(im)
    {
        Buf = null;
    }
}

// wbuffer_write_thread.c

public void WbufferWriteThread(object data, object userData)
{
    WriteBuffer wbuffer = (WriteBuffer)data;

    for (; ; )
    {
        // Wait to be told to write.
        Go.WaitOne();

        if (Kill)
            break;

        // Now block until the last worker finishes on this buffer.
        NWrite.WaitOne(0);

        WbufferWrite(wbuffer);

        // Signal write complete.
        Done.Release();
    }

    // We are exiting: tell the main thread.
    Finish.Release();
}

// wbuffer_write.c

public void WbufferWrite(WriteBuffer wbuffer)
{
    Write write = wbuffer.Write;

    VIPS_DEBUG_MSG("wbuffer_write: %d bytes from wbuffer %p\n",
        wbuffer.Region.Bpl * wbuffer.Area.Height, wbuffer);

    VIPS_GATE_START("wbuffer_write: work");

    wbuffer.WriteErrno = write.WriteFn(wbuffer.Region,
        ref wbuffer.Area, write.A);

    VIPS_GATE_STOP("wbuffer_write: work");
}

// wbuffer_new.c

public WriteBuffer WbufferNew(Write write)
{
    WriteBuffer wbuffer;

    if (!(wbuffer = new WriteBuffer(null, null)))
        return null;
    wbuffer.Write = write;
    wbuffer.Region = null;
    Go = new Semaphore(0, "go");
    NWrite = new Semaphore(0, "nwrite");
    Done = new Semaphore(0, "done");
    Finish = new Semaphore(0, "finish");
    WriteErrno = 0;
    Running = false;
    Kill = false;

    if (!(wbuffer.Region = VipsRegion.New(write.SinkBase.Im)))
    {
        wbuffer.Dispose();
        return null;
    }

    // The worker threads need to be able to move the buffers around.
    Vips__Region_No_Ownership(wbuffer.Region);

    // Make this last (picks up parts of wbuffer on startup).
    if (VipsThread.Execute("wbuffer", WbufferWriteThread, wbuffer))
    {
        wbuffer.Dispose();
        return null;
    }

    wbuffer.Running = true;

    return wbuffer;
}

// wbuffer_flush.c

public int WbufferFlush(Write write)
{
    VIPS_DEBUG_MSG("wbuffer_flush:\n");

    // Block until the other buffer has been written. We have to do this
    // before we can set this buffer writing or we'll lose output ordering.
    if (write.Buf.Area.Top > 0)
    {
        Done.WaitOne();

        if (WriteCheckError(write))
            return -1;
    }

    // Set the background writer going for this buffer.
    Go.Release();

    return 0;
}

// wbuffer_position.c

public int WbufferPosition(WriteBuffer wbuffer, int top, int height)
{
    VipsRect image, area;
    int result;

    image.Left = 0;
    image.Top = 0;
    image.Width = wbuffer.Write.SinkBase.Im.Xsize;
    image.Height = wbuffer.Write.SinkBase.Im.Ysize;

    area.Left = 0;
    area.Top = top;
    area.Width = wbuffer.Write.SinkBase.Im.Xsize;
    area.Height = height;

    VipsRect.IntersectRect(ref area, ref image, ref wbuffer.Area);

    // The workers take turns to move the buffers.
    Vips__Region_Take_Ownership(wbuffer.Region);

    result = VipsRegion.Buffer(wbuffer.Region, ref wbuffer.Area);

    Vips__Region_No_Ownership(wbuffer.Region);

    // This should be an exclusive buffer, hopefully.
    if (!result)
        G.Assert(!wbuffer.Region.Buffer.Done);

    return result;
}

// wbuffer_allocate_fn.c

public bool WbufferAllocateFn(VipsThreadState state, object a, ref bool stop)
{
    WriteThreadState wstate = (WriteThreadState)state;
    Write write = (Write)a;
    SinkBase sinkBase = (SinkBase)write;

    VIPS_DEBUG_MSG("wbuffer_allocate_fn:\n");

    // Is the state x/y OK? New line or maybe new buffer or maybe even all done.
    if (sinkBase.X >= write.Buf.Area.Width)
    {
        sinkBase.X = 0;
        sinkBase.Y += sinkBase.TileHeight;

        if (sinkBase.Y >= VipsRect.Bottom(ref write.Buf.Area))
        {
            VIPS_DEBUG_MSG("wbuffer_allocate_fn: "
                "finished top = %d, height = %d\n",
                write.Buf.Area.Top, write.Buf.Area.Height);

            // Block until the write of the previous buffer
            // is done, then set write of this buffer going.
            if (WbufferFlush(write))
            {
                stop = true;
                return false;
            }

            // End of image?
            if (sinkBase.Y >= sinkBase.Im.Ysize)
            {
                stop = true;
                return false;
            }

            VIPS_DEBUG_MSG("wbuffer_allocate_fn: "
                "starting top = %d, height = %d\n",
                sinkBase.Y, sinkBase.NLines);

            // Swap buffers.
            VipsSwap(ref write.Buf, ref write.BufBack);

            // Position buf at the new y.
            if (WbufferPosition(write.Buf,
                sinkBase.Y, sinkBase.NLines))
            {
                stop = true;
                return false;
            }

            // This will be the first tile of a new buffer ... mark this as a
            // good place to stall for a moment if we want to stress the
            // caching system. See threadpool.c.
            state.Stall = true;
        }
    }

    // x, y and buf are good: save params for thread.
    image.Left = 0;
    image.Top = 0;
    image.Width = sinkBase.Im.Xsize;
    image.Height = sinkBase.Im.Ysize;
    tile.Left = sinkBase.X;
    tile.Top = sinkBase.Y;
    tile.Width = sinkBase.TileWidth;
    tile.Height = sinkBase.TileHeight;
    VipsRect.IntersectRect(ref image, ref tile, ref state.Pos);

    // The thread needs to know which buffer it's writing to.
    wstate.Buf = write.Buf;

    VIPS_DEBUG_MSG("  thread %p allocated "
        "left = %d, top = %d, width = %d, height = %d\n",
        G.ThreadSelf(),
        tile.Left, tile.Top, tile.Width, tile.Height);

    // Add to the number of writers on the buffer.
    NWrite.Release(-1);

    // Move state on.
    sinkBase.X += sinkBase.TileWidth;

    // Add the number of pixels we've just allocated to progress.
    sinkBase.Processed += state.Pos.Width * state.Pos.Height;

    return true;
}

// wbuffer_work_fn.c

public int WbufferWorkFn(VipsThreadState state, object a)
{
    WriteThreadState wstate = (WriteThreadState)state;

    int result;

    VIPS_DEBUG_MSG("wbuffer_work_fn: thread %p, %d x %d\n",
        G.ThreadSelf(),
        state.Pos.Left, state.Pos.Top);

    result = VipsRegion.Prepare_To(state.Reg, wstate.Buf.Region,
        ref state.Pos, state.Pos.Left, state.Pos.Top);

    VIPS_DEBUG_MSG("wbuffer_work_fn: thread %p result = %d\n",
        G.ThreadSelf(), result);

    // Tell the bg write thread we've left.
    NWrite.Release(1);

    return result;
}

// vips_sink_disc.c

public int VipsSinkDisc(VipsImage im, VipsRegionWrite writeFn, object a)
{
    Write write;

    VipsImage.Preeval(im);

    write = new Write(null, null, writeFn, a);

    int result = 0;
    if (!write.Buf ||
        !write.BufBack ||
        WbufferPosition(write.Buf, 0, write.SinkBase.NLines) ||
        VipsThreadpool.Run(im,
            (state, a) => new WriteThreadState(state.Im),
            WbufferAllocateFn,
            WbufferWorkFn,
            VipsSinkBase.Progress,
            ref write))
    {
        result = -1;
    }

    // Just before allocate signalled stop, it set write.buf writing. We
    // need to wait for this write to finish.
    if (!result)
        Done.WaitOne();

    VipsImage.Posteval(im);

    // The final write might have failed, pick up any error code.
    result |= WriteCheckError(write);

    write.Dispose();

    VipsImage.Minimise_All(im);

    return result;
}
```