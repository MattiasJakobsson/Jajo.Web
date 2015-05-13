using System.Linq;
using Raven.Client.Indexes;

namespace SuperGlue.EventStore.Timeouts.RavenDb
{
    public class RavenTimeOutDataIndex : AbstractIndexCreationTask<RavenTimeOutData>
    {
        public RavenTimeOutDataIndex()
        {
            Map = docs => from doc in docs
                select new
                {
                    doc.Time,
                    doc.OwningTimeOutManager
                };
        }
    }
}