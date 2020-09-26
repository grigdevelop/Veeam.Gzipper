using System;
using Veeam.Gzipper.Core.Abstractions;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.Types;

namespace Veeam.Gzipper.Core.Utilities
{
    public class GzipperUserInputReader
    {
        private readonly IInputOutput _inOut;

        public GzipperUserInputReader(IInputOutput inOut)
        {
            _inOut = inOut ?? throw new ArgumentNullException(nameof(inOut));
        }

        public GzipperInputData ParseUserInputData(string[] args)
        {
            var compressDecompress = args.Length == 0 ? _inOut.ReadLine(MessageConstants.EnterCompressionMethodMessage) : args[0];
            switch (compressDecompress)
            {
                case "compress":
                    {
                        var sourcePath = args.Length < 2 ? _inOut.ReadLine(MessageConstants.EnterSourceMessage) : args[1];
                        var zipPath = args.Length < 3 ? _inOut.ReadLine(MessageConstants.EnterTargetMessage) : args[2];
                        return new GzipperInputData(GzipperAction.Compress, zipPath, sourcePath);
                    }
                case "decompress":
                    {
                        var zipPath = args.Length < 2 ? _inOut.ReadLine(MessageConstants.EnterSourceMessage) : args[1];
                        var targetPath = args.Length < 3 ? _inOut.ReadLine(MessageConstants.EnterTargetMessage) : args[2];
                        return new GzipperInputData(GzipperAction.Decompress, zipPath, targetPath);
                    }
                default:
                    throw new InvalidOperationException($"Unknown '{compressDecompress}' entered.");
            }
        }
    }
}
