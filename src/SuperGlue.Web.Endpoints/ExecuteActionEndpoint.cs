using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteActionEndpoint : IExecuteTypeOfEndpoint<Action>
    {
        public Task Execute(Action endpoint, IDictionary<string, object> environment)
        {
            endpoint();

            return Task.CompletedTask;
        }
    }
}