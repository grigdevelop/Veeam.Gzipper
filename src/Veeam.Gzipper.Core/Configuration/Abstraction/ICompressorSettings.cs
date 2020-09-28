namespace Veeam.Gzipper.Core.Configuration.Abstraction
{
    /// <summary>
    /// Represents Compressor settings
    /// </summary>
    public interface ICompressorSettings
    {
        /// <summary>
        /// Gets the available memory size
        /// </summary>
        long AvailableMemorySize { get; }

        /// <summary>
        /// Gets the chunk size
        /// </summary>
        int ChunkSize { get; }

        /// <summary>
        /// Set chunk size based on 'originalFileSize' and 'availableMemorySize' values
        /// </summary>
        /// <param name="originalFileSize">originalFileSize</param>
        /// <param name="availableMemorySize">availableMemorySize</param>
        void AutoSetChunkSize(long originalFileSize, long? availableMemorySize = null);
    }
}
