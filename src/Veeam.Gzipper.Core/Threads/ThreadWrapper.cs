using System;
using System.Threading;

namespace Veeam.Gzipper.Core.Threads
{
    public class ThreadWrapper
    {
        #region Fields

        private readonly AutoResetEvent _threadCompleteResetEvent;

        #endregion

        #region Properties

        public Thread BaseThread { get; }

        public bool IsCompleted { get; private set; }

        public Exception Error { get; private set; }

        #endregion

        #region Events

        public event Action<ThreadWrapper, Exception> OnError;

        #endregion

        #region Public methods

        public ThreadWrapper(ParameterizedThreadStart start)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));

            BaseThread = new Thread((obj) => ExceptionHandlerWrapper(start, obj));

            _threadCompleteResetEvent = new AutoResetEvent(false);
        }

        public void Start(object obj)
        {
            BaseThread.Start(obj);
        }

        public void Wait()
        {
            if (IsCompleted)
                return;
            _threadCompleteResetEvent.WaitOne();
        }


        public override int GetHashCode()
        {
            return BaseThread.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is ThreadWrapper other && BaseThread.Equals(other.BaseThread);
        }

        #endregion

        #region Private methods


        private void ExceptionHandlerWrapper(ParameterizedThreadStart start, object? obj)
        {
            try
            {
                start(obj);
                IsCompleted = true;
                _threadCompleteResetEvent.Set();
            }
            catch (Exception e)
            {
                Error = e;
                OnError?.Invoke(this, e);
            }
        }

        #endregion
    }
}
