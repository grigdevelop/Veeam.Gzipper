using System;
using System.Collections.Generic;
using System.IO.Compression;
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
            using(var sourceFileStream = _streamFactory.CreateSourceFileStream(input.SourceOrTargetFilePath))
            {
                using(var targetFileStream = _streamFactory.CreateTargetFileStream(input.SourceOrTargetFilePath))
                {
                    using(var gzipStream = new GZipStream(targetFileStream, CompressionMode.Compress))
                    {

                        var sourceSize = sourceFileStream.Length;
                        if (sourceSize <= DataChunk.CHUNK_SIZE) // we only need one thread to read the data
                        {

                        }
                        else
                        {
                            // let's calculate how many threads we need to work asynchronously
                            var maxThreads = (int)sourceSize / DataChunk.CHUNK_SIZE; // last data size will be sourceSize % CHUNK_SIZE
                            var bytesLeft = sourceSize % DataChunk.CHUNK_SIZE;
                            if (bytesLeft > 0) maxThreads += 1;
                            var activeThreadsLimit = ProcessorConstants.AVAILABLE_MEMEORY / DataChunk.CHUNK_SIZE;
                            activeThreadsLimit = Math.Min(activeThreadsLimit, maxThreads);

                            var semaphore = new Semaphore(activeThreadsLimit, activeThreadsLimit);

                            // buffer
                            var buffer = new byte[20];
                        }


                    }
                }
            }
        }

        private void StartReadingData()
        {
            
        }
    }
}
