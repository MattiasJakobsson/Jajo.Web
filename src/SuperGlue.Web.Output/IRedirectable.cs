using System.Collections.Generic;

namespace SuperGlue.Web.Output
{
    public interface IRedirectable
    {
        string GetUrl(IDictionary<string, object> environment);
    }
}