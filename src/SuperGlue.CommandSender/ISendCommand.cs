using System.Threading.Tasks;

namespace SuperGlue.CommandSender
{
    public interface ISendCommand
    {
        Task Send<TCommand>(TCommand command);
    }
}