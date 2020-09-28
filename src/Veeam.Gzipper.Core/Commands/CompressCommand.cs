using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Utilities;

namespace Veeam.Gzipper.Core.Commands
{
    public class CompressCommand : ICommand
    {
        private readonly IStreamFactory _streamFactory;
        private readonly ILogger _logger;
        private readonly ICompressorSettings _settings;

        public CompressCommand(IStreamFactory streamFactory, ILogger logger, ICompressorSettings settings)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void StartSync(UserInputData input)
        {
            // stream for reading from source
            using var sourceFileStream = _streamFactory.CreateSourceFileStream(input.SourceFilePath);

            // set chunk size
            _settings.AutoSetChunkSize(sourceFileStream.Length);

            // streams for compressing and saving in file
            using var targetFileStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);
            using var gzipStream = new GZipStream(targetFileStream, CompressionMode.Compress);

            // gzip not support async writing. create synced stream 
            var safeStream = Stream.Synchronized(gzipStream);

            // write the source size at the beginning of compressed file
            var sizeBuffer = BitConverter.GetBytes(sourceFileStream.Length);
            safeStream.Write(sizeBuffer, 0, sizeBuffer.Length);

            // write current available memory ( which can be changed from configs ) after file size
            sizeBuffer = BitConverter.GetBytes(_settings.AvailableMemorySize);
            safeStream.Write(sizeBuffer, 0, sizeBuffer.Length);

            // start async reading from the source by chunks. Chunks contains original position of partial data
            using var chunkReader = new StreamChunkReader(sourceFileStream, _settings.ChunkSize, _settings.AvailableMemorySize);

            // count how many threads executed
            var executedCount = 0;

            // monitor progress
            var progressThread = new Thread(() => ShowProgress(ref executedCount, chunkReader));
            progressThread.Start();

            try
            {
                chunkReader.Read(chunk =>
                {
                    safeStream.Write(chunk.Data, 0, chunk.Data.Length);
                    executedCount++;
                });

                // wait until all threads executed
                while (executedCount < chunkReader.MaxThreadsLimit)
                {

                }
            }
            finally
            {
                progressThread.Interrupt();
            }

            _logger.InfoStatic("100 %\n\n");
        }

        private void ShowProgress(ref int executedCount, StreamChunkReader reader)
        {
            try
            {
                while (executedCount < reader.MaxThreadsLimit)
                {
                    _logger.InfoStatic(Math.Round((executedCount / (double)reader.MaxThreadsLimit) * 100.0, 2) + " %");
                    Thread.Sleep(100);
                }
            }
            catch (ThreadInterruptedException)
            {
                // ignore ThreadInterruptedException
            }
        }
    }
}
