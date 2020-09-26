using System;
using System.Collections.Concurrent;
using System.Threading;
using Veeam.Gzipper.Cmd.Concrete;
using Veeam.Gzipper.Core.Factories;
using Veeam.Gzipper.Core.Processors;
using Veeam.Gzipper.Core.Types;
using Veeam.Gzipper.Core.Utilities;

namespace GZipTest
{
    class Program
    {
        public static Random rd = new Random();
        public class SltState
        {
            private readonly ConcurrentDictionary<Thread, byte> _threads;
            private readonly InterruptChecker _ic;
            public int Index { get; }

            public SltState(ConcurrentDictionary<Thread, byte> threads, InterruptChecker ic, int index)
            {
                _threads = threads;
                _ic = ic;
                Index = index;
            }

            public void Release()
            {
                while (_threads.TryRemove(Thread.CurrentThread, out _))
                {

                }
            }

            public void Interrupt(Exception ex = null)
            {
                _ic.SetException(ex);
                _ic.Interrupt();
                Release();
                foreach (var t in _threads)
                {
                    t.Key.Interrupt();
                }

            }
        }

        public class InterruptChecker
        {
            public bool Interrupted { get; private set; }

            public Exception Error { get; private set; }

            public void Interrupt()
            {
                Interrupted = true;
            }

            public void SetException(Exception exception)
            {
                Error = exception;
            }
        }

        static void Main(string[] args)
        {
            var threads = new ConcurrentDictionary<Thread, byte>();
            var ic = new InterruptChecker();

            var activeMax = 3;
            var max = 20;
            for (var i = 0; i < max; i++)
            {
                var t = new Thread(ThreadMethod);
                while (threads.TryAdd(t, 0))
                {

                }
                t.Start(new SltState(threads, ic, i));
                while (threads.Count >= activeMax)
                {

                }
                if (ic.Interrupted) break;
            }
            while (threads.Count > 0)
            {

            }
            if (ic.Error != null)
            {
                Console.WriteLine(ic.Error.Message);
            }


            Console.WriteLine("Ending");

            //args = new[] { "compress", "source.pdf", "target.zip" };

            //ZipMain(args);

            //args = new[] { "decompress", "target.zip", "source_decompressed.pdf" };

            //ZipMain(args);
        }

        static void ThreadMethod(object arg)
        {
            var state = (SltState)arg;
            try
            {

                var i = 0;
                while (i++ < 5)
                {
                    Console.WriteLine("thread still running: " + Thread.CurrentThread.ManagedThreadId + " | " + state.Index);
                    Thread.Sleep(rd.Next(200, 1000));
                }
                if (state.Index == 5 || state.Index == 4) throw new UnauthorizedAccessException();

                state.Release();
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine("oho interruped");
                state.Release();
            }
            catch (Exception e)
            {
                Console.WriteLine("Oho, error happanned");
                state.Interrupt(e);
            }
        }

        static void ZipMain(string[] args)
        {
            IStreamFactory streamFactory = new CompressorStreamFactory();
            var input = new GzipperUserInputReader(new ConsoleInputOutput()).ParseUserInputData(args);

            // new c# features :) 
            IProcessor processor = input.Action switch
            {
                GzipperAction.Compress => new CompressProcessor(streamFactory),
                GzipperAction.Decompress => new DecompressProcessor(streamFactory),
                _ => throw new InvalidCastException()
            };
            processor.StartSync(input);
        }
    }
}
