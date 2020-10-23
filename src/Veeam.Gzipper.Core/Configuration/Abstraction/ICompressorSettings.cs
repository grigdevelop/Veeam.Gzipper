namespace Veeam.Gzipper.Core.Configuration.Abstraction
{
    /// <summary>
    /// Represents Compressor settings
    /// </summary>
    public interface ICompressorSettings
    {
        /// <summary>
        /// Gets the buffer size
        /// </summary>
        int BufferSize { get; }

        /// <summary>
        /// Gets the available cores count
        /// </summary>
        int Cores { get; }
    }
}
