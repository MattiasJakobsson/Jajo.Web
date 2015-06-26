using System;
using System.Threading.Tasks;

namespace SuperGlue.UnitOfWork
{
    public interface IApplicationTask
    {
        Task Start();

        Task ShutDown();

        Task Exception(Exception exception);
    }
}