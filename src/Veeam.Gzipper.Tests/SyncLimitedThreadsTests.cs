using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Veeam.Gzipper.Core.Utilities;

namespace Veeam.Gzipper.Tests
{
    [TestClass]
    public class SyncLimitedThreadsTests
    {
        [TestMethod]
        public void ShouldValidateParameters()
        {
            var slt = new SyncLimitedThreads(i => {}, 1, 10);
        }
    }
}
