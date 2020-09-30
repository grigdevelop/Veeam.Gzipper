using Veeam.Gzipper.Core.Configuration.Abstraction;

namespace Veeam.Gzipper.Core.Configuration
{
    /// <inheritdoc cref="ICompressorSettings"/>
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
                ChunkSize = (int)(originalFileSize / 4);
            else
            {
                if (availableMemorySize <= 1024)
                {
                    ChunkSize = (int)availableMemorySize;
                    return;
                }


                if (availableMemorySize <= 1024 * 128)
                {
                    ChunkSize = (int)(availableMemorySize / 2);
                    return;
                }


                if (availableMemorySize <= 1024 * 512)
                {
                    ChunkSize = (int)(availableMemorySize / 4);
                    return;
                }

                ChunkSize = (int)(availableMemorySize / 8);
            }
        }
    }
}
