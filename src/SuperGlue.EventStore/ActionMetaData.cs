using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public class ActionMetaData
    {
        public ActionMetaData(IDictionary<string, object> environment, object associatedCommand = null, IDictionary<string, object> metaData = null, int? expectedVersion = null)
        {
            MetaData = metaData ?? new Dictionary<string, object>();
            Environment = environment;
            ExpectedVersion = expectedVersion;
            AssociatedCommand = associatedCommand;
        }

        public IDictionary<string, object> Environment { get; private set; }
        public object AssociatedCommand { get; private set; }
        public IDictionary<string, object> MetaData { get; private set; }
        public int? ExpectedVersion { get; private set; }
    }
}