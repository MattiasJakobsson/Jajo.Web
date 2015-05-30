using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Http
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RedirectTo
    {
        private readonly RedirectToOptions _options;

        public RedirectTo(AppFunc next, RedirectToOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _options = options;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var redirectTo = _options.GetRedirectUrl(environment);

            environment.GetResponse().StatusCode = 301;
            environment.GetResponse().Headers.Location = redirectTo;

            return Task.Factory.StartNew(() => { });
        }
    }

    public class RedirectToOptions
    {
        public RedirectToOptions(Func<IDictionary<string, object>, string> getRedirectUrl)
        {
            GetRedirectUrl = getRedirectUrl;
        }

        public Func<IDictionary<string, object>, string> GetRedirectUrl { get; private set; }
    }
}