using System;
using System.IO;
using System.IO.Compression;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.Factories;
using Veeam.Gzipper.Core.Types;
using Veeam.Gzipper.Core.Utilities;

namespace Veeam.Gzipper.Core.Processors
{
    public class CompressProcessor : IProcessor
    {
        private readonly IStreamFactory _streamFactory;

        public CompressProcessor(IStreamFactory streamFactory)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        }

        public void StartSync(GzipperInputData input)
        {
            // stream for reading from source
            using var sourceFileStream = _streamFactory.CreateSourceFileStream(input.SourceFilePath);

            // streams for compressing and saving in file
            using var targetFileStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);
            using var gzipStream = new GZipStream(targetFileStream, CompressionMode.Compress);

            // gzip not support async writing. create synced stream 
            var safeStream = Stream.Synchronized(gzipStream);

            // write the source size at the beginning of compressed file
            var sizeBuffer = BitConverter.GetBytes(sourceFileStream.Length);
            safeStream.Write(sizeBuffer, 0, sizeBuffer.Length);

            // start async reading from the source by chunks. Chunks contains original position of partial data
            using var chunkReader = new StreamChunkReader(sourceFileStream, ProcessorConstants.CHUNK_SIZE, ProcessorConstants.AVAILABLE_MEMEORY);

            // count how many threads executed
            var executedCount = 0;

            chunkReader.Read(chunk =>
            {
                // TODO: try async write with sync context
                safeStream.Write(chunk.Data, 0, chunk.Data.Length);
                executedCount++;
            });

            // wait until all threads executed
            while (executedCount < chunkReader.MaxThreadsLimit)
            {

            }
        }
    }
}
