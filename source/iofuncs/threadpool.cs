Here's the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

public class VipsThreadpool
{
    public static void Init()
    {
        // 3 is the useful minimum, and huge values can crash the machine.
        int maxThreads = Environment.GetEnvironmentVariable("VIPS_MAX_THREADS") != null ? 
            Math.Min(1024, int.Parse(Environment.GetEnvironmentVariable("VIPS_MAX_THREADS"))) : 0;

        if (Environment.GetEnvironmentVariable("VIPS_STALL") != null)
            Vips__stall = true;

        // max_threads > 0 will create a set of threads on startup. This is
        // necessary for wasm, but may break on systems that try to fork()
        // after init.
        vips_threadset = new VipsThreadset(maxThreads);
    }

    public static void Shutdown()
    {
        VIPS_FREEF(vips_threadset_free, vips_threadset);
    }

    [Obsolete("Use VipsImage instead")]
    public class VipsThreadState : VipsObject
    {
        private VipsImage im;
        private object a;

        public VipsThreadState(VipsImage image, object arg)
            : base()
        {
            im = image;
            a = arg;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // unreffing the worker state will trigger stop in the threadstate,
                // so we need to single-thread.
                lock (allocate_lock)
                {
                    VIPS_FREEF(g_object_unref, state);
                }
            }

            base.Dispose(disposing);
        }

        public override void Build()
        {
            if (!(state = vips_region_new(im)))
                return -1;

            return VIPS_OBJECT_CLASS(vips_thread_state_parent_class).Build(this);
        }

