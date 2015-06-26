using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.EventStore
{
    public class ActionMetaData
    {
        public ActionMetaData(IDictionary<string, object> environment, IReadOnlyDictionary<string, object> metaData = null, int? expectedVersion = null)
        {
            MetaData = metaData ?? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
            Environment = environment;
            ExpectedVersion = expectedVersion;
        }

        public IDictionary<string, object> Environment { get; private set; }
        public IReadOnlyDictionary<string, object> MetaData { get; private set; }
        public int? ExpectedVersion { get; private set; }
    }
}