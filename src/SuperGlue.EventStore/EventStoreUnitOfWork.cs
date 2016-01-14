using System;
using System.Threading.Tasks;
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

        public Task Begin()
        {
            return Task.CompletedTask;
        }

        public async Task Commit()
        {
            await _repository.SaveChanges().ConfigureAwait(false);
        }

        public Task Rollback(Exception exception = null)
        {
            _repository.ThrowAwayChanges();

            return Task.CompletedTask;
        }
    }
}