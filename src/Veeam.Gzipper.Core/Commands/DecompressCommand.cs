using System;
using System.IO;
using System.IO.Compression;
using System.Text;
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
            byte[] sizeBuffer = new byte[8];
            sourceStream.Read(sizeBuffer);
            var currentThreadSize = BitConverter.ToInt64(sizeBuffer);
            Console.WriteLine("The size is: " + currentThreadSize);

            using var zipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            var _bufferSize = _settings.ChunkSize;

            var done = 0L;
            var buffer = new byte[_bufferSize];
            int read;
            sourceStream.Seek(8, SeekOrigin.Begin);
            while (done < currentThreadSize - _bufferSize)
            {
                read = zipStream.Read(buffer, 0, buffer.Length);
                //reader.SetData(buffer, read);
                Console.WriteLine(Encoding.Default.GetString(buffer));
                done += read;
            }

            var left = (int)(currentThreadSize - done);
            read = zipStream.Read(buffer, 0, left);
            Console.WriteLine(Encoding.Default.GetString(buffer));

            //using var zipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            // create target streams
            //using var targetStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);



            // create chunk reader
            //var chunkReader = new ChunkedStreamReader(zipStream, _settings.ChunkSize);


            //chunkReader.ReadParallel(reader =>
            //{
            //    //var data = reader.Read();
            //    //var index = reader.Index;
            //    //while(data != null)
            //    //{

            //    //}
            //});

            //chunkReader.ReadParallel(chunk =>
            //{
            //    safeStream.Seek(chunk.Position, SeekOrigin.Begin);
            //    safeStream.Write(chunk.Data, 0, chunk.Data.Length);

            //    executed++;
            //});
        }
    }
}
