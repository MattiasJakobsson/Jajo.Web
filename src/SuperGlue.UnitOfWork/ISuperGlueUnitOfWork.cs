using System;

namespace SuperGlue.UnitOfWork
{
    public interface ISuperGlueUnitOfWork
    {
        void Begin();
        void Commit();
        void Rollback(Exception exception = null);
    }
}