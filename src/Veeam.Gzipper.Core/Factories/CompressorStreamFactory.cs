using System.IO;

namespace Veeam.Gzipper.Core.Factories
{
    public class CompressorStreamFactory : ICompressorStreamFactory
    {
        public Stream CreateSourceFileStream(string sourceFilePath)
        {
            return File.OpenRead(sourceFilePath);
        }

        public Stream CreateTargetFileStream(string targetFilePath)
        {
            return File.Open(targetFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        }
    }
}
