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
        void Start();
        Task Apply(object evnt, int version, IDictionary<string, object> metaData);
        Task Commit(IDictionary<string, object> environment);
    }
}