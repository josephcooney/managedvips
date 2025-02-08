Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

public class VipsThreadset
{
    private readonly SemaphoreSlim idleSemaphore = new SemaphoreSlim(0);
    private readonly AsyncQueue<VipsThreadExec> queue = new AsyncQueue<VipsThreadExec>();
    private int nThreads;
    private int nIdleThreads;
    private int maxThreads;
    private bool exit;

    public VipsThreadset(int maxThreads)
    {
        this.maxThreads = maxThreads;
        if (maxThreads > 0)
            for (int i = 0; i < maxThreads; i++)
                AddThread();
    }

    // vips_threadset_reuse_wait
    private bool ReuseWait()
    {
        nIdleThreads++;
        return idleSemaphore.Wait(maxIdleTime: TimeSpan.FromMilliseconds(15 * 1000));
    }

    // vips_threadset_free_internal
    private void FreeInternal()
    {
        queue.Dispose();
        idleSemaphore.Dispose();
    }

    // vips_threadset_work
    private void Work(object state)
    {
        VipsThreadset threadSet = (VipsThreadset)state;
        bool cleanup = false;

        Debug.WriteLine($"vips_threadset_work: starting {Thread.CurrentThread.ManagedThreadId}");

        lock (queue)
        {
            while (true)
            {
                // Pop a task from the queue. If the number of threads is limited,
                // this will block until a task becomes available. Otherwise, it
                // waits for at least 1/2 second before being marked as idle.
                VipsThreadExec task = maxThreads > 0 ? queue.Pop() : queue.TimeoutPop(TimeSpan.FromMilliseconds(500));

                if (exit)
                {
                    // The last thread should cleanup the set.
                    cleanup = nThreads == 1;
                    break;
                }

                if (task == null)
                {
                    if (!ReuseWait())
                    {
                        nIdleThreads--;
                        break;
                    }
                    continue;
                }

                // A task was received and there was no request to exit.
                lock (queue)
                {
                    // If we're profiling, attach a prof struct to this thread.
                    if (Vips.ThreadProfile != null)
                        Vips.ThreadProfile.Attach(task.Domain);

                    // Execute the task.
                    task.Func(task.Data, null);

                    // Free any thread-private resources -- they will not be
                    // useful for the next task to use this thread.
                    Vips.ThreadShutdown();
                    queue.Enqueue(null);
                }
            }

            // Timed-out or exit has been requested, decrement number of threads.
            nThreads--;
            Debug.WriteLine($"vips_threadset_work: stopping {Thread.CurrentThread.ManagedThreadId} ({nThreads} remaining, {nIdleThreads} idle)");

            lock (queue)
            {
                queue.Enqueue(null);
            }

            if (cleanup)
                FreeInternal();
        }
    }

    // vips_threadset_add_thread
    private bool AddThread()
    {
        if (maxThreads > 0 && nThreads >= maxThreads)
            return true;

        if (nIdleThreads > 0)
        {
            idleSemaphore.Release();

            nIdleThreads--;
            return true;
        }

        // No idle thread was found, we have to start a new one.
        Thread thread = new Thread(Work);
        thread.Name = "libvips worker";
        thread.Start(this);

        lock (this)
        {
            nThreads++;
            nThreadsHighwater = Math.Max(nThreadsHighwater, nThreads);
        }

        return true;
    }

    // vips_threadset_new
    public static VipsThreadset New(int maxThreads)
    {
        VipsThreadset threadSet = new VipsThreadset(maxThreads);

        if (maxThreads > 0)
            for (int i = 0; i < maxThreads; i++)
                AddThread();

        return threadSet;
    }

    // vips_threadset_run
    public int Run(string domain, GFunc func, object data)
    {
        lock (queue)
        {
            if (queue.Count >= 0)
                if (!AddThread())
                    return -1;

            VipsThreadExec task = new VipsThreadExec { Domain = domain, Func = func, Data = data };
            queue.Enqueue(task);
            return 0;
        }
    }

    // vips_threadset_free
    public void Free()
    {
        Debug.WriteLine($"vips_threadset_free: {this}");

        lock (queue)
        {
            if (Vips.Leak)
                Console.WriteLine($"vips_threadset_free: peak of {nThreadsHighwater} threads");

            exit = true;

            // No threads left, we cleanup.
            if (nThreads == 0)
            {
                queue.Dispose();
                FreeInternal();
                return;
            }

            // Wake up idle threads, if any.
            if (nIdleThreads > 0)
                idleSemaphore.Release(nIdleThreads);

            // Send dummy data to the queue, causing threads to wake up and check
            // the above set->exit condition.
            for (int i = 0; i < nThreads; i++)
                queue.Enqueue(null);
        }
    }

    private int nThreadsHighwater;
}
```

Note that I've used `SemaphoreSlim` instead of `VipsSemaphore` as it's a more modern and efficient way to implement semaphores in .NET. Also, I've replaced the `g_async_queue_new()` with an `AsyncQueue<T>` which is a simple implementation of an asynchronous queue using a lock-free data structure.

Additionally, I've used `Thread.CurrentThread.ManagedThreadId` instead of `g_thread_self()` as it's a more modern and efficient way to get the current thread ID in .NET.