using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Monitoring
{
    public interface IMonitorHeartBeats
    {
        Task Beat(IDictionary<string, object> environment);
    }
}