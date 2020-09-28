using Veeam.Gzipper.Core.Configuration.Abstraction;

namespace Veeam.Gzipper.Core.Configuration
{
    public class CompressorSettings : ICompressorSettings
    {
        public long AvailableMemorySize => 1024 * 1024 * 8;
        public int ChunkSize { get; private set; }

        public void AutoSetChunkSize(long originalFileSize, long? availableMemorySize = null)
        {
            availableMemorySize ??= this.AvailableMemorySize;

            // actually for very small files compressing is not optimal
            // it will also keep the addition metadata
            if (originalFileSize < 5)
            {
                ChunkSize = 1;
                return;
            }

            if (availableMemorySize > originalFileSize)
                ChunkSize = (int)(originalFileSize / 5);
            else
            {
                ChunkSize = (int)(availableMemorySize / 10);
            }
        }
    }
}
