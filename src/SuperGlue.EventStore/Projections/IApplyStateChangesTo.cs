using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public interface IApplyStateChangesTo : IDisposable
    {
        Task Apply(object evnt, int version, IDictionary<string, object> metaData);
    }
}