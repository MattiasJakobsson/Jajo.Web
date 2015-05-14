using System;
using SuperGlue.UnitOfWork;

namespace SuperGlue.RavenDb.Search
{
    public class RavenUnitOfWork : ISuperGlueUnitOfWork
    {
        private readonly IRavenSessions _ravenSessions;

        public RavenUnitOfWork(IRavenSessions ravenSessions)
        {
            _ravenSessions = ravenSessions;
        }

        public void Begin()
        {

        }

        public void Commit()
        {
            _ravenSessions.SaveChanges();
        }

        public void Rollback(Exception exception)
        {

        }
    }
}