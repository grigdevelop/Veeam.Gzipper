using System;

namespace Veeam.Gzipper.Core.Streams.Types
{
    /// <summary>
    /// Represents indexed chunk of binary data
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// Gets binary data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets index
        /// </summary>
        public long Index { get; }

        public const int INDEX_SIZE = 8; // 8 bytes for long

        /// <summary>
        /// Create an instance of <see cref="Chunk"/>
        /// </summary>
        /// <param name="bufferSize">Binary buffer size</param>
        /// <param name="index">Index</param>
        public Chunk(int bufferSize, long index) :
            this(new byte[bufferSize + INDEX_SIZE], index)
        {
        }

        private Chunk(byte[] data, long index)
        {
            Index = index;
            var bytes = BitConverter.GetBytes(index);
            bytes.CopyTo(data, 0);

            Data = data;

        }

        /// <summary>
        /// Makes index negative so chunk reader will understand that this chunk have empty slots
        /// </summary>
        public void MarkAsWithEmptySlots()
        {
            var bytes = BitConverter.GetBytes(-Index);
            bytes.CopyTo(Data, 0);
        }
    }
}
