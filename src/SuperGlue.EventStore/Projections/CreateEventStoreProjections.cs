using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Data;
using SuperGlue.UnitOfWork;

namespace SuperGlue.EventStore.Projections
{
    public class CreateEventStoreProjections : IApplicationTask
    {
        private readonly EventStoreConnectionString _eventStoreConnectionString;
        private readonly IEnumerable<IEventStoreProjection> _projections;

        public CreateEventStoreProjections(EventStoreConnectionString eventStoreConnectionString, IEnumerable<IEventStoreProjection> projections)
        {
            _eventStoreConnectionString = eventStoreConnectionString;
            _projections = projections;
        }

        public async Task Start()
        {
            var projectionBuilder = new ProjectionBuilder();
            var projectionManager = _eventStoreConnectionString.CreateProjectionsManager();
            var credentials = _eventStoreConnectionString.GetUserCredentials();

            foreach (var projection in _projections)
            {
                var name = string.Format("project-to-{0}", projection.ProjectionName);
                var query = projectionBuilder.BuildStreamProjection(projection.GetInterestingStreams(), projection.ProjectionName);

                await projectionManager.CreateOrUpdateContinuousQueryAsync(name, query, credentials);
            }
        }

        public Task ShutDown()
        {
            return Task.CompletedTask;
        }

        public Task Exception(Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}