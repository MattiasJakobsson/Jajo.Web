using System.Collections.Generic;

namespace SuperGlue.EventTracking
{
    public interface ICanApplyEvents
    {
        string Id { get; }
        IEnumerable<object> GetAppliedEvents();
        void ClearAppliedEvents();
        string GetStreamName(IDictionary<string, object> environment);
    }
}