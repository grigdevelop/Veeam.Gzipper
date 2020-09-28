namespace Veeam.Gzipper.Core.Configuration.Abstraction
{
    public interface ICompressorSettings
    {
        /// <summary>
        /// Gets the available memory size
        /// </summary>
        long AvailableMemorySize { get; }

        /// <summary>
        /// Gets the chunk size
        /// </summary>
        int ChunkSize{ get; }

        void AutoSetChunkSize(long originalFileSize, long? availableMemorySize = null);
    }
}
