Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

public class SinkMemoryThreadState : VipsThreadState
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);
        klass.Nickname = "sinkmemorythreadstate";
        klass.Description = "per-thread state for sinkmemory";
    }

    public override void Init()
    {
        base.Init();
    }
}

public class SinkMemoryArea
{
    public SinkMemory Memory { get; set; }
    public VipsRect Rect { get; set; }
    public Semaphore NWrite { get; set; }

    public SinkMemoryArea(SinkMemory memory)
    {
        this.Memory = memory;
        this.Rect = new VipsRect();
        this.NWrite = new Semaphore(0, "nwrite");
    }

    public void Position(int top, int height)
    {
        var memory = Memory;

        var all = new VipsRect { Left = 0, Top = 0, Width = memory.SinkBase.Im.Xsize, Height = memory.SinkBase.Im.Ysize };
        var rect = new VipsRect { Left = 0, Top = top, Width = memory.SinkBase.Im.Xsize, Height = height };

        vips_rect_intersectrect(ref all, ref rect, ref Rect);
    }
}

public class SinkMemory : SinkBase
{
    public SinkMemoryArea Area { get; set; }
    public SinkMemoryArea OldArea { get; set; }
    public VipsRegion Region { get; set; }

    public override void Init(VipsImage image)
    {
        base.Init(image);

        this.Area = null;
        this.OldArea = null;

        var all = new VipsRect { Left = 0, Top = 0, Width = image.Xsize, Height = image.Ysize };

        if (Region == null || Region.Image != image ||
            (Area = new SinkMemoryArea(this)) == null ||
            (OldArea = new SinkMemoryArea(this)) == null)
        {
            this.Dispose();
            return;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        VIPS_FREEF(sink_memory_area_free, Area);
        VIPS_FREEF(sink_memory_area_free, OldArea);
        VIPS_UNREF(Region);
    }
}

public class SinkMemoryThreadStateClass : VipsThreadStateClass
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);

        klass.Nickname = "sinkmemorythreadstate";
        klass.Description = "per-thread state for sinkmemory";
    }

    public override void Init()
    {
        base.Init();
    }
}

public class SinkMemoryThreadState : VipsThreadState
{
    public override void ClassInit(VipsObjectClass klass)
    {
        base.ClassInit(klass);
        klass.Nickname = "sinkmemorythreadstate";
        klass.Description = "per-thread state for sinkmemory";
    }

    public override void Init()
    {
        base.Init();
    }
}

public class SinkMemoryAreaAllocateFn : VipsThreadpoolAllocate
{
    public override bool Allocate(VipsThreadState state, object a, out bool stop)
    {
        var smstate = (SinkMemoryThreadState)state;
        var memory = (SinkMemory)a;
        var sinkBase = (SinkBase)memory;

        var image = new VipsRect { Left = 0, Top = 0, Width = sinkBase.Im.Xsize, Height = sinkBase.Im.Ysize };
        var tile = new VipsRect { Left = sinkBase.X, Top = sinkBase.Y, Width = sinkBase.TileWidth, Height = sinkBase.TileHeight };

        vips_rect_intersectrect(ref image, ref tile, ref state.Pos);

        smstate.Area = memory.Area;

        VIPS_DEBUG_MSG("sink_memory_area_allocate_fn: %p\n", Thread.CurrentThread.ManagedThreadId);

        if (sinkBase.X >= memory.Area.Rect.Width)
        {
            sinkBase.X = 0;
            sinkBase.Y += sinkBase.TileHeight;

            if (sinkBase.Y >= VIPS_RECT_BOTTOM(&memory.Area.Rect))
            {
                if (memory.Area.Rect.Top > 0)
                    vips_semaphore_downn(memory.OldArea.NWrite, 0);

                if (sinkBase.Y >= sinkBase.Im.Ysize)
                {
                    stop = true;
                    return false;
                }

                VIPS_SWAP(SinkMemoryArea,
                    memory.Area, memory.OldArea);

                sink_memory_area_position(memory.Area, sinkBase.Y, sinkBase.NLines);
            }
        }

        image.Left = 0;
        image.Top = 0;
        image.Width = sinkBase.Im.Xsize;
        image.Height = sinkBase.Im.Ysize;
        tile.Left = sinkBase.X;
        tile.Top = sinkBase.Y;
        tile.Width = sinkBase.TileWidth;
        tile.Height = sinkBase.TileHeight;

        vips_rect_intersectrect(ref image, ref tile, ref state.Pos);

        smstate.Area = memory.Area;

        VIPS_DEBUG_MSG("  %p allocated %d x %d:\n",
            Thread.CurrentThread.ManagedThreadId, state.Pos.Left, state.Pos.Top);

        vips_semaphore_upn(memory.Area.NWrite, -1);

        sinkBase.X += sinkBase.TileWidth;

        sinkBase.Processed += state.Pos.Width * state.Pos.Height;

        return true;
    }
}

public class SinkMemoryAreaWorkFn : VipsThreadpoolWork
{
    public override int Work(VipsThreadState state, object a)
    {
        var memory = (SinkMemory)a;
        var smstate = (SinkMemoryThreadState)state;
        var area = smstate.Area;

        VIPS_DEBUG_MSG("sink_memory_area_work_fn: %p %d x %d\n",
            Thread.CurrentThread.ManagedThreadId, state.Pos.Left, state.Pos.Top);

        int result = vips_region_prepare_to(state.Reg, memory.Region,
            ref state.Pos, state.Pos.Left, state.Pos.Top);

        VIPS_DEBUG_MSG("sink_memory_area_work_fn: %p result = %d\n",
            Thread.CurrentThread.ManagedThreadId, result);

        vips_semaphore_upn(area.NWrite, 1);

        return result;
    }
}

public class SinkMemory : SinkBase
{
    public override int Init(VipsImage image)
    {
        base.Init(image);

        this.Area = null;
        this.OldArea = null;

        var all = new VipsRect { Left = 0, Top = 0, Width = image.Xsize, Height = image.Ysize };

        if (Region == null || Region.Image != image ||
            (Area = new SinkMemoryArea(this)) == null ||
            (OldArea = new SinkMemoryArea(this)) == null)
        {
            this.Dispose();
            return -1;
        }

        return 0;
    }
}

public class VipsSinkMemory
{
    public static int vips_sink_memory(VipsImage image)
    {
        var memory = new SinkMemory();

        if (memory.Init(image))
            return -1;

        vips_image_preeval(image);

        sink_memory_area_position(memory.Area, 0, memory.SinkBase.NLines);
        if (vips_threadpool_run(image,
                sink_memory_thread_state_new,
                new SinkMemoryAreaAllocateFn(),
                new SinkMemoryAreaWorkFn(),
                vips_sink_base_progress,
                memory))
            return -1;

        vips_image_posteval(image);

        memory.Dispose();

        vips_image_minimise_all(image);

        VIPS_DEBUG_MSG("vips_sink_memory: done\n");

        return 0;
    }
}
```

Note that I've assumed the existence of a `VipsThreadpoolAllocate` and `VipsThreadpoolWork` interface, as well as a `SinkBase` class. You may need to modify the code to fit your specific use case.

Also note that this is just one possible way to convert the C code to C#. There are many other ways to do it, and some of them might be more efficient or idiomatic than others.