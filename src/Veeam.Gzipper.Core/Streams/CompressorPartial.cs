using System;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Veeam.Gzipper.Core.Streams
{
    public class CompressorPartial
    {
        private readonly MemoryMappedFile _mmf;
        private readonly string _targetFilePath;
        private readonly long _offset;
        private readonly long _size;
        private readonly int _bufferSize;

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private readonly CompressorAsyncResult _asyncResult;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _cancellationToken;

        private Exception _error;

        public CompressorPartial(MemoryMappedFile mmf, string targetFilePath, long offset, long size, int bufferSize)
        {
            _mmf = mmf;
            _targetFilePath = targetFilePath;
            _offset = offset;
            _size = size;
            _bufferSize = bufferSize;

            _asyncResult = new CompressorAsyncResult(_resetEvent);
        }

        public IAsyncResult Start()
        {
            _cancellationToken = _cancellationTokenSource.Token;

            var thread = new Thread(() =>
            {
                try
                {
                    Compress();
                }
                catch (Exception e)
                {
                    _error = e;
                    throw _error;
                }
                finally
                {
                    _asyncResult.Complete();
                }
            });
            thread.Start();

            return _asyncResult;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            Wait();
        }

        public bool Wait()
        {
            return _resetEvent.WaitOne();
        }

        private void Compress()
        {
            using var sourceStream = _mmf.CreateViewStream(_offset, _size, MemoryMappedFileAccess.Read);
            using var targetStream = File.Open(_targetFilePath, FileMode.Create, FileAccess.Write);
            using var compressorStream = new GZipStream(targetStream, CompressionMode.Compress);

            var buffer = new byte[_bufferSize];
            var readNum = sourceStream.Read(buffer);
            while (!_cancellationToken.IsCancellationRequested && readNum > 0)
            {
                compressorStream.Write(buffer, 0, readNum);
                readNum = sourceStream.Read(buffer);
            }
        }

        #region Neasted classes

        class CompressorAsyncResult : IAsyncResult
        {
            private readonly ManualResetEvent _resetEvent;

            // always null
            public object? AsyncState => null;
            public WaitHandle AsyncWaitHandle => _resetEvent;
            public bool CompletedSynchronously => false;
            public bool IsCompleted { get; private set; }

            public CompressorAsyncResult(ManualResetEvent resetEvent)
            {
                _resetEvent = resetEvent;
            }

            public void Complete()
            {
                _resetEvent.Set();
                this.IsCompleted = true;
            }
        }

        #endregion

    }
}
