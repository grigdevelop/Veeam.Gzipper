using System;
using System.IO;

namespace Veeam.Gzipper.Core.Utilities
{
    public class Chunk
    {
        public byte[] Data { get; }

        public int Index { get; }

        public const int INDEX_SIZE = 4; // 4 bytes for integer

        private Chunk(byte[] data, int index)
        {
            Index = index;
            var bytes = BitConverter.GetBytes(index);
            bytes.CopyTo(data, 0); // first 4 bytes for index

            Data = data;

        }

        public void MarkAsWithEmptySlots()
        {
            var bytes = BitConverter.GetBytes(-Index);
            bytes.CopyTo(Data, 0);
        }

        public Chunk(int bufferSize, int index) :
            this(new byte[bufferSize + INDEX_SIZE], index)
        {
        }
    }

    public class StreamChunkReader : IDisposable
    {
        private readonly int _maxThreadsLimit;
        private readonly int _actualThreadsLimit;
        private readonly int _bufferSize;

        public Stream BaseStream { get; }

        public int MaxThreadsLimit => _maxThreadsLimit;


        public StreamChunkReader(Stream stream, int bufferSize, int allocatedMemoryLimitSize)
        {
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _bufferSize = bufferSize;

            // count how many threads will work totally
            var maxThreadsLimit = (int)(BaseStream.Length / bufferSize);
            if (BaseStream.Length % bufferSize > 0) maxThreadsLimit++;
            _maxThreadsLimit = maxThreadsLimit;

            // count how many threads will work at time
            var actualThreadsLimit = allocatedMemoryLimitSize / bufferSize;
            if (actualThreadsLimit == 0) actualThreadsLimit++;
            _actualThreadsLimit = actualThreadsLimit;
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
                BaseStream.Seek(i * _bufferSize, SeekOrigin.Begin);
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
