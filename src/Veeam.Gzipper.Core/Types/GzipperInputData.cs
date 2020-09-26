using System;
using System.IO;

namespace Veeam.Gzipper.Core.Types
{
    public class GzipperInputData
    {
        public GzipperAction Action { get; }

        public string SourceFilePath { get; }

        public string TargetFilePath { get; }

        public GzipperInputData(GzipperAction action, string sourceFilePath, string targetFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
                throw new InvalidDataException(nameof(sourceFilePath));
            if (string.IsNullOrEmpty(targetFilePath))
                throw new InvalidDataException(nameof(targetFilePath));

            Action = action;
            SourceFilePath = sourceFilePath;
            TargetFilePath = targetFilePath;
        }
    }
}
