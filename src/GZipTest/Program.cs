using System;
using System.Threading;
using Veeam.Gzipper.Cmd.IO;
using Veeam.Gzipper.Cmd.Logging;
using Veeam.Gzipper.Core;
using Veeam.Gzipper.Core.Configuration;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.IO;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams.Factory;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Streams.Types;

namespace GZipTest
{
    class Program
    {

        static void _Main(string[] args)
        {
            var reader = new ConcurReadWrite();
            var rd = new Random();
            var work = true;

            var readBuffer = new byte[5];
            var writeBuffer = new byte[5];

            var readerThread = new Thread(() =>
            {
                while (work)
                {
                    Console.WriteLine("READER: Trying to read data");
                    reader.Read(readBuffer);
                    Thread.Sleep(rd.Next(1, 10));

                    Console.WriteLine("READER: Read done");
                }

            });

            var setterThread = new Thread(() =>
            {
                //Thread.Sleep(1000);
                while (work)
                {
                    Thread.Sleep(rd.Next(1, 10));
                    Console.WriteLine("SETTER: Trying to set data");
                    reader.SetData(writeBuffer);
                    Console.WriteLine("SETTER: Set done");
                }
            });

            readerThread.Start();
            setterThread.Start();

            Console.ReadLine();
            work = false;
        }

        static void Main(string[] args)
        {
            // dependencies
            IInputOutput io = new ConsoleInputOutput();
            ILogger logger = new ConsoleLogger();
            IStreamFactory streamFactory = new CompressorStreamFactory();
            ICompressorSettings settings = new CompressorSettings();

            var app = new CompressorApplication(io, logger, streamFactory, settings);
            app.Execute(args);

            Console.ReadLine();
        }
    }
}
