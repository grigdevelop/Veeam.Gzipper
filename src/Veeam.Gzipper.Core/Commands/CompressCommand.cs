using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Utilities;

namespace Veeam.Gzipper.Core.Commands
{
    public class CompressCommand : ICommand
    {
        private readonly IStreamFactory _streamFactory;
        private readonly ILogger _logger;

        public CompressCommand(IStreamFactory streamFactory, ILogger logger)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void StartSync(UserInputData input)
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
            new Thread(() => ShowPercentage(ref executedCount, chunkReader)).Start();

            chunkReader.Read(chunk =>
            {
                // TODO: try async write with sync context
                safeStream.Write(chunk.Data, 0, chunk.Data.Length);
                //GC.SuppressFinalize(chunk);
                executedCount++;
            });

            // wait until all threads executed
            while (executedCount < chunkReader.MaxThreadsLimit)
            {

            }

            _logger.InfoStatic("100 %\n\n");
        }

        private void ShowPercentage(ref int executedCount, StreamChunkReader reader)
        {
            while (executedCount < reader.MaxThreadsLimit)
            {
                _logger.InfoStatic(((executedCount / (double)reader.MaxThreadsLimit) * 100.0) + " %");
                Thread.Sleep(100);
            }
        }
    }
}
