using Veeam.Gzipper.Core.Configuration.Types;

namespace Veeam.Gzipper.Core.Commands
{
    public interface ICommand
    {
        void StartSync(UserInputData input);
    }
}
