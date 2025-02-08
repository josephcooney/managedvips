Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Threading;

namespace Vips {
    public class Semaphore {
        private int v;
        private string name;
        private Mutex mutex;
        private ConditionalVariable cond;

        public Semaphore(int v, string name) {
            this.v = v;
            this.name = name;
            this.mutex = new Mutex();
            this.cond = new ConditionalVariable(mutex);
        }

        ~Semaphore() {
            Destroy();
        }

        public void Destroy() {
            mutex.Dispose();
            cond.Dispose();
        }

        /* Add n to the semaphore and signal any threads that are blocked waiting
         * a change.
         */
        public int Upn(int n) {
            lock (mutex) {
                v += n;
                if (n == 1)
                    cond.SignalOne();
                else
                    cond.Broadcast();

#ifdef DEBUG_IO
                Console.WriteLine($"Upn(\"{name}\",{n}) = {v}");
                if (v > 1)
                    throw new Exception("Up over 1!");
#endif /*DEBUG_IO*/

                return v;
            }
        }

        /* Increment the semaphore.
         */
        public int Up() {
            return Upn(1);
        }

        /* Wait for sem > n, then subtract n.
         * Returns -1 when the monotonic time in @end_time was passed.
         */
        private static int SemaphoreDownnUntil(Semaphore s, int n, long end_time) {
            VipsGate.Start("SemaphoreDownnUntil: wait");

            lock (s.mutex) {
                while (s.v < n) {
                    if (end_time == -1)
                        s.cond.WaitOne();
                    else if (!s.cond.WaitUntil(end_time)) {
                        /* timeout has passed.
                         */
                        VipsGate.Stop("SemaphoreDownnUntil: wait");
                        return -1;
                    }
                }

                s.v -= n;

#ifdef DEBUG_IO
                Console.WriteLine($"SemaphoreDownnUntil(\"{s.name}\",{n}): {s.v}");
#endif /*DEBUG_IO*/

                VipsGate.Stop("SemaphoreDownnUntil: wait");

                return s.v;
            }
        }

        /* Wait for sem>n, then subtract n. n must be >= 0. Returns the new semaphore
         * value.
         */
        public int Downn(int n) {
            if (n < 0)
                throw new ArgumentException("n must be >= 0");

            return SemaphoreDownnUntil(this, n, -1);
        }

        /* Wait for sem > 0, then decrement. Returns the new semaphore value.
         */
        public int Down() {
            return Downn(1);
        }

        /* Wait for sem > 0, then decrement.
         * Returns -1 when @timeout (in microseconds) has passed, or the new
         * semaphore value.
         */
        public int DownTimeout(long timeout) {
            long end_time = DateTime.Now.Ticks + (long)(timeout / 10000);

            return SemaphoreDownnUntil(this, 1, end_time);
        }
    }

    // VipsGate and VIPS_GATE_START/STOP are not shown as they are likely custom classes
}
```

Note that I've used the `System.Threading` namespace for mutexes and conditional variables. Also, I've replaced the `g_get_monotonic_time()` function with `DateTime.Now.Ticks`, which is a similar way to get the current time in ticks. The `VipsGate` class and its methods are not shown as they seem to be custom classes.