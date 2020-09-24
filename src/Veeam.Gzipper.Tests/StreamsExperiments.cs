using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.Gzipper.Tests
{
    [TestClass]
    public class StreamsExperiments
    {
        private static readonly object lobj = new object();

        [TestMethod]
        public void TestCompress()
        {
            // we have string 60 length and 60 size
            const string numbers = "0123456789ABCDEFGHIJ";
            var sb = new StringBuilder(60);
            for (var i = 0; i < 3; i++)
            {
                sb.Append(numbers);
            }
            var text = sb.ToString();
            var data = Encoding.Default.GetBytes(text);
            
            // var buffer = new byte[14];
            // use memory stream instead of file stream
            using (Stream memoryStream = new MemoryStream(data))
            {
                var tasks = new Task[12];
                for (int i = 0; i < 12; i++)
                {
                    var local = i;
                    tasks[i] = (new Task(() =>
                   {
                       Task<int> task;
                       var buff = new byte[9];
                       lock (lobj)
                       {
                           var number = BitConverter.GetBytes(local);
                           number.CopyTo(buff, 0);

                           //memoryStream.Read(buff, 4, 5);
                           memoryStream.Seek(local * 5, SeekOrigin.Begin);
                           task = memoryStream.ReadAsync(buff, 4, 5);
                       }

                       var result = task.Result;
                       Console.WriteLine(Encoding.Default.GetString(buff, 4, 5) + "|" + BitConverter.ToInt32(buff, 0));

                   }));
                }

                foreach (var task in tasks)
                {
                    task.Start();
                }

                Task.WaitAll(tasks);
            }
        }


    }
}
