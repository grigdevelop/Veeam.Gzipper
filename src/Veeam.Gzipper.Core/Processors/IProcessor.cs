using Veeam.Gzipper.Core.Types;

namespace Veeam.Gzipper.Core.Processors
{
    public interface IProcessor
    {
        void StartSync(GzipperInputData input);
    }
}
