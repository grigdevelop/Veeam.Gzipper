namespace Veeam.Gzipper.Core.Logging.Abstraction
{
    /// <summary>
    /// Represents object for logging messages
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Print information message
        /// </summary>
        /// <param name="message">Message</param>
        void Info(string message);

        /// <summary>
        /// Print warning message
        /// </summary>
        /// <param name="message">Message</param>
        void Warning(string message);

        /// <summary>
        /// Print error message
        /// </summary>
        /// <param name="message">Message</param>
        void Error(string message);

        /// <summary>
        /// Log static information
        /// </summary>
        /// <param name="message"></param>
        void InfoStatic(string message);
    }
}
