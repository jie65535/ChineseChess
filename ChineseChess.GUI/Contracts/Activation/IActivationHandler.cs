using System.Threading.Tasks;

namespace ChineseChess.GUI.Contracts.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle();

        Task HandleAsync();
    }
}
