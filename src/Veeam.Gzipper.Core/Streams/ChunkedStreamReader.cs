using System;
using System.IO;
using System.Linq;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads.Limitations;

namespace Veeam.Gzipper.Core.Streams
{

    public class ChunkedStreamReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly ICompressorSettings _settings;

        public long OriginalSourceSize { get; }

        public long CompressTimeAvailableMemoryLimit { get; }

        private int? _maxThreadsCount;
        public int MaxThreadsCount
        {
            get
            {
                if (_maxThreadsCount == null)
                {
                    _maxThreadsCount = (int)(OriginalSourceSize / _settings.ChunkSize);
                    var leftSize = (int)(OriginalSourceSize % _settings.ChunkSize);
                    if (leftSize > 0) _maxThreadsCount++;
                }
                return _maxThreadsCount.Value;
            }
        }

        public ChunkedStreamReader(Stream stream, ICompressorSettings settings)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // read metadata
            var buffer = new byte[Chunk.INDEX_SIZE];
            _stream.Read(buffer, 0, buffer.Length);
            OriginalSourceSize = BitConverter.ToInt64(buffer, 0);

            _stream.Read(buffer, 0, buffer.Length);
            CompressTimeAvailableMemoryLimit = BitConverter.ToInt64(buffer, 0);

            _settings.AutoSetChunkSize(OriginalSourceSize, CompressTimeAvailableMemoryLimit);
        }

        public void ReadAll(Action<ChunkedData> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var maxThreadsCount = (int)(OriginalSourceSize / _settings.ChunkSize);
            var leftSize = (int)(OriginalSourceSize % _settings.ChunkSize);
            if (leftSize > 0) maxThreadsCount++;

            // count how many threads will work at time
            var actualThreadsLimit = (int)(_settings.AvailableMemorySize / _settings.ChunkSize);
            if (actualThreadsLimit == 0) actualThreadsLimit++;
            if (actualThreadsLimit > maxThreadsCount) actualThreadsLimit = maxThreadsCount;

            var buffer = new byte[Chunk.INDEX_SIZE + _settings.ChunkSize];

            // limit active threads and chunks count by semaphore
            var slt = new SyncLimitedThreads(state =>
            {
                callback.Invoke((ChunkedData)state.SyncResult); // sync method
            }, actualThreadsLimit, maxThreadsCount, () =>
            {
                _stream.Read(buffer, 0, buffer.Length);
                var index = BitConverter.ToInt64(buffer!, 0);
                var query = buffer.Skip(Chunk.INDEX_SIZE);

                if (index < 0)
                {
                    index = -index;
                    query = query.Take(leftSize);
                }
                var chunk = new ChunkedData(index * _settings.ChunkSize, query.ToArray());
                return chunk;
            });
            slt.StartSync();

            //var semaphore = new Semaphore(actualThreadsLimit, actualThreadsLimit);

            //for (var i = 0; i < maxThreadsCount; i++)
            //{
            //    _stream.Read(buffer, 0, buffer.Length);
            //    var index = BitConverter.ToInt64(buffer!, 0);
            //    var query = buffer.Skip(Chunk.INDEX_SIZE);

            //    if (index < 0)
            //    {
            //        index = -index;
            //        query = query.Take(leftSize);
            //    }
            //    var chunk = new ChunkedData(index * _settings.ChunkSize, query.ToArray());
            //    new Thread(() =>
            //    {
            //        semaphore.WaitOne();
            //        callback.Invoke(chunk); // sync method
            //        semaphore.Release();
            //    }).Start();
            //}

        }

        public void Dispose()
        {
            //_stream?.Dispose();
        }
    }
}
