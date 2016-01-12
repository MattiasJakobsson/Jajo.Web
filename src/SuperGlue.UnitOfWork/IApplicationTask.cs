using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.UnitOfWork
{
    public interface IApplicationTask
    {
        Task Start(IDictionary<string, object> environment);

        Task ShutDown(IDictionary<string, object> environment);

        Task Exception(IDictionary<string, object> environment, Exception exception);
    }
}