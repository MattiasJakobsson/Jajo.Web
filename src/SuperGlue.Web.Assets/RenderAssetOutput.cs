using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public class RenderAssetOutput : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var assetInformation = environment.GetOutput() as IContainAssetInformation;

            return assetInformation == null ? Task.Factory.StartNew(() => new OutputRenderingResult(new MemoryStream(), "")) : Task.Factory.StartNew(() => new OutputRenderingResult(assetInformation.Asset, assetInformation.MimeType));
        }
    }
}