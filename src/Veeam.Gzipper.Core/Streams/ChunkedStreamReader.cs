using System;
using System.IO;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Utilities;

namespace Veeam.Gzipper.Core.Streams
{
    
    public class ChunkedStreamReader : IDisposable
    {
        private readonly Stream _stream;

        private long? _originalSourceSize;
        public long OriginalSourceSize
        {
            get
            {
                if (_originalSourceSize == null)
                {
                    // get the original size of the source file
                    const int lengthSize = sizeof(long);
                    var sizeBuffer = new byte[lengthSize];
                    _stream.Read(sizeBuffer, 0, lengthSize);
                    _originalSourceSize = BitConverter.ToInt64(sizeBuffer, 0);
                }

                return _originalSourceSize.Value;
            }
        }

        public ChunkedStreamReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }



        public void ReadAll(Action<ChunkedData> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // our chunk size is constant, so it's possible that we will have some empty bytes in our source stream
            var emptySlotsSize = ProcessorConstants.CHUNK_SIZE - (int)(OriginalSourceSize % ProcessorConstants.CHUNK_SIZE);
            
            /*
             NOTE: Reading itself is sync, because of gzip compression algorithm,
             but we can proceed received data independently
             */

            var indexBuffer = new byte[Chunk.INDEX_SIZE];
            var read = _stream.Read(indexBuffer, 0, indexBuffer.Length);

            var buffer = new byte[ProcessorConstants.CHUNK_SIZE];

            while (read > 0)
            {
                var index = BitConverter.ToInt32(indexBuffer, 0);

                var currentBuffer = buffer;

                if (index < 0)
                {
                    index = -index; // make it positive
                    currentBuffer = new byte[ProcessorConstants.CHUNK_SIZE - emptySlotsSize];

                    // read partial chunk
                    _stream.Read(currentBuffer, 0, currentBuffer.Length);

                    // read empty chunk
                    var emptyChunk = new byte[emptySlotsSize];
                    _stream.Read(emptyChunk, 0, emptyChunk.Length);
                }
                else
                {
                    // read full chunk
                    _stream.Read(currentBuffer, 0, currentBuffer.Length);
                }

                // this call can be async. We don't need to wait 
                callback.Invoke(new ChunkedData((long)index * ProcessorConstants.CHUNK_SIZE, currentBuffer));

                // read next index
                read = _stream.Read(indexBuffer, 0, indexBuffer.Length);
            }


        }

        public void Dispose()
        {
            //_stream?.Dispose();
        }
    }
}
