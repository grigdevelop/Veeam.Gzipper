using System;
using System.Collections;
using System.IO;
using System.Text;
using Veeam.Gzipper.Cmd.IO;
using Veeam.Gzipper.Cmd.Logging;
using Veeam.Gzipper.Core;
using Veeam.Gzipper.Core.Configuration;
using Veeam.Gzipper.Core.Configuration.Abstraction;
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
            args = new[] { "compress", "source.txt", "target.zip" };

            // dependencies
            IInputOutput io = new ConsoleInputOutput();
            ILogger logger = new ConsoleLogger();
            IStreamFactory streamFactory = new CompressorStreamFactory();
            ICompressorSettings settings = new CompressorSettings();
            var app = new CompressorApplication(io, logger, streamFactory, settings);
            app.Execute(args);

            Console.ReadLine();
            Stack s = new Stack();
        }
    }
}
