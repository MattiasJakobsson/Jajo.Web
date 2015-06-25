using System;
using System.Threading.Tasks;

namespace SuperGlue.UnitOfWork
{
    public interface ISuperGlueUnitOfWork
    {
        Task Begin();
        Task Commit();
        Task Rollback(Exception exception = null);
    }
}