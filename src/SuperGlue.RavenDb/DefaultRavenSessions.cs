using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.EventTracking;

namespace SuperGlue.RavenDb
{
    public class DefaultRavenSessions : IRavenSessions
    {
        private readonly ConcurrentDictionary<string, IAsyncDocumentSession> _openedSessions = new ConcurrentDictionary<string, IAsyncDocumentSession>();
        private readonly ConcurrentDictionary<string, IDocumentSession> _openedSyncSessions = new ConcurrentDictionary<string, IDocumentSession>();

        private readonly IDocumentStore _documentStore;
        private readonly IDictionary<string, object> _environment;
        private readonly ITrackEntitiesWithEvents _trackEntitiesWithEvents;

        public DefaultRavenSessions(IDocumentStore documentStore, IDictionary<string, object> environment, ITrackEntitiesWithEvents trackEntitiesWithEvents)
        {
            _documentStore = documentStore;
            _environment = environment;
            _trackEntitiesWithEvents = trackEntitiesWithEvents;
        }

        public IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        public virtual IAsyncDocumentSession GetFor(string database)
        {
            IAsyncDocumentSession session;

            if (_openedSessions.TryGetValue(database, out session))
                return session;

            session = new TrackingSession(_documentStore.OpenAsyncSession(database), _trackEntitiesWithEvents);
            _openedSessions[database] = session;

            return session;
        }

        public IDocumentSession GetSyncFor(string database)
        {
            IDocumentSession session;

            if (_openedSyncSessions.TryGetValue(database, out session))
                return session;

            session = new TrackingSyncSession(_documentStore.OpenSession(database), _trackEntitiesWithEvents);
            _openedSyncSessions[database] = session;

            return session;
        }

        public virtual async Task SaveChanges()
        {
            foreach (var session in _openedSessions)
                await session.Value.SaveChangesAsync();

            foreach (var session in _openedSyncSessions)
                session.Value.SaveChanges();

            _openedSessions.Clear();
            _openedSyncSessions.Clear();
        }
    }
}