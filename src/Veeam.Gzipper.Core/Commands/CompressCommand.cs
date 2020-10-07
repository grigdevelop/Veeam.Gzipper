using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads;

namespace Veeam.Gzipper.Core.Commands
{
    public class CompressParallelOption
    {
        public int Index { get; }

        public long SourceSize { get; }

        public long PortionSize { get; }

        public long CurrentThreadSize { get; }

        public long Offset => Index * CurrentThreadSize;

        public CompressParallelOption(
            int index,
            long sourceSize,
            long portionSize,
            long currentThreadSize)
        {
            Index = index;
            SourceSize = sourceSize;
            PortionSize = portionSize;
            CurrentThreadSize = currentThreadSize;
        }
    }

    public class CompressCommand : ICommand
    {
        private readonly IStreamFactory _streamFactory;
        private readonly ILogger _logger;
        private readonly ICompressorSettings _settings;

        private static string GetTempFileName(int i) => $"temp_compress_{i}.zip";


        public CompressCommand(IStreamFactory streamFactory, ILogger logger, ICompressorSettings settings)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void StartSync(UserInputData input)
        {
            using var targetFileStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);

            // get source size
            long sourceSize;
            using (var fs = File.OpenRead(input.SourceFilePath))
            {
                sourceSize = fs.Length;
            }

            var options = new CompressParallelOption[_settings.Cores];
            for (var i = 0; i < options.Length; i++)
            {
                // the size which should process one thread
                var currentThreadSize = (sourceSize / _settings.Cores);
                currentThreadSize = (currentThreadSize / _settings.BufferSize) * _settings.BufferSize;
                var portionSize = currentThreadSize;
                if (i == _settings.Cores - 1)
                {
                    portionSize = sourceSize - (currentThreadSize * (_settings.Cores - 1));
                }

                options[i] = new CompressParallelOption(i, sourceSize, portionSize, currentThreadSize);
            }

            var threads = new ThreadWrapper[options.Length];
            for (var i = 0; i < options.Length; i++)
            {
                threads[i] = new ThreadWrapper(Compress);
            }

            using (var mmf = MemoryMappedFile.CreateFromFile(input.SourceFilePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read))
            {
                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i].Start((options[i], mmf));
                }

                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i].Wait();
                    using (var fs = File.OpenRead(GetTempFileName(i)))
                    {
                        fs.CopyTo(targetFileStream);
                    }
                    File.Delete(GetTempFileName(i));
                    Console.WriteLine($"thread {i} done");
                }
            }
        }

        private void Compress(object obj)
        {
            var (option, mmf) = (ValueTuple<CompressParallelOption, MemoryMappedFile>)obj;
            using var sourceStream = mmf.CreateViewStream(option.Offset, option.PortionSize, MemoryMappedFileAccess.Read);
            using var targetStream = _streamFactory.CreateTargetFileStream(GetTempFileName(option.Index));
            using var zipStream = new GZipStream(targetStream, CompressionMode.Compress);

            var buffer = new byte[_settings.BufferSize];
            var numRead = sourceStream.Read(buffer);
            while (numRead > 0)
            {
                zipStream.Write(buffer, 0, numRead);
                numRead = sourceStream.Read(buffer);
            }
        }

        private Thread SetupProgress(StreamChunkReader chunkReader, CancellationToken cancellationToken)
        {

            var sourceSize = chunkReader.SourceSize;
            long done = 0;
            chunkReader.OnRead += read =>
            {
                done += read;
            };

            return new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _logger.InfoStatic($"{ Math.Round(((double)done / sourceSize) * 100.0, 0)} %");
                    Thread.Sleep(250);
                }
            });
        }

    }

    public class PartialCompressorCollection
    {
        public PartialCompressorCollection(Stream sourceStream)
        {
            
        }
    }
}
