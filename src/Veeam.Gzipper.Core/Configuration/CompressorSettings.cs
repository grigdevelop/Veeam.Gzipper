using Veeam.Gzipper.Core.Configuration.Abstraction;

namespace Veeam.Gzipper.Core.Configuration
{
    /// <inheritdoc cref="ICompressorSettings"/>
    public class CompressorSettings : ICompressorSettings
    {
        //public int ChunkSize => 1024 * 1024;
        public int ChunkSize => 20;
    }
}
