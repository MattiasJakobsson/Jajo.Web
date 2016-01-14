using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Http
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetCacheHeaders
    {
        private readonly AppFunc _next;

        public SetCacheHeaders(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var cachedOutput = environment.GetOutput() as IWantToBeCached;

            if(cachedOutput != null)
                SetCacheOptions(cachedOutput.GetCacheSettings(), environment);

            await _next(environment).ConfigureAwait(false);
        }

        private static void SetCacheOptions(CacheOptions cacheOptions, IDictionary<string, object> environment)
        {
            var response = environment.GetResponse();

            if (cacheOptions == null)
                return;

            if (cacheOptions.Disable)
            {
                response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                response.Headers.Pragma = "no-cache";
                response.Headers.Expires = null;
                return;
            }

            response.Headers.CacheControl = cacheOptions.ToHeaderString();

            if (cacheOptions.AbsoluteExpiry.HasValue)
                response.Headers.Expires = cacheOptions.AbsoluteExpiry.Value;

            if (cacheOptions.VaryByHeaders != null && cacheOptions.VaryByHeaders.Count > 0)
                response.Headers.Vary = string.Join(", ", cacheOptions.VaryByHeaders);
        }
    }
}