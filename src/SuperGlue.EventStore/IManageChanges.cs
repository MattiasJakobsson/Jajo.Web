using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore
{
    public interface IManageChanges
    {
        Task ChangesSaved(IEnumerable<object> changes, IDictionary<string, object> metaData);
    }
}