using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public interface IEventStoreProjection
    {
        string ProjectionName { get; }
        IEnumerable<string> GetInterestingStreams();
        void Start();
        Task Apply(object evnt, int version, IDictionary<string, object> metaData);
        Task Commit();
    }
}