using System;
using System.Threading;

namespace Veeam.Gzipper.Core.Threads
{
    public class ExThread
    {
        public Thread BaseThread { get; }

        public event Action<ExThread, Exception> OnError;

        public event Action<object> OnCompleted;

        public bool IsCompleted { get; private set; }

        public Exception Error { get; private set; }

        public ExThread(ParameterizedThreadStart start)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));

            BaseThread = new Thread((obj) => ExceptionHandlerWrapper(start, obj));
        }

        public void Start(object obj)
        {
            BaseThread.Start(obj);
        }

        private void ExceptionHandlerWrapper(ParameterizedThreadStart start, object? obj)
        {
            try
            {
                start(obj);
                IsCompleted = true;
                OnCompleted?.Invoke(this);
            }
            catch (ThreadInterruptedException)
            {
                // ignore ThreadInterruptedException
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Error = e;
                OnError?.Invoke(this, e);
            }
        }


        public override int GetHashCode()
        {
            return BaseThread.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is ExThread other && BaseThread.Equals(other.BaseThread);
        }
    }
}
