using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads;

namespace Veeam.Gzipper.Core.Streams
{
    public class StreamChunkReader : IDisposable
    {
        private readonly string _path;
        private readonly int _bufferSize;
        private readonly int _cores;
        private readonly MemoryMappedFile _mmf;

        private long? _sourceSize;
        public long SourceSize
        {
            get
            {
                if (_sourceSize.HasValue) return _sourceSize.Value;
                using var tempStream = File.OpenRead(_path);
                _sourceSize = tempStream.Length;
                return _sourceSize.Value;
            }
        }

        public event Action<int> OnRead;

        public StreamChunkReader(string path, int bufferSize, int cores)
        {
            _path = path;
            _bufferSize = bufferSize;
            _cores = cores;

            _mmf = MemoryMappedFile.CreateFromFile(_path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        }

        public void ReadParallel(Action<PortionChunkReader[]> callback)
        {
            const int cores = 4;
            var threads = new ThreadWrapper[cores];
            var chunkReaders = new PortionChunkReader[cores];

            for (var i = 0; i < cores; i++)
            {
                chunkReaders[i] = new PortionChunkReader(i);
                threads[i] = new ThreadWrapper(ReadDataFromSource);
                threads[i].OnError += (source, e) =>
                {
                    foreach (var t in threads)
                    {
                        if (Equals(t, source)) continue;
                        t.BaseThread.Interrupt();
                    }
                };
            }

            for (var i = 0; i < cores; i++)
            {
                threads[i].Start(chunkReaders[i]);
            }

            callback(chunkReaders);
        }

        private void ReadDataFromSource(object obj)
        {
            var reader = (PortionChunkReader)obj;

            // the size which should process one thread
            var currentThreadSize = (SourceSize / _cores);
            currentThreadSize = (currentThreadSize / _bufferSize) * _bufferSize;
            var portionSize = currentThreadSize;
            if (reader.Index == _cores - 1)
            {
                portionSize = SourceSize - (currentThreadSize * (_cores - 1));
            }

            using var sourceStream = _mmf.CreateViewStream(reader.Index * currentThreadSize, portionSize, MemoryMappedFileAccess.Read);
            var buffer = new byte[_bufferSize];
            int read;

            do
            {
                read = sourceStream.Read(buffer);
                reader.SetData(buffer, read);
                ExecuteOnRead(read);
            } while (read > 0);
        }

        public void Dispose()
        {
            _mmf.Dispose();
        }

        private void ExecuteOnRead(int read)
        {
            OnRead?.Invoke(read);
        }
    }
}
