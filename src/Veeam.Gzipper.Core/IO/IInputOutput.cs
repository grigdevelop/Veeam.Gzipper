namespace Veeam.Gzipper.Core.IO
{
    /// <summary>
    /// Abstraction for reading and writing data from/into different sources
    /// </summary>
    public interface IInputOutput
    {
        /// <summary>
        /// Read string input data from source
        /// </summary>
        /// <param name="message">Message to show before reading</param>
        /// <returns>Input data</returns>
        string ReadLine(string message = "");

        /// <summary>
        /// Write message into the source
        /// </summary>
        /// <param name="message">Message to show</param>
        void Write(string message);
    }
}
