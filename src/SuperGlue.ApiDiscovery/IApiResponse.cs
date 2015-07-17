using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiResponse
    {
        IEnumerable<ApiLink> GetLinks();
        IEnumerable<ApiForm> GetForms();
        IReadOnlyDictionary<string, IEnumerable<IApiResponse>> GetChildren();
    }
}