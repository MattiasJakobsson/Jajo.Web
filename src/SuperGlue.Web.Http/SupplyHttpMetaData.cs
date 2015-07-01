using System.Collections.Generic;
using System.Threading;
using SuperGlue.MetaData;

namespace SuperGlue.Web.Http
{
    public class SupplyHttpMetaData : ISupplyRequestMetaData
    {
        public bool CanHandleChain(string chain)
        {
            return chain == "chains.Web";
        }

        public IDictionary<string, object> GetMetaData(IDictionary<string, object> environment)
        {
            return new Dictionary<string, object>
            {
                {HttpMetaDataKeys.IpAddress, environment.GetRequest().Headers.GetHeader("X-Forwarded-For")},
                {HttpMetaDataKeys.Culture, Thread.CurrentThread.CurrentCulture.Name}
            };
        }
    }
}