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

        private static object l = new object();

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
            new Thread(() => ShowPercentage(ref executed, chunkReader)).Start();

            chunkReader.ReadAll(chunk =>
            {
                targetStream.Seek(chunk.Position, SeekOrigin.Begin);
                targetStream.Write(chunk.Data, 0, chunk.Data.Length);
                executed++;
            });

            // wait until all threads executed
            while (executed < chunkReader.MaxThreadsCount)
            {

            }

            _logger.InfoStatic("100 %\n\n");
        }


        private void ShowPercentage(ref int executedCount, ChunkedStreamReader reader)
        {

            while (executedCount < reader.MaxThreadsCount)
            {
                _logger.InfoStatic(Math.Round((executedCount / (double)reader.MaxThreadsCount) * 100.0, 2) + " %");
                Thread.Sleep(100);
            }
        }
    }
}
