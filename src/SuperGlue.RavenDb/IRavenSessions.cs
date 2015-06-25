using System.Collections.Generic;
using Raven.Client;

namespace SuperGlue.RavenDb
{
    public interface IRavenSessions
    {
        IDictionary<string, object> Environment { get; } 
        IDocumentSession GetFor(string database, object associatedCommand = null, IDictionary<string, object> commandMetaData = null);
        void SaveChanges();
    }
}