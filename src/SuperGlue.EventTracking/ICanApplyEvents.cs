using System.Collections.Generic;

namespace SuperGlue.EventTracking
{
    public interface ICanApplyEvents
    {
        string Id { get; }
        IEnumerable<object> GetAppliedEvents();
        void ClearAppliedEvents();
        IDictionary<string, object> GetMetaData(IDictionary<string, object> environment);
        string GetStreamName(IDictionary<string, object> environment);
    }
}