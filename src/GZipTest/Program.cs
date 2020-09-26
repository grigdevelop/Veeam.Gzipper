using System;
using System.IO;
using System.Linq;
using System.Text;
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
        static void Main(string[] args)
        {
            args = new[] { "compress", "source.mp4", "target.zip" };

            ZipMain(args);

            args = new[] { "decompress", "target.zip", "source_decompressed.mp4" };

            ZipMain(args);
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
