using System;
using System.IO;

namespace Veeam.Gzipper.Core.Types
{
    public class GzipperInputData
    {
        public GzipperAction Action { get; }

        public string ZipFilePath { get; }

        public string SourceOrTargetFilePath { get; }

        public GzipperInputData(GzipperAction action, string zipFilePath, string sourceOrTargetFilePath)
        {
            if (string.IsNullOrEmpty(zipFilePath))
                throw new InvalidDataException(nameof(zipFilePath));
            if (string.IsNullOrEmpty(sourceOrTargetFilePath))
                throw new InvalidDataException(nameof(sourceOrTargetFilePath));

            Action = action;
            ZipFilePath = zipFilePath;
            SourceOrTargetFilePath = sourceOrTargetFilePath;
        }
    }
}
