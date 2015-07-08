using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SuperGlue.MetaData;

namespace SuperGlue.Web.Http
{
    public class SupplyHttpMetaData : ISupplyRequestMetaData
    {
        public bool CanHandleChain(string chain)
        {
            return chain == "chains.Web";
        }

        public Task<IDictionary<string, object>> GetMetaData(IDictionary<string, object> environment)
        {
            return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>
            {
                {HttpMetaDataKeys.IpAddress, environment.GetRequest().RemoteIpAddress},
                {HttpMetaDataKeys.Culture, Thread.CurrentThread.CurrentCulture.Name}
            });
        }
    }
}