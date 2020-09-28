namespace Veeam.Gzipper.Core.Streams.Types
{
    /// <summary>
    /// Represents already chunked binary data
    /// </summary>
    public class ChunkedData
    {
        /// <summary>
        /// Gets a position in source file
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Gets binary data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Creates an instance of <see cref="ChunkedData"/>
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="data">Binary data</param>
        public ChunkedData(long position, byte[] data)
        {
            Position = position;
            Data = data;
        }
    }
}
