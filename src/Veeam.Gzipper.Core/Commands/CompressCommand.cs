using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.IO;
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
            //using var targetFileStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);

            var fi = new FileInfo(input.SourceFilePath);
            var partialSizeList = fi.GetPartialSizeList(_settings.Cores, _settings.BufferSize);

            //using var sourceFileStream = _streamFactory.CreateSourceFileStream(input.SourceFilePath);
            using var mmf = MemoryMappedFile.CreateFromFile(input.SourceFilePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

            var compressors = new CompressorPartial[partialSizeList.Length];
            for (var i = 0; i < partialSizeList.Length; i++)
            {
                compressors[i] = new CompressorPartial(mmf, $"temp_{i}.txt.zip", partialSizeList[i] * i, partialSizeList[i], _settings.BufferSize);
            }

            var asyncResults = new IAsyncResult[partialSizeList.Length];
            for (var i = 0; i < compressors.Length; i++)
                asyncResults[i] = compressors[i].Start();

            foreach (var asyncResult in asyncResults)
                asyncResult.AsyncWaitHandle.WaitOne();

            // first stream can start a write
            // the second stream 


        }



        public void StartSync1(UserInputData input)
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
            var (index, partiallyFileReader, cancellationToken) = (ValueTuple<int, PartiallyFileReader, CancellationToken>)obj;
            using var sourceStream = partiallyFileReader.CreatePartialStream(index);
            using var targetStream = _streamFactory.CreateTargetFileStream(GetTempFileName(index));
            using var zipStream = new GZipStream(targetStream, CompressionMode.Compress);

            var buffer = new byte[_settings.BufferSize];
            var numRead = sourceStream.Read(buffer);
            while (numRead > 0 && !cancellationToken.IsCancellationRequested)
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

    class MyClass : IDisposable
    {
        private readonly PartiallyFileReader _partiallyFileReader;

        public MyClass(PartiallyFileReader partiallyFileReader)
        {
            _partiallyFileReader = partiallyFileReader ?? throw new ArgumentNullException(nameof(partiallyFileReader));
        }

        public void Dispose()
        {
            _partiallyFileReader.Dispose();
        }
    }


}
