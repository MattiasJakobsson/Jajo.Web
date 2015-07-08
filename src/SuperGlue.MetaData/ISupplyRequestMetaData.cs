using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.MetaData
{
    public interface ISupplyRequestMetaData
    {
        bool CanHandleChain(string chain);
        Task<IDictionary<string, object>> GetMetaData(IDictionary<string, object> environment);
    }
}