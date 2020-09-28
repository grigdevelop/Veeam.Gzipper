using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Veeam.Gzipper.Core.Threads.Limitations
{
    /// <summary>
    /// Context State for <see cref="SyncLimitedThreads"/> 
    /// </summary>
    public class SltState
    {
        private readonly ConcurrentDictionary<Thread, sbyte> _threads;
        private readonly StlInterrupt _ic;

        /// <summary>
        /// Gets the thread identifier based on 0 ( from 0 to max threads count )
        /// </summary>
        public int Index { get; }

        public SltState(ConcurrentDictionary<Thread, sbyte> threads, StlInterrupt ic, int index)
        {
            _threads = threads;
            _ic = ic;
            Index = index;
        }

        /// <summary>
        /// Release thread from active threads
        /// </summary>
        public void Release()
        {
            while (_threads.TryRemove(Thread.CurrentThread, out _))
            {

            }
        }

        /// <summary>
        /// Release and interrupt all active threads also 
        /// </summary>
        /// <param name="ex">Pass null if interruption happened because of other thread exception</param>
        public void Interrupt(Exception ex = null)
        {
            _ic.SetException(ex);
            _ic.Interrupt();
            Release();
            foreach (var t in _threads)
            {
                t.Key.Interrupt();
            }

        }
    }
}
