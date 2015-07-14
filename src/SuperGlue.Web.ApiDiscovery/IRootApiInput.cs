using System.Collections.Generic;

namespace SuperGlue.Web.ApiDiscovery
{
    public interface IRootApiInput
    {
        string GetName(IDictionary<string, object> environment);
    }
}