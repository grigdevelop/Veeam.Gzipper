using System;
using System.IO;
using System.IO.Compression;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;

namespace Veeam.Gzipper.Core.Commands
{
    public class DecompressCommand : ICommand
    {
        private readonly IStreamFactory _streamFactory;

        public DecompressCommand(IStreamFactory streamFactory)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        }

        public void StartSync(UserInputData input)
        {
            // create source streams
            using var sourceStream = _streamFactory.CreateSourceFileStream(input.SourceFilePath);
            using var zipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            // create target streams
            using var targetStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);

            // create chunk reader
            using var chunkReader = new ChunkedStreamReader(zipStream);

            // set original file size
            targetStream.SetLength(chunkReader.OriginalSourceSize);

            // count how many thread executed
            var executed = 0;

            chunkReader.ReadAll(chunk =>
            {
                targetStream.Seek(chunk.Position, SeekOrigin.Begin);
                targetStream.Write(chunk.Data, 0, chunk.Data.Length);
                executed++;
            });
        }
    }
}
