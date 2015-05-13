using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IManageChanges
    {
        void ChangesSaved(IEnumerable<object> changes, IDictionary<string, object> metaData);
    }
}