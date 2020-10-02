namespace Veeam.Gzipper.Core.Configuration.Abstraction
{
    /// <summary>
    /// Represents Compressor settings
    /// </summary>
    public interface ICompressorSettings
    {
        /// <summary>
        /// Gets the chunk size
        /// </summary>
        int ChunkSize { get; }
    }
}
