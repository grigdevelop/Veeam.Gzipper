using System.Linq;
using System.Threading;

namespace Veeam.Gzipper.Core.Streams.Types
{
    public class PortionChunkReader
    {
        private readonly AutoResetEvent _readerReset = new AutoResetEvent(false);
        private readonly AutoResetEvent _setterReset = new AutoResetEvent(true);
        private readonly object lockForRead = new object();
        private readonly object lockForSet = new object();

        private byte[] _data;
        private int _read;

        public int Index { get; }


        public PortionChunkReader(int index)
        {
            Index = index;
        }

        public int Read(byte[] buffer)
        {
            lock (lockForRead)
            {
                _readerReset.WaitOne();
                _data?.CopyTo(buffer, 0);
                var readLocal = _read;
                _setterReset.Set();
                return readLocal;
            }
        }

        public void SetData(byte[] data, int read)
        {
            lock (lockForSet)
            {
                _setterReset.WaitOne();
                if (read > 0 && data != null)
                {
                    if (read < data.Length)
                    {
                        data = data.Take(read).ToArray();
                    }

                    _data = new byte[read];
                    data.CopyTo(_data, 0);
                }
                else
                {
                    _data = null;
                }

                _read = read;
                _readerReset.Set();
            }
        }
    }
}
