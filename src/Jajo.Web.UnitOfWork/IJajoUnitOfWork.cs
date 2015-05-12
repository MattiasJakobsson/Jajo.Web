using System;

namespace Jajo.Web.UnitOfWork
{
    public interface IJajoUnitOfWork
    {
        void Begin();
        void Commit();
        void Rollback(Exception exception = null);
    }
}