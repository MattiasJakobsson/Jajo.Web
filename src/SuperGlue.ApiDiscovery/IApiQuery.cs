using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiQuery
    {
        IApiQuery TravelTo(IApiLinkTravelInstruction travelInstruction);
        Task<IApiResource> Query(IDictionary<string, object> data);
        Task<IHttpResponse> ExecuteForm(IFormTravelInstruction travelToForm, IDictionary<string, object> data);
    }
}