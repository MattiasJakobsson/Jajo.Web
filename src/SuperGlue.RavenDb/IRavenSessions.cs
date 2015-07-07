using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;

namespace SuperGlue.RavenDb
{
    public interface IRavenSessions
    {
        IDictionary<string, object> Environment { get; }
        IAsyncDocumentSession GetFor(string database);
        IDocumentSession GetSyncFor(string database);
        Task SaveChanges();
        Task EnsureDbExists(string name);
    }
}