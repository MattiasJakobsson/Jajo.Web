using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.CommandSender;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Data
{
    public class StoreCommands : IWrapMiddleware<ExecuteCurrentCommand>
    {
        private readonly IRepository _repository;

        public StoreCommands(IRepository repository)
        {
            _repository = repository;
        }

        public Task<IEndThings> Begin(IDictionary<string, object> environment, Type middlewareType)
        {
            var command = environment.GetCurrentCommand();

            return command?.CommandObject == null ? Task.FromResult<IEndThings>(new EmptyEnd()) : Task.FromResult<IEndThings>(new AttacheCommandToRepository(_repository, command));
        }

        private class AttacheCommandToRepository : IEndThings
        {
            private readonly IRepository _repository;
            private readonly EnvironmentCommandExtensions.Command _command;

            public AttacheCommandToRepository(IRepository repository, EnvironmentCommandExtensions.Command command)
            {
                _repository = repository;
                _command = command;
            }

            public Task End()
            {
                _repository.Attache(_command.CommandObject, _command.CommandId, _command.CausedBy);

                return Task.CompletedTask;
            }
        }
    }
}