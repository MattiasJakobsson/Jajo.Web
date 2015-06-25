using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.EventTracking
{
    public class TrackedEntity
    {
        public TrackedEntity(ICanApplyEvents entity, object associatedCommand, IDictionary<string, object> commandMetaData)
        {
            CommandMetaData = commandMetaData ?? new Dictionary<string, object>();
            AssociatedCommand = associatedCommand;
            Entity = entity;
        }

        public ICanApplyEvents Entity { get; private set; }
        public object AssociatedCommand { get; private set; }
        public IDictionary<string, object> CommandMetaData { get; private set; }

        public void SetMetaData(object associatedCommand, IDictionary<string, object> metaData)
        {
            if (AssociatedCommand == null)
                AssociatedCommand = associatedCommand;

            if (CommandMetaData == null || !CommandMetaData.Any())
                CommandMetaData = metaData;
        }
    }
}