using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.FileUpload
{
    public interface IPersistFiles
    {
        Task Persist(IDictionary<string, object> environment, string name, Stream content);
        string GetUrlFor(IDictionary<string, object> environment, string file, ITransformFile transformer = null);
    }
}