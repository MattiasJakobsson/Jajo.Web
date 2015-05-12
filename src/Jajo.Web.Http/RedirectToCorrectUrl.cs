using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jajo.Web.Http
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RedirectToCorrectUrl
    {
        private readonly AppFunc _next;
        private readonly RedirectToCorrectUrlOptions _options;

        public RedirectToCorrectUrl(AppFunc next, RedirectToCorrectUrlOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var original = environment.GetUri().ToString();
            var currentUri = original;

            currentUri = _options.CorrectionFunctions.Aggregate(currentUri, (current, correctionFunction) => correctionFunction(current));

            if (currentUri != original)
            {
                environment["owin.ResponseStatusCode"] = 301;
                environment.GetResponseHeaders()["Location"] = new[] { currentUri };
                return;
            }

            await _next(environment);
        }
    }

    public class RedirectToCorrectUrlOptions
    {
        public RedirectToCorrectUrlOptions(params Func<string, string>[] correctionFunctions)
        {
            CorrectionFunctions = correctionFunctions;
        }

        public IEnumerable<Func<string, string>> CorrectionFunctions { get; private set; }
    }
}