using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Veeam.Gzipper.Core.Configuration;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.IO;
using Veeam.Gzipper.Core.Streams;

namespace Veeam.Gzipper.Tests
{
    /// <summary>
    /// DEMO unit tests for <see cref="GzipperUserInputReader"/> class
    /// </summary>
    [TestClass]
    public class GzipperUserInputReaderTests
    {
        private Mock<IInputOutput> _mockInputOutput;

        [TestInitialize]
        public void Setup()
        {
            _mockInputOutput = new Mock<IInputOutput>();
        }

        [TestMethod]
        [DataRow(new string[] { })]
        [DataRow(new[] { "compress" })]
        [DataRow(new[] { "compress", "file.txt" })]
        [DataRow(new[] { "compress", "file.txt", "file.gzip" })]
        public void ShouldAskForRequiredInputData(string[] args)
        {
            if (args.Length == 0)
                _mockInputOutput.Setup(x => x.ReadLine(MessageConstants.EnterCompressionMethodMessage))
                    .Returns("compress").Verifiable();

            if (args.Length < 2)
                _mockInputOutput.Setup(x => x.ReadLine(MessageConstants.EnterSourceMessage))
                    .Returns("file.txt").Verifiable();

            if (args.Length < 3)
                _mockInputOutput.Setup(x => x.ReadLine(MessageConstants.EnterTargetMessage))
                    .Returns("file.gzip").Verifiable();

            var inputReader = new GzipperUserInputReader(_mockInputOutput.Object);
            inputReader.ParseUserInputData(args);

            _mockInputOutput.Verify();
        }
    }
}
