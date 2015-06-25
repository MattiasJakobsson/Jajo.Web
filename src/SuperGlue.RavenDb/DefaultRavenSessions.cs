using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;

namespace SuperGlue.RavenDb
{
    public class DefaultRavenSessions : IRavenSessions
    {
        private readonly ConcurrentDictionary<string, IAsyncDocumentSession> _openedSessions = new ConcurrentDictionary<string, IAsyncDocumentSession>();

        private readonly IDocumentStore _documentStore;
        private readonly IDictionary<string, object> _environment;

        public DefaultRavenSessions(IDocumentStore documentStore, IDictionary<string, object> environment)
        {
            _documentStore = documentStore;
            _environment = environment;
        }

        public IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        public virtual IAsyncDocumentSession GetFor(string database, object associatedCommand = null, IDictionary<string, object> commandMetaData = null)
        {
            IAsyncDocumentSession session;

            if (_openedSessions.TryGetValue(database, out session))
                return session;
            
            session = _documentStore.OpenAsyncSession(database);
            _openedSessions[database] = session;

            return session;
        }

        public virtual async Task SaveChanges()
        {
            foreach (var session in _openedSessions)
                await session.Value.SaveChangesAsync();

            _openedSessions.Clear();
        }
    }
}