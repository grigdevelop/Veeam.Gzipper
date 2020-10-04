using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Types;

namespace Veeam.Gzipper.Tests
{
    [TestClass]
    public class StreamChunkReaderTests
    {
        [TestMethod]
        public void ShouldSplitChunksWithRightIndexes()
        {
            //const string origin = "1020304050";

            //using var sourceStream = new MemoryStream(Encoding.Default.GetBytes(origin));
            //using var csr = new StreamChunkReader(sourceStream, 2, 100);

            //var forLock = new object();
            //var chunks = new List<Chunk>();

            //csr.Read((chunk) =>
            //{
            //    lock (forLock)
            //    {
            //        chunks.Add(chunk);
            //    }
            //});

            //var chunksStrings = chunks.OrderBy(x => x.Index).Select(x => Encoding.Default.GetString(x.Data.Skip(8).ToArray()));
            //var result = string.Join(string.Empty, chunksStrings);

            //result.Should().BeEquivalentTo(origin);
        }

        [TestMethod]
        public void TestThread()
        {
            var thread = new Thread(() =>
            {
                Console.WriteLine("Running thread: " + Thread.CurrentThread.ManagedThreadId);
            });

            thread.Start();
            while (thread.IsAlive) { }
            thread.Start();
        }
    }
}
