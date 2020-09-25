using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //MyApp app = new MyApp();
            //app.Start();
            var text = "1234567890123456789012345678901234567890";
            var bytes = Encoding.Default.GetBytes(text);
            var rd = new Random();
            using(var sourceStream = new MemoryStream(bytes))
            {
                using(var reader = new StreamChunkReader(sourceStream, 3, 10))
                {
                    reader.Read(chunk =>
                    {  
                        Console.WriteLine($"Started: {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(rd.Next(100, 2000));
                        Console.WriteLine($"Ended: {chunk.Index} | {Encoding.Default.GetString(chunk.Data.Skip(4).ToArray())}");
                    });
                    
                }
                
            }

            Console.WriteLine("Hello World!");
        }
    }
}
