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

        public static async Task<Stream> ExecutePartial(IDictionary<string, object> environment, object partial, object input = null)
        {
            if(partialChain == null)
                throw new InvalidOperationException("You can't use partials when it's not initialized");

            var partialEnvironment = new Dictionary<string, object>();
            foreach (var item in environment)
                partialEnvironment[item.Key] = item.Value;

            //TODO:Find parameters and set
            partialEnvironment.SetRouteDestination(partial);
            var responseBody = new MemoryStream();
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.ResponseBody] = responseBody;

            if(input != null)
                partialEnvironment.Set(input.GetType(), input);

            await partialChain(partialEnvironment);

            responseBody.Position = 0;
            return responseBody;
        }
    }
}
