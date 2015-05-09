using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public class ExecuteActionEndpoint : IExecuteTypeOfEndpoint<Action>
    {
        public async Task Execute(Action endpoint, IDictionary<string, object> environment)
        {
            endpoint();
        }
    }
}