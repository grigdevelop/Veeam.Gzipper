using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Veeam.Gzipper.Core.Types
{
    public class CompressorChunkState
    {
        private readonly Stream _sourceStream;
        private readonly GZipStream _zipStream;

        public CompressorChunkState(
            Stream sourceStream, GZipStream zipStream
            )
        {
            _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _zipStream = zipStream ?? throw new ArgumentNullException(nameof(zipStream));
        }
    }
}
