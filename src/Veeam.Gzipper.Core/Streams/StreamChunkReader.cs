using System;
using System.IO;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads.Limitations;

namespace Veeam.Gzipper.Core.Utilities
{


    public class StreamChunkReader : IDisposable
    {
        private readonly int _maxThreadsLimit;
        private readonly int _actualThreadsLimit;
        private readonly int _bufferSize;

        public Stream BaseStream { get; }

        public int MaxThreadsLimit => _maxThreadsLimit;

        public long SourceSize { get; }


        public StreamChunkReader(Stream stream, int bufferSize, long allocatedMemoryLimitSize)
        {
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _bufferSize = bufferSize;

            SourceSize = BaseStream.Length;
            if (SourceSize == 0)
                throw new NotSupportedException("Unable to compress empty source");

            // count how many threads will work totally
            var maxThreadsLimit = (int)(SourceSize / bufferSize);
            if (BaseStream.Length % bufferSize > 0) maxThreadsLimit++;
            _maxThreadsLimit = maxThreadsLimit;

            // count how many threads will work at time
            var actualThreadsLimit = (int)(allocatedMemoryLimitSize / bufferSize);
            if (actualThreadsLimit == 0) actualThreadsLimit++;
            _actualThreadsLimit = actualThreadsLimit > _maxThreadsLimit ? _maxThreadsLimit : actualThreadsLimit;
        }

        public void Read(Action<Chunk> callback)
        {
            var threadsDone = 0;
            var slt = new SyncLimitedThreads(i =>
            {
                // create buffer and start reading
                var chunk = new Chunk(_bufferSize, i);
                var asyncResult = BaseStream.BeginRead(chunk.Data, Chunk.INDEX_SIZE, _bufferSize, null!, null);

                // before end reading set position
                BaseStream.Seek((long)i * _bufferSize, SeekOrigin.Begin);
                var read = BaseStream.EndRead(asyncResult);
                if (read < _bufferSize)
                {
                    chunk.MarkAsWithEmptySlots();
                }
                callback.Invoke(chunk);
                threadsDone++;
            }, _actualThreadsLimit, _maxThreadsLimit);

            slt.StartSync();


            // StartSync part itself will work synchronously
            // But for handling callbacks need additional functionality
            while (threadsDone < _maxThreadsLimit)
            {

            }
        }

        public void Dispose()
        {
            //BaseStream?.Dispose();
        }
    }
}
