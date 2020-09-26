using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Veeam.Gzipper.Core.Utilities;

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
    

   
}
