using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;

namespace Veeam.Gzipper.Core.Commands
{
    public class DecompressCommand : ICommand
    {
        private readonly IStreamFactory _streamFactory;
        private readonly ICompressorSettings _settings;
        private readonly ILogger _logger;

        public DecompressCommand(IStreamFactory streamFactory, ICompressorSettings settings, ILogger logger)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void StartSync(UserInputData input)
        {
            // create source streams
            using var sourceStream = _streamFactory.CreateSourceFileStream(input.SourceFilePath);
            using var zipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            // create target streams
            using var targetStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);

            // create chunk reader
            using var chunkReader = new ChunkedStreamReader(zipStream, _settings);

            // set original file size
            targetStream.SetLength(chunkReader.OriginalSourceSize);

            // count how many thread executed
            var executed = 0;

            // monitor progress
            var progressThread = new Thread(() => ShowProgress(ref executed, chunkReader));
            progressThread.Start();

            var safeStream = Stream.Synchronized(targetStream);

            try
            {
                chunkReader.ReadAll(chunk =>
                {
                    safeStream.Seek(chunk.Position, SeekOrigin.Begin);
                    safeStream.Write(chunk.Data, 0, chunk.Data.Length);

                    executed++;
                });

                // wait until all threads executed
                while (executed < chunkReader.MaxThreadsCount)
                {

                }
            }
            finally
            {
                progressThread.Interrupt();
            }

            _logger.InfoStatic("100 %\n\n");
        }


        private void ShowProgress(ref int executedCount, ChunkedStreamReader reader)
        {
            try
            {
                while (executedCount < reader.MaxThreadsCount)
                {
                    _logger.InfoStatic(Math.Round((executedCount / (double)reader.MaxThreadsCount) * 100.0, 2) + " %");
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
