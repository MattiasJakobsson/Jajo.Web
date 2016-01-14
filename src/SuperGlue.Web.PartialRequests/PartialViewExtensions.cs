using System.IO;
using System.Threading.Tasks;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.PartialRequests
{
    public static class PartialViewExtensions
    {
        public static async Task<string> Partial(this ISuperGlueView view, object partial)
        {
            var stream = await Partials.ExecutePartial(view.Environment, partial).ConfigureAwait(false);

            using (var streamReader = new StreamReader(stream))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}