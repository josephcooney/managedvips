```csharp
// vips_sink_area_free: (method)
// @area: free this area
public static void SinkAreaFree(SinkArea area)
{
    VipsSemaphore.Destroy(area.NThread);
    G.Free(area);
}

// vips_sink_area_new: (method)
// @sink: new sink area for this sink
public static SinkArea SinkAreaNew(Sink sink)
{
    var area = new SinkArea();
    if (area == null) return null;
    area.Sink = sink;
    VipsSemaphore.Init(ref area.NThread, 0, "n_thread");
    return area;
}

// vips_sink_area_position: (method)
// @area: position this area
public static void SinkAreaPosition(SinkArea area, int top, int height)
{
    var sink = area.Sink;

    VipsRect all, rect;

    all.Left = 0;
    all.Top = 0;
    all.Width = sink.Im.Xsize;
    all.Height = sink.Im.Ysize;

    rect.Left = 0;
    rect.Top = top;
    rect.Width = sink.Im.Xsize;
    rect.Height = height;

    VipsRect.IntersectRect(ref all, ref rect, ref area.Rect);
}

// vips_sink_area_allocate_fn: (method)
// @state: allocate a new tile for this thread
public static bool SinkAreaAllocateFn(VipsThreadState state, void* a, bool* stop)
{
    var sstate = (SinkThreadState)state;
    var sink = (Sink)a;

    VipsRect image;
    VipsRect tile;

    // Is the state x/y OK? New line or maybe new buffer or maybe even all done.
    if (sink.X >= sink.Area.Rect.Width)
    {
        sink.X = 0;
        sink.Y += sink.TileHeight;

        if (sink.Y >= VipsRect.Bottom(sink.Area.Rect))
        {
            // Block until the previous area is done.
            if (sink.Area.Rect.Top > 0) VipsSemaphore.Downn(ref sink.OldArea.NThread, 0);

            // End of image?
            if (sink.Y >= sink.Im.Ysize)
            {
                *stop = true;
                return false;
            }

            // Swap buffers.
            VIPS_SWAP(SinkArea, ref sink.Area, ref sink.OldArea);

            // Position buf at the new y.
            SinkAreaPosition(sink.Area, sink.Y, sink.NLines);
        }
    }

    // x, y and buf are good: save params for thread.
    image.Left = 0;
    image.Top = 0;
    image.Width = sink.Im.Xsize;
    image.Height = sink.Im.Ysize;
    tile.Left = sink.X;
    tile.Top = sink.Y;
    tile.Width = sink.TileWidth;
    tile.Height = sink.TileHeight;
    VipsRect.IntersectRect(ref image, ref tile, ref state.Pos);

    // The thread needs to know which area it's writing to.
    sstate.Area = sink.Area;

    VIPS_DEBUG_MSG("  {0} allocated {1} x {2}:\n", G.ThreadSelf(), state.Pos.Left, state.Pos.Top);

    // Add to the number of writers on the area.
    VipsSemaphore.Upn(ref sink.Area.NThread, -1);

    // Move state on.
    sink.X += sink.TileWidth;

    // Add the number of pixels we've just allocated to progress.
    sink.Processed += state.Pos.Width * state.Pos.Height;

    return false;
}

// vips_sink_call_stop: (method)
// @sink: call stop function for this thread
public static int SinkCallStop(Sink sink, SinkThreadState state)
{
    if (state.Seq != null && sink.StopFn != null)
    {
        var result = sink.StopFn(state.Seq, sink.A, sink.B);

        if (result != 0)
        {
            var sinkBase = (SinkBase)sink;
            VipsError("vips_sink", _("stop function failed for image \"{0}\""), sinkBase.Im.Filename);
            return -1;
        }

        state.Seq = null;
    }

    return 0;
}

// vips_sink_thread_state_dispose: (method)
// @gobject: dispose of this thread
public static void SinkThreadStateDispose(GObject gobject)
{
    var state = (SinkThreadState)gobject;
    var sink = (Sink)((VipsThreadState)state).A;

    SinkCallStop(sink, state);
    VIPS_UNREF(state.Reg);

    G.ObjectClass.SinkThreadStateParentClass.Dispose(gobject);
}

// vips_sink_call_start: (method)
// @sink: call start function for this thread
public static int SinkCallStart(Sink sink, SinkThreadState state)
{
    if (state.Seq == null && sink.StartFn != null)
    {
        VIPS_DEBUG_MSG("sink_call_start: state = {0}\n", state);

        state.Seq = sink.StartFn(sink.T, sink.A, sink.B);

        if (state.Seq == null)
        {
            var sinkBase = (SinkBase)sink;
            VipsError("vips_sink", _("start function failed for image \"{0}\""), sinkBase.Im.Filename);
            return -1;
        }
    }

    return 0;
}

// vips_sink_thread_state_build: (method)
// @object: build this thread
public static int SinkThreadStateBuild(VipsObject object)
{
    var state = (SinkThreadState)object;
    var sink = (Sink)((VipsThreadState)state).A;

    if ((state.Reg = VipsRegion.New(sink.T)) == null || SinkCallStart(sink, state))
        return -1;

    return G.ObjectClass.SinkThreadStateParentClass.Build(object);
}

// vips_sink_thread_state_class_init: (method)
// @class: class init for sink thread state
public static void SinkThreadStateClassInit(SinkThreadStateClass class_)
{
    var gobjectClass = G.ObjectClass.GetClass(class_);
    var objectClass = VipsObjectClass.GetClass(class_);

    gobjectClass.Dispose += SinkThreadStateDispose;

    objectClass.Build += SinkThreadStateBuild;
    objectClass.Nickname = "sinkthreadstate";
    objectClass.Description = _("per-thread state for sink");
}

// vips_sink_thread_state_init: (method)
// @state: init this thread
public static void SinkThreadStateInit(SinkThreadState state)
{
    state.Seq = null;
    state.Reg = null;
}

// vips_sink_thread_state_new: (method)
// @im: new sink thread state for this image
public static VipsThreadState VipsSinkThreadStateNew(VipsImage im, void* a)
{
    return VIPS_THREAD_STATE(G.Object.New(SinkThreadState.GetType(), VipsObject.Set, im, a));
}

// vips_sink_free: (method)
// @sink: free this sink
public static void VipsSinkFree(Sink sink)
{
    G.FreeF(SinkAreaFree, sink.Area);
    G.FreeF(SinkAreaFree, sink.OldArea);
    G.Object.Unref(sink.T);
}

// vips_sink_base_init: (method)
// @sink_base: init this sink base
public static void VipsSinkBaseInit(SinkBase sinkBase, VipsImage image)
{
    // Always clear kill before we start looping. See the call to vips_image_iskilled() below.
    VipsImage.SetKill(image, false);

    sinkBase.Im = image;
    sinkBase.X = 0;
    sinkBase.Y = 0;

    VipsGetTileSize(image, out sinkBase.TileWidth, out sinkBase.TileHeight, out sinkBase.NLines);

    sinkBase.Processed = 0;
}

// vips_sink_init: (method)
// @sink: init this sink
public static int VipsSinkInit(Sink sink, VipsImage image, VipsStartFn startFn, VipsGenerateFn generateFn, VipsStopFn stopFn, void* a, void* b)
{
    G.Assert(generateFn);

    VipsSinkBaseInit((SinkBase)sink, image);

    sink.T = null;
    sink.StartFn = startFn;
    sink.GenerateFn = generateFn;
    sink.StopFn = stopFn;
    sink.A = a;
    sink.B = b;

    sink.Area = null;
    sink.OldArea = null;

    if ((sink.T = VipsImage.New()) == null || (sink.Area = SinkAreaNew(sink)) == null || (sink.OldArea = SinkAreaNew(sink)) == null || VipsImage.Write(image, sink.T))
        VipsSinkFree(sink);
    return -1;
}

// vips_sink_work: (method)
// @state: work on this thread
public static int VipsSinkWork(VipsThreadState state, void* a)
{
    var sstate = (SinkThreadState)state;
    var sink = (Sink)a;
    var area = sstate.Area;

    int result;

    result = VipsRegion.Prepare(sstate.Reg, ref state.Pos);
    if (!result)
        result = sink.GenerateFn(sstate.Reg, sstate.Seq, sink.A, sink.B, ref state.Stop);

    // Tell the allocator we're done.
    VipsSemaphore.Upn(ref area.NThread, 1);

    return result;
}

// vips_sink_base_progress: (method)
// @a: progress on this image
public static int VipsSinkBaseProgress(void* a)
{
    var sinkBase = (SinkBase)a;

    VIPS_DEBUG_MSG("vips_sink_base_progress:\n");

    // Trigger any eval callbacks on our source image and check for errors.
    VipsImage.Eval(sinkBase.Im, sinkBase.Processed);
    if (VipsImage.IsKilled(sinkBase.Im))
        return -1;

    return 0;
}

// vips_sink_tile: (method)
// @im: scan over this image
public static int VipsSinkTile(VipsImage im, int tileWidth, int tileHeight, VipsStartFn startFn, VipsGenerateFn generateFn, VipsStopFn stopFn, void* a, void* b)
{
    var sink = new Sink();
    int result;

    G.Assert(VipsObject.Sanity(VipsObject.Get(im)));

    // We don't use this, but make sure it's set in case any old binaries are expecting it.
    im.Bits = VipsFormat.Sizeof(im.Format) << 3;

    if (VipsSinkInit(sink, im, startFn, generateFn, stopFn, a, b))
        return -1;

    if (tileWidth > 0)
    {
        sink.SinkBase.TileWidth = tileWidth;
        sink.SinkBase.TileHeight = tileHeight;
    }

    // vips_sink_base_progress() signals progress on im, so we have to do pre/post on that too.
    VipsImage.Preeval(im);

    SinkAreaPosition(sink.Area, 0, sink.SinkBase.NLines);
    result = VipsThreadPool.Run(im, VipsSinkThreadStateNew, VipsSinkAreaAllocateFn, VipsSinkWork, VipsSinkBaseProgress, ref sink);

    VipsImage.Posteval(im);

    VipsSinkFree(sink);

    VipsImage.MinimiseAll(im);

    return result;
}

// vips_sink: (method)
// @im: scan over this image
public static int VipsSink(VipsImage im, VipsStartFn startFn, VipsGenerateFn generateFn, VipsStopFn stopFn, void* a, void* b)
{
    return VipsSinkTile(im, -1, -1, startFn, generateFn, stopFn, a, b);
}
```