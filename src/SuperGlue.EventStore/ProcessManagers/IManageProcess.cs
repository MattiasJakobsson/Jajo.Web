using System.Collections.Generic;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IManageProcess
    {
        string ProcessName { get; }

        IEnumerable<string> GetStreamsToProcess();
        void Start();
        void Apply(object evnt, int version, IDictionary<string, object> metaData);
        void Commit();
    }
}