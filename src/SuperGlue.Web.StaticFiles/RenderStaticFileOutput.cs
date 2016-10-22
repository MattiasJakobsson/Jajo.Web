using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.FileSystem;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.StaticFiles
{
    public class RenderStaticFileOutput : IRenderOutput
    {
        public async Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput() as StaticFileOutput;

            if (output == null)
                return new OutputRenderingResult(null, "");

            environment.GetResponse().Headers.CacheControl = output.CacheControl;

            var fileSystem = environment.Resolve<IFileSystem>();

            return new OutputRenderingResult(await fileSystem.ReadFile(output.FilePath).ConfigureAwait(false), environment.GetRequest().Headers.Accept.Split(',').Select(x => x.Trim()).FirstOrDefault());
        }
    }
}