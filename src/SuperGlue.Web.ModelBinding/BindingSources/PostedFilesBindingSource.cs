using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.BindingSources
{
    public class PostedFilesBindingSource : IBindingSource
    {
        public async Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            var files = await envinronment.GetRequest().ReadFiles();

            return files.ToDictionary(x => x.Name.ToLower(), x => (object)new PostedFile(x.Name, x.ContentType, x.Value));
        }
    }
}