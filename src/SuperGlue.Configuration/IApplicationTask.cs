using System;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public interface IApplicationTask
    {
        Task Start();

        Task ShutDown();

        Task Exception(Exception exception);
    }
}