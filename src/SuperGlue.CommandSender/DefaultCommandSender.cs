using System;
using System.Collections.Generic;
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

        public async Task Send<TCommand>(TCommand command)
        {
            var commandId = Guid.NewGuid();

            using (_environment.OpenCommandContext(command, commandId))
            using (_environment.OpenCausationContext(commandId.ToString()))
            {
                await CommandPipeline.CurrentPipeline(_environment);
            }
        }
    }
}