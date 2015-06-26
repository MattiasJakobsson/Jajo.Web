using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventTracking
{
    public class TrackedEntity
    {
        public TrackedEntity(ICanApplyEvents entity, IReadOnlyDictionary<string, object> commandMetaData)
        {
            CommandMetaData = commandMetaData ?? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
            Entity = entity;
        }

        public ICanApplyEvents Entity { get; private set; }
        public IReadOnlyDictionary<string, object> CommandMetaData { get; private set; }

        public void SetMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (CommandMetaData == null || !CommandMetaData.Any())
                CommandMetaData = metaData;
        }
    }
}