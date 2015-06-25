using System.Collections.Generic;

namespace SuperGlue.EventTracking
{
    public interface ICanApplyEvents
    {
        string Id { get; }
        string Context { get; }
        IEnumerable<object> GetAppliedEvents();
        void ClearAppliedEvents();
    }
}