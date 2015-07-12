using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiResponse
    {
        IEnumerable<IApiLink> GetLinks();
        IEnumerable<IApiForm> GetForms();
        IReadOnlyDictionary<string, IEnumerable<IApiResponse>> GetChildren();
    }
}