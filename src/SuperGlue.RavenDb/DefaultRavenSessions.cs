using System.Collections.Concurrent;
using System.Collections.Generic;
using Raven.Client;

namespace SuperGlue.RavenDb
{
    public class DefaultRavenSessions : IRavenSessions
    {
        private readonly ConcurrentDictionary<string, IDocumentSession> _openedSessions = new ConcurrentDictionary<string, IDocumentSession>();

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

        public virtual IDocumentSession GetFor(string database, object associatedCommand = null, IDictionary<string, object> commandMetaData = null)
        {
            IDocumentSession session;

            if (_openedSessions.TryGetValue(database, out session))
                return session;

            session = _documentStore.OpenSession(database);
            _openedSessions[database] = session;

            return session;
        }

        public virtual void SaveChanges()
        {
            foreach (var session in _openedSessions)
                session.Value.SaveChanges();

            _openedSessions.Clear();
        }
    }
}