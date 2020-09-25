using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GZipTest
{
    public class MyState
    {
        private readonly int _maxCount;
        private readonly Semaphore _sm;
        public int Current { get; private set; }
        public bool Done { get; private set; }

        public MyState(int activeCount, int maxCount)
        {
            Current = 0;
            _maxCount = maxCount;
            _sm = new Semaphore(activeCount, activeCount);
        }

        public bool Next()
        {
            if (Current < _maxCount - 1)
            {
                Current++;
                return true;
            }
            else
            {
                Done = true;
                return false;
            }
        }

        public void Wait()
        {
            _sm.WaitOne();
        }

        public void Release()
        {
            _sm.Release();
        }
    }

    public class MyApp
    {

        public void Start()
        {
            var state = new MyState(100, 2000);
            new Thread(RecStart).Start(state);
            while (!state.Done)
            {
                Thread.Sleep(100);
            }
            //t.Join();
        }

        public void RecStart(object arg)
        {
            var state = (MyState)arg;
            Console.WriteLine("Started new thread: " + state.Current + " | Pool: " + Thread.CurrentThread.ManagedThreadId);
            state.Wait();
            if (!state.Next()) return;
            new Thread(RecStart).Start(state);

            var rd = new Random();
            Thread.Sleep(rd.Next(100, 200));

            state.Release();
        }
    }

    public class SyncLimitedThreads
    {
        private readonly Action<int> _execute;
        private readonly int _maxCount;
        private readonly Semaphore _semaphore;

        public SyncLimitedThreads(Action<int> execute, int activeCount, int maxCount)
        {
            _execute = execute;
            _maxCount = maxCount;
            _semaphore = new Semaphore(activeCount, activeCount);
        }

        public void StartSync()
        {
            new Thread(ActionStarter).Start(0);
        }

        private void ActionStarter(object arg)
        {
            var n = (int)arg;
            if (n >= _maxCount)
            {
                return;
            }

            _semaphore.WaitOne(); // limit active threads

            // call next one if limit is not over
            new Thread(ActionStarter).Start(n + 1);

            // execute action sync
            _execute.Invoke(n);

            _semaphore.Release();
        }
    }

    public class Chunk
    {
        public byte[] Data { get; }

        public int Index { get; }

        public const int INDEX_SIZE = 4; // 4 bytes for integer

        private Chunk(byte[] data, int index)
        {
            Index = index;
            var bytes = BitConverter.GetBytes(index);
            bytes.CopyTo(data, 0); // first 4 bytes for index

            Data = data;
            
        }

        public Chunk(int bufferSize, int index) :
            this(new byte[bufferSize + INDEX_SIZE], index)
        {
        }
    }

    public class StreamChunkReader : IDisposable
    {
        private readonly int _maxThreadsLimit;
        private readonly int _actualThreadsLimit;
        private readonly int _bufferSize;

        public Stream BaseStream { get; }


        public StreamChunkReader(Stream stream, int bufferSize, int allocatedMemoryLimitSize)
        {
            BaseStream = stream;
            _bufferSize = bufferSize;

            // count how many threads will work totally
            var maxThreadsLimit = (int)(BaseStream.Length / bufferSize);
            if (BaseStream.Length % bufferSize > 0) maxThreadsLimit++;
            _maxThreadsLimit = maxThreadsLimit;

            // count how many threads will work at time
            var actualThreadsLimit = allocatedMemoryLimitSize / bufferSize;
            if (actualThreadsLimit == 0) actualThreadsLimit++;
            _actualThreadsLimit = actualThreadsLimit;
        }

        public void Read(Action<Chunk> callback)
        {
            var threadsDone = 0;
            var slt = new SyncLimitedThreads(i =>
            {
                // create buffer and start reading
                var chunk = new Chunk(_bufferSize, i);
                var asyncResult = BaseStream.BeginRead(chunk.Data, Chunk.INDEX_SIZE, _bufferSize, null!, null);

                // before end reading set position
                BaseStream.Seek(i * _bufferSize, SeekOrigin.Begin);
                BaseStream.EndRead(asyncResult);
                callback.Invoke(chunk);
                threadsDone++;
            }, _actualThreadsLimit, _maxThreadsLimit);

            slt.StartSync();


            // StartSync part itself will work synchronously
            // But for handling callbacks need additional functionality
            while (threadsDone != _maxThreadsLimit)
            {

            }
        }

        public void Dispose()
        {
            //BaseStream?.Dispose();
        }
    }
}
