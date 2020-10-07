using System;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Extensions;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Threads;

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
            using var mmf = MemoryMappedFile.CreateFromFile((FileStream)sourceStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.Inheritable, false);

            var portionSizes = new long[_settings.Cores];
            portionSizes[0] = sourceStream.ReadInt64();
            for (var i = 1; i < portionSizes.Length; i++)
            {
                sourceStream.Seek(portionSizes[i - 1], SeekOrigin.Current);
                portionSizes[i] = sourceStream.ReadInt64();
            }

            var offsets = new long[_settings.Cores];
            offsets[0] = sizeof(long);
            for (var i = 1; i < offsets.Length; i++)
            {
                offsets[i] = offsets[i - 1] + portionSizes[i - 1] + sizeof(long);
            }

            var threads = new ThreadWrapper[_settings.Cores];
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new ThreadWrapper(DecompressPortion);
            }

            for (var i = 0; i < _settings.Cores; i++)
            {
                var offset = offsets[i];
                var size = portionSizes[i];

                var view = mmf.CreateViewStream(offset, size, MemoryMappedFileAccess.Read);
                var zipStream = new GZipStream(view, CompressionMode.Decompress);

                threads[i].Start(new Tuple<Stream, int>(zipStream, i));
            }

            using var targetStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);
            for (var i = 0; i < _settings.Cores; i++)
            {
                var thread = threads[i];
                while (!thread.IsCompleted)
                {

                }
                using (var fs = File.OpenRead($"temp_decompress_{i}"))
                {
                    fs.CopyTo(targetStream);
                }
                File.Delete($"temp_decompress_{i}");
            }
        }

        private void DecompressPortion(object obj)
        {
            var (sourceStream, index) = (Tuple<Stream, int>)obj;

            using (sourceStream)
            {
                using var tempTargetStream = File.Open($"temp_decompress_{index}", FileMode.Create, FileAccess.Write);
                var buffer = new byte[_settings.BufferSize];
                var read = sourceStream.Read(buffer);

                while (read > 0)
                {
                    tempTargetStream.Write(buffer, 0, read);
                    read = sourceStream.Read(buffer);
                }
            }
        }
    }
}
