using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Veeam.Gzipper.Core.Streams
{
    public class PartiallyFileReader : IDisposable
    {
        private readonly MemoryMappedFile _mmf;

        public long SourceLength { get; }

        public long Capacity { get; }

        public int BufferSize { get; }

        public PartiallyFileReader(string path, int bufferSize, int capacity)
        {
            BufferSize = bufferSize;
            Capacity = capacity;

            using (var fs = File.OpenRead(path))
            {
                SourceLength = fs.Length;
            }

            _mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        }

        public Stream CreatePartialStream(int part)
        {
            // the size which should process one thread
            var currentThreadSize = (SourceLength / Capacity);
            currentThreadSize = (currentThreadSize / BufferSize) * BufferSize;
            var portionSize = currentThreadSize;
            if (part == Capacity - 1)
            {
                portionSize = SourceLength - (currentThreadSize * (Capacity - 1));
            }
            var offset = part * currentThreadSize;

            return _mmf.CreateViewStream(offset, portionSize, MemoryMappedFileAccess.Read);
        }


        public void Dispose()
        {
            _mmf.Dispose();
        }
    }
}