        private object state;
    }

    public static int ThreadExecute(string domain, Action<object> func, object data)
    {
        return vips_threadset_run(vips_threadset, domain, func, data);
    }

    [Obsolete("Use VipsImage instead")]
    public class VipsWorker
    {
        private VipsThreadpool pool;
        private VipsThreadState state;

        public VipsWorker(VipsThreadpool threadPool)
        {
            pool = threadPool;
            state = null;
        }
    }

    public static int WorkerAllocate(VipsWorker worker)
    {
        VipsThreadpool pool = worker.pool;

        g_assert(!pool.stop);

        if (!worker.state &&
            !(worker.state = pool.start(pool.im, pool.a)))
            return -1;

        if (pool.allocate(worker.state, pool.a, ref pool.stop))
            return -1;

        return 0;
    }

    public static void WorkerWorkUnit(VipsWorker worker)
    {
        VipsThreadpool pool = worker.pool;

        VIPS_GATE_START("vips_worker_work_unit: wait");

        lock (pool.allocate_lock)
        {
            // Has another worker signaled stop while we've been waiting?
            if (pool.stop)
            {
                worker.stop = true;
                return;
            }

            // Has a thread been asked to exit? Volunteer if yes.
            if (g_atomic_int_add(ref pool.exit, -1) > 0)
            {
                // A thread had been asked to exit, and we've grabbed the
                // flag.
                worker.stop = true;
                return;
            }
            else
            {
                // No one had been asked to exit and we've mistakenly taken
                // the exit count below zero. Put it back up again.
                g_atomic_int_inc(ref pool.exit);
            }

            if (WorkerAllocate(worker))
            {
                pool.error = true;
                worker.stop = true;
                return;
            }
        }

        // Have we just signalled stop?
        if (pool.stop)
        {
            worker.stop = true;
            return;
        }

        lock (pool.allocate_lock);

        if (worker.state.stall &&
            Vips__stall)
        {
            // Sleep for 0.5s. Handy for stressing the seq system. Stall
            // is set by allocate funcs in various places.
            Thread.Sleep(500000);
            worker.state.stall = false;
            Console.WriteLine("vips_worker_work_unit: stall done, releasing y = " + worker.state.y);
        }

        // Process a work unit.
        if (pool.work(worker.state, pool.a))
        {
            worker.stop = true;
            pool.error = true;
        }
    }

    public static void ThreadMainLoop(object data)
    {
        VipsWorker worker = (VipsWorker)data;

        g_assert(pool == worker.pool);

        VIPS_GATE_START("vips_thread_main_loop: thread");

        while (!pool.stop &&
            !worker.stop &&
            !pool.error)
        {
            VIPS_GATE_START("vips_worker_work_unit: u");
            WorkerWorkUnit(worker);
            VIPS_GATE_STOP("vips_worker_work_unit: u");
            vips_semaphore_up(ref pool.tick);
        }

        VIPS_GATE_STOP("vips_thread_main_loop: thread");

        // unreffing the worker state will trigger stop in the threadstate,
        // so we need to single-thread.
        lock (pool.allocate_lock)
        {
            VIPS_FREEF(g_object_unref, worker.state);

            g_mutex_unlock(pool.allocate_lock);
        }

        // We are done: tell the main thread.
        vips_semaphore_upn(ref pool.n_workers, 1);
    }

    public static int WorkerNew(VipsThreadpool pool)
    {
        VipsWorker worker;

        if (!(worker = new VipsWorker(pool)))
            return -1;
        worker.pool = pool;
        worker.state = null;

        // We can't build the state here, it has to be done by the worker
        // itself the first time that allocate runs so that any regions are
        // owned by the correct thread.

        if (VipsThreadExecute("worker", ThreadMainLoop, worker))
        {
            g_free(worker);
            return -1;
        }

        // One more worker in the pool.
        vips_semaphore_upn(ref pool.n_workers, -1);

        return 0;
    }

    public static void WorkerLock(GMutex mutex)
    {
        VipsWorker worker = (VipsWorker)g_private_get(&worker_key);

        if (worker)
            g_atomic_int_inc(ref worker.pool.n_waiting);
        lock (mutex)
        {
            if (worker)
                g_atomic_int_dec_and_test(ref worker.pool.n_waiting);
        }
    }

    public static void WorkerCondWait(GCond cond, GMutex mutex)
    {
        VipsWorker worker = (VipsWorker)g_private_get(&worker_key);

        if (worker)
            g_atomic_int_inc(ref worker.pool.n_waiting);
        lock (mutex)
        {
            g_cond_wait(cond, mutex);
            if (worker)
                g_atomic_int_dec_and_test(ref worker.pool.n_waiting);
        }
    }

    public static void ThreadpoolWait(VipsThreadpool pool)
    {
        // Wait for them all to exit.
        pool.stop = true;
        vips_semaphore_downn(ref pool.n_workers, 0);
    }

    public static void ThreadpoolFree(VipsThreadpool pool)
    {
        VIPS_DEBUG_MSG("vips_threadpool_free: \"" + pool.im.filename + "\" (" + pool + ")");

        ThreadpoolWait(pool);

        VIPS_FREEF(vips_g_mutex_free, pool.allocate_lock);
        vips_semaphore_destroy(ref pool.n_workers);
        vips_semaphore_destroy(ref pool.tick);
        VIPS_FREE(pool);
    }

    public static VipsThreadpool New(VipsImage image)
    {
        VipsThreadpool pool;
        int tileWidth;
        int tileHeight;
        long nTiles;
        int nLines;

        // Allocate and init new thread block.
        if (!(pool = new VipsThreadpool()))
            return null;
        pool.im = image;
        pool.allocate = null;
        pool.work = null;
        pool.allocate_lock = vips_g_mutex_new();
        pool.max_workers = VipsConcurrencyGet();
        vips_semaphore_init(ref pool.n_workers, 0, "n_workers");
        vips_semaphore_init(ref pool.tick, 0, "tick");
        pool.error = false;
        pool.stop = false;
        pool.exit = 0;

        // If this is a tiny image, we won't need all max_workers threads.
        // Guess how
        // many tiles we might need to cover the image and use that to limit
        // the number of threads we create.
        vips_get_tile_size(image, out tileWidth, out tileHeight, out nLines);
        nTiles = (1 + (long)image.Xsize / tileWidth) *
            (1 + (long)image.Ysize / tileHeight);
        nTiles = VIPS_CLIP(1, nTiles, 1024);
        pool.max_workers = Math.Min(pool.max_workers, nTiles);

        // VIPS_META_CONCURRENCY on the image can optionally override
        // concurrency.
        pool.max_workers = VipsImageGetConcurrency(image, pool.max_workers);

        VIPS_DEBUG_MSG("vips_threadpool_new: \"" + image.filename + "\" (" + pool + "), with " + pool.max_workers + " threads");

        return pool;
    }

    public static int ThreadpoolRun(VipsImage image,
        Action<VipsThreadState> start,
        Func<VipsThreadState, bool> allocate,
        Action<VipsThreadState> work,
        Action<object> progress,
        object a)
    {
        VipsThreadpool pool;
        int result;
        int nWaiting;
        int nWorking;

        if (!(pool = ThreadpoolNew(image)))
            return -1;

        pool.start = start;
        pool.allocate = allocate;
        pool.work = work;
        pool.a = a;

        // Start with half of the max number of threads, then let it drift up
        // and down with load.
        for (nWorking = 0; nWorking < 1 + pool.max_workers / 2; nWorking++)
            if (WorkerNew(pool))
            {
                ThreadpoolFree(pool);
                return -1;
            }

        while (true)
        {
            // Wait for a tick from a worker.
            vips_semaphore_down(ref pool.tick);

            VIPS_DEBUG_MSG("vips_threadpool_run: tick");

            if (pool.stop ||
                pool.error)
                break;

            if (progress != null &&
                progress(pool.a))
                pool.error = true;

            if (pool.stop ||
                pool.error)
                break;

            nWaiting = g_atomic_int_get(ref pool.n_waiting);
            VIPS_DEBUG_MSG("n_waiting = " + nWaiting);
            VIPS_DEBUG_MSG("n_working = " + nWorking);
            VIPS_DEBUG_MSG("exit = " + pool.exit);

            if (nWaiting > 3 &&
                nWorking > 1)
            {
                VIPS_DEBUG_MSG("shrinking thread pool");
                g_atomic_int_inc(ref pool.exit);
                nWorking -= 1;
            }
            else if (nWaiting < 2 &&
                nWorking < pool.max_workers)
            {
                VIPS_DEBUG_MSG("expanding thread pool");
                if (WorkerNew(pool))
                {
                    ThreadpoolFree(pool);
                    return -1;
                }
                nWorking += 1;
            }
        }

        // This will block until the last worker completes.
        ThreadpoolWait(pool);

        // Return 0 for success.
        result = pool.error ? -1 : 0;

        ThreadpoolFree(pool);

        if (!VipsImageGetConcurrency(image, 0))
            Console.WriteLine("threadpool completed with " + nWorking + " workers");

        // "minimise" is only emitted for top-level threadpools.
        if (!VipsImageGetType(image, "vips-no-minimise"))
            VipsImageMinimiseAll(image);

        return result;
    }
}
```