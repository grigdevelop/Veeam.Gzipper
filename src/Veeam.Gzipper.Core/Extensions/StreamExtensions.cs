using System;
using System.IO;

namespace Veeam.Gzipper.Core.Extensions
{
    public static class StreamExtensions
    {
        public static long ReadInt64(this Stream stream)
        {
            var buffer = new byte[sizeof(long)];
            stream.Read(buffer);
            return BitConverter.ToInt64(buffer);
        }

        public static long ReadInt32(this Stream stream)
        {
            var buffer = new byte[sizeof(int)];
            stream.Read(buffer);
            return BitConverter.ToInt32(buffer);
        }
    }
}
