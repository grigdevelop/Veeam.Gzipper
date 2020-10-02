using System;
using System.Linq;
using System.Text;
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
                //Console.WriteLine("GETTING: " + Encoding.Default.GetString(_data ?? new byte[0]));
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
                //Console.WriteLine("PUSHING: " + Encoding.Default.GetString(data ?? new byte[0]));
                if (read > 0 && data != null)
                {
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

    public class ConcurReadWrite
    {
        private readonly AutoResetEvent _readerReset = new AutoResetEvent(false);
        private readonly AutoResetEvent _setterReset = new AutoResetEvent(true);
        private readonly object lockForRead = new object();
        private readonly object lockForSet = new object();

        private byte[] _data;

        public void Read(byte[] buffer)
        {
            lock (lockForRead)
            {
                _readerReset.WaitOne();
                _data?.CopyTo(buffer, 0);
                _setterReset.Set();
            }
        }

        public void SetData(byte[] data)
        {
            lock (lockForSet)
            {
                _setterReset.WaitOne();
                _data = data;
                _readerReset.Set();
            }

        }
    }
}
