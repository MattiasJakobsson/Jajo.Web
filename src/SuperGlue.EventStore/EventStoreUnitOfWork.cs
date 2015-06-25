using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.EventStore.Data;
using SuperGlue.EventTracking;
using SuperGlue.UnitOfWork;

namespace SuperGlue.EventStore
{
    public class EventStoreUnitOfWork : ISuperGlueUnitOfWork
    {
        private readonly IRepository _repository;
        private readonly ITrackEntitiesWithEvents _trackEntitiesWithEvents;
        private readonly IDictionary<string, object> _environment;

        public EventStoreUnitOfWork(IRepository repository, ITrackEntitiesWithEvents trackEntitiesWithEvents, IDictionary<string, object> environment)
        {
            _repository = repository;
            _trackEntitiesWithEvents = trackEntitiesWithEvents;
            _environment = environment;
        }

        public Task Begin()
        {
            return Task.Factory.StartNew(() => { });
        }

        public async Task Commit()
        {
            while (_trackEntitiesWithEvents.Count > 0)
            {
                var entity = _trackEntitiesWithEvents.Pop();
                var changes = entity.Entity.GetAppliedEvents().ToList();

                await _repository.SaveToStream(string.Format("entity-{0}-{1}", entity.GetType().Name, entity.Entity.Id.Replace("/", "-")), changes, Guid.NewGuid(), entity.Entity.Context, new ActionMetaData(_environment, entity.AssociatedCommand, entity.CommandMetaData));
            }

            await _repository.SaveChanges();
        }

        public Task Rollback(Exception exception = null)
        {
            //TODO:Implement
            return Task.Factory.StartNew(() => { });
        }
    }
}