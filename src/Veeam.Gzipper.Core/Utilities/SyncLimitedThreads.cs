using System;
using System.Threading;

namespace Veeam.Gzipper.Core.Utilities
{
    public class SyncLimitedThreads
    {
        private readonly object _syncLock = new object();

        private readonly Action<int> _callback;
        private readonly int _maxCount;
        private readonly Semaphore _semaphore;

        private bool _started;

        public SyncLimitedThreads(Action<int> callback, int activeCount, int maxCount)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));

            if (activeCount < 1)
                throw new ArgumentException($"The '{nameof(activeCount)}' parameter should be greater then equal 1");
            if (maxCount < activeCount)
                throw new ArgumentException($"The '{nameof(maxCount)}' parameter should be greater then equal '{activeCount}' parameter");

            _maxCount = maxCount;
            _semaphore = new Semaphore(activeCount, activeCount);
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

                new Thread(ActionStarter).Start(0);
            }


        }

        private void ActionStarter(object arg)
        {
            var n = (int)arg;
            if (n >= _maxCount)
            {
                //_semaphore.Dispose();
                return;
            }

            _semaphore.WaitOne(); // limit active threads

            // call next one if limit is not over
            new Thread(ActionStarter).Start(n + 1);

            // execute action sync
            _callback.Invoke(n);


            _semaphore.Release();
        }
    }
}
