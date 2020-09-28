using System;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.Constants;
using Veeam.Gzipper.Core.IO;

namespace Veeam.Gzipper.Core.Streams
{
    public class GzipperUserInputReader
    {
        private readonly IInputOutput _inOut;

        public GzipperUserInputReader(IInputOutput inOut)
        {
            _inOut = inOut ?? throw new ArgumentNullException(nameof(inOut));
        }

        public UserInputData ParseUserInputData(string[] args)
        {
            var compressDecompress = args.Length == 0 ? _inOut.ReadLine(MessageConstants.EnterCompressionMethodMessage) : args[0];
            UserAction action = compressDecompress switch
            {
                "compress" => UserAction.Compress,
                "decompress" => UserAction.Decompress,
                _ => throw new InvalidOperationException($"Unknown command '{compressDecompress}' entered.")
            };

            var sourcePath = args.Length < 2 ? _inOut.ReadLine(MessageConstants.EnterSourceMessage) : args[1];
            var targetPath = args.Length < 3 ? _inOut.ReadLine(MessageConstants.EnterTargetMessage) : args[2];

            return new UserInputData(action, sourcePath, targetPath);
        }
    }
}
