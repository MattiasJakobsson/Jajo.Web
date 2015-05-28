using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.CommandSender
{
    public interface IHandleCommand<in TCommand>
    {
        Task Handle(TCommand command, IDictionary<string, object> environment);
    }
}