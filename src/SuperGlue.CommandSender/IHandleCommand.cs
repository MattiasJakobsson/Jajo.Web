using System.Threading.Tasks;

namespace SuperGlue.CommandSender
{
    public interface IHandleCommand<in TCommand>
    {
        Task Handle(TCommand command);
    }
}