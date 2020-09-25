using System;

namespace Veeam.Gzipper.Core.Types
{
    /// <summary>
    /// Represents a chunk of data with his position in source
    /// </summary>
    public class DataChunk
    {
        public const int CHUNK_SIZE = 1024;

        /// <summary>
        /// Gets the binary of the chunk data
        /// Contains position and data
        /// </summary>
        public byte[] Data { get;  }

        public DataChunk(int position)
        {
            Data = new byte[CHUNK_SIZE];
            var positionBytes = BitConverter.GetBytes(position);
            positionBytes.CopyTo(Data, 0); // use first 4 byte for integer to save the position
        }
    }
}
