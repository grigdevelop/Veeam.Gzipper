using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.Factories;
using Veeam.Gzipper.Core.Types;
using Veeam.Gzipper.Core.Utilities;

namespace Veeam.Gzipper.Core.Processors
{
    public class CompressorProcessor
    {
        private readonly ICompressorStreamFactory _streamFactory;

        public CompressorProcessor(ICompressorStreamFactory streamFactory)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        }

        public void ProceedSync(GzipperInputData input)
        {
            // stream for reading from source
            using var sourceFileStream = _streamFactory.CreateSourceFileStream(input.SourceOrTargetFilePath);

            // streams for compressing
            using var targetFileStream = _streamFactory.CreateTargetFileStream(input.ZipFilePath);
            using var gzipStream = new GZipStream(targetFileStream, CompressionMode.Compress);

            // gzip not support async writing. create synced stream 
            var safeStream = Stream.Synchronized(gzipStream);

            // write the source size at the beginning of compressed file
            var sizeBuffer = BitConverter.GetBytes(sourceFileStream.Length);
            safeStream.Write(sizeBuffer, 0, sizeBuffer.Length);

            // write empty bytes in case if size of last partial of source data less then chunk size
            //var emptyBuffer = new byte[sizeBuffer.Length % (ProcessorConstants.CHUNK_SIZE + Chunk.INDEX_SIZE)];
            //safeStream.Write(emptyBuffer, 0, emptyBuffer.Length);

            // start async reading from the source by chunks. Chunks contains original position of partial data
            using var chunkReader = new StreamChunkReader(sourceFileStream, ProcessorConstants.CHUNK_SIZE, ProcessorConstants.AVAILABLE_MEMEORY);
            chunkReader.Read(chunk =>
            {
                //Console.WriteLine(chunk.Data.Skip(4));
                safeStream.Write(chunk.Data, 0, chunk.Data.Length);
            });

            //Thread.Sleep(5000);
        }
    }
}
