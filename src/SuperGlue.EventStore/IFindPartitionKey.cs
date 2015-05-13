using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IFindPartitionKey
    {
        string FindFrom(IDictionary<string, object> metadata);
    }
}