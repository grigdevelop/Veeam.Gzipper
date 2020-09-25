using System.IO;

namespace Veeam.Gzipper.Core.Factories
{
    /// <summary>
    /// Abstraction for getting compression streams
    /// </summary>
    public interface ICompressorStreamFactory
    {
        /// <summary>
        /// Create a stream for reading source data
        /// </summary>
        /// <param name="sourceFilePath">Path to the source file</param>
        /// <returns>Source Stream</returns>
        Stream CreateSourceFileStream(string sourceFilePath);

        /// <summary>
        /// Create a stream for writing data into the target source
        /// </summary>
        /// <param name="targetFilePath">Target source file path</param>
        /// <returns>Target Stream</returns>
        Stream CreateTargetFileStream(string targetFilePath);
    }
}
