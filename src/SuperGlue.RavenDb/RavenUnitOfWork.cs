using System;
using System.Threading.Tasks;
using SuperGlue.UnitOfWork;

namespace SuperGlue.RavenDb
{
    public class RavenUnitOfWork : ISuperGlueUnitOfWork
    {
        private readonly IRavenSessions _ravenSessions;

        public RavenUnitOfWork(IRavenSessions ravenSessions)
        {
            _ravenSessions = ravenSessions;
        }

        public Task Begin()
        {
            return Task.CompletedTask;
        }

        public Task Commit()
        {
            return _ravenSessions.SaveChanges();
        }

        public Task Rollback(Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}