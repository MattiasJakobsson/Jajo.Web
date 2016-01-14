using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.PartialRequests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class Partials
    {
        private static AppFunc partialChain;

        public static void Initialize(AppFunc chain)
        {
            partialChain = chain;
        }

        public static async Task<Stream> ExecutePartial(IDictionary<string, object> environment, object partial)
        {
            if(partialChain == null)
                throw new InvalidOperationException("You can't use partials when it's not initialized");

            var partialEnvironment = environment.CreatePartialRequest(partial);

            await partialChain(partialEnvironment).ConfigureAwait(false);

            var responseBody = partialEnvironment.GetResponse().Body;

            responseBody.Position = 0;
            return responseBody;
        }
    }
}
