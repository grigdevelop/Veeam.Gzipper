using System;
using System.IO;
using System.IO.MemoryMappedFiles;
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

        static void _Main(string[] args)
        {
            using var mmf = MemoryMappedFile.CreateFromFile("source.txt");
            using (var view = mmf.CreateViewStream(0, 20))
            {
                ShowStream(view);
            }

            Console.WriteLine("-----------------------------");

            using (var view = mmf.CreateViewStream(20, 20))
            {
                ShowStream(view);
            }
        }

        static void ShowStream(Stream stream)
        {
            var buffer = new byte[3];
            var read = stream.Read(buffer);
            while (read > 0)
            {
                Console.WriteLine(Encoding.Default.GetString(buffer));
                read = stream.Read(buffer);
            }
        }

        static void Main(string[] args)
        {
            args = new[] { "compress", "source.txt", "target.zip" };
            args = new[] { "decompress", "target.zip", "s.txt" };

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
