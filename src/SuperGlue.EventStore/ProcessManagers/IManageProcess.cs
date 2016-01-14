using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IManageProcess
    {
        string ProcessName { get; }

        IEnumerable<string> GetStreamsToProcess();
        IReadOnlyDictionary<Type, string> GetEventMappings();
        Task Apply(object evnt, IDictionary<string, object> metaData);
    }
}