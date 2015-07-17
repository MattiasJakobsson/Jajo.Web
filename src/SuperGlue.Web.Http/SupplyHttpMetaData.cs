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
            var realIp = environment.GetRequest().Headers.GetHeader("X-Real-IP");

            return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>
            {
                {HttpMetaDataKeys.IpAddress, string.IsNullOrEmpty(realIp) ? environment.GetRequest().RemoteIpAddress : realIp},
                {HttpMetaDataKeys.Culture, Thread.CurrentThread.CurrentCulture.Name}
            });
        }
    }
}