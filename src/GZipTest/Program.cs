using System;
using Veeam.Gzipper.Cmd.IO;
using Veeam.Gzipper.Cmd.Logging;
using Veeam.Gzipper.Core;
using Veeam.Gzipper.Core.IO;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams.Factory;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;

namespace GZipTest
{
    class Program
    {

        static void Main(string[] args)
        {
            // declare dependencies
            IInputOutput io = new ConsoleInputOutput();
            ILogger logger = new ConsoleLogger();
            IStreamFactory streamFactory = new CompressorStreamFactory();


            var app = new CompressorApplication(io, logger, streamFactory);
            //args = new[] { "compress", "source.mp4", "target.zip" };
            app.Execute(args);
            //args = new[] { "decompress", "target.zip", "source_decompressed.mp4" };
            //app.Execute(args);

            Console.WriteLine();
        }
    }
}
