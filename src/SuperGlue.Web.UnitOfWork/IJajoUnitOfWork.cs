using System;

namespace SuperGlue.Web.UnitOfWork
{
    public interface ISuperGlueUnitOfWork
    {
        void Begin();
        void Commit();
        void Rollback(Exception exception = null);
    }
}