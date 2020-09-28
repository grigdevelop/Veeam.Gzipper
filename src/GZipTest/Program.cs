using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
using Veeam.Gzipper.Core.Threads.Limitations;

namespace GZipTest
{
    class Program
    {

        static void Main_(string[] args)
        {
            using var sourceStream = File.OpenRead("target.zip");
            using var zipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            var skip = new byte[8];
            zipStream.Read(skip, 0, skip.Length);

            var stl = new SyncLimitedThreads(i =>
            {
                Console.WriteLine("starting: " + i);

                var buffer = new byte[20 + 8];
                zipStream.Read(buffer, 0, buffer.Length);

                var index = BitConverter.ToInt64(buffer, 0);
                var buf = buffer.Skip(8).ToArray();

                Console.WriteLine(Encoding.Default.GetString(buf) + " | " + index);
                Thread.Sleep(1000);
            }, 3, 10);
            stl.StartSync();

            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            // declare dependencies
            IInputOutput io = new ConsoleInputOutput();
            ILogger logger = new ConsoleLogger();
            IStreamFactory streamFactory = new CompressorStreamFactory();
            ICompressorSettings settings = new CompressorSettings();


            var app = new CompressorApplication(io, logger, streamFactory, settings);
            //args = new[] { "compress", "source.mp4", "target.zip" };
            app.Execute(args);
            //args = new[] { "decompress", "target.zip", "source_decompressed.mp4" };
            //app.Execute(args);

            Console.WriteLine();
        }
    }
}
