using System;
using SuperGlue.EventStore.Data;
using SuperGlue.UnitOfWork;

namespace SuperGlue.EventStore
{
    public class EventStoreUnitOfWork : ISuperGlueUnitOfWork
    {
        private readonly IRepository _repository;

        public EventStoreUnitOfWork(IRepository repository)
        {
            _repository = repository;
        }

        public void Begin()
        {
            
        }

        public void Commit()
        {
            _repository.SaveChanges();
        }

        public void Rollback(Exception exception = null)
        {
            
        }
    }
}