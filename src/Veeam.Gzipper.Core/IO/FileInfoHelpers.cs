using System.IO;

namespace Veeam.Gzipper.Core.IO
{
    public static class FileInfoHelpers
    {
        public static StreamPosition[] GetPartialSizeList(this long length, int capacity, int bufferSize)
        {
            if (bufferSize >= length)
            {
                return new[] { new StreamPosition(0, length),  };
            }

            var sizeList = new StreamPosition[capacity];

            for (var i = 0; i < capacity; i++)
            {
                var size = (length / capacity);
                size = (size / bufferSize) * bufferSize;
                var offset = size * i;
                if (i == capacity - 1)
                {
                    size = length - (size * (capacity - 1));
                }
                
                sizeList[i] = new StreamPosition(offset, size);                    
                

            }

            return sizeList;
        }
    }

    public class StreamPosition
    {
        public long Offset { get; }
        public long Size { get; }

        public StreamPosition(long offset, long size)
        {
            Offset = offset;
            Size = size;
        }
    }
}
