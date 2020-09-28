using System;

namespace Veeam.Gzipper.Core.Threads.Limitations
{
    /// <summary>
    /// Represents if state is interrupted and interruption reason of type <see cref="Exception"/>
    /// </summary>
    public class StlInterrupt
    {
        /// <summary>
        /// Gets if ony of threads interrupted or not
        /// </summary>
        public bool Interrupted { get; private set; }

        /// <summary>
        /// Gets the <see cref="Exception"/>
        /// </summary>
        public Exception Error { get; private set; }

        public void Interrupt()
        {
            Interrupted = true;
        }

        /// <summary>
        /// Sets the <see cref="Exception"/>
        /// </summary>
        /// <param name="exception"></param>
        public void SetException(Exception exception)
        {
            Error = exception;
        }
    }
}
