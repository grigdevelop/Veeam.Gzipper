using System.IO;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;

namespace Veeam.Gzipper.Core.Streams.Factory
{
    public class CompressorStreamFactory : IStreamFactory
    {
        public Stream CreateSourceFileStream(string sourceFilePath)
        {
            return File.OpenRead(sourceFilePath);
        }

        public Stream CreateTargetFileStream(string targetFilePath)
        {
            return File.Open(targetFilePath, FileMode.Create, FileAccess.Write);
        }
    }
}
