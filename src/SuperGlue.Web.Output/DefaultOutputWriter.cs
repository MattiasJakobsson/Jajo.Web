using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class DefaultOutputWriter : IWriteToOutput
    {
        public async Task Write(IDictionary<string, object> environment, OutputRenderingResult result)
        {
            if (result == null)
                return;

            var response = environment.GetResponse();

            response.Headers.ContentType = result.ContentType;

            using (result.Body)
                await response.Write(result.Body).ConfigureAwait(false);
        }
    }
}