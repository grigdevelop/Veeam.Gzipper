using System;
using System.Threading;

namespace Veeam.Gzipper.Core.Utilities
{
    public class SyncLimitedThreads
    {
        private readonly Action<int> _execute;
        private readonly int _maxCount;
        private readonly Semaphore _semaphore;

        public SyncLimitedThreads(Action<int> execute, int activeCount, int maxCount)
        {
            _execute = execute;
            _maxCount = maxCount;
            _semaphore = new Semaphore(activeCount, activeCount);
        }

        public void StartSync()
        {
            new Thread(ActionStarter).Start(0);
        }

        private void ActionStarter(object arg)
        {
            var n = (int)arg;
            if (n >= _maxCount)
            {
                return;
            }

            _semaphore.WaitOne(); // limit active threads

            // call next one if limit is not over
            new Thread(ActionStarter).Start(n + 1);

            // execute action sync
            _execute.Invoke(n);

            _semaphore.Release();
        }
    }
}
