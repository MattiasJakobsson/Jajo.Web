using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public interface IFindPartitionKey
    {
        string FindFrom(IDictionary<string, object> metadata);
    }
}