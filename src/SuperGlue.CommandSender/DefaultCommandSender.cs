using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.CommandSender
{
    public class DefaultCommandSender : ISendCommand
    {
        private readonly IDictionary<string, object> _environment;

        public DefaultCommandSender(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public Task Send<TCommand>(TCommand command)
        {
            var environment = _environment.ToDictionary(x => x.Key, x => x.Value);
            environment[EnvironmentCommandExtensions.CommandConstants.CurrentCommand] = command;

            return CommandPipeline.CurrentPipeline(environment);
        }
    }
}