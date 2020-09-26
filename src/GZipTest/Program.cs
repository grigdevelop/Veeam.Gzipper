using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Veeam.Gzipper.Cmd.Concrete;
using Veeam.Gzipper.Core.Constants;
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
            args = new[] { "compress", "source.txt", "target.zip" };
            var cp = new CompressorProcessor(new CompressorStreamFactory());
            cp.ProceedSync(new GzipperUserInputReader(new ConsoleInputOutput()).ParseUserInputData(args));

            byte[] originalBuffer;
            using (var source = File.OpenRead("target.zip"))
            {
                using (var zip = new GZipStream(source, CompressionMode.Decompress))
                {
                    // get the original size of the source file
                    const int lengthSize = sizeof(long);
                    var sizeBuffer = new byte[lengthSize];
                    zip.Read(sizeBuffer, 0, lengthSize);
                    var size = BitConverter.ToInt64(sizeBuffer, 0);

                    // escape empty bytes
                    var emptySlotsSize = ProcessorConstants.CHUNK_SIZE -  (int)(size % ProcessorConstants.CHUNK_SIZE);
                    //zip.Read(emptyBuffer, 0, emptyBuffer.Length);

                    originalBuffer = new byte[size];

                    using (var memoryStream = new MemoryStream(originalBuffer))
                    {

                        //Console.WriteLine(" zip length" +zip.Length);
                        var buffer = new byte[Chunk.INDEX_SIZE + ProcessorConstants.CHUNK_SIZE];
                        var read = zip.Read(buffer, 0, buffer.Length);
                        while (read > 0)
                        {
                            var index = BitConverter.ToInt32(buffer, 0);
                            var chunkSIze = ProcessorConstants.CHUNK_SIZE;
                            if (index < 0)
                            {
                                chunkSIze -= emptySlotsSize;
                                index = -index;
                            }
                            memoryStream.Seek(index * ProcessorConstants.CHUNK_SIZE, SeekOrigin.Begin);
                            memoryStream.Write(buffer, Chunk.INDEX_SIZE, chunkSIze);

                            read = zip.Read(buffer, 0, buffer.Length);
                        }

                    }


                }
            }

            Console.WriteLine(Encoding.Default.GetString(originalBuffer));

            Console.WriteLine("Hello World!");
        }

        static void Test1()
        {
            var text = "1234567890123456789012345678901234567890";
            var bytes = Encoding.Default.GetBytes(text);
            var rd = new Random();
            using (var sourceStream = new MemoryStream(bytes))
            {
                using (var reader = new StreamChunkReader(sourceStream, 100, 120))
                {
                    reader.Read(chunk =>
                    {
                        Console.WriteLine($"Started: {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(rd.Next(100, 2000));
                        Console.WriteLine($"Ended: {chunk.Index} | {Encoding.Default.GetString(chunk.Data.Skip(4).ToArray())}");
                    });

                }

            }
        }
    }
}
