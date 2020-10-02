using System;
using System.IO;
using System.IO.Compression;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads;

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
            var targetFileStream = File.Open(input.TargetFilePath, FileMode.Create, FileAccess.Write);

            // start async reading from the source by chunks. Chunks contains original position of partial data
            var chunkReader = new StreamChunkReader(input.SourceFilePath, _streamFactory, _settings.ChunkSize);

            chunkReader.ReadParallel((readers) =>
            {
                var threads = new ExThread[readers.Length];

                for (var i = 0; i < readers.Length; i++)
                {
                    threads[i] = new ExThread(PartialCompress);
                }

                for (var i = 0; i < readers.Length; i++)
                {
                    threads[i].Start(readers[i]);
                }

                for (var i = 0; i < readers.Length; i++)
                {
                    while (!threads[i].IsCompleted)
                    {
                    }
                    var tempPath = GetTempFileName(i);
                    using (var fs = File.OpenRead(tempPath))
                    {
                        targetFileStream.Write(BitConverter.GetBytes(fs.Length));
                        fs.CopyTo(targetFileStream);
                    }
                    File.Delete(tempPath);
                }

                targetFileStream.Dispose();

            });

        }

        private void PartialCompress(object obj)
        {
            try
            {
                var reader = (PortionChunkReader)obj;

                var tempFilePath = GetTempFileName(reader.Index);
                using var targetStream = File.Open(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                using var zipStream = new GZipStream(targetStream, CompressionMode.Compress);
                var buffer = new byte[_settings.ChunkSize];

                var read = reader.Read(buffer);
                while (read > 0)
                {
                    zipStream.Write(buffer, 0, read);
                    read = reader.Read(buffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private static string GetTempFileName(int i) => $"temp_compress_{i}.txt.zip";


    }
}
