using System.IO;

namespace Veeam.Gzipper.Core.IO
{
    public static class FileInfoHelpers
    {
        public static long[] GetPartialSizeList(this FileInfo fi, int capacity, int bufferSize)
        {
            var length = fi.Length;

            if (bufferSize >= length)
            {
                return new[] { length };
            }

            var sizeList = new long[capacity];

            for (var i = 0; i < capacity; i++)
            {
                var currentThreadSize = (length / capacity);
                currentThreadSize = (currentThreadSize / bufferSize) * bufferSize;

                if (i == capacity - 1)
                {
                    currentThreadSize = length - (currentThreadSize * (capacity - 1));
                }

                sizeList[i] = currentThreadSize;
            }

            return sizeList;
        }
    }
}
