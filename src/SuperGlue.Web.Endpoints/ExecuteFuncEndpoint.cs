using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteFuncEndpoint : IExecuteTypeOfEndpoint<Func<Task>>
    {
        public async Task Execute(Func<Task> endpoint, IDictionary<string, object> environment)
        {
            await endpoint();
        }
    }
}