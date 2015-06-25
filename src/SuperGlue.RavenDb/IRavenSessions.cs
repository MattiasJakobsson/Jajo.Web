using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;

namespace SuperGlue.RavenDb
{
    public interface IRavenSessions
    {
        IDictionary<string, object> Environment { get; }
        IAsyncDocumentSession GetFor(string database, object associatedCommand = null, IDictionary<string, object> commandMetaData = null);
        Task SaveChanges();
    }
}