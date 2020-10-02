using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads;

namespace Veeam.Gzipper.Core.Streams
{
    public class StreamChunkReader : IDisposable
    {
        private readonly string _path;
        private readonly IStreamFactory _streamFactory;
        private readonly int _bufferSize;
        private readonly int _cores;
        private readonly MemoryMappedFile _mmf;
        private readonly Stream _stream;
        
        public StreamChunkReader(string path, IStreamFactory streamFactory, int bufferSize, int cores)
        {
            _path = path;
            _streamFactory = streamFactory;
            _bufferSize = bufferSize;
            _cores = cores;

            _mmf = MemoryMappedFile.CreateFromFile(_path);
            _stream = _mmf.CreateViewStream();
        }

        public void ReadParallel(Action<PortionChunkReader[]> callback)
        {
            const int cores = 4;
            var threads = new ExThread[cores];
            var chunkReaders = new PortionChunkReader[cores];

            for (var i = 0; i < cores; i++)
            {
                chunkReaders[i] = new PortionChunkReader(i);
                threads[i] = new ExThread(ReadDataFromSource);
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

            // create a separate stream 

            var sourceSize = _stream.Length;

            // the size which should process one thread
            var currentThreadSize = (sourceSize / _cores);
            var endSize = (reader.Index == _cores - 1) ? 0 : currentThreadSize;

            // before end reading set position
            using var sourceStream = _mmf.CreateViewStream(reader.Index * currentThreadSize, endSize);

            // save size
            reader.SetData(BitConverter.GetBytes(sourceStream.Length), 8);

            //var done = 0L;
            var buffer = new byte[_bufferSize];
            int read;

            do
            {
                read = sourceStream.Read(buffer);
                reader.SetData(buffer, read);
            } while (read > 0);
        }

        public void Dispose()
        {
            _stream.Dispose();
            _mmf.Dispose();
        }
    }
}
