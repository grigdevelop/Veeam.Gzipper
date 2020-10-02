using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Veeam.Gzipper.Core.Threads.Limitations
{
    public class SyncLimitedThreads
    {
        private readonly object _syncLock = new object();

        private readonly Action<SltState> _callback;
        private readonly int _maxCount;
        private readonly Func<int, object> _syncBeforeCallback;
        private readonly int _activeCount;

        private bool _started;

        public SyncLimitedThreads(Action<SltState> callback, int activeCount, int maxCount, Func<int, object> syncBeforeCallback = null)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));

            if (activeCount < 1)
                throw new ArgumentException($"The '{nameof(activeCount)}' parameter should be greater then equal 1");
            if (maxCount < activeCount)
                throw new ArgumentException($"The '{nameof(maxCount)}' parameter should be greater then equal '{nameof(activeCount)}' parameter");

            _activeCount = activeCount;
            _maxCount = maxCount;
            _syncBeforeCallback = syncBeforeCallback;
        }

        /// <summary>
        /// Start and keep running 'activeCount' threads, until 'maxCount' of them will be executed.
        /// Method will wait until all threads will be executed.
        /// </summary>
        public void StartSync()
        {
            lock (_syncLock)
            {
                // prevent to call 'StartSync' more then one time
                if (_started) throw new InvalidOperationException("Limited Threads already started.");
                _started = true;

                // There's no ConcurrentHashSet, but we can use ConcurrentDictionary instead
                // GetHashCode (thread id which) and Equals already implemented, so we can use it as a key
                var threads = new ConcurrentDictionary<Thread, sbyte>();
                var interrupt = new StlInterrupt();

                for (var i = 0; i < _maxCount; i++)
                {
                    var thread = new Thread(Handler);
                    while (threads.TryAdd(thread, 0))
                    {

                    }

                    // start a thread with a new state
                    object syncResult = null;
                    if (_syncBeforeCallback != null) syncResult = _syncBeforeCallback(i);
                    var state = new SltState(threads, interrupt, i, syncResult);
                    thread.Start(state);

                    while (threads.Count >= _activeCount)
                    {
                        // keep active threads limit 
                    }

                    // if one of threads interrupted, stop adding others 
                    if (interrupt.Interrupted) break;
                }

                // wait for threads that are left
                while (threads.Count > 0)
                {

                }

                // all threads should be released before throwing exception
                if (interrupt.Error != null) throw interrupt.Error;
            }
        }

        private void Handler(object arg)
        {
            var state = (SltState)arg;
            try
            {

                _callback.Invoke(state);
                state.Release();
            }
            catch (ThreadInterruptedException)
            {
                state.Release();
            }
            catch (Exception e)
            {
                // this action will interrupt threads and will call state.Release() method to clean from the active threat list
                state.Interrupt(e);
            }
        }
    }
}
