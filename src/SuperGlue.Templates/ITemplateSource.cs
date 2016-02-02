using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Templates
{
    public interface ITemplateSource
    {
        Task<string> Find(IDictionary<string, object> environment, string template);
    }
}