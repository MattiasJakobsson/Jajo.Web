using System.IO;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.PartialRequests
{
    public static class PartialViewExtensions
    {
        public static string Partial(this ISuperGlueView view, object partial)
        {
            var stream = Partials.ExecutePartial(view.Environment, partial).Result;

            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}