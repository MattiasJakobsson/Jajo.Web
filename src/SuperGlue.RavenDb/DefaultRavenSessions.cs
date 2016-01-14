using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Indexes;
using SuperGlue.EventTracking;

namespace SuperGlue.RavenDb
{
    public class DefaultRavenSessions : IRavenSessions
    {
        private readonly ConcurrentDictionary<string, IAsyncDocumentSession> _openedSessions = new ConcurrentDictionary<string, IAsyncDocumentSession>();
        private readonly ConcurrentDictionary<string, IDocumentSession> _openedSyncSessions = new ConcurrentDictionary<string, IDocumentSession>();

        private readonly IDocumentStore _documentStore;
        private readonly IEnumerable<IAmInterestedInEventAwareItems> _amInterestedInEventAwareItems;

        public DefaultRavenSessions(IDocumentStore documentStore, IDictionary<string, object> environment, IEnumerable<IAmInterestedInEventAwareItems> amInterestedInEventAwareItems)
        {
            _documentStore = documentStore;
            Environment = environment;
            _amInterestedInEventAwareItems = amInterestedInEventAwareItems;
        }

        public IDictionary<string, object> Environment { get; }

        public virtual IAsyncDocumentSession GetFor(string database)
        {
            IAsyncDocumentSession session;

            if (_openedSessions.TryGetValue(database, out session))
                return session;

            session = new TrackingSession(_documentStore.OpenAsyncSession(database), _amInterestedInEventAwareItems);
            _openedSessions[database] = session;

            return session;
        }

        public IDocumentSession GetSyncFor(string database)
        {
            IDocumentSession session;

            if (_openedSyncSessions.TryGetValue(database, out session))
                return session;

            session = new TrackingSyncSession(_documentStore.OpenSession(database), _amInterestedInEventAwareItems);
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

        public Task EnsureDbExists(string name)
        {
            return _documentStore.AsyncDatabaseCommands.GlobalAdmin.EnsureDatabaseExistsAsync(name);
        }

        public Task CreateIndexes(Assembly assembly, string database)
        {
            var documentStore = RavenDocumentStore.Create(Environment, database);

            return IndexCreation.CreateIndexesAsync(assembly, documentStore);
        }
    }
}