using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using Veeam.Gzipper.Core.Threads.Limitations;

namespace Veeam.Gzipper.Tests
{
    [TestClass]
    public class SyncLimitedThreadsTests
    {
        [TestMethod]
        [Timeout(8000)]
        public void ShouldCallAsyncLimitedThreads()
        {
            // should 3 times call 3 threads and wait around one second on each call
            // and then one more thread and also one second
            // sum should sleep around 3+1=4 second

            var sw = new Stopwatch();
            sw.Start();

            var slt = new SyncLimitedThreads(i =>
            {
                Thread.Sleep(1000);
            }, 3, 10);
            slt.StartSync();

            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeCloseTo(4000, 100);
        }
    }
}
