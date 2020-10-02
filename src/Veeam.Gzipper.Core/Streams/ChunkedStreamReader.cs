using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Veeam.Gzipper.Core.Streams.Types;

namespace Veeam.Gzipper.Core.Streams
{

    public class ChunkedStreamReader
    {
        private readonly Stream _stream;
        private readonly int _bufferSize;

        public ChunkedStreamReader(Stream stream, int bufferSize)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _bufferSize = bufferSize;
        }

        public void ReadParallel(Action<PortionChunkReader> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var chunkReaders = new Dictionary<int, PortionChunkReader>();
            for (var i = 0; i < 4; i++)
            {
                var chunkReader = new PortionChunkReader(i);
                chunkReaders.Add(i, chunkReader);
                new Thread((obj) => callback((PortionChunkReader)obj)).Start(chunkReader);

            }

            const int indexSize = sizeof(int);
            var buffer = new byte[_bufferSize + indexSize];

            var read = _stream.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                var index = BitConverter.ToInt32(buffer, 0);
                chunkReaders[index].SetData(buffer, read);

                read = _stream.Read(buffer, 0, buffer.Length);
            }

            foreach (var portionChunkReader in chunkReaders)
            {
                //portionChunkReader.Value.SetData(null);
            }
        }
    }


}
