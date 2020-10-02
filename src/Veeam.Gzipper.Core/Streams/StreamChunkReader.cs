using System;
using System.IO;
using System.Threading;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads;

namespace Veeam.Gzipper.Core.Streams
{
    public class StreamChunkReader
    {
        private readonly string _path;
        private readonly IStreamFactory _streamFactory;
        private readonly int _bufferSize;

        public StreamChunkReader(string path, IStreamFactory streamFactory, int bufferSize)
        {
            _path = path;
            _streamFactory = streamFactory;
            _bufferSize = bufferSize;
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
            using var sourceStream = _streamFactory.CreateSourceFileStream(_path);

            var sourceSize = sourceStream.Length;

            // the size which should process one thread
            var currentThreadSize = (sourceSize / 4);

            // for last portion read until the end
            if (reader.Index == 3)
            {
                var diff = (sourceSize % 4);
                if (diff > 0)
                    currentThreadSize += diff;
            }

            // before end reading set position
            var startPosition = reader.Index * currentThreadSize;
            sourceStream.Seek(startPosition, SeekOrigin.Begin);

            // save size
            reader.SetData(BitConverter.GetBytes(currentThreadSize), 8);

            var done = 0L;
            var buffer = new byte[_bufferSize];
            int read;

            while (done < currentThreadSize - _bufferSize)
            {
                read = sourceStream.Read(buffer, 0, buffer.Length);
                reader.SetData(buffer, read);
                done += read;
            }

            var left = (int)(currentThreadSize - done);
            buffer = new byte[left];
            read = sourceStream.Read(buffer, 0, left);
            reader.SetData(buffer, read);

            Console.WriteLine($"THREAD {reader.Index}: Is Done");

            reader.SetData(null, 0);
        }
    }
}
