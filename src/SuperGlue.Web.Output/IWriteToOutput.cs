using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public interface IWriteToOutput
    {
        Task Write(IDictionary<string, object> environment, OutputRenderingResult result);
    }
}