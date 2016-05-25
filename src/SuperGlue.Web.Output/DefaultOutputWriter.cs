using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class DefaultOutputWriter : IWriteToOutput
    {
        public Task Write(IDictionary<string, object> environment, OutputRenderingResult result)
        {
            if (result == null)
                return Task.CompletedTask;

            var response = environment.GetResponse();

            response.Headers.ContentType = result.ContentType;

            using (result.Body)
                return response.Write(result.Body);
        }
    }
}