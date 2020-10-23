using Veeam.Gzipper.Core.Configuration.Abstraction;

namespace Veeam.Gzipper.Core.Configuration
{
    /// <inheritdoc cref="ICompressorSettings"/>
    public class CompressorSettings : ICompressorSettings
    {
        public int BufferSize => 14;

        public int Cores => 4;
    }
}
