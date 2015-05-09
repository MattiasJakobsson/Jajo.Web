using System;

namespace OpenWeb.UnitOfWork
{
    public interface IOpenWebUnitOfWork
    {
        void Begin();
        void Commit();
        void Rollback(Exception exception = null);
    }
}